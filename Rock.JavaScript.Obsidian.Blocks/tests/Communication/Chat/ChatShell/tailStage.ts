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
// The browser a view's tail moves in, stood in for jsdom, which has no layout and no animations.
// Animations keep each motion with its element and mark the element with the motion's name, where
// a selector finds it as it finds a row's own motion classes. A stage also lays the view out and
// moves its boxes the way Chromium does, so a cell can read what a person sees in any frame:
//
// - an animation counts from the call that creates it, at the stage's clock: a box read right after
//   `animate()` already includes its first keyframe, and `cancel()` takes it away at once;
// - boxes are laid out from a few rules (blocks stack, a line lays its children side by side on
//   their bottom edge, a fill takes a short conversation's spare height, an absolute box stands at
//   its top and left in its nearest positioned ancestor), with `offsetTop` and `offsetParent` as the
//   real tree has them;
// - what a person sees of an element is its seen opacity, the product over its ancestors, and its
//   box clipped by its clip;
// - the scroller's `scrollHeight` grows by the part of the content's downward transform past its
//   bottom padding, and its `scrollTop` is clamped to it as it shrinks.
//
// Every motion is drawn linear, whatever its easing names, so a sampled frame is exact interpolation.

/** One motion the tail played. */
export type Played = {
    element: HTMLElement;
    id: string;
    keyframes: Keyframe[];
    options: KeyframeAnimationOptions;
    /** When it was played, on the stage's clock. */
    start: number;
    /** Ends the motion as the browser would, so what waits for it goes. */
    finish: () => void;
};

/** Every tail motion a selector finds by the mark the stand-in leaves on its element. */
export const tailPlayed = (id: string): string => `[data-tail-played~='${id}']`;

/** How a stage lays a box out, matched by selector; `w` and `h` make it a box of that size whose children are not laid out. */
export type Size = {
    w?: number;
    h?: number;
    /** Space above and below it in its parent's column. */
    mt?: number;
    mb?: number;
    /** Its own inset on the left, and on the top and bottom of its column. */
    pl?: number;
    pt?: number;
    pb?: number;
    /** Lays its children side by side on their bottom edge, this far apart. */
    line?: number;
};

/** What a stage lays out. */
export type StageOptions = {
    /** The view's scroller and its content, found once the view has drawn them. */
    scroller: () => HTMLElement | null;
    content: () => HTMLElement | null;

    /** The scroller's top on screen, its width and its height. */
    top?: number;
    width?: number;
    height?: number;

    /** The scroller's padding above and below its content. */
    padTop?: number;
    padBottom?: number;

    /** Sizes by selector, the first that matches; an element's own `data-w` and `data-h` come first. */
    sizes?: [string, Size][];

    /** The ancestors a stylesheet positions, by selector; an inline position counts too. */
    positioned?: string;

    /** The element that takes a short conversation's spare height, so its rows stand at the foot. */
    fill?: string;

    /** What a stylesheet keeps out of the flow and unseen, by selector, unless drawn by an inline visibility. */
    hidden?: string;
};

/** A laid-out box, in the document's coordinates before any scroll or motion. */
type Box = { x: number, y: number, w: number, h: number };

type Running = {
    animation: Animation;
    motion: Played;
    delay: number;
    duration: number;
    isFilledBack: boolean;
    isDone: boolean;
};

function rect(left: number, top: number, width: number, height: number): DOMRect {
    return { left, top, width, height, right: left + width, bottom: top + height, x: left, y: top, toJSON: () => ({}) } as DOMRect;
}

function px(value: string | undefined): number {
    const parsed = parseFloat(value ?? "");
    return Number.isNaN(parsed) ? 0 : parsed;
}

/** A transform's translation, from the few forms the tail writes. */
function translationOf(value: string): { x: number, y: number } {
    const both = /translate\(\s*(-?[\d.]+)px\s*,\s*(-?[\d.]+)px\s*\)/.exec(value);
    if (both) {
        return { x: parseFloat(both[1]), y: parseFloat(both[2]) };
    }
    const y = /translateY\(\s*(-?[\d.]+)px\s*\)/.exec(value);
    return { x: 0, y: y ? parseFloat(y[1]) : 0 };
}

/** A clip's insets, top right bottom left, from `inset(...)`. */
function insetsOf(value: string): number[] {
    const inner = /inset\(([^)]*?)(?:round[^)]*)?\)/.exec(value)?.[1] ?? "";
    const parts = inner.trim().split(/\s+/).filter(part => !!part).map(px);
    const [top = 0, right = top, bottom = top, left = right] = parts;
    return [top, right, bottom, left];
}

/** The keyframes with every offset stated, as the browser spaces them. */
function spaced(keyframes: Keyframe[]): Keyframe[] {
    return keyframes.map((frame, index) => ({ ...frame, offset: frame.offset ?? (keyframes.length === 1 ? 1 : index / (keyframes.length - 1)) }));
}

/** A property's value at a progress, interpolated between the frames that name it. */
function valueAt(keyframes: Keyframe[], property: string, progress: number, mix: (a: string, b: string, t: number) => string): string | null {
    const frames = spaced(keyframes).filter(frame => frame[property] !== undefined);
    if (frames.length === 0) {
        return null;
    }
    if (progress <= (frames[0].offset as number)) {
        return String(frames[0][property]);
    }
    for (let index = 1; index < frames.length; index++) {
        const from = frames[index - 1];
        const to = frames[index];
        if (progress <= (to.offset as number)) {
            const span = (to.offset as number) - (from.offset as number);
            return mix(String(from[property]), String(to[property]), span === 0 ? 1 : (progress - (from.offset as number)) / span);
        }
    }
    return String(frames[frames.length - 1][property]);
}

const mixNumber = (a: string, b: string, t: number): string => String(px(a) + (px(b) - px(a)) * t);

const mixTranslate = (a: string, b: string, t: number): string => {
    const from = translationOf(a);
    const to = translationOf(b);
    return `translate(${from.x + (to.x - from.x) * t}px, ${from.y + (to.y - from.y) * t}px)`;
};

const mixInset = (a: string, b: string, t: number): string => {
    const from = insetsOf(a);
    const to = insetsOf(b);
    return `inset(${from.map((value, index) => `${value + (to[index] - value) * t}px`).join(" ")})`;
};

/** The stand-in browser: its clock, its motions and, when it lays a view out, its boxes. */
export type Stage = {
    played: Played[];
    cancelled: Played[];

    /** The stage's clock, in milliseconds. */
    now: () => number;

    /** Moves the clock on, ending each motion whose time is up in the order it ends. */
    advance: (ms: number) => void;

    /** Every motion still running. */
    running: () => Played[];

    /** An element's box on screen, its motions and its clip included. */
    seenBox: (element: Element) => DOMRect;

    /** An element's opacity as seen: the product over it and its ancestors. */
    seenOpacity: (element: Element) => number;

    restore: () => void;
};

/**
 * Stands the browser's animations in, and when a scroller is named lays the view out and moves its
 * boxes as Chromium does, from now until restored.
 *
 * @param options What to lay out; without them only motions are kept.
 *
 * @returns The stage.
 */
export function createStage(options?: StageOptions): Stage {
    const played: Played[] = [];
    const cancelled: Played[] = [];
    const running: Running[] = [];
    let clock = 0;
    const scrollTops = new WeakMap<Element, number>();

    const elementProto = Element.prototype as unknown as Record<string, unknown>;
    const htmlProto = HTMLElement.prototype as unknown as Record<string, unknown>;
    const saved: [Record<string, unknown>, string, PropertyDescriptor | undefined][] = [];
    function replace(target: Record<string, unknown>, name: string, descriptor: PropertyDescriptor): void {
        saved.push([target, name, Object.getOwnPropertyDescriptor(target, name)]);
        Object.defineProperty(target, name, { configurable: true, ...descriptor });
    }

    function end(entry: Running, how: "finish" | "cancel"): void {
        if (entry.isDone) {
            return;
        }
        entry.isDone = true;
        running.splice(running.indexOf(entry), 1);
        (entry.animation as unknown as { playState: string }).playState = how === "finish" ? "finished" : "idle";
        if (how === "cancel") {
            cancelled.push(entry.motion);
            entry.animation.oncancel?.(undefined as never);
        }
        else {
            entry.animation.onfinish?.(undefined as never);
        }
    }

    replace(elementProto, "animate", {
        writable: true,
        value: function (this: HTMLElement, keyframes: Keyframe[], timing?: KeyframeAnimationOptions): Animation {
            const settings = timing ?? {};
            const id = settings.id ?? "";
            this.dataset.tailPlayed = [this.dataset.tailPlayed, id].filter(part => !!part).join(" ");
            const entry = {} as Running;
            const animation = {
                id,
                playState: "running",
                onfinish: null,
                oncancel: null,
                get currentTime(): number {
                    return clock - entry.motion.start;
                },
                effect: { getComputedTiming: () => ({ endTime: entry.delay + entry.duration }) },
                cancel: (): void => end(entry, "cancel"),
                finish: (): void => end(entry, "finish")
            } as unknown as Animation;
            const motion: Played = { element: this, id, keyframes, options: settings, start: clock, finish: () => end(entry, "finish") };
            Object.assign(entry, {
                animation,
                motion,
                delay: Number(settings.delay ?? 0),
                duration: Number(settings.duration ?? 0),
                isFilledBack: settings.fill === "backwards" || settings.fill === "both",
                isDone: false
            });
            played.push(motion);
            running.push(entry);
            return animation;
        }
    });

    replace(elementProto, "getAnimations", {
        writable: true,
        value: function (this: Element, settings?: { subtree?: boolean }): Animation[] {
            return running
                .filter(entry => entry.motion.element === this || (settings?.subtree === true && this.contains(entry.motion.element)))
                .map(entry => entry.animation);
        }
    });

    /** A property's value on an element now, from the newest motion that draws it. */
    function animated(element: Element, property: string, mix: (a: string, b: string, t: number) => string): string | null {
        let value: string | null = null;
        for (const entry of running) {
            if (entry.motion.element !== element) {
                continue;
            }
            const local = clock - entry.motion.start - entry.delay;
            if (local < 0 && !entry.isFilledBack) {
                continue;
            }
            const progress = entry.duration <= 0 ? 1 : Math.min(1, Math.max(0, local / entry.duration));
            value = valueAt(entry.motion.keyframes, property, progress, mix) ?? value;
        }
        return value;
    }

    function advance(ms: number): void {
        const until = clock + ms;
        for (; ;) {
            const next = running
                .map(entry => ({ entry, at: entry.motion.start + entry.delay + entry.duration }))
                .filter(item => item.at <= until)
                .sort((a, b) => a.at - b.at)[0];
            if (!next) {
                break;
            }
            clock = Math.max(clock, next.at);
            end(next.entry, "finish");
        }
        clock = until;
        // the browser clamps a scroller to its end as the content's transform shrinks
        const scroller = options?.scroller();
        if (scroller) {
            const top = scroller.scrollTop;
            scroller.scrollTop = top;
        }
    }

    // #region Layout

    const sizes = options?.sizes ?? [];
    const sizeCache = new WeakMap<Element, Size>();

    function sizeOf(element: Element): Size {
        const cached = sizeCache.get(element);
        if (cached) {
            return cached;
        }
        const html = element as HTMLElement;
        const found = sizes.find(([selector]) => element.matches(selector))?.[1] ?? {};
        const size: Size = { ...found };
        if (html.dataset?.w !== undefined) {
            size.w = Number(html.dataset.w);
        }
        if (html.dataset?.h !== undefined) {
            size.h = Number(html.dataset.h);
        }
        return size;
    }

    function isPositioned(element: Element): boolean {
        const position = (element as HTMLElement).style?.position;
        return position === "relative" || position === "absolute" || (!!options?.positioned && element.matches(options.positioned));
    }

    function isHidden(element: Element): boolean {
        return !!options?.hidden && element.matches(options.hidden) && (element as HTMLElement).style?.visibility !== "visible";
    }

    function isOut(element: Element): boolean {
        return (element as HTMLElement).style?.position === "absolute" || (!!options?.hidden && element.matches(options.hidden));
    }

    function isShown(element: Element): boolean {
        return (element as HTMLElement).style?.display !== "none";
    }

    /** An element's width when nothing gives it one: its widest child's. */
    function natural(element: Element): number {
        const size = sizeOf(element);
        if (size.w !== undefined) {
            return size.w;
        }
        const children = [...element.children].filter(child => isShown(child) && !isOut(child));
        if (size.line !== undefined) {
            return (size.pl ?? 0) + children.reduce((sum, child) => sum + natural(child), 0) + Math.max(0, children.length - 1) * size.line;
        }
        return (size.pl ?? 0) + children.reduce((widest, child) => Math.max(widest, natural(child)), 0);
    }

    /** An element's height laid out at a width. */
    function heightOf(element: Element, width: number): number {
        const size = sizeOf(element);
        if (size.h !== undefined) {
            return size.h;
        }
        const children = [...element.children].filter(child => isShown(child) && !isOut(child));
        const inner = width - (size.pl ?? 0);
        if (size.line !== undefined) {
            return children.reduce((tallest, child) => Math.max(tallest, heightOf(child, natural(child))), 0);
        }
        return (size.pt ?? 0) + (size.pb ?? 0) + children.reduce((sum, child) => {
            const own = sizeOf(child);
            return sum + (own.mt ?? 0) + heightOf(child, own.w ?? inner) + (own.mb ?? 0);
        }, 0);
    }

    /** Lays an element and everything in it out at a place, into the boxes. */
    function place(element: Element, x: number, y: number, width: number, boxes: Map<Element, Box>, spare = 0): void {
        const size = sizeOf(element);
        const height = heightOf(element, width) + spare;
        boxes.set(element, { x, y, w: width, h: height });
        if (size.h !== undefined) {
            return;
        }
        const left = x + (size.pl ?? 0);
        const inner = width - (size.pl ?? 0);
        let at = y + (size.pt ?? 0);
        let across = left;
        const children = [...element.children].filter(isShown);
        const inFlow = children.filter(child => !isOut(child));
        // a fill takes what the column has spare, so a short conversation stands at the foot
        const fill = options?.fill ? inFlow.find(child => child.matches(options.fill as string)) : undefined;
        const spareHeight = fill && spare > 0 ? spare : 0;
        for (const child of children) {
            if (isOut(child)) {
                continue;
            }
            const own = sizeOf(child);
            if (size.line !== undefined) {
                const childWidth = natural(child);
                const childHeight = heightOf(child, childWidth);
                const bottom = y + height;
                place(child, across, bottom - childHeight, childWidth, boxes);
                across += childWidth + size.line;
                continue;
            }
            at += own.mt ?? 0;
            const childWidth = own.w ?? inner;
            place(child, left, at, childWidth, boxes, child === fill ? spareHeight : 0);
            at += (boxes.get(child)?.h ?? 0) + (own.mb ?? 0);
        }
        // an absolute box stands at its top and left in its nearest positioned ancestor, laid out at its own width
        for (const child of children.filter(isOut)) {
            const holder = holderOf(child);
            const base = holder ? boxes.get(holder) : undefined;
            const style = (child as HTMLElement).style;
            const childWidth = style.width ? px(style.width) : natural(child);
            place(child, (base?.x ?? 0) + px(style.left), (base?.y ?? 0) + px(style.top), childWidth, boxes);
        }
    }

    function holderOf(element: Element): Element | null {
        for (let parent = element.parentElement; parent; parent = parent.parentElement) {
            if (isPositioned(parent)) {
                return parent;
            }
        }
        return null;
    }

    const top = options?.top ?? 0;
    const width = options?.width ?? 480;
    const height = options?.height ?? 600;
    const padTop = options?.padTop ?? 0;
    const padBottom = options?.padBottom ?? 0;

    /** The view laid out: the scroller at its place, the content in it from its top padding, a fill taking a short conversation's room. */
    function layout(): Map<Element, Box> {
        const boxes = new Map<Element, Box>();
        const scroller = options?.scroller();
        const content = options?.content();
        if (!scroller || !content) {
            return boxes;
        }
        boxes.set(scroller, { x: 0, y: top, w: width, h: height });
        const room = height - padTop - padBottom;
        const spare = Math.max(0, room - heightOf(content, width));
        place(content, 0, top + padTop, width, boxes, spare);
        return boxes;
    }

    /** An element's translation now, with its ancestors' up to the scroller. */
    function moved(element: Element): { x: number, y: number } {
        const scroller = options?.scroller();
        let x = 0;
        let y = 0;
        for (let at: Element | null = element; at && at !== scroller; at = at.parentElement) {
            const value = animated(at, "transform", mixTranslate);
            if (value) {
                const by = translationOf(value);
                x += by.x;
                y += by.y;
            }
        }
        return { x, y };
    }

    function screenBox(element: Element): DOMRect {
        const box = layout().get(element);
        const scroller = options?.scroller();
        if (!box || !scroller) {
            return rect(0, 0, 0, 0);
        }
        const scroll = element === scroller ? 0 : scroller.scrollTop;
        const by = element === scroller ? { x: 0, y: 0 } : moved(element);
        return rect(box.x + by.x, box.y - scroll + by.y, box.w, box.h);
    }

    function contentExtent(): { bottom: number, moved: number } {
        const scroller = options?.scroller();
        const content = options?.content();
        if (!scroller || !content) {
            return { bottom: 0, moved: 0 };
        }
        const box = layout().get(content);
        const value = animated(content, "transform", mixTranslate);
        return { bottom: padTop + (box?.h ?? 0), moved: value ? translationOf(value).y : 0 };
    }

    // a style read sees an element's opacity as its motions draw it now, as the browser's does
    const windowRecord = window as unknown as Record<string, unknown>;
    const ownStyle = window.getComputedStyle.bind(window);
    replace(windowRecord, "getComputedStyle", { writable: true, value: (element: Element, pseudo?: string | null): CSSStyleDeclaration => {
        const style = ownStyle(element, pseudo);
        const value = animated(element, "opacity", mixNumber);
        if (value === null) {
            return style;
        }
        return new Proxy(style, { get: (target, name) => name === "opacity" ? value
            : name === "getPropertyValue" ? (property: string): string => property === "opacity" ? value : target.getPropertyValue(property)
                : typeof Reflect.get(target, name) === "function" ? (Reflect.get(target, name) as (...args: unknown[]) => unknown).bind(target) : Reflect.get(target, name) });
    } });

    if (options) {
        replace(htmlProto, "getBoundingClientRect", { writable: true, value: function (this: HTMLElement): DOMRect {
            return screenBox(this);
        } });

        const offsets = (pick: (box: Box, base: Box) => number): PropertyDescriptor => ({ get(this: HTMLElement): number {
            const boxes = layout();
            const box = boxes.get(this);
            if (!box) {
                return 0;
            }
            const holder = holderOf(this);
            const base = (holder && boxes.get(holder)) || { x: 0, y: 0, w: 0, h: 0 };
            return pick(box, base);
        } });
        replace(htmlProto, "offsetTop", offsets((box, base) => box.y - base.y));
        replace(htmlProto, "offsetLeft", offsets((box, base) => box.x - base.x));
        replace(htmlProto, "offsetWidth", offsets(box => box.w));
        replace(htmlProto, "offsetHeight", offsets(box => box.h));
        replace(htmlProto, "offsetParent", { get(this: HTMLElement): Element | null {
            return holderOf(this) ?? document.body;
        } });

        replace(elementProto, "clientHeight", { get(this: Element): number {
            return this === options.scroller() ? height : 0;
        } });
        replace(elementProto, "scrollHeight", { get(this: Element): number {
            if (this !== options.scroller()) {
                return 0;
            }
            const extent = contentExtent();
            return Math.max(height, extent.bottom + padBottom, extent.bottom + Math.max(0, extent.moved));
        } });
        replace(elementProto, "scrollTop", {
            get(this: Element): number {
                return scrollTops.get(this) ?? 0;
            },
            set(this: Element, value: number): void {
                const max = this === options.scroller() ? Math.max(0, this.scrollHeight - height) : 0;
                scrollTops.set(this, Math.min(Math.max(0, value), max));
            }
        });
    }

    // #endregion Layout

    function seenOpacity(element: Element): number {
        const scroller = options?.scroller();
        let product = 1;
        for (let at: Element | null = element; at && at !== scroller; at = at.parentElement) {
            const style = (at as HTMLElement).style;
            if (style?.visibility === "hidden" || isHidden(at)) {
                return 0;
            }
            const value = animated(at, "opacity", mixNumber);
            product *= value !== null ? px(value) : style?.opacity ? px(style.opacity) : 1;
        }
        return product;
    }

    function seenBox(element: Element): DOMRect {
        const box = screenBox(element);
        const clip = animated(element, "clipPath", mixInset);
        if (!clip) {
            return box;
        }
        const [t, r, b, l] = insetsOf(clip);
        return rect(box.left + l, box.top + t, box.width - l - r, box.height - t - b);
    }

    return {
        played,
        cancelled,
        now: () => clock,
        advance,
        running: () => running.map(entry => entry.motion),
        seenBox,
        seenOpacity,
        restore: () => {
            for (const [target, name, descriptor] of saved.reverse()) {
                if (descriptor) {
                    Object.defineProperty(target, name, descriptor);
                }
                else {
                    delete target[name];
                }
            }
        }
    };
}

/**
 * Stands the browser's animations in from now until restored, with no layout.
 *
 * @returns Every motion played, in order, and the restore.
 */
export function playAnimations(): { played: Played[], cancelled: Played[], restore: () => void } {
    return createStage();
}
