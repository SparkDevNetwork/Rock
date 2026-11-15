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

// What the device tells the page about itself, read in one place: its width as the shell
// measures it, whether the person asked for less motion, and whether the pointer can hover.

import { InjectionKey, onScopeDispose, Ref, ref } from "vue";

/**
 * Whether the chat is drawn at a phone's width, provided once by the shell for a part that sits
 * several layers below it, such as a message's marker, to open its menu as a sheet the way the
 * composer's menus do without every layer between carrying the flag. A part drawn alone reads it
 * as false.
 */
export const isPhoneKey: InjectionKey<Ref<boolean>> = Symbol("chat-is-phone");

// #region The device's media answers

const reducedMotionQuery = "(prefers-reduced-motion: reduce)";
const hoverNoneQuery = "(hover: none)";

/**
 * A media query's answer as the page reads it now. A browser or test page without matchMedia
 * answers no, as a page with no preference would.
 */
function matchesMedia(query: string): boolean {
    return window.matchMedia?.(query).matches ?? false;
}

/**
 * A media query's answer, followed as it changes for as long as the part that asked lives. Called
 * from a component's setup or another effect scope, which is when the listener is ended.
 */
function useMediaFlag(query: string): Ref<boolean> {
    const list = window.matchMedia?.(query);
    const flag = ref(list?.matches ?? false);
    const onChange = (event: { matches: boolean }): void => {
        flag.value = event.matches;
    };

    // an older browser's list may lack the listener methods; it keeps its first answer
    list?.addEventListener?.("change", onChange);
    onScopeDispose(() => list?.removeEventListener?.("change", onChange));
    return flag;
}

/**
 * Whether the person asked the page for less motion, read at the moment a part decides how to
 * move, such as before a smooth scroll or an animation that cannot be left to the sheet.
 *
 * @returns True while less motion is asked for.
 */
export function isMotionReduced(): boolean {
    return matchesMedia(reducedMotionQuery);
}

/**
 * Whether the person asked the page for less motion, followed as it changes: the setting can be
 * turned on with the chat open, and what was drawn moving must go still at once.
 *
 * @returns A ref that is true while less motion is asked for.
 */
export function useIsMotionReduced(): Ref<boolean> {
    return useMediaFlag(reducedMotionQuery);
}

/**
 * Whether the main pointer cannot hover, a finger, read when a part decides whether to move focus
 * or wait for a hover that will never come. Asked at a moment, never followed: no part redraws
 * when a mouse is plugged in.
 *
 * @returns True when the pointer cannot hover.
 */
export function isHoverless(): boolean {
    return matchesMedia(hoverNoneQuery);
}

// #endregion The device's media answers
