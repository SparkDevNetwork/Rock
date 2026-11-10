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
// The motions a view's tail plays, which run through the browser's own animations: jsdom has
// none, so a test stands one in that keeps each motion with its element and marks the element
// with the motion's name, where a selector finds it as it finds a row's own motion classes.

/** One motion the tail played. */
export type Played = {
    element: HTMLElement;
    id: string;
    keyframes: Keyframe[];
    options: KeyframeAnimationOptions;
    /** Ends the motion as the browser would, so what waits for it goes. */
    finish: () => void;
};

/** Every tail motion a selector finds by the mark the stand-in leaves on its element. */
export const tailPlayed = (id: string): string => `[data-tail-played~='${id}']`;

/**
 * Stands the browser's animations in from now until restored.
 *
 * @returns Every motion played, in order, and the restore.
 */
export function playAnimations(): { played: Played[], cancelled: Played[], restore: () => void } {
    const played: Played[] = [];
    const cancelled: Played[] = [];
    const running = new Map<Animation, Played>();
    const prototype = Element.prototype as unknown as { animate?: unknown, getAnimations?: unknown };
    const previous = { animate: prototype.animate, getAnimations: prototype.getAnimations };

    prototype.animate = function (this: HTMLElement, keyframes: Keyframe[], options?: KeyframeAnimationOptions): Animation {
        const timing = options ?? {};
        const id = timing.id ?? "";
        this.dataset.tailPlayed = [this.dataset.tailPlayed, id].filter(part => !!part).join(" ");
        const animation = {
            id,
            playState: "running",
            onfinish: null as (() => void) | null,
            oncancel: null as (() => void) | null,
            cancel: (): void => {
                const motion = running.get(animation as unknown as Animation);
                if (motion) {
                    running.delete(animation as unknown as Animation);
                    cancelled.push(motion);
                    animation.oncancel?.();
                }
            }
        };
        const motion: Played = {
            element: this,
            id,
            keyframes,
            options: timing,
            finish: () => {
                if (running.delete(animation as unknown as Animation)) {
                    animation.onfinish?.();
                }
            }
        };
        played.push(motion);
        running.set(animation as unknown as Animation, motion);
        return animation as unknown as Animation;
    };

    prototype.getAnimations = function (this: Element): Animation[] {
        return [...running].filter(([, motion]) => motion.element === this).map(([animation]) => animation);
    };

    return {
        played,
        cancelled,
        restore: () => {
            for (const name of ["animate", "getAnimations"] as const) {
                if (previous[name] === undefined) {
                    delete prototype[name];
                }
                else {
                    prototype[name] = previous[name];
                }
            }
        }
    };
}
