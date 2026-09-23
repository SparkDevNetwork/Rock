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
        last_sender_listed: true,
        last_message_preview: "hi",
        read_cursor: 10,
        is_unread: false,
        mention_count: 0,
        dm_names: null,
        dm_other_count: null,
        ...extra
    };
}

function store(): { store: ReturnType<typeof createChannelStore>, reloads: () => number } {
    let reloads = 0;
    const s = createChannelStore({ reloadSidebar: () => reloads++ });
    s.setSidebar([row(one), row(two)]);
    return { store: s, reloads: () => reloads };
}

describe("createChannelStore", () => {
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

    test("a save that caught up clears the bold", () => {
        const { store: s } = store();
        s.applyPersonalEvent("channel.unread", { channel_id: two, message_id: 11 });

        s.applyMarkRead(two, { read_cursor: 11, last_message_id: 11 });

        expect(s.rows.find(r => r.channel_id === two)?.is_unread).toBe(false);
    });

    test("a fresh sidebar replaces the rows", () => {
        const { store: s } = store();

        s.setSidebar([row(hidden, { is_unread: true })]);

        expect(s.rows.map(r => r.channel_id)).toEqual([hidden]);
    });
});
