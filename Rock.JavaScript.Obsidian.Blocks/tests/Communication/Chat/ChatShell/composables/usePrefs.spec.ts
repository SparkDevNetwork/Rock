// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
// The person's settings: a room's notify mode, star and hide, and their own switches. Every change
// shows at once and writes at once, one save per change: a room's through chat_set_prefs, the
// reaction switch through it too with no room, and the two privacy switches through Rock's
// SavePersonSetting door. Hiding unstars and starring unhides in the same one patch. A save that
// lands shows nothing more; one that fails puts the control back to the last value saved, with the
// server's own sentence and, where another try can pass, Try again under it, said aloud. A change
// made from the header's menu says so in a toast instead. An answer to a save the person has
// since changed again moves nothing but what is known saved, and once stopped no answer reaches
// the screen.
//
// The fakes answer in the platform's and the door's own shapes: chat_set_prefs's
// {channel: {channel_id, is_favorite, hidden, notify_mode, notify_mode_effective}, me: {...}},
// its refusals as the shell's one call path classifies them, and ChatPersonSettingResultBag's
// {code, isPending, message}.
/* eslint-disable @typescript-eslint/naming-convention */
import { watch } from "vue";
import { createPrefs, PersonSettingKey, PersonSettingResult, PrefsCallResult, statusSaveOutcome } from "../../../../../src/Communication/Chat/ChatShell/composables/usePrefs.partial";
import { readOnlySentence as readOnlyWordsOfOurs } from "../../../../../src/Communication/Chat/ChatShell/errors.partial";
import { CallResult, createPlatformCall } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { createChannelStore } from "../../../../../src/Communication/Chat/ChatShell/stores/channelStore.partial";
import { SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { setStatusAnswers, StatusSaveAnswer, statusSaveFailures } from "../statusShapes";

// #region Fixtures

const team = "c0000001-0000-4000-8000-000000000000";
const prayer = "c0000002-0000-4000-8000-000000000000";

const readOnlySentence = "Chat is read only for a few minutes while Cedar Hill updates it. Try again soon.";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void, reject: (error: unknown) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    let reject!: (error: unknown) => void;
    const promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });
    return { promise, resolve, reject };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 20; i++) {
        await Promise.resolve();
    }
}

/** A sidebar row in the exact shape chat_get_bootstrap answers. */
function sidebarRow(channelId: string, extra: Partial<SidebarRow> = {}): SidebarRow {
    return {
        channel_id: channelId,
        name: "Room",
        icon_url: null,
        channel_type: "shared",
        is_public: true,
        always_shown: false,
        is_member: true,
        is_favorite: false,
        notify_mode: "all",
        is_leader: false,
        can_post_announcements: false,
        can_mention_all: false,
        last_message_id: 10,
        last_message_at: "2026-10-19T09:00:00Z",
        last_message_type: "text",
        last_sender_nick_name: "Ada",
        last_sender_last_name: "Member",
        last_sender_avatar_url: null,
        last_sender_listed: true,
        last_message_preview: "old",
        last_message_is_own: false,
        last_message_posted_by_rock: false,
        read_cursor: 10,
        is_unread: false,
        mention_count: 0,
        all_mention_count: 0,
        can_mention: true,
        dm_people: null,
        dm_other_count: null,
        room_kind: "channel",
        title: extra.name ?? "Room",
        preview_line: "",
        ...extra
    };
}

/** chat_set_prefs's answer for a room, as it reads the rows back after writing them. */
function written(channelId: string, channel: { is_favorite?: boolean, hidden?: boolean, notify_mode?: string | null } = {}, me: Record<string, unknown> = {}): PrefsCallResult {
    const mode = channel.notify_mode ?? null;
    return {
        ok: true,
        data: {
            channel: {
                channel_id: channelId,
                is_favorite: channel.is_favorite ?? false,
                hidden: channel.hidden ?? false,
                notify_mode: mode,
                notify_mode_effective: mode ?? "all"
            },
            me: { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true, ...me }
        }
    };
}

/** chat_set_prefs's answer with no room, as the reaction switch is written. */
function writtenMe(me: Record<string, unknown>): PrefsCallResult {
    return { ok: true, data: { channel: null, me: { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true, ...me } } };
}

/** A refusal as the shell's call path hands it on: the platform's code and its own sentence. */
function refused(code = "rpc.read_only", text: string | null = readOnlySentence): PrefsCallResult {
    return { ok: false, error: { code, severity: "failed", text } };
}

/** The door's answer, as ChatPersonSettingResultBag reaches the browser. */
function door(code: string, isPending = false, message: string | null = null): PersonSettingResult {
    return { code, isPending, message };
}

type Setup = {
    prefs: ReturnType<typeof createPrefs>;
    channels: ReturnType<typeof createChannelStore>;
    prefCalls: Array<{ channelId: string | null, changes: Record<string, unknown>, answer: Deferred<PrefsCallResult> }>;
    doorCalls: Array<{ setting: PersonSettingKey, value: boolean, answer: Deferred<PersonSettingResult> }>;
    toasts: Array<{ code: string, text: string, icon: string }>;
    offline: { value: boolean };
};

function setup(rows: SidebarRow[] = [sidebarRow(team), sidebarRow(prayer, { name: "Prayer" })]): Setup {
    const prefCalls: Setup["prefCalls"] = [];
    const doorCalls: Setup["doorCalls"] = [];
    const toasts: Setup["toasts"] = [];
    const offline = { value: false };
    const channels = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
    channels.setSidebar(rows);

    const prefs = createPrefs({
        setPrefs: (channelId, changes) => {
            const answer = deferred<PrefsCallResult>();
            prefCalls.push({ channelId, changes, answer });
            return answer.promise;
        },
        savePersonSetting: (setting, value) => {
            const answer = deferred<PersonSettingResult>();
            doorCalls.push({ setting, value, answer });
            return answer.promise;
        },
        channels,
        isOffline: () => offline.value,
        notify: notice => toasts.push(notice)
    });

    return { prefs, channels, prefCalls, doorCalls, toasts, offline };
}

function rowOf(s: Setup, channelId: string): SidebarRow | undefined {
    return s.channels.rows.find(row => row.channel_id === channelId);
}

// #endregion Fixtures

describe("a room's notify mode", () => {
    test("shows at once and writes one key of chat_set_prefs for the room", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(s.prefCalls).toEqual([expect.objectContaining({ channelId: team, changes: { notify_mode: "mentions" } })]);

        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();

        // A save that lands says nothing: the control already shows it.
        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(s.prefs.roomOf(team)).toEqual({ notes: {} });
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("asks nothing when the mode chosen is the one the room already has", async () => {
        const s = setup();

        await s.prefs.setNotifyMode(team, "all", false);

        expect(s.prefCalls).toEqual([]);
        expect(s.prefs.roomOf(team)).toEqual({ notes: {} });
    });

    test("goes back as it was on a refusal, the server's own sentence under the choices", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("all");
        expect(s.prefs.roomOf(team).notes.notify).toEqual({ kind: "error", text: readOnlySentence });
        expect(s.prefs.state.announcement).toEqual({ text: `Not saved. ${readOnlySentence}`, isAssertive: true, count: expect.any(Number) });
    });

    test("Try again asks the refused choice again, shown at once, and clears the note", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(refused("rpc.busy", "Chat is busy."));
        await settle();

        s.prefs.roomOf(team).notes.notify?.retry?.();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();
        expect(s.prefCalls[1].changes).toEqual({ notify_mode: "silent" });

        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes).toEqual({});
    });

    test("from the header's menu, says what changed in a toast, and a refusal's sentence in one", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", true);
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        expect(s.toasts.map(t => t.text)).toEqual(["Muted. A mention by name still reaches you."]);

        void s.prefs.setNotifyMode(team, "mentions", true);
        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();
        void s.prefs.setNotifyMode(team, "all", true);
        s.prefCalls[2].answer.resolve(refused());
        await settle();

        expect(s.toasts.map(t => t.text)).toEqual([
            "Muted. A mention by name still reaches you.",
            "You'll be notified only when mentioned",
            readOnlySentence
        ]);
        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
    });

    test("an older save's refusal moves nothing once the person has chosen again", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();

        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();
    });

    // One save of a setting is out at a time, so the newest is never answered before an older one:
    // it waits, shown, and is asked once the older is answered.
    test("a save made while an older one is out waits for it, the older's landing moves nothing on screen, and the newer's refusal goes back to what the older landed", async () => {
        const s = setup();

        const mentions = s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }]);

        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await mentions;
        await settle();

        // The older's answer is not the newest, so it neither draws its mode nor says anything.
        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }, { notify_mode: "silent" }]);

        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(s.prefs.roomOf(team).notes.notify?.kind).toBe("error");
    });

    test("the newest save refused after an older one landed goes back to the older one", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
    });

    test("both of two quick saves refused go back to where it started", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(refused());
        await settle();
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("all");
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }, { notify_mode: "silent" }]);
    });

    test("quick changes send one flight, then only the newest, the skipped ones resolving false without a call", async () => {
        const s = setup();

        const first = s.prefs.setNotifyMode(team, "mentions", false);
        let isSkippedDone = false;
        void s.prefs.setNotifyMode(team, "silent", false).then(() => {
            isSkippedDone = true;
        });
        const newest = s.prefs.setNotifyMode(team, "all", false);
        await settle();

        expect(isSkippedDone).toBe(true);
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await first;
        await settle();
        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "all" }));
        await newest;

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }, { notify_mode: "all" }]);
        expect(rowOf(s, team)?.notify_mode).toBe("all");
    });

    test("takes the effective mode the platform read back, so a cleared choice shows the room's default", async () => {
        const s = setup([sidebarRow(team, { notify_mode: "mentions" })]);

        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve({
            ok: true,
            data: { channel: { channel_id: team, is_favorite: false, hidden: false, notify_mode: "silent", notify_mode_effective: "silent" }, me: {} }
        });
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
    });
});

describe("star and hide", () => {
    test("starring writes the room's whole sidebar setting and moves the room into Starred at once", async () => {
        const s = setup();

        void s.prefs.setStarred(prayer, true, false);

        expect(rowOf(s, prayer)?.is_favorite).toBe(true);
        // starred rooms sort first, as the platform's sidebar sorts them
        expect(s.channels.rows[0].channel_id).toBe(prayer);
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ is_favorite: true, hidden: false }]);

        s.prefCalls[0].answer.resolve(written(prayer, { is_favorite: true }));
        await settle();

        expect(rowOf(s, prayer)?.is_favorite).toBe(true);
        expect(s.prefs.roomOf(prayer).notes).toEqual({});
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("hiding a starred room unstars it in the same one patch, and says so under Hide", async () => {
        const s = setup([sidebarRow(team, { is_favorite: true })]);

        void s.prefs.setHidden(team, true);

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ is_favorite: false, hidden: true }]);
        expect(rowOf(s, team)?.is_favorite).toBe(false);
        expect(rowOf(s, team)?.hidden).toBe(true);
        // What hiding means is said with the change, not after its save.
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden, and taken out of Starred. It leaves your list when you open another conversation." });

        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();

        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden, and taken out of Starred. It leaves your list when you open another conversation." });
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("hiding a room that is not starred writes it hidden and still not starred", async () => {
        const s = setup();

        void s.prefs.setHidden(team, true);

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ is_favorite: false, hidden: true }]);
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden. It leaves your list when you open another conversation." });
    });

    // The platform reads a key left out of the patch as unchanged, so the bytes sent are what
    // matters, not the object: toEqual would pass an undefined favorite that the wire drops.
    test("a hide and a star send the row's star and hidden both, by value, on the wire", async () => {
        const s = setup();

        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();
        void s.prefs.setStarred(prayer, true, false);

        expect(s.prefCalls.map(c => JSON.stringify(c.changes))).toEqual([
            "{\"is_favorite\":false,\"hidden\":true}",
            "{\"is_favorite\":true,\"hidden\":false}"
        ]);
    });

    test("starring a hidden room unhides it in the same one patch, and says so under Star", async () => {
        const s = setup();
        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();

        void s.prefs.setStarred(team, true, false);

        expect(s.prefCalls[1].changes).toEqual({ is_favorite: true, hidden: false });
        expect(rowOf(s, team)?.hidden).toBe(false);
        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "info", text: "Starred, so it's back in your list." });
        expect(s.prefs.roomOf(team).notes.hide).toBeUndefined();
    });

    test("a refusal puts both switches back and carries the server's sentence under the one pressed", async () => {
        const s = setup([sidebarRow(team, { is_favorite: true })]);

        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(true);
        expect(rowOf(s, team)?.hidden).toBeFalsy();
        // The note said with the change gives way to the refusal.
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "error", text: readOnlySentence });
    });

    // The star in the header shows it, so only a refusal is toasted.
    test("from the header, a star that lands says nothing, and a refusal's sentence is toasted", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, true);
        s.prefCalls[0].answer.resolve(written(team, { is_favorite: true }));
        await settle();
        void s.prefs.setStarred(team, false, true);
        s.prefCalls[1].answer.resolve(written(team));
        await settle();
        void s.prefs.setStarred(team, true, true);
        s.prefCalls[2].answer.resolve(refused());
        await settle();

        expect(s.toasts.map(t => [t.text, t.icon])).toEqual([[readOnlySentence, "alert"]]);
        expect(s.prefs.state.announcement?.text).toBe(`Not saved. ${readOnlySentence}`);
        expect(rowOf(s, team)?.is_favorite).toBe(false);
    });

    test("a lost connection's refusal says the person is offline", async () => {
        const s = setup();
        s.offline.value = true;

        void s.prefs.setStarred(team, true, false);
        s.prefCalls[0].answer.resolve(refused("rpc.transport", null));
        await settle();

        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "error", text: "You're offline.", retry: expect.any(Function) });
    });

    test("a call that reached nothing while the connection holds is not called offline", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, false);
        s.prefCalls[0].answer.resolve(refused("rpc.transport", null));
        await settle();

        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "error", text: "Something went wrong in chat.", retry: expect.any(Function) });
    });

    // Star and hide are one save's two halves, so what is known saved is both of them.
    test("a star that lands and then a hide refused goes back to what landed: starred and not hidden", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, false);
        void s.prefs.setHidden(team, true);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(written(team, { is_favorite: true }));
        await settle();
        expect(s.prefCalls[1].changes).toEqual({ is_favorite: false, hidden: true });
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(true);
        expect(rowOf(s, team)?.hidden).toBe(false);
    });

    // The newest save is the whole value shown, so it lands what is shown whatever an older one
    // does: here the unhide is refused, but the star after it carried the unhide too.
    test("an unhide still out and then a star: the star asks for the room shown, starred and not hidden", async () => {
        const s = setup([sidebarRow(team, { hidden: true }), sidebarRow(prayer, { name: "Prayer" })]);

        void s.prefs.setHidden(team, false);
        void s.prefs.setStarred(team, true, false);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        // The unhide's refusal is not the newest, so it puts nothing back while the star waits.
        expect(rowOf(s, team)?.hidden).toBe(false);
        expect(s.prefCalls[1].changes).toEqual({ is_favorite: true, hidden: false });
        s.prefCalls[1].answer.resolve(written(team, { is_favorite: true }));
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(true);
        expect(rowOf(s, team)?.hidden).toBe(false);
    });

    test("a save whose call throws goes back, says it couldn't save with Try again, and is no longer out", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, false);
        s.prefCalls[0].answer.reject(new Error("lost"));
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(false);
        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "error", text: "Couldn't save.", retry: expect.any(Function) });

        // Nothing is out, so the next load is taken as read.
        s.prefs.setRooms([sidebarRow(team, { is_favorite: true }), sidebarRow(prayer, { name: "Prayer" })]);
        expect(rowOf(s, team)?.is_favorite).toBe(true);
    });
});

describe("a sidebar load", () => {
    test("while a room's save is out leaves that room's setting as the person chose it, in its place in the list", async () => {
        const s = setup();

        void s.prefs.setStarred(prayer, true, false);
        void s.prefs.setNotifyMode(team, "silent", false);
        void s.prefs.setHidden(team, true);

        // A load read before the saves landed.
        s.prefs.setRooms([sidebarRow(team, { last_message_id: 11 }), sidebarRow(prayer, { name: "Prayer" })]);

        expect(rowOf(s, team)?.last_message_id).toBe(11);
        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(rowOf(s, team)?.hidden).toBe(true);
        expect(rowOf(s, prayer)?.is_favorite).toBe(true);
        expect(s.channels.rows[0].channel_id).toBe(prayer);
    });

    test("once the saves are answered takes the rooms as read", async () => {
        const s = setup();

        void s.prefs.setStarred(prayer, true, false);
        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(written(prayer, { is_favorite: true }));
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        s.prefs.setRooms([sidebarRow(team, { notify_mode: "mentions" }), sidebarRow(prayer, { name: "Prayer" })]);

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(rowOf(s, prayer)?.is_favorite).toBe(false);
    });
});

describe("the person's own switches", () => {
    test("start from the bootstrap's me", () => {
        const s = setup();

        s.prefs.setMe({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, unread_mentions: [] } as never);

        expect(s.prefs.state.me).toEqual({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, busy_until: null, appear_offline: false });
    });

    test("her Busy and Appear offline come from the bootstrap's me at every load, a Busy with no end as infinity, and anything else as none", () => {
        const s = setup();

        s.prefs.setMe({ busy_until: "2026-11-10T16:00:00.000000+00:00", appear_offline: true });
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: "2026-11-10T16:00:00.000000+00:00", appear_offline: true }));

        s.prefs.setMe({ busy_until: "infinity", appear_offline: false });
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: "infinity", appear_offline: false }));

        s.prefs.setMe({ busy_until: 5, appear_offline: "yes" } as never);
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: null, appear_offline: false }));

        s.prefs.setMe({ busy_until: "infinity" });
        s.prefs.setMe({ show_profile_details: true });
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: null, appear_offline: false }));
    });

    test("are read only once a me has been stated, and are already hers when that says so", () => {
        const s = setup();
        const seen: Array<boolean | string | null> = [];
        const stop = watch(() => s.prefs.state.isMeRead, read => seen.push(read, s.prefs.state.me.appear_offline, s.prefs.state.me.busy_until), { flush: "sync" });
        expect(s.prefs.state.isMeRead).toBe(false);

        s.prefs.setMe(null);
        expect(s.prefs.state.isMeRead).toBe(false);

        s.prefs.setMe({ busy_until: "infinity", appear_offline: true });
        stop();

        expect(seen).toEqual([true, true, "infinity"]);
    });

    test("the reaction switch writes chat_set_prefs with no room", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setReactionPush(false);

        expect(s.prefs.state.me.reaction_push).toBe(false);
        expect(s.prefCalls).toEqual([expect.objectContaining({ channelId: null, changes: { reaction_push: false } })]);

        s.prefCalls[0].answer.resolve(writtenMe({ reaction_push: false }));
        await settle();

        expect(s.prefs.state.me.reaction_push).toBe(false);
        expect(s.prefs.state.meNotes).toEqual({});
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("one switch refused leaves another switch saved as the person set it", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setReactionPush(false);
        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[0].answer.resolve(door("ok"));
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(s.prefs.state.me).toEqual({ show_profile_details: true, is_open_dm_allowed: true, reaction_push: true, busy_until: null, appear_offline: false });
        expect(Object.keys(s.prefs.state.meNotes)).toEqual(["reaction_push"]);
    });

    test("Try again on a switch asks its refused value again, shown at once", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[0].answer.resolve(door("door.unavailable"));
        await settle();
        expect(s.prefs.state.me.is_open_dm_allowed).toBe(false);

        s.prefs.state.meNotes.open_dm?.retry?.();

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.prefs.state.meNotes.open_dm).toBeUndefined();
        expect(s.doorCalls.map(c => [c.setting, c.value])).toEqual([["open_dm", true], ["open_dm", true]]);
    });

    test("a sidebar load while a switch's save is out leaves that switch as the person set it", () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.prefs.setMe({ show_profile_details: false, is_open_dm_allowed: false, reaction_push: false });

        expect(s.prefs.state.me).toEqual({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, busy_until: null, appear_offline: false });
    });

    test("a privacy switch goes through Rock's door, one setting a call", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.doorCalls.map(c => [c.setting, c.value])).toEqual([["open_dm", true]]);
        expect(s.prefCalls).toEqual([]);

        s.doorCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("a door that answers pending has saved, so it shows nothing more either", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("profile_details", false);
        s.doorCalls[0].answer.resolve(door("ok", true));
        await settle();

        expect(s.prefs.state.me.show_profile_details).toBe(false);
        expect(s.prefs.state.meNotes).toEqual({});
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("goes back on the door's refusal with the door's own sentence", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("profile_details", false);
        s.doorCalls[0].answer.resolve(door("age_restricted", false, "Chat is not available to you right now."));
        await settle();

        expect(s.prefs.state.me.show_profile_details).toBe(true);
        expect(s.prefs.state.meNotes.profile_details).toEqual({ kind: "error", text: "Chat is not available to you right now." });
        expect(s.prefs.state.announcement?.text).toBe("Not saved. Chat is not available to you right now.");
    });

    test("goes back when Rock answered without its door, saying to try again, or offline while the connection is lost", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[0].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(false);
        expect(s.prefs.state.meNotes.open_dm).toEqual({ kind: "error", text: "Couldn't save.", retry: expect.any(Function) });

        s.offline.value = true;
        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[1].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.prefs.state.meNotes.open_dm).toEqual({ kind: "error", text: "You're offline.", retry: expect.any(Function) });
    });

    test("a page with no door refuses a privacy switch and leaves it as it was", async () => {
        const channels = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        const prefs = createPrefs({ setPrefs: async () => writtenMe({}), channels, isOffline: () => false, notify: () => undefined });
        prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        await prefs.setPersonSetting("open_dm", true);

        expect(prefs.state.me.is_open_dm_allowed).toBe(false);
        expect(prefs.state.meNotes.open_dm?.kind).toBe("error");
    });
});

// The block hands every room control's choice here as it came: the details say so, the header's
// menus leave it out, which tells the save in a toast; the header's Star turns the room's star
// over from its row. Chat settings' switches go where each is owned.
describe("a room's choice and Chat settings' switches", () => {
    test("a menu's notify mode is told in a toast and the details' is not", async () => {
        const s = setup();

        void s.prefs.choose(team, { kind: "notify", mode: "mentions" });
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();
        void s.prefs.choose(prayer, { kind: "notify", mode: "silent", isFromMenu: false });
        s.prefCalls[1].answer.resolve(written(prayer, { notify_mode: "silent" }));
        await settle();

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([[team, { notify_mode: "mentions" }], [prayer, { notify_mode: "silent" }]]);
        expect(s.toasts.map(t => t.text)).toEqual(["You'll be notified only when mentioned"]);
        expect(s.prefs.roomOf(prayer).notes).toEqual({});
    });

    test("Star with no value turns the room's star over, and a value is kept as given", async () => {
        const s = setup([sidebarRow(team, { is_favorite: true }), sidebarRow(prayer)]);

        void s.prefs.choose(team, { kind: "star" });
        void s.prefs.choose(prayer, { kind: "star" });
        void s.prefs.choose(prayer, { kind: "star", isStarred: true, isFromMenu: false });
        // the second change of one room's star waits for the first's answer
        s.prefCalls[1].answer.resolve(written(prayer, { is_favorite: true }));
        await settle();

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([
            [team, { is_favorite: false, hidden: false }],
            [prayer, { is_favorite: true, hidden: false }],
            [prayer, { is_favorite: true, hidden: false }]
        ]);
    });

    test("hide writes Hide, and with no room open nothing is asked", () => {
        const s = setup();

        void s.prefs.choose(null, { kind: "notify", mode: "silent" });
        void s.prefs.choose(team, { kind: "hide", isHidden: true });

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([[team, { is_favorite: false, hidden: true }]]);
    });

    test("the reaction switch goes to the platform and the privacy switches to Rock's door", () => {
        const s = setup();

        void s.prefs.setSwitch("reaction_push", false);
        void s.prefs.setSwitch("open_dm", true);
        void s.prefs.setSwitch("profile_details", false);

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([[null, { reaction_push: false }]]);
        expect(s.doorCalls.map(c => [c.setting, c.value])).toEqual([["open_dm", true], ["profile_details", false]]);
    });
});

describe("once stopped", () => {
    test("no answer still on its way reaches the screen, and nothing more is asked", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setNotifyMode(team, "silent", false);
        void s.prefs.setPersonSetting("open_dm", true);
        s.prefs.stop();
        s.prefCalls[0].answer.resolve(refused());
        s.doorCalls[0].answer.resolve(door("ok", true));
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes).toEqual({});
        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.prefs.state.announcement).toBeNull();
        expect(s.toasts).toEqual([]);

        await s.prefs.setStarred(team, true, false);
        expect(s.prefCalls).toHaveLength(1);
    });

    test("a change waiting behind a save still out is never asked, and resolves", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", false);
        const waiting = s.prefs.setNotifyMode(team, "mentions", false);
        s.prefs.stop();
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        await waiting;
        expect(s.prefCalls).toHaveLength(1);
    });
});

// #region A status save's answer

/** The answer as the shell's one call path reads it off the wire, through the real classification. */
async function statusCallResult(answer: StatusSaveAnswer): Promise<CallResult> {
    const controller = new AbortController();
    const call = createPlatformCall({
        fetch: async (_to, init) => {
            if ("unreached" in answer) {
                if (answer.unreached === "network") {
                    throw new TypeError("Failed to fetch");
                }
                if (answer.unreached === "deadline") {
                    return new Promise<Response>((_, reject) => {
                        init.signal?.addEventListener("abort", () => reject(new DOMException("aborted", "AbortError")));
                        controller.abort();
                    });
                }
                return {
                    ok: true,
                    status: 200,
                    headers: { get: () => null },
                    json: () => new Promise((_, reject) => {
                        init.signal?.addEventListener("abort", () => reject(new DOMException("aborted", "AbortError")));
                        controller.abort();
                    })
                } as unknown as Response;
            }

            const headers = answer.headers ?? {};
            return {
                ok: answer.status >= 200 && answer.status < 300,
                status: answer.status,
                headers: { get: (name: string) => headers[name.toLowerCase()] ?? null },
                json: async () => answer.body === null ? Promise.reject(new SyntaxError("Unexpected token <")) : answer.body
            } as unknown as Response;
        },
        projectUrl: "http://platform",
        publishableKey: "k",
        currentToken: () => "platform-token",
        routes: () => ({ send: "direct" })
    });

    return call.call("chat_set_status", { p_client: null, p_change: { status: "busy" } }, { signal: controller.signal });
}

/** The backoff's wait for a failure count and a random draw, to the millisecond: 10, 20, 40, then every 60 s, times 0.8 to 1.2. */
function backoffMs(failures: number, random: number): number {
    return Math.round(Math.min(60_000, 10_000 * 2 ** (failures - 1)) * (0.8 + 0.4 * random));
}

describe("a status save's answer", () => {
    test("a landing is saved, with what the server read back", () => {
        expect(statusSaveOutcome({ ok: true, data: setStatusAnswers.unchanged }, 0)).toEqual({ isSaved: true, data: setStatusAnswers.unchanged });
    });

    // None of these is the server's answer about the choice: the network, the deadline, the token,
    // the path or the platform, so the choice stays pending and is asked again on the backoff.
    test.each([
        ["no network", "network"],
        ["the deadline's abort", "deadline"],
        ["the deadline's abort while the answer is read", "deadlineWhileReading"],
        ["a 401 for an expired token", "expired"],
        ["a proxy's 408", "proxyTimeout"],
        ["a 500", "serverError"],
        ["a 503 for maintenance", "maintenance"],
        ["the status rule's own 429, which names no Retry-After", "rateLimited"]
    ] as const)("%s is asked again after the backoff for the failure count, spread by its random draw", async (_name, shape) => {
        const result = await statusCallResult(statusSaveFailures[shape]);

        for (const failures of [1, 2, 3, 4, 7]) {
            for (const random of [0, 0.5, 0.999]) {
                const outcome = statusSaveOutcome(result, failures, () => random);
                expect(outcome).toMatchObject({ isSaved: false });
                expect((outcome as { retryInMs?: number }).retryInMs).toBe(backoffMs(failures, random));
            }
        }
    });

    test("the backoff grows 10, 20, 40, then holds at 60 s, each inside 0.8 to 1.2 of its step", async () => {
        const result = await statusCallResult(statusSaveFailures.network);
        const waits = (random: number): number[] => [1, 2, 3, 4, 5].map(n => (statusSaveOutcome(result, n, () => random) as { retryInMs: number }).retryInMs);

        expect(waits(0)).toEqual([8_000, 16_000, 32_000, 48_000, 48_000]);
        expect(waits(0.5)).toEqual([10_000, 20_000, 40_000, 60_000, 60_000]);
        expect(waits(1)).toEqual([12_000, 24_000, 48_000, 72_000, 72_000]);
    });

    test("a gateway's 429 with no code of ours is asked again after the longer of the backoff and its Retry-After", async () => {
        const result = await statusCallResult(statusSaveFailures.gatewayRateLimited);
        expect(result).toMatchObject({ ok: false, status: 429, error: { code: "rpc.unknown" }, retryAfterSeconds: 90 });

        expect(statusSaveOutcome(result, 1, () => 0.5)).toMatchObject({ isSaved: false, retryInMs: 90_000 });
        expect(statusSaveOutcome(result, 4, () => 1)).toMatchObject({ isSaved: false, retryInMs: 90_000 });

        const shortWait = await statusCallResult({ ...statusSaveFailures.gatewayRateLimited, headers: { "retry-after": "5" } });
        expect(statusSaveOutcome(shortWait, 2, () => 0.5)).toMatchObject({ isSaved: false, retryInMs: 20_000 });
    });

    // Read only refuses the save through the writer check while her pages still beat, so asking
    // again would leave her own faces on a choice viewers never see: it is refused, in the words
    // the banner says.
    test.each([
        ["a church's banner", "readOnlyBanner", "Chat is read only tonight while Cedar Hill moves to its new server."],
        ["the server's own sentence", "readOnlyServerSentence", "Chat is read only right now. You can read messages but not send them."],
        ["no sentence at all", "readOnlyNoHint", readOnlyWordsOfOurs]
    ] as const)("a 503 for read only with %s is refused in the banner's words and never asked again", async (_name, shape, words) => {
        const result = await statusCallResult(statusSaveFailures[shape]);

        const outcome = statusSaveOutcome(result, 1, () => 0.5);

        expect(outcome).toEqual({ isSaved: false, failure: { alone: words, besideRetry: words, canRetry: false } });
    });

    test.each([
        ["a Busy end that has passed", "badRequest", "Chat could not understand that request."],
        ["a person who may not set a status", "notLivePerson", "Chat isn't available for your account."]
    ] as const)("the platform's answer about the choice, %s, is refused with its own words", async (_name, shape, words) => {
        const outcome = statusSaveOutcome(await statusCallResult(statusSaveFailures[shape]), 1, () => 0.5);

        expect(outcome).toEqual({ isSaved: false, failure: { alone: words, besideRetry: words, canRetry: false } });
    });

    // Asking again would only meet the same answer: the same schema cache until a reload, the same
    // proxy rule for the same request.
    test.each([
        ["a 404 from a schema cache that has not loaded the function", "notInSchemaCache"],
        ["a proxy's 403", "proxyForbidden"],
        ["a proxy's 413", "proxyTooLarge"]
    ] as const)("%s, with no code of ours, is refused by name with the generic sentence", async (_name, shape) => {
        const outcome = statusSaveOutcome(await statusCallResult(statusSaveFailures[shape]), 1, () => 0.5);

        expect(outcome).toEqual({ isSaved: false, failure: expect.objectContaining({ alone: "Something went wrong in chat. Try again in a moment." }) });
        expect("retryInMs" in outcome).toBe(false);
    });

    test("a refusal of a code this release does not know, carrying a sentence, is shown in that sentence", () => {
        const outcome = statusSaveOutcome({ ok: false, status: 409, error: { code: "rpc.status_conflict", severity: "failed", text: "Your status changed somewhere else." }, retryAfterSeconds: null }, 1);

        expect(outcome).toEqual({ isSaved: false, failure: expect.objectContaining({ alone: "Your status changed somewhere else." }) });
        expect("retryInMs" in outcome).toBe(false);
    });
});

// #endregion A status save's answer
