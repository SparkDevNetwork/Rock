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
            threads.state.thread = { channelId, rootId };
        }),
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
    let frame!: Frame;

    const Host = defineComponent({
        setup() {
            const rootElement = ref<HTMLElement>();
            frame = useFrame({
                shell: shallowRef(shell),
                activeChannelId: computed(() => state.activeChannelId),
                draft: ref(null),
                rootElement,
                linkedReply: null,
                landOnReply: async () => undefined,
                forgetReplyLanding: () => undefined
            });

            return () => h("div", { ref: rootElement }, [
                h("button", { "data-testid": "browse-channels" }, "Browse channels"),
                h("section", { "data-testid": "frame-main" }, [
                    h("button", { "data-testid": "header-title" }, "Title"),
                    h("button", { "data-testid": "header-threads" }, "Threads"),
                    hasJoinBar.value
                        ? h("button", { "data-testid": "join-channel" }, "Join channel")
                        : h("textarea", { "data-testid": "composer-input" }),
                    frame.mainView.value
                        ? h("div", { "data-testid": "main-view" }, [h("input", { "data-autofocus": "" })])
                        : null
                ]),
                h("aside", { ref: frame.panelElement }, [h("button", { "data-testid": "panel-close" }, "Close")])
            ]);
        }
    });

    const wrapper = mount(Host, { attachTo: document.body });
    return { frame, calls, state, wrapper, hasJoinBar };
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

    test("on a phone, is the conversation pane, and Back gives focus to what opened it", async () => {
        mounted = mountFrame();
        const { frame, wrapper, calls } = mounted;
        resize(frame, 390);
        (wrapper.find("[data-testid='browse-channels']").element as HTMLElement).focus();

        await frame.openMainView("newdm");
        expect(calls).toEqual(["directMessages.begin:"]);
        expect(frame.pane.value).toBe("conversation");
        expect(focused()).not.toBe("autofocus");

        await frame.onBack();
        expect(frame.pane.value).toBe("list");
        expect(focused()).toBe("browse-channels");
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
