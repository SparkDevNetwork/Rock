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
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, reactive } from "vue";
import MessageFeed from "../../../../../src/Communication/Chat/ChatShell/components/messageFeed.partial.obs";
import MessageRow from "../../../../../src/Communication/Chat/ChatShell/components/messageRow.partial.obs";
import { TimelineState } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { createSender, SendResult } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { PendingMessage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

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

/** Gives the feed's scroller the sizes a browser would, and puts the reader at a scroll position. */
function placeScroll(wrapper: VueWrapper, scrollTop: number, scrollHeight = 2000, clientHeight = 500): void {
    const scroller = wrapper.get("[data-testid='message-feed']").element as HTMLElement;
    let top = scrollTop;
    Object.defineProperty(scroller, "scrollHeight", { configurable: true, get: () => scrollHeight });
    Object.defineProperty(scroller, "clientHeight", { configurable: true, get: () => clientHeight });
    Object.defineProperty(scroller, "scrollTop", { configurable: true, get: () => top, set: (value: number) => top = value });
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
};

function draw(props: FeedProps): VueWrapper {
    return mount(MessageFeed, { props: { pending: [], personAliasGuid: me, ...props } });
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

    test("a failed send's Try again and Delete reach the shell by the row's local id", async () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(14, 50))]), pending: [pendingRow("b", "failed")] });

        await wrapper.get("[data-testid='retry-send']").trigger("click");
        await wrapper.get("[data-testid='discard-send']").trigger("click");

        expect(wrapper.emitted("retry")).toEqual([["b"]]);
        expect(wrapper.emitted("discard")).toEqual([["b"]]);
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
// the block draws is simply there. Decided as the feed mounts and never again, so nothing a
// redraw changes can replay it.
describe("the feed's entrance", () => {
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

    type Race = { wrapper: VueWrapper, answer: Deferred<SendResult>, send: () => Promise<boolean>, liveCopy: () => void, other: (id: number) => void };

    /** The feed fed by a real sender and real timelines, whose send answers when the test says so. */
    async function race(isChannel = false): Promise<Race> {
        const answer = deferred<SendResult>();
        let sender: ReturnType<typeof createSender> | null = null;
        const timelines = createTimelines({
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
            setup: () => () => h(MessageFeed, { timeline: timelines.state("c1"), pending: sender?.pending("c1") ?? [], personAliasGuid: me, isChannel, sentKeys: sender?.sentKeys })
        });
        const wrapper = mount(Host);

        return {
            wrapper,
            answer,
            send: () => (sender as ReturnType<typeof createSender>).send("c1", "On my way"),
            liveCopy: () => timelines.applyEvent("c1", "message.created", created(2, me, "local-1")),
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
});

describe("day sections", () => {
    test("each day is its own section with its own pill", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(9, 0, 1)), message(2, anna, at(9, 0))]) });

        const sections = wrapper.findAll("[data-testid='day-section']");
        expect(sections).toHaveLength(2);
        expect(sections.map(section => section.get("[data-testid='day-pill']").text())).toEqual(["Yesterday", "Today"]);
    });
});

describe("the New divider and the unread strip", () => {
    function unread(): VueWrapper {
        return draw({
            timeline: timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 7 })
        });
    }

    test("the divider reads New and sits above the first message past the read position", () => {
        const wrapper = unread();

        const divider = wrapper.get("[data-testid='unread-divider']");
        expect(divider.text()).toBe("New");
        expect(isBefore(wrapper.get("[data-message-id='1']").element, divider.element)).toBe(true);
        expect(isBefore(divider.element, wrapper.get("[data-message-id='2']").element)).toBe(true);
    });

    test("no divider and no strip with nothing unread", () => {
        const wrapper = draw({ timeline: timeline([message(1, anna, at(11, 0))], { readCursor: 1, unreadCount: 0 }) });

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
            timeline: timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 2 }),
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
        const state = timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 7 });
        const wrapper = draw({ timeline: state });

        await wrapper.setProps({ pending: [pendingRow("a")] });
        await nextTick();
        expect(wrapper.find("[data-testid='unread-divider']").exists()).toBe(false);

        // A reconnect's newest page: the send stored, then Anna wrote after it.
        state.messages.push(message(4, me, at(11, 40)), message(5, anna, at(11, 45)));
        state.readCursor = 4;
        state.unreadCount = 1;
        await nextTick();

        const divider = wrapper.get("[data-testid='unread-divider']");
        expect(isBefore(wrapper.get("[data-message-id='4']").element, divider.element)).toBe(true);
        expect(isBefore(divider.element, wrapper.get("[data-message-id='5']").element)).toBe(true);
        FakeIntersectionObserver.report(divider.element, false);
        await nextTick();
        expect(isShown(wrapper, "[data-testid='unread-strip']")).toBe(true);
    });

    test("after a send, a catch-up whose divider falls at or before the newest message at the send keeps them hidden", async () => {
        const state = timeline([message(1, anna, at(11, 0)), message(2, anna, at(11, 30)), message(3, anna, at(11, 31))], { readCursor: 1, unreadCount: 7 });
        const wrapper = draw({ timeline: state });
        FakeIntersectionObserver.report(wrapper.get("[data-testid='unread-divider']").element, false);
        await nextTick();

        await wrapper.setProps({ pending: [pendingRow("a")] });
        await nextTick();

        // The same read position comes back with the send stored after it: nothing new from others.
        state.messages.push(message(4, me, at(11, 40)));
        state.readCursor = 1;
        state.unreadCount = 3;
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

    test("with the reader's own messages past the read position, the strip counts only the others' after the divider", async () => {
        // The platform's count is everything past the read position, the reader's own included.
        const wrapper = draw({
            timeline: timeline([
                message(1, anna, at(11, 0)),
                message(2, me, at(11, 20)),
                message(3, anna, at(11, 30)),
                message(4, me, at(11, 31)),
                message(5, anna, at(11, 32)),
                message(6, anna, at(11, 33), { message_type: "system" })
            ], { readCursor: 1, unreadCount: 4 })
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
        // the failed row grows by its Not sent line
        placeScroll(wrapper, 1500, 2060);

        await wrapper.setProps({ pending: [pendingRow("a", "failed")] });
        await nextTick();
        await nextTick();

        expect((wrapper.get("[data-testid='message-feed']").element as HTMLElement).scrollTop).toBe(2060);
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
