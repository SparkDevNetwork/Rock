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
// The thread drawer, in the panel host: the root, a divider that counts the replies or invites the
// first, the replies in the conversation's look, a Follow toggle in the header and the reply
// composer, whose foot offers to send the reply to the conversation too. Escape closes it, or goes
// back to the threads list it was opened from. A phone never focuses the composer on its own,
// since the keyboard would cover the thread.
//
// The component these tests are written to, threadDrawer.partial.obs:
//
//   props: root: TimelineMessage | null, replies: TimelineMessage[] (oldest first),
//          pending: PendingMessage[] (default []), isFollowing: boolean, conversationTitle: string,
//          isChannel: boolean, isDirectMessage: boolean, isPhone: boolean, isFromList: boolean,
//          personAliasGuid: string (required)
//          mentionSource: MentionPickerSource | null (the reply composer's @ picker; null offers none)
//          draft: ComposerDraft (the reply its owner keeps for the thread)
//   emits: close, back, follow(isFollowing: boolean), send(body: string, isShownInChannel: boolean),
//          seen(messageId: number), loadOlder, retry(localIds: string[]) (oldest first, sent in turn),
//          discard(localId: string),
//          mentionQuery(query: string | null) (the reply composer's @ query, for the search),
//          composerFocus (focus came into the reply composer),
//          draftText(text: string), draftMention(piece) (the reply composer's changes, for its owner)
//   marks: thread-divider, thread-follow (aria-pressed), and the composer's composer-input and
//          composer-also (the foot's label around its switch).
//
// It sits in the shared panel host, ui/panelHost.partial.obs:
//
//   props: title: string (required), subtitle: string | null, isPhone: boolean,
//          backLabel: string | null (a back chevron with this label), closeLabel: string
//   slots: actions (header buttons), top (above the scroller), default (the body)
//   emits: close, back; Escape inside it, and the back chevron, emit back when there is a back
//          label, else close
//   marks: panel-host (labelled by its title), panel-title, panel-subtitle, panel-close (the X,
//          desktop only), panel-back (the chevron: always on a phone, labelled "Back" there unless
//          a back label says otherwise)
//
// The toggle changes at once; a refused change comes back as followError, shown under the header
// with Try again where another try can pass. Only an unfollow is toasted, from useThreads' unfollowNotice, once it lands.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ThreadDrawer from "../../../../../src/Communication/Chat/ChatShell/components/threadDrawer.partial.obs";
import Composer from "../../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import { unfollowNotice } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { createMessageMotion, messageMotionKey } from "../../../../../src/Communication/Chat/ChatShell/messageMotion.partial";
import type { Typing } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import TypingBubble from "../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs";
import { playAnimations } from "../tailStage";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

/** The place the stores record a send or an arrival in; the drawer here is woken by what it draws. */
const place = "c1/1";
const ben = "a0000003-0000-4000-8000-000000000000";

/** The channel's name as the header shows it: plain, the hash being an icon, never a character. */
const channelTitle = "young-adults";

function message(id: number, extra: Record<string, unknown> = {}): TimelineMessage {
    return {
        id,
        parent_id: null,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: `message ${id}`,
        pieces: null,
        blocks: null,
        created_at: new Date(2026, 9, 13, 9, id % 60).toISOString(),
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        ...extra
    } as unknown as TimelineMessage;
}

function rootWith(replyCount: number): TimelineMessage {
    return message(40, {
        body: "Who's in for Saturday's hike?",
        reply_count: replyCount,
        last_reply_at: replyCount > 0 ? new Date(2026, 9, 13, 9, 41).toISOString() : null,
        repliers: replyCount > 0 ? [{ person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor", avatar_url: null }] : null,
        thread_following: true,
        thread_unread: false
    });
}

function replies(count: number): TimelineMessage[] {
    return Array.from({ length: count }, (_, i) => message(41 + i, { parent_id: 40, person_alias_guid: ben, sender_nick_name: "Ben", sender_last_name: "Okafor" }));
}

let wrapper: VueWrapper | null = null;

/**
 * Mounts the drawer, its owner keeping the reply composer's text as the shell keeps each thread's.
 * A test about an entrance keeps Vue's real Transition, which marks an element entering as a
 * browser plays it.
 */
function draw(props: Record<string, unknown> = {}, isTransitionKept = false, motion = createMessageMotion()): VueWrapper {
    const drawn: VueWrapper = mount(ThreadDrawer, {
        global: { provide: { [messageMotionKey as symbol]: motion }, ...(isTransitionKept ? { stubs: { transition: false } } : {}) },
        props: {
            draft: { text: "", mentions: [] },
            onDraftText: (text: string) => drawn.setProps({ draft: { text, mentions: [] } }),
            root: rootWith(3),
            replies: replies(3),
            pending: [],
            isFollowing: false,
            conversationTitle: channelTitle,
            isChannel: true,
            isDirectMessage: false,
            isPhone: false,
            isFromList: false,
            personAliasGuid: me,
            ...props
        },
        attachTo: document.body
    });
    wrapper = drawn;
    return wrapper;
}

/** Text as a person reads it. */
function plain(text: string | undefined | null): string {
    return (text ?? "").replace(/\s+/g, " ").trim();
}

/** Longer than the drawer waits before it focuses the composer. */
async function afterOpen(): Promise<void> {
    await nextTick();
    await new Promise(resolve => setTimeout(resolve, 150));
    await nextTick();
}

beforeAll(() => {
    // jsdom has no ResizeObserver or IntersectionObserver; the feed and the tooltips use them.
    window.ResizeObserver ??= class {
        observe(): void { /* jsdom lays nothing out. */ }
        unobserve(): void { /* As above. */ }
        disconnect(): void { /* As above. */ }
    } as unknown as typeof ResizeObserver;
    window.IntersectionObserver ??= class {
        observe(): void { /* jsdom computes no intersections. */ }
        unobserve(): void { /* As above. */ }
        disconnect(): void { /* As above. */ }
        takeRecords(): IntersectionObserverEntry[] {
            return [];
        }
    } as unknown as typeof IntersectionObserver;
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

// The tail's motions run through the browser's animations, which jsdom lacks, so a stand-in keeps them.
let animations: ReturnType<typeof playAnimations>;
beforeEach(() => {
    animations = playAnimations();
});
afterEach(() => {
    animations.restore();
});

/**
 * The panel host's scroller laid out at the sizes given, as a browser would, its scroll clamped
 * to them; the thread's body fills it from its top, so the newest end read from layout is where
 * the scroll height puts it at rest.
 */
function layOut(w: VueWrapper, sizes: { scrollHeight: number, clientHeight: number, scrollTop: number }): void {
    const scroller = w.get(".chat-panel-body").element as HTMLElement;
    const body = w.get(".chat-th-body").element as HTMLElement;
    Object.defineProperty(body, "offsetTop", { configurable: true, get: () => 0 });
    Object.defineProperty(body, "offsetHeight", { configurable: true, get: () => sizes.scrollHeight });
    Object.defineProperty(scroller, "scrollHeight", { configurable: true, get: () => sizes.scrollHeight });
    Object.defineProperty(scroller, "clientHeight", { configurable: true, get: () => sizes.clientHeight });
    Object.defineProperty(scroller, "scrollTop", {
        configurable: true,
        get: () => sizes.scrollTop,
        set: (value: number) => sizes.scrollTop = Math.max(0, Math.min(value, sizes.scrollHeight - sizes.clientHeight))
    });
}

// The reply composer under the thread grows with its text and takes that height from the thread,
// as the conversation's composer takes it from the feed. Whether the reader is at the newest reply
// is theirs to change by scrolling, never measured again as the drawer changes size, so a reader
// there stays there, and their own reply arrives as the conversation's does.
describe("a reader at the newest reply", () => {
    const sending = { localId: "k1", channelId: "c1", body: "On my way", status: "sending", errorCode: null, parentId: 40, isShownInChannel: false };
    let observers: Array<{ callback: ResizeObserverCallback, targets: Element[] }> = [];
    let previous: typeof ResizeObserver;

    beforeEach(() => {
        observers = [];
        previous = window.ResizeObserver;
        window.ResizeObserver = class {
            private readonly entry: { callback: ResizeObserverCallback, targets: Element[] };
            constructor(callback: ResizeObserverCallback) {
                this.entry = { callback, targets: [] };
                observers.push(this.entry);
            }
            observe(target: Element): void {
                this.entry.targets.push(target);
            }
            unobserve(): void { /* Nothing is watched apart. */ }
            disconnect(): void {
                this.entry.targets = [];
            }
        } as unknown as typeof ResizeObserver;
    });

    afterEach(() => {
        window.ResizeObserver = previous;
    });

    /** The reply composer growing by the height given, with no scroll; `isObserved` once the browser has told the drawer. */
    async function composerGrows(w: VueWrapper, sizes: { clientHeight: number }, by: number, isObserved: boolean): Promise<void> {
        sizes.clientHeight -= by;
        if (isObserved) {
            const scroller = w.get(".chat-panel-body").element;
            for (const observer of observers.filter(o => o.targets.includes(scroller))) {
                observer.callback([], observer as unknown as ResizeObserver);
            }
        }
        await nextTick();
    }

    test("keeps the newest reply in view as the reply composer grows past the near-bottom reach", async () => {
        const w = draw({ isChannel: false });
        await afterOpen();
        const sizes = { scrollHeight: 1000, clientHeight: 400, scrollTop: 600 };
        layOut(w, sizes);

        await composerGrows(w, sizes, 100, true);

        expect(sizes.scrollTop).toBe(700);
    });

    test("scrolled up, is left where they are as the reply composer grows", async () => {
        const w = draw({ isChannel: false });
        await afterOpen();
        const sizes = { scrollHeight: 1000, clientHeight: 400, scrollTop: 600 };
        layOut(w, sizes);
        // the reader scrolls up from the newest reply, the browser telling the drawer along the way
        const scroller = w.get(".chat-panel-body").element;
        scroller.dispatchEvent(new Event("scroll"));
        sizes.scrollTop = 200;
        scroller.dispatchEvent(new Event("scroll"));

        await composerGrows(w, sizes, 100, true);

        expect(sizes.scrollTop).toBe(200);
    });

    // The send can land in the frame the composer grew, before the browser has said so.
    test.each([true, false])("their own reply sent once the reply composer grew arrives at the thread's end as the conversation's does (resize heard first: %s)", async isObserved => {
        const motion = createMessageMotion();
        const w = draw({ isChannel: false, replies: replies(3), pending: [] }, false, motion);
        await afterOpen();
        const sizes = { scrollHeight: 1000, clientHeight: 400, scrollTop: 600 };
        layOut(w, sizes);
        await composerGrows(w, sizes, 100, isObserved);

        motion.sent("k1", place);
        await w.setProps({ pending: [sending] });
        await nextTick();
        await nextTick();

        expect(w.get("[data-testid='pending-row']").attributes("data-tail-played") ?? "").toContain("chat-tail-in");
        expect(sizes.scrollTop).toBe(700);
    });

    // A push or a link lands the reader on a reply far up with a smooth scroll, whose first scroll
    // event comes a frame or more later. A reply's photo decoding before it must not read them as
    // still at the newest reply and pull them back there.
    test("landed on a reply far up, a size change before the scroll's first frame leaves them going to it", async () => {
        const w = draw({ isChannel: false, replies: replies(20) });
        await afterOpen();
        const sizes = { scrollHeight: 1000, clientHeight: 400, scrollTop: 600 };
        layOut(w, sizes);
        const scroller = w.get(".chat-panel-body").element as HTMLElement;
        const landed = w.get("[data-reply-id='42']").element as HTMLElement;
        // the reply stands 320 px above the scroller's top, so centring it scrolls to 100
        landed.getBoundingClientRect = () => ({ top: -320, height: 40, bottom: -280, left: 0, right: 300, width: 300, x: 0, y: -320, toJSON: () => ({}) }) as DOMRect;
        scroller.getBoundingClientRect = () => ({ top: 0, height: 400, bottom: 400, left: 0, right: 300, width: 300, x: 0, y: 0, toJSON: () => ({}) }) as DOMRect;
        const centred = jest.fn();
        landed.scrollIntoView = centred;

        await w.setProps({ landOnId: 42 });
        await nextTick();
        await nextTick();
        expect(centred).toHaveBeenCalledTimes(1);

        // a photo decodes in a reply, before the smooth scroll has moved
        sizes.scrollHeight = 1100;
        for (const observer of observers.filter(o => o.targets.includes(scroller))) {
            observer.callback([], observer as unknown as ResizeObserver);
        }
        await nextTick();

        expect(sizes.scrollTop).toBe(600);
    });
});

describe("the header", () => {
    test("titles the thread and names its conversation, labelled by its title", () => {
        const w = draw();

        expect(w.get("[data-testid='panel-title']").text()).toBe("Thread");
        expect(w.get("[data-testid='panel-subtitle']").text()).toBe(channelTitle);
        expect(w.get("[data-testid='panel-host']").attributes("aria-label")).toBe("Thread");
    });

    test("names a direct message's conversation by its title", () => {
        const w = draw({ conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true });

        expect(w.get("[data-testid='panel-subtitle']").text()).toBe("Anna Reyes");
    });

    test("closes from its X on a wide block, with no back chevron", async () => {
        const w = draw();

        const close = w.get("[data-testid='panel-close']");
        expect(close.attributes("aria-label")).toBe("Close thread");
        expect(w.find("[data-testid='panel-back']").exists()).toBe(false);

        await close.trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });
});

describe("the body", () => {
    test("draws the root, then the replies, in the channel's look", () => {
        const w = draw();

        const rows = w.findAllComponents(MessageRow);
        expect(rows.map(r => (r.props("message") as TimelineMessage | null)?.id)).toEqual([40, 41, 42, 43]);
        expect(rows.every(r => r.props("isChannel") === true)).toBe(true);
    });

    test("draws no thread pill under the root and no reply label over a reply also sent", () => {
        const w = draw({ replies: [...replies(2), message(43, { parent_id: 40, shown_in_channel: true })] });

        expect(w.find("[data-testid='thread-summary']").exists()).toBe(false);
        expect(w.find("[data-testid='thread-reply-label']").exists()).toBe(false);
    });

    test("draws this root's replies still on their way after the stored ones, and no other root's", () => {
        const sending = { localId: "mine", channelId: "c1", body: "On my way", status: "sending", errorCode: null, parentId: 40, isShownInChannel: false };
        const elsewhere = { ...sending, localId: "elsewhere", parentId: 99 };
        const plainSend = { ...sending, localId: "conversation", parentId: null };
        const w = draw({ pending: [sending, elsewhere, plainSend] });

        const rows = w.findAllComponents(MessageRow);
        expect(rows.map(r => (r.props("pending") as { localId: string } | null)?.localId ?? null)).toEqual([null, null, null, null, "mine"]);
    });

    test("draws a direct message's thread in its look", () => {
        const w = draw({ conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true });

        expect(w.findAllComponents(MessageRow).every(r => r.props("isChannel") === false)).toBe(true);
    });
});

// A reply the person sends is one row from the moment it is sent, as in the conversation: the row
// still sending and every copy of the reply share the send's key, and a reply this session sent
// never rises a second time when it is drawn afresh. What rises is what was sent here or heard live
// while the drawer was open; a reply a page or a reread brings is simply there.
describe("the person's own reply", () => {
    const sending = { localId: "k1", channelId: "c1", body: "On my way", status: "sending", errorCode: null, parentId: 40, isShownInChannel: false };
    const mine = (id: number, key: string): TimelineMessage => message(id, { parent_id: 40, person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Lee", client_key: key });

    test("a copy that arrives while its row is still out takes the row's place, in the same element", async () => {
        const w = draw({ replies: replies(1), pending: [sending] });
        await nextTick();
        const drawn = w.get("[data-testid='pending-row']").element;

        await w.setProps({ replies: [...replies(1), mine(50, "k1")] });

        expect(w.find("[data-testid='pending-row']").exists()).toBe(false);
        expect(w.get("[data-reply-id='50']").element).toBe(drawn);
    });

    test("moved between groups by another person's newer reply, it rises only once; another tab's reply rises", async () => {
        const recent = { created_at: new Date().toISOString() };
        const earlier = { ...mine(45, "k0"), ...recent };
        const motion = createMessageMotion();
        motion.sent("k0", place);
        const w = draw({ isChannel: false, replies: [earlier], pending: [] }, false, motion);
        await afterOpen();
        motion.sent("k1", place);
        await w.setProps({ pending: [sending] });
        await nextTick();
        expect(w.get("[data-testid='pending-row']").attributes("data-tail-played") ?? "").toContain("chat-tail-in");

        const ben51 = message(51, { parent_id: 40, ...recent });
        motion.arrived(ben51, place);
        await w.setProps({ replies: [earlier, ben51] });
        await nextTick();
        expect(w.get("[data-testid='pending-row']").attributes("data-tail-played") ?? "").not.toContain("chat-tail-in");

        const anotherTab = { ...mine(52, "another-tab"), ...recent };
        motion.arrived(anotherTab, place);
        await w.setProps({ replies: [earlier, { ...mine(50, "k1"), ...recent }, ben51, anotherTab], pending: [] });
        await nextTick();

        expect(w.get("[data-reply-id='50']").attributes("data-tail-played") ?? "").not.toContain("chat-tail-in");
        expect(w.get("[data-reply-id='51']").attributes("data-tail-played") ?? "").toContain("chat-tail-in");
        expect(w.get("[data-reply-id='52']").attributes("data-tail-played") ?? "").toContain("chat-tail-in");
    });

    test("a reply that settles while the thread is open shows Sent for 2.2 seconds, then the time alone; another tab's never does", async () => {
        jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
        try {
            const row = { ...sending };
            const w = draw({ replies: replies(1), pending: [row], sentKeys: new Set(["k1"]) });
            await nextTick();

            // The confirmed reply goes in before the row comes out, naming the message it
            // became, as the send does it.
            await w.setProps({ replies: [...replies(1), mine(50, "k1"), mine(52, "another-tab")] });
            row.confirmedId = 50;
            await w.setProps({ pending: [] });
            await nextTick();

            const state = (id: number): string | null => {
                const found = w.find(`[data-reply-id='${id}'] [data-testid='message-state']`);
                return found.exists() ? found.text() : null;
            };
            expect(state(50)).toBe("Sent");
            expect(state(52)).toBeNull();

            jest.advanceTimersByTime(2199);
            await nextTick();
            expect(state(50)).toBe("Sent");

            jest.advanceTimersByTime(1);
            await nextTick();
            expect(state(50)).toBeNull();
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("a leader's own reply in a channel keeps one Leader pill from sending to stored, whatever its copy says", async () => {
        const w = draw({ replies: replies(1), pending: [sending], isPersonLeader: true });
        await nextTick();
        const pill = w.get("[data-testid='pending-row'] [data-testid='leader-pill']").element;

        await w.setProps({ replies: [...replies(1), { ...mine(50, "k1"), sender_is_leader: false }] });

        expect(w.get("[data-reply-id='50'] [data-testid='leader-pill']").element).toBe(pill);
    });
});

describe("the divider", () => {
    test.each([[3, "3 replies"], [1, "1 reply"]])("counts %i replies as %s", (count, words) => {
        const w = draw({ root: rootWith(count), replies: replies(count) });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe(words);
    });

    test("counts every reply of the thread, not only the ones loaded", () => {
        const w = draw({ root: rootWith(50), replies: replies(3) });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe("50 replies");
    });

    test.each([[99, "99 replies"], [100, "99+ replies"], [120, "99+ replies"]])("counts %i replies as %s, as the thread pill does", (count, words) => {
        const w = draw({ root: rootWith(count), replies: replies(3) });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe(words);
    });

    test("invites the first reply when there is none", () => {
        const w = draw({ root: rootWith(0), replies: [] });

        expect(plain(w.get("[data-testid='thread-divider']").text())).toBe("No replies yet. Start the thread below.");
    });
});

describe("the Follow toggle", () => {
    test("reads Follow and asks to follow", async () => {
        const w = draw({ isFollowing: false });

        const toggle = w.get("[data-testid='thread-follow']");
        expect(plain(toggle.text())).toBe("Follow");
        expect(toggle.attributes("aria-pressed")).toBe("false");

        await toggle.trigger("click");
        expect(w.emitted("follow")).toEqual([[true]]);
    });

    test("reads Following and asks to stop", async () => {
        const w = draw({ isFollowing: true });

        const toggle = w.get("[data-testid='thread-follow']");
        expect(plain(toggle.text())).toBe("Following");
        expect(toggle.attributes("aria-pressed")).toBe("true");

        await toggle.trigger("click");
        expect(w.emitted("follow")).toEqual([[false]]);
    });

    test.each([
        [false, "Follow to be notified about every reply."],
        [true, "You're notified about every reply."]
    ])("following %s, is described by what following means, and shows it as its tooltip on hover", async (isFollowing, words) => {
        jest.useFakeTimers();
        try {
            const w = draw({ isFollowing });
            const toggle = w.get("[data-testid='thread-follow']");

            expect(document.getElementById(toggle.attributes("aria-describedby") ?? "")?.textContent?.trim()).toBe(words);
            // The sentence is the button's description, never its name.
            expect(plain(toggle.text())).toBe(isFollowing ? "Following" : "Follow");

            await toggle.trigger("mouseenter");
            jest.advanceTimersByTime(600);
            await nextTick();

            expect(document.querySelector("[role='tooltip']")?.textContent?.trim()).toBe(words);

            await toggle.trigger("mouseleave");
            expect(document.querySelector("[role='tooltip']")).toBeNull();
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("is not offered while the thread's first page is on its way, since whether the reader follows is not known yet", () => {
        const w = draw({ root: null, replies: [], isLoading: true, isFollowing: false });

        expect(w.find("[data-testid='thread-follow']").exists()).toBe(false);
    });

    test("is offered once the first page has answered", async () => {
        const w = draw({ root: null, replies: [], isLoading: true });
        await w.setProps({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: true });

        expect(plain(w.get("[data-testid='thread-follow']").text())).toBe("Following");
    });

    test("is one toggle however often the thread loads again, in the production runtime the block ships with", async () => {
        // The built block keeps template comments and runs Vue's production runtime, which keeps
        // no element for a comment it patches in place; the test renderer runs the development
        // runtime, which does. So this mounts the drawer in a module registry of its own, with
        // Vue's production build, where a comment in the header's slot once broke the next reload.
        const previousEnvironment = process.env.NODE_ENV;
        process.env.NODE_ENV = "production";
        let production!: { mount: typeof mount, nextTick: typeof nextTick, drawer: typeof ThreadDrawer };
        jest.isolateModules(() => {
            production = {
                mount: jest.requireActual("@vue/test-utils").mount,
                nextTick: jest.requireActual("vue").nextTick,
                drawer: jest.requireActual("../../../../../src/Communication/Chat/ChatShell/components/threadDrawer.partial.obs").default
            };
        });
        process.env.NODE_ENV = previousEnvironment;

        // The production runtime logs a render that threw rather than rethrowing it.
        const logged = jest.spyOn(console, "error");
        const w = production.mount(production.drawer, {
            props: { root: null, replies: [], isLoading: true, personAliasGuid: me, draft: { text: "", mentions: [] } },
            attachTo: document.body
        });
        wrapper = w;

        for (let load = 0; load < 3; load++) {
            await w.setProps({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: false });
            // The answer redrawn in place, as a follow change does, before the thread loads again.
            await w.setProps({ isFollowing: true });
            await production.nextTick();
            await w.setProps({ isLoading: true });
        }
        await w.setProps({ isLoading: false });

        const loggedErrors = logged.mock.calls.length;
        logged.mockRestore();
        expect(loggedErrors).toBe(0);
        expect(document.querySelectorAll("[data-testid='thread-follow']")).toHaveLength(1);
    });

    test("is not offered after the first page failed, since whether the reader follows is still not known", async () => {
        const w = draw({ root: null, replies: [], isLoading: true });
        await w.setProps({ isLoading: false, error: { code: "net.offline", severity: "degraded", text: null } });

        expect(w.find("[data-testid='thread-error']").exists()).toBe(true);
        expect(w.find("[data-testid='thread-follow']").exists()).toBe(false);
    });

    test("stays offered after a page of older replies failed, with that failure said", async () => {
        const w = draw({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: true });
        await w.setProps({ olderError: { code: "net.offline", severity: "degraded", text: null } });

        expect(w.find("[data-testid='thread-error']").exists()).toBe(true);
        expect(plain(w.get("[data-testid='thread-follow']").text())).toBe("Following");
    });

    test("a refused change shows its sentence and Try again under the header, said at once, and Try again asks it again with focus on the toggle", async () => {
        const w = draw({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: true, followError: { rootId: 40, isFollowing: false, failure: { alone: "You're offline. Try again when you're back.", besideRetry: "You're offline.", canRetry: true } } });
        await nextTick();

        const error = w.get("[data-testid='thread-follow-error']");
        expect(plain(error.text())).toBe("You're offline. Try again");
        expect(error.attributes("role")).toBe("alert");

        await error.get("[data-testid='note-retry']").trigger("click");

        expect(w.emitted("follow")).toEqual([[false]]);
        expect(document.activeElement).toBe(w.get("[data-testid='thread-follow']").element);
    });

    test("a refusal another try cannot pass shows its sentence under the header with no Try again", async () => {
        const w = draw({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: true, followError: { rootId: 40, isFollowing: false, failure: { alone: "You're no longer in this channel.", besideRetry: "You're no longer in this channel.", canRetry: false } } });
        await nextTick();

        const error = w.get("[data-testid='thread-follow-error']");
        expect(plain(error.text())).toBe("You're no longer in this channel.");
        expect(error.find("[data-testid='note-retry']").exists()).toBe(false);
    });

    test("shows no refusal while there is none", () => {
        const w = draw({ root: rootWith(3), replies: replies(3), isLoading: false, isFollowing: true });

        expect(w.find("[data-testid='thread-follow-error']").exists()).toBe(false);
    });

    // Following shows as the bell itself; only unfollowing says what it means, as a notify mode does.
    test("toasts only an unfollow, in the design's words", () => {
        expect(unfollowNotice.text).toBe("You won't be notified about new replies");
    });
});

/** Types one line into the reply composer's editor as a browser does: the text, then input. */
async function typeReply(w: VueWrapper, text: string): Promise<void> {
    const editor = w.get("[data-testid='composer-input']").element as HTMLElement;
    editor.textContent = text;
    editor.dispatchEvent(new Event("input", { bubbles: true }));
    await nextTick();
}

describe("the reply composer", () => {
    test("asks for a reply in the thread", () => {
        const w = draw();

        expect(w.get("[data-testid='composer-input']").attributes("data-placeholder")).toBe("Reply in thread");
    });

    test("offers to send the reply to the channel too, by the channel's name", () => {
        const w = draw();

        expect(plain(w.get("[data-testid='composer-also']").text())).toBe(`Also send to ${channelTitle}`);
        expect(w.find("[data-testid='composer-hint']").exists()).toBe(false);
    });

    test("offers to send the reply to a direct message as the conversation", () => {
        const w = draw({ conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true });

        expect(plain(w.get("[data-testid='composer-also']").text())).toBe("Also send to the conversation");
    });

    test("sends a reply to the thread alone unless the switch is on", async () => {
        const w = draw();

        await typeReply(w, "Count me in");
        await w.get("[data-testid='composer-input']").trigger("keydown", { key: "Enter" });

        expect(w.emitted("send")).toEqual([["Count me in", false]]);
    });

    test("sends a reply to the conversation too when the switch is on", async () => {
        const w = draw();

        await w.get("[data-testid='composer-also'] [role='switch']").trigger("click");
        await typeReply(w, "Count me in");
        await w.get("[data-testid='composer-input']").trigger("keydown", { key: "Enter" });

        expect(w.emitted("send")).toEqual([["Count me in", true]]);
    });
});

// The reply composer types into the thread, not the conversation, so what it keeps for one place
// (the names its chips draw, what it last said aloud) is the thread's.
test("the reply composer is keyed by its thread", () => {
    const w = draw();

    expect(w.findComponent(Composer).props("conversationKey")).toBe("thread:40");
});

// Someone typing a reply in this thread is drawn as the thread's newest row, as the conversation
// draws its own, and kept in view the way a new reply is: followed by a reader at the bottom, and
// out of sight below one scrolled up, who is not moved.
describe("the marks the tail reads", () => {
    test("a group's face and name line carry its first reply's key, and every reply its box and time line", () => {
        const w = draw({ isChannel: false, replies: [...replies(2), message(43, { parent_id: 40 })] });
        const rows = w.findAll("[data-reply-id]").map(row => row.element as HTMLElement);
        const keys = rows.map(row => row.dataset.rowKey);

        expect(rows[0].querySelector("[data-tail^='name:']")?.getAttribute("data-tail")).toBe(`name:${keys[0]}`);
        expect(rows[1].querySelector("[data-tail^='face:']")?.getAttribute("data-tail")).toBe(`face:${keys[0]}`);
        expect(rows[0].querySelector("[data-tail^='face:']")).toBeNull();
        expect(rows[2].querySelector("[data-tail^='face:']")?.getAttribute("data-tail")).toBe(`face:${keys[2]}`);
        expect(rows.every(row => row.querySelectorAll("[data-tail='box']").length === 1 && row.querySelectorAll("[data-tail='meta']").length === 1)).toBe(true);
    });
});

describe("someone typing a reply", () => {
    const annaTyping: Typing = { typists: [{ alias: anna, person: { person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null } }], names: "Anna", sentence: "Anna is typing…" };

    test("is the thread's newest row, after the last reply, in a group with its name line", () => {
        const w = draw({ isChannel: false, typing: annaTyping });
        const bubble = w.get("[data-testid='typing-bubble']").element;
        const rows = w.findAll("[data-reply-id]").map(row => row.element);

        expect(rows.every(row => (row.compareDocumentPosition(bubble) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0)).toBe(true);
        // its layout is its own, so no group rule of the drawer's can turn its grid into another box
        expect(bubble.classList.contains("chat-grp")).toBe(false);
        expect(w.get("[data-testid='typing-names']").text()).toBe("Anna");
    });

    // As in the conversation, a reply of hers newest makes the bubble her run's next row.
    test("after the typist's own newest reply it is her run's next row, the face moving to it", () => {
        jest.useFakeTimers({ now: new Date(2026, 9, 13, 9, 44), doNotFake: ["queueMicrotask", "nextTick"] });
        try {
            const hers = message(44, { parent_id: 40 });
            const w = draw({ isChannel: false, replies: [...replies(3), hers], typing: annaTyping });
            const row = w.get("[data-reply-id='44']");

            expect(w.findComponent(TypingBubble).props("isInRun")).toBe(true);
            expect(w.find("[data-testid='typing-names']").exists()).toBe(false);
            expect(row.find("[data-testid='sender-avatar']").exists()).toBe(false);
            expect(row.find("[data-testid='message-meta']:not([hidden])").exists()).toBe(true);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("in her run it is a group of its own once her run's five minutes are over", async () => {
        jest.useFakeTimers({ now: new Date(2026, 9, 13, 9, 44), doNotFake: ["queueMicrotask", "nextTick"] });
        try {
            const w = draw({ isChannel: false, replies: [...replies(3), message(44, { parent_id: 40 })], typing: annaTyping });
            expect(w.findComponent(TypingBubble).props("isInRun")).toBe(true);

            jest.advanceTimersByTime(5 * 60 * 1000 + 1);
            await nextTick();
            expect(w.findComponent(TypingBubble).props("isInRun")).toBe(false);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("after someone else's reply it is a group of its own", () => {
        jest.useFakeTimers({ now: new Date(2026, 9, 13, 9, 44), doNotFake: ["queueMicrotask", "nextTick"] });
        try {
            const w = draw({ isChannel: false, typing: annaTyping });

            expect(w.findComponent(TypingBubble).props("isInRun")).toBe(false);
        }
        finally {
            jest.useRealTimers();
        }
    });

    test("is not drawn before the thread's first page is there", async () => {
        const w = draw({ isChannel: false, root: null, replies: [], isLoading: true, typing: annaTyping });
        expect(w.find("[data-testid='typing-bubble']").exists()).toBe(false);

        await w.setProps({ root: rootWith(3), replies: replies(3), isLoading: false });
        expect(w.find("[data-testid='typing-bubble']").exists()).toBe(true);
    });

    test("in a direct message it has no name line, and with no one typing it is not drawn", async () => {
        const w = draw({ isChannel: false, isDirectMessage: true, typing: annaTyping });
        expect(w.findComponent(TypingBubble).props("isGroup")).toBe(false);
        expect(w.find("[data-testid='typing-names']").exists()).toBe(false);

        await w.setProps({ typing: null });
        expect(w.find("[data-testid='typing-bubble']").exists()).toBe(false);
    });

    test("is given no replies, only who types and where it stands", () => {
        const w = draw({ isChannel: false, typing: annaTyping });

        expect(Object.keys(w.findComponent(TypingBubble).props()).sort()).toEqual(["groupKey", "isGroup", "isInRun", "typing"]);
    });

    test("the reply composer is the same with someone typing as without, and is given no typing", async () => {
        const w = draw({ isChannel: false });
        await nextTick();
        const alone = w.get("[data-testid='composer-input']").element.closest(".chat-composer")?.outerHTML;

        await w.setProps({ typing: annaTyping });

        expect(w.findComponent(Composer).props()).not.toHaveProperty("typingLine");
        expect(w.get("[data-testid='composer-input']").element.closest(".chat-composer")?.outerHTML).toBe(alone);
    });

    test("a reader at the bottom follows it into view", async () => {
        const w = draw({ isChannel: false });
        await nextTick();
        await nextTick();
        const sizes = { scrollHeight: 1000, clientHeight: 400, scrollTop: 600 };
        layOut(w, sizes);

        await w.setProps({ typing: annaTyping });
        sizes.scrollHeight = 1065;
        await nextTick();
        await nextTick();

        expect(sizes.scrollTop).toBe(665);
    });

    test("a reader scrolled up is not moved", async () => {
        const w = draw({ isChannel: false });
        await nextTick();
        await nextTick();
        const sizes = { scrollHeight: 1000, clientHeight: 400, scrollTop: 600 };
        layOut(w, sizes);
        // the reader scrolls up from the newest reply, the browser telling the drawer along the way
        const scroller = w.get(".chat-panel-body").element;
        scroller.dispatchEvent(new Event("scroll"));
        sizes.scrollTop = 200;
        scroller.dispatchEvent(new Event("scroll"));

        await w.setProps({ typing: annaTyping });
        sizes.scrollHeight = 1065;
        await nextTick();
        await nextTick();

        expect(sizes.scrollTop).toBe(200);
    });
});

describe("the reply composer's @ picker", () => {
    test("opens on @ from the drawer's mention source, tells the drawer's owner the query, and takes Enter for the chip", async () => {
        const mentionSource = { canMention: true, people: [{ person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor" }], isAllOffered: false };
        const w = draw({ mentionSource });

        // Typed as a browser leaves it, the caret after the query.
        const editor = w.get("[data-testid='composer-input']").element as HTMLElement;
        const text = document.createTextNode("Thanks @be");
        editor.replaceChildren(text);
        const range = document.createRange();
        range.setStart(text, text.data.length);
        range.collapse(true);
        window.getSelection()?.removeAllRanges();
        window.getSelection()?.addRange(range);
        editor.dispatchEvent(new Event("input", { bubbles: true }));
        await nextTick();
        await nextTick();

        expect(document.querySelector("[data-testid='mention-picker']")).not.toBeNull();
        expect(w.emitted("mentionQuery")?.slice(-1)).toEqual([["be"]]);

        await w.get("[data-testid='composer-input']").trigger("keydown", { key: "Enter" });
        await nextTick();

        expect(w.emitted("send")).toBeUndefined();
        expect(editor.querySelectorAll("[data-testid='editor-chip']")).toHaveLength(1);
        expect(document.querySelector("[data-testid='mention-picker']")).toBeNull();
    });

    test("tells the drawer's owner when the reply composer takes focus, so the picker's people are fetched before the @", () => {
        const w = draw({ mentionSource: { canMention: true, people: [], isAllOffered: false } });

        (w.get("[data-testid='composer-input']").element as HTMLElement).focus();

        expect(w.emitted("composerFocus")).toHaveLength(1);
    });
});

describe("Escape", () => {
    test("closes a thread opened from the conversation", async () => {
        const w = draw();

        await w.get("[data-testid='panel-host']").trigger("keydown", { key: "Escape" });

        expect(w.emitted("close")).toHaveLength(1);
        expect(w.emitted("back")).toBeUndefined();
    });

    test("returns to the threads list a thread was opened from, as its back chevron does", async () => {
        const w = draw({ isFromList: true });

        const back = w.get("[data-testid='panel-back']");
        expect(back.attributes("aria-label")).toBe("Back to Threads");

        await w.get("[data-testid='panel-host']").trigger("keydown", { key: "Escape" });
        expect(w.emitted("back")).toHaveLength(1);
        expect(w.emitted("close")).toBeUndefined();

        await back.trigger("click");
        expect(w.emitted("back")).toHaveLength(2);
    });
});

describe("focus on open", () => {
    test("goes to the reply composer on a wide block", async () => {
        const w = draw();
        await afterOpen();

        expect(document.activeElement).toBe(w.get("[data-testid='composer-input']").element);
    });

    test("goes to the back chevron on a phone, never the composer, and the phone has no X", async () => {
        const w = draw({ isPhone: true });
        await afterOpen();

        expect(document.activeElement).not.toBe(w.get("[data-testid='composer-input']").element);
        expect(document.activeElement).toBe(w.get("[data-testid='panel-back']").element);
        expect(w.get("[data-testid='panel-back']").attributes("aria-label")).toBe("Back");
        expect(w.find("[data-testid='panel-close']").exists()).toBe(false);

        await w.get("[data-testid='panel-back']").trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });
});

// The person's own reply is edited in place in the drawer like any message: Up in the empty reply
// box asks for the newest of their replies, and the reply open in the editor draws it with any
// refusal under it, asking the block to save or cancel as a conversation's row does.
describe("editing the person's own reply", () => {
    const ownReply = (): TimelineMessage => message(44, { parent_id: 40, person_alias_guid: me, sender_nick_name: "Jordan", sender_last_name: "Ellis", body: "Count me in" });

    test("Up in the empty reply box asks to edit the newest reply", async () => {
        const w = draw();

        await w.get("[data-testid='composer-input']").trigger("keydown", { key: "ArrowUp" });

        expect(w.emitted("editLast")).toHaveLength(1);
    });

    test("the reply open in the editor is drawn in place, with the refusal it met", async () => {
        const w = draw({
            replies: [...replies(2), ownReply()],
            editingId: 44,
            editError: { code: "rpc.not_editable", severity: "failed", text: "This message can't be edited." }
        });
        await nextTick();

        expect(w.findAll("[data-testid='message-editor']")).toHaveLength(1);
        expect(w.get("[data-testid='edit-error']").text()).toBe("This message can't be edited.");

        await w.get("[data-testid='edit-cancel']").trigger("click");

        expect(w.emitted("messageAction")).toEqual([[expect.objectContaining({ kind: "cancelEdit" })]]);
    });
});

// The root is drawn on its own, outside the replies' groups, and can be a block post: a press on
// its button must still reach the drawer's owner, or the button stays busy and nothing is run.
describe("a block post as the root", () => {
    test("a press on its button reaches the drawer's owner as the message's server action", async () => {
        const card = message(40, {
            message_type: "system",
            body: "Count me in",
            reply_count: 1,
            blocks: [{
                type: "buttons",
                fallback: { type: "text", text: "Count me in" },
                buttons: [{ id: "join", label: "Count me in", call: "chat_action_join" }]
            }]
        });
        const drawer = draw({ root: card, replies: replies(1) });
        await nextTick();

        await drawer.get("[data-testid='block-button']").trigger("click");

        const asked = drawer.emitted("messageAction") as [{ kind: string, message: TimelineMessage, action: { id: string, call: string } }][] | undefined;
        expect(asked?.length).toBe(1);
        expect(asked?.[0][0].kind).toBe("server");
        expect(asked?.[0][0].message.id).toBe(40);
        expect(asked?.[0][0].action).toMatchObject({ id: "join", call: "chat_action_join" });
    });
});

// The root's bar and menu are the conversation's: the person's own root offers Edit, as their own
// row does there, and someone else's does not. Whether a message is the person's is the server's
// is_own, which also covers a root sent under another of their aliases.
describe("the root's menu", () => {
    /** Rests the pointer on the root until its bar shows. */
    async function hoverRoot(w: VueWrapper): Promise<void> {
        await w.get("[data-hot-id='40']").trigger("pointerover");
        await new Promise(resolve => setTimeout(resolve, 170));
        await nextTick();
    }

    /** Opens the root's More menu and reads its item labels, key hints left out. */
    async function rootMenu(w: VueWrapper): Promise<string[]> {
        await hoverRoot(w);
        (w.get("[data-testid='action-bar'] button[aria-label='More actions']").element as HTMLElement).click();
        await nextTick();
        await nextTick();

        return Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).map(item => {
            const copy = item.cloneNode(true) as HTMLElement;
            copy.querySelectorAll("kbd").forEach(kbd => kbd.remove());
            return (copy.textContent ?? "").trim();
        });
    }

    test("offers Edit on the person's own root", async () => {
        const ownRoot = message(40, { person_alias_guid: "a0000009-0000-4000-8000-000000000000", is_own: true, sender_nick_name: "Jordan", sender_last_name: "Ellis", reply_count: 3 });
        const w = draw({ root: ownRoot });
        await nextTick();

        expect(await rootMenu(w)).toContain("Edit message");
    });

    test("leaves Edit off someone else's root", async () => {
        const w = draw({ root: { ...rootWith(3), is_own: false } });
        await nextTick();

        const items = await rootMenu(w);
        expect(items).toContain("Copy text");
        expect(items).not.toContain("Edit message");
    });

    test("its Quote asks the drawer's owner to quote the root, as a conversation row's does", async () => {
        const root = rootWith(3);
        const w = draw({ root });
        await nextTick();
        await hoverRoot(w);

        (w.get("[data-testid='action-bar'] button[aria-label='Quote']").element as HTMLElement).click();
        await nextTick();

        expect(w.emitted("messageAction")).toEqual([[{ kind: "quote", message: root }]]);
    });

    test("its Copy text asks the drawer's owner to copy the root, as a conversation row's does", async () => {
        const root = rootWith(3);
        const w = draw({ root });
        await nextTick();
        await rootMenu(w);

        const copy = Array.from(document.querySelectorAll<HTMLElement>("[role='menu'] [role='menuitem']")).find(item => (item.textContent ?? "").includes("Copy text"));
        copy?.click();
        await nextTick();

        expect(w.emitted("messageAction")).toEqual([[{ kind: "copy", message: root }]]);
    });
});

// A thread waterfalls in when it opens: the root, the replies line and each run of replies rise
// one after another as the first page is drawn. A reply that arrives later rises on its own, as
// in the conversation, and the opening does not play again.
describe("the thread's entrance", () => {
    function entered(w: VueWrapper): { isEntering: boolean, order: string[] } {
        return {
            isEntering: w.get(".chat-panel-body").classes().includes("is-entering"),
            order: w.findAll(".chat-enter").map(e => (e.element as HTMLElement).style.getPropertyValue("--chat-enter-i"))
        };
    }

    test("the root, the replies line and the replies rise in, in order, as the first page is drawn", async () => {
        const w = draw({ root: null, replies: [], isLoading: true });
        expect(entered(w).isEntering).toBe(false);

        const mixed = [...replies(1), message(42, { parent_id: 40 }), ...replies(1).map(r => ({ ...r, id: 43 }))];
        await w.setProps({ root: rootWith(3), replies: mixed, isLoading: false });

        const drawn = entered(w);
        expect(drawn.isEntering).toBe(true);
        expect(drawn.order).toEqual(["0", "1", "2", "3", "4"]);
        expect(w.get("[data-testid='thread-divider']").classes()).toContain("chat-enter");
    });

    test("a reply that arrives after the thread opened does not rise in, and the rest go on rising", async () => {
        const w = draw({ root: rootWith(1), replies: replies(1).map(r => ({ ...r, person_alias_guid: anna, sender_nick_name: "Anna", sender_last_name: "Reyes" })) });
        expect(entered(w).isEntering).toBe(true);

        const before = w.findAll(".chat-enter").length;
        await w.setProps({ replies: [...replies(1).map(r => ({ ...r, person_alias_guid: anna, sender_nick_name: "Anna", sender_last_name: "Reyes" })), message(60, { parent_id: 40, person_alias_guid: ben, sender_nick_name: "Ben", sender_last_name: "Okafor" })] });
        expect(entered(w).isEntering).toBe(true);
        const settledRows = w.findAll(".chat-enter").filter(e => e.attributes("data-chat-settled") !== undefined);
        expect(settledRows.length).toBe(w.findAll(".chat-enter").length - before);
        expect(settledRows.length).toBeGreaterThan(0);
    });
});

// The thread keeps one action bar, as the conversation's feed does: it shows once the pointer
// rests on a message and then moves from message to message at once.
describe("the thread's action bar", () => {
    test("is one bar, which moves to the next message the pointer reaches once it shows", async () => {
        jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
        try {
            const w = draw();
            await nextTick();

            await w.get("[data-hot-id='41']").trigger("pointerover");
            jest.advanceTimersByTime(149);
            await nextTick();
            expect(w.findAll("[data-testid='action-bar']")).toHaveLength(0);

            jest.advanceTimersByTime(1);
            await nextTick();
            await nextTick();
            expect(w.get("[data-hot-id='41']").find("[data-testid='action-bar']").exists()).toBe(true);

            await w.get("[data-hot-id='42']").trigger("pointerover");
            await nextTick();
            expect(w.findAll("[data-testid='action-bar']")).toHaveLength(1);
            expect(w.get("[data-hot-id='42']").find("[data-testid='action-bar']").exists()).toBe(true);
        }
        finally {
            jest.useRealTimers();
        }
    });

    describe("once shown", () => {
        beforeEach(() => {
            jest.useFakeTimers({ doNotFake: ["queueMicrotask", "nextTick"] });
        });

        afterEach(() => {
            jest.useRealTimers();
        });

        async function settle(): Promise<void> {
            await nextTick();
            await nextTick();
        }

        async function wait(ms: number): Promise<void> {
            jest.advanceTimersByTime(ms);
            await settle();
        }

        /** The reply the one bar is drawn on, or null with none drawn. */
        function barOn(w: VueWrapper): number | null {
            const drawn = w.findAll("[data-testid='action-bar']");
            expect(drawn.length).toBeLessThanOrEqual(1);
            const row = drawn[0]?.element.closest<HTMLElement>("[data-hot-id]");
            return row ? Number(row.dataset.hotId) : null;
        }

        async function showOn(w: VueWrapper, id: number): Promise<void> {
            await w.get(`[data-hot-id='${id}']`).trigger("pointerover");
            await wait(150);
        }

        // A drawer opened on another thread is a fresh drawer, and its first bar enters as any other.
        test("the first bar in a drawer just opened plays its entrance", async () => {
            const w = draw({}, true);
            await settle();

            await showOn(w, 41);

            expect(barOn(w)).toBe(41);
            expect(w.get("[data-testid='action-bar']").classes()).toContain("chat-tools-enter-active");
        });

        test("hides 220ms after the pointer leaves the thread, and a pointer back inside that keeps it", async () => {
            const w = draw();
            await settle();
            await showOn(w, 41);
            const body = w.get(".chat-th-body");

            await body.trigger("pointerleave");
            await wait(150);
            await w.get("[data-hot-id='42']").trigger("pointerover");
            await wait(1000);
            expect(barOn(w)).toBe(42);

            await body.trigger("pointerleave");
            await wait(219);
            expect(barOn(w)).toBe(42);
            await wait(1);
            expect(barOn(w)).toBeNull();
        });

        test("a scroll of the thread hides it at once", async () => {
            const w = draw();
            await settle();
            await showOn(w, 41);

            w.get(".chat-panel-body").element.dispatchEvent(new Event("scroll"));
            await settle();

            expect(barOn(w)).toBeNull();
        });
    });
});

// A reply that failed is marked on its row; the strip over the reply composer speaks for this
// thread's failed replies alone, and its buttons reach the drawer's owner one reply at a time.
describe("the person's failed replies", () => {
    const failed = (localId: string, extra: Record<string, unknown> = {}): Record<string, unknown> =>
        ({ localId, channelId: "c1", body: localId, status: "failed", errorCode: "rpc.transport", reason: { alone: "You're offline. Try again when you're back.", besideRetry: "You're offline.", canRetry: true }, parentId: 40, isShownInChannel: false, ...extra });
    const refusal = { code: "authz.no_mention_all_right", severity: "permission", text: "You can't mention everyone in this channel." };

    test("are counted in the reply composer's strip, this thread's only", () => {
        const w = draw({ pending: [failed("a"), failed("b"), failed("elsewhere", { parentId: 99 }), failed("plain", { parentId: null })] });

        expect(w.get("[data-testid='composer-failed-lead']").text()).toBe("2 messages weren't sent");
        expect(w.get("[data-testid='composer-failed-reason']").text()).toBe("You're offline.");
    });

    test("Try again asks for each that can go, oldest first, in one go so they are sent in turn, and Delete drops each", async () => {
        const w = draw({ pending: [failed("a"), failed("refused", { refusal, reason: { alone: refusal.text, besideRetry: refusal.text, canRetry: false } }), failed("b")] });

        await w.get("[data-testid='composer-failed-retry']").trigger("click");
        expect(w.emitted("retry")).toEqual([[["a", "b"]]]);

        await w.get("[data-testid='composer-failed-discard']").trigger("click");
        expect(w.emitted("discard")).toEqual([["a"], ["refused"], ["b"]]);
    });

    test("say a reply's failure as it happens, from the sender's news", async () => {
        const w = draw({ pending: [failed("a")] });

        await w.setProps({ lastFailure: { localId: "a", count: 1 } });

        expect(w.get("[data-testid='composer-failed-announcement']").text()).toBe("Your message wasn't sent. You're offline.");
    });

    test("draw no strip while every reply is on its way", () => {
        const w = draw({ pending: [failed("a", { status: "sending", errorCode: null })] });

        expect(w.find("[data-testid='composer-failed']").exists()).toBe(false);
    });
});

// The thread's failed replies are mended one at a time from their own Not sent marker, as a
// conversation's are, and reach the drawer's owner by the same two events as the strip.
describe("a failed reply's own choices", () => {
    const failedReply = (localId: string): Record<string, unknown> =>
        ({ localId, channelId: "c1", body: localId, status: "failed", errorCode: "rpc.transport", parentId: 40, isShownInChannel: false });

    test("Try again on one reply asks for that reply alone, and Delete drops that reply alone", async () => {
        const w = draw({ pending: [failedReply("a"), failedReply("b")] });
        const markers = w.findAll("[data-testid='message-failed-actions']");
        expect(markers).toHaveLength(2);

        await markers[1].trigger("click");
        await nextTick();
        await w.findAll("[role='menuitem']")[0].trigger("click");
        expect(w.emitted("retry")).toEqual([[["b"]]]);

        await w.findAll("[data-testid='message-failed-actions']")[0].trigger("click");
        await nextTick();
        await w.findAll("[role='menuitem']")[1].trigger("click");
        expect(w.emitted("discard")).toEqual([["a"]]);
    });
});
