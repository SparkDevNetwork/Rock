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
// The frame's columns, on their own: the panel column holds one panel at a time, a thread opened
// from a list backs out to that list, the column keeps drawing what it held while it closes, and
// focus returns to what opened it. A full view such as Browse channels takes the conversation's
// place, the conversation left as it was underneath, and gives it back when it closes or a room
// is opened from it. On a phone the pane in front follows both the panel and the full view.
//
// The frame is mounted in a bare host whose template holds only the elements focus is aimed at,
// over a stand-in shell holding just what the frame calls.
import { mount, VueWrapper } from "@vue/test-utils";
import { computed, defineComponent, h, nextTick, reactive, ref, shallowRef } from "vue";
import { Frame, useFrameView } from "../../../../../src/Communication/Chat/ChatShell/composables/useFrameView.partial";
import { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";

const team = "c0000001-0000-4000-8000-000000000000";
const volunteers = "c0000002-0000-4000-8000-000000000000";
const pairWithAnna = "c0000003-0000-4000-8000-000000000000";

type Calls = string[];

/** The shell as the frame reaches it, recording each call it makes in order. */
function fakeShell(calls: Calls): { shell: ChatShell, state: { activeChannelId: string | null, hiddenPanes: { conversation: boolean, thread: boolean } } } {
    const state = reactive({ activeChannelId: null as string | null, hiddenPanes: { conversation: false, thread: false } });
    const threads = {
        state: reactive({ thread: null as { channelId: string, rootId: number } | null }),
        openList: jest.fn(async (_channelId: string, tab?: string) => {
            calls.push(tab === "all" ? "threads.openList:all" : "threads.openList");
        }),
        closeList: jest.fn(() => {
            calls.push("threads.closeList");
        }),
        openThread: jest.fn(async (channelId: string, rootId: number) => {
            calls.push(`threads.openThread:${rootId}`);
            threads.state.thread = { channelId, rootId };
        }),
        isOpen: (channelId: string, rootId: number) => threads.state.thread?.channelId === channelId && threads.state.thread.rootId === rootId,
        closeThread: jest.fn(async () => {
            threads.state.thread = null;
        })
    };
    const shell = {
        state,
        // Browse's preview reading its room's history, which no order here waits on
        readPreview: jest.fn(async () => undefined),
        selectChannel: jest.fn(async (channelId: string) => {
            calls.push(`selectChannel:${channelId}`);
            state.activeChannelId = channelId;
        }),
        threads,
        mentions: {
            state: { inbox: { rows: [] }, hasDot: false },
            openInbox: jest.fn(async () => {
                calls.push("mentions.openInbox");
            }),
            closeInbox: jest.fn(() => {
                calls.push("mentions.closeInbox");
            }),
            threadOpened: jest.fn()
        },
        rooms: {
            openDirectory: jest.fn(() => {
                calls.push("rooms.openDirectory");
            }),
            closeDirectory: jest.fn(() => {
                calls.push("rooms.closeDirectory");
            }),
            previewRoom: jest.fn((channelId: string) => {
                calls.push(`rooms.previewRoom:${channelId}`);
            })
        },
        // no card open, which a profile in the column would follow
        profile: {
            state: { card: null },
            closeCard: jest.fn()
        },
        // A channel and a conversation of two with Anna.
        channels: {
            rows: [
                { channel_id: team, channel_type: "shared", name: "Team", dm_people: null, dm_other_count: null, room_kind: "channel", title: "Team" },
                { channel_id: pairWithAnna, channel_type: "dm", name: null, dm_people: [{ person_alias_guid: "anna-reyes", nick_name: "Anna", last_name: "Reyes", avatar_url: null }], dm_other_count: 1, room_kind: "person", title: "Anna Reyes" }
            ],
            roomKindOf: (channelId: string) => channelId === team ? "channel" : channelId === pairWithAnna ? "person" : null
        },
        members: {
            open: jest.fn(async (channelId: string) => {
                calls.push(`members.open:${channelId}`);
            }),
            close: jest.fn(() => {
                calls.push("members.close");
            }),
            openAdd: jest.fn(() => {
                calls.push("members.openAdd");
            })
        },
        directMessages: {
            begin: jest.fn((people?: Array<{ nick_name: string }>) => {
                calls.push(`directMessages.begin:${(people ?? []).map(p => p.nick_name).join(",")}`);
            }),
            close: jest.fn(() => {
                calls.push("directMessages.close");
            })
        }
    };

    return { shell: shell as unknown as ChatShell, state };
}

type Mounted = {
    frame: Frame;
    shell: ChatShell;
    calls: Calls;
    state: ReturnType<typeof fakeShell>["state"];
    wrapper: VueWrapper;
    hasJoinBar: { value: boolean };
    hasPanelField: { value: boolean };
    hasPanelRow: { value: boolean };
};

/**
 * Mounts the frame in a host drawing the targets focus moves to: the sidebar's Browse row, the
 * header's buttons and title, the full view's first control, and the conversation's join bar or
 * composer.
 */
function mountFrame(): Mounted {
    const calls: Calls = [];
    const { shell, state } = fakeShell(calls);
    const hasJoinBar = ref(false);
    const hasPanelField = ref(false);
    const hasPanelRow = ref(false);
    let frame!: Frame;

    const Host = defineComponent({
        setup() {
            const rootElement = ref<HTMLElement>();
            frame = useFrameView({
                shell: shallowRef(shell),
                activeChannelId: computed(() => state.activeChannelId),
                rootElement,
                linkedReply: null,
                campuses: [],
                showNotice: () => undefined
            });

            // the frame notes the column focus comes into, as the block's frame does
            return () => h("div", { ref: rootElement, onFocusin: frame.onFocusIn }, [
                h("button", { "data-testid": "browse-channels" }, "Browse channels"),
                h("section", { ref: frame.mainElement, "data-testid": "frame-main" }, [
                    h("button", { "data-testid": "header-title", "data-focus-stop": "header" }, "Title"),
                    h("button", { "data-testid": "header-threads", "data-focus-stop": "threads-button" }, "Threads"),
                    hasJoinBar.value
                        ? h("button", { "data-testid": "join-channel", "data-focus-stop": "join" }, "Join channel")
                        : h("textarea", { "data-testid": "composer-input", "data-focus-stop": "composer" }),
                    frame.mainView.value
                        ? h("div", { ref: frame.mainViewElement, "data-testid": "main-view" }, [h("input", { "data-autofocus": "" })])
                        : null
                ]),
                h("aside", { ref: frame.panelElement }, [
                    hasPanelField.value ? h("input", { "data-testid": "panel-field", "data-autofocus": "" }) : null,
                    h("button", { "data-testid": "panel-close", "data-focus-stop": "panel-close" }, "Close"),
                    hasPanelRow.value ? h("button", { "data-testid": "panel-row" }, "A row") : null
                ])
            ]);
        }
    });

    const wrapper = mount(Host, { attachTo: document.body });
    return { frame, shell, calls, state, wrapper, hasJoinBar, hasPanelField, hasPanelRow };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 10; i++) {
        await Promise.resolve();
    }
}

/** Reports the block's own width, as its ResizeObserver does. */
function resize(frame: Frame, width: number): void {
    frame.onResize([{ contentRect: { width } } as unknown as ResizeObserverEntry]);
}

function focused(): string | null {
    return document.activeElement?.getAttribute("data-testid") ?? (document.activeElement?.hasAttribute("data-autofocus") ? "autofocus" : null);
}

let mounted: Mounted | null = null;

afterEach(() => {
    mounted?.wrapper.unmount();
    mounted = null;
    jest.useRealTimers();
});

describe("the panel column", () => {
    test("holds one panel at a time: a list opened over another lets the first go", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();

        await frame.onListPanel("threads");
        await frame.onListPanel("activity");

        expect(frame.panelKind.value).toBe("activity");
        expect(frame.activityTab.value).toBe("mentions");
        expect(calls).toEqual(["threads.openList", "threads.closeList", "mentions.openInbox"]);
    });

    test("a thread opened from the threads list backs out to the list", async () => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("threads");

        frame.openThreadFromList(7, team);
        await settle();
        expect(frame.panelKind.value).toBe("thread");
        expect(frame.threadOrigin.value).toBe("threads");

        await frame.onThreadBack();
        expect(frame.panelKind.value).toBe("threads");
    });

    // A thread pill pressed again, or a push for a reply of the thread on screen, opens what is open:
    // both reach the drawer through the landing, which opens it here.
    test("opening the thread it draws changes nothing, keeping the list it was opened from", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("threads");
        frame.openThreadFromList(7, team);
        await settle();
        const openCount = frame.threadOpenCount();
        calls.splice(0);

        void frame.openThreadDrawer(team, 7, null);
        void frame.openThreadDrawer(team, 7, null);
        await settle();

        expect(frame.panelKind.value).toBe("thread");
        expect(frame.threadOrigin.value).toBe("threads");
        expect(frame.threadOpenCount()).toBe(openCount);
        // The landing it carries is the landing's own to let go of and land again, so the open
        // thread is only waited on.
        expect(calls).toEqual(["threads.openThread:7", "threads.openThread:7"]);

        await frame.onThreadBack();
        expect(frame.panelKind.value).toBe("threads");
    });

    test("the column draws its panel through the close, and focus returns to the button that opened it", async () => {
        jest.useFakeTimers();
        mounted = mountFrame();
        const { frame, state, wrapper } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("threads");

        frame.closePanel();
        await wrapper.vm.$nextTick();

        expect(frame.panelKind.value).toBeNull();
        expect(frame.shownPanel.value).toBe("threads");
        expect(focused()).toBe("header-threads");

        jest.runAllTimers();
        expect(frame.shownPanel.value).toBeNull();
    });

    // Members, like Threads, is about the open conversation, so another conversation keeps it open
    // and lists its own people, focus staying where the person has it.
    test("the members list stays open and re-points at another conversation, focus kept inside it", async () => {
        mounted = mountFrame();
        const { frame, calls, state, wrapper } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("members");
        wrapper.find<HTMLElement>("[data-testid='panel-close']").element.focus();

        state.activeChannelId = volunteers;
        await settle();
        await wrapper.vm.$nextTick();

        expect(frame.panelKind.value).toBe("members");
        expect(calls).toEqual([`members.open:${team}`, `members.open:${volunteers}`]);
        expect(focused()).toBe("panel-close");
    });

    // The header offers Members only where a room has a roster to show, never in a conversation
    // of two, so the list does not follow the person there: it closes, as the header's own does.
    test("the members list closes rather than following into a conversation of two, which offers none", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("members");
        expect(frame.isMembersOffered.value).toBe(true);

        state.activeChannelId = pairWithAnna;
        await settle();

        expect(frame.isMembersOffered.value).toBe(false);
        expect(frame.panelKind.value).toBeNull();
        expect(calls).toEqual([`members.open:${team}`, "members.close"]);

        await frame.onListPanel("members");
        expect(frame.panelKind.value).toBeNull();
    });

    // The roster read afresh for another conversation takes its old rows away, so focus on one
    // goes where the panel starts, its filter, rather than with the row off the page.
    test("a list re-pointed at another conversation keeps focus in the panel when the row that had it goes", async () => {
        mounted = mountFrame();
        const { frame, shell, state, wrapper, hasPanelField, hasPanelRow } = mounted;
        state.activeChannelId = team;
        hasPanelField.value = true;
        hasPanelRow.value = true;
        await settle();
        await frame.onListPanel("members");
        (shell.members.open as jest.Mock).mockImplementation(async () => {
            hasPanelRow.value = false;
        });
        wrapper.get<HTMLElement>("[data-testid='panel-row']").element.focus();

        state.activeChannelId = volunteers;
        await settle();
        await wrapper.vm.$nextTick();

        expect(frame.panelKind.value).toBe("members");
        expect(focused()).toBe("panel-field");
    });

    test("the threads list follows the open conversation", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("threads");

        state.activeChannelId = volunteers;
        await settle();

        expect(frame.panelKind.value).toBe("threads");
        expect(calls).toEqual(["threads.openList", "threads.openList"]);
    });
});

// Activity is the one inbox across conversations, opened from the sidebar's head: Mentions and the
// threads the reader follows under one Segmented. It opens on Mentions while a mention waits,
// else on the tab last chosen; it stays as it is when the conversation changes.
describe("Activity", () => {
    test("opens on Mentions while a mention waits, else on the tab last chosen", async () => {
        mounted = mountFrame();
        const { frame, shell, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();

        await frame.onListPanel("activity");
        expect(frame.activityTab.value).toBe("mentions");
        frame.onActivityTab("threads");
        await settle();
        expect(calls).toEqual(["mentions.openInbox", "mentions.closeInbox", "threads.openList:all"]);

        frame.closePanel();
        await frame.onListPanel("activity");
        expect(frame.activityTab.value).toBe("threads");

        frame.closePanel();
        (shell.mentions.state as { hasDot: boolean }).hasDot = true;
        await frame.onListPanel("activity");
        expect(frame.activityTab.value).toBe("mentions");
    });

    test("a thread opened from its Threads backs out to Activity on Threads", async () => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("activity");
        frame.onActivityTab("threads");
        await settle();

        frame.openThreadFromList(7, team);
        await settle();
        expect(frame.threadOrigin.value).toBe("activity");

        await frame.onThreadBack();
        expect(frame.panelKind.value).toBe("activity");
        expect(frame.activityTab.value).toBe("threads");
    });

    test("a conversation's empty threads list asking for every followed thread opens Activity on Threads", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("threads");

        frame.onThreadsTab("all");
        await settle();

        expect(frame.panelKind.value).toBe("activity");
        expect(frame.activityTab.value).toBe("threads");
        expect(calls).toEqual(["threads.openList", "threads.closeList", "threads.openList:all"]);
    });

    test("stays as it is when the conversation changes", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("activity");

        state.activeChannelId = volunteers;
        await settle();

        expect(frame.panelKind.value).toBe("activity");
        expect(calls).toEqual(["mentions.openInbox"]);
    });
});

// In a window too narrow for the sidebar, the conversation and a panel side by side (narrower
// than the sidebar, the panel and the conversation's least width, as the sheet names them), an
// open panel folds the sidebar away; closing the panel brings it back. While folded, the header's
// sidebar button opens it over the conversation, and choosing a conversation folds it again.
describe("the narrow fold", () => {
    test("folds the sidebar while a panel is open below the sidebar, panel and conversation's least width, and restores it on close", async () => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        state.activeChannelId = team;
        await settle();
        resize(frame, 1100);

        expect(frame.sidebarFold.value).toBeNull();
        await frame.onListPanel("members");
        expect(frame.sidebarFold.value).toBe("folded");

        frame.closePanel();
        expect(frame.sidebarFold.value).toBeNull();
    });

    test("does not fold where all three fit, nor on a phone", async () => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("members");

        resize(frame, 1168);
        expect(frame.sidebarFold.value).toBeNull();
        resize(frame, 1167);
        expect(frame.sidebarFold.value).toBe("folded");
        resize(frame, 600);
        expect(frame.sidebarFold.value).toBeNull();
    });

    test("the header's sidebar button opens it over the conversation, and choosing a conversation folds it again", async () => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        state.activeChannelId = team;
        await settle();
        resize(frame, 1100);
        await frame.onListPanel("threads");

        frame.onSidebarOver();
        expect(frame.sidebarFold.value).toBe("over");

        await frame.onSelect(volunteers);
        expect(frame.sidebarFold.value).toBe("folded");
        expect(frame.isSidebarHidden.value).toBe(true);
    });

    // The row chosen folds away with the sidebar and out of reach, so focus goes to the
    // conversation it opened, at its title, as a phone's row hands it over.
    test("a conversation chosen from the sidebar over the conversation gives focus to its title as the sidebar folds", async () => {
        mounted = mountFrame();
        const { frame, state, wrapper } = mounted;
        state.activeChannelId = team;
        await settle();
        resize(frame, 1100);
        await frame.onListPanel("threads");
        frame.onSidebarOver();
        await settle();
        (wrapper.get("[data-testid='browse-channels']").element as HTMLElement).focus();

        await frame.onSelect(volunteers);
        await settle();

        expect(frame.sidebarFold.value).toBe("folded");
        expect(focused()).toBe("header-title");
    });
});

describe("a full view", () => {
    test("takes the main column, the conversation left open underneath and out of view, and gives it back on close", async () => {
        mounted = mountFrame();
        const { frame, calls, state, wrapper } = mounted;
        state.activeChannelId = team;
        await settle();

        (wrapper.find("[data-testid='browse-channels']").element as HTMLElement).focus();
        await frame.openMainView("directory");

        expect(frame.mainView.value).toBe("directory");
        expect(calls).toEqual(["rooms.openDirectory"]);
        expect(state.activeChannelId).toBe(team);
        expect(state.hiddenPanes.conversation).toBe(true);
        expect(focused()).toBe("autofocus");

        frame.closeMainView();
        await settle();

        expect(frame.mainView.value).toBeNull();
        expect(calls).toEqual(["rooms.openDirectory", "rooms.closeDirectory"]);
        expect(state.hiddenPanes.conversation).toBe(false);
        expect(focused()).toBe("composer-input");
    });

    test("closed with no conversation under it gives focus back to what opened it", async () => {
        mounted = mountFrame();
        const { frame, wrapper } = mounted;
        (wrapper.find("[data-testid='browse-channels']").element as HTMLElement).focus();
        await frame.openMainView("directory");

        // the host draws a composer whether or not a conversation is open, so take it away
        wrapper.find("[data-testid='composer-input']").element.remove();
        frame.closeMainView();
        await settle();

        expect(focused()).toBe("browse-channels");
    });

    test("closes the open panel or thread as it opens", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("threads");

        await frame.openMainView("directory");

        expect(frame.panelKind.value).toBeNull();
        expect(calls).toEqual(["threads.openList", "threads.closeList", "rooms.openDirectory"]);
    });

    test("opening a room from it previews that room before it opens, closes the view and lands on Join", async () => {
        mounted = mountFrame();
        const { frame, calls, state, hasJoinBar } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.openMainView("directory");
        hasJoinBar.value = true;

        await frame.openFromMainView(volunteers);
        await settle();

        expect(calls).toEqual(["rooms.openDirectory", `rooms.previewRoom:${volunteers}`, "rooms.closeDirectory", `selectChannel:${volunteers}`]);
        expect(frame.mainView.value).toBeNull();
        expect(state.activeChannelId).toBe(volunteers);
        expect(focused()).toBe("join-channel");
    });

    // The preview's end keeps the room on screen, so Open puts its room there before letting the
    // preview go, and the page the person just read is kept for the open to draw.
    test("opening a room from it puts the room on screen before the preview is let go", async () => {
        mounted = mountFrame();
        const { frame, shell, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.openMainView("directory");
        frame.onPreview(volunteers);
        const select = shell.selectChannel as unknown as jest.Mock;
        const read = shell.readPreview as unknown as jest.Mock;
        select.mockImplementation(() => {
            state.activeChannelId = volunteers;
            return new Promise<void>(() => undefined);
        });
        let shownAtLetGo: string | null = null;
        read.mockImplementation(async (channelId: string | null) => {
            if (channelId === null) {
                shownAtLetGo = state.activeChannelId;
            }
        });

        void frame.openFromMainView(volunteers);
        await settle();

        expect(read).toHaveBeenLastCalledWith(null);
        expect(shownAtLetGo).toBe(volunteers);
    });

    test("opening the conversation already underneath from it still closes the view", async () => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.openMainView("directory");

        await frame.openFromMainView(team);

        expect(frame.mainView.value).toBeNull();
    });

    test("a sidebar row chosen closes it, the conversation underneath included", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.openMainView("directory");

        await frame.onSelect(team);

        expect(frame.mainView.value).toBeNull();
        expect(calls).toContain("rooms.closeDirectory");
    });

    test("another conversation opening by any other way closes it", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.openMainView("directory");

        state.activeChannelId = volunteers;
        await settle();

        expect(frame.mainView.value).toBeNull();
        expect(calls).toEqual(["rooms.openDirectory", "rooms.closeDirectory"]);
    });
});

// New message is the second full view: it takes the conversation's place as Browse channels does,
// begins afresh or with the person a profile card's Message chose, and lets go of the direct
// messages' New message whenever it closes, by its X or by any conversation opening.
describe("New message as a full view", () => {
    const anna = { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Reyes", avatar_url: null };

    test("takes the main column, beginning afresh, the conversation left open underneath and out of view", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();

        await frame.openMainView("newdm");

        expect(frame.mainView.value).toBe("newdm");
        expect(calls).toEqual(["directMessages.begin:"]);
        expect(state.activeChannelId).toBe(team);
        expect(state.hiddenPanes.conversation).toBe(true);
        expect(focused()).toBe("autofocus");
    });

    test("begins with the person chosen when a profile card's Message opens it", async () => {
        mounted = mountFrame();
        const { frame, calls } = mounted;

        await frame.openMainView("newdm", [anna]);

        expect(frame.mainView.value).toBe("newdm");
        expect(calls).toEqual(["directMessages.begin:Anna"]);
    });

    test("closing it lets go of New message and gives the conversation back", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.openMainView("newdm");

        frame.closeMainView();
        await settle();

        expect(frame.mainView.value).toBeNull();
        expect(calls).toEqual(["directMessages.begin:", "directMessages.close"]);
        expect(focused()).toBe("composer-input");
    });

    test("takes Browse channels' place, which closes first", async () => {
        mounted = mountFrame();
        const { frame, calls } = mounted;
        await frame.openMainView("directory");

        await frame.openMainView("newdm");

        expect(frame.mainView.value).toBe("newdm");
        expect(calls).toEqual(["rooms.openDirectory", "rooms.closeDirectory", "directMessages.begin:"]);
    });

    test("Browse channels taking its place lets go of New message", async () => {
        mounted = mountFrame();
        const { frame, calls } = mounted;
        await frame.openMainView("newdm");

        await frame.openMainView("directory");

        expect(calls).toEqual(["directMessages.begin:", "directMessages.close", "rooms.openDirectory"]);
    });

    test("a conversation opening, as the first message's does, closes it", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        await frame.openMainView("newdm");

        state.activeChannelId = volunteers;
        await settle();

        expect(frame.mainView.value).toBeNull();
        expect(calls).toEqual(["directMessages.begin:", "directMessages.close"]);
    });

    test("on a phone, is the conversation pane with focus in its To field, and Back gives focus to what opened it", async () => {
        mounted = mountFrame();
        const { frame, wrapper, calls } = mounted;
        resize(frame, 390);
        (wrapper.find("[data-testid='browse-channels']").element as HTMLElement).focus();

        await frame.openMainView("newdm");
        expect(calls).toEqual(["directMessages.begin:"]);
        expect(frame.pane.value).toBe("conversation");
        expect(focused()).toBe("autofocus");

        await frame.onBack();
        expect(frame.pane.value).toBe("list");
        expect(focused()).toBe("browse-channels");
    });

    test("on a phone, Browse channels takes no first focus, which would raise the keyboard over its list", async () => {
        mounted = mountFrame();
        const { frame, wrapper } = mounted;
        resize(frame, 390);
        (wrapper.find("[data-testid='browse-channels']").element as HTMLElement).focus();

        await frame.openMainView("directory");

        expect(frame.pane.value).toBe("conversation");
        expect(focused()).not.toBe("autofocus");
    });
});

// Add people, wherever it is asked from: a channel or a group opens its dialog. Rock keeps a
// conversation of two as it is, so it has no Add people and nothing opens there.
describe("Add people", () => {
    test("in a channel or a group opens the dialog", async () => {
        mounted = mountFrame();
        const { frame, calls } = mounted;

        await frame.openAddPeople(team);

        expect(calls).toEqual(["members.openAdd"]);
        expect(frame.mainView.value).toBeNull();
    });

    test("in a conversation of two opens nothing", async () => {
        mounted = mountFrame();
        const { frame, calls } = mounted;

        await frame.openAddPeople(pairWithAnna);

        expect(calls).toEqual([]);
        expect(frame.mainView.value).toBeNull();
    });
});

describe("on a phone", () => {
    test("the pane in front follows both the full view and the panel", async () => {
        mounted = mountFrame();
        const { frame, calls, state } = mounted;
        resize(frame, 390);
        await settle();
        expect(frame.pane.value).toBe("list");

        // Browse channels is the conversation pane, with nothing open under it
        await frame.openMainView("directory");
        expect(frame.pane.value).toBe("conversation");
        expect(frame.isSidebarHidden.value).toBe(true);

        // Back shows the list, the view kept behind it, and Browse brings the same view forward
        await frame.onBack();
        expect(frame.pane.value).toBe("list");
        expect(frame.mainView.value).toBe("directory");
        await frame.openMainView("directory");
        expect(frame.pane.value).toBe("conversation");
        expect(calls.filter(c => c === "rooms.openDirectory")).toHaveLength(1);

        // a room opened from it is the conversation pane
        await frame.openFromMainView(volunteers);
        await settle();
        expect(frame.pane.value).toBe("conversation");
        expect(state.activeChannelId).toBe(volunteers);

        // a panel comes over it, and the view opening again takes the panel's place
        await frame.onListPanel("threads");
        expect(frame.pane.value).toBe("panel");
        await frame.openMainView("directory");
        expect(frame.pane.value).toBe("conversation");
        expect(frame.panelKind.value).toBeNull();
    });

    test("Back from the full view gives focus to the Browse row it was opened from", async () => {
        mounted = mountFrame();
        const { frame, wrapper } = mounted;
        resize(frame, 390);
        (wrapper.find("[data-testid='browse-channels']").element as HTMLElement).focus();
        await frame.openMainView("directory");

        await frame.onBack();

        expect(focused()).toBe("browse-channels");
    });

    test("no field is focused on its own as the view opens, which would raise the keyboard", async () => {
        mounted = mountFrame();
        const { frame } = mounted;
        resize(frame, 390);

        await frame.openMainView("directory");

        expect(focused()).not.toBe("autofocus");
    });
});

// A panel or pane is still sliding in when focus is put inside it. A focus that scrolls would
// scroll the clipping box around it, the panel column growing from 0px or the frame with a phone's
// pane waiting off to its right, which drags the content in from the edge instead of letting it
// slide. jsdom does not scroll, so these cells pin the focus call that keeps the box still.
describe("focus into what is opening", () => {
    /** Each focus the frame makes from here on, by the element's test id and the options it passed. */
    function recordFocus(): Array<{ target: string | null, options: FocusOptions | undefined }> {
        const focuses: Array<{ target: string | null, options: FocusOptions | undefined }> = [];
        const focus = HTMLElement.prototype.focus;
        jest.spyOn(HTMLElement.prototype, "focus").mockImplementation(function (this: HTMLElement, options?: FocusOptions) {
            focuses.push({ target: this.getAttribute("data-testid") ?? (this.hasAttribute("data-autofocus") ? "autofocus" : null), options });
            focus.call(this, options);
        });

        return focuses;
    }

    afterEach(() => {
        jest.restoreAllMocks();
    });

    test.each(["threads", "mentions", "members"] as const)("the %s list's first control takes focus without scrolling the opening column", async kind => {
        mounted = mountFrame();
        const { frame, state, hasPanelField } = mounted;
        state.activeChannelId = team;
        hasPanelField.value = true;
        await settle();
        const focuses = recordFocus();

        await frame.onListPanel(kind);

        expect(focuses).toEqual([{ target: "panel-field", options: { preventScroll: true } }]);
    });

    test.each(["threads", "mentions", "members"] as const)("the %s list with no control yet puts focus on its X without scrolling the opening column", async kind => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        state.activeChannelId = team;
        await settle();
        const focuses = recordFocus();

        await frame.onListPanel(kind);

        expect(focuses).toEqual([{ target: "panel-close", options: { preventScroll: true } }]);
        expect(focused()).toBe("panel-close");
    });

    test("on a phone, a list opened as the panel pane takes focus without scrolling the frame", async () => {
        mounted = mountFrame();
        const { frame, state } = mounted;
        resize(frame, 390);
        state.activeChannelId = team;
        await settle();
        const focuses = recordFocus();

        await frame.onListPanel("threads");

        expect(frame.pane.value).toBe("panel");
        expect(focuses).toEqual([{ target: "panel-close", options: { preventScroll: true } }]);
    });

    test("on a phone, a row's conversation sliding in takes focus at its title without scrolling the frame", async () => {
        mounted = mountFrame();
        const { frame } = mounted;
        resize(frame, 390);
        await settle();
        const focuses = recordFocus();

        await frame.onSelect(team);

        expect(focuses).toEqual([{ target: "header-title", options: { preventScroll: true } }]);
    });

    test.each([[false, "directory"], [false, "newdm"], [true, "newdm"]] as const)("a full view opening (phone %s, %s) takes focus without scrolling what holds it", async (isPhone, kind) => {
        mounted = mountFrame();
        const { frame } = mounted;
        resize(frame, isPhone ? 390 : 1200);
        await settle();
        const focuses = recordFocus();

        await frame.openMainView(kind);

        expect(focuses).toEqual([{ target: "autofocus", options: { preventScroll: true } }]);
    });
});

// One helper puts focus on the first stop of an order that is drawn, each found by its own mark,
// so no lookup reads a test id. A phone skips the composer, whose focus would raise the keyboard.
// The host here draws its stops with their marks alone, as nothing in production is found by a test id.
describe("the one focus helper", () => {
    type Drawn = { join: boolean, composer: boolean, threadsButton: boolean, more: boolean, message: boolean, summary: boolean, isSidebarInert: boolean };

    function mountMarked(): { frame: Frame, state: ReturnType<typeof fakeShell>["state"], wrapper: VueWrapper, drawn: Drawn } {
        const { shell, state } = fakeShell([]);
        const drawn = reactive<Drawn>({ join: false, composer: true, threadsButton: true, more: false, message: true, summary: true, isSidebarInert: false });
        let frame!: Frame;

        const Host = defineComponent({
            setup() {
                const rootElement = ref<HTMLElement>();
                frame = useFrameView({
                    shell: shallowRef(shell),
                    activeChannelId: computed(() => state.activeChannelId),
                    rootElement,
                    linkedReply: null,
                    campuses: [],
                    showNotice: () => undefined
                });

                // the frame notes the column focus comes into, as the block's frame does
                return () => h("div", { ref: rootElement, onFocusin: frame.onFocusIn }, [
                    // the sidebar's head, out of reach as a phone's list behind the conversation is
                    h("div", { inert: drawn.isSidebarInert || undefined }, [
                        h("button", { "data-focus-stop": "activity-button" }, "Activity"),
                        h("button", { "data-focus-stop": "new-message" }, "New message")
                    ]),
                    h("section", { ref: frame.mainElement }, [
                        h("b", { tabindex: -1, "data-focus-stop": "header" }, "Title"),
                        drawn.threadsButton ? h("button", { "data-focus-stop": "threads-button" }, "Threads") : null,
                        drawn.more ? h("span", { "data-focus-stop": "more" }, [h("button", "More")]) : null,
                        // A message lives in the feed, which what the drawer's close gives focus to must be in view of.
                        h("div", { "data-chat-scroller": "" }, [
                            drawn.message
                                ? h("div", { "data-message-id": "40", tabindex: -1 }, [drawn.summary ? h("button", { "data-focus-stop": "thread-summary" }, "3 replies") : null])
                                : null
                        ]),
                        drawn.join ? h("button", { "data-focus-stop": "join" }, "Join channel") : null,
                        drawn.composer ? h("div", { contenteditable: "true", tabindex: 0, "data-focus-stop": "composer" }) : null,
                        frame.mainView.value ? h("div", { ref: frame.mainViewElement }, [h("input", { "data-autofocus": "" })]) : null
                    ]),
                    h("aside", { ref: frame.panelElement }, [h("button", { "data-focus-stop": "panel-close" }, "Close")])
                ]);
            }
        });

        const wrapper = mount(Host, { attachTo: document.body });
        mounted = { frame, calls: [], state, wrapper, hasJoinBar: { value: false }, hasPanelField: { value: false }, hasPanelRow: { value: false } };
        return { frame, state, wrapper, drawn };
    }

    /** The stop focus is on, by its mark, or the header or the root message it fell back to. */
    function focusedStop(): string | null {
        const active = document.activeElement;
        if (!active || active === document.body) {
            return null;
        }

        return active.closest("[data-focus-stop]")?.getAttribute("data-focus-stop")
            ?? (active.hasAttribute("data-message-id") ? "message" : null);
    }

    test("focuses Join when drawn, the composer on desktop when not, the header otherwise, and on a phone skips the composer", async () => {
        const { frame, drawn } = mountMarked();
        drawn.join = true;
        await nextTick();

        frame.focusStop(["join", "composer", "header"]);
        expect(focusedStop()).toBe("join");

        drawn.join = false;
        await nextTick();
        frame.focusStop(["join", "composer", "header"]);
        expect(focusedStop()).toBe("composer");

        resize(frame, 390);
        frame.focusStop(["join", "composer", "header"]);
        expect(focusedStop()).toBe("header");

        resize(frame, 1200);
        drawn.composer = false;
        await nextTick();
        frame.focusStop(["join", "composer", "header"]);
        expect(focusedStop()).toBe("header");
    });

    test("a room opened from a full view lands on Join, else the composer on desktop, else the title", async () => {
        const { frame, state, drawn } = mountMarked();
        state.activeChannelId = team;
        await settle();

        drawn.join = true;
        await frame.openMainView("directory");
        await frame.openFromMainView(volunteers);
        await settle();
        expect(focusedStop()).toBe("join");

        drawn.join = false;
        await frame.openMainView("directory");
        await frame.openFromMainView(team);
        await settle();
        expect(focusedStop()).toBe("composer");

        resize(frame, 390);
        await frame.openMainView("directory");
        await frame.openFromMainView(volunteers);
        await settle();
        expect(focusedStop()).toBe("header");
    });

    test("a full view closing gives focus to the composer, else what opened it, else the title", async () => {
        const { frame, state, wrapper, drawn } = mountMarked();
        state.activeChannelId = team;
        await settle();
        const newMessage = wrapper.get("[data-focus-stop='new-message']").element as HTMLElement;

        newMessage.focus();
        await frame.openMainView("newdm");
        frame.closeMainView();
        await settle();
        expect(focusedStop()).toBe("composer");

        drawn.composer = false;
        newMessage.focus();
        await frame.openMainView("newdm");
        frame.closeMainView();
        await settle();
        expect(focusedStop()).toBe("new-message");

        newMessage.focus();
        await frame.openMainView("newdm");
        newMessage.remove();
        frame.closeMainView();
        await settle();
        expect(focusedStop()).toBe("header");
    });

    test("a panel closing gives focus to the header button that opened it, else More, else the title", async () => {
        const { frame, state, drawn } = mountMarked();
        state.activeChannelId = team;
        await settle();

        await frame.onListPanel("threads");
        frame.closePanel();
        await settle();
        expect(focusedStop()).toBe("threads-button");

        drawn.threadsButton = false;
        drawn.more = true;
        await frame.onListPanel("threads");
        frame.closePanel();
        await settle();
        expect(focusedStop()).toBe("more");

        drawn.more = false;
        await frame.onListPanel("threads");
        frame.closePanel();
        await settle();
        expect(focusedStop()).toBe("header");
    });

    // A stop in a pane out of reach, as the list behind a phone's conversation, cannot take focus,
    // so the order goes on past it rather than ending on a focus that silently fails.
    test("passes over a stop in a pane out of reach and goes on down its order", async () => {
        const { frame, state, drawn } = mountMarked();
        state.activeChannelId = team;
        drawn.more = true;
        await settle();

        frame.focusStop(["activity-button", "more", "header"]);
        expect(focusedStop()).toBe("activity-button");

        drawn.isSidebarInert = true;
        await nextTick();
        frame.focusStop(["activity-button", "more", "header"]);
        expect(focusedStop()).toBe("more");

        drawn.more = false;
        await nextTick();
        frame.focusStop(["activity-button", "more", "header"]);
        expect(focusedStop()).toBe("header");
    });

    test("a drawer closing gives focus to the root's thread pill, else the root message, else the title", async () => {
        const { frame, state, drawn } = mountMarked();
        state.activeChannelId = team;
        await settle();

        await frame.openThreadDrawer(team, 40, null);
        frame.closeThreadDrawer();
        await settle();
        expect(focusedStop()).toBe("thread-summary");

        drawn.summary = false;
        await frame.openThreadDrawer(team, 40, null);
        frame.closeThreadDrawer();
        await settle();
        expect(focusedStop()).toBe("message");

        drawn.message = false;
        await frame.openThreadDrawer(team, 40, null);
        frame.closeThreadDrawer();
        await settle();
        expect(focusedStop()).toBe("header");
    });
});
