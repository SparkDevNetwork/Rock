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
// A turned surface's height, stood in for jsdom, which lays nothing out and plays no animation.
// A surface reads as tall as the cell says it is drawn now, it takes the panel's duration and the
// entering curve from the token sheet as a page in the chat would, and every height change it
// plays is kept, so a cell reads what a person would see the surface do between two pages.

/** One height change a surface played. */
export type HeightPlayed = {
    element: Element;
    from: unknown;
    to: unknown;
    duration: unknown;
    easing: unknown;
};

/** The panel's duration and the entering curve the stand-in's surfaces take from the token sheet. */
export const panelMotion = { duration: 300, easing: "cubic-bezier(.2, 0, 0, 1)" };

/**
 * Stands in for the browser's layout and animations of a surface's height until restored.
 *
 * @returns The height changes played, how to size a surface, and how to restore the browser.
 */
export function standInHeights(): { played: HeightPlayed[], frames: Keyframe[][], size: (surface: HTMLElement, height: () => number) => void, restore: () => void } {
    const played: HeightPlayed[] = [];
    // every change's whole frames, for what moves with the height, such as the surface's inset
    const frames: Keyframe[][] = [];
    const proto = HTMLElement.prototype as { animate?: unknown };
    const original = proto.animate;

    proto.animate = function (this: HTMLElement, keyframes: Keyframe[], options: KeyframeAnimationOptions): Animation {
        played.push({ element: this, from: keyframes[0]?.height, to: keyframes[keyframes.length - 1]?.height, duration: options.duration, easing: options.easing });
        frames.push(keyframes);
        return { cancel: () => undefined } as unknown as Animation;
    };

    function size(surface: HTMLElement, height: () => number): void {
        surface.style.setProperty("--chat-t-panel", `${panelMotion.duration}ms`);
        surface.style.setProperty("--chat-ease-in", panelMotion.easing);
        surface.getBoundingClientRect = () => ({ height: height() }) as DOMRect;
    }

    function restore(): void {
        if (original) {
            proto.animate = original;
        }
        else {
            delete proto.animate;
        }
    }

    return { played, frames, size, restore };
}
