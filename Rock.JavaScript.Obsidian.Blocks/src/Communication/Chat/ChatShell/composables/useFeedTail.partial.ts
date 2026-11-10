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
// How a view's tail moves: the newest end of the conversation or the thread, where messages arrive
// and someone typing is drawn. One owner per view, so the stack moves as one: a message arriving
// glides the rows above up by its height while it fades in riding with them, the typing bubble
// comes in and slides out the same way, and her message grows out of the bubble it ends. It moves
// only for what the record says happened while the view was open, never for what a page, a re-read
// or a switch states, and nothing moves for a reader scrolled up.
//
// Before the render that draws a change it reads where the tail stands on screen, motion in flight
// included; after it, it plays from there to where the change put things, so a change that lands
// mid-motion continues from what is on screen and two motions never stack. While the view's own
// entrance plays, whatever lands at its tail is laid into the entrance rather than stepping after it.
// The motion is one transform on the box that holds everything, plus the few parts that change row.

import { inject, InjectionKey, nextTick, onBeforeUnmount, provide, Ref, watch } from "vue";
import { ArrivalMotion, messageMotionKey } from "../messageMotion.partial";

/** What a view hands its tail. */
export type FeedTailOptions = {
    /** The box that holds everything the view draws, which moves as one. */
    content: Ref<HTMLElement | null>;

    /** The view's scroller, whose own entrance a change at the tail is laid into; null where it has none. */
    scroller: () => HTMLElement | null;

    /** The view's place, whose typing the bubble draws. */
    place: () => string | null;

    /** Whether the reader is at the newest message, read before a change lands. */
    isFollowing: () => boolean;

    /** Keeps the reader at the newest message, as the view's own follow does. */
    follow: () => void;

    /** How a row drawn under a key arrives: the view's one claim of it, so it plays once. */
    arrives: (key: string) => ArrivalMotion | null;

    /** What the tail draws as state, so what lands inside the entrance without an event is laid into it too. */
    drawn: () => unknown;
};

/** What the typing bubble hands its tail: its element going, so it has no motion of its own. */
export type FeedTail = {
    /** The bubble is going; `done` takes it away. */
    leave: (element: Element, done: () => void) => void;
};

/** How a view hands its tail to the typing bubble it draws. */
export const feedTailKey: InjectionKey<FeedTail> = Symbol("chat-feed-tail");

/** Where the tail stood on screen before a change. */
type Before = {
    isFollowing: boolean;

    /** A row that stands before and after, by its key, and its top. */
    anchor: { key: string, top: number } | null;

    /** Every row key drawn. */
    keys: Set<string>;

    /** The typing bubble's dots, its face, and each row's face by its mark, where each is on screen. */
    bubble: DOMRect | null;
    faces: Map<string, DOMRect>;
};

/** The motion tokens, read from the view, so reduced motion's shorter values apply. */
type Tokens = { msg: number, base: number, fast: number, easeIn: string, easeState: string };

const tailIds = ["chat-tail-glide", "chat-tail-open", "chat-tail-in", "chat-tail-out", "chat-tail-handoff"];

/** A token's milliseconds, as the sheet writes them ("260ms", ".26s"). */
function ms(value: string, fallback: number): number {
    const text = value.trim();
    const parsed = parseFloat(text);
    if (Number.isNaN(parsed)) {
        return fallback;
    }
    return text.endsWith("ms") ? parsed : text.endsWith("s") ? parsed * 1000 : parsed;
}

/** The motion tokens as they apply to an element; where the sheet names none, nothing moves. */
function tokensOf(element: Element): Tokens {
    const style = getComputedStyle(element);
    const read = (name: string): string => style.getPropertyValue(name);
    return {
        msg: ms(read("--chat-t-msg"), 0),
        base: ms(read("--chat-t-base"), 0),
        fast: ms(read("--chat-t-fast"), 0),
        easeIn: read("--chat-ease-in").trim() || "linear",
        easeState: read("--chat-ease-state").trim() || "linear"
    };
}

function isMotionReduced(): boolean {
    return typeof window.matchMedia === "function" && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

/** Plays a motion where the browser can; one that cannot animates nothing and lands where it is. */
function play(element: Element, frames: Keyframe[], options: KeyframeAnimationOptions): Animation | null {
    return typeof (element as HTMLElement).animate === "function" ? (element as HTMLElement).animate(frames, options) : null;
}

/** Fades a leaving bubble out, and takes it away once it has. */
function fadeOut(element: Element, duration: number, tokens: Tokens, done: () => void): void {
    const fade = play(element, [{ opacity: 1 }, { opacity: 0 }], { duration, easing: tokens.easeState, id: "chat-tail-out" });
    if (fade) {
        fade.onfinish = done;
        fade.oncancel = done;
    }
    else {
        done();
    }
}

/** The row keys drawn in a box, in order. */
function rowsOf(content: HTMLElement): HTMLElement[] {
    return [...content.querySelectorAll<HTMLElement>("[data-row-key]")];
}

/** Each mark's last element, the newest: a face is marked by whose it is, and the tail moves only the newest. */
function facesOf(content: HTMLElement): Map<string, HTMLElement> {
    const faces = new Map<string, HTMLElement>();
    content.querySelectorAll<HTMLElement>("[data-tail-face]").forEach(element => faces.set(element.dataset.tailFace ?? "", element));
    return faces;
}

/**
 * The time left of the view's own entrance, in milliseconds, or 0 when none plays.
 */
function entranceLeft(scroller: HTMLElement | null): number {
    const entrance = scroller?.getAnimations?.().find(animation => (animation as CSSAnimation).animationName === "chat-feed-enter");
    const timing = entrance?.effect?.getComputedTiming();
    if (!entrance || !timing || entrance.playState !== "running") {
        return 0;
    }
    return Math.max(0, Number(timing.endTime ?? 0) - Number(entrance.currentTime ?? 0));
}

/**
 * A view's tail: the conversation's feed or the thread's drawer. Provides itself to the typing
 * bubble the view draws.
 *
 * @param options What the view hands its tail.
 *
 * @returns The tail.
 */
export function useFeedTail(options: FeedTailOptions): FeedTail {
    const motion = inject(messageMotionKey, null);
    const since = motion?.now() ?? 0;

    /** Where the tail stood before the change being drawn, read once per change. */
    let before: Before | null = null;

    /** The bubble going, with its way to be taken away. */
    let leaving: { element: Element, done: () => void } | null = null;

    /** The newest typing start and end this view has played, so each plays once. */
    let playedStart = since;
    let playedEnd = since;

    let isStopped = false;

    /** Reads where the tail stands on screen, before anything moves. */
    function read(): Before | null {
        const content = options.content.value;
        if (!content) {
            return null;
        }
        const rows = rowsOf(content);
        const first = rows[0];
        const faces = new Map<string, DOMRect>();
        facesOf(content).forEach((element, key) => faces.set(key, element.getBoundingClientRect()));
        return {
            isFollowing: options.isFollowing(),
            anchor: first ? { key: first.dataset.rowKey ?? "", top: first.getBoundingClientRect().top } : null,
            keys: new Set(rows.map(row => row.dataset.rowKey ?? "")),
            bubble: content.querySelector("[data-tail-bubble]")?.getBoundingClientRect() ?? null,
            faces
        };
    }

    /** How far the stack moved, from where it stood on screen to where it now lays out. */
    function offsetFrom(content: HTMLElement, was: Before): number {
        if (!was.anchor) {
            return 0;
        }
        const anchor = rowsOf(content).find(row => row.dataset.rowKey === was.anchor?.key);
        return anchor ? was.anchor.top - anchor.getBoundingClientRect().top : 0;
    }

    /** Takes a leaving bubble out of the flow where it stands, so the stack can settle into its room while it goes. */
    function lift(element: HTMLElement): void {
        const { offsetTop, offsetLeft, offsetWidth } = element;
        Object.assign(element.style, { position: "absolute", top: `${offsetTop}px`, left: `${offsetLeft}px`, width: `${offsetWidth}px`, margin: "0" });
    }

    /** The typing in this view's place that this view has not played yet: a start, an end and why. */
    function typingNow(): { isStart: boolean, end: string | null } {
        const place = options.place();
        const typing = place ? motion?.typingOf(place) : undefined;
        const started = typing?.started?.seq ?? 0;
        const ended = typing?.ended;
        const isStart = started > playedStart;
        const end = ended && ended.seq > playedEnd ? ended.cause : null;
        playedStart = Math.max(playedStart, started);
        playedEnd = Math.max(playedEnd, ended?.seq ?? 0);
        return { isStart, end };
    }

    /** Lets a leaving bubble go now. */
    function release(): void {
        const going = leaving;
        leaving = null;
        going?.done();
    }

    /** Grows her message out of the bubble it ends: from the bubble's box on screen to its own, its words fading in. */
    function grow(row: HTMLElement, from: DOMRect, offset: number, tokens: Tokens): void {
        const target = row.querySelector<HTMLElement>(".chat-bubble") ?? row;
        const to = target.getBoundingClientRect();
        const dx = from.left - to.left;
        const dy = from.bottom - (to.bottom + offset);
        // the box grows up and toward the far side from the corner by her face; a message narrower
        // than the bubble starts at its own width
        const top = Math.max(0, to.height - from.height);
        const right = Math.max(0, to.width - from.width);
        const timing = { duration: tokens.msg, easing: tokens.easeIn, id: "chat-tail-handoff" };
        play(target, [
            { transform: `translate(${dx}px, ${dy}px)`, clipPath: `inset(${top}px ${right}px 0 0 round var(--chat-r-bubble))` },
            { transform: "none", clipPath: "inset(0 0 0 0 round var(--chat-r-bubble))" }
        ], timing);
        for (const part of target.children) {
            play(part, [{ opacity: 0, offset: 0 }, { opacity: 0, offset: .3 }, { opacity: 1, offset: .9 }, { opacity: 1, offset: 1 }], timing);
        }
    }

    /** Plays the change, from where the tail stood before it to where it now lays out. */
    function settle(was: Before): void {
        const content = options.content.value;
        if (isStopped || !content) {
            release();
            return;
        }

        const typing = typingNow();
        const reduced = isMotionReduced();
        const tokens = tokensOf(content);

        // Every row drawn is claimed, whether or not anything plays, so a row drawn again later is
        // never news: a folded line claims the messages that joined it too. Only a row new to the
        // view can arrive.
        const arrivals = rowsOf(content)
            .map(row => ({ row, how: options.arrives(row.dataset.rowKey ?? "") }))
            .filter(({ row }) => !was.keys.has(row.dataset.rowKey ?? ""));
        const handoff = typing.end === "sent" ? arrivals.find(arrival => arrival.how === "inPlace") : undefined;
        // This view moves only for what its own place recorded: a message arriving here, typing
        // starting, or typing ending as a person sees it end. An end for what a page brought, or
        // a session's stop, is state, and so is whatever else changed with it.
        const isMotion = arrivals.some(arrival => arrival.how !== null) || typing.isStart || (typing.end !== null && typing.end !== "overtaken" && typing.end !== "cleared");

        // A bubble going for her message goes as it lands, which grows out of it; one going as
        // state goes at once. Under less motion it fades where it stands before its room closes.
        const goes = typing.end === "stopped" || typing.end === "expired" || typing.end === "blocked";
        if (leaving && reduced && was.isFollowing && (goes || handoff)) {
            const { element, done } = leaving;
            leaving = null;
            if (handoff) {
                lift(element as HTMLElement);
            }
            fadeOut(element, handoff ? tokens.msg : tokens.fast, tokens, done);
        }
        else if (leaving && !reduced && was.isFollowing && goes) {
            lift(leaving.element as HTMLElement);
        }
        else {
            release();
        }

        const entrance = entranceLeft(options.scroller());
        if (!was.isFollowing || (!isMotion && entrance === 0)) {
            // Scrolled up, nothing moves; a change of state alone after the entrance is simply there.
            release();
            return;
        }

        // Only the stack's own glide is let go: it is read on screen above, so the new one takes over from there.
        content.getAnimations?.().filter(animation => tailIds.includes(animation.id)).forEach(animation => animation.cancel());
        options.follow();
        const offset = offsetFrom(content, was);

        if (entrance > 0) {
            // The open's one motion: what lands while the view enters rides the entrance to its end.
            if (Math.abs(offset) >= .5 && !reduced) {
                play(content, [{ transform: `translateY(${offset}px)` }, { transform: "none" }], { duration: entrance, easing: tokens.easeIn, id: "chat-tail-open" });
            }
            release();
            return;
        }

        const duration = goes && arrivals.length === 0 ? tokens.base : tokens.msg;
        const easing = goes && arrivals.length === 0 ? tokens.easeState : tokens.easeIn;
        if (!reduced && Math.abs(offset) >= .5) {
            play(content, [{ transform: `translateY(${offset}px)` }, { transform: "none" }], { duration, easing, id: "chat-tail-glide" });
        }

        // A leaving bubble slides down out under the edge with the stack and fades.
        if (leaving) {
            const { element, done } = leaving;
            leaving = null;
            fadeOut(element, tokens.base, tokens, done);
        }

        // Each message new here fades in riding the stack; hers grows out of the bubble.
        for (const { row, how } of arrivals) {
            if (how === null) {
                continue;
            }
            if (row === handoff?.row && was.bubble && !reduced) {
                grow(row, was.bubble, offset, tokens);
            }
            else {
                play(row, [{ opacity: 0 }, { opacity: 1 }], { duration: tokens.msg, easing: reduced ? tokens.easeState : tokens.easeIn, id: "chat-tail-in" });
            }
        }

        // Typing starting: the bubble fades in riding the stack.
        const bubble = typing.isStart ? content.querySelector("[data-tail-typing]:not(.is-leaving)") : null;
        if (bubble) {
            play(bubble, [{ opacity: 0 }, { opacity: 1 }], { duration: tokens.msg, easing: reduced ? tokens.easeState : tokens.easeIn, id: "chat-tail-in" });
        }

        // The parts that change row: her face from the bubble, or from the row above, to her newest message.
        if (!reduced) {
            facesOf(content).forEach((element, key) => {
                const at = was.faces.get(key);
                if (!at) {
                    return;
                }
                const to = element.getBoundingClientRect();
                const dx = at.left - to.left;
                const dy = at.top - (to.top + offset);
                if (Math.abs(dx) >= .5 || Math.abs(dy) >= .5) {
                    play(element, [{ transform: `translate(${dx}px, ${dy}px)` }, { transform: "none" }], { duration, easing, id: "chat-tail-glide" });
                }
            });
        }
    }

    // Before the render, so where the tail stands is read before anything in it moves. Every
    // writer records in the same pass as its change of state, so the count and the state move together.
    watch([() => motion?.tail(), options.drawn], () => {
        if (isStopped || before) {
            return;
        }
        before = read();
        // after the whole render, the bubble's own coming and going included
        void nextTick(() => {
            const was = before;
            before = null;
            if (was) {
                settle(was);
            }
        });
    }, { flush: "pre" });

    const tail: FeedTail = {
        leave: (element, done) => {
            // Going, it is no longer where her face or her message's box is read from.
            element.classList.add("is-leaving");
            element.querySelectorAll("[data-tail-face], [data-tail-bubble]").forEach(part => {
                part.removeAttribute("data-tail-face");
                part.removeAttribute("data-tail-bubble");
            });
            // A bubble going with no change read before it has nothing to play from, and goes at once.
            if (!before) {
                done();
                return;
            }
            release();
            leaving = { element, done };
        }
    };

    provide(feedTailKey, tail);

    onBeforeUnmount(() => {
        isStopped = true;
        release();
    });

    return tail;
}
