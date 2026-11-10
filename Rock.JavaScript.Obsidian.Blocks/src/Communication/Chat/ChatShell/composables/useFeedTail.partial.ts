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
// included. After it, it plans, then plays: it takes what is going out of the flow, lets go of the
// motions this change moves again, reads where everything now lays out with nothing of its own
// moving, and plays each part from where it stood on screen to there, so a change that lands
// mid-motion continues from what is on screen and two motions never stack. While the view's own
// entrance plays, whatever lands at its tail is laid into the entrance rather than stepping after it.
//
// The motion is one transform on the box that holds everything, plus the parts that do not ride
// their row, each marked `data-tail`: a group's face and name line by the group's key, `face:<key>`
// (so a face moves only while its run continues), the typing bubble and its dots, and a row's time
// line, its message's box, and in an edited message's time line its time and Edited, `meta`, `box`,
// `stamp` and `edited`, which are their row's. A part of the bubble leaving comes through the tail's
// one leave hook, from a Transition that hands it over; a row keeps its time line, and an edited
// message's time, and hides them, and the tail fades out the one it read standing that a row now hides.

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

/** What the parts a view draws hand its tail: a part going, so it has no motion of its own. */
export type FeedTail = {
    /** A marked part is going; `done` takes it away. */
    leave: (element: Element, done: () => void) => void;
};

/** How a view hands its tail to the parts it draws. */
export const feedTailKey: InjectionKey<FeedTail> = Symbol("chat-feed-tail");

/** A part of the tail and where it stood on screen. */
type Part = { id: string, element: HTMLElement, box: DOMRect };

/** Where the tail stood on screen before a change. */
type Before = {
    isFollowing: boolean;

    /** Every row key drawn. */
    keys: Set<string>;

    /** The rows on screen at the tail, newest first, with their tops. */
    rows: { key: string, top: number }[];

    /** The parts on screen at the tail, by mark and by element. */
    parts: Map<string, Part>;
    elements: Map<Element, Part>;
};

/** A part going, with its way to be taken away. */
type Leaving = { element: HTMLElement, done: () => void };

/** The motion tokens, read from the view, so reduced motion's shorter values apply. */
type Tokens = { msg: number, base: number, fast: number, easeIn: string, easeOut: string, easeState: string };

/** One start offset, the transform a part or the stack plays from to none. */
type Offset = { x: number, y: number };

const glideIds = ["chat-tail-glide", "chat-tail-open"];
const moveIds = ["chat-tail-move", "chat-tail-foot"];

/** What the tail writes on a part it takes out of the flow, taken off again when a kept part is done. */
const liftedStyles = ["position", "top", "left", "width", "margin", "visibility"];

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
        easeOut: read("--chat-ease-out").trim() || "linear",
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

/** The running motions on an element, or in it too. */
function motionsOf(element: Element, isSubtree = false): Animation[] {
    return element.getAnimations?.({ subtree: isSubtree }) ?? [];
}

/** Fades in everything in a box but one part, which keeps the strength it has. */
function fadeAround(holder: Element, kept: Element, fadeIn: (element: Element) => void): void {
    for (const child of holder.children) {
        if (child === kept) {
            continue;
        }
        if (child.contains(kept)) {
            fadeAround(child, kept, fadeIn);
        }
        else {
            fadeIn(child);
        }
    }
}

/** How far up from its own bottom a foot clips a row now, its motion in flight included. */
function footOf(holder: Element): number {
    const inner = /inset\(([^)]*)\)/.exec(getComputedStyle(holder).clipPath)?.[1].trim().split(/\s+/) ?? [];
    const bottom = parseFloat(inner[2] ?? inner[0] ?? "");
    return Number.isNaN(bottom) ? 0 : bottom;
}

/** The rows drawn in a box, in order. */
function rowsOf(content: HTMLElement): HTMLElement[] {
    return [...content.querySelectorAll<HTMLElement>("[data-row-key]")];
}

const kindOf = (id: string): string => id.split(":")[0];

/** The marks a row keys by its own key, since what draws them does not know it. */
const rowMarks = ["meta", "box", "stamp", "edited"];

/** The marks a row keeps and hides rather than drawing again: the tail fades out one it read standing. */
const keptMarks = ["meta", "stamp"];

/** A part's id: its mark, or for a row's own parts, its mark and its row's key. */
function idOf(element: HTMLElement): string | undefined {
    const mark = element.dataset?.tail;
    return mark && rowMarks.includes(mark) ? `${mark}:${element.closest<HTMLElement>("[data-row-key]")?.dataset.rowKey ?? ""}` : mark;
}

/** An element's own opacity, its motions included. */
function ownOpacity(element: Element): number {
    const value = parseFloat(getComputedStyle(element).opacity);
    return Number.isNaN(value) ? 1 : value;
}

/** How strongly an element shows as a person sees it: its opacity and every ancestor's, to the scroller. */
function seenOpacity(element: Element, scroller: Element | null): number {
    let product = 1;
    for (let at: Element | null = element; at && at !== scroller; at = at.parentElement) {
        product *= ownOpacity(at);
    }
    return product;
}

/** The time left of the view's own entrance, in milliseconds, or 0 when none plays. */
function entranceLeft(scroller: HTMLElement | null): number {
    // named in the token sheet, where no build renames it as it would a scoped keyframe
    const entrance = scroller?.getAnimations?.().find(animation => (animation as CSSAnimation).animationName === "chat-feed-enter");
    const timing = entrance?.effect?.getComputedTiming();
    if (!entrance || !timing || entrance.playState !== "running") {
        return 0;
    }
    return Math.max(0, Number(timing.endTime ?? 0) - Number(entrance.currentTime ?? 0));
}

/**
 * A view's tail: the conversation's feed or the thread's drawer. Provides itself to the parts the
 * view draws.
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

    /** The parts going in the change being drawn. */
    let leaving: Leaving[] = [];

    /** Parts out of the flow still playing their way out, from this change or one before. */
    const going = new Map<HTMLElement, () => void>();

    /** The newest typing start and end this view has played, so each plays once. */
    let playedStart = since;
    let playedEnd = since;

    let isStopped = false;

    /**
     * Reads where the tail stands on screen, before anything moves: the rows from the newest back to
     * the first one above the scroller's top, so a wakeup reads about the rows a person sees, and
     * the parts in them and in the typing bubble.
     */
    function read(): Before | null {
        const content = options.content.value;
        if (!content) {
            return null;
        }
        const rows = rowsOf(content);
        const edge = options.scroller()?.getBoundingClientRect().top ?? Number.NEGATIVE_INFINITY;
        const onScreen: { key: string, top: number }[] = [];
        const parts = new Map<string, Part>();
        const elements = new Map<Element, Part>();
        const take = (holder: Element): void => {
            for (const element of [holder, ...holder.querySelectorAll<HTMLElement>("[data-tail]")] as HTMLElement[]) {
                const id = idOf(element);
                if (id && !element.hidden && !going.has(element) && !isInGoing(element)) {
                    const part = { id, element, box: element.getBoundingClientRect() };
                    parts.set(id, part);
                    elements.set(element, part);
                }
            }
        };
        for (let index = rows.length - 1; index >= 0; index--) {
            const top = rows[index].getBoundingClientRect().top;
            onScreen.push({ key: rows[index].dataset.rowKey ?? "", top });
            take(rows[index]);
            if (top < edge) {
                break;
            }
        }
        const bubble = content.querySelector<HTMLElement>("[data-tail='typing:typing']");
        if (bubble) {
            take(bubble);
        }
        return {
            isFollowing: options.isFollowing(),
            keys: new Set(rows.map(row => row.dataset.rowKey ?? "")),
            rows: onScreen,
            parts,
            elements
        };
    }

    /** Whether an element is inside a part already going. */
    function isInGoing(element: Element): boolean {
        for (const held of going.keys()) {
            if (held !== element && held.contains(element)) {
                return true;
            }
        }
        return false;
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

    /** Takes a part going out of the flow, its room closing as the stack settles; where it stands is placed once the layout is read. */
    function lift(element: HTMLElement, width: number): void {
        Object.assign(element.style, { position: "absolute", top: "0px", left: "0px", width: `${width}px`, margin: "0", visibility: "visible" });
    }

    /** A part its row keeps, hidden now: going, it is drawn where it stood; done, it is left as its row draws it. */
    function hiding(element: HTMLElement): Leaving {
        return { element, done: () => liftedStyles.forEach(name => element.style.removeProperty(name)) };
    }

    /** Places a part out of the flow in its container, laid out at `holder`: at a box on screen less what its container plays from. */
    function placeAt(element: HTMLElement, holder: DOMRect, screen: { left: number, top: number }, from: Offset): void {
        Object.assign(element.style, { top: `${screen.top - holder.top - from.y}px`, left: `${screen.left - holder.left - from.x}px` });
    }

    /** The laid-out box of the container a part out of the flow is placed in. */
    function holderBox(element: HTMLElement): DOMRect | undefined {
        return ((element.offsetParent as HTMLElement | null) ?? element.parentElement)?.getBoundingClientRect();
    }

    /** A part going: kept out of the flow while it fades, then taken away; a cancel takes it away at once. */
    function fadeAway(element: HTMLElement, done: () => void, from: number, timing: KeyframeAnimationOptions): void {
        going.set(element, done);
        const finish = (): void => {
            going.delete(element);
            done();
        };
        const fade = play(element, [{ opacity: from }, { opacity: 0 }], { ...timing, id: "chat-tail-out" });
        if (fade) {
            fade.onfinish = finish;
            fade.oncancel = finish;
        }
        else {
            finish();
        }
    }

    /** Plays the change, from where the tail stood before it to where it now lays out. */
    function settle(was: Before): void {
        const content = options.content.value;
        const handed = leaving;
        leaving = [];
        const release = (): void => handed.forEach(part => part.done());
        // a time line or a time its row shows again is back in the flow at once, whatever plays
        for (const [element, done] of [...going]) {
            if (keptMarks.includes(element.dataset.tail ?? "") && !element.hidden) {
                going.delete(element);
                motionsOf(element).forEach(animation => animation.cancel());
                done();
            }
        }
        if (isStopped || !content) {
            release();
            return;
        }

        const typing = typingNow();
        // Every row drawn is claimed, whether or not anything plays, so a row drawn again later is
        // never news: a folded line claims the messages that joined it too. Only a row new to the
        // view can arrive.
        const arrivals = rowsOf(content)
            .map(row => ({ row, how: options.arrives(row.dataset.rowKey ?? "") }))
            .filter(({ row, how }) => !was.keys.has(row.dataset.rowKey ?? "") && how !== null);
        const handoff = typing.end === "sent" ? arrivals.find(arrival => arrival.how === "inPlace") : undefined;
        const goes = typing.end === "stopped" || typing.end === "expired" || typing.end === "blocked";
        // This view moves only for what its own place recorded: a message arriving here, typing
        // starting, or typing ending as a person sees it end. An end for what a page brought, or
        // a session's stop, is state, and so is whatever else changed with it.
        const isMotion = arrivals.length > 0 || typing.isStart || goes || !!handoff;
        const scroller = options.scroller();
        const entrance = entranceLeft(scroller);
        if (!was.isFollowing || (!isMotion && entrance === 0)) {
            // Scrolled up, nothing moves; a change of state alone after the entrance is simply there.
            release();
            return;
        }

        const tokens = tokensOf(content);
        const reduced = isMotionReduced();
        // What goes: the parts handed over, and each time line or time read standing that its row now hides.
        const parting = [...handed, ...[...was.parts.values()]
            .filter(part => keptMarks.includes(kindOf(part.id)) && part.element.isConnected && part.element.hidden)
            .map(part => hiding(part.element))];
        const bubbleGoing = parting.find(part => part.element.dataset.tail === "typing:typing");
        const dotsWere = bubbleGoing?.element.querySelector<HTMLElement>("[data-tail='dots:typing']");
        // what the dots showed, read before anything is let go: her message starts from it
        const dotsSeen = dotsWere ? seenOpacity(dotsWere, scroller) : 1;
        // A part going fades from its own strength: what holds it fades on around it, so its
        // ancestors' strength is already in what a person sees.
        const partingSeen = new Map(parting.map(part => [part.element, ownOpacity(part.element)]));

        // Plan step 1: what goes leaves the flow, so the layout read next is final. A part the tail
        // did not read before goes at once; under reduced motion a bubble going keeps its room.
        const lifted: Leaving[] = [];
        for (const part of parting) {
            const at = was.elements.get(part.element);
            const keepsRoom = reduced && part === bubbleGoing && !handoff;
            if (!at || entrance > 0) {
                part.done();
                continue;
            }
            if (!keepsRoom) {
                lift(part.element, at.box.width);
            }
            lifted.push(part);
        }

        // Plan step 2: let go of what this change moves again, each already read where it is on
        // screen: the stack, every part's own move, and anything still going that this change takes
        // the room of. A fade, a grow and the words in it run on inside boxes that now continue from
        // the screen.
        const isFilled = arrivals.length > 0 || typing.isStart;
        for (const [element, done] of [...going]) {
            if (isFilled && element.dataset.tail === "typing:typing") {
                going.delete(element);
                motionsOf(element, true).forEach(animation => animation.cancel());
                done();
            }
        }
        // a foot in flight is let go with the moves, so what it shows now is read first: it opens from there
        const feetWere = new Map<Element, number>();
        for (const animation of motionsOf(content, true)) {
            const holder = (animation.effect as KeyframeEffect | null)?.target;
            if (animation.id === "chat-tail-foot" && holder) {
                feetWere.set(holder, footOf(holder));
            }
        }
        motionsOf(content, true).filter(animation => glideIds.includes(animation.id) || moveIds.includes(animation.id)).forEach(animation => animation.cancel());
        if (bubbleGoing) {
            motionsOf(bubbleGoing.element, true).forEach(animation => animation.cancel());
        }

        // Plan step 3: the reader follows to the newest end, read from layout.
        options.follow();

        // Plan step 4: read after, once, with nothing of the tail's own moving. The stack moved by
        // how far the newest row that stands before and after moved; a row changing height above it
        // does not move where the stack starts. A move longer than the scroller starts just off it.
        const height = scroller?.clientHeight ?? Number.POSITIVE_INFINITY;
        const drawnRows = new Map(rowsOf(content).map(row => [row.dataset.rowKey ?? "", row]));
        let offset = 0;
        for (const stood of was.rows) {
            const now = drawnRows.get(stood.key);
            if (now) {
                offset = Math.max(-height, Math.min(height, stood.top - now.getBoundingClientRect().top));
                break;
            }
        }

        // A time line fades where it stood only while its row rides the glide: what takes its place
        // then moves in, and so is drawn over it. Under reduced motion nothing moves in, so it would
        // be drawn over what stands in its place; and a row that steps, as the rows above the newest
        // do when something above them changes, leaves it behind. Either way it goes at once.
        for (const part of [...lifted]) {
            if (!keptMarks.includes(part.element.dataset.tail ?? "")) {
                continue;
            }
            const key = part.element.closest<HTMLElement>("[data-row-key]")?.dataset.rowKey ?? "";
            const stood = was.rows.find(row => row.key === key);
            const now = drawnRows.get(key);
            if (reduced || !stood || !now || Math.abs(stood.top - now.getBoundingClientRect().top - offset) > .5) {
                lifted.splice(lifted.indexOf(part), 1);
                part.done();
            }
        }

        // where each part going is placed from, and her box, laid out before anything plays
        const holders = new Map(lifted.map(part => [part.element, holderBox(part.element)]));
        const handoffRow = handoff?.row;
        const handoffBox = handoffRow?.querySelector<HTMLElement>("[data-tail='box']") ?? null;
        const handoffAt = handoffBox?.getBoundingClientRect();

        if (entrance > 0) {
            // The open's one motion: what lands while the view enters rides the entrance to its end,
            // closing the rest of the way evenly so a short remainder does not open with a jump.
            if (Math.abs(offset) >= .5 && !reduced) {
                play(content, [{ transform: `translateY(${offset}px)` }, { transform: "none" }], { duration: entrance, easing: tokens.easeState, id: "chat-tail-open" });
            }
            lifted.forEach(part => part.done());
            return;
        }

        // Plan step 5: every motion as data. Each geometric motion of one change shares one timing,
        // so a part inside a moving box lands with it exactly.
        const isLeaveOnly = goes && arrivals.length === 0;
        const geometry: KeyframeAnimationOptions = { duration: isLeaveOnly ? tokens.base : tokens.msg, easing: isLeaveOnly ? tokens.easeState : tokens.easeIn };
        const starts = new Map<Element, Offset>([[content, { x: 0, y: reduced ? 0 : offset }]]);
        const startOf = (element: Element): Offset => {
            const sum = { x: 0, y: 0 };
            for (let at: Element | null = element.parentElement; at; at = at.parentElement) {
                const start = starts.get(at);
                if (start) {
                    sum.x += start.x;
                    sum.y += start.y;
                }
                if (at === content) {
                    break;
                }
            }
            return sum;
        };
        const isLifted = (element: Element): boolean => lifted.some(part => part.element === element || part.element.contains(element));

        // the parts drawn now, at the rows read before and the rows new here, and in the bubble
        const tailNow = new Set<Element>();
        const stoodKeys = new Set(was.rows.map(stood => stood.key));
        for (const [key, row] of drawnRows) {
            if (stoodKeys.has(key) || !was.keys.has(key)) {
                tailNow.add(row);
            }
        }
        const bubbleNow = [...content.querySelectorAll<HTMLElement>("[data-tail='typing:typing']")].find(element => !isLifted(element) && !going.has(element));
        if (bubbleNow) {
            tailNow.add(bubbleNow);
        }
        const now: Part[] = [];
        for (const holder of tailNow) {
            for (const element of [holder, ...holder.querySelectorAll<HTMLElement>("[data-tail]")] as HTMLElement[]) {
                const id = idOf(element);
                if (id && !isLifted(element) && !going.has(element)) {
                    now.push({ id, element, box: element.getBoundingClientRect() });
                }
            }
        }
        // A hand-off gives her new row the bubble's face and name line: they move into hers.
        const fromOf = (part: Part): Part | undefined => was.parts.get(part.id)
            ?? (handoff && (kindOf(part.id) === "face" || kindOf(part.id) === "name") ? was.parts.get(`${kindOf(part.id)}:typing`) : undefined);
        const taken = new Set(now.flatMap(part => {
            const from = fromOf(part);
            return from ? [from.element] : [];
        }));
        // A bubble that stands is the foot, as the scroller's edge is: a row riding up from below
        // its top is clipped there for as long as it rides, so it comes out from under the bubble.
        // The clip closes with the stack, both linear in the same timing, so from where the row
        // starts to where it lands it never shows past the bubble's top.
        const foot = bubbleNow && !typing.isStart && !reduced && Math.abs(offset) >= .5 ? was.parts.get("typing:typing") : undefined;
        const under = foot
            ? [...tailNow].filter(holder => holder !== bubbleNow).map(holder => ({ holder, by: holder.getBoundingClientRect().bottom + offset - foot.box.top })).filter(clip => clip.by >= .5)
            : [];

        // Plan step 6 and play. The stack first, then its parts in the document's order, so each
        // container's start is planned before what is in it.
        if (!reduced && Math.abs(offset) >= .5) {
            play(content, [{ transform: `translateY(${offset}px)` }, { transform: "none" }], { ...geometry, id: "chat-tail-glide" });
        }
        // Only the foot clips: above and beside, a face gliding in from the row above is drawn whole.
        // Nothing moving in the content starts further from a row than the content is wide and tall
        // plus the glide, so a margin that large never cuts; it is the content's, not the scroller's,
        // since a view with no scroller has the same foot.
        const extent = content.getBoundingClientRect();
        const open = Math.ceil(extent.width + extent.height + Math.abs(offset));
        for (const { holder, by } of under) {
            play(holder, [{ clipPath: `inset(-${open}px -${open}px ${by}px)` }, { clipPath: `inset(-${open}px -${open}px 0px)` }], { ...geometry, id: "chat-tail-foot" });
        }
        // a row this change no longer clips at a foot, its foot cut short, opens from what it showed
        for (const [holder, by] of feetWere) {
            if (holder.isConnected && by >= .5 && !reduced && !under.some(clip => clip.holder === holder)) {
                play(holder, [{ clipPath: `inset(-${open}px -${open}px ${by}px)` }, { clipPath: `inset(-${open}px -${open}px 0px)` }], { ...geometry, id: "chat-tail-foot" });
            }
        }

        const dotsBefore = was.parts.get("dots:typing");
        // Text crossing text: a name line going as a new one takes its place, and Edited taking the
        // room of the time going beside it. Two words drawn over each other read as neither, so what
        // goes is gone by half way and what comes in comes in over the second half, one half each.
        const crossHalf = tokens.base / 2;
        const namesGoing = new Set<Element>();
        for (const part of now) {
            const kind = kindOf(part.id);
            if (part.element === handoffBox || kind === "meta" || kind === "stamp" || kind === "box" || kind === "dots") {
                continue;
            }
            const from = fromOf(part);
            // a face or a name line new in a bubble that stays crosses with the one going, both where it stood
            const crossing = parting.find(other => other !== bubbleGoing && other.element !== part.element && idOf(other.element) === part.id);
            if (crossing) {
                if (kind === "name") {
                    namesGoing.add(crossing.element);
                }
                if (!reduced) {
                    const timing = kind === "name" ? { delay: crossHalf, duration: crossHalf, fill: "backwards" as const } : { duration: tokens.base };
                    play(part.element, [{ opacity: 0 }, { opacity: 1 }], { ...timing, easing: tokens.easeState, id: "chat-tail-in" });
                }
                continue;
            }
            if (!from || reduced) {
                continue;
            }
            const start = startOf(part.element);
            // A face hangs from its foot, and a row's face column is as tall as its line, so a face
            // moves from foot to foot; anything else moves from its top.
            const y = kind === "face" ? from.box.bottom - (part.box.bottom + start.y) : from.box.top - (part.box.top + start.y);
            const by = { x: from.box.left - (part.box.left + start.x), y };
            if (kind === "edited" && Math.abs(by.x) >= .5) {
                // Edited moving along its line crosses the time going beside it, so it does not
                // move: it fades out where it stood as the time does, then in where it now lays out.
                // It holds a transform, so a later change lets it go and replans it as a move.
                const stood = `translate(${by.x}px, ${by.y}px)`;
                play(part.element, [
                    { offset: 0, transform: stood, opacity: 1, easing: tokens.easeState },
                    { offset: .5, transform: stood, opacity: 0 },
                    { offset: .5, transform: "none", opacity: 0, easing: tokens.easeState },
                    { offset: 1, transform: "none", opacity: 1 }
                ], { duration: 2 * crossHalf, id: "chat-tail-move" });
            }
            else if (Math.abs(by.x) >= .5 || Math.abs(by.y) >= .5) {
                starts.set(part.element, by);
                play(part.element, [{ transform: `translate(${by.x}px, ${by.y}px)` }, { transform: "none" }], { ...geometry, id: "chat-tail-move" });
            }
        }

        // Each message new here fades in riding the stack; hers grows out of the dots.
        for (const { row } of arrivals) {
            if (row === handoffRow && handoffBox && handoffAt && dotsBefore) {
                grow(row, handoffBox, handoffAt, dotsBefore.box, startOf(handoffBox), dotsSeen, tokens, reduced);
            }
            else {
                // The run's face moving into her new row is the face that stood a moment ago, so it
                // keeps its strength while the rest of the row fades in around it.
                const face = now.find(other => kindOf(other.id) === "face" && row.contains(other.element) && was.parts.has(other.id))?.element;
                const fadeIn = (element: Element): void => void play(element, [{ opacity: 0 }, { opacity: 1 }], { duration: tokens.msg, easing: reduced ? tokens.easeState : tokens.easeIn, id: "chat-tail-in" });
                if (face) {
                    fadeAround(row, face, fadeIn);
                }
                else {
                    fadeIn(row);
                }
            }
        }

        // Typing starting: its dots and name line fade in riding the stack, and a face new with it;
        // a face moving in from her run keeps its strength.
        if (typing.isStart && bubbleNow) {
            for (const part of now) {
                const kind = kindOf(part.id);
                if (bubbleNow.contains(part.element) && (kind === "dots" || kind === "name" || (kind === "face" && !fromOf(part)))) {
                    play(part.element, [{ opacity: 0 }, { opacity: 1 }], { duration: tokens.msg, easing: reduced ? tokens.easeState : tokens.easeIn, id: "chat-tail-in" });
                }
            }
        }

        // What goes: placed where it stood on screen, less what its container now plays from, then faded.
        for (const part of lifted) {
            const at = was.elements.get(part.element) as Part;
            const seen = partingSeen.get(part.element) ?? 1;
            // a face or a name line a part drawn now takes is not drawn twice
            part.element.querySelectorAll<HTMLElement>("[data-tail^='face:'], [data-tail^='name:']").forEach(inner => {
                if (taken.has(inner)) {
                    inner.style.visibility = "hidden";
                }
            });
            const isKept = part.element.style.position !== "absolute";
            const holder = holders.get(part.element);
            if (part === bubbleGoing && handoffBox && handoffAt && dotsBefore && holder) {
                // the dots ride her box's corner as it grows, fading over its first third
                placeDots(part.element, holder, dotsBefore, at, handoffAt, startOf(handoffBox), reduced, tokens);
                fadeAway(part.element, part.done, Math.min(seen, dotsSeen), reduced
                    ? { duration: tokens.msg, easing: tokens.easeState }
                    : { duration: .35 * tokens.msg, easing: tokens.easeOut });
                continue;
            }
            if (!isKept && holder) {
                placeAt(part.element, holder, at.box, startOf(part.element));
            }
            if (part === bubbleGoing) {
                // the bubble goes down out under the edge with the stack; under reduced motion it fades where it stands
                fadeAway(part.element, part.done, seen, reduced ? { duration: tokens.fast, easing: tokens.easeState } : { duration: tokens.base, easing: tokens.easeState });
            }
            else if (part.element.dataset.tail === "stamp") {
                // a time its row no longer shows gives Edited its room, so it is gone by half way, riding its row
                fadeAway(part.element, part.done, seen, { duration: crossHalf, easing: tokens.easeState });
            }
            else if (keptMarks.includes(part.element.dataset.tail ?? "")) {
                // a time line its row no longer shows fades out where it stood, riding its row
                fadeAway(part.element, part.done, seen, { duration: tokens.fast, easing: tokens.easeOut });
            }
            else if (!reduced) {
                fadeAway(part.element, part.done, seen, { duration: namesGoing.has(part.element) ? crossHalf : tokens.base, easing: tokens.easeState });
            }
            else {
                part.done();
            }
        }
    }

    /**
     * Grows her message out of the dots: her box from the dots' box on screen to its own, clipped
     * from the corner by her face; her words, and every other part of her row with no motion of its
     * own, coming in from a third of the way to nine tenths; her time line rising in after. Under
     * reduced motion nothing grows: her box stands where it lays out with the dots fading over its
     * corner, while everything in her row but the box and her face fades in.
     */
    function grow(row: HTMLElement, box: HTMLElement, to: DOMRect, from: DOMRect, start: Offset, seen: number, tokens: Tokens, reduced: boolean): void {
        if (!reduced) {
            const dx = from.left - (to.left + start.x);
            const dy = from.bottom - (to.bottom + start.y);
            const top = Math.max(0, to.height - from.height);
            const right = Math.max(0, to.width - from.width);
            play(box, [
                { transform: `translate(${dx}px, ${dy}px)`, clipPath: `inset(${top}px ${right}px 0 0 round var(--chat-r-bubble))` },
                { transform: "none", clipPath: "inset(0 0 0 0 round var(--chat-r-bubble))" }
            ], { duration: tokens.msg, easing: tokens.easeIn, id: "chat-tail-handoff" });
        }
        const words: KeyframeAnimationOptions = reduced
            ? { duration: tokens.msg, easing: tokens.easeState, id: "chat-tail-in" }
            : { delay: .3 * tokens.msg, duration: .6 * tokens.msg, easing: tokens.easeState, fill: "backwards", id: "chat-tail-handoff" };
        const fadeIn = (element: Element): void => void play(element, [{ opacity: 0 }, { opacity: 1 }], words);
        [...box.children].forEach(fadeIn);
        // the rest of her row that is not a part of its own: files beside text, a quote, a label, reactions
        const rest = (holder: Element): void => {
            for (const child of holder.children) {
                if ((child as HTMLElement).dataset?.tail !== undefined) {
                    continue;
                }
                if (child.querySelector("[data-tail]")) {
                    rest(child);
                }
                else {
                    fadeIn(child);
                }
            }
        };
        rest(row);
        const meta = row.querySelector<HTMLElement>("[data-tail='meta']");
        if (meta && !meta.hidden) {
            if (reduced) {
                fadeIn(meta);
            }
            else {
                play(meta, [{ opacity: 0, transform: "translateY(3px)" }, { opacity: 1, transform: "none" }], { delay: .45 * tokens.msg, duration: tokens.base, easing: tokens.easeIn, fill: "backwards", id: "chat-tail-in" });
            }
        }
        if (seen < .999) {
            // the dots were still coming in: her row comes the rest of the way from what they showed
            play(row, [{ opacity: seen }, { opacity: 1 }], { duration: .6 * tokens.msg, easing: tokens.easeState, id: "chat-tail-in" });
        }
    }

    /**
     * Places the going bubble in its container, laid out at `holder`, so its dots stand on her box's
     * laid-out corner, and moves it as her box moves, so the two are one box while the dots fade.
     */
    function placeDots(element: HTMLElement, holder: DOMRect, dots: Part, bubble: Part, to: DOMRect, start: Offset, reduced: boolean, tokens: Tokens): void {
        const inBubble = { x: dots.box.left - bubble.box.left, y: dots.box.bottom - bubble.box.top };
        Object.assign(element.style, { top: `${to.bottom - holder.top - inBubble.y}px`, left: `${to.left - holder.left - inBubble.x}px` });
        if (!reduced) {
            const dx = dots.box.left - (to.left + start.x);
            const dy = dots.box.bottom - (to.bottom + start.y);
            play(element, [{ transform: `translate(${dx}px, ${dy}px)` }, { transform: "none" }], { duration: tokens.msg, easing: tokens.easeIn, id: "chat-tail-handoff" });
        }
    }

    // Before the render, so where the tail stands is read before anything in it moves. Every
    // writer records in the same pass as its change of state, so the count and the state move together.
    watch([() => {
        const place = options.place();
        return place ? motion?.versionOf(place) : undefined;
    }, options.drawn], () => {
        if (isStopped || before) {
            return;
        }
        before = read();
        // after the whole render, every part's coming and going included
        void nextTick(() => {
            const was = before;
            before = null;
            if (was) {
                settle(was);
            }
            else {
                leaving.splice(0).forEach(part => part.done());
            }
        });
    }, { flush: "pre" });

    const tail: FeedTail = {
        leave: (element, done) => {
            // A part going with no change read before it has nothing to play from, and goes at once.
            if (!before || isStopped) {
                done();
                return;
            }
            leaving.push({ element: element as HTMLElement, done });
        }
    };

    provide(feedTailKey, tail);

    onBeforeUnmount(() => {
        isStopped = true;
        leaving.splice(0).forEach(part => part.done());
        [...going.values()].forEach(done => done());
        going.clear();
    });

    return tail;
}
