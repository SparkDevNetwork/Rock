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

import { onBeforeUnmount, Ref, ref } from "vue";
import { autoUpdate, flip, offset, shift, useFloating } from "@Obsidian/Libs/floating-ui";
import { tokenDuration } from "./brand.partial";

/** A tooltip's state and the handlers its anchor binds. */
export type ChatTooltip = {
    /** The drawn tip, for the placement to measure. */
    tipElement: Ref<HTMLElement | undefined>;
    isTipShown: Ref<boolean>;
    floatingStyles: ReturnType<typeof useFloating>["floatingStyles"];
    hideTip: () => void;
    onPointerEnter: () => void;
    onPointerLeave: () => void;
    onPointerDown: () => void;
    onFocus: () => void;
    onBlur: () => void;
};

/** The design's tip delay, used when the sheet is not on the page. */
const fallbackTipDelayMs = 500;

/**
 * The chat's tooltip for one anchor. The caller draws the tip, class chat-tip, role tooltip, at
 * the floating styles while it is shown, and binds the anchor's handlers. A tip shows once the
 * pointer has rested or a keyboard focus has arrived, never under a press, and hides on a click.
 *
 * @param anchor The element the tip belongs to.
 * @param isDisabled Whether the tip is kept away for now, such as while a menu covers it.
 */
export function useTooltip(anchor: Ref<HTMLElement | undefined>, isDisabled: () => boolean = () => false): ChatTooltip {
    const tipElement = ref<HTMLElement>();
    const isTipShown = ref(false);

    let tipTimer: number | undefined;

    /** Whether the focus that is coming was a press, since a tip under the pointer is noise. */
    let isPointerFocus = false;

    // Above the anchor, or below it when there is no room, kept 8px inside the window.
    const { floatingStyles } = useFloating(anchor, tipElement, {
        placement: "top",
        strategy: "fixed",
        middleware: [offset(8), flip(), shift({ padding: 8 })],
        whileElementsMounted: autoUpdate
    });

    /** Shows the tip once the pointer has rested, so passing over a row of buttons does not flicker tips. */
    function scheduleTip(): void {
        if (isDisabled()) {
            return;
        }

        window.clearTimeout(tipTimer);
        const delay = anchor.value ? tokenDuration(anchor.value, "--chat-d-tip-delay", fallbackTipDelayMs) : fallbackTipDelayMs;
        tipTimer = window.setTimeout(() => {
            isTipShown.value = true;
        }, delay);
    }

    function hideTip(): void {
        window.clearTimeout(tipTimer);
        isTipShown.value = false;
    }

    /**
     * Only a keyboard focus shows the tip. Told apart by the press before it rather than by
     * :focus-visible, which the oldest Safari Rock supports cannot match from script.
     */
    function onFocus(): void {
        if (!isPointerFocus) {
            scheduleTip();
        }
    }

    function onPointerDown(): void {
        isPointerFocus = true;
    }

    function onBlur(): void {
        isPointerFocus = false;
        hideTip();
    }

    onBeforeUnmount(() => {
        window.clearTimeout(tipTimer);
    });

    return {
        tipElement,
        isTipShown,
        floatingStyles,
        hideTip,
        onPointerEnter: scheduleTip,
        onPointerLeave: hideTip,
        onPointerDown,
        onFocus,
        onBlur
    };
}
