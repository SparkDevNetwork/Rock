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
// nothing moves; under reduced motion nothing slides or grows; what lands inside the view's own
// entrance rides it. Drawn here in a host of its own over a laid-out stack that jsdom cannot lay
// out: every row stands on the one below it, the newest at the foot.
import { flushPromises, mount } from "@vue/test-utils";
import { defineComponent, h, nextTick, reactive, ref } from "vue";
import TypingBubble from "../../../../../src/Communication/Chat/ChatShell/components/typingBubble.partial.obs";
import { useFeedTail } from "../../../../../src/Communication/Chat/ChatShell/composables/useFeedTail.partial";
import type { Typing } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresence.partial";
import { createMessageMotion, MessageMotion, messageMotionKey, useViewArrivals } from "../../../../../src/Communication/Chat/ChatShell/messageMotion.partial";
import { Played, playAnimations } from "../tailFakes";

const anna = "a0000002-0000-4000-8000-000000000000";
const place = "c1";
const annaTyping: Typing = { typists: [{ alias: anna, person: { person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes", avatar_url: null } }], names: "Anna", sentence: "Anna is typing…" };

/** A row's height, the bubble's with its gap, and where the stack's foot stands on screen. */
const rowHeight = 40;
const typingHeight = 48;
const foot = 600;

/** A box on screen; jsdom has no DOMRect. */
function rect(left: number, top: number, width: number, height: number): DOMRect {
    return { left, top, width, height, right: left + width, bottom: top + height, x: left, y: top, toJSON: () => ({}) } as DOMRect;
}

/** How far a motion in flight still draws the stack from where it lays out, as the browser would. */
let drawnBy = 0;

/** A row's or the bubble's box, the stack laid out from the foot up. */
function stackBox(element: HTMLElement): DOMRect | null {
    const content = element.parentElement;
    if (!content || content.dataset.testid !== "tail-content") {
        return null;
    }
    let bottom = foot + drawnBy;
    for (const child of [...content.children].reverse() as HTMLElement[]) {
        if (child.style.position === "absolute") {
            continue;
        }
        const height = child.dataset.tailTyping !== undefined ? typingHeight : rowHeight;
        if (child === element) {
            return rect(20, bottom - height, 400, height);
        }
        bottom -= height;
    }
    return null;
}

/** Boxes as a browser would lay them: a row's bubble and the typist's dots at the foot of their row, faces beside them. */
function box(this: HTMLElement): DOMRect {
    const own = stackBox(this);
    if (own) {
        return own;
    }
    const row = this.closest<HTMLElement>("[data-row-key], [data-tail-typing]");
    const at = row ? stackBox(row) : null;
    if (!at) {
        return rect(0, 0, 0, 0);
    }
    if (this.dataset.tailFace !== undefined) {
        return rect(20, at.bottom - 34, 34, 34);
    }
    if (this.dataset.tailBubble !== undefined) {
        return rect(62, at.bottom - 34, 60, 34);
    }
    // her message's bubble: 200 wide and as tall as its row
    return rect(62, at.top, 200, at.height);
}

/**
 * A view over the real record and the real bubble, drawing each key as a row with its bubble and
 * the last row of each sender with her face, the way the feed marks them for its tail.
 */
function draw(motion: MessageMotion, options: { rows?: string[], typing?: Typing | null, isFollowing?: boolean } = {}) {
    const rows = reactive<string[]>([...options.rows ?? []]);
    const typing = ref<Typing | null>(options.typing ?? null);
    const following = ref(options.isFollowing ?? true);
    const follows = { count: 0 };
    const scroller = ref<HTMLElement | null>(null);

    const View = defineComponent({
        setup() {
            const content = ref<HTMLElement | null>(null);
            const { arrives } = useViewArrivals();
            useFeedTail({
                content,
                scroller: () => scroller.value,
                place: () => place,
                isFollowing: () => following.value,
                follow: () => follows.count++,
                arrives,
                drawn: () => [typing.value, rows.length]
            });
            // the motion tokens as the chat's sheet names them, on the box the tail reads them from
            const tokens = "--chat-t-msg: 260ms; --chat-t-base: 180ms; --chat-t-fast: 120ms; --chat-ease-in: ease-out; --chat-ease-state: ease-in-out";
            return () => h("div", { ref: scroller }, [
                h("div", { ref: content, style: tokens, "data-testid": "tail-content" }, [
                    ...rows.map((key, index) => h("div", { key, "data-row-key": key }, [
                        h("div", { class: "chat-bubble" }, [h("span", key)]),
                        index === rows.length - 1 ? h("span", { "data-tail-face": anna }) : null
                    ])),
                    h(TypingBubble, { typing: typing.value })
                ])
            ]);
        }
    });

    // the real Transition, which keeps a leaving bubble while its tail plays it, as a browser does
    const wrapper = mount(View, { attachTo: document.body, global: { stubs: { transition: false }, provide: { [messageMotionKey as symbol]: motion } } });
    return { wrapper, rows, typing, following, follows };
}

/** The render and the tail after it. */
async function settle(): Promise<void> {
    await flushPromises();
    await nextTick();
    await nextTick();
}

/** Each motion played, by its name and what it played on; a face changing row is its own cell's. */
function named(played: Played[]): string[] {
    return played.filter(motion => motion.element.dataset.tailFace === undefined).map(motion => `${motion.id} ${motion.element.dataset.rowKey ?? motion.element.dataset.testid ?? motion.element.className}`);
}

/** Her message, as a holder hands it to the record. */
const message = (id: number): { id: number, client_key: null } => ({ id, client_key: null });

function setReducedMotion(isReduced: boolean): void {
    window.matchMedia = ((query: string) => ({ matches: isReduced && query.includes("reduce"), media: query, addEventListener: () => undefined, removeEventListener: () => undefined })) as unknown as typeof window.matchMedia;
}

let animations: ReturnType<typeof playAnimations>;
const realBox = HTMLElement.prototype.getBoundingClientRect;

beforeEach(() => {
    animations = playAnimations();
    HTMLElement.prototype.getBoundingClientRect = box;
    drawnBy = 0;
    setReducedMotion(false);
});

afterEach(() => {
    animations.restore();
    HTMLElement.prototype.getBoundingClientRect = realBox;
    document.body.innerHTML = "";
});

describe("a message arriving", () => {
    test("glides the stack up from where it stood by the new row's height, the new row fading in riding it, once", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1", "k:2"] });

        motion.arrived(message(3));
        view.rows.push("k:3");
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-glide tail-content", "chat-tail-in k:3"]);
        expect(animations.played[0].keyframes).toEqual([{ transform: `translateY(${rowHeight}px)` }, { transform: "none" }]);
        expect(animations.played[0].options).toMatchObject({ duration: 260, id: "chat-tail-glide" });
        expect(view.follows.count).toBe(1);

        // the same message heard again, and a page restating it, move nothing more
        animations.played.length = 0;
        motion.arrived(message(3));
        await settle();
        expect(named(animations.played)).toEqual([]);
    });

    test("several in one change glide once, each new row fading in", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"] });

        motion.arrived(message(2));
        motion.arrived(message(3));
        view.rows.push("k:2", "k:3");
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-glide tail-content", "chat-tail-in k:2", "chat-tail-in k:3"]);
        expect(animations.played[0].keyframes[0]).toEqual({ transform: `translateY(${2 * rowHeight}px)` });
    });

    // Step 1 reads the stack where a glide in flight draws it, so the next glide takes over from there.
    test("one landing mid-glide takes over from where the stack is on screen, and the glide before is let go", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"] });
        motion.arrived(message(2));
        view.rows.push("k:2");
        await settle();
        const first = animations.played[0];

        // halfway: the stack is still drawn 20px below where it lays out
        drawnBy = 20;
        const cancelling = (): void => {
            drawnBy = 0;
        };
        Object.defineProperty(first, "cancelled", { value: true });
        animations.played.length = 0;
        const glideBefore = view.wrapper.get("[data-testid='tail-content']").element.getAnimations()[0];
        glideBefore.oncancel = cancelling as never;

        motion.arrived(message(3));
        view.rows.push("k:3");
        await settle();

        expect(animations.cancelled.map(motion => motion.id)).toContain("chat-tail-glide");
        expect(named(animations.played)).toEqual(["chat-tail-glide tail-content", "chat-tail-in k:3"]);
        expect(animations.played[0].keyframes[0]).toEqual({ transform: `translateY(${rowHeight + 20}px)` });
    });

    test("scrolled up nothing moves, and the row is claimed, so it never fades in later", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"], isFollowing: false });

        motion.arrived(message(2));
        view.rows.push("k:2");
        await settle();
        expect(named(animations.played)).toEqual([]);
        expect(view.follows.count).toBe(0);

        view.following.value = true;
        motion.arrived(message(3));
        view.rows.push("k:3");
        await settle();
        expect(named(animations.played)).toEqual(["chat-tail-glide tail-content", "chat-tail-in k:3"]);
    });

    test("a row a page, a catch-up or a switch back brings, with nothing recorded, is simply there", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"] });

        view.rows.push("k:2");
        await settle();

        expect(named(animations.played)).toEqual([]);
    });

    test("a view opened after a message arrived plays nothing for it", async () => {
        const motion = createMessageMotion();
        motion.arrived(message(2));
        const view = draw(motion, { rows: ["k:1"] });

        view.rows.push("k:2");
        await settle();

        expect(named(animations.played)).toEqual([]);
    });

    test("under reduced motion the new row fades in and nothing slides", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"] });

        motion.arrived(message(2));
        view.rows.push("k:2");
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-in k:2"]);
        expect(animations.played.every(motion => motion.keyframes.every(frame => frame.transform === undefined))).toBe(true);
    });
});

describe("someone typing", () => {
    test("coming in glides the stack up by the bubble's height, the bubble fading in riding it", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"] });

        motion.typingStarted(place);
        view.typing.value = annaTyping;
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-glide tail-content", "chat-tail-in typing-bubble"]);
        expect(animations.played[0].keyframes[0]).toEqual({ transform: `translateY(${typingHeight}px)` });
    });

    test("drawn as what is, from a page's answer or a switch back, it comes with no motion", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"] });

        view.typing.value = annaTyping;
        await settle();

        expect(named(animations.played)).toEqual([]);
    });

    test("stopping, the bubble slides down out under the edge with the stack and fades, and goes once it has", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"], typing: annaTyping });

        motion.typingEnded(place, "stopped");
        view.typing.value = null;
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-glide tail-content", "chat-tail-out typing-bubble"]);
        expect(animations.played[0].keyframes[0]).toEqual({ transform: `translateY(${-typingHeight}px)` });
        expect(animations.played[0].options).toMatchObject({ duration: 180 });
        const going = view.wrapper.get("[data-testid='typing-bubble']").element as HTMLElement;
        expect(going.style.position).toBe("absolute");

        animations.played[1].finish();
        await nextTick();
        expect(view.wrapper.find("[data-testid='typing-bubble']").exists()).toBe(false);
    });

    test("overtaken by a message a page brought, it goes at once with nothing moving", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"], typing: annaTyping });

        motion.typingEnded(place, "overtaken");
        view.typing.value = null;
        await settle();

        expect(named(animations.played)).toEqual([]);
        expect(view.wrapper.find("[data-testid='typing-bubble']").exists()).toBe(false);
    });

    test("under reduced motion it fades where it stands, and its room closes once it has", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"], typing: annaTyping });

        motion.typingEnded(place, "expired");
        view.typing.value = null;
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-out typing-bubble"]);
        expect((view.wrapper.get("[data-testid='typing-bubble']").element as HTMLElement).style.position).toBe("");
        animations.played[0].finish();
        await nextTick();
        expect(view.wrapper.find("[data-testid='typing-bubble']").exists()).toBe(false);
    });
});

describe("her message ending her typing", () => {
    /** Her message heard live, recorded as the live router does: the arrival, then the typing's end naming it. */
    function sends(motion: MessageMotion, view: ReturnType<typeof draw>, id: number): void {
        motion.arrived(message(id));
        motion.typingEnded(place, "sent", message(id));
        view.rows.push(`k:${id}`);
        view.typing.value = null;
    }

    test("grows out of the bubble: from the dots' box on screen to its own, its words fading in, the bubble gone as it lands", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"], typing: annaTyping });

        sends(motion, view, 2);
        await settle();

        expect(view.wrapper.find("[data-testid='typing-bubble']").exists()).toBe(false);
        const handoff = animations.played.filter(motion => motion.id === "chat-tail-handoff");
        expect(handoff.map(motion => motion.element.className || motion.element.tagName)).toEqual(["chat-bubble", "SPAN"]);
        // the stack settles by the difference of the bubble and her row; her bubble starts as the dots' box
        const offset = rowHeight - typingHeight;
        expect(animations.played[0]).toMatchObject({ id: "chat-tail-glide", keyframes: [{ transform: `translateY(${offset}px)` }, { transform: "none" }] });
        const from = { left: 62, bottom: foot, width: 60, height: 34 };
        const to = { left: 62, bottom: foot, width: 200, height: rowHeight };
        expect(handoff[0].keyframes[0]).toEqual({
            transform: `translate(${from.left - to.left}px, ${from.bottom - (to.bottom + offset)}px)`,
            clipPath: `inset(${to.height - from.height}px ${to.width - from.width}px 0 0 round var(--chat-r-bubble))`
        });
        expect(handoff[1].keyframes.map(frame => [frame.offset, frame.opacity])).toEqual([[0, 0], [.3, 0], [.9, 1], [1, 1]]);
        expect(named(animations.played)).not.toContain("chat-tail-in k:2");
    });

    test("her face glides from the bubble's to her message's", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"], typing: annaTyping });

        sends(motion, view, 2);
        await settle();

        const faces = animations.played.filter(motion => motion.element.dataset.tailFace !== undefined);
        expect(faces).toHaveLength(1);
        expect(faces[0].id).toBe("chat-tail-glide");
    });

    test("under reduced motion it is a crossfade: the bubble fades out where it stood as her message fades in", async () => {
        setReducedMotion(true);
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"], typing: annaTyping });

        sends(motion, view, 2);
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-out typing-bubble", "chat-tail-in k:2"]);
        expect(animations.played.every(motion => motion.keyframes.every(frame => frame.transform === undefined && frame.clipPath === undefined))).toBe(true);
    });

    test("one of two typists sending rises in above the bubble, which stays", async () => {
        const motion = createMessageMotion();
        const two: Typing = { ...annaTyping, typists: [...annaTyping.typists, { alias: "a0000003-0000-4000-8000-000000000000", person: null }], names: "Anna and someone" };
        const view = draw(motion, { rows: ["k:1"], typing: two });

        motion.arrived(message(2));
        view.rows.push("k:2");
        view.typing.value = annaTyping;
        await settle();

        expect(named(animations.played)).toContain("chat-tail-in k:2");
        expect(animations.played.some(motion => motion.id === "chat-tail-handoff")).toBe(false);
        expect(view.wrapper.find("[data-testid='typing-bubble']").exists()).toBe(true);
    });
});

describe("the view's own entrance", () => {
    /** The scroller entering, as the feed's entrance plays on it, this much of its time left. */
    function entering(view: ReturnType<typeof draw>, left: number): void {
        const scroller = view.wrapper.element as HTMLElement;
        const entrance = { animationName: "chat-feed-enter", playState: "running", currentTime: 220 - left, effect: { getComputedTiming: () => ({ endTime: 220 }) } };
        scroller.getAnimations = (() => [entrance]) as never;
    }

    // The open's re-read lands about a tenth of a second in: what it brings rides the entrance to its end.
    test("what lands at the tail while the view enters, a page's rows or a typist it names, rides the entrance to its end", async () => {
        const motion = createMessageMotion();
        const view = draw(motion, { rows: ["k:1"] });
        entering(view, 120);

        view.rows.push("k:2");
        view.typing.value = annaTyping;
        await settle();

        expect(named(animations.played)).toEqual(["chat-tail-open tail-content"]);
        expect(animations.played[0].keyframes[0]).toEqual({ transform: `translateY(${rowHeight + typingHeight}px)` });
        expect(animations.played[0].options).toMatchObject({ duration: 120 });
    });
});
