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
// A row moves only for something that happened while it was on screen: a message arriving live or
// sent here, an edit, a delete, a hide, the reader's own reaction, a thread's first reply. A mount,
// a room switch and back, a page read again, a catch-up and a remount only state what is, and play
// nothing. Driven through the real stores, wired and fed the live events in the order the shell
// does, and the real feed and thread drawer, fed the platform's own message shapes.
import { enableAutoUnmount, flushPromises, mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import MessageFeed from "../../../../../src/Communication/Chat/ChatShell/components/messageFeed.partial.obs";
import ThreadDrawer from "../../../../../src/Communication/Chat/ChatShell/components/threadDrawer.partial.obs";
import { createTimelines } from "../../../../../src/Communication/Chat/ChatShell/composables/useHistory.partial";
import { createThreads } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";
import { createReactions } from "../../../../../src/Communication/Chat/ChatShell/composables/useReactions.partial";
import { createSender, Sender, SendResult } from "../../../../../src/Communication/Chat/ChatShell/composables/useSend.partial";
import { Blocks, createBlocks } from "../../../../../src/Communication/Chat/ChatShell/composables/useBlocks.partial";
import { createMessageMotion, messageMotionKey } from "../../../../../src/Communication/Chat/ChatShell/messageMotion.partial";
import { CallResult } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { HistoryPage, TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { anna, channelId, liveCreated, me, pageRow } from "../messageShapes";
import { playAnimations, tailPlayed } from "../tailStage";

/** Every class a row's one-off motion sets, a crossfade, Edited sliding in, a chip's pop or roll, a thread's pill, and the tail's arrival on a row. */
const motionSelector = `.is-crossfading, .is-swapping, .is-popping, .is-rolling, ${tailPlayed("chat-tail-in")}, ${tailPlayed("chat-tail-handoff")}`;

const heart = { code: "heart", emoji: "❤️", image_url: null, label: "Heart" };

/** Someone the reader blocks and unblocks. */
const dee = "a0000003-0000-4000-8000-000000000000";

/** A page as the platform answers it. */
function page(messages: TimelineMessage[], rev = 1): HistoryPage {
    return { messages, read_cursor: null, unread_count: 0, has_more: false, rev };
}

/** A send whose answer comes when the test says. */
type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

/**
 * The stores a shell holds, wired to each other as the shell wires them, with the platform's
 * answers queued by the test, and a live router that masks an event for the people blocked and
 * hands it to each holder in the shell's order: the threads, the thread's reactions, then the
 * timeline, then the sender.
 */
function wire() {
    const pages: HistoryPage[] = [];
    // a thread page the test may hold back, to land when it says
    const threadPages: (Record<string, unknown> | Promise<Record<string, unknown>>)[] = [];
    const answers: Deferred<SendResult>[] = [];
    const reactionAnswers: Deferred<CallResult>[] = [];
    let sender: Sender | null = null;
    // eslint-disable-next-line prefer-const
    let blocks: Blocks;
    let keys = 0;
    const motion = createMessageMotion();

    const call = async (name: string): Promise<CallResult> => {
        if (name === "chat_get_thread_history") {
            return { ok: true, data: await threadPages.shift() ?? { root: null, messages: [], has_more: false, following: false, read_cursor: null, unread_count: 0 } } as CallResult;
        }
        if (name === "chat_add_reaction" || name === "chat_remove_reaction") {
            const answer = deferred<CallResult>();
            reactionAnswers.push(answer);
            return answer.promise;
        }
        return { ok: true, data: {} } as CallResult;
    };

    const timelines = createTimelines({
        limits: () => ({ history_page_size: 50 }),
        fetchPage: async () => pages.shift() ?? page([]),
        onLanded: message => sender?.landed(message),
        applyReaction: (id, event, payload, isLive) => reactions.applyEvent(id, event, payload, isLive),
        mask: message => blocks.mask(message),
        motion
    });
    const threads = createThreads({
        call,
        timelines,
        personAliasGuid: me,
        limits: () => ({ history_page_size: 50, thread_page_size: 25 }),
        onLanded: message => sender?.landed(message),
        mask: message => blocks.mask(message),
        isBlocked: alias => blocks.isBlocked(alias),
        motion
    });
    const reactions = createReactions({
        call,
        timelines,
        threads,
        personAliasGuid: me,
        notify: () => undefined,
        favorites: [],
        churchKey: "t",
        maskFaces: people => blocks.maskFaces(people),
        mask: message => blocks.mask(message),
        motion
    });
    sender = createSender({
        send: () => {
            const answer = deferred<SendResult>();
            answers.push(answer);
            return answer.promise;
        },
        timelines,
        onReplyConfirmed: (id, message) => threads.upsertReply(id, message),
        personAliasGuid: me,
        newLocalId: () => `k${++keys}`,
        motion
    });
    // a change of the set masks again what is held, and an unblock marks what only a read gives back
    blocks = createBlocks({
        call,
        onChanged: (_isInSidebarAnswer, removed) => timelines.release(removed),
        remask: added => {
            timelines.remask(added);
            threads.remask();
        },
        notify: () => undefined
    });
    blocks.setAliases([]);

    function live(event: string, rawPayload: Record<string, unknown>): void {
        const payload = blocks.maskEvent(event, rawPayload) as Record<string, unknown>;
        threads.applyEvent(channelId, event, payload);
        if (event === "reaction.added" || event === "reaction.removed") {
            reactions.applyThreadEvent(channelId, event, payload);
        }
        timelines.applyEvent(channelId, event, payload);
        if (event === "message.created" && typeof payload.client_key === "string") {
            (sender as Sender).landed({ id: payload.id as number, client_key: payload.client_key });
        }
    }

    return { pages, threadPages, answers, reactionAnswers, motion, timelines, threads, reactions, blocks, sender: sender as Sender, live, provide: { [messageMotionKey as symbol]: motion } };
}

type Wired = ReturnType<typeof wire>;

async function settle(): Promise<void> {
    for (let i = 0; i < 6; i++) {
        await flushPromises();
        await nextTick();
    }
}

/**
 * The conversation's feed as the shell draws it, keyed by the room so a switch away and back
 * mounts a new one, over the real timeline and the sender's rows.
 */
function drawFeed(w: Wired): { wrapper: VueWrapper, switchAwayAndBack: (pageOnReturn: HistoryPage) => Promise<void> } {
    const isShown = ref(true);
    const opened = ref(0);
    const Host = defineComponent({
        setup: () => () => isShown.value
            ? h(MessageFeed, { key: opened.value, timeline: w.timelines.state(channelId), pending: w.sender.pending(channelId), personAliasGuid: me })
            : null
    });
    const wrapper = mount(Host, { attachTo: document.body, global: { provide: w.provide } });

    return {
        wrapper,
        switchAwayAndBack: async (pageOnReturn: HistoryPage): Promise<void> => {
            w.timelines.leave(channelId);
            isShown.value = false;
            await nextTick();
            opened.value++;
            isShown.value = true;
            await nextTick();
            w.pages.push(pageOnReturn);
            await w.timelines.loadNewest(channelId);
            await settle();
        }
    };
}

/** Opens the room at a first page. */
async function open(w: Wired, messages: TimelineMessage[]): Promise<void> {
    w.pages.push(page(messages));
    await w.timelines.loadNewest(channelId);
}

/** A motion's element, named by the row it is on: the message's id, or a send still out. */
function named(element: Element): string {
    const row = element.closest("[data-message-id], [data-reply-id], [data-testid='pending-row']");
    const tail = (element as HTMLElement).dataset.tailPlayed;
    return `${row?.getAttribute("data-message-id") ?? row?.getAttribute("data-reply-id") ?? "pending"}: ${element.className}${tail ? ` ${tail}` : ""}`;
}

/** Every one-off motion drawn now, by the row it is on. */
function moving(wrapper: VueWrapper): string[] {
    return [...wrapper.element.querySelectorAll(motionSelector)].map(named);
}

/**
 * Watches views for motion added from now: an element that already played keeps its class, so
 * only an element that did not carry one when last marked counts, a new element included.
 */
function motionSince(...views: VueWrapper[]): { added: () => string[], mark: () => void } {
    const all = (): Element[] => views.flatMap(view => [...view.element.querySelectorAll(motionSelector)]);
    let before = new Set(all());
    return {
        added: () => all().filter(element => !before.has(element)).map(named),
        mark: () => {
            before = new Set(all());
        }
    };
}

/** Ends every one-off motion as the browser would at its keyframe's end. */
async function endMotion(wrapper: VueWrapper): Promise<void> {
    for (const element of [...wrapper.element.querySelectorAll(".is-crossfading, .is-swapping")]) {
        for (const name of ["chat-crossfade", "chat-meta-in"]) {
            const event = new Event("animationend", { bubbles: true });
            Object.defineProperty(event, "animationName", { value: name });
            element.dispatchEvent(event);
        }
    }
    await nextTick();
}

// The tail's motions run through the browser's animations, which jsdom lacks, so a stand-in keeps them.
let animations: ReturnType<typeof playAnimations>;
beforeEach(() => {
    animations = playAnimations();
});
afterEach(() => {
    animations.restore();
});

class QuietObserver {
    observe(): void { /* jsdom lays nothing out */ }
    unobserve(): void { /* nothing to stop */ }
    disconnect(): void { /* nothing to stop */ }
    takeRecords(): [] {
        return [];
    }
}

beforeEach(() => {
    (window as unknown as { IntersectionObserver: unknown }).IntersectionObserver = QuietObserver;
});

enableAutoUnmount(afterEach);

describe("a room switch and back plays nothing on its rows", () => {
    test("someone's messages heard live, read again on the return as the page states them, do not crossfade", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        w.live("message.created", liveCreated({ id: 2, sender: anna }));
        await settle();

        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna }), pageRow({ id: 2, sender: anna })]));

        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("the person's own sends, answered and heard live, read again on the return, do not crossfade or rise again", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        const sending = w.sender.send(channelId, "On my way");
        await settle();
        w.answers[0].resolve({ ok: true, id: 2, createdAt: "2026-10-12T14:01:00.000Z", notice: null, pieces: null, blocks: null });
        await sending;
        w.live("message.created", liveCreated({ id: 2, key: "k1", body: "On my way" }));
        await settle();

        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna }), pageRow({ id: 2, key: "k1", body: "On my way" })]));

        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("messages sent while the reader was in another room do not rise as if heard live", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna }), pageRow({ id: 2, sender: anna })]);
        const feed = drawFeed(w);

        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna }), pageRow({ id: 2, sender: anna }), pageRow({ id: 3, sender: anna })]));

        expect(feed.wrapper.find("[data-message-id='3']").exists()).toBe(true);
        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("a send still on its way when the reader left does not rise a second time on the return", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        const sending = w.sender.send(channelId, "On my way");
        await settle();

        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna })]));

        expect(feed.wrapper.find("[data-testid='pending-row']").exists()).toBe(true);
        expect(moving(feed.wrapper)).toEqual([]);

        // its answer and its live copy land after the return, in the element the return drew,
        // and still nothing rises: the send was made here, before this feed was drawn
        const drawn = feed.wrapper.get("[data-testid='pending-row']").element;
        const createdAt = new Date().toISOString();
        w.answers[0].resolve({ ok: true, id: 2, createdAt, notice: null, pieces: null, blocks: null });
        await sending;
        w.live("message.created", liveCreated({ id: 2, key: "k1", body: "On my way", createdAt }, 2));
        await settle();

        expect(feed.wrapper.get("[data-message-id='2']").element).toBe(drawn);
        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("a send still on its way when the reader left, whose live copy lands after the return in a new element, does not rise", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        const createdAt = new Date().toISOString();
        const first = w.sender.send(channelId, "First");
        await settle();
        w.answers[0].resolve({ ok: true, id: 2, createdAt, notice: null, pieces: null, blocks: null });
        await first;
        // a second send, drawn in the person's group under the first, still out as they leave
        const second = w.sender.send(channelId, "Second");
        await settle();

        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna }), pageRow({ id: 2, key: "k1", body: "First", createdAt })]));
        const rising = (): (string | null | undefined)[] => feed.wrapper.findAll(tailPlayed("chat-tail-in")).map(row => row.element.closest("[data-message-id]")?.getAttribute("data-message-id"));
        expect(rising()).toEqual([]);

        // someone's message and the send's live copy heard in one turn: the person's message now
        // stands in a group of its own below hers, a new element, and only hers rises
        w.live("message.created", liveCreated({ id: 3, sender: anna, createdAt }, 2));
        w.live("message.created", liveCreated({ id: 4, key: "k2", body: "Second", createdAt }, 3));
        await settle();
        w.answers[1].resolve({ ok: true, id: 4, createdAt, notice: null, pieces: null, blocks: null });
        await second;
        await settle();

        expect(feed.wrapper.find("[data-message-id='4']").exists()).toBe(true);
        expect(rising()).toEqual(["3"]);
    });

    test("a chip of the reader's that popped before the switch does not pop again on the return", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();
        void w.reactions.toggle(channelId, 1, heart);
        await settle();
        expect(feed.wrapper.get("[data-message-id='1'] [data-testid='reaction-chip']").classes()).toContain("is-popping");

        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna }, { reactions: [{ ...heart, count: 1, mine: true }] })]));

        expect(feed.wrapper.find("[data-message-id='1'] [data-testid='reaction-chip']").exists()).toBe(true);
        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("the reader's reaction from another device, read on the return onto a copy heard live, does not pop", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        w.live("message.created", liveCreated({ id: 2, sender: anna }));
        await settle();

        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna }), pageRow({ id: 2, sender: anna }, { reactions: [{ ...heart, count: 1, mine: true }] })]));

        expect(feed.wrapper.find("[data-message-id='2'] [data-testid='reaction-chip']").exists()).toBe(true);
        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("a thread begun while the reader was away, read on the return onto a copy heard live, does not pop its pill", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        w.live("message.created", liveCreated({ id: 2, sender: anna }));
        await settle();

        const replied = { reply_count: 1, last_reply_at: "2026-10-12T14:05:00.000Z", participant_count: 1, repliers: [{ person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null }] };
        await feed.switchAwayAndBack(page([pageRow({ id: 1, sender: anna }), pageRow({ id: 2, sender: anna }, replied)]));

        expect(feed.wrapper.find("[data-message-id='2'] .chat-tsum").exists()).toBe(true);
        expect(moving(feed.wrapper)).toEqual([]);
    });
});

/** Opens the thread under a root the conversation holds, at a first page, and draws the drawer over it. */
async function drawThread(w: Wired, root: TimelineMessage, replies: TimelineMessage[]): Promise<VueWrapper> {
    w.threadPages.push({ root, messages: replies, has_more: false, following: true, read_cursor: replies[replies.length - 1]?.id ?? null, unread_count: 0, rev: 1 });
    await w.threads.openThread(channelId, root.id);
    const Host = defineComponent({
        setup: () => () => w.threads.state.thread?.root
            ? h(ThreadDrawer, { root: w.threads.state.thread.root, replies: w.threads.state.thread.replies, pending: [], isFollowing: true, isPhone: false, isFromList: false, personAliasGuid: me, conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true, draft: { text: "", mentions: [] } })
            : null
    });
    const drawer = mount(Host, { attachTo: document.body, global: { provide: w.provide } });
    await settle();
    return drawer;
}

// Each real event, applied by the store that owns it while its row is on screen, plays its motion
// once, through the same stores and views the quiet cells drive.
describe("what happens while a row is on screen plays once", () => {
    test.each([["an edit", "message.edited", { body: "Morning everyone", pieces: null, edited_at: "2026-10-12T14:09:00.000Z" }, true], ["a delete", "message.deleted", { deleted_at: "2026-10-12T14:09:00.000Z" }, false], ["a moderator's hide", "message.visibility", { state: "hidden", notice: "Hidden by a moderator" }, false]] as const)("%s heard live crossfades the row, sliding Edited in only for an edit", async (_kind, event, change, isEdit) => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();

        w.live(event, { id: 1, channel_id: channelId, ...change, rev: 2 });
        await settle();

        expect(feed.wrapper.findAll("[data-message-id='1'] .is-crossfading")).toHaveLength(1);
        expect(feed.wrapper.findAll("[data-message-id='1'] .is-swapping").length > 0).toBe(isEdit);
    });

    test("a hide and then a show each crossfade", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();

        w.live("message.visibility", { id: 1, channel_id: channelId, state: "hidden", notice: "Hidden by a moderator", rev: 2 });
        await settle();
        expect(feed.wrapper.findAll("[data-message-id='1'] .is-crossfading")).toHaveLength(1);
        await endMotion(feed.wrapper);

        w.live("message.visibility", { id: 1, channel_id: channelId, state: "visible", notice: null, body: "m1", rev: 3 });
        await settle();
        expect(feed.wrapper.findAll("[data-message-id='1'] .is-crossfading")).toHaveLength(1);
    });

    test("an edit of a root open in the drawer crossfades its row in the conversation and in the drawer, each once", async () => {
        const w = wire();
        const root = pageRow({ id: 40, sender: anna }, { reply_count: 1, participant_count: 1 });
        await open(w, [root]);
        const feed = drawFeed(w);
        const drawer = await drawThread(w, root, [pageRow({ id: 41, sender: anna, parentId: 40 })]);

        w.live("message.edited", { id: 40, channel_id: channelId, body: "Hike moved", pieces: null, edited_at: "2026-10-12T14:09:00.000Z", rev: 2 });
        await settle();

        expect(feed.wrapper.findAll("[data-message-id='40'] .is-crossfading")).toHaveLength(1);
        expect(drawer.findAll(".is-crossfading")).toHaveLength(1);
    });

    test("an edit of a reply shown only in its thread crossfades its row in the drawer", async () => {
        const w = wire();
        const root = pageRow({ id: 40, sender: anna }, { reply_count: 1, participant_count: 1 });
        await open(w, [root]);
        const drawer = await drawThread(w, root, [pageRow({ id: 41, sender: anna, parentId: 40 })]);

        w.live("message.edited", { id: 41, channel_id: channelId, body: "See you there", pieces: null, edited_at: "2026-10-12T14:09:00.000Z", rev: 2 });
        await settle();

        expect(drawer.findAll("[data-reply-id='41'] .is-crossfading")).toHaveLength(1);
    });

    test("someone's message heard live rises once; the same message heard again moves nothing more", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();

        w.live("message.created", liveCreated({ id: 2, sender: anna }, 2));
        await settle();
        expect(moving(feed.wrapper)).toEqual(["2: chat-msg is-conversational is-message chat-tail-in"]);

        w.live("message.created", liveCreated({ id: 2, sender: anna }, 2));
        await settle();
        expect(moving(feed.wrapper)).toEqual(["2: chat-msg is-conversational is-message chat-tail-in"]);
    });

    test("a message heard while the first page was still on its way is drawn with that page, not risen", async () => {
        const w = wire();
        const feed = drawFeed(w);
        w.live("message.created", liveCreated({ id: 2, sender: anna }, 2));
        await open(w, [pageRow({ id: 1, sender: anna })]);
        await settle();

        expect(feed.wrapper.find("[data-message-id='2']").exists()).toBe(true);
        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("the reader's own reaction heard while the first page was still on its way is part of that page, recorded as nothing", async () => {
        const w = wire();
        const feed = drawFeed(w);
        // heard after the page was read and before it landed, so held and laid over it
        w.pages.push(page([pageRow({ id: 1, sender: anna })]));
        const loading = w.timelines.loadNewest(channelId);
        w.live("reaction.added", { message_id: 1, channel_id: channelId, person_alias_guid: me, ...heart, count: 1, reactors: [] });
        await loading;
        await settle();

        expect(feed.wrapper.find("[data-message-id='1'] [data-testid='reaction-chip']").exists()).toBe(true);
        expect(w.motion.of("k:1")).toBeUndefined();
        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("a reply heard while the thread's first page was still on its way is drawn with that page, not risen", async () => {
        const w = wire();
        const root = pageRow({ id: 40, sender: anna }, { reply_count: 1, participant_count: 1 });
        await open(w, [root]);
        const firstPage = deferred<Record<string, unknown>>();
        w.threadPages.push(firstPage.promise);
        const opening = w.threads.openThread(channelId, 40);
        // the drawer as the shell draws it, from the moment the thread is asked, loading until its page lands
        const Host = defineComponent({
            setup: () => () => w.threads.state.thread
                ? h(ThreadDrawer, { root: w.threads.state.thread.root, replies: w.threads.state.thread.replies, isLoading: w.threads.state.thread.isLoading, pending: [], isFollowing: true, isPhone: false, isFromList: false, personAliasGuid: me, conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true, draft: { text: "", mentions: [] } })
                : null
        });
        const drawer = mount(Host, { attachTo: document.body, global: { provide: w.provide } });
        await settle();

        w.live("message.created", liveCreated({ id: 42, sender: anna, parentId: 40 }, 2));
        await settle();
        firstPage.resolve({ root, messages: [pageRow({ id: 41, sender: anna, parentId: 40 })], has_more: false, following: true, read_cursor: 41, unread_count: 0, rev: 1 });
        await opening;
        await settle();

        expect(drawer.find("[data-reply-id='42']").exists()).toBe(true);
        expect(moving(drawer)).toEqual([]);
    });

    test("the person's send rises once as it is sent, and its answer and live copy keep it in the one element", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();

        const sending = w.sender.send(channelId, "On my way");
        await settle();
        const drawn = feed.wrapper.get("[data-testid='pending-row']").element;
        expect(((drawn as HTMLElement).dataset.tailPlayed?.includes("chat-tail-in") === true)).toBe(true);

        // stored now, so it is drawn in today's section beside where its row was
        const createdAt = new Date().toISOString();
        w.answers[0].resolve({ ok: true, id: 2, createdAt, notice: null, pieces: null, blocks: null });
        await sending;
        w.live("message.created", liveCreated({ id: 2, key: "k1", body: "On my way", createdAt }, 2));
        await settle();

        expect(feed.wrapper.get("[data-message-id='2']").element).toBe(drawn);
        expect(feed.wrapper.findAll(tailPlayed("chat-tail-in"))).toHaveLength(1);
    });

    test("a reply heard live in the open drawer rises there once", async () => {
        const w = wire();
        const root = pageRow({ id: 40, sender: anna }, { reply_count: 1, participant_count: 1 });
        await open(w, [root]);
        const drawer = await drawThread(w, root, [pageRow({ id: 41, sender: anna, parentId: 40 })]);

        w.live("message.created", liveCreated({ id: 42, sender: anna, parentId: 40 }, 2));
        await settle();

        expect(moving(drawer)).toEqual(["42: chat-msg is-conversational is-message chat-tail-in"]);
    });

    test("the reader's press adds a first chip that pops, and joining others' chip rolls it without a pop", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna }), pageRow({ id: 2, sender: anna }, { reactions: [{ ...heart, count: 1, mine: false }] })]);
        const feed = drawFeed(w);
        await settle();

        void w.reactions.toggle(channelId, 1, heart);
        void w.reactions.toggle(channelId, 2, heart);
        await settle();

        expect(feed.wrapper.get("[data-message-id='1'] [data-testid='reaction-chip']").classes()).toContain("is-popping");
        expect(feed.wrapper.get("[data-message-id='2'] [data-testid='reaction-chip']").classes()).not.toContain("is-popping");
        expect(feed.wrapper.get("[data-message-id='2'] [data-testid='reaction-count']").classes()).toContain("is-rolling");
    });

    test("the platform's answer to the reader's press states the chips again and pops nothing more", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();
        void w.reactions.toggle(channelId, 1, heart);
        await settle();
        const chip = feed.wrapper.get("[data-message-id='1'] [data-testid='reaction-chip']");
        expect(chip.classes()).toContain("is-popping");
        chip.element.dispatchEvent(new Event("animationend"));
        await nextTick();
        expect(chip.classes()).not.toContain("is-popping");
        const pressed = w.motion.of("k:1")?.reaction?.seq;

        w.reactionAnswers[0].resolve({ ok: true, data: { message_id: 1, reactions: [{ ...heart, count: 1, mine: true, reactors: [] }] } } as CallResult);
        await settle();

        expect(feed.wrapper.findAll("[data-message-id='1'] .is-popping, [data-message-id='1'] .is-rolling")).toHaveLength(0);
        expect(w.motion.of("k:1")?.reaction?.seq).toBe(pressed);
    });

    test("the person's own message sent from another tab rises as it is heard, as anyone's does", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();

        w.live("message.created", liveCreated({ id: 2, key: "c0ffee00-0000-4000-8000-000000000000", body: "From my phone" }, 2));
        await settle();

        expect(feed.wrapper.findAll(tailPlayed("chat-tail-in")).map(row => row.element.closest("[data-message-id]")?.getAttribute("data-message-id"))).toEqual(["2"]);
    });

    test("the reader's own reaction made on another device pops as it is heard; someone else's moves nothing", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna }), pageRow({ id: 2, sender: anna })]);
        const feed = drawFeed(w);
        await settle();

        w.live("reaction.added", { message_id: 1, channel_id: channelId, person_alias_guid: me, ...heart, count: 1, reactors: [] });
        w.live("reaction.added", { message_id: 2, channel_id: channelId, person_alias_guid: anna, ...heart, count: 1, reactors: [] });
        await settle();

        expect(feed.wrapper.get("[data-message-id='1'] [data-testid='reaction-chip']").classes()).toContain("is-popping");
        expect(feed.wrapper.findAll("[data-message-id='2'] .is-popping, [data-message-id='2'] .is-rolling")).toHaveLength(0);
    });

    test("a thread's first reply heard live pops its pill once; its second moves nothing", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 1, sender: anna })]);
        const feed = drawFeed(w);
        await settle();
        const counts = (count: number): Record<string, unknown> => ({ root_id: 1, channel_id: channelId, reply_count: count, last_reply_at: "2026-10-12T14:05:00.000Z", participant_count: 1, repliers: [{ person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null }] });

        w.live("thread.updated", { ...counts(1), rev: 2 });
        await settle();
        expect(feed.wrapper.get("[data-message-id='1'] .chat-tsum").classes()).toContain("is-popping");
        const started = w.motion.of("k:1")?.thread?.seq;

        w.live("thread.updated", { ...counts(2), rev: 3 });
        await settle();
        expect(w.motion.of("k:1")?.thread?.seq).toBe(started);
    });
});

describe("the thread drawer's reread plays nothing", () => {
    test("replies a repair reads again, above the newest when the thread was placed, do not rise", async () => {
        const w = wire();
        await open(w, [pageRow({ id: 40, sender: anna }, { reply_count: 1 })]);
        const reply = (id: number): TimelineMessage => pageRow({ id, sender: anna, parentId: 40 });
        const root = pageRow({ id: 40, sender: anna }, { reply_count: 1 });
        w.threadPages.push({ root, messages: [reply(41)], has_more: false, following: true, read_cursor: 41, unread_count: 0, rev: 1 });
        await w.threads.openThread(channelId, 40);
        const Host = defineComponent({
            setup: () => () => w.threads.state.thread?.root
                ? h(ThreadDrawer, { root: w.threads.state.thread.root, replies: w.threads.state.thread.replies, pending: [], isFollowing: true, isPhone: false, isFromList: false, personAliasGuid: me, conversationTitle: "Anna Reyes", isChannel: false, isDirectMessage: true, draft: { text: "", mentions: [] } })
                : null
        });
        const drawer = mount(Host, { attachTo: document.body, global: { provide: w.provide } });
        await settle();

        w.threadPages.push({ root: { ...root, reply_count: 3 }, messages: [reply(41), reply(42), reply(43)], has_more: false, following: true, read_cursor: 41, unread_count: 2, rev: 3 });
        w.threads.repair(channelId);
        await settle();

        expect(drawer.find("[data-reply-id='43']").exists()).toBe(true);
        expect(moving(drawer)).toEqual([]);
    });
});

// The guard against the class coming back: a feed and an open thread over every state a row can
// carry, some held as heard live and some as a page states them, then every way state is written
// without anything happening. Any motion inferred from comparing what a row was handed across
// renders, on any field, shows here without anyone having to name the field.
describe("the quiet sweep", () => {
    test("a re-read, the drawer's reread, a newest page that replaces what is held, a catch-up, a block, an unblock and a remount play no motion anywhere", async () => {
        const w = wire();
        const replied = { reply_count: 2, last_reply_at: "2026-10-12T14:05:00.000Z", participant_count: 1, repliers: [{ person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null }] };
        const held = [
            pageRow({ id: 1, sender: anna }, { edited_at: "2026-10-12T14:02:00.000Z" }),
            pageRow({ id: 2, sender: anna }, { deleted_at: "2026-10-12T14:02:00.000Z", body: null }),
            pageRow({ id: 3, sender: anna }, { visibility: "hidden", visibility_notice: "Hidden by a moderator", body: null }),
            pageRow({ id: 4, sender: anna }, { reactions: [{ ...heart, count: 2, mine: true }] }),
            pageRow({ id: 5, sender: anna }, replied),
            pageRow({ id: 6, sender: anna }, { blocked: true, body: null, sender_nick_name: null, sender_last_name: null })
        ];
        await open(w, held);
        const feed = drawFeed(w);
        const reply = (id: number, extra: Partial<TimelineMessage> = {}): TimelineMessage => pageRow({ id, sender: anna, parentId: 5 }, extra);
        const drawer = await drawThread(w, held[4], [reply(51), reply(52)]);
        // heard live: someone's message, the person's own send answered, a send still out, a reply
        // in the open thread and the reader's own reaction to it from another device
        w.live("message.created", liveCreated({ id: 7, sender: anna }));
        const sending = w.sender.send(channelId, "On my way");
        await settle();
        w.answers[0].resolve({ ok: true, id: 8, createdAt: "2026-10-12T14:08:00.000Z", notice: null, pieces: null, blocks: null });
        await sending;
        void w.sender.send(channelId, "Still going");
        w.live("message.created", liveCreated({ id: 53, sender: anna, parentId: 5 }));
        await settle();
        w.live("reaction.added", { message_id: 53, channel_id: channelId, person_alias_guid: me, ...heart, count: 1, reactors: [] });
        await settle();
        expect(drawer.get("[data-reply-id='53'] [data-testid='reaction-chip']").classes()).toContain("is-popping");
        await endMotion(feed.wrapper);
        await endMotion(drawer);
        const readAgain = (): HistoryPage => page([...held, pageRow({ id: 7, sender: anna }), pageRow({ id: 8, key: "k1", body: "On my way" })], 2);
        const motion = motionSince(feed.wrapper, drawer);

        // a re-read of the room while it is open, as a reconnect's catch-up reads it
        await w.timelines.onJoined(channelId);
        w.pages.push(readAgain());
        await w.timelines.onJoined(channelId);
        await settle();
        expect(motion.added()).toEqual([]);

        // the drawer's reread, as a repair reads it, stating as a page does what was heard live
        w.threadPages.push({ root: { ...held[4], reply_count: 3 }, messages: [reply(51), reply(52), reply(53, { reactions: [{ ...heart, count: 1, mine: true }] })], has_more: false, following: true, read_cursor: 53, unread_count: 0, rev: 2 });
        w.threads.repair(channelId);
        await settle();
        expect(drawer.find("[data-reply-id='53'] [data-testid='reaction-chip']").exists()).toBe(true);
        expect(motion.added()).toEqual([]);

        // a newest page that does not join what is held, which replaces it in one step, and brings
        // messages the reader has not seen
        w.pages.push({ ...page([pageRow({ id: 9, sender: anna }), pageRow({ id: 10, sender: anna })], 3), has_more: true });
        await w.timelines.loadNewest(channelId);
        await settle();
        expect(feed.wrapper.find("[data-message-id='10']").exists()).toBe(true);
        expect(motion.added()).toEqual([]);

        // a block of someone whose message is on screen folds it into a line, and two more heard
        // while blocked join that line
        w.live("message.created", liveCreated({ id: 11, sender: dee }));
        await settle();
        motion.mark();
        w.blocks.setAliases([dee]);
        await settle();
        w.live("message.created", liveCreated({ id: 12, sender: dee }));
        w.live("message.created", liveCreated({ id: 13, sender: dee }));
        await settle();
        expect(feed.wrapper.findAll("[data-message-id='13']")).toHaveLength(1);
        expect(motion.added()).toEqual([]);

        // an unblock, and the room read again with their words: the line's messages are drawn
        // one by one, and none of them rises as if heard now
        w.blocks.setAliases([]);
        const unblocked = page([pageRow({ id: 9, sender: anna }), pageRow({ id: 10, sender: anna }), pageRow({ id: 11, sender: dee }), pageRow({ id: 12, sender: dee }), pageRow({ id: 13, sender: dee })], 4);
        w.pages.push(unblocked, unblocked, unblocked);
        await w.timelines.onJoined(channelId);
        await settle();
        expect(feed.wrapper.get("[data-message-id='13']").text()).toContain("m13");
        expect(feed.wrapper.get("[data-message-id='12']").text()).toContain("m12");
        expect(motion.added()).toEqual([]);

        // a switch away and back, the page read again
        await feed.switchAwayAndBack(page([pageRow({ id: 9, sender: anna }), pageRow({ id: 10, sender: anna }, { edited_at: "2026-10-12T14:09:00.000Z", reactions: [{ ...heart, count: 1, mine: true }] })], 5));
        expect(moving(feed.wrapper)).toEqual([]);
    });

    test("an unblock in the open thread draws again the replies its line folded, and none of them rises", async () => {
        const w = wire();
        const root = pageRow({ id: 40, sender: anna }, { reply_count: 1, participant_count: 1 });
        await open(w, [root]);
        const drawer = await drawThread(w, root, [pageRow({ id: 41, sender: anna, parentId: 40 })]);
        w.blocks.setAliases([dee]);
        w.live("message.created", liveCreated({ id: 42, sender: dee, parentId: 40 }));
        w.live("message.created", liveCreated({ id: 43, sender: dee, parentId: 40 }));
        await settle();
        await endMotion(drawer);
        const motion = motionSince(drawer);

        w.blocks.setAliases([]);
        w.threadPages.push({ root: { ...root, reply_count: 3 }, messages: [41, 42, 43].map(id => pageRow({ id, sender: id === 41 ? anna : dee, parentId: 40 })), has_more: false, following: true, read_cursor: 43, unread_count: 0, rev: 2 });
        w.threads.repair(channelId);
        await settle();

        expect(drawer.get("[data-reply-id='43']").text()).toContain("m43");
        expect(motion.added()).toEqual([]);
    });
});
