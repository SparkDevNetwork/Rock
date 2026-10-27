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
// The sidebar: bold on the person's own signal, reloaded for a channel it does not hold, and
// kept in step with the platform after each save of the read position.
import { createChannelStore } from "../../../../../src/Communication/Chat/ChatShell/stores/channelStore.partial";
import { SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const one = "c0000001-0000-4000-8000-000000000000";
const two = "c0000002-0000-4000-8000-000000000000";
const hidden = "c0000009-0000-4000-8000-000000000000";

function row(channelId: string, extra: Partial<SidebarRow> = {}): SidebarRow {
    return {
        channel_id: channelId,
        name: channelId,
        icon_url: null,
        channel_type: "group",
        is_public: false,
        always_shown: false,
        is_member: true,
        is_favorite: false,
        notify_mode: "all",
        is_leader: false,
        can_post_announcements: false,
        can_mention_all: false,
        last_message_id: 10,
        last_message_at: "2026-09-23T10:00:00Z",
        last_message_type: "text",
        last_sender_nick_name: "Ada",
        last_sender_last_name: "Lovelace",
        last_sender_avatar_url: null,
        last_sender_listed: true,
        last_message_preview: "hi",
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
        title: "Room",
        preview_line: "",
        ...extra
    };
}

function store(): { store: ReturnType<typeof createChannelStore>, reloads: () => number } {
    let reloads = 0;
    const s = createChannelStore({ reloadSidebar: () => reloads++, retryFirstLoad: () => undefined });
    s.setSidebar([row(one), row(two)]);
    return { store: s, reloads: () => reloads };
}

describe("createChannelStore", () => {
    test("is loading until its first sidebar arrives, an empty one included, and stays loaded", () => {
        const s = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        expect(s.loadState).toBe("loading");

        s.setSidebar([]);
        expect(s.loadState).toBe("loaded");

        s.setLoadFailed();
        s.setSidebar([row(one)]);
        expect(s.loadState).toBe("loaded");
    });

    test("a previewed room's row does not make the sidebar loaded", () => {
        const s = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });

        s.setPreview(row(hidden));

        expect(s.rows).toHaveLength(1);
        expect(s.loadState).toBe("loading");
    });

    test("a failed first sidebar is failed until a retry, which loads again and reads it again", () => {
        let reloads = 0;
        const s = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => reloads++ });

        s.setLoadFailed();
        expect(s.loadState).toBe("failed");

        s.retryLoad();
        expect(s.loadState).toBe("loading");
        expect(reloads).toBe(1);

        s.setSidebar([row(one)]);
        expect(s.loadState).toBe("loaded");
    });

    test("a reload that fails after the first sidebar arrived keeps the rows loaded, and a retry asks nothing", () => {
        const { store: s, reloads } = store();

        s.setLoadFailed();
        s.retryLoad();

        expect(s.loadState).toBe("loaded");
        expect(reloads()).toBe(0);
    });

    test("a signal for a channel in the sidebar bolds it", () => {
        const { store: s, reloads } = store();

        s.applyPersonalEvent("channel.unread", { channel_id: two, message_id: 11 });

        expect(s.rows.find(r => r.channel_id === two)?.is_unread).toBe(true);
        expect(s.rows.find(r => r.channel_id === two)?.last_message_id).toBe(11);
        expect(reloads()).toBe(0);
    });

    test("a signal for a channel not in the sidebar reloads the sidebar", () => {
        const { store: s, reloads } = store();

        s.applyPersonalEvent("channel.unread", { channel_id: hidden, message_id: 12 });

        expect(reloads()).toBe(1);
        expect(s.rows.map(r => r.channel_id)).toEqual([one, two]);
    });

    test("a signal for the channel on screen does not bold it", () => {
        const { store: s } = store();
        s.setActive(one);

        s.applyPersonalEvent("channel.unread", { channel_id: one, message_id: 11 });

        expect(s.rows.find(r => r.channel_id === one)?.is_unread).toBe(false);
    });

    test("a membership change reloads the sidebar", () => {
        const { store: s, reloads } = store();

        s.applyPersonalEvent("membership.changed", { channel_id: one });

        expect(reloads()).toBe(1);
    });

    test("a room's name, description or kind changed reloads the sidebar", () => {
        const { store: s, reloads } = store();

        s.applyPersonalEvent("channel.updated", { channel_id: one });

        expect(reloads()).toBe(1);
    });

    test("an event name it does not know is ignored", () => {
        const { store: s, reloads } = store();

        s.applyPersonalEvent("something.new", { channel_id: one });
        s.applyPersonalEvent("channel.unread", null);

        expect(reloads()).toBe(0);
        expect(s.rows.every(r => !r.is_unread)).toBe(true);
    });

    test("a save that came back with a newer last message leaves the channel bold", () => {
        const { store: s } = store();

        s.applyMarkRead(one, { read_cursor: 10, last_message_id: 11 });

        const saved = s.rows.find(r => r.channel_id === one);
        expect(saved?.is_unread).toBe(true);
        expect(saved?.read_cursor).toBe(10);
        expect(saved?.last_message_id).toBe(11);
    });

    test("a save that comes back while its channel is on screen again does not bold it", () => {
        const { store: s } = store();
        s.setActive(one);

        s.applyMarkRead(one, { read_cursor: 10, last_message_id: 11 });

        expect(s.rows.find(r => r.channel_id === one)?.is_unread).toBe(false);
        expect(s.rows.find(r => r.channel_id === one)?.read_cursor).toBe(10);
    });

    test("a save that caught up clears the bold", () => {
        const { store: s } = store();
        s.applyPersonalEvent("channel.unread", { channel_id: two, message_id: 11 });

        s.applyMarkRead(two, { read_cursor: 11, last_message_id: 11 });

        expect(s.rows.find(r => r.channel_id === two)?.is_unread).toBe(false);
    });

    test("a save sent after the row heard of a message since deleted takes the answer's walked-back pointer and clears the bold and mentions", () => {
        const { store: s } = store();
        s.setSidebar([row(one, { last_message_id: 11, read_cursor: 11 }), row(two)]);
        s.applyPersonalEvent("channel.unread", { channel_id: one, message_id: 12 });
        s.applyPersonalEvent("channel.mention", { channel_id: one, message_id: 12, parent_id: null, in_channel: true, all: false });

        // 12 is deleted; the person opens the room, reads to 11 and leaves
        const sentAt = s.saveSent();
        s.applyMarkRead(one, { read_cursor: 11, last_message_id: 11 }, sentAt);

        expect(s.rows.find(r => r.channel_id === one)).toEqual(expect.objectContaining({ last_message_id: 11, is_unread: false, mention_count: 0 }));
    });

    test("a save sent before a newer message reached the row keeps that message and its bold", () => {
        const { store: s } = store();
        s.setSidebar([row(one, { last_message_id: 11, read_cursor: 10 }), row(two)]);

        const sentAt = s.saveSent();
        s.applyPersonalEvent("channel.unread", { channel_id: one, message_id: 12 });
        s.applyMarkRead(one, { read_cursor: 11, last_message_id: 11 }, sentAt);

        expect(s.rows.find(r => r.channel_id === one)).toEqual(expect.objectContaining({ last_message_id: 12, read_cursor: 11, is_unread: true }));
    });

    test("a mention by name in the conversation adds one to the row's mentions", () => {
        const { store: s } = store();

        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 11, parent_id: null, in_channel: true, all: false });
        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 12, parent_id: null, in_channel: true, all: false });

        const mentioned = s.rows.find(r => r.channel_id === two);
        expect(mentioned?.mention_count).toBe(2);
        expect(mentioned?.all_mention_count).toBe(0);
    });

    test("an @all mention adds one to the row's @all mentions, never its mentions by name", () => {
        const { store: s } = store();

        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 11, parent_id: null, in_channel: true, all: true });

        const mentioned = s.rows.find(r => r.channel_id === two);
        expect(mentioned?.all_mention_count).toBe(1);
        expect(mentioned?.mention_count).toBe(0);
    });

    test("a mention in the conversation on screen moves nothing, but one in another conversation does", () => {
        const { store: s } = store();
        s.setActive(one);

        s.applyPersonalEvent("channel.mention", { channel_id: one, message_id: 11, parent_id: null, in_channel: true, all: false });
        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 11, parent_id: null, in_channel: true, all: false });

        expect(s.rows.find(r => r.channel_id === one)?.mention_count).toBe(0);
        expect(s.rows.find(r => r.channel_id === two)?.mention_count).toBe(1);
    });

    test("a mention kept to a thread moves nothing in the sidebar, which counts the conversation's", () => {
        const { store: s } = store();

        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 11, parent_id: 5, in_channel: false, all: false });
        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 12, parent_id: 5, in_channel: true, all: false });

        expect(s.rows.find(r => r.channel_id === two)?.mention_count).toBe(1);
    });

    test("a save that caught up clears the row's mentions, by name and @all, and one behind keeps them", () => {
        const { store: s } = store();
        s.setSidebar([row(one, { mention_count: 2, all_mention_count: 1 }), row(two, { mention_count: 3, all_mention_count: 2 })]);

        s.applyMarkRead(one, { read_cursor: 11, last_message_id: 11 });
        s.applyMarkRead(two, { read_cursor: 10, last_message_id: 11 });

        expect(s.rows.find(r => r.channel_id === one)).toEqual(expect.objectContaining({ mention_count: 0, all_mention_count: 0 }));
        expect(s.rows.find(r => r.channel_id === two)).toEqual(expect.objectContaining({ mention_count: 3, all_mention_count: 2 }));
    });

    test("a mention in a message already read, as an edit naming the person brings, moves nothing", () => {
        const { store: s } = store();

        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 10, parent_id: null, in_channel: true, all: false });
        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 9, parent_id: null, in_channel: true, all: true });

        expect(s.rows.find(r => r.channel_id === two)).toEqual(expect.objectContaining({ mention_count: 0, all_mention_count: 0 }));
    });

    test("a fresh sidebar replaces the rows", () => {
        const { store: s } = store();

        s.setSidebar([row(hidden, { is_unread: true })]);

        expect(s.rows.map(r => r.channel_id)).toEqual([hidden]);
    });
});

// A direct message's row is kept live by dm.preview, sent to every member's topic, the sender's
// own included, whenever the room's last timeline message changes: a send, an edit of the newest,
// a delete or hide walking back to the one before, or to none. Its payload, as the platform's
// signal builds it: the message, its preview as text, when it was sent, the sender's current
// names, whether it is this reader's, and the signal's own id.
describe("dm.preview", () => {
    const dmOne = "d0000001-0000-4000-8000-000000000000";
    const dmTwo = "d0000002-0000-4000-8000-000000000000";
    const starredRoom = "c0000005-0000-4000-8000-000000000000";

    /** A direct message's row, read up to its last message. */
    function dm(channelId: string, extra: Partial<SidebarRow> = {}): SidebarRow {
        return row(channelId, { name: null, channel_type: "dm", dm_people: [{ person_alias_guid: "ben-okafor", nick_name: "Ben", last_name: "Okafor", avatar_url: null }], dm_other_count: 1, room_kind: "person", title: "Ben Okafor", ...extra });
    }

    /** dm.preview exactly as the platform sends it. */
    function preview(channelId: string, messageId: number | null, extra: Record<string, unknown> = {}): Record<string, unknown> {
        return {
            channel_id: channelId,
            message_id: messageId,
            preview: messageId === null ? null : `message ${messageId}`,
            sent_at: messageId === null ? null : "2026-10-19T11:00:00.123456+00:00",
            sender: messageId === null ? null : { nick_name: "Ben", last_name: "Okafor" },
            is_own: false,
            posted_by_rock: false,
            preview_line: messageId === null ? "No messages yet" : `message ${messageId}`,
            id: "5e000001-0000-4000-8000-000000000000",
            ...extra
        };
    }

    function dmStore(rows: SidebarRow[]): { store: ReturnType<typeof createChannelStore>, reloads: () => number } {
        let reloads = 0;
        const s = createChannelStore({ reloadSidebar: () => reloads++, retryFirstLoad: () => undefined });
        s.setSidebar(rows);
        return { store: s, reloads: () => reloads };
    }

    test("counts each fresh sidebar read and not a live preview, so the sidebar can tell a reread from news", () => {
        const { store: s } = dmStore([dm(dmOne)]);
        expect(s.sidebarReads).toBe(1);

        s.applyPersonalEvent("dm.preview", preview(dmOne, 11));
        expect(s.sidebarReads).toBe(1);

        s.setSidebar([dm(dmOne, { last_message_id: 12 })]);
        expect(s.sidebarReads).toBe(2);
    });

    test("moves the row's preview, time, pointer, sender and own flag", () => {
        const { store: s, reloads } = dmStore([dm(dmOne)]);

        s.applyPersonalEvent("dm.preview", preview(dmOne, 11));

        expect(s.rows[0]).toEqual(expect.objectContaining({
            last_message_id: 11,
            last_message_at: "2026-10-19T11:00:00.123456+00:00",
            last_message_preview: "message 11",
            last_sender_nick_name: "Ben",
            last_sender_last_name: "Okafor",
            last_sender_avatar_url: null,
            last_message_is_own: false
        }));
        expect(reloads()).toBe(0);
    });

    test("takes the line the platform worked out for this reader, as sent, the reader's own and an emptied room's included", () => {
        const { store: s } = dmStore([dm(dmOne), dm(dmTwo)]);

        s.applyPersonalEvent("dm.preview", preview(dmOne, 11, { is_own: true, preview_line: "You: message 11" }));
        s.applyPersonalEvent("dm.preview", preview(dmTwo, null));

        expect(s.rows.find(r => r.channel_id === dmOne)?.preview_line).toBe("You: message 11");
        expect(s.rows.find(r => r.channel_id === dmTwo)?.preview_line).toBe("No messages yet");
    });

    test("re-sorts the rows by the platform's own order: starred first, newest message, then the id", () => {
        const { store: s } = dmStore([
            row(starredRoom, { is_favorite: true, last_message_at: "2026-10-19T08:00:00Z" }),
            dm(dmOne, { last_message_at: "2026-10-19T10:00:00Z" }),
            dm(dmTwo, { last_message_at: "2026-10-19T09:00:00Z" }),
            row(one, { last_message_at: "2026-10-19T09:30:00Z" })
        ]);

        s.applyPersonalEvent("dm.preview", preview(dmTwo, 11));

        expect(s.rows.map(r => r.channel_id)).toEqual([starredRoom, dmTwo, dmOne, one]);

        // two at the same time fall back to the room's id, as the platform breaks the tie
        s.applyPersonalEvent("dm.preview", preview(dmOne, 12));
        expect(s.rows.map(r => r.channel_id)).toEqual([starredRoom, dmOne, dmTwo, one]);
    });

    test("bolds the row only when the message is not the reader's, not on screen and past their place", () => {
        const { store: s } = dmStore([dm(dmOne), dm(dmTwo), dm(starredRoom, { read_cursor: 12 }), dm(hidden)]);
        s.setActive(dmTwo);

        s.applyPersonalEvent("dm.preview", preview(dmOne, 11, { is_own: true }));
        s.applyPersonalEvent("dm.preview", preview(dmTwo, 11));
        // an edit of the newest message, which the reader has already read
        s.applyPersonalEvent("dm.preview", preview(starredRoom, 12));
        s.applyPersonalEvent("dm.preview", preview(hidden, 11));

        const unread = (id: string): boolean | undefined => s.rows.find(r => r.channel_id === id)?.is_unread;
        expect(unread(dmOne)).toBe(false);
        expect(s.rows.find(r => r.channel_id === dmOne)?.last_message_is_own).toBe(true);
        expect(unread(dmTwo)).toBe(false);
        expect(unread(starredRoom)).toBe(false);
        expect(unread(hidden)).toBe(true);
    });

    test("a walk-back to an older message recomputes the bold, so a deleted unread message unbolds the row", () => {
        const { store: s } = dmStore([dm(dmOne, { last_message_id: 11, read_cursor: 10, is_unread: true })]);

        s.applyPersonalEvent("dm.preview", preview(dmOne, 10));

        expect(s.rows[0]).toEqual(expect.objectContaining({ last_message_id: 10, last_message_preview: "message 10", is_unread: false }));
    });

    test("a walk-back to nothing empties the row, which then draws No messages yet", () => {
        const { store: s } = dmStore([dm(dmOne, { last_message_id: 11, read_cursor: 10, is_unread: true })]);

        s.applyPersonalEvent("dm.preview", preview(dmOne, null));

        expect(s.rows[0]).toEqual(expect.objectContaining({
            last_message_id: null,
            last_message_at: null,
            last_message_preview: null,
            last_sender_nick_name: null,
            last_sender_last_name: null,
            last_sender_avatar_url: null,
            last_message_is_own: false,
            is_unread: false
        }));
    });

    test("Rock's post in the reader's name bolds the row live and reads as theirs, where a message they typed does not bold it", () => {
        const { store: s } = dmStore([dm(dmOne), dm(dmTwo)]);

        s.applyPersonalEvent("dm.preview", preview(dmOne, 11, { is_own: true, posted_by_rock: true }));
        s.applyPersonalEvent("dm.preview", preview(dmTwo, 11, { is_own: true, posted_by_rock: false }));

        expect(s.rows.find(r => r.channel_id === dmOne)).toEqual(expect.objectContaining({ is_unread: true, last_message_is_own: true, last_message_posted_by_rock: true }));
        expect(s.rows.find(r => r.channel_id === dmTwo)).toEqual(expect.objectContaining({ is_unread: false, last_message_is_own: true, last_message_posted_by_rock: false }));
    });

    test("a walk-back to a message the reader typed leaves the row read, and to one Rock posted in their name bolds it", () => {
        const { store: s } = dmStore([
            dm(dmOne, { last_message_id: 12, read_cursor: 10, is_unread: true }),
            dm(dmTwo, { last_message_id: 12, read_cursor: 10, is_unread: true })
        ]);

        s.applyPersonalEvent("dm.preview", preview(dmOne, 11, { is_own: true }));
        s.applyPersonalEvent("dm.preview", preview(dmTwo, 11, { is_own: true, posted_by_rock: true }));

        expect(s.rows.find(r => r.channel_id === dmOne)?.is_unread).toBe(false);
        expect(s.rows.find(r => r.channel_id === dmTwo)?.is_unread).toBe(true);
    });

    test("a read from another device short of a last message the reader typed leaves the row read", () => {
        // loaded as the platform answers it: their own last message, their place below it, not bold
        const { store: s } = dmStore([dm(dmOne, { last_message_id: 10, read_cursor: 8, is_unread: false, last_message_is_own: true })]);

        s.applyPersonalEvent("channel.read", { channel_id: dmOne, read_cursor: 9, last_message_id: 10, id: "5e000009-0000-4000-8000-000000000000" });
        expect(s.rows[0].is_unread).toBe(false);

        // and the device's own save answers the same
        s.applyMarkRead(dmOne, { read_cursor: 9, last_message_id: 10 });
        expect(s.rows[0].is_unread).toBe(false);
    });

    test("a save's answer that comes back after a newer message keeps the newer pointer and its bold, on this device and from another", () => {
        const { store: s } = dmStore([dm(dmOne, { last_message_id: 10, read_cursor: 10 }), dm(dmTwo, { last_message_id: 10, read_cursor: 10 })]);

        // the person saves 11 as they leave; Ben's 12 lands before the save's answer does
        s.applyPersonalEvent("dm.preview", preview(dmOne, 11));
        s.applyPersonalEvent("dm.preview", preview(dmOne, 12));
        s.applyMarkRead(dmOne, { read_cursor: 11, last_message_id: 11 });

        // and the same order on another device, through channel.read
        s.applyPersonalEvent("dm.preview", preview(dmTwo, 11));
        s.applyPersonalEvent("dm.preview", preview(dmTwo, 12));
        s.applyPersonalEvent("channel.read", { channel_id: dmTwo, read_cursor: 11, last_message_id: 11, id: "5e00000c-0000-4000-8000-000000000000" });

        for (const id of [dmOne, dmTwo]) {
            expect(s.rows.find(r => r.channel_id === id)).toEqual(expect.objectContaining({ last_message_id: 12, read_cursor: 11, is_unread: true }));
        }
    });

    test("for a room the sidebar does not hold, reloads only when the signal carries a message", () => {
        const { store: s, reloads } = dmStore([dm(dmOne)]);

        s.applyPersonalEvent("dm.preview", preview(hidden, null));
        expect(reloads()).toBe(0);

        // a hidden direct message comes back on a new message, as the sidebar's hide rule reads
        s.applyPersonalEvent("dm.preview", preview(hidden, 12));
        expect(reloads()).toBe(1);
    });

    // The platform lists a direct message only once a message is written in it, so the conversation
    // the person is starting is open on its stand-in row until its first message's dm.preview.
    describe("a direct message not yet written in", () => {
        const fresh = "d0000003-0000-4000-8000-000000000000";
        const empty: Partial<SidebarRow> = { last_message_id: null, last_message_at: null, last_message_type: null, last_message_preview: null, last_sender_nick_name: null, last_sender_last_name: null, read_cursor: null, preview_line: "" };

        /** The page's sidebar in the platform's order: two direct messages with messages and a channel. */
        function loaded(): ReturnType<typeof createChannelStore> {
            return dmStore([
                dm(dmOne, { last_message_at: "2026-10-19T10:00:00Z" }),
                row(one, { last_message_at: "2026-10-19T09:30:00Z" }),
                dm(dmTwo, { last_message_at: "2026-10-19T09:00:00Z" })
            ]).store;
        }

        test("is held for the conversation on screen but not listed, through a reread that leaves it out", () => {
            const s = loaded();

            s.setPreview(dm(fresh, empty));
            s.setActive(fresh);
            s.setSidebar(s.listed.slice());

            expect(s.rows.find(r => r.channel_id === fresh)?.title).toBe("Ben Okafor");
            expect(s.listed.map(r => r.channel_id)).toEqual([dmOne, one, dmTwo]);
        });

        test("its first message lists it first, never below the others, and ends the stand-in", () => {
            const s = loaded();
            s.setPreview(dm(fresh, empty));
            s.setActive(fresh);

            s.applyPersonalEvent("dm.preview", preview(fresh, 20, { is_own: true, preview_line: "You: message 20" }));

            expect(s.listed.map(r => r.channel_id)).toEqual([fresh, dmOne, one, dmTwo]);
            expect(s.previewChannelId).toBeNull();

            // the next sidebar holds it as the platform lists it now
            s.setSidebar(s.rows.map(r => ({ ...r })));
            expect(s.listed.map(r => r.channel_id)).toEqual([fresh, dmOne, one, dmTwo]);
        });

        test("a signal that it still has nothing keeps it unlisted", () => {
            const s = loaded();
            s.setPreview(dm(fresh, empty));

            s.applyPersonalEvent("dm.preview", preview(fresh, null));

            expect(s.listed.some(r => r.channel_id === fresh)).toBe(false);
            expect(s.previewChannelId).toBe(fresh);
        });

        test("a channel's stand-in is still listed, and a direct message emptied by deletes stays listed", () => {
            const s = loaded();

            s.setPreview(row(two, { ...empty, is_member: false }));
            s.applyPersonalEvent("dm.preview", preview(dmTwo, null));

            // the emptied one sorts with no message, beside the stand-in by the rooms' ids
            expect(s.listed.map(r => r.channel_id)).toEqual([dmOne, one, two, dmTwo]);
        });
    });
});

// channel.read reaches the person's other devices when one of them saves a read position that
// moved: the save's own answer, applied exactly as the saving device applies it.
describe("channel.read", () => {
    test("applies as a save: caught up clears the bold and the counts, one behind keeps them", () => {
        const { store: s } = store();
        s.setSidebar([
            row(one, { last_message_id: 12, read_cursor: 10, is_unread: true, mention_count: 2, all_mention_count: 1 }),
            row(two, { last_message_id: 12, read_cursor: 10, is_unread: true, mention_count: 2, all_mention_count: 1 })
        ]);

        s.applyPersonalEvent("channel.read", { channel_id: one, read_cursor: 12, last_message_id: 12, id: "5e000002-0000-4000-8000-000000000000" });
        s.applyPersonalEvent("channel.read", { channel_id: two, read_cursor: 11, last_message_id: 12, id: "5e000003-0000-4000-8000-000000000000" });

        expect(s.rows.find(r => r.channel_id === one)).toEqual(expect.objectContaining({ read_cursor: 12, is_unread: false, mention_count: 0, all_mention_count: 0 }));
        expect(s.rows.find(r => r.channel_id === two)).toEqual(expect.objectContaining({ read_cursor: 11, is_unread: true, mention_count: 2, all_mention_count: 1 }));
    });

    test("a channel whose last message the reader typed bolds on a newer message from someone else, and a read short of it keeps the bold", () => {
        const { store: s } = store();
        s.setSidebar([row(one, { last_message_id: 10, read_cursor: 8, is_unread: false, last_message_is_own: true })]);

        s.applyPersonalEvent("channel.unread", { channel_id: one, message_id: 11, id: "5e00000a-0000-4000-8000-000000000000" });
        s.applyPersonalEvent("channel.read", { channel_id: one, read_cursor: 10, last_message_id: 11, id: "5e00000b-0000-4000-8000-000000000000" });

        expect(s.rows[0]).toEqual(expect.objectContaining({ last_message_id: 11, last_message_is_own: false, is_unread: true }));
    });

    test("carries the room's pointer, so a channel row behind it stays bold after a read short of the newest", () => {
        const { store: s } = store();
        // this device heard of message 11 only; the room has since reached 12
        s.setSidebar([row(one, { last_message_id: 11, read_cursor: 10, is_unread: true })]);

        s.applyPersonalEvent("channel.read", { channel_id: one, read_cursor: 11, last_message_id: 12, id: "5e000004-0000-4000-8000-000000000000" });

        expect(s.rows[0]).toEqual(expect.objectContaining({ read_cursor: 11, last_message_id: 12, is_unread: true }));
    });
});

// A sidebar reload in flight keeps every personal event that arrived meanwhile and replays them
// over its answer. Each states a value except channel.mention, which adds one: replayed, it adds
// only for a message above the answer's own pointer for that row, which the answer did not count.
describe("a replayed channel.mention", () => {
    test("adds only above the answer's own pointer, even after a replayed signal has moved the row's", () => {
        const { store: s } = store();
        s.setSidebar([row(two, { last_message_id: 10, read_cursor: 8, is_unread: true, mention_count: 1 })]);

        // counted by the answer already
        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 9, parent_id: null, in_channel: true, all: false, id: "5e000005-0000-4000-8000-000000000000" }, true);
        expect(s.rows[0].mention_count).toBe(1);

        // after the answer was read: the unread signal moves the row's pointer to it, and its
        // mention still counts
        s.applyPersonalEvent("channel.unread", { channel_id: two, message_id: 11, id: "5e000006-0000-4000-8000-000000000000" }, true);
        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 11, parent_id: null, in_channel: true, all: false, id: "5e000007-0000-4000-8000-000000000000" }, true);
        expect(s.rows[0].mention_count).toBe(2);
    });

    test("a live mention, not replayed, still counts below the row's pointer when past the reader's place", () => {
        const { store: s } = store();
        s.setSidebar([row(two, { last_message_id: 10, read_cursor: 8, is_unread: true, mention_count: 1 })]);

        s.applyPersonalEvent("channel.mention", { channel_id: two, message_id: 9, parent_id: null, in_channel: true, all: false, id: "5e000008-0000-4000-8000-000000000000" });

        expect(s.rows[0].mention_count).toBe(2);
    });
});

describe("badges", () => {
    test("the store starts with no badges and holds the ones it is given", () => {
        const s = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        expect(s.badges).toEqual([]);

        const badge = { badge_key: "b0000001-0000-4000-8000-000000000000", name: "Serving", icon_css: "fa fa-hands-helping", bg_color: "#E8F5E9", fg_color: "#1B5E20", sort_order: 1 };
        s.setBadges([badge]);
        expect(s.badges).toEqual([badge]);
    });
});

// The person's own preferences show in the sidebar at once: a star moves a room into Starred,
// sorted first as the platform sorts it, and a notify mode mutes or unmutes its row. A room hidden
// from the conversation on screen stays in the list while it is open and leaves once another
// conversation opens, as the platform's sidebar leaves it out from then on; a reload while it is
// still open keeps it.
describe("the person's own preferences", () => {
    test("a star sorts the room first, and a mode is the row's at once", () => {
        const { store: s } = store();

        s.applyPrefs(two, { is_favorite: true, notify_mode: "silent" });

        expect(s.rows.map(r => r.channel_id)).toEqual([two, one]);
        expect(s.rows[0].is_favorite).toBe(true);
        expect(s.rows[0].notify_mode).toBe("silent");

        s.applyPrefs(two, { is_favorite: false });
        expect(s.rows.map(r => r.channel_id)).toEqual([one, two]);
    });

    test("a hidden room stays while it is open, through a reload, and leaves when another opens", () => {
        const { store: s } = store();
        s.setActive(two);

        s.applyPrefs(two, { hidden: true });
        expect(s.rows.map(r => r.channel_id)).toEqual([one, two]);

        // the platform's sidebar leaves a hidden room out
        s.setSidebar([row(one)]);
        expect(s.rows.map(r => r.channel_id)).toEqual([one, two]);

        s.setActive(one);
        expect(s.rows.map(r => r.channel_id)).toEqual([one]);
    });

    test("a room shown again before another opens stays", () => {
        const { store: s } = store();
        s.setActive(two);

        s.applyPrefs(two, { hidden: true });
        s.applyPrefs(two, { hidden: false });
        s.setActive(one);

        expect(s.rows.map(r => r.channel_id)).toEqual([one, two]);
    });

    // The platform's sidebar shows a hidden room again once a message newer than the hide lands.
    test("a new message in a room hidden while open brings it back, so it stays when another opens", () => {
        const { store: s } = store();
        s.setActive(two);
        s.applyPrefs(two, { hidden: true });

        s.applyPersonalEvent("channel.unread", { channel_id: two, message_id: 99 });
        s.setActive(one);

        expect(s.rows.map(r => r.channel_id)).toEqual([one, two]);
        expect(s.rows[1].hidden).toBe(false);
    });

    test("a direct message's new message does the same, and one walked back to an older message does not", () => {
        const dmOne = "d0000001-0000-4000-8000-000000000000";
        const dmTwo = "d0000002-0000-4000-8000-000000000000";
        const dmRow = (channelId: string): SidebarRow => row(channelId, { name: null, channel_type: "dm", dm_people: [{ person_alias_guid: "ben-okafor", nick_name: "Ben", last_name: "Okafor", avatar_url: null }], dm_other_count: 1, room_kind: "person", title: "Ben Okafor" });
        const preview = (channelId: string, messageId: number | null): Record<string, unknown> => ({
            channel_id: channelId,
            message_id: messageId,
            sent_at: "2026-10-21T10:00:00Z",
            preview: "hi",
            sender: { nick_name: "Ben", last_name: "Okafor" },
            is_own: false
        });
        const s = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        s.setSidebar([dmRow(dmOne), dmRow(dmTwo)]);

        s.setActive(dmTwo);
        s.applyPrefs(dmTwo, { hidden: true });
        s.applyPersonalEvent("dm.preview", preview(dmTwo, null));
        expect(s.rows.find(r => r.channel_id === dmTwo)?.hidden).toBe(true);

        s.applyPersonalEvent("dm.preview", preview(dmTwo, 50));
        s.setActive(dmOne);

        expect(s.rows.map(r => r.channel_id)).toContain(dmTwo);
    });

    test("a room the sidebar does not hold is left alone", () => {
        const { store: s } = store();

        s.applyPrefs(hidden, { is_favorite: true });

        expect(s.rows.map(r => r.channel_id)).toEqual([one, two]);
    });
});

// dm.preview names its sender as a person, so the row keeps the sender's photo beside their name.
describe("dm.preview's sender", () => {
    test("moves the row's sender photo with the name", () => {
        const s = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        s.setSidebar([row("c0000099-0000-4000-8000-000000000000", { name: null, channel_type: "dm", room_kind: "person", title: "Ben Okafor" })]);

        s.applyPersonalEvent("dm.preview", {
            channel_id: "c0000099-0000-4000-8000-000000000000", message_id: 11, preview: "hi", sent_at: "2026-10-19T11:00:00Z",
            sender: { person_alias_guid: "ben", nick_name: "Ben", last_name: "Okafor", avatar_url: "https://photos.example/ben.jpg" },
            is_own: false, posted_by_rock: false, preview_line: "hi", id: "5e000002-0000-4000-8000-000000000000"
        });

        expect(s.rows[0].last_sender_avatar_url).toBe("https://photos.example/ben.jpg");
    });
});
