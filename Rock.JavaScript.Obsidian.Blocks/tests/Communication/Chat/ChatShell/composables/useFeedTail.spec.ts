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
// A view's tail, the one owner of how the newest end of a conversation or a thread moves: a
// message arriving glides the stack as it fades in, someone typing comes in and slides out with the
// stack, and her message grows out of the bubble it ends, each from where things stand on screen,
// each once, and only for what the record says happened while the view was open. Scrolled up
// nothing moves; under reduced motion nothing slides or grows; what the open's first read changes,
// inside the view's entrance or after it, glides in over a message's time through the same plan.
//
// Drawn in a host of its own over the real typing bubble, on a stage that lays the view out and
// moves its boxes as the browser does, so each cell reads what a person sees frame by frame: where
// each box stands on screen and how strongly it shows. Every motion runs linear here.
import { readFileSync } from "fs";
import { join } from "path";
import { compileStyle, parse } from "@vue/compiler-sfc";
import { flushPromises, mount } from "@vue/test-utils";
import { defineComponent, h, nextTick, reactive, ref, watch } from "vue";
import TypingBubble from "../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs";
import { useFeedTail } from "../../../../../src/Communication/Chat/ChatShell/composables/useFeedTail.partial";
import { endOf } from "../../../../../src/Communication/Chat/ChatShell/composables/useNewestEnd.partial";
import type { Typing } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import { createMessageMotion, MessageMotion, messageMotionKey, useViewArrivals } from "../../../../../src/Communication/Chat/ChatShell/messageMotion.partial";
import { createStage, Played, Stage } from "../tailStage";

const anna = "a0000002-0000-4000-8000-000000000000";
const ted = "a0000003-0000-4000-8000-000000000000";
const marcus = "a0000004-0000-4000-8000-000000000000";
const me = "me";
const place = "c1";

const person = (alias: string, nick: string): Typing["typists"][number] => ({ alias, person: { person_alias_guid: alias, nick_name: nick, last_name: "Reyes", avatar_url: null } });
const typingBy = (...typists: Typing["typists"]): Typing => ({ typists, names: typists.map(typist => typist.person?.nick_name).join(" and "), sentence: "typing…" });
const annaTyping = typingBy(person(anna, "Anna"));
const tedTyping = typingBy(person(ted, "Ted"));

/** The motion tokens, linear, so every sampled frame is exact interpolation. */
const msg = 260;
const base = 180;
const fast = 120;
const tokens = `--chat-t-msg: ${msg}ms; --chat-t-base: ${base}ms; --chat-t-fast: ${fast}ms; --chat-ease-in: linear; --chat-ease-out: linear; --chat-ease-state: linear; position: relative`;

/** The scroller on screen: its top under a header, its height, its padding. */
const scrollerTop = 100;
const scrollerHeight = 500;
const padTop = 10;
const padBottom = 18;

/** A row: its key, who sent it, its bubble's size, and its files' box when it has no text. */
type Row = { key: string, sender: string, h?: number, w?: number, filesOnly?: boolean, isNewGroup?: boolean, isFull?: boolean, isEdited?: boolean, isReacted?: boolean };

/** A row group: its key is its first row's, as the feed's layout keys a group. */
type Group = { key: string, sender: string, rows: Row[] };

function groupsOf(rows: Row[]): Group[] {
    const groups: Group[] = [];
    for (const row of rows) {
        const last = groups[groups.length - 1];
        if (last && last.sender === row.sender && !row.isNewGroup) {
            last.rows.push(row);
        }
        else {
            groups.push({ key: row.key, sender: row.sender, rows: [row] });
        }
    }
    return groups;
}

/**
 * A view over the real record and the real bubble, drawing each row as the feed does, with the
 * marks the tail reads: a group's face and name line by the group's key, each row's box and its
 * time line as its row's, the time line kept and hidden when its row does not show it; the face
 * also carries the person mark the feed drew before.
 */
function draw(motion: MessageMotion, stageOf: (scroller: () => HTMLElement | null, content: () => HTMLElement | null) => Stage, options: { rows?: Row[], typing?: Typing | null, isInRun?: boolean, isGroup?: boolean, isFollowing?: boolean, ownFollow?: "before" | "after", isScrollerless?: boolean, isLanded?: boolean, isLandedByRead?: boolean } = {}) {
    const rows = reactive<Row[]>([...(options.rows ?? [])]);
    // whether the view's open has drawn its first read: a view drawn here has, unless a cell opens it
    const landed = ref(options.isLanded ?? true);
    const typing = ref<Typing | null>(options.typing ?? null);
    const isInRun = ref(options.isInRun ?? false);
    const following = ref(options.isFollowing ?? true);
    const follows = { count: 0 };
    // each time the tail reads where it stands, as it does once for every change it is woken for
    const reads = { count: 0 };
    const scroller = ref<HTMLElement | null>(null);
    const content = ref<HTMLElement | null>(null);
    const stage = stageOf(() => scroller.value, () => content.value);
    const follow = (): void => {
        follows.count++;
        if (scroller.value && content.value) {
            scroller.value.scrollTop = endOf(scroller.value, content.value);
        }
    };

    const View = defineComponent({
        setup() {
            const { arrives } = useViewArrivals(() => new Map());
            // the view's own follow of its newest row, which runs on the render after the tail's or before it
            const ownFollow = (): void => {
                watch(() => rows.length, () => void nextTick(follow));
            };
            if (options.ownFollow === "before") {
                ownFollow();
            }
            const tail = {
                content,
                // the thread's drawer hands its tail no scroller
                scroller: () => options.isScrollerless ? null : scroller.value,
                place: () => place,
                isFollowing: () => {
                    reads.count++;
                    return following.value;
                },
                follow,
                arrives,
                drawn: () => [typing.value, rows.length, isInRun.value, rows.map(row => row.h).join()],
                landed: () => landed.value
            };
            useFeedTail(tail);
            // a view that marks its open landed as it draws what its first read brought, in the same render
            if (options.isLandedByRead) {
                watch(typing, () => {
                    landed.value = true;
                });
            }
            if (options.ownFollow === "after") {
                ownFollow();
            }

            return () => {
                const groups = groupsOf(rows);
                const newest = groups[groups.length - 1];
                return h("div", { ref: scroller, "data-testid": "tail-scroller", style: `position: relative; padding: ${padTop}px 0 ${padBottom}px` }, [
                    h("div", { ref: content, style: tokens, "data-testid": "tail-content" }, [
                        h("div", { class: "chat-feed-fill" }),
                        ...groups.flatMap(group => group.rows.map((row, index) => {
                            const isOwn = group.sender === me;
                            const inRunNext = isInRun.value && group === newest && !!typing.value;
                            const isLast = index === group.rows.length - 1 && !inRunNext;
                            const isMetaShown = index === group.rows.length - 1;
                            return h("div", { key: row.key, class: ["chat-msg", { "is-head": index === 0 }], "data-row-key": row.key }, [
                                options.isGroup && index === 0 && !isOwn ? h("div", { class: "chat-msg-name", "data-tail": `name:${group.key}` }, group.sender) : null,
                                // a message with everything a row can carry: the thread label, a quote, files beside its text, reactions
                                row.isFull ? h("button", { class: "chat-thread-label" }, "Replied to a thread") : null,
                                row.isFull ? h("div", { class: "chat-quote" }, "quoted") : null,
                                h("div", { class: "chat-msg-line" }, [
                                    // the face column stretches to its line's height, its face hanging from the foot
                                    isOwn ? null : h("span", { class: "chat-msg-face", "data-tail": isLast ? `face:${group.key}` : undefined, "data-tail-face": isLast ? group.sender : undefined, "data-h": Math.max(34, row.h ?? (row.filesOnly ? 120 : 40)) }),
                                    h("div", { class: "chat-bub-anchor" }, row.filesOnly
                                        ? [h("div", { class: "chat-msg-files", "data-tail": "box", "data-w": row.w ?? 160, "data-h": row.h ?? 120 })]
                                        : [
                                            h("div", { class: "chat-bubble", "data-tail": "box", "data-w": row.w ?? 200, "data-h": row.h ?? 40 }, [h("span", { class: "chat-words" }, row.key)]),
                                            row.isFull ? h("div", { class: "chat-msg-files", "data-w": 160, "data-h": 120 }) : null
                                        ])
                                ]),
                                row.isFull || row.isReacted ? h("div", { class: "chat-reacts" }, "👍 1") : null,
                                // an edited message's time line keeps its time and says Edited alone when its row gives the time up
                                h("div", { class: "chat-meta", "data-tail": "meta", hidden: !isMetaShown && !row.isEdited }, row.isEdited
                                    ? [
                                        h("span", { class: "chat-meta-stamp", "data-tail": "stamp", hidden: !isMetaShown }, [h("span", { class: "chat-meta-time" }, "2:15 PM"), h("span", { class: "chat-meta-dot" }, "·")]),
                                        h("span", { class: "chat-meta-edited", "data-tail": "edited" }, "Edited")
                                    ]
                                    : [h("span", { class: "chat-meta-time" }, "2:15 PM")])
                            ]);
                        })),
                        h(TypingBubble, { typing: typing.value, isGroup: options.isGroup ?? false, isInRun: isInRun.value, groupKey: isInRun.value ? newest?.key : "typing" })
                    ])
                ]);
            };
        }
    });

    // the bubble's real Transitions, which keep a leaving part while its tail plays it, as a browser does
    const wrapper = mount(View, { attachTo: document.body, global: { stubs: { transition: false }, provide: { [messageMotionKey as symbol]: motion } } });
    follow();
    follows.count = 0;
    return { wrapper, rows, typing, isInRun, following, landed, follows, reads, stage, scroller, content, endOf };
}

type View = ReturnType<typeof draw>;

/** How the view lays out, as the chat's sheet draws it. */
const sizes: [string, { w?: number, h?: number, mt?: number, pl?: number, line?: number }][] = [
    [".chat-msg.is-head", { mt: 12, pl: 20 }],
    [".chat-msg", { mt: 2, pl: 20 }],
    [".chat-msg-name, .chat-typing-names", { h: 18 }],
    [".chat-msg-line, .chat-typing-line", { line: 8 }],
    [".chat-msg-face, .chat-typing-face", { w: 34, h: 34 }],
    [".chat-meta", { line: 6, pl: 34 }],
    [".chat-meta-stamp", { line: 6 }],
    [".chat-meta-time", { w: 45, h: 16 }],
    [".chat-meta-dot", { w: 4, h: 16 }],
    [".chat-meta-edited", { w: 40, h: 16 }],
    [".chat-thread-label, .chat-reacts", { h: 16 }],
    [".chat-quote", { h: 30 }],
    [".chat-typing.is-in-run", { mt: 2, pl: 20 }],
    [".chat-typing", { mt: 12, pl: 20 }],
    [".chat-typing-bubble", { w: 60, h: 34 }],
    [".chat-sr-only", { w: 0, h: 0 }]
];

function stageFor(scroller: () => HTMLElement | null, content: () => HTMLElement | null): Stage {
    return createStage({
        scroller,
        content,
        top: scrollerTop,
        height: scrollerHeight,
        padTop,
        padBottom,
        sizes,
        positioned: ".chat-msg, .chat-typing",
        fill: ".chat-feed-fill",
        hidden: ".chat-meta[hidden], .chat-meta-stamp[hidden]"
    });
}

let stage: Stage | null = null;

function view(motion: MessageMotion, options: Parameters<typeof draw>[2] = {}): View {
    const drawn = draw(motion, (scroller, content) => {
        stage = stageFor(scroller, content);
        return stage;
    }, options);
    return drawn;
}

/** The render and the tail after it. */
async function settle(): Promise<void> {
    await flushPromises();
    await nextTick();
    await nextTick();
}

/** Her message, as a holder hands it to the record. */
const message = (id: number): { id: number, client_key: null } => ({ id, client_key: null });

function setReducedMotion(isReduced: boolean): void {
    window.matchMedia = ((query: string) => ({ matches: isReduced && query.includes("reduce"), media: query, addEventListener: () => undefined, removeEventListener: () => undefined })) as unknown as typeof window.matchMedia;
}

/** A part by its mark, the one standing in the flow rather than one going; a row's own time line and box by their row. */
function part(v: View, id: string): HTMLElement {
    const own = /^(meta|box):(.*)$/.exec(id);
    if (own) {
        return row(v, own[2]).querySelector<HTMLElement>(`[data-tail='${own[1]}']`) as HTMLElement;
    }
    const all = [...v.wrapper.element.querySelectorAll<HTMLElement>(`[data-tail='${id}']`)];
    const found = all.find(element => element.style.position !== "absolute" && !element.closest("[data-testid='typing-bubble'][style*='absolute']")) ?? all[0];
    if (!found) {
        throw new Error(`no part ${id}`);
    }
    return found;
}

const row = (v: View, key: string): HTMLElement => v.wrapper.get(`[data-row-key='${key}']`).element as HTMLElement;
const bubble = (v: View): HTMLElement | null => v.wrapper.element.querySelector<HTMLElement>("[data-testid='typing-bubble']");
const bubbles = (v: View): HTMLElement[] => [...v.wrapper.element.querySelectorAll<HTMLElement>("[data-testid='typing-bubble']")];
const dots = (v: View): HTMLElement => v.wrapper.get("[data-testid='typing-dots']").element as HTMLElement;
const bubbleFace = (v: View): HTMLElement => v.wrapper.get(".chat-typing-face").element as HTMLElement;

/** The frames a cell samples, in milliseconds from the change. */
const frames = [0, 16, 26, 52, 65, 91, 104, 117, 130, 156, 182, 195, 234, 259];

/** Reads each sampled frame from the change on, the clock moved to each in turn. */
function sample<T>(read: (t: number) => T, at: number[] = frames): T[] {
    const start = (stage as Stage).now();
    return at.map(t => {
        (stage as Stage).advance(start + t - (stage as Stage).now());
        return read(t);
    });
}

const seen = (element: Element): DOMRect => (stage as Stage).seenBox(element);
const opacity = (element: Element): number => (stage as Stage).seenOpacity(element);
const corner = (box: DOMRect): { left: number, bottom: number } => ({ left: Math.round(box.left * 10) / 10, bottom: Math.round(box.bottom * 10) / 10 });
const near = (a: number, b: number, within = 1): boolean => Math.abs(a - b) <= within;

/** Each motion played, by its name and what it played on. */
function named(played: Played[]): string[] {
    return played.map(motion => `${motion.id} ${motion.element.dataset.rowKey ?? motion.element.dataset.tail ?? motion.element.dataset.testid ?? motion.element.className}`);
}

/** The rows that faded in, each once: on the row, or on its parts around the run's face moving into it. */
function fadedIn(played: Played[]): string[] {
    return [...new Set(played.filter(motion => motion.id === "chat-tail-in").map(motion => motion.element.closest<HTMLElement>("[data-row-key]")?.dataset.rowKey ?? ""))].filter(key => !!key);
}

/** Her message heard live, recorded as the live router does: the arrival, then the typing's end naming it. */
function sends(motion: MessageMotion, v: View, id: number, sender = anna, extra: Partial<Row> = {}): void {
    motion.arrived(message(id), place);
    motion.typingEnded(place, "sent", message(id));
    v.rows.push({ key: `k:${id}`, sender, ...extra });
    v.typing.value = null;
    v.isInRun.value = false;
}

const realStyle = window.getComputedStyle;

beforeEach(() => {
    setReducedMotion(false);
});

afterEach(() => {
    stage?.restore();
    stage = null;
    window.getComputedStyle = realStyle;
    document.body.innerHTML = "";
});

describe("a message arriving", () => {
    test("glides the stack up from where it stood by the new row's height, the new row fading in riding it, once", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }, { key: "k:2", sender: anna }] });
        const top = seen(row(v, "k:2")).top;

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();

        const s = stage as Stage;
        expect(named(s.played).filter(name => name.includes("tail-content"))).toEqual(["chat-tail-glide tail-content"]);
        expect(fadedIn(s.played)).toEqual(["k:3"]);
        expect(v.follows.count).toBeGreaterThan(0);
        // the row that stood stays where it stood at the change's first frame, then rises by the new row's room
        expect(near(seen(row(v, "k:2")).top, top)).toBe(true);
        sample(() => undefined, [msg]);
        expect(near(seen(row(v, "k:2")).top, top - 42)).toBe(true);

        // the same message heard again, and a page restating it, move nothing more
        s.played.length = 0;
        motion.arrived(message(3), place);
        await settle();
        expect(s.played).toEqual([]);
    });

    test("several in one change glide once, each new row fading in", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }] });

        motion.arrived(message(2), place);
        motion.arrived(message(3), place);
        v.rows.push({ key: "k:2", sender: anna }, { key: "k:3", sender: anna });
        await settle();

        expect(named((stage as Stage).played).filter(name => name.includes("tail-content"))).toEqual(["chat-tail-glide tail-content"]);
        expect(fadedIn((stage as Stage).played)).toEqual(["k:2", "k:3"]);
    });

    test("scrolled up nothing moves, and the row is claimed, so it never fades in later", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }], isFollowing: false });

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: anna });
        await settle();
        expect((stage as Stage).played).toEqual([]);
        expect(v.follows.count).toBe(0);

        v.following.value = true;
        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();
        expect(fadedIn((stage as Stage).played)).toEqual(["k:3"]);
    });

    test("a row a page, a catch-up or a switch back brings, with nothing recorded, is simply there", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }] });

        v.rows.push({ key: "k:2", sender: ted });
        await settle();

        expect((stage as Stage).played).toEqual([]);
    });

    test("a view opened after a message arrived plays nothing for it", async () => {
        const motion = createMessageMotion();
        motion.arrived(message(2), place);
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }] });

        v.rows.push({ key: "k:2", sender: ted });
        await settle();

        expect((stage as Stage).played).toEqual([]);
    });

    test("a view opened after typing started replays nothing for it", async () => {
        const motion = createMessageMotion();
        motion.typingStarted(place);
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }] });

        v.typing.value = annaTyping;
        await settle();

        expect((stage as Stage).played).toEqual([]);
    });

    test("under reduced motion the new row fades in and nothing slides", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }] });

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: ted });
        await settle();

        expect(named((stage as Stage).played)).toEqual(["chat-tail-in k:2"]);
        expect((stage as Stage).played.every(motion => motion.keyframes.every(frame => frame.transform === undefined))).toBe(true);
    });

    test("a wakeup with nothing to play reads no style", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }] });
        const reads = jest.fn(realStyle);
        window.getComputedStyle = reads;

        v.rows.push({ key: "k:2", sender: ted });
        await settle();

        expect(reads).not.toHaveBeenCalled();
    });

    // A fold or a reaction row changing size above in the same render must not move where the stack starts.
    test("a row above changing height in the same change leaves the newest row that stands where it stood", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }, { key: "k:3", sender: anna }] });
        const top = seen(row(v, "k:3")).top;

        motion.arrived(message(4), place);
        v.rows[0].h = 80;
        v.rows.push({ key: "k:4", sender: anna });
        await settle();

        expect(seen(row(v, "k:3")).top).toBeCloseTo(top, 0);
    });

    // A row that steps, as the rows above one changing height do, steps whole: its face hangs from
    // its foot in every frame, never gliding in behind it.
    test("a row stepping as one above it changes height takes its face with it in every frame", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }, { key: "k:3", sender: ted }] });

        motion.arrived(message(4), place);
        v.rows[1].h = 80;
        v.rows.push({ key: "k:4", sender: me });
        await settle();

        const gaps = sample(() => Math.round(seen(row(v, "k:1")).bottom - seen(part(v, "face:k:1")).bottom));
        expect(new Set(gaps).size).toBe(1);
    });

    test.each(["before", "after"] as const)("the view's own follow running %s the tail's lands on the laid-out end, and every frame is the same", async order => {
        const run = async (ownFollow: "before" | "after" | undefined): Promise<{ scroll: number[], tops: number[] }> => {
            const motion = createMessageMotion();
            const rows = Array.from({ length: 14 }, (_, index) => ({ key: `k:${index + 1}`, sender: index % 3 === 0 ? ted : anna }));
            const v = view(motion, { rows, ownFollow });
            motion.arrived(message(15), place);
            v.rows.push({ key: "k:15", sender: anna });
            await settle();
            const end = (): number => v.content.value!.offsetTop + v.content.value!.offsetHeight + padBottom - scrollerHeight;
            const result = sample(() => ({ scroll: v.scroller.value!.scrollTop - end(), top: seen(row(v, "k:14")).top }));
            stage?.restore();
            document.body.innerHTML = "";
            return { scroll: result.map(frame => Math.round(frame.scroll)), tops: result.map(frame => Math.round(frame.top * 10) / 10) };
        };

        const alone = await run(undefined);
        const both = await run(order);

        expect(both.scroll.every(value => value === 0)).toBe(true);
        expect(both.tops).toEqual(alone.tops);
    });
});

describe("someone typing", () => {
    test("coming in glides the stack up by the bubble's room, its dots fading in riding it", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }] });
        const top = seen(row(v, "k:1")).top;

        motion.typingStarted(place);
        v.typing.value = annaTyping;
        await settle();

        expect(near(seen(row(v, "k:1")).top, top)).toBe(true);
        const shown = sample(() => ({ dots: opacity(dots(v)), top: seen(row(v, "k:1")).top }), [0, msg / 2, msg]);
        expect(shown.map(frame => Math.round(frame.dots * 100) / 100)).toEqual([0, .5, 1]);
        expect(near(shown[2].top, top - 46)).toBe(true);
    });

    test("drawn as what is, from a page's answer or a switch back, it comes with no motion", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }] });

        v.typing.value = annaTyping;
        await settle();

        expect((stage as Stage).played).toEqual([]);
    });

    test("continuing her run, her face moves from her last row to the bubble at full strength, and back as it goes", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }] });
        const face = seen(part(v, "face:k:2"));

        motion.typingStarted(place);
        v.typing.value = annaTyping;
        v.isInRun.value = true;
        await settle();

        const coming = sample(() => ({ box: seen(bubbleFace(v)), seen: opacity(bubbleFace(v)) }));
        expect(corner(coming[0].box)).toEqual(corner(face));
        expect(coming.every(frame => frame.seen > .99)).toBe(true);

        const atBubble = seen(bubbleFace(v));
        motion.typingEnded(place, "stopped");
        v.typing.value = null;
        v.isInRun.value = false;
        await settle();

        const going = sample(() => ({ box: seen(part(v, "face:k:2")), seen: opacity(part(v, "face:k:2")) }), [0, 60, 120, 179]);
        expect(corner(going[0].box)).toEqual(corner(atBubble));
        expect(going.every(frame => frame.seen > .99)).toBe(true);
        // the bubble going shows no face of its own while hers moves back
        expect(bubbles(v).every(going => opacity(going.querySelector(".chat-typing-face") as Element) === 0)).toBe(true);
    });

    test("stopping, the bubble goes down out under the edge with the stack where it stood, and goes once it has faded", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        const was = seen(dots(v));

        motion.typingEnded(place, "stopped");
        v.typing.value = null;
        await settle();

        const going = bubble(v) as HTMLElement;
        expect(going.style.position).toBe("absolute");
        const tops = sample(() => seen(going.querySelector("[data-testid='typing-dots']") as Element).top, [0, 45, 90, 135, 179]);
        expect(near(tops[0], was.top)).toBe(true);
        expect(tops.every((top, index) => index === 0 || top >= tops[index - 1] - .01)).toBe(true);
        (stage as Stage).advance(2);
        await nextTick();
        expect(bubble(v)).toBeNull();
    });

    // A short conversation stands at the foot by the fill: as the bubble's room closes the rows move inside the content.
    test("walked away in a short conversation, the bubble starts where it stood and only goes down", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }], typing: annaTyping });
        const was = seen(dots(v));

        motion.typingEnded(place, "expired");
        v.typing.value = null;
        await settle();

        const going = bubble(v) as HTMLElement;
        const tops = sample(() => seen(going.querySelector("[data-testid='typing-dots']") as Element).top, [0, 45, 90, 135, 179]);
        expect(near(tops[0], was.top)).toBe(true);
        expect(tops.every((top, index) => index === 0 || top >= tops[index - 1] - .01)).toBe(true);
    });

    test("overtaken by a message a page brought, it goes at once with nothing moving", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });

        motion.typingEnded(place, "overtaken");
        v.typing.value = null;
        await settle();

        expect((stage as Stage).played).toEqual([]);
        expect(bubble(v)).toBeNull();
    });

    test("under reduced motion it fades where it stands, and its room closes once it has", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });

        motion.typingEnded(place, "expired");
        v.typing.value = null;
        await settle();

        expect(named((stage as Stage).played)).toEqual(["chat-tail-out typing:typing"]);
        expect((bubble(v) as HTMLElement).style.position).toBe("");
        (stage as Stage).advance(fast);
        await nextTick();
        expect(bubble(v)).toBeNull();
    });

    test("a message landing as the bubble goes takes the going bubble away in the frame her row comes: one box", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        motion.typingEnded(place, "stopped");
        v.typing.value = null;
        await settle();
        sample(() => undefined, [60]);

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: anna });
        await settle();

        expect(bubbles(v)).toHaveLength(0);
    });

    test("typing again as the bubble goes draws one bubble", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        motion.typingEnded(place, "stopped");
        v.typing.value = null;
        await settle();
        sample(() => undefined, [60]);

        motion.typingStarted(place);
        v.typing.value = annaTyping;
        await settle();

        expect(bubbles(v)).toHaveLength(1);
    });

    test("a reply while the bubble shows rides in above it, and the bubble stands still", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: tedTyping });
        const was = seen(dots(v));

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: me });
        await settle();

        const boxes = sample(() => seen(dots(v)));
        expect(boxes.every(box => near(box.top, was.top) && near(box.left, was.left))).toBe(true);
        expect(named((stage as Stage).played)).toContain("chat-tail-in k:2");
    });

    // A bubble standing at the foot is the foot, as the scroller's edge is: a reply wide enough to
    // reach the dots comes out from under it rather than riding up through it.
    test.each([
        ["a reply wide enough to reach the dots", undefined],
        ["a reply 100 ms into a reply", 100]
    ])("%s comes out from under a bubble that stands: never drawn over its dots or its face", async (_, into) => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: tedTyping });
        /** How far, in pixels down the screen, a reply a person sees is drawn over the dots or the bubble's face. */
        const drawnOver = (): number => Math.max(0, ...v.rows.filter(each => each.sender === me).map(each => {
            const holder = row(v, each.key);
            if (opacity(holder) === 0) {
                return 0;
            }
            // what a person sees of the reply: its box, inside its row's clip
            const clip = seen(holder);
            const box = seen(part(v, `box:${each.key}`));
            const top = Math.max(box.top, clip.top);
            const bottom = Math.min(box.bottom, clip.bottom);
            return Math.max(...[seen(dots(v)), seen(bubbleFace(v))].map(other => Math.min(box.right, other.right) > Math.max(box.left, other.left)
                ? Math.max(0, Math.min(bottom, other.bottom) - Math.max(top, other.top))
                : 0));
        }));

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: me, w: 320 });
        await settle();
        const first = into === undefined ? [] : sample(drawnOver, [0, 16, 52, into]);
        if (into !== undefined) {
            motion.arrived(message(3), place);
            v.rows.push({ key: "k:3", sender: me, w: 320 });
            await settle();
        }

        expect(Math.max(...first, ...sample(drawnOver))).toBeLessThanOrEqual(1);
    });

    // A change cancels the foot with every move, so a clip in flight is replanned from what it
    // shows, as a moved part is: the bubble going a moment after a reply rode in under it lets the
    // reply's foot open from where it stood rather than showing its lower part in one frame.
    test("the bubble going while a reply still comes out from under it: the reply's foot opens from where it stood", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: tedTyping });
        /** How far down the screen a person sees the reply: its box inside its row's clip. */
        const footOf = (): number => Math.min(seen(part(v, "box:k:2")).bottom, seen(row(v, "k:2")).bottom);

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: me, w: 320 });
        await settle();
        const was = sample(footOf, [0, 52, 100]);
        expect(was[2]).toBeLessThan(seen(part(v, "box:k:2")).bottom - 5);

        motion.typingEnded(place, "stopped", null);
        v.typing.value = null;
        await settle();

        const after = sample(footOf, [0, 16, 52, 104, base]);
        expect(Math.abs(after[0] - was[2])).toBeLessThanOrEqual(1);
        const steps = after.slice(1).map((at, index) => Math.abs(at - after[index]));
        expect(Math.max(...steps)).toBeLessThan(20);
    });

    test("one of two typists sending rides in above the bubble, which stands still, its faces crossing", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: me }], typing: typingBy(person(ted, "Ted"), person(marcus, "Marcus")), isGroup: true });
        const was = seen(dots(v));
        const wasFace = seen(bubbleFace(v));

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: ted });
        v.typing.value = typingBy(person(marcus, "Marcus"));
        await settle();

        expect((stage as Stage).played.some(motion => motion.id === "chat-tail-handoff")).toBe(false);
        const shown = sample(() => {
            const faces = [...(bubble(v) as HTMLElement).querySelectorAll(".chat-typing-face")];
            return { dots: seen(dots(v)), faces: faces.map(face => ({ box: seen(face), seen: opacity(face) })) };
        }, [0, 90, base - 1]);
        expect(shown.every(frame => near(frame.dots.top, was.top) && near(frame.dots.left, was.left))).toBe(true);
        // the face of the two going and the face of the one coming cross, both where the face stood
        expect(shown[1].faces).toHaveLength(2);
        expect(shown[1].faces.every(face => near(face.box.top, wasFace.top) && near(face.box.left, wasFace.left))).toBe(true);
        expect(shown[1].faces.map(face => Math.round(face.seen * 10) / 10).sort()).toEqual([.5, .5]);
    });

    // Two names drawn over each other read as neither, so the line that names the two goes before
    // the one that names who is left comes, in the same time a face takes to cross.
    test("one of two typists sending: the name line naming both goes, then the one naming who is left comes, never two drawn at once", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: me }], typing: typingBy(person(ted, "Ted"), person(marcus, "Marcus")), isGroup: true });

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: ted });
        v.typing.value = typingBy(person(marcus, "Marcus"));
        await settle();

        const lines = sample(() => [...(bubble(v) as HTMLElement).querySelectorAll<HTMLElement>(".chat-typing-names")]
            .map(line => ({ text: line.textContent?.trim(), seen: opacity(line) }))
            .filter(line => line.seen > .01), [0, 30, 60, 89, 91, 120, 150, base - 1, base]);
        expect(lines.filter(frame => frame.length > 1)).toEqual([]);
        expect(lines[0]).toEqual([expect.objectContaining({ text: "Ted and Marcus" })]);
        expect(lines[lines.length - 1]).toEqual([{ text: "Marcus", seen: 1 }]);
    });
});

describe("her message ending her typing", () => {
    test("grows out of the dots: her box's corner rides from the dots' corner, the dots riding it until they fade", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        const was = seen(dots(v));

        sends(motion, v, 2);
        await settle();

        const box = part(v, "box:k:2");
        const kept = bubble(v);
        const shown = sample(t => ({ t, box: seen(box), dots: kept ? seen(kept.querySelector("[data-testid='typing-dots']") as Element) : null, seenDots: kept && kept.isConnected ? opacity(kept.querySelector("[data-testid='typing-dots']") as Element) : 0 }));
        expect(corner(shown[0].box)).toEqual(corner(was));
        expect(near(shown[0].box.height, was.height)).toBe(true);
        for (const frame of shown) {
            if (frame.dots && frame.seenDots > 0) {
                expect(near(frame.dots.left, frame.box.left)).toBe(true);
                expect(near(frame.dots.bottom, frame.box.bottom)).toBe(true);
            }
        }
        // the dots fade out over the first 35% of the motion and are gone after
        expect(shown.filter(frame => frame.t < .35 * msg - 1).every(frame => frame.seenDots > 0)).toBe(true);
        expect(shown.filter(frame => frame.t >= .35 * msg).every(frame => frame.seenDots === 0)).toBe(true);
    });

    test("her words show from 30% to 90% of the motion, and her time line rises in from 45%", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });

        sends(motion, v, 2);
        await settle();

        const words = row(v, "k:2").querySelector(".chat-words") as Element;
        const meta = part(v, "meta:k:2");
        const shown = sample(t => ({ t, words: opacity(words), meta: opacity(meta), metaTop: seen(meta).top - seen(row(v, "k:2")).top }));
        expect(shown.filter(frame => frame.t <= .3 * msg).every(frame => frame.words === 0)).toBe(true);
        expect(shown.filter(frame => frame.t >= .9 * msg).every(frame => frame.words === 1)).toBe(true);
        expect(shown.filter(frame => frame.t <= .45 * msg).every(frame => frame.meta === 0)).toBe(true);
        const rising = shown.filter(frame => frame.t > .45 * msg && frame.meta > 0 && frame.meta < 1);
        expect(rising.length).toBeGreaterThan(0);
        const [settled] = sample(() => seen(meta).top - seen(row(v, "k:2")).top, [.45 * msg + base]);
        expect(rising.every(frame => frame.metaTop > settled)).toBe(true);
    });

    test("her face glides from the bubble's face, level with her box's bottom in every frame", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        const was = seen(bubbleFace(v));

        sends(motion, v, 2);
        await settle();

        const face = part(v, "face:k:2");
        const shown = sample(() => ({ face: seen(face), box: seen(part(v, "box:k:2")), seen: opacity(face) }));
        expect(corner(shown[0].face)).toEqual(corner(was));
        expect(shown.every(frame => near(frame.face.bottom, frame.box.bottom) && frame.seen > .99)).toBe(true);
    });

    test("in a group, her name line glides from the bubble's", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping, isGroup: true });
        const was = seen(v.wrapper.get("[data-testid='typing-names']").element);

        sends(motion, v, 2);
        await settle();

        const name = seen(part(v, "name:k:2"));
        expect(corner(name)).toEqual(corner(was));
    });

    test("continuing her run, the time line above fades out where it stood with its row, never at full after the change", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }], typing: annaTyping, isInRun: true });
        const meta = part(v, "meta:k:2");
        const was = seen(meta).top - seen(row(v, "k:2")).top;

        sends(motion, v, 3);
        await settle();

        const shown = sample(t => ({ t, seen: opacity(meta), at: seen(meta).top - seen(row(v, "k:2")).top }), [...frames, fast / 2].sort((a, b) => a - b));
        expect(shown.every(frame => frame.seen < 1 || frame.t === 0)).toBe(true);
        expect(shown.filter(frame => frame.seen > 0).every(frame => near(frame.at, was))).toBe(true);
        // still drawn half way through its fade, so a time line taken away at once fails here
        const half = shown.find(frame => frame.t === fast / 2);
        expect(half?.seen).toBeGreaterThan(0);
        expect(half?.seen).toBeLessThan(1);
        expect(shown.filter(frame => frame.t >= fast).every(frame => frame.seen === 0)).toBe(true);
        // done, it is kept as its row draws it: hidden, out of the flow
        expect(meta.isConnected && meta.hidden && meta.style.position === "").toBe(true);
    });

    test("from a bubble still coming in, her message starts at the strength the dots showed", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }] });
        motion.typingStarted(place);
        v.typing.value = annaTyping;
        await settle();
        sample(() => undefined, [100]);
        const shownDots = opacity(dots(v));

        sends(motion, v, 2);
        await settle();

        expect(opacity(row(v, "k:2"))).toBeCloseTo(shownDots, 1);
    });

    // Only her box grows; everything else of her row with no motion of its own comes in with her
    // words, so nothing of it stands at full beside a box still growing.
    test("a message with a quote, the thread label, files beside its text and reactions: her box grows, the rest fades in with her words", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });

        sends(motion, v, 2, anna, { isFull: true });
        await settle();

        const hers = row(v, "k:2");
        const rest = [".chat-thread-label", ".chat-quote", ".chat-msg-files", ".chat-reacts"].map(selector => hers.querySelector(selector) as HTMLElement);
        const handoff = (stage as Stage).played.filter(motion => motion.id === "chat-tail-handoff");
        expect(handoff.some(motion => motion.element === part(v, "box:k:2") && motion.keyframes.some(frame => frame.clipPath !== undefined))).toBe(true);
        const shown = sample(() => rest.map(opacity), [0, .3 * msg, .9 * msg]);
        expect(shown[0]).toEqual([0, 0, 0, 0]);
        expect(shown[1]).toEqual([0, 0, 0, 0]);
        expect(shown[2]).toEqual([1, 1, 1, 1]);
        // none of them grows or moves: only her box does
        expect(handoff.filter(motion => rest.includes(motion.element)).every(motion => motion.keyframes.every(frame => frame.transform === undefined && frame.clipPath === undefined))).toBe(true);
    });

    test("a message of files alone grows its files box out of the dots", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        const was = seen(dots(v));

        sends(motion, v, 2, anna, { filesOnly: true });
        await settle();

        const files = seen(part(v, "box:k:2"));
        expect(corner(files)).toEqual(corner(was));
        expect(near(files.height, was.height)).toBe(true);
    });

    test("under reduced motion it is one box: the dots fade over her box's corner as her message fades in, nothing moving", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });

        sends(motion, v, 2);
        await settle();

        const kept = bubble(v);
        // her row stands at full, so her face never blinks out; her words fade in under the dots
        expect(opacity(row(v, "k:2"))).toBe(1);
        expect(opacity(part(v, "face:k:2"))).toBe(1);
        expect(opacity(row(v, "k:2").querySelector(".chat-words") as Element)).toBe(0);
        const shown = sample(() => ({ box: seen(part(v, "box:k:2")), dots: kept?.isConnected ? seen(kept.querySelector("[data-testid='typing-dots']") as Element) : null, seenDots: kept?.isConnected ? opacity(kept.querySelector("[data-testid='typing-dots']") as Element) : 0 }), [0, 35, 70, 105, 139]);
        for (const frame of shown) {
            if (frame.dots && frame.seenDots > 0) {
                expect(near(frame.dots.left, frame.box.left)).toBe(true);
                expect(near(frame.dots.bottom, frame.box.bottom)).toBe(true);
            }
        }
        expect((stage as Stage).played.every(motion => motion.keyframes.every(frame => frame.transform === undefined && frame.clipPath === undefined))).toBe(true);
    });
});

    test("under reduced motion, continuing her run, the time line above goes at once: never drawn over what stepped into its place", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }], typing: annaTyping, isInRun: true });
        const meta = part(v, "meta:k:2");

        sends(motion, v, 3);
        await settle();

        expect(opacity(meta)).toBe(0);
        expect((stage as Stage).played.some(motion => motion.element === meta && motion.id === "chat-tail-out")).toBe(false);
    });

    // Two of her groups joining, as clearing the unread line between them does, steps the rows above
    // the newest: a time line its row gives up as it steps goes with it, rather than standing where
    // it was over the row that stepped into its place.
    test("a time line its row gives up as the row steps, not riding the stack, goes at once", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: anna }, { key: "k:2", sender: anna, isNewGroup: true }] });
        const meta = part(v, "meta:k:1");
        expect(opacity(meta)).toBe(1);

        v.rows[1].isNewGroup = false;
        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: me });
        await settle();

        expect(opacity(meta)).toBe(0);
        expect((stage as Stage).played.some(motion => motion.element === meta)).toBe(false);
    });

    // Every row of a run keeps a hidden time line; one hidden before and after a change is no part
    // of it, or each change would lift every one on screen and fade it out from full.
    test("a time line hidden before and after a change is never drawn", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }, { key: "k:3", sender: anna }] });
        const meta = part(v, "meta:k:2");
        expect(meta.hidden).toBe(true);

        motion.arrived(message(4), place);
        v.rows.push({ key: "k:4", sender: anna });
        await settle();

        expect(sample(() => opacity(meta)).every(value => value === 0)).toBe(true);
        expect((stage as Stage).played.some(motion => motion.element === meta)).toBe(false);
        expect(meta.style.position).toBe("");
    });

    test("a time line its row shows again while it fades is back in the flow at once", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }] });
        const meta = part(v, "meta:k:2");
        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();
        sample(() => undefined, [40]);
        expect(meta.style.position).toBe("absolute");

        // her newest goes, so the row above is her group's last again
        v.rows.pop();
        motion.arrived(message(4), place);
        v.rows.push({ key: "k:4", sender: ted });
        await settle();

        expect(meta.hidden).toBe(false);
        expect(meta.style.position).toBe("");
        expect(opacity(meta)).toBe(1);
    });

describe("a change landing mid-motion", () => {
    /** Every part's seen opacity in a frame, by its mark. */
    function strengths(v: View): Map<string, number> {
        const all = new Map<string, number>();
        v.wrapper.element.querySelectorAll<HTMLElement>("[data-row-key], [data-tail]").forEach(element => {
            const holder = element.closest<HTMLElement>("[data-row-key]")?.dataset.rowKey ?? "";
            all.set(element.dataset.rowKey ?? `${element.dataset.tail}:${holder}`, opacity(element));
        });
        return all;
    }

    function isSmooth(before: Map<string, number>[], step: number): boolean {
        return before.every((frame, index) => index === 0 || [...frame].every(([key, value]) => {
            const was = before[index - 1].get(key);
            return was === undefined || (value >= was - .001 && value - was <= step);
        }));
    }

    test("an arrival 100 ms into an arrival continues every fade and every box from where it is", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }] });
        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: anna });
        await settle();
        sample(() => undefined, [100]);
        const top = seen(row(v, "k:2")).top;
        const was = strengths(v);

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: ted });
        await settle();

        expect(near(seen(row(v, "k:2")).top, top)).toBe(true);
        const shown = [was, ...sample(() => strengths(v), [0, 16, 32, 48, 64, 80, 96])];
        // a linear fade over the message's time moves about 16/260 a frame
        expect(isSmooth(shown, 20 / msg)).toBe(true);
    });

    test("an arrival 100 ms into a hand-off keeps her box growing from where it is", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        sends(motion, v, 2);
        await settle();
        sample(() => undefined, [100]);
        const box = part(v, "box:k:2");
        const was = seen(box);
        const strengthsWas = strengths(v);

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: ted });
        await settle();

        const now = seen(box);
        expect(near(now.left, was.left) && near(now.bottom, was.bottom) && near(now.height, was.height)).toBe(true);
        const shown = [strengthsWas, ...sample(() => strengths(v), [0, 16, 32, 48, 64])];
        expect(isSmooth(shown, 20 / (.6 * msg))).toBe(true);
    });
});

    test("an arrival 30 ms into an arrival in her run: the time line her newer row gives up fades from what it showed, never dipping", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }] });
        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();
        sample(() => undefined, [30]);
        const meta = part(v, "meta:k:3");
        const was = opacity(meta);
        expect(was).toBeGreaterThan(0);

        motion.arrived(message(4), place);
        v.rows.push({ key: "k:4", sender: anna });
        await settle();

        const [first] = sample(() => opacity(meta), [0]);
        expect(first).toBeCloseTo(was, 2);
    });

    test("an arrival 100 ms into an arrival in her run: the run's face, gliding, goes on from where it is to her newest row", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }] });
        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();
        sample(() => undefined, [100]);
        const was = seen(part(v, "face:k:2"));

        motion.arrived(message(4), place);
        v.rows.push({ key: "k:4", sender: anna });
        await settle();

        const shown = sample(() => seen(part(v, "face:k:2")));
        expect(corner(shown[0])).toEqual(corner(was));
        sample(() => undefined, [msg + 1]);
        const face = seen(part(v, "face:k:2"));
        const box = seen(part(v, "box:k:4"));
        expect(part(v, "face:k:2").closest("[data-row-key]")?.getAttribute("data-row-key")).toBe("k:4");
        expect(near(face.bottom, box.bottom)).toBe(true);
    });

    test("a reply 100 ms into a reply under a bubble that stays: the bubble stands still in every frame", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: tedTyping });
        const was = seen(dots(v));
        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: me });
        await settle();
        const first = sample(() => seen(dots(v)), [0, 16, 52, 100]);

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: me });
        await settle();

        const then = sample(() => seen(dots(v)));
        expect([...first, ...then].every(box => near(box.top, was.top) && near(box.left, was.left))).toBe(true);
    });

describe("what wakes a view", () => {
    test("an arrival, typing or its expiry in another place reads nothing of this view's tail", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: annaTyping });
        v.reads.count = 0;

        motion.arrived(message(7), "c2");
        motion.typingStarted("c2");
        motion.typingEnded("c2", "expired");
        motion.typingStarted("c1/9");
        await settle();

        expect(v.reads.count).toBe(0);
        expect((stage as Stage).played).toEqual([]);
    });
});

describe("an also-sent reply", () => {
    test("drawn in the conversation while someone else types there, it rides in above the bubble and grows out of nothing", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: me }], typing: tedTyping });

        // her typing ended in the thread, where she sent it; the conversation's bubble is Ted's
        motion.arrived(message(5), place);
        motion.typingEnded("c1:thread", "sent", message(5));
        v.rows.push({ key: "k:5", sender: anna });
        await settle();

        expect((stage as Stage).played.some(motion => motion.id === "chat-tail-handoff")).toBe(false);
        expect(named((stage as Stage).played)).toContain("chat-tail-in k:5");
        expect(bubbles(v)).toHaveLength(1);
    });
});

describe("the newest end", () => {
    // The drawer's body sits under the panel's header: the end is read from the scroller's own top.
    test.each([3, 14])("read from layout from the scroller's own top, it is where the scroll height puts it at rest, and stays put while the stack glides (%i rows)", async count => {
        const motion = createMessageMotion();
        const rows = Array.from({ length: count }, (_, index) => ({ key: `k:${index + 1}`, sender: index % 2 === 0 ? ted : anna }));
        const v = view(motion, { rows });
        const scroller = v.scroller.value as HTMLElement;
        const content = v.content.value as HTMLElement;
        expect(endOf(scroller, content)).toBe(scroller.scrollHeight - scroller.clientHeight);

        motion.arrived(message(40), place);
        v.rows.push({ key: "k:40", sender: me, h: 80 });
        await settle();

        const rest = sample(() => undefined, [msg]).length && scroller.scrollHeight - scroller.clientHeight;
        expect(endOf(scroller, content)).toBe(rest);
        expect(scroller.scrollTop).toBe(rest);
    });
});

describe("faces belong to their group", () => {
    test("Ted, then you, then Ted again: his new face plays no glide of its own and never crosses your row", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }] });

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: ted });
        await settle();

        const face = part(v, "face:k:3");
        expect((stage as Stage).played.filter(motion => motion.element === face)).toEqual([]);
        const shown = sample(() => ({ face: seen(face), mine: seen(row(v, "k:2")) }));
        expect(shown.every(frame => frame.face.top >= frame.mine.bottom - .01)).toBe(true);
    });

    test("her own run continuing glides the run's face from the row above to her new row", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }] });
        const was = seen(part(v, "face:k:2"));

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();

        expect(corner(seen(part(v, "face:k:2")))).toEqual(corner(was));
        sample(() => undefined, [msg]);
        expect(near(seen(part(v, "face:k:2")).bottom, seen(part(v, "box:k:3")).bottom)).toBe(true);
    });

    // Her new row fades in, but the run's face moving into it is the face that stood a moment
    // ago, so it keeps its strength rather than blinking out and fading back with the row.
    test.each([false, true])("her own run continuing keeps the run's face at full in every frame, her new message fading in around it (reduced motion: %s)", async isReduced => {
        setReducedMotion(isReduced);
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }] });

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();

        const shown = sample(() => ({ face: opacity(part(v, "face:k:2")), box: opacity(part(v, "box:k:3")) }));
        expect(shown.every(frame => frame.face > .999)).toBe(true);
        expect(shown[0].box).toBeLessThan(.1);
        expect(shown[shown.length - 1].box).toBeGreaterThan(.9);
    });

    // The thread's drawer hands its tail no scroller: the foot clip cuts only at the foot there too,
    // so a run's face gliding into a row clipped under the bubble is drawn whole above it.
    test.each([false, true])("her run continuing above a bubble that stands keeps the run's face whole in every frame (no scroller: %s)", async isScrollerless => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: me }, { key: "k:2", sender: anna }], typing: tedTyping, isScrollerless });

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();

        expect((stage as Stage).played.some(motion => motion.id === "chat-tail-foot")).toBe(true);
        // how much of the face a person does not see: its box outside its row's clip
        const cut = sample(() => {
            const face = seen(part(v, "face:k:2"));
            const clip = seen(row(v, "k:3"));
            const shown = Math.max(0, Math.min(face.bottom, clip.bottom) - Math.max(face.top, clip.top));
            return Math.round((face.height - shown) * 10) / 10;
        });
        expect(Math.max(...cut)).toBe(0);
    });
});

describe("a time line giving up its time", () => {
    // Another person's time line stands at its row's left, so the time going gives Edited the time's
    // room. Two words drawn over each other read as neither, so it crosses as a name line does: the
    // time fades out where it stood over the first half of t-base, and Edited fades out where it
    // stood over the same half and in at its new place over the second.
    test("another person's edited message followed up: the time fades where it stood by half way, Edited fades out there and in where it now lays out, never drawn over the time", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna, isEdited: true }] });
        const edited = (): HTMLElement => row(v, "k:2").querySelector<HTMLElement>(".chat-meta-edited") as HTMLElement;
        const stamp = row(v, "k:2").querySelector<HTMLElement>(".chat-meta-stamp") as HTMLElement;
        const was = seen(edited());
        const rowWas = seen(row(v, "k:2"));
        // the time's room: the time and its dot, and the line's gap after them
        const room = seen(stamp).width + 6;

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();

        const half = base / 2;
        const at = Array.from({ length: 27 }, (_, index) => Math.round(index * msg / 26));
        const shown = sample(() => {
            const box = seen(edited());
            const time = seen(stamp);
            const isOver = Math.min(box.right, time.right) - Math.max(box.left, time.left) > 1 && Math.min(box.bottom, time.bottom) - Math.max(box.top, time.top) > 1;
            return { left: box.left, inRow: box.top - seen(row(v, "k:2")).top, edited: opacity(edited()), stamp: opacity(stamp), isOver };
        }, at);
        const first = shown.filter((_, index) => at[index] < half);
        const second = shown.filter((_, index) => at[index] > half);
        // the first half: it stands where it stood in its row, fading out from full
        expect(first.every(frame => near(frame.left, was.left) && near(frame.inRow, was.top - rowWas.top))).toBe(true);
        expect(first[0].edited).toBeGreaterThan(.95);
        expect(first.slice(1).every((frame, index) => frame.edited < first[index].edited)).toBe(true);
        // the second half: it stands where its row lays it out, the time's room closed, fading in to full
        expect(second.every(frame => near(frame.left, was.left - room))).toBe(true);
        expect(second.slice(1).every((frame, index) => frame.edited >= second[index].edited)).toBe(true);
        expect(second.filter((_, index) => at[shown.indexOf(second[index])] >= base).every(frame => frame.edited > .99)).toBe(true);
        // never seen between the two places, and never drawn over the time while both are seen
        expect(shown.filter(frame => frame.edited > .05 && !near(frame.left, was.left) && !near(frame.left, was.left - room))).toEqual([]);
        expect(shown.filter(frame => frame.isOver && frame.edited > .05 && frame.stamp > .05)).toEqual([]);
        // the time and its dot fade out from full where they stood, gone by half way
        expect(shown[0].stamp).toBeGreaterThan(.95);
        expect(shown.filter((_, index) => at[index] >= half).every(frame => frame.stamp < .01)).toBe(true);
        expect(stamp.hidden).toBe(true);
        expect(stamp.style.position).toBe("");
    });

    test("under reduced motion the time goes at once and Edited stands where it lays out", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna, isEdited: true }] });
        const stamp = row(v, "k:2").querySelector<HTMLElement>(".chat-meta-stamp") as HTMLElement;

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: anna });
        await settle();

        expect(opacity(stamp)).toBe(0);
        expect((stage as Stage).played.filter(motion => motion.element.dataset.tail === "edited")).toEqual([]);
    });
});

describe("the view's own entrance", () => {
    /**
     * The name the feed's entrance plays under in the page, read from the feed's own style blocks
     * compiled as the build compiles them: a keyframe defined in a scoped block is renamed there,
     * so the stage plays the entrance under the name the page does, not the one the source spells.
     */
    function builtEntranceName(): string {
        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/messageFeed.partial.obs"), "utf8");
        const css = parse(source).descriptor.styles
            .map(style => compileStyle({ source: style.content, id: "data-v-feed", scoped: style.scoped, filename: "messageFeed.partial.obs" }).code)
            .join("\n");
        return /\.chat-feed\.is-entering[^{]*\{\s*animation:\s*([\w-]+)/.exec(css)?.[1] ?? "";
    }

    /** The scroller entering, as the feed's entrance plays on it in the page, this much of its time left. */
    function entering(v: View, left: number): void {
        const scroller = v.scroller.value as HTMLElement;
        const entrance = { animationName: builtEntranceName(), playState: "running", currentTime: 220 - left, effect: { getComputedTiming: () => ({ endTime: 220 }) } };
        scroller.getAnimations = (() => [entrance]) as never;
    }

    // The open's first read names a typist about a tenth of a second in, as the only change. No
    // event is recorded for her, so the open's plan carries the bubble's room: the rows above rise
    // over a message's time, never stepping in one frame.
    test("a typist the open's read names, alone, glides in over a message's time: the rows above never step", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }] });
        entering(v, 140);
        const top = seen(row(v, "k:1")).top;

        v.typing.value = tedTyping;
        await settle();

        expect(named((stage as Stage).played)).toEqual(["chat-tail-open tail-content"]);
        const tops = sample(() => seen(row(v, "k:1")).top, [0, 16, 35, 70, 105, 140]);
        expect(near(tops[0], top)).toBe(true);
        for (let index = 1; index < tops.length; index++) {
            expect(Math.abs(tops[index] - tops[index - 1])).toBeLessThan(16);
        }
        expect(tops[tops.length - 1]).toBeLessThan(top - 20);
    });

    // The open's first read lands about a tenth of a second in: what it brings closes over a message's
    // time from where it stood, never faster, however little of the entrance is left.
    test("what lands at the tail while the view enters, a page's rows or a typist it names, glides in over a message's time", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }] });
        entering(v, 120);
        const top = seen(row(v, "k:1")).top;

        v.rows.push({ key: "k:2", sender: me });
        v.typing.value = annaTyping;
        await settle();

        expect(named((stage as Stage).played)).toEqual(["chat-tail-open tail-content"]);
        expect((stage as Stage).played[0].options).toMatchObject({ duration: msg, easing: "linear" });
        expect(near(seen(row(v, "k:1")).top, top)).toBe(true);
    });
});

// A switch back draws the room as it was held at once, and the open's first read says what it holds
// now, who is typing included, a moment later. On a slow link that read can land after the feed's
// entrance has ended: what it brings is still the open's, so it rides the open's own motion rather
// than stepping. Only that first read: what a later read states is simply there.
describe("the open's first read", () => {
    test("landing after the entrance ended, the typist it names glides in with the stack: the rows above never step", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }], isLanded: false });
        const top = seen(row(v, "k:1")).top;
        // the two curves told apart, though the stage draws both linear
        (v.content.value as HTMLElement).style.setProperty("--chat-ease-in", "cubic-bezier(.1, .9, .2, 1)");
        (v.content.value as HTMLElement).style.setProperty("--chat-ease-state", "cubic-bezier(.42, 0, .58, 1)");

        v.landed.value = true;
        v.typing.value = tedTyping;
        await settle();

        expect(named((stage as Stage).played)).toEqual(["chat-tail-open tail-content"]);
        expect((stage as Stage).played[0].options).toMatchObject({ duration: msg, easing: "cubic-bezier(.42, 0, .58, 1)" });
        const tops = sample(() => seen(row(v, "k:1")).top);
        expect(near(tops[0], top)).toBe(true);
        for (let index = 1; index < tops.length; index++) {
            expect(Math.abs(tops[index] - tops[index - 1])).toBeLessThan(16);
        }
        (stage as Stage).advance(msg);
        expect(near(seen(row(v, "k:1")).top, top - 46)).toBe(true);
    });

    test("landing inside the entrance, it glides over a message's time from where it stood", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }], isLanded: false });
        const scroller = v.scroller.value as HTMLElement;
        const entrance = { animationName: "chat-feed-enter", playState: "running", currentTime: 80, effect: { getComputedTiming: () => ({ endTime: 220 }) } };
        scroller.getAnimations = (() => [entrance]) as never;

        v.landed.value = true;
        v.typing.value = tedTyping;
        await settle();

        expect(named((stage as Stage).played)).toEqual(["chat-tail-open tail-content"]);
        expect((stage as Stage).played[0].options).toMatchObject({ duration: msg, easing: "linear" });
    });

    /** The scroller entering, the feed's entrance this much of its time from its end. */
    function entering(v: View, left: number): void {
        const entrance = { animationName: "chat-feed-enter", playState: "running", currentTime: 220 - left, effect: { getComputedTiming: () => ({ endTime: 220 }) } };
        (v.scroller.value as HTMLElement).getAnimations = (() => [entrance]) as never;
    }

    // Most reads on a slow link land in the entrance's last frames: what is left of it is no
    // measure of how fast the rows may close, or a remainder of a few milliseconds is a step.
    test.each([10, 40])("landing with %i ms of the entrance left, the rows above never move 16px in a frame", async left => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }], isLanded: false });
        entering(v, left);
        const top = seen(row(v, "k:1")).top;

        v.landed.value = true;
        v.typing.value = tedTyping;
        await settle();

        const tops = sample(() => seen(row(v, "k:1")).top, Array.from({ length: 18 }, (_, index) => index * 16));
        expect(near(tops[0], top)).toBe(true);
        for (let index = 1; index < tops.length; index++) {
            expect(Math.abs(tops[index] - tops[index - 1])).toBeLessThan(16);
        }
        expect(near(tops[tops.length - 1], top - 46)).toBe(true);
    });

    // Her typing continues her run, so her face goes from her last row to the bubble: at rest it
    // moves foot to foot at full strength, as a live start's does, never vanishing and rising.
    test("landing after the entrance with her typing continuing her run, her face moves from her row to the bubble at full strength", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }], isLanded: false });
        const face = seen(part(v, "face:k:2"));

        v.landed.value = true;
        v.typing.value = annaTyping;
        v.isInRun.value = true;
        await settle();

        const coming = sample(() => ({ box: seen(bubbleFace(v)), seen: opacity(bubbleFace(v)) }));
        expect(corner(coming[0].box)).toEqual(corner(face));
        expect(coming.every(frame => frame.seen > .99)).toBe(true);
    });

    // A typist held from before the switch stands at the foot; the read brings a row above her.
    test("landing with a bubble standing at the foot and a row above it, the bubble stands still in every frame", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: tedTyping, isLanded: false });
        const was = seen(dots(v));

        v.landed.value = true;
        v.rows.push({ key: "k:2", sender: me });
        await settle();

        const boxes = sample(() => seen(dots(v)));
        expect(boxes.every(box => near(box.top, was.top) && near(box.left, was.left))).toBe(true);
        expect((stage as Stage).played.some(motion => motion.id === "chat-tail-open")).toBe(true);
    });

    test("landing while a reply still comes out from under a standing bubble, the reply's foot opens from where it stood", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: tedTyping, isLanded: false });
        /** How far down the screen a person sees the reply: its box inside its row's clip. */
        const footOf = (): number => Math.min(seen(part(v, "box:k:2")).bottom, seen(row(v, "k:2")).bottom);

        motion.arrived(message(2), place);
        v.rows.push({ key: "k:2", sender: me, w: 320 });
        await settle();
        const was = sample(footOf, [0, 52, 100]);
        expect(was[2]).toBeLessThan(seen(part(v, "box:k:2")).bottom - 5);

        // the read lands with the typist gone from it
        v.landed.value = true;
        v.typing.value = null;
        await settle();

        const after = sample(footOf, [0, 16, 52, 104, base]);
        expect(Math.abs(after[0] - was[2])).toBeLessThanOrEqual(1);
        const steps = after.slice(1).map((at, index) => Math.abs(at - after[index]));
        expect(Math.max(...steps)).toBeLessThan(20);
    });

    // A view can mark its open landed in the same render as what the read brought, after the tail has
    // read where it stood for that render: the change is still the open's, so it plays as the open.
    test("landing in the render its read already woke the tail for, the typist it names glides in with the stack", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], isLanded: false, isLandedByRead: true });
        const top = seen(row(v, "k:1")).top;

        v.typing.value = tedTyping;
        await settle();

        expect(v.landed.value).toBe(true);
        expect(named((stage as Stage).played).filter(name => name.includes("tail-content"))).toEqual(["chat-tail-open tail-content"]);
        const tops = sample(() => seen(row(v, "k:1")).top);
        expect(near(tops[0], top)).toBe(true);
        expect(tops[tops.length - 1]).toBeLessThan(top - 20);
    });

    test("once it has landed naming no one, a typist a later read names is simply there", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], isLanded: false });

        v.landed.value = true;
        await settle();
        v.typing.value = tedTyping;
        await settle();

        expect((stage as Stage).played).toEqual([]);
    });

    test("scrolled up, it moves nothing", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], isLanded: false, isFollowing: false });

        v.landed.value = true;
        v.typing.value = tedTyping;
        await settle();

        expect((stage as Stage).played).toEqual([]);
    });

    test("under reduced motion, the typist it names is simply there", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], isLanded: false });

        v.landed.value = true;
        v.typing.value = tedTyping;
        await settle();

        expect((stage as Stage).played).toEqual([]);
    });

    // The read can bring a row's new size: a reaction or an edit made while the view was away. The
    // rows above it step, each whole, its face and its name line with it in every frame.
    test.each([
        ["a direct message", false, [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }, { key: "k:3", sender: ted }], 1, [["face:k:1", "k:1", "bottom"]]],
        ["a group", true, [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }, { key: "k:3", sender: me }], 1, [["face:k:1", "k:1", "bottom"], ["name:k:1", "k:1", "top"], ["name:k:2", "k:2", "top"]]]
    ] as [string, boolean, Row[], number, [string, string, "top" | "bottom"][]][])("landing with a row it grows, in %s, the rows above step whole: faces and name lines ride their rows", async (_, isGroup, rows, grown, parts) => {
        const motion = createMessageMotion();
        const v = view(motion, { rows, isGroup, isLanded: false });

        v.landed.value = true;
        v.rows[grown].h = 80;
        v.typing.value = tedTyping;
        await settle();

        // every part read in each frame of the one pass, so each is read while it moves
        const frames = sample(() => parts.map(([id, key, edge]) => Math.round(seen(row(v, key))[edge] - seen(part(v, id))[edge])));
        parts.forEach(([id], index) => {
            expect([id, new Set(frames.map(frame => frame[index])).size]).toEqual([id, 1]);
        });
    });

    // One plan for every change: a recorded change landing while the open's plan plays goes on from
    // where everything is on screen, inside the entrance as after it.
    test("a stop recorded while the open's plan moves her face into the bubble inside the entrance moves it back from where it is", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: anna }], isLanded: false });
        entering(v, 200);

        v.landed.value = true;
        v.typing.value = annaTyping;
        v.isInRun.value = true;
        await settle();
        sample(() => 0, [60]);
        const was = seen(bubbleFace(v));

        motion.typingEnded(place, "stopped");
        v.typing.value = null;
        v.isInRun.value = false;
        await settle();

        const going = sample(() => seen(part(v, "face:k:2")).bottom, [0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176]);
        expect(Math.abs(going[0] - was.bottom)).toBeLessThanOrEqual(1);
        for (let index = 1; index < going.length; index++) {
            expect(Math.abs(going[index] - going[index - 1])).toBeLessThan(16);
        }
    });

    test("an arrival recorded while the open's plan brings a row out from under a standing bubble inside the entrance lets its foot open from where it stood", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }], typing: tedTyping, isLanded: false });
        entering(v, 200);
        /** How far down the screen a person sees the row: its box inside its row's clip. */
        const footOf = (): number => Math.min(seen(part(v, "box:k:2")).bottom, seen(row(v, "k:2")).bottom);

        v.landed.value = true;
        v.rows.push({ key: "k:2", sender: me, w: 320 });
        await settle();
        const was = sample(footOf, [0, 30, 60]);
        expect(was[2]).toBeLessThan(seen(part(v, "box:k:2")).bottom - 5);

        motion.arrived(message(3), place);
        v.rows.push({ key: "k:3", sender: me });
        await settle();

        const after = sample(footOf, [0, 16, 32, 48, 64, 80, 96]);
        expect(Math.abs(after[0] - was[2])).toBeLessThanOrEqual(1);
        for (let index = 1; index < after.length; index++) {
            expect(Math.abs(after[index] - after[index - 1])).toBeLessThan(16);
        }
    });
});

// A part hangs in a box of its row and rides that box, not the row's edge: a face hangs from its
// line at its bubble's foot, a name line from its row's top, Edited from its time line. A row that
// grows below a face, a reaction or a thread summary under its line, moves the row's foot and not
// the face's; a row that grows above a time line moves Edited with it.
describe("a part rides the box it hangs in", () => {
    /** Each frame, how far a part's edge stands from the same edge of the box it hangs in. */
    const gaps = (v: View, id: string, holder: () => Element, edge: "top" | "bottom"): number[] => sample(() => Math.round(seen(holder())[edge] - seen(part(v, id))[edge]));
    const faceGaps = (v: View, key: string): number[] => gaps(v, `face:${key}`, () => part(v, `box:${key}`), "bottom");

    test("the open's read bringing a reaction to the newest row, his: his face stays at his bubble's foot from the first frame", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: me }, { key: "k:2", sender: ted }], isLanded: false });
        const before = seen(part(v, "face:k:2")).bottom;

        v.landed.value = true;
        v.rows[1].isReacted = true;
        await settle();

        const first = seen(part(v, "face:k:2")).bottom;
        expect(new Set(faceGaps(v, "k:2")).size).toBe(1);
        expect(Math.abs(first - before)).toBeLessThanOrEqual(1);
    });

    test("the open's read bringing a reaction to his row above the newest: his face rides his bubble as the row steps", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }], isLanded: false });

        v.landed.value = true;
        v.rows[0].isReacted = true;
        await settle();

        expect(new Set(faceGaps(v, "k:1")).size).toBe(1);
    });

    test("the open's read bringing a reaction to an edited row above the newest: Edited stays on its time line", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted, isEdited: true }, { key: "k:2", sender: me }], isLanded: false });

        v.landed.value = true;
        v.rows[0].isReacted = true;
        await settle();

        expect(new Set(gaps(v, "edited", () => part(v, "meta:k:1"), "top")).size).toBe(1);
    });

    test.each([
        ["above the newest", [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }], 0, "k:1"],
        ["the newest", [{ key: "k:1", sender: me }, { key: "k:2", sender: ted }], 1, "k:2"]
    ] as [string, Row[], number, string][])("the open's read growing his bubble, %s: his face rides his bubble's foot", async (_, rows, grown, key) => {
        const motion = createMessageMotion();
        const v = view(motion, { rows, isLanded: false });

        v.landed.value = true;
        v.rows[grown].h = 80;
        await settle();

        expect(new Set(faceGaps(v, key)).size).toBe(1);
    });

    test("live, a message arriving in the render that brings a reaction to his row above: his face rides his bubble", async () => {
        const motion = createMessageMotion();
        const v = view(motion, { rows: [{ key: "k:1", sender: ted }, { key: "k:2", sender: me }] });

        motion.arrived(message(3), place);
        v.rows[0].isReacted = true;
        v.rows.push({ key: "k:3", sender: me });
        await settle();

        expect(new Set(faceGaps(v, "k:1")).size).toBe(1);
    });
});
