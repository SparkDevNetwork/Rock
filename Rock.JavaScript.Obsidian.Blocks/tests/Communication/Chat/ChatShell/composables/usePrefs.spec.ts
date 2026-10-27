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
// SavePersonSetting door. Hiding unstars and starring unhides in the same one patch. A save says
// Saving, then Saved, Saved, syncing only where the door answers pending, or Not saved, when the
// control goes back as it was and the row's note carries the server's own sentence. A change made
// from the header's menu says so in a toast instead. An answer to a save the person has since
// changed again moves nothing, and once stopped no answer reaches the screen.
//
// The fakes answer in the platform's and the door's own shapes: chat_set_prefs's
// {channel: {channel_id, is_favorite, hidden, notify_mode, notify_mode_effective}, me: {...}},
// its refusals as the shell's one call path classifies them, and ChatPersonSettingResultBag's
// {code, isPending, message}.
/* eslint-disable @typescript-eslint/naming-convention */
import { createPrefs, PersonSettingKey, PersonSettingResult, PrefsCallResult } from "../../../../../src/Communication/Chat/ChatShell/composables/usePrefs.partial";
import { createChannelStore } from "../../../../../src/Communication/Chat/ChatShell/stores/channelStore.partial";
import { SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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
        expect(s.prefs.roomOf(team).save?.state).toBe("saving");

        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();

        expect(s.prefs.roomOf(team).save?.state).toBe("saved");
        expect(s.prefs.state.announcement?.text).toBe("Mentions only. Saved.");
    });

    test("asks nothing when the mode chosen is the one the room already has", async () => {
        const s = setup();

        await s.prefs.setNotifyMode(team, "all", false);

        expect(s.prefCalls).toEqual([]);
        expect(s.prefs.roomOf(team).save).toBeNull();
    });

    test("goes back as it was on a refusal, the server's own sentence under the choices", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("all");
        expect(s.prefs.roomOf(team).save?.state).toBe("refused");
        expect(s.prefs.roomOf(team).notes.notify).toEqual({ kind: "error", text: readOnlySentence });
        expect(s.prefs.state.announcement?.text).toBe(`Not saved. ${readOnlySentence}`);
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
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).save?.state).toBe("saving");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();

        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).save?.state).toBe("saved");
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
    test("starring writes the star alone and moves the room into Starred at once", async () => {
        const s = setup();

        void s.prefs.setStarred(prayer, true, false);

        expect(rowOf(s, prayer)?.is_favorite).toBe(true);
        // starred rooms sort first, as the platform's sidebar sorts them
        expect(s.channels.rows[0].channel_id).toBe(prayer);
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ is_favorite: true }]);

        s.prefCalls[0].answer.resolve(written(prayer, { is_favorite: true }));
        await settle();

        expect(s.prefs.roomOf(prayer).save?.state).toBe("saved");
        expect(s.prefs.state.announcement?.text).toBe("Starred. Saved.");
    });

    test("hiding a starred room unstars it in the same one patch, and says so under Hide", async () => {
        const s = setup([sidebarRow(team, { is_favorite: true })]);

        void s.prefs.setHidden(team, true);

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ hidden: true, is_favorite: false }]);
        expect(rowOf(s, team)?.is_favorite).toBe(false);
        expect(rowOf(s, team)?.hidden).toBe(true);

        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();

        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden, and taken out of Starred. It leaves your list when you open another conversation." });
        expect(s.prefs.state.announcement?.text).toBe("Hidden from your sidebar, and unstarred. Saved.");
    });

    test("hiding a room that is not starred writes Hide alone", async () => {
        const s = setup();

        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ hidden: true }]);
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden. It leaves your list when you open another conversation." });
    });

    test("starring a hidden room unhides it in the same one patch, and says so under Star", async () => {
        const s = setup();
        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();

        void s.prefs.setStarred(team, true, false);

        expect(s.prefCalls[1].changes).toEqual({ is_favorite: true, hidden: false });
        expect(rowOf(s, team)?.hidden).toBe(false);

        s.prefCalls[1].answer.resolve(written(team, { is_favorite: true }));
        await settle();

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
        expect(s.prefs.roomOf(team).save?.state).toBe("refused");
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "error", text: readOnlySentence });
    });

    test("from the header, a star is toasted, and a refusal's sentence is toasted", async () => {
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

        expect(s.toasts.map(t => [t.text, t.icon])).toEqual([["Starred", "star"], ["Removed from Starred", "star"], [readOnlySentence, "alert"]]);
        expect(rowOf(s, team)?.is_favorite).toBe(false);
    });

    test("a lost connection's refusal says the person is offline", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, false);
        s.prefCalls[0].answer.resolve(refused("rpc.transport", null));
        await settle();

        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "error", text: "You're offline. Try again when you're back." });
    });
});

describe("the person's own switches", () => {
    test("start from the bootstrap's me", () => {
        const s = setup();

        s.prefs.setMe({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, unread_mentions: [] } as never);

        expect(s.prefs.state.me).toEqual({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false });
    });

    test("the reaction switch writes chat_set_prefs with no room", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setReactionPush(false);

        expect(s.prefs.state.me.reaction_push).toBe(false);
        expect(s.prefCalls).toEqual([expect.objectContaining({ channelId: null, changes: { reaction_push: false } })]);

        s.prefCalls[0].answer.resolve(writtenMe({ reaction_push: false }));
        await settle();

        expect(s.prefs.state.meSave?.state).toBe("saved");
        expect(s.prefs.state.announcement?.text).toBe("Reactions to my messages, off. Saved.");
    });

    test("one switch's answer leaves another's Saving standing, and the head says Saving until both are in", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setReactionPush(false);
        void s.prefs.setPersonSetting("open_dm", true);
        expect(s.prefs.state.meSaving).toEqual({ reaction_push: true, open_dm: true });

        s.doorCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.prefs.state.meSaving).toEqual({ reaction_push: true });
        expect(s.prefs.state.meSave?.state).toBe("saving");

        s.prefCalls[0].answer.resolve(writtenMe({ reaction_push: false }));
        await settle();

        expect(s.prefs.state.meSaving).toEqual({});
        expect(s.prefs.state.meSave?.state).toBe("saved");
    });

    test("a sidebar load while a switch's save is out leaves that switch as the person set it", () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.prefs.setMe({ show_profile_details: false, is_open_dm_allowed: false, reaction_push: false });

        expect(s.prefs.state.me).toEqual({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false });
    });

    test("a privacy switch goes through Rock's door, one setting a call", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.doorCalls.map(c => [c.setting, c.value])).toEqual([["open_dm", true]]);
        expect(s.prefCalls).toEqual([]);
        expect(s.prefs.state.meSave?.state).toBe("saving");

        s.doorCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.prefs.state.meSave?.state).toBe("saved");
        expect(s.prefs.state.announcement?.text).toBe("Anyone can message me, on. Saved.");
    });

    test("says Saved, syncing only when the door answers pending, with a note under the switch", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("profile_details", false);
        s.doorCalls[0].answer.resolve(door("ok", true));
        await settle();

        expect(s.prefs.state.me.show_profile_details).toBe(false);
        expect(s.prefs.state.meSave?.state).toBe("pending");
        expect(s.prefs.state.meNotes.profile_details).toEqual({ kind: "pending", text: "Saved. It can take a minute to reach everyone." });
    });

    test("goes back on the door's refusal with the door's own sentence", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("profile_details", false);
        s.doorCalls[0].answer.resolve(door("age_restricted", false, "Chat is not available to you right now."));
        await settle();

        expect(s.prefs.state.me.show_profile_details).toBe(true);
        expect(s.prefs.state.meSave?.state).toBe("refused");
        expect(s.prefs.state.meNotes.profile_details).toEqual({ kind: "error", text: "Chat is not available to you right now." });
    });

    test("goes back when Rock answered without its door, saying to try again, or offline while the connection is lost", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[0].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(false);
        expect(s.prefs.state.meNotes.open_dm).toEqual({ kind: "error", text: "Couldn't save. Try again in a moment." });

        s.offline.value = true;
        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[1].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.prefs.state.meNotes.open_dm).toEqual({ kind: "error", text: "You're offline. Try again when you're back." });
    });

    test("a page with no door refuses a privacy switch and leaves it as it was", async () => {
        const channels = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        const prefs = createPrefs({ setPrefs: async () => writtenMe({}), channels, isOffline: () => false, notify: () => undefined });
        prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        await prefs.setPersonSetting("open_dm", true);

        expect(prefs.state.me.is_open_dm_allowed).toBe(false);
        expect(prefs.state.meSave?.state).toBe("refused");
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
        expect(s.prefs.roomOf(prayer).save?.state).toBe("saved");
    });

    test("Star with no value turns the room's star over, and a value is kept as given", () => {
        const s = setup([sidebarRow(team, { is_favorite: true }), sidebarRow(prayer)]);

        void s.prefs.choose(team, { kind: "star" });
        void s.prefs.choose(prayer, { kind: "star" });
        void s.prefs.choose(prayer, { kind: "star", isStarred: true, isFromMenu: false });

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([
            [team, { is_favorite: false }],
            [prayer, { is_favorite: true }],
            [prayer, { is_favorite: true }]
        ]);
    });

    test("hide writes Hide, and with no room open nothing is asked", () => {
        const s = setup();

        void s.prefs.choose(null, { kind: "notify", mode: "silent" });
        void s.prefs.choose(team, { kind: "hide", isHidden: true });

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([[team, { hidden: true }]]);
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
        expect(s.prefs.roomOf(team).save?.state).toBe("saving");
        expect(s.prefs.state.meSave?.state).toBe("saving");
        expect(s.toasts).toEqual([]);

        await s.prefs.setStarred(team, true, false);
        expect(s.prefCalls).toHaveLength(1);
    });
});
