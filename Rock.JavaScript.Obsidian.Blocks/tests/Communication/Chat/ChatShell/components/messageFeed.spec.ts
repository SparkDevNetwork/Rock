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
// The feed, mounted: the timeline in day sections, the person's own sends settling in place among
// the stored messages, and the furniture around them. The New divider marks where reading stopped;
// the unread strip points to it only while it is off screen, and Mark as read saves the read
// position at the newest message. Jump to latest appears once the reader has scrolled up and
// counts what arrived meanwhile. A slow first page shows the skeleton, a quick one never does.
//
// jsdom lays nothing out, so the tests stand in for the browser twice: an IntersectionObserver
// that the test reports through, and the feed's scroll sizes set by hand.
import { flushPromises, mount, VueWrapper } from "@vue/test-utils";
import { readFileSync } from "fs";
import { join } from "path";
import { defineComponent, h, nextTick, reactive } from "vue";
import ChannelIntro from "../../../../../src/Communication/Chat/ChatShell/components/channelIntro.partial.obs";
import MessageFeed from "../../../../../src/Communication/Chat/ChatShell/components/messageFeed.partial.obs";
import ActionBar from "../../../../../src/Communication/Chat/ChatShell/components/actionBar.partial.obs";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import { TimelineState } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { pageRevealKey } from "../../../../../src/Communication/Chat/ChatShell/pageLoad.partial";
import { createSender, SendResult } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { ChannelFacts, PendingMessage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

// #region Browser stand-ins

/** An IntersectionObserver the test reports through, since jsdom computes no intersections. */
class FakeIntersectionObserver {
    static all: FakeIntersectionObserver[] = [];

    readonly watched = new Set<Element>();

    constructor(private readonly callback: IntersectionObserverCallback) {
        FakeIntersectionObserver.all.push(this);
    }

    observe(element: Element): void {
        this.watched.add(element);
    }

    unobserve(element: Element): void {
        this.watched.delete(element);
    }

    disconnect(): void {
        this.watched.clear();
    }

    takeRecords(): IntersectionObserverEntry[] {
        return [];
    }

    /** Tells every observer watching the element whether it is on screen. */
    static report(element: Element, isIntersecting: boolean, top = isIntersecting ? 100 : -200): void {
        for (const observer of FakeIntersectionObserver.all) {
            if (!observer.watched.has(element)) {
                continue;
            }

            const rect = { top, bottom: top + 20, left: 0, right: 100, width: 100, height: 20, x: 0, y: top, toJSON: () => ({}) } as DOMRectReadOnly;
            const rootBounds = { top: 0, bottom: 500, left: 0, right: 100, width: 100, height: 500, x: 0, y: 0, toJSON: () => ({}) } as DOMRectReadOnly;
            const entry = {
                target: element,
                isIntersecting,
                intersectionRatio: isIntersecting ? 1 : 0,
                boundingClientRect: rect,
                intersectionRect: rect,
                rootBounds,
                time: 0
            } as IntersectionObserverEntry;
            observer.callback([entry], observer as unknown as IntersectionObserver);
        }
    }
}

/**
 * Gives the feed's scroller the sizes a browser would, and puts the reader at a scroll position.
 * The first time, the scroller is laid out where the feed scrolled itself as it mounted, at its
 * newest message, which the browser reports with a scroll before the reader moves from there.
 */
function placeScroll(wrapper: VueWrapper, scrollTop: number, scrollHeight = 2000, clientHeight = 500): void {
    const scroller = wrapper.get("[data-testid='message-feed']").element as HTMLElement;
    const isFirstLayout = !Object.getOwnPropertyDescriptor(scroller, "scrollHeight");
    let top = isFirstLayout ? scrollHeight - clientHeight : scroller.scrollTop;
    Object.defineProperty(scroller, "scrollHeight", { configurable: true, get: () => scrollHeight });
    Object.defineProperty(scroller, "clientHeight", { configurable: true, get: () => clientHeight });
    Object.defineProperty(scroller, "scrollTop", { configurable: true, get: () => top, set: (value: number) => top = value });
    if (isFirstLayout) {
        scroller.dispatchEvent(new Event("scroll"));
    }
    top = scrollTop;
    scroller.dispatchEvent(new Event("scroll"));
}

// #endregion Browser stand-ins

/** Monday, October 12, 2026, 3 PM, in local time. */
const now = new Date(2026, 9, 12, 15, 0);

function at(hour: number, minute: number, daysBefore = 0): string {
    return new Date(2026, 9, 12 - daysBefore, hour, minute).toISOString();
}

/** A stored message, in the shape chat_get_history returns. */
function message(id: number, sender: string, createdAt: string, extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return {
        id,
        person_alias_guid: sender,
        sender_nick_name: sender === anna ? "Anna" : "Jordan",
        sender_last_name: sender === anna ? "Reyes" : "Lee",
        sender_listed: true,
        message_type: "text",
        body: `m${id}`,
        pieces: null,
        blocks: null,
        created_at: createdAt,
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        ...extra
    };
}

function timeline(messages: TimelineMessage[], extra: Partial<TimelineState> = {}): TimelineState {
    return reactive({
        messages,
        isLoaded: true,
        hasMore: false,
        readCursor: null,
        unreadCount: 0,
        firstUnreadId: null,
        isBehind: false,
        verifiedThrough: messages.length ? messages[messages.length - 1].id : null,
        ...extra
    }) as TimelineState;
}

function pendingRow(localId: string, status: "sending" | "failed" = "sending"): PendingMessage {
    return { localId, channelId: "c1", body: `pending ${localId}`, status, errorCode: status === "failed" ? "network" : null };
}

type FeedProps = {
    timeline: TimelineState;
    pending?: PendingMessage[];
    isChannel?: boolean;
    isDirectMessage?: boolean;
    isEntering?: boolean;
    isCovered?: boolean;
};

function draw(props: FeedProps, reveal?: { isMainRevealed: boolean }): VueWrapper {
    const provide = reveal ? { [pageRevealKey as symbol]: { state: reveal, holdFace: () => undefined } } : {};
    return mount(MessageFeed, { props: { pending: [], personAliasGuid: me, ...props }, global: { provide } });
}

/**
 * Whether an element is drawn and shown. The unread strip and Jump to latest stay drawn while they
 * fade out, so they say they are out of sight by being hidden from assistive technology.
 */
function isShown(wrapper: VueWrapper, selector: string): boolean {
    const found = wrapper.find(selector);
    return found.exists() && (found.element as HTMLElement).style.display !== "none" && found.attributes("aria-hidden") !== "true";
}

/** Whether a comes before b in the document. */
function isBefore(a: Element, b: Element): boolean {
    return (a.compareDocumentPosition(b) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0;
}

/** Text as a person reads it; some ICU builds put a narrow no-break space before AM and PM. */
function plain(text: string): string {
    return text.replace(/\s+/g, " ").trim();
}

beforeEach(() => {
    jest.useFakeTimers({ now, doNotFake: ["queueMicrotask", "nextTick"] });
    FakeIntersectionObserver.all = [];
    (window as unknown as { IntersectionObserver: unknown }).IntersectionObserver = FakeIntersectionObserver;
});

afterEach(() => {
    jest.useRealTimers();
});

describe("the timeline", () => {
    test("every message, pending and failed sends included, is drawn by the message row", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50)), message(2, anna, at(14, 51))]), pending: [pendingRow("a"), pendingRow("b", "failed")] });

        expect(wrapper.findAllComponents(MessageRow)).toHaveLength(4);
    });

    test("a send says Sending… only after a quiet second, and a failure still says Not sent", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), pending: [pendingRow("a")] });

        expect(wrapper.text()).not.toContain("Sending");
        jest.advanceTimersByTime(999);
        await wrapper.vm.$nextTick();
        expect(wrapper.text()).not.toContain("Sending");
        jest.advanceTimersByTime(1);
        await wrapper.vm.$nextTick();
        expect(wrapper.text()).toContain("Sending…");

        await wrapper.setProps({ pending: [pendingRow("a", "failed")] });
        expect(wrapper.text()).not.toContain("Sending");
        expect(wrapper.text()).toContain("Not sent");
    });

    test("a reply still sending is drawn here only when it is also sent to the conversation", () => {
        const threadOnly = { ...pendingRow("thread-only"), parentId: 1, isShownInChannel: false };
        const alsoHere = { ...pendingRow("also-here"), parentId: 1, isShownInChannel: true };
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), pending: [pendingRow("a"), threadOnly, alsoHere] });

        const drawn = wrapper.findAllComponents(MessageRow).map(row => (row.props("pending") as PendingMessage | null)?.localId ?? null);
        expect(drawn).toEqual([null, "a", "also-here"]);
    });

    test("pending and failed sends sit in the timeline after the newest stored message, in today's section", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50)), message(2, anna, at(14, 51))]), pending: [pendingRow("a"), pendingRow("b", "failed")] });

        const stored = wrapper.findAll("[data-testid='message-row']").map(row => row.element);
        const pending = wrapper.findAll("[data-testid='pending-row']").map(row => row.element);
        expect(pending.map(row => row.getAttribute("data-status"))).toEqual(["sending", "failed"]);
        expect(isBefore(stored[1], pending[0])).toBe(true);

        const sections = wrapper.findAll("[data-testid='day-section']");
        expect(sections[sections.length - 1].element.contains(pending[0])).toBe(true);
        expect(sections[sections.length - 1].element.contains(pending[1])).toBe(true);
    });

    // What to do about a failed send is the strip's, over the composer; the feed only marks it.
    test("a failed send is marked Not sent, with nothing on the row to press", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), pending: [pendingRow("b", "failed")] });
        const row = wrapper.get("[data-testid='pending-row']");

        expect(row.get("[data-testid='message-state']").text()).toBe("Not sent");
        expect(row.findAll("button").filter(button => /Try again|Delete/.test(button.text()))).toHaveLength(0);
    });

    test("each stored row still carries its message id, which the read position is saved from", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]) });

        expect(wrapper.find("[data-message-id='1']").exists()).toBe(true);
    });

    test("a channel's kind reaches every row, and the feed takes Community's look from it", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), isChannel: true });

        expect(wrapper.findComponent(MessageRow).props("isChannel")).toBe(true);
        expect(wrapper.get(".chat-feed-frame").classes()).toContain("is-community");
    });

    test("a direct message reaches every row, and the feed keeps Conversational's look", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), isDirectMessage: true });

        const row = wrapper.findComponent(MessageRow);
        expect(row.props("isChannel")).toBe(false);
        expect(row.props("isDirectMessage")).toBe(true);
        expect(wrapper.get(".chat-feed-frame").classes()).not.toContain("is-community");
    });

    test("a send confirmed while the feed is open shows Sent for 2.2 seconds, then the time alone", async () => {
        const state = timeline([message(1, anna, at(14, 50))]);
        const wrapper = draw({ timeline: state, pending: [pendingRow("a")] });

        // The confirmed message goes in before the pending row comes out, naming the message it
        // became, as the send does it.
        state.messages.push(message(2, me, at(15, 0), { sender_nick_name: undefined, sender_last_name: undefined, body: "pending a", client_key: "a" }));
        (wrapper.props("pending") as PendingMessage[])[0].confirmedId = 2;
        await wrapper.setProps({ pending: [] });
        await nextTick();

        const sent = (): boolean => wrapper.find("[data-message-id='2'] [data-testid='message-state']").exists()
            && wrapper.get("[data-message-id='2'] [data-testid='message-state']").text() === "Sent";
        expect(sent()).toBe(true);

        jest.advanceTimersByTime(2199);
        await nextTick();
        expect(sent()).toBe(true);

        jest.advanceTimersByTime(1);
        await nextTick();
        expect(sent()).toBe(false);
    });

    test("an own message already there when the feed opens never shows Sent", () => {
        const wrapper = draw({ timeline: timeline([message(1, me, at(14, 59))]) });

        expect(wrapper.find("[data-testid='message-state']").exists()).toBe(false);
    });
});

// A person's own message is one thing on the page from the moment it is sent: the row still sending
// and every copy of the message share the send's key, and the feed keys its rows by it. A message
// newer than the feed's first page rises in as it is drawn, unless this session sent it, since its
// pending row already rose; a copy drawn afresh in another group does not rise a second time.
// A conversation opened in place of another enters, as the switch's one motion; the first one
// the block draws is simply there when its page is already held. Decided as the feed mounts and
// never again, so nothing a redraw changes can replay it. A feed that mounts before its first
// page has landed enters as that page is drawn instead, so the rise plays on its content rather
// than over an empty box, on a switch and on the page's first load alike.
describe("the feed's entrance", () => {
    test("a feed opened in place of another before its page has landed enters as the page is drawn", async () => {
        const state = timeline([], { isLoaded: false });
        const wrapper = draw({ timeline: state, isEntering: true });

        expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");

        state.messages.push(message(1, anna, at(14, 50)));
        state.isLoaded = true;
        await nextTick();

        expect(wrapper.get("[data-testid='message-feed']").classes()).toContain("is-entering");
    });

    test("the first feed the block draws enters as its first page is drawn, when it mounted before the page", async () => {
        const state = timeline([], { isLoaded: false });
        const wrapper = draw({ timeline: state });

        expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");

        state.messages.push(message(1, anna, at(14, 50)));
        state.isLoaded = true;
        await nextTick();

        expect(wrapper.get("[data-testid='message-feed']").classes()).toContain("is-entering");
    });

    // The page's reveal fades the column in over the veil; a feed that rose under it would rise
    // again from nothing when the reveal ended, so the first load's entrance is the reveal's moment.
    test("a feed on the first load holds its entrance until the page reveals the column", async () => {
        const state = timeline([], { isLoaded: false });
        const reveal = reactive({ isMainRevealed: false });
        const wrapper = draw({ timeline: state }, reveal);

        state.messages.push(message(1, anna, at(14, 50)));
        state.isLoaded = true;
        await nextTick();
        expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");

        reveal.isMainRevealed = true;
        await nextTick();
        expect(wrapper.get("[data-testid='message-feed']").classes()).toContain("is-entering");
    });

    test("a feed whose first page lands after the page revealed the column enters as the page is drawn", async () => {
        const state = timeline([], { isLoaded: false });
        const reveal = reactive({ isMainRevealed: true });
        const wrapper = draw({ timeline: state }, reveal);

        state.messages.push(message(1, anna, at(14, 50)));
        state.isLoaded = true;
        await nextTick();

        expect(wrapper.get("[data-testid='message-feed']").classes()).toContain("is-entering");
    });

    test("a revealed column does not replay a feed's entrance that already played", async () => {
        const state = timeline([], { isLoaded: false });
        const reveal = reactive({ isMainRevealed: true });
        const wrapper = draw({ timeline: state }, reveal);
        state.isLoaded = true;
        await nextTick();

        const classes: boolean[] = [];
        const element = wrapper.get("[data-testid='message-feed']").element;
        new MutationObserver(() => classes.push(element.classList.contains("is-entering"))).observe(element, { attributes: true, attributeFilter: ["class"] });
        reveal.isMainRevealed = false;
        reveal.isMainRevealed = true;
        await nextTick();

        expect(classes).not.toContain(false);
    });

    test("a feed mounted with its page never enters later, even if its timeline reports loaded again", async () => {
        const state = timeline([message(1, anna, at(14, 50))]);
        const wrapper = draw({ timeline: state });

        state.isLoaded = false;
        await nextTick();
        state.isLoaded = true;
        await nextTick();

        expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");
    });

    test("a feed told it replaces another conversation enters, and keeps that once drawn", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), isEntering: true });

        expect(wrapper.get("[data-testid='message-feed']").classes()).toContain("is-entering");

        await wrapper.setProps({ isEntering: false });
        expect(wrapper.get("[data-testid='message-feed']").classes()).toContain("is-entering");
    });

    test("a feed drawn first does not enter, whatever it is told later", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]) });

        expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");

        await wrapper.setProps({ isEntering: true });
        expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");
    });

    // A full view over the conversation leaves the feed laid out, so its place survives; the feed
    // enters again as the view gives it back, the same rise a switch plays.
    describe("when a full view that covered it closes", () => {
        function recordEntrances(wrapper: VueWrapper): { seen: boolean[], stop: () => void } {
            const element = wrapper.get("[data-testid='message-feed']").element;
            const seen: boolean[] = [];
            const observer = new MutationObserver(() => seen.push(element.classList.contains("is-entering")));
            observer.observe(element, { attributes: true, attributeFilter: ["class"] });
            return { seen, stop: () => observer.disconnect() };
        }

        test("a feed that had not entered enters", async () => {
            const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), isCovered: true });
            const feed = wrapper.get("[data-testid='message-feed']");
            expect(feed.classes()).not.toContain("is-entering");

            await wrapper.setProps({ isCovered: false });
            await flushPromises();

            expect(feed.classes()).toContain("is-entering");
        });

        test("a feed that had entered starts its entrance over", async () => {
            const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), isEntering: true, isCovered: true });
            const { seen, stop } = recordEntrances(wrapper);

            await wrapper.setProps({ isCovered: false });
            await flushPromises();
            stop();

            // The class leaves and returns, which is what restarts a CSS animation.
            expect(seen).toEqual([false, true]);
        });

        test("opening the view does not play it", async () => {
            const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]) });

            await wrapper.setProps({ isCovered: true });
            await flushPromises();

            expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");
        });

        test("a feed whose page has not landed waits for the page, as every entrance does", async () => {
            const state = timeline([], { isLoaded: false });
            const wrapper = draw({ timeline: state, isCovered: true });

            await wrapper.setProps({ isCovered: false });
            await flushPromises();

            expect(wrapper.get("[data-testid='message-feed']").classes()).not.toContain("is-entering");
        });
    });
});

describe("the person's own message is one row from send to stored", () => {
    test("a copy that arrives while the row is still out takes the row's place, in the same element", async () => {
        const state = timeline([message(1, anna, at(14, 50))]);
        const wrapper = draw({ timeline: state, pending: [pendingRow("k1")] });
        await nextTick();
        const drawn = wrapper.get("[data-testid='pending-row']").element;

        state.messages.push(message(2, me, at(15, 0), { client_key: "k1", body: "pending k1" }));
        await nextTick();

        expect(wrapper.find("[data-testid='pending-row']").exists()).toBe(false);
        expect(wrapper.findAll(".chat-msg")).toHaveLength(2);
        expect(wrapper.get("[data-message-id='2']").element).toBe(drawn);
    });

    // Anna's message takes the next id while the send is out, so the row moves out of the
    // person's group to below hers, and the stored copy then moves back above it: each time into
    // another group, which is a new element, and neither rises a second time.
    test("moved between groups by another person's newer message, the person's own send rises only once", async () => {
        const state = timeline([message(1, me, at(14, 58))]);
        const wrapper = mount(MessageFeed, { props: { timeline: state, pending: [pendingRow("k1")], personAliasGuid: me, sentKeys: new Set(["k1"]) } });
        await nextTick();
        expect(wrapper.get("[data-testid='pending-row']").classes()).toContain("is-arriving");

        state.messages.push(message(3, anna, at(15, 0)));
        await nextTick();
        expect(wrapper.get("[data-testid='pending-row']").classes()).not.toContain("is-arriving");
        expect(wrapper.get("[data-message-id='3']").classes()).toContain("is-arriving");

        state.messages.splice(1, 0, message(2, me, at(15, 0), { client_key: "k1", body: "pending k1" }));
        await wrapper.setProps({ pending: [] });
        await nextTick();

        expect(wrapper.findAll(".chat-msg")).toHaveLength(3);
        expect(wrapper.get("[data-message-id='2']").classes()).not.toContain("is-arriving");
    });

    test("the person's message sent from another tab is news here, and rises in", async () => {
        const state = timeline([message(1, anna, at(14, 50))]);
        const wrapper = mount(MessageFeed, { props: { timeline: state, pending: [], personAliasGuid: me, sentKeys: new Set(["k1"]) } });
        await nextTick();

        state.messages.push(message(2, me, at(15, 0), { client_key: "from-another-tab" }));
        await nextTick();

        expect(wrapper.get("[data-message-id='2']").classes()).toContain("is-arriving");
    });
});

// The send's answer and the message's live copy race each other to the page, and either may never
// come. The person's own message shows Sent for 2.2 seconds from the first copy that lands, which
// is when its row goes, and a later copy neither restarts nor cancels it. Every order is driven by
// hand: the answer is a deferred the test resolves, the live copy is applied to the timelines as
// the shell applies a live event, and the timelines tell the sender what lands, as the shell does.
describe("the Sent hold, from whichever copy of the message comes first", () => {
    type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

    function deferred<T>(): Deferred<T> {
        let resolve!: (value: T) => void;
        const promise = new Promise<T>(r => resolve = r);
        return { promise, resolve };
    }

    type Race = { wrapper: VueWrapper, answer: Deferred<SendResult>, send: () => Promise<boolean>, liveCopy: (fields?: Record<string, unknown>) => void, other: (id: number) => void };

    /** The feed fed by a real sender and real timelines, whose send answers when the test says so. */
    async function race(isChannel = false, isPersonLeader = false): Promise<Race> {
        const answer = deferred<SendResult>();
        let sender: ReturnType<typeof createSender> | null = null;
        const timelines = createTimelines({
            limits: () => ({ history_page_size: 50 }),
            fetchPage: async () => ({ messages: [message(1, anna, at(14, 50))], read_cursor: null, unread_count: 0, has_more: false }),
            onLanded: m => sender?.landed(m)
        });
        await timelines.loadNewest("c1");
        sender = createSender({
            send: () => answer.promise,
            timelines,
            personAliasGuid: me,
            newLocalId: () => "local-1"
        });
        const created = (id: number, sender: string, key: string | null): Record<string, unknown> => ({
            id, channel_id: "c1", parent_id: null, shown_in_channel: false, person_alias_guid: sender,
            message_type: "text", body: id === 2 ? "On my way" : `m${id}`, created_at: at(15, 0), client_key: key
        });
        const Host = defineComponent({
            setup: () => () => h(MessageFeed, { timeline: timelines.state("c1"), pending: sender?.pending("c1") ?? [], personAliasGuid: me, isChannel, isPersonLeader, sentKeys: sender?.sentKeys })
        });
        const wrapper = mount(Host);

        return {
            wrapper,
            answer,
            send: () => (sender as ReturnType<typeof createSender>).send("c1", "On my way"),
            liveCopy: (fields = {}) => timelines.applyEvent("c1", "message.created", { ...created(2, me, "local-1"), ...fields }),
            other: (id: number) => timelines.applyEvent("c1", "message.created", created(id, anna, null))
        };
    }

    const confirmedAnswer: SendResult = { ok: true, id: 2, createdAt: at(15, 0) };

    async function flush(): Promise<void> {
        for (let i = 0; i < 5; i++) {
            await Promise.resolve();
            await nextTick();
        }
    }

    function isSent(wrapper: VueWrapper): boolean {
        const state = wrapper.find("[data-message-id='2'] [data-testid='message-state']");
        return state.exists() && state.text() === "Sent";
    }

    test("the answer first: Sent from the answer, and the live copy after it changes nothing", async () => {
        const r = await race();
        const sending = r.send();
        await flush();

        r.answer.resolve(confirmedAnswer);
        await sending;
        await flush();
        expect(isSent(r.wrapper)).toBe(true);

        jest.advanceTimersByTime(400);
        r.liveCopy();
        await flush();
        expect(isSent(r.wrapper)).toBe(true);

        jest.advanceTimersByTime(1799);
        await flush();
        expect(isSent(r.wrapper)).toBe(true);

        jest.advanceTimersByTime(1);
        await flush();
        expect(isSent(r.wrapper)).toBe(false);
    });

    test("the live copy first: Sent from the live copy, the answer after it not restarting it", async () => {
        const r = await race();
        const sending = r.send();
        await flush();

        r.liveCopy();
        await flush();
        expect(r.wrapper.find("[data-testid='pending-row']").exists()).toBe(false);
        expect(isSent(r.wrapper)).toBe(true);
        jest.advanceTimersByTime(400);

        r.answer.resolve(confirmedAnswer);
        await sending;
        await flush();
        expect(isSent(r.wrapper)).toBe(true);

        jest.advanceTimersByTime(1799);
        await flush();
        expect(isSent(r.wrapper)).toBe(true);

        jest.advanceTimersByTime(1);
        await flush();
        expect(isSent(r.wrapper)).toBe(false);
    });

    test("an answer that never comes: the live copy alone shows Sent for 2.2 seconds, then the time", async () => {
        const r = await race();
        void r.send();
        await flush();

        r.liveCopy();
        await flush();
        expect(isSent(r.wrapper)).toBe(true);

        jest.advanceTimersByTime(2199);
        await flush();
        expect(isSent(r.wrapper)).toBe(true);

        jest.advanceTimersByTime(1);
        await flush();
        expect(isSent(r.wrapper)).toBe(false);
    });

    // Anna's message takes the next id while the send is out, so the person's message is drawn
    // above hers in a group of its own once a copy of it lands: one row, which does not rise a
    // second time and still says Sent.
    test.each([["a direct message", false], ["a channel", true]] as const)("in %s, another person's newer message before the answer: one row, no second rise, Sent still shown", async (_kind, isChannel) => {
        const r = await race(isChannel);
        const sending = r.send();
        await flush();
        const drawn = r.wrapper.get("[data-testid='pending-row']").element;

        r.other(3);
        await flush();
        r.answer.resolve(confirmedAnswer);
        await sending;
        await flush();

        // Either the row's own element, moved with its group, or a new one that does not rise.
        const settled = r.wrapper.get("[data-message-id='2']").element;
        expect(settled === drawn || !settled.classList.contains("is-arriving")).toBe(true);
        expect(r.wrapper.findAll(".chat-msg")).toHaveLength(3);
        expect(r.wrapper.find("[data-testid='pending-row']").exists()).toBe(false);
        expect(isSent(r.wrapper)).toBe(true);

        r.liveCopy();
        await flush();
        expect(r.wrapper.findAll(".chat-msg")).toHaveLength(3);
    });

    // The row a send is drawn as is the element its message settles in: a new element would cut
    // the rise short at the answer, and a class that changed with Sent would play it again after.
    test.each([["a direct message", false], ["a channel", true]] as const)("in %s, the sending row becomes the sent message in place and rises once", async (_kind, isChannel) => {
        const r = await race(isChannel);
        const sending = r.send();
        await flush();
        const drawn = r.wrapper.get("[data-testid='pending-row']").element;
        expect(drawn.classList.contains("is-arriving")).toBe(true);

        r.answer.resolve(confirmedAnswer);
        await sending;
        await flush();
        expect(r.wrapper.get("[data-message-id='2']").element).toBe(drawn);
        expect(isSent(r.wrapper)).toBe(true);
        expect(drawn.classList.contains("is-arriving")).toBe(true);
        expect(r.wrapper.findAll(".chat-msg")).toHaveLength(2);

        jest.advanceTimersByTime(2200);
        await flush();
        expect(isSent(r.wrapper)).toBe(false);
        expect(r.wrapper.get("[data-message-id='2']").element).toBe(drawn);
        expect(drawn.classList.contains("is-arriving")).toBe(true);
    });

    // A leader's own send carries the Leader pill from its first frame to its last, one element
    // throughout, though only the live copy says who leads and, in a room that hides its roster,
    // says no for everyone.
    test("a leader's own send in a channel keeps one Leader pill from sending to the live copy", async () => {
        const r = await race(true, true);
        const sending = r.send();
        await flush();
        const pill = r.wrapper.get("[data-testid='pending-row'] [data-testid='leader-pill']").element;

        r.answer.resolve(confirmedAnswer);
        await sending;
        await flush();
        expect(r.wrapper.get("[data-message-id='2'] [data-testid='leader-pill']").element).toBe(pill);

        r.liveCopy({ sender_is_leader: false, sender_listed: true });
        await flush();
        expect(r.wrapper.get("[data-message-id='2'] [data-testid='leader-pill']").element).toBe(pill);
    });
});

describe("day sections", () => {
    test("each day is its own section with its own pill", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });

        const sections = wrapper.findAll("[data-testid='day-section']");
        expect(sections).toHaveLength(2);
        expect(sections.map(section => section.get("[data-testid='day-pill']").text())).toEqual(["Yesterday", "Today"]);
    });

    /** Puts an element at a height on screen, as a browser lays it out. */
    function placeAt(element: Element, top: number): void {
        element.getBoundingClientRect = () => ({ top, bottom: top + 26, left: 0, right: 300, width: 300, height: 26, x: 0, y: top, toJSON: () => ({}) }) as DOMRect;
    }

    /**
     * Lays the label's box out at a height in the frame, which a transform never moves. The feed
     * stands at the frame's top in these tests, so the frame's heights are the feed's too.
     */
    function layOut(element: Element, top: number, height: number): void {
        Object.defineProperty(element, "offsetTop", { configurable: true, get: () => top });
        Object.defineProperty(element, "offsetHeight", { configurable: true, get: () => height });
    }

    // A label stuck inside the feed sits on whatever text scrolls under it, since the feed's fade
    // cannot hide that text without hiding the label too. The day of what is at the feed's top is
    // drawn over the feed instead, above the band the fade hides.
    test("the day the reader has scrolled into is labelled above the feed, not by a pill stuck inside it", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });
        const float = wrapper.get("[data-testid='day-float']");
        const pills = wrapper.findAll("[data-testid='day-pill']").map(pill => pill.element);
        const scroller = wrapper.get("[data-testid='message-feed']").element;

        expect(scroller.contains(float.element)).toBe(false);
        expect(float.attributes("aria-hidden")).toBe("true");

        // At the top, every day's own pill is in view below the label's place, so none is labelled.
        layOut(float.element, 108, 26);
        placeAt(pills[0], 160);
        placeAt(pills[1], 400);
        placeScroll(wrapper, 0);
        await nextTick();
        expect(float.classes()).not.toContain("is-on");

        // Yesterday's pill has gone up under the label's place.
        placeAt(pills[0], 108);
        placeScroll(wrapper, 52);
        await nextTick();
        expect(float.classes()).toContain("is-on");
        expect(float.text()).toBe("Yesterday");

        placeAt(pills[0], -200);
        placeAt(pills[1], 100);
        placeScroll(wrapper, 360);
        await nextTick();
        expect(float.text()).toBe("Today");
    });

    test("no day pill sticks inside the feed, and the feed's fade hides all of the band the label sits over", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageFeed.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        const pill = /\.chat-day-pill\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";
        const float = /\.chat-day-float\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";
        const mask = /\.chat-feed-skeleton\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";

        expect(pill).not.toMatch(/position:\s*sticky/);
        expect(float).toMatch(/top:\s*var\(--chat-day-top\)/);
        // Clear through the label's bottom edge, so no text shows beside or under it.
        expect(mask).toMatch(/mask-image:\s*linear-gradient\(to bottom,\s*transparent 0,\s*transparent var\(--chat-day-bottom\)/);
    });

    /**
     * The label's box is the band over the feed's top: the label's own height and the fade under
     * it, here from 108 to 146.
     */
    function placeBand(element: Element, top: number): void {
        layOut(element, top, 38);
    }

    /** Draws two days with their pills at the given heights on screen, the label's band at 108. */
    async function twoDaysAt(wrapper: VueWrapper, yesterday: number, today: number): Promise<void> {
        const pills = wrapper.findAll("[data-testid='day-pill']").map(pill => pill.element);
        placeBand(wrapper.get("[data-testid='day-float']").element, 108);
        placeAt(pills[0], yesterday);
        placeAt(pills[1], today);
        placeScroll(wrapper, 400);
        await nextTick();
    }

    function shown(wrapper: VueWrapper): { label: string | null, offset: string | undefined, carried: boolean[] } {
        const float = wrapper.get("[data-testid='day-float']");
        return {
            label: float.classes().includes("is-on") ? float.text() : null,
            offset: (float.element as HTMLElement).style.getPropertyValue("--chat-day-offset"),
            carried: wrapper.findAll("[data-testid='day-pill']").map(pill => pill.classes().includes("is-carried"))
        };
    }

    // The bug: a day's pill rising into the band showed its lower part faded in under the label,
    // which still said the day before. The pill that enters the band is carried by the label from
    // there: the label takes its day and stands where the pill is, the pill itself is not drawn,
    // and the label parks once it reaches its place. So the band only ever shows one pill.
    test("a day's pill that reaches the band is carried up by the label, and is never drawn under it", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });

        // Today's pill is below the band, solid in the feed; the label says the day above it.
        await twoDaysAt(wrapper, -200, 160);
        expect(shown(wrapper)).toEqual({ label: "Yesterday", offset: "0px", carried: [true, false] });

        // Its top crosses into the fade: the label says Today where the pill is, and only once.
        await twoDaysAt(wrapper, -220, 140);
        expect(shown(wrapper)).toEqual({ label: "Today", offset: "32px", carried: [false, true] });

        await twoDaysAt(wrapper, -240, 120);
        expect(shown(wrapper)).toEqual({ label: "Today", offset: "12px", carried: [false, true] });

        // At the label's place and past it, the label stays parked there.
        await twoDaysAt(wrapper, -252, 108);
        expect(shown(wrapper)).toEqual({ label: "Today", offset: "0px", carried: [false, true] });

        await twoDaysAt(wrapper, -500, -140);
        expect(shown(wrapper)).toEqual({ label: "Today", offset: "0px", carried: [false, true] });
    });

    test("scrolling back down the band, the label lets the pill go as it leaves the band, in one frame, however far it jumped", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });

        await twoDaysAt(wrapper, -240, 120);
        expect(shown(wrapper).label).toBe("Today");

        // A fling or a jump moves the pill straight out of the band below it.
        await twoDaysAt(wrapper, -100, 260);
        expect(shown(wrapper)).toEqual({ label: "Yesterday", offset: "0px", carried: [true, false] });

        // A jump the other way lands with Today's pill far above; nothing is left between.
        await twoDaysAt(wrapper, -900, -500);
        expect(shown(wrapper)).toEqual({ label: "Today", offset: "0px", carried: [false, true] });
    });

    // The feed rises 6px into place as a conversation opens, and the landing scroll runs while it
    // does. A pill is placed by where the feed lays it out, so the label carries the same pill the
    // same distance during the rise as after it.
    test("a feed still rising in carries the pill by where it is laid out, not where the rise has it", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });
        const scroller = wrapper.get("[data-testid='message-feed']").element;

        // Laid out, Today's pill stands at 140, 32px into the band; the rise draws everything 6px lower.
        placeAt(scroller, 6);
        await twoDaysAt(wrapper, -214, 146);
        expect(shown(wrapper)).toEqual({ label: "Today", offset: "32px", carried: [false, true] });

        // Laid out at 143, 3px above the band's foot: the rise would put it past the foot, uncarried.
        await twoDaysAt(wrapper, -211, 149);
        expect(shown(wrapper)).toEqual({ label: "Today", offset: "35px", carried: [false, true] });
    });

    // A reflow moves the pills with no scroll: the column resized, a panel opened beside it.
    test("the label is placed again when the feed changes size, without a scroll", async () => {
        const watching: Array<{ callback: ResizeObserverCallback, targets: Element[] }> = [];
        const previous = window.ResizeObserver;
        window.ResizeObserver = class {
            private readonly entry: { callback: ResizeObserverCallback, targets: Element[] };
            constructor(callback: ResizeObserverCallback) {
                this.entry = { callback, targets: [] };
                watching.push(this.entry);
            }
            observe(target: Element): void {
                this.entry.targets.push(target);
            }
            unobserve(): void { /* Nothing is watched apart. */ }
            disconnect(): void {
                this.entry.targets = [];
            }
        } as unknown as typeof ResizeObserver;

        try {
            const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });
            const scroller = wrapper.get("[data-testid='message-feed']").element;
            await twoDaysAt(wrapper, -200, 160);
            expect(shown(wrapper).label).toBe("Yesterday");

            // Narrower, the text above wraps longer and Today's pill rises into the band.
            const pills = wrapper.findAll("[data-testid='day-pill']").map(pill => pill.element);
            placeAt(pills[0], -260);
            placeAt(pills[1], 120);
            for (const observer of watching.filter(o => o.targets.includes(scroller))) {
                observer.callback([], observer as unknown as ResizeObserver);
            }
            await nextTick();

            expect(shown(wrapper)).toEqual({ label: "Today", offset: "12px", carried: [false, true] });
        }
        finally {
            window.ResizeObserver = previous;
        }
    });

    test("at the top of history every pill is drawn where its day starts, and nothing is labelled", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });

        // The first pill starts below the band, as the feed's top room puts it.
        await twoDaysAt(wrapper, 160, 400);
        expect(shown(wrapper)).toEqual({ label: null, offset: "0px", carried: [false, false] });

        // A pixel into the band, the label already carries it, so the pill never fades under it.
        await twoDaysAt(wrapper, 145, 385);
        expect(shown(wrapper)).toEqual({ label: "Yesterday", offset: "37px", carried: [true, false] });
    });

    test("the label's box spans the band, it moves only by the carried pill's offset, and the carried pill stays readable to assistive technology", () => {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageFeed.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        const float = /\.chat-day-float\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";
        const floatLabel = /\.chat-day-float span\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";
        const carried = /\.chat-day-pill\.is-carried\s*\{([^}]*)\}/.exec(style)?.[1] ?? "";

        expect(float).toMatch(/height:\s*calc\(var\(--chat-day-height\) \+ var\(--chat-day-fade\)\)/);
        expect(float).toMatch(/align-items:\s*flex-start/);
        expect(floatLabel).toMatch(/transform:\s*translateY\(var\(--chat-day-offset\)\)/);
        // Hidden by opacity, not visibility, so the pill still names its day to a screen reader.
        expect(carried).toMatch(/opacity:\s*0/);
        expect(carried).not.toMatch(/visibility/);
    });
});

describe("the New divider and the unread strip", () => {
    function unread(): VueWrapper {
        return draw({
            timeline: timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 7, firstUnreadId: 2 })
        });
    }

    test("the divider reads New and sits above the first unread message the platform found", () => {
        const wrapper = unread();

        const divider = wrapper.get("[data-testid='unread-divider']");
        expect(divider.text()).toBe("New");
        expect(isBefore(wrapper.get("[data-message-id='1']").element, divider.element)).toBe(true);
        expect(isBefore(divider.element, wrapper.get("[data-message-id='2']").element)).toBe(true);
    });

    // The strip lies over the feed's top, so while it may show the feed starts its content below
    // it: scrolled to the very top, the first day and message are under neither the strip nor the
    // band the label keeps. The room stays while the strip comes and goes, so nothing moves then.
    test("the feed keeps room for the strip at its top while there is something unread, whether or not the strip shows", async () => {
        const wrapper = unread();
        const frame = wrapper.get(".chat-feed-frame");
        expect(frame.classes()).toContain("has-strip-room");

        FakeIntersectionObserver.report(wrapper.get("[data-testid='unread-divider']").element, false);
        await nextTick();
        expect(frame.classes()).toContain("has-strip-room");

        await wrapper.get("[data-testid='mark-read']").trigger("click");
        expect(frame.classes()).not.toContain("has-strip-room");

        const read = draw({ timeline: timeline([message(1, anna, at(11, 0))], { readCursor: 1, unreadCount: 0, firstUnreadId: null }) });
        expect(read.get(".chat-feed-frame").classes()).not.toContain("has-strip-room");
    });

    test("the divider stands above a post Rock made in the reader's name when the platform says it is the first unread", () => {
        // The reader's own message by its sender, which the feed itself would never call new.
        const wrapper = draw({
            timeline: timeline([message(1, anna, at(11, 0)), message(2, me, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 2, firstUnreadId: 2 })
        });

        const divider = wrapper.get("[data-testid='unread-divider']");
        expect(isBefore(wrapper.get("[data-message-id='1']").element, divider.element)).toBe(true);
        expect(isBefore(divider.element, wrapper.get("[data-message-id='2']").element)).toBe(true);
    });

    test("the divider stands at the page's top when the first unread is older than the page loaded", () => {
        const wrapper = draw({
            timeline: timeline([message(40, anna, at(11, 0)), message(41, anna, at(11, 30))], { readCursor: 3, unreadCount: 99, firstUnreadId: 4 })
        });

        const divider = wrapper.get("[data-testid='unread-divider']");
        expect(isBefore(divider.element, wrapper.get("[data-message-id='40']").element)).toBe(true);
    });

    test("no divider and no strip with nothing unread", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(11, 0))], { readCursor: 1, unreadCount: 0, firstUnreadId: null }) });

        expect(wrapper.find("[data-testid='unread-divider']").exists()).toBe(false);
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(false);
    });

    test("the strip shows only while the divider is off screen", async () => {
        const wrapper = unread();
        const divider = wrapper.get("[data-testid='unread-divider']").element;

        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(false);

        FakeIntersectionObserver.report(divider, false);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(true);

        FakeIntersectionObserver.report(divider, true);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(false);
    });

    // It leaves the way it came, so it stays drawn as it goes, in its off state and out of reach.
    test("the strip stays drawn as it goes, switched off and out of reach rather than removed", async () => {
        const wrapper = unread();
        const divider = wrapper.get("[data-testid='unread-divider']").element;
        FakeIntersectionObserver.report(divider, false);
        await nextTick();
        expect(wrapper.get("[data-testid='unread-strip']").classes()).toContain("is-on");

        FakeIntersectionObserver.report(divider, true);
        await nextTick();
        const strip = wrapper.get("[data-testid='unread-strip']");
        expect((strip.element as HTMLElement).style.display).not.toBe("none");
        expect(strip.classes()).not.toContain("is-on");
        expect(strip.attributes("aria-hidden")).toBe("true");
        expect(strip.attributes("inert")).toBeDefined();
    });

    test("the strip says how many are new since the first of them", async () => {
        const wrapper = unread();
        FakeIntersectionObserver.report(wrapper.get("[data-testid='unread-divider']").element, false);
        await nextTick();

        const strip = plain(wrapper.get("[data-testid='unread-strip']").text());
        expect(strip).toContain("7 new messages");
        expect(strip).toContain("since 11:30 AM");
        expect(wrapper.get("[data-testid='mark-read']").text()).toBe("Mark as read");
    });

    test("Mark as read saves the read position at the newest stored message", async () => {
        const wrapper = draw({
            timeline: timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 2, firstUnreadId: 2 }),
            pending: [pendingRow("a")]
        });
        FakeIntersectionObserver.report(wrapper.get("[data-testid='unread-divider']").element, false);
        await nextTick();

        await wrapper.get("[data-testid='mark-read']").trigger("click");

        expect(wrapper.emitted("markRead")).toEqual([[3]]);
    });

    test("the reader's own send takes the divider and the strip away, and saves no read position", async () => {
        const wrapper = unread();
        FakeIntersectionObserver.report(wrapper.get("[data-testid='unread-divider']").element, false);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(true);

        await wrapper.setProps({ pending: [pendingRow("a")] });
        await nextTick();

        expect(wrapper.find("[data-testid='unread-divider']").exists()).toBe(false);
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(false);
        expect(wrapper.emitted("markRead")).toBeUndefined();
    });

    test("after a send, a catch-up whose divider falls past the newest message at the send brings the divider and the strip back", async () => {
        const state = timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 7, firstUnreadId: 2 });
        const wrapper = draw({ timeline: state });

        await wrapper.setProps({ pending: [pendingRow("a")] });
        await nextTick();
        expect(wrapper.find("[data-testid='unread-divider']").exists()).toBe(false);

        // A reconnect's newest page: the send stored, then Anna wrote after it.
        state.messages.push(message(4, me, at(11, 40)), message(5, anna, at(11, 45)));
        state.readCursor = 4;
        state.unreadCount = 1;
        state.firstUnreadId = 5;
        await nextTick();

        const divider = wrapper.get("[data-testid='unread-divider']");
        expect(isBefore(wrapper.get("[data-message-id='4']").element, divider.element)).toBe(true);
        expect(isBefore(divider.element, wrapper.get("[data-message-id='5']").element)).toBe(true);
        FakeIntersectionObserver.report(divider.element, false);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(true);
    });

    test("after a send, a catch-up whose divider falls at or before the newest message at the send keeps them hidden", async () => {
        const state = timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 7, firstUnreadId: 2 });
        const wrapper = draw({ timeline: state });
        FakeIntersectionObserver.report(wrapper.get("[data-testid='unread-divider']").element, false);
        await nextTick();

        await wrapper.setProps({ pending: [pendingRow("a")] });
        await nextTick();

        // The same read position comes back with the send stored after it, and an answer that
        // still names an unread message from before the send.
        state.messages.push(message(4, me, at(11, 40)));
        state.readCursor = 1;
        state.unreadCount = 3;
        state.firstUnreadId = 2;
        await nextTick();

        expect(wrapper.find("[data-testid='unread-divider']").exists()).toBe(false);
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(false);
    });

    test("a send that only fails still takes them away, since the reader has moved on", async () => {
        const wrapper = unread();

        await wrapper.setProps({ pending: [pendingRow("a", "failed")] });
        await nextTick();

        expect(wrapper.find("[data-testid='unread-divider']").exists()).toBe(false);
    });

    test("the strip says the platform's count, since the first unread it found, with the reader's own messages among them", async () => {
        // The platform's count already leaves out the reader's own and the system line.
        const wrapper = draw({
            timeline: timeline([
                message(1, anna, at(11, 0)),
                message(2, me, at(11, 20)),
                message(3, anna, at(11, 30)),
                message(4, me, at(11, 31)),
                message(5, anna, at(11, 32)),
                message(6, anna, at(11, 33), { message_type: "system" })
            ], { readCursor: 1, unreadCount: 2, firstUnreadId: 3 })
        });
        FakeIntersectionObserver.report(wrapper.get("[data-testid='unread-divider']").element, false);
        await nextTick();

        const strip = plain(wrapper.get("[data-testid='unread-strip']").text());
        expect(strip).toContain("2 new messages");
        expect(strip).toContain("since 11:30 AM");
    });
});

describe("Jump to latest", () => {
    test("is hidden at the bottom and shows once the reader scrolls up", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 0))]) });

        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);

        placeScroll(wrapper, 0);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(true);
        expect(wrapper.get("[data-testid='jump-latest']").text()).toBe("Jump to latest");

        placeScroll(wrapper, 1500);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
    });

    // It leaves the way it came, so it stays drawn as it goes, in its off state and out of reach.
    test("stays drawn as it goes, switched off and out of reach rather than removed", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 0))]) });
        placeScroll(wrapper, 0);
        await nextTick();
        expect(wrapper.get("[data-testid='jump-latest']").classes()).toContain("is-on");

        placeScroll(wrapper, 1500);
        await nextTick();
        const jump = wrapper.get("[data-testid='jump-latest']");
        expect((jump.element as HTMLElement).style.display).not.toBe("none");
        expect(jump.classes()).not.toContain("is-on");
        expect(jump.attributes("aria-hidden")).toBe("true");
        expect(jump.attributes("inert")).toBeDefined();
    });

    test("counts the messages that arrive while the reader is scrolled up", async () => {
        const state = timeline([message(1, anna, at(14, 0))]);
        const wrapper = draw({ timeline: state });
        placeScroll(wrapper, 0);
        await nextTick();

        state.messages.push(message(2, anna, at(14, 1)), message(3, anna, at(14, 2)), message(4, anna, at(14, 3)));
        await nextTick();
        await nextTick();

        expect(wrapper.get("[data-testid='jump-latest']").text()).toBe("3 new messages");
    });

    // One near-bottom rule, the design's 48 px, decides both whether Jump shows and whether a new
    // message is followed, so Jump never shows and then vanishes as the feed snaps down anyway.
    test("a reader 60 px up stays put when another person sends, and Jump counts it", async () => {
        const state = timeline([message(1, anna, at(14, 0))]);
        const wrapper = draw({ timeline: state });
        placeScroll(wrapper, 1440);
        await nextTick();

        state.messages.push(message(2, anna, at(14, 1)));
        await nextTick();
        await nextTick();

        expect((wrapper.get("[data-testid='message-feed']").element as HTMLElement).scrollTop).toBe(1440);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(true);
        expect(wrapper.get("[data-testid='jump-latest']").text()).toBe("1 new message");
    });

    test("a reader 40 px up follows another person's message to the bottom, and Jump stays hidden", async () => {
        const state = timeline([message(1, anna, at(14, 0))]);
        const wrapper = draw({ timeline: state });
        placeScroll(wrapper, 1460);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);

        state.messages.push(message(2, anna, at(14, 1)));
        await nextTick();
        await nextTick();

        expect((wrapper.get("[data-testid='message-feed']").element as HTMLElement).scrollTop).toBe(2000);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
    });

    // A message can land mid-scroll, before the browser has run the scroll handler for where the
    // reader now is, so the count is decided from the feed's position at that moment.
    test("counts a message that lands before the scroll handler has caught up with the reader", async () => {
        const state = timeline([message(1, anna, at(14, 0))]);
        const wrapper = draw({ timeline: state });
        placeScroll(wrapper, 1500);
        await nextTick();

        const scroller = wrapper.get("[data-testid='message-feed']").element as HTMLElement;
        Object.defineProperty(scroller, "scrollTop", { configurable: true, writable: true, value: 1442 });
        state.messages.push(message(2, anna, at(14, 1)));
        await nextTick();
        await nextTick();

        expect(scroller.scrollTop).toBe(1442);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(true);
        expect(wrapper.get("[data-testid='jump-latest']").text()).toBe("1 new message");
    });

    test("from the keyboard, hands focus to the newest message, since the button hides once there", async () => {
        const wrapper = mount(MessageFeed, {
            props: { pending: [], personAliasGuid: me, timeline: timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]) },
            attachTo: document.body
        });
        placeScroll(wrapper, 0);
        await nextTick();

        const jump = wrapper.get("[data-testid='jump-latest']");
        (jump.element as HTMLElement).focus();
        await jump.trigger("click");

        expect(document.activeElement).toBe(wrapper.get("[data-message-id='2']").element);
        wrapper.unmount();
    });
});

describe("a send that fails", () => {
    test("is scrolled into view, out from under the feed's bottom fade", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), pending: [pendingRow("a")] });
        await nextTick();
        // the strip over the composer takes its height from the feed
        placeScroll(wrapper, 1500, 2060);

        await wrapper.setProps({ pending: [pendingRow("a", "failed")] });
        await nextTick();
        await nextTick();

        expect((wrapper.get("[data-testid='message-feed']").element as HTMLElement).scrollTop).toBe(2060);
    });
});

// The composer under the feed grows with its text, its strip of files and its failure strip, and
// takes that height from the feed. A reader at the newest message keeps it in view as the feed
// shrinks; a reader scrolled up is left where they are.
describe("the feed giving height to the composer", () => {
    let resized: Array<{ callback: ResizeObserverCallback, targets: Element[] }> = [];
    let previous: typeof ResizeObserver;

    beforeEach(() => {
        resized = [];
        previous = window.ResizeObserver;
        window.ResizeObserver = class {
            private readonly entry: { callback: ResizeObserverCallback, targets: Element[] };
            constructor(callback: ResizeObserverCallback) {
                this.entry = { callback, targets: [] };
                resized.push(this.entry);
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

    /** Shrinks the feed's box to a new height, as the composer below it growing does, with no scroll. */
    async function shrinkTo(wrapper: VueWrapper, clientHeight: number): Promise<void> {
        const scroller = wrapper.get("[data-testid='message-feed']").element;
        Object.defineProperty(scroller, "clientHeight", { configurable: true, get: () => clientHeight });
        for (const observer of resized.filter(o => o.targets.includes(scroller))) {
            observer.callback([], observer as unknown as ResizeObserver);
        }
        await nextTick();
    }

    test("a reader at the newest message keeps it in view as the composer's strip appears", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]) });
        placeScroll(wrapper, 1500, 2000, 500);
        await nextTick();

        await shrinkTo(wrapper, 430);

        const scroller = wrapper.get("[data-testid='message-feed']").element as HTMLElement;
        expect(scroller.scrollTop).toBe(2000);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
    });

    test("a reader scrolled up is left where they are", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]) });
        placeScroll(wrapper, 800, 2000, 500);
        await nextTick();

        await shrinkTo(wrapper, 430);

        expect((wrapper.get("[data-testid='message-feed']").element as HTMLElement).scrollTop).toBe(800);
    });
});

// A conversation opens at its newest message and a reader there stays there. Whether the reader is
// at the newest message is theirs to change by scrolling, not something to measure again whenever
// the feed happens to change, so rows that grow (a photo, a font, a reaction) and a page that
// replaces the held one at the same count keep a reader at the newest message and leave a reader
// scrolled up alone.
describe("a feed holding its reader at the newest message", () => {
    const channel = "c0000001-0000-4000-8000-000000000000";
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

    /**
     * The scroller's sizes as a browser lays them out, changeable as content settles; a scroll is
     * clamped to them. It is laid out where the feed scrolled itself as it mounted, at its newest
     * message, which the browser reports with a scroll.
     */
    function layOut(wrapper: VueWrapper, sizes: { scrollHeight: number, clientHeight: number, scrollTop: number }): void {
        const scroller = wrapper.get("[data-testid='message-feed']").element as HTMLElement;
        Object.defineProperty(scroller, "scrollHeight", { configurable: true, get: () => sizes.scrollHeight });
        Object.defineProperty(scroller, "clientHeight", { configurable: true, get: () => sizes.clientHeight });
        Object.defineProperty(scroller, "scrollTop", {
            configurable: true,
            get: () => sizes.scrollTop,
            set: (value: number) => sizes.scrollTop = Math.max(0, Math.min(value, sizes.scrollHeight - sizes.clientHeight))
        });
        sizes.scrollTop = Math.max(0, sizes.scrollHeight - sizes.clientHeight);
        scroller.dispatchEvent(new Event("scroll"));
    }

    /**
     * Lets the feed's own smooth scrolls start but not finish, as on the frames after one is asked
     * for: each request is recorded and the scroller stays where it is.
     */
    function holdSmoothScrolls(wrapper: VueWrapper): number[] {
        const scroller = wrapper.get("[data-testid='message-feed']").element as HTMLElement;
        const requested: number[] = [];
        scroller.scrollTo = ((options: ScrollToOptions) => {
            requested.push(options.top ?? 0);
        }) as typeof scroller.scrollTo;
        return requested;
    }

    /** A scroll event with the scroller where it is, as the browser reports a frame after the feed scrolled itself. */
    function echoScroll(wrapper: VueWrapper): void {
        wrapper.get("[data-testid='message-feed']").element.dispatchEvent(new Event("scroll"));
    }

    /** Scrolls the reader to a place, as their own scrolling does. */
    function scrollReader(wrapper: VueWrapper, top: number): void {
        const scroller = wrapper.get("[data-testid='message-feed']").element as HTMLElement;
        scroller.scrollTop = top;
        scroller.dispatchEvent(new Event("scroll"));
    }

    /** Tells every size observer its target changed, as the browser does after layout. */
    async function resize(): Promise<void> {
        for (const observer of observers) {
            observer.callback([], observer as unknown as ResizeObserver);
        }
        await nextTick();
    }

    function page(ids: number[], hasMore: boolean) {
        return {
            messages: [...ids].sort((a, b) => b - a).map(id => message(id, anna, at(14, id % 60))),
            read_cursor: null,
            unread_count: 0,
            first_unread_id: null,
            has_more: hasMore
        };
    }

    test("a held conversation whose newest page replaces it at the same count opens at the newest message, its new rows watched", async () => {
        let answer = page([1, 2, 3], true);
        const timelines = createTimelines({ limits: () => ({ history_page_size: 3 }), fetchPage: async () => answer });
        await timelines.loadNewest(channel);
        timelines.leave(channel);

        const wrapper = draw({ timeline: timelines.state(channel) });
        const sizes = { scrollHeight: 600, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        scrollReader(wrapper, 600);
        expect(sizes.scrollTop).toBe(100);

        // the room was busy, so the newest page replaces the held one, and its rows are taller
        answer = page([20, 21, 22], true);
        await timelines.loadNewest(channel);
        sizes.scrollHeight = 900;
        await nextTick();
        await nextTick();

        const rows = wrapper.findAll("[data-message-id]");
        expect(rows.map(row => row.attributes("data-message-id"))).toEqual(["20", "21", "22"]);
        expect(rows.every(row => row.attributes("data-observed") === "true")).toBe(true);
        expect(sizes.scrollTop).toBe(400);
    });

    test("a row that grows after the feed is drawn keeps a reader at the newest message", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]) });
        const sizes = { scrollHeight: 600, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        scrollReader(wrapper, 600);
        expect(sizes.scrollTop).toBe(100);

        // no new message and the feed's own box unchanged: an image decoding, a font swapping in
        sizes.scrollHeight = 760;
        await resize();

        expect(sizes.scrollTop).toBe(260);
        // the content, not only the box, is what is watched
        const watched = observers.flatMap(observer => observer.targets);
        expect(watched.some(target => (target as HTMLElement).dataset.testid === "feed-content")).toBe(true);
    });

    test("a reader at the newest message whose last row grew still follows the next message, and Jump stays hidden", async () => {
        const state = timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]);
        const wrapper = draw({ timeline: state });
        const sizes = { scrollHeight: 600, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        scrollReader(wrapper, 600);

        // the last row grows well past the near-the-bottom margin with no scroll, then someone sends
        sizes.scrollHeight = 760;
        state.messages.push(message(3, anna, at(14, 2)));
        await nextTick();
        sizes.scrollHeight = 820;
        await nextTick();
        await nextTick();

        expect(sizes.scrollTop).toBe(320);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
    });

    test("a reader scrolled up stays where they are when content grows and when the timeline is replaced", async () => {
        let answer = page([1, 2, 3], true);
        const timelines = createTimelines({ limits: () => ({ history_page_size: 3 }), fetchPage: async () => answer });
        await timelines.loadNewest(channel);

        const wrapper = draw({ timeline: timelines.state(channel) });
        const sizes = { scrollHeight: 2000, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        scrollReader(wrapper, 700);

        sizes.scrollHeight = 2300;
        await resize();
        expect(sizes.scrollTop).toBe(700);

        answer = page([20, 21, 22], true);
        await timelines.loadNewest(channel);
        await nextTick();
        await nextTick();
        expect(sizes.scrollTop).toBe(700);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(true);
    });

    // The browser reports a scroll the feed made a frame later, after the frame's layout. A row
    // that grew in that frame puts the reader more than the margin above the bottom by then, but
    // the reader has not moved, so they are still at the newest message and kept there.
    test("the feed's scroll to follow a message, reported after the next row grew, leaves the reader at the newest message", async () => {
        const state = timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]);
        const wrapper = draw({ timeline: state });
        const sizes = { scrollHeight: 2000, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);

        state.messages.push(message(3, anna, at(14, 2)));
        sizes.scrollHeight = 2100;
        await nextTick();
        await nextTick();
        expect(sizes.scrollTop).toBe(1600);

        sizes.scrollHeight = 2300;
        echoScroll(wrapper);
        await resize();

        expect(sizes.scrollTop).toBe(1800);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
    });

    test("a photo that loads in the frame after the feed was kept at the newest message for the one before it keeps the reader there", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]) });
        const sizes = { scrollHeight: 2000, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);

        sizes.scrollHeight = 2200;
        await resize();
        expect(sizes.scrollTop).toBe(1700);

        // the next photo decodes before the browser reports the scroll that kept the reader there
        sizes.scrollHeight = 2350;
        echoScroll(wrapper);
        await resize();

        expect(sizes.scrollTop).toBe(1850);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
    });

    // A jump is the feed taking the reader somewhere: they are where it goes from the moment it
    // starts, so neither its smooth scroll's first frames, still near the bottom, nor rows settling
    // on the way pull them back to the newest message.
    test("a jump to an older message is not pulled back to the newest message as rows settle on the way", async () => {
        const wrapper = draw({ timeline: timeline([1, 2, 3, 4, 5].map(id => message(id, anna, at(14, id)))) });
        const sizes = { scrollHeight: 2000, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        const requested = holdSmoothScrolls(wrapper);
        const row = wrapper.get("[data-message-id='2']").element;
        row.getBoundingClientRect = () => ({ top: -700, bottom: -660, height: 40, left: 0, right: 100, width: 100, x: 0, y: -700, toJSON: () => ({}) });

        await wrapper.setProps({ landOnId: 2 });
        await nextTick();
        await nextTick();
        await nextTick();

        // the scroll has moved a few pixels, and a photo in the rows above decodes
        sizes.scrollTop = 1495;
        echoScroll(wrapper);
        sizes.scrollHeight = 2300;
        await resize();

        expect(sizes.scrollTop).toBe(1495);
        // the row is brought to the middle of the feed
        expect(requested).toEqual([1500 - 700 - (500 - 40) / 2]);
    });

    test("the unread strip's jump up to the New divider is not pulled back to the newest message as rows settle on the way", async () => {
        const wrapper = draw({
            timeline: timeline([1, 2, 3, 4, 5].map(id => message(id, anna, at(14, id))), { readCursor: 1, unreadCount: 4, firstUnreadId: 2 })
        });
        const sizes = { scrollHeight: 2000, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        const requested = holdSmoothScrolls(wrapper);
        const divider = wrapper.get("[data-testid='unread-divider']").element;
        divider.getBoundingClientRect = () => ({ top: -600, bottom: -580, height: 20, left: 0, right: 100, width: 100, x: 0, y: -600, toJSON: () => ({}) });
        FakeIntersectionObserver.report(divider, false);
        await nextTick();

        await wrapper.get(".chat-strip-jump").trigger("click");
        sizes.scrollTop = 1496;
        echoScroll(wrapper);
        sizes.scrollHeight = 2300;
        await resize();

        expect(sizes.scrollTop).toBe(1496);
        expect(requested).toEqual([1500 - 600 - 52]);
    });

    test("Jump to latest takes the reader to the newest message, so rows settling on the way keep them going there", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]) });
        const sizes = { scrollHeight: 2000, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        scrollReader(wrapper, 400);
        await nextTick();
        const requested = holdSmoothScrolls(wrapper);

        await wrapper.get("[data-testid='jump-latest']").trigger("click");
        // the scroll down has begun, and a photo above decodes
        sizes.scrollTop = 450;
        echoScroll(wrapper);
        sizes.scrollHeight = 2300;
        await resize();

        expect(sizes.scrollTop).toBe(1800);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
        expect(requested).toEqual([1500]);
    });

    test("a reader just above the bottom whom shrinking content leaves at the end is at the newest message, so Jump to latest hides and an arrival is followed", async () => {
        const state = timeline([message(1, anna, at(14, 0)), message(2, anna, at(14, 1))]);
        const wrapper = draw({ timeline: state });
        const sizes = { scrollHeight: 2000, clientHeight: 500, scrollTop: 0 };
        layOut(wrapper, sizes);
        scrollReader(wrapper, 1440);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(true);

        // content gets shorter (a deletion), and the browser clamps the reader to the end
        sizes.scrollHeight = 1900;
        sizes.scrollTop = 1400;
        echoScroll(wrapper);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);

        state.messages.push(message(3, anna, at(14, 2)));
        sizes.scrollHeight = 2000;
        await nextTick();
        await resize();

        expect(sizes.scrollTop).toBe(1500);
        expect(isShown(wrapper, "[data-testid='jump-latest']")).toBe(false);
    });
});

describe("a block post's buttons", () => {
    test("a press reaches the feed's owner as the message's server action", async () => {
        const card = message(7, anna, at(14, 50), {
            message_type: "system",
            body: "Count me in",
            blocks: [{
                type: "buttons",
                fallback: { type: "text", text: "Count me in" },
                buttons: [{ id: "join", label: "Count me in", call: "chat_action_join" }]
            }]
        } as Partial<TimelineMessage>);
        const wrapper = draw({ timeline: timeline([card]), isChannel: true });
        await nextTick();

        await wrapper.get("[data-testid='block-button']").trigger("click");

        const asked = wrapper.emitted("messageAction") as [{ kind: string, message: TimelineMessage, action: { id: string, call: string } }][] | undefined;
        expect(asked?.length).toBe(1);
        expect(asked?.[0][0].kind).toBe("server");
        expect(asked?.[0][0].message.id).toBe(7);
        expect(asked?.[0][0].action).toMatchObject({ id: "join", call: "chat_action_join" });
    });
});

describe("the history skeleton", () => {
    test("shows only once the first page has taken more than a second, and goes when it lands", async () => {
        const state = timeline([], { isLoaded: false });
        const wrapper = draw({ timeline: state });

        jest.advanceTimersByTime(999);
        await nextTick();
        expect(wrapper.find("[data-testid='history-skeleton']").exists()).toBe(false);

        jest.advanceTimersByTime(1);
        await nextTick();
        expect(wrapper.find("[data-testid='history-skeleton']").exists()).toBe(true);

        state.isLoaded = true;
        state.messages.push(message(1, anna, at(14, 0)));
        await nextTick();
        expect(wrapper.find("[data-testid='history-skeleton']").exists()).toBe(false);
    });

    test("stands over the feed rather than in it, so it can fade out over the first page as that page rises in", async () => {
        const wrapper = draw({ timeline: timeline([], { isLoaded: false }) });

        jest.advanceTimersByTime(1000);
        await nextTick();

        const skeleton = wrapper.get("[data-testid='history-skeleton']").element;
        expect(wrapper.get("[data-testid='message-feed']").element.contains(skeleton)).toBe(false);
        expect(wrapper.element.contains(skeleton)).toBe(true);
    });

    test("draws a channel's flat rows with 36px faces and a direct message's bubbles", async () => {
        const channel = draw({ timeline: timeline([], { isLoaded: false }), isChannel: true });
        const direct = draw({ timeline: timeline([], { isLoaded: false }), isDirectMessage: true });

        jest.advanceTimersByTime(1000);
        await nextTick();

        expect(channel.findAll("[data-testid='skeleton-row']")).toHaveLength(5);
        expect(channel.findAll("[data-testid='skeleton-face']")).toHaveLength(5);
        expect(channel.find("[data-testid='skeleton-bubble']").exists()).toBe(false);
        expect(direct.findAll("[data-testid='skeleton-bubble']")).toHaveLength(5);
        expect(direct.find("[data-testid='skeleton-face']").exists()).toBe(false);
    });

    test("never shows for a page that lands within the second", async () => {
        const state = timeline([], { isLoaded: false });
        const wrapper = draw({ timeline: state });

        jest.advanceTimersByTime(500);
        state.isLoaded = true;
        await nextTick();
        jest.advanceTimersByTime(2000);
        await nextTick();

        expect(wrapper.find("[data-testid='history-skeleton']").exists()).toBe(false);
    });
});

describe("the channel intro", () => {
    const volunteers: ChannelFacts = {
        channel_id: "c1",
        name: "Volunteers",
        icon_url: null,
        channel_type: "shared",
        is_member: true,
        can_post: true,
        can_post_code: null,
        can_post_reason: null,
        max_length: 5000,
        can_leave: true,
        description: "Serve teams across the church",
        group_type_name: "Serving Team",
        campus_id: null,
        is_public: true,
        member_count: 42,
        leaders: [],
        faces: [],
        people: []
    };

    function drawWithFacts(state: TimelineState, extra: Record<string, unknown> = {}): VueWrapper {
        return mount(MessageFeed, { props: { pending: [], personAliasGuid: me, isChannel: true, timeline: state, facts: volunteers, campuses: [], notifyMode: "mentions", ...extra } });
    }

    test("is drawn at the top of a channel once its oldest page is loaded, before the first day", () => {
        const wrapper = drawWithFacts(timeline([message(1, anna, at(14, 50))]));

        const intro = wrapper.getComponent(ChannelIntro);
        expect(intro.props("facts")).toEqual(volunteers);
        expect(intro.props("notifyMode")).toBe("mentions");
        expect(intro.props("isEmpty")).toBe(false);
        expect(isBefore(intro.element, wrapper.get("[data-testid='day-section']").element)).toBe(true);
        expect(wrapper.get("[data-testid='message-feed']").element.contains(intro.element)).toBe(true);
    });

    test("stays at the top of a short channel, the free room between it and the messages kept by the composer", () => {
        const wrapper = drawWithFacts(timeline([message(1, anna, at(14, 50))]));
        const fill = wrapper.get("[data-testid='feed-fill']").element;

        expect(isBefore(wrapper.getComponent(ChannelIntro).element, fill)).toBe(true);
        expect(isBefore(fill, wrapper.get("[data-testid='day-section']").element)).toBe(true);
    });

    test("tells the intro the person's join was refused, for its foot", () => {
        const wrapper = drawWithFacts(timeline([]), { isJoinRefused: true });

        expect(wrapper.getComponent(ChannelIntro).props("isJoinRefused")).toBe(true);
    });

    test("tells the intro an empty channel is empty, for its foot", () => {
        const wrapper = drawWithFacts(timeline([]));

        expect(wrapper.getComponent(ChannelIntro).props("isEmpty")).toBe(true);
    });

    test("is not drawn while older messages are still to load", () => {
        const wrapper = drawWithFacts(timeline([message(1, anna, at(14, 50))], { hasMore: true }));

        expect(wrapper.findComponent(ChannelIntro).exists()).toBe(false);
    });

    test("is not drawn before the first page lands", () => {
        const wrapper = drawWithFacts(timeline([], { isLoaded: false }));

        expect(wrapper.findComponent(ChannelIntro).exists()).toBe(false);
    });

    test("is not drawn before the room's facts are read", () => {
        const wrapper = drawWithFacts(timeline([message(1, anna, at(14, 50))]), { facts: null });

        expect(wrapper.findComponent(ChannelIntro).exists()).toBe(false);
    });

    test("is never drawn in a direct message", () => {
        const wrapper = drawWithFacts(timeline([message(1, anna, at(14, 50))]), { isChannel: false, isDirectMessage: true, facts: { ...volunteers, channel_type: "dm" } });

        expect(wrapper.findComponent(ChannelIntro).exists()).toBe(false);
    });

    test("appears once the last older page is loaded", async () => {
        const state = timeline([message(5, anna, at(14, 50))], { hasMore: true });
        const wrapper = drawWithFacts(state);

        state.messages.unshift(message(1, anna, at(14, 40)));
        state.hasMore = false;
        await nextTick();

        expect(wrapper.findComponent(ChannelIntro).exists()).toBe(true);
    });
});

// The feed keeps one action bar, which moves to the message the pointer is on. It first shows
// once the pointer rests on a message, then follows the pointer from message to message at once,
// and hides a moment after the pointer leaves the messages. A menu or the picker open from it
// holds it where it is; a keyboard reaching a message shows it there at once.
describe("the action bar", () => {
    let mounted: VueWrapper | null = null;

    beforeAll(() => {
        // jsdom has no ResizeObserver, and floating placement keeps the bar's menu beside it with one.
        if (typeof window.ResizeObserver === "undefined") {
            window.ResizeObserver = class {
                observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
                unobserve(): void { /* As above. */ }
                disconnect(): void { /* As above. */ }
            } as unknown as typeof ResizeObserver;
        }
    });

    afterEach(() => {
        mounted?.unmount();
        mounted = null;
        document.body.innerHTML = "";
        jest.restoreAllMocks();
    });

    /** The four messages drawFour draws, held so a test can change one as the platform would. */
    let held: TimelineState;

    /**
     * Four messages from Anna, in a channel or a direct message, the feed in the page so focus can
     * land. A test about the bar's entrance keeps Vue's real Transition, which marks an element
     * entering as a browser plays it.
     */
    function drawFour(isChannel: boolean, isTransitionKept = false): VueWrapper {
        held = timeline([1, 2, 3, 4].map(id => message(id, anna, at(14, 40 + id))));
        mounted = mount(MessageFeed, {
            props: { pending: [], personAliasGuid: me, timeline: held, isChannel, isDirectMessage: !isChannel },
            attachTo: document.body,
            global: isTransitionKept ? { stubs: { transition: false } } : {}
        });

        // Vue drops an event stamped no later than the moment its listener was attached, as one
        // fired while the page was being drawn, and the fake clock stands still until moved.
        jest.advanceTimersByTime(1);
        return mounted;
    }

    /** Four messages from Anna in a channel, the second as given, with the feed's own props. */
    function drawCase(second: Partial<TimelineMessage>, feedProps: Record<string, unknown> = {}): VueWrapper {
        const messages = [1, 2, 3, 4].map(id => message(id, anna, at(14, 40 + id), id === 2 ? second : {}));
        mounted = mount(MessageFeed, { props: { pending: [], personAliasGuid: me, timeline: timeline(messages), isChannel: true, ...feedProps }, attachTo: document.body });
        jest.advanceTimersByTime(1);
        return mounted;
    }

    /** Whether the bar on screen is playing its entrance. */
    function isEntering(w: VueWrapper): boolean {
        return bars(w)[0]?.classList.contains("chat-tools-enter-active") ?? false;
    }

    /** A keyboard's focus, which a browser tells apart by :focus-visible and jsdom cannot, so is told. */
    function focusIsKeyboard(): void {
        const matches = Element.prototype.matches;
        jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string) {
            return selector === ":focus-visible" ? true : matches.call(this, selector);
        });
    }

    async function settle(): Promise<void> {
        await nextTick();
        await nextTick();
    }

    function bars(w: VueWrapper): Element[] {
        return w.findAll("[data-testid='action-bar']").map(found => found.element);
    }

    /** The message the one bar is drawn on, or null with none drawn. */
    function barOn(w: VueWrapper): number | null {
        const drawn = bars(w);
        expect(drawn.length).toBeLessThanOrEqual(1);
        const row = drawn[0]?.closest<HTMLElement>("[data-hot-id]");
        return row ? Number(row.dataset.hotId) : null;
    }

    async function pointTo(w: VueWrapper, id: number): Promise<void> {
        await w.get(`[data-hot-id='${id}']`).trigger("pointerover");
        await settle();
    }

    async function wait(ms: number): Promise<void> {
        jest.advanceTimersByTime(ms);
        await settle();
    }

    test.each([["a channel", true], ["a direct message", false]])("in %s, shows once the pointer has rested 150ms on a message, not before", async (_kind, isChannel) => {
        const w = drawFour(isChannel);

        await pointTo(w, 2);
        await wait(149);
        expect(barOn(w)).toBeNull();

        await wait(1);
        expect(barOn(w)).toBe(2);
    });

    test("a pointer sweeping over the messages never stops long enough to show it", async () => {
        const w = drawFour(true);

        for (const id of [1, 2, 3, 4, 3, 2, 1]) {
            await pointTo(w, id);
            await wait(100);
            expect(barOn(w)).toBeNull();
        }
    });

    test("once shown, moves to the next message the pointer reaches at once, and there is only ever one", async () => {
        const w = drawFour(true);
        await pointTo(w, 1);
        await wait(150);

        for (const id of [2, 3, 4, 3, 1]) {
            await pointTo(w, id);
            expect(barOn(w)).toBe(id);
            expect(bars(w)).toHaveLength(1);
        }

        await wait(1000);
        expect(bars(w)).toHaveLength(1);
    });

    test("is one bar for the whole feed, never one a message", async () => {
        const w = drawFour(false);
        await pointTo(w, 1);
        await wait(150);
        await pointTo(w, 3);

        expect(w.findAllComponents(ActionBar)).toHaveLength(1);
        expect(w.findAll("[data-testid='action-bar']")).toHaveLength(1);
    });

    test("hides 220ms after the pointer leaves the feed, and a pointer back inside that keeps it", async () => {
        const w = drawFour(true);
        await pointTo(w, 2);
        await wait(150);

        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(150);
        await pointTo(w, 3);
        await wait(1000);
        expect(barOn(w)).toBe(3);

        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(219);
        expect(barOn(w)).toBe(3);

        await wait(1);
        expect(barOn(w)).toBeNull();
    });

    test("once hidden, waits for the rest again", async () => {
        const w = drawFour(true);
        await pointTo(w, 2);
        await wait(150);
        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(220);

        await pointTo(w, 4);
        await wait(149);
        expect(barOn(w)).toBeNull();

        await wait(1);
        expect(barOn(w)).toBe(4);
    });

    test("stays on its message while its menu is open, wherever the pointer goes, and leaves once it closes", async () => {
        // A mouse's focus never matches :focus-visible in a browser, so it holds no bar; jsdom
        // cannot tell, and is told.
        const matches = Element.prototype.matches;
        jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string) {
            return selector === ":focus-visible" ? false : matches.call(this, selector);
        });
        const w = drawFour(true);
        await pointTo(w, 2);
        await wait(150);

        (w.get("[data-testid='action-bar'] button[aria-label='More actions']").element as HTMLElement).click();
        await settle();
        expect(document.querySelector("[role='menu']")).not.toBeNull();

        await pointTo(w, 3);
        expect(barOn(w)).toBe(2);
        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(1000);
        expect(barOn(w)).toBe(2);

        // A press outside closes the menu; the pointer is away from the messages, so the bar's grace starts.
        document.body.dispatchEvent(new Event("pointerdown", { bubbles: true }));
        await settle();
        expect(document.querySelector("[role='menu']")).toBeNull();

        await wait(220);
        expect(barOn(w)).toBeNull();
    });

    test("shows at once on the message a keyboard reaches", async () => {
        const matches = Element.prototype.matches;
        jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string) {
            return selector === ":focus-visible" ? true : matches.call(this, selector);
        });
        const w = drawFour(true);

        const row = w.get("[data-hot-id='3']").element as HTMLElement;
        row.dispatchEvent(new FocusEvent("focusin", { bubbles: true }));
        await settle();

        expect(barOn(w)).toBe(3);
    });

    test("a scroll hides it at once", async () => {
        const w = drawFour(true);
        await pointTo(w, 2);
        await wait(150);

        placeScroll(w, 100);
        await settle();

        expect(barOn(w)).toBeNull();
    });

    // Whether a message offers a bar is one rule, the row's: a message that offers none is not a
    // place the bar can be, so resting on it shows nothing and counts for nothing.
    test.each([
        ["a deleted message", { deleted_at: at(14, 50), body: null }, {}],
        ["a hidden message", { visibility: "hidden", visibility_notice: "Hidden by a moderator.", body: null }, {}],
        ["a removed message", { visibility: "removed", visibility_notice: "Removed by Cedar Hill staff.", body: null }, {}],
        ["a message open in the editor", {}, { editingId: 2 }]
    ] as Array<[string, Partial<TimelineMessage>, Record<string, unknown>]>)("%s offers no bar, so the next message's bar still waits for the rest", async (_what, second, feedProps) => {
        const w = drawCase(second, feedProps);

        expect(w.find("[data-hot-id='2']").exists()).toBe(false);
        await w.get("[data-message-id='2']").trigger("pointerover");
        await wait(1000);
        expect(barOn(w)).toBeNull();

        await pointTo(w, 3);
        expect(barOn(w)).toBeNull();
        await wait(150);
        expect(barOn(w)).toBe(3);
    });

    test("a room the person is previewing offers no bar on any message", async () => {
        const w = drawCase({}, { isPreviewing: true });

        expect(w.findAll("[data-hot-id]")).toHaveLength(0);
        await w.get("[data-message-id='2']").trigger("pointerover");
        await wait(1000);
        expect(bars(w)).toHaveLength(0);
    });

    test("goes at once when its message opens in the editor, and the next message's bar waits for the rest", async () => {
        const w = drawFour(true);
        await pointTo(w, 2);
        await wait(150);

        await w.setProps({ editingId: 2 });
        await settle();
        expect(barOn(w)).toBeNull();

        await pointTo(w, 3);
        expect(barOn(w)).toBeNull();
        await wait(150);
        expect(barOn(w)).toBe(3);
    });

    test("goes when its message opens in the editor while its menu holds it, and holds nothing after", async () => {
        const matches = Element.prototype.matches;
        jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string) {
            return selector === ":focus-visible" ? false : matches.call(this, selector);
        });
        const w = drawFour(true);
        await pointTo(w, 2);
        await wait(150);
        (w.get("[data-testid='action-bar'] button[aria-label='More actions']").element as HTMLElement).click();
        await settle();

        await w.setProps({ editingId: 2 });
        await settle();
        expect(barOn(w)).toBeNull();

        await pointTo(w, 3);
        expect(barOn(w)).toBeNull();
        await wait(150);
        expect(barOn(w)).toBe(3);
        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(220);
        expect(barOn(w)).toBeNull();
    });

    test("a send on its way or failed offers no bar", async () => {
        const messages = [1, 2].map(id => message(id, me, at(14, 40 + id)));
        const pending: PendingMessage[] = [
            { localId: "local-1", channelId: "c1", body: "On my way", status: "sending", errorCode: null },
            { localId: "local-2", channelId: "c1", body: "Still here", status: "failed", errorCode: null }
        ];
        mounted = mount(MessageFeed, { props: { pending, personAliasGuid: me, timeline: timeline(messages), isChannel: true }, attachTo: document.body });

        const rows = mounted.findAll("[data-testid='pending-row']");
        expect(rows).toHaveLength(2);
        for (const row of rows) {
            expect(row.attributes("data-hot-id")).toBeUndefined();
            await row.trigger("pointerover");
            await wait(1000);
            expect(bars(mounted)).toHaveLength(0);
        }
    });

    // The bar is one element for the feed's life, so its first show, the first in a conversation
    // just opened, plays the same entrance as every show after.
    test.each([["a channel", true], ["a direct message", false]])("in %s, plays its entrance the first time it shows, as every time after", async (_kind, isChannel) => {
        const w = drawFour(isChannel, true);

        await pointTo(w, 2);
        await wait(150);
        expect(barOn(w)).toBe(2);
        expect(isEntering(w)).toBe(true);

        // Vue's Transition holds a leaving element until the frames after its fade.
        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(1000);
        await wait(100);
        expect(barOn(w)).toBeNull();

        await pointTo(w, 3);
        await wait(150);
        expect(barOn(w)).toBe(3);
        expect(isEntering(w)).toBe(true);
    });

    test("goes at once when its message is deleted while it shows, and the next bar plays its entrance", async () => {
        const w = drawFour(true, true);
        await pointTo(w, 2);
        await wait(150);

        held.messages[1].deleted_at = at(14, 50);
        held.messages[1].body = null;
        await settle();
        expect(barOn(w)).toBeNull();

        await pointTo(w, 3);
        await wait(150);
        expect(barOn(w)).toBe(3);
        expect(isEntering(w)).toBe(true);
    });

    test("goes when its message is deleted while its menu holds it, and holds nothing after", async () => {
        const matches = Element.prototype.matches;
        jest.spyOn(Element.prototype, "matches").mockImplementation(function (this: Element, selector: string) {
            return selector === ":focus-visible" ? false : matches.call(this, selector);
        });
        const w = drawFour(true);
        await pointTo(w, 2);
        await wait(150);
        (w.get("[data-testid='action-bar'] button[aria-label='More actions']").element as HTMLElement).click();
        await settle();

        held.messages[1].deleted_at = at(14, 50);
        held.messages[1].body = null;
        await settle();
        expect(barOn(w)).toBeNull();

        // Nothing still holds it: the next message shows its bar after the rest, and it leaves after the grace.
        await pointTo(w, 3);
        await wait(150);
        expect(barOn(w)).toBe(3);
        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(220);
        expect(barOn(w)).toBeNull();
    });

    // The bar's own tools sit inside its message's row, so Tab out of them into the next message
    // must leave the bar there, not keep it held where it was.
    test("Tab from one message's bar into another message shows that message's bar at once", async () => {
        focusIsKeyboard();
        const w = drawFour(true);

        (w.get("[data-hot-id='2']").element as HTMLElement).focus();
        await settle();
        expect(barOn(w)).toBe(2);

        (w.get("[data-testid='action-bar'] button").element as HTMLElement).focus();
        await settle();
        expect(barOn(w)).toBe(2);

        (w.get("[data-hot-id='3']").element as HTMLElement).focus();
        await settle();
        expect(barOn(w)).toBe(3);

        await wait(1000);
        expect(barOn(w)).toBe(3);
    });

    test("a keyboard's focus inside the bar holds it while the pointer goes, and once focus leaves the feed it goes after its grace", async () => {
        focusIsKeyboard();
        const w = drawFour(true);
        const outside = document.createElement("button");
        document.body.appendChild(outside);

        (w.get("[data-hot-id='2']").element as HTMLElement).focus();
        await settle();
        (w.get("[data-testid='action-bar'] button").element as HTMLElement).focus();
        await settle();

        await pointTo(w, 4);
        await w.get("[data-testid='message-feed']").trigger("pointerleave");
        await wait(1000);
        expect(barOn(w)).toBe(2);

        outside.focus();
        await wait(219);
        expect(barOn(w)).toBe(2);
        await wait(1);
        expect(barOn(w)).toBeNull();
    });
});

// A failed send's own Not sent marker reaches the feed's owner as the one message's Try again or
// Delete, by its key, so one failure among several is mended alone.
describe("a failed send's own choices", () => {
    test("Try again and Delete on one failed message ask the owner for that message alone", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), pending: [pendingRow("a", "failed"), pendingRow("b", "failed"), pendingRow("c", "failed")] });
        const markers = wrapper.findAll("[data-testid='message-failed-actions']");
        expect(markers).toHaveLength(3);

        await markers[1].trigger("click");
        await nextTick();
        await wrapper.findAll("[role='menuitem']")[0].trigger("click");
        expect(wrapper.emitted("retry")).toEqual([[["b"]]]);

        await wrapper.findAll("[data-testid='message-failed-actions']")[1].trigger("click");
        await nextTick();
        await wrapper.findAll("[role='menuitem']")[1].trigger("click");
        expect(wrapper.emitted("discard")).toEqual([["b"]]);
    });
});
