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
import { computed, defineComponent, h, reactive, ref, shallowRef } from "vue";
import { Frame, useFrame } from "../../../../../src/Communication/Chat/ChatShell/composables/useFrame.partial";
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
        openList: jest.fn(async () => {
            calls.push("threads.openList");
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
        selectChannel: jest.fn(async (channelId: string) => {
            calls.push(`selectChannel:${channelId}`);
            state.activeChannelId = channelId;
        }),
        threads,
        mentions: {
            state: { inbox: { rows: [] } },
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
        // A channel and a conversation of two with Anna.
        channels: {
            rows: [
                { channel_id: team, channel_type: "shared", name: "Team", dm_people: null, dm_other_count: null, room_kind: "channel", title: "Team" },
                { channel_id: pairWithAnna, channel_type: "dm", name: null, dm_people: [{ person_alias_guid: "anna-reyes", nick_name: "Anna", last_name: "Reyes", avatar_url: null }], dm_other_count: 1, room_kind: "person", title: "Anna Reyes" }
            ]
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
    calls: Calls;
    state: ReturnType<typeof fakeShell>["state"];
    wrapper: VueWrapper;
    hasJoinBar: { value: boolean };
    hasPanelField: { value: boolean };
};

/**
 * Mounts the frame in a host drawing the targets focus moves to: the sidebar's Browse row, the
 * header's buttons and title, the full view's first control, and the conversation's join bar or
 * composer.
 */
function mountFrame(): Mounted {
    const calls: Calls = [];
    const forgetReplyLanding = (): void => void calls.push("forgetReplyLanding");
    const { shell, state } = fakeShell(calls);
    const hasJoinBar = ref(false);
    const hasPanelField = ref(false);
    let frame!: Frame;

    const Host = defineComponent({
        setup() {
            const rootElement = ref<HTMLElement>();
            frame = useFrame({
                shell: shallowRef(shell),
                activeChannelId: computed(() => state.activeChannelId),
                rootElement,
                linkedReply: null,
                landOnReply: async () => undefined,
                forgetReplyLanding
            });

            return () => h("div", { ref: rootElement }, [
                h("button", { "data-testid": "browse-channels" }, "Browse channels"),
                h("section", { "data-testid": "frame-main" }, [
                    h("button", { "data-testid": "header-title", "data-header-focus": "" }, "Title"),
                    h("button", { "data-testid": "header-threads" }, "Threads"),
                    hasJoinBar.value
                        ? h("button", { "data-testid": "join-channel" }, "Join channel")
                        : h("textarea", { "data-testid": "composer-input" }),
                    frame.mainView.value
                        ? h("div", { "data-testid": "main-view" }, [h("input", { "data-autofocus": "" })])
                        : null
                ]),
                h("aside", { ref: frame.panelElement }, [
                    hasPanelField.value ? h("input", { "data-testid": "panel-field", "data-autofocus": "" }) : null,
                    h("button", { "data-testid": "panel-close" }, "Close")
                ])
            ]);
        }
    });

    const wrapper = mount(Host, { attachTo: document.body });
    return { frame, calls, state, wrapper, hasJoinBar, hasPanelField };
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
        await frame.onListPanel("mentions");

        expect(frame.panelKind.value).toBe("mentions");
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

    test("the members list belongs to its conversation, so opening another closes it, focus inside it going to the title", async () => {
        mounted = mountFrame();
        const { frame, calls, state, wrapper } = mounted;
        state.activeChannelId = team;
        await settle();
        await frame.onListPanel("members");
        wrapper.find<HTMLElement>("[data-testid='panel-close']").element.focus();

        state.activeChannelId = volunteers;
        await settle();
        await wrapper.vm.$nextTick();

        expect(frame.panelKind.value).toBeNull();
        expect(calls).toEqual([`members.open:${team}`, "members.close"]);
        expect(focused()).toBe("header-title");
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
