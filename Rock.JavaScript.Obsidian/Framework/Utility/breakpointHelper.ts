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

import { computed, ComputedRef, ref } from "vue";

/** A Bootstrap responsive breakpoint, or "unknown" before the first measurement. */
export type Breakpoint = "xs" | "sm" | "md" | "lg" | "xl" | "unknown";

/**
 * A snapshot of the active Bootstrap breakpoint, with convenience flags for the
 * common "this size and smaller / larger" comparisons.
 */
export type BreakpointHelper = {
    breakpoint: Breakpoint;
    breakpoints: string;

    isXs: boolean;
    isSm: boolean;
    isMd: boolean;
    isLg: boolean;
    isXl: boolean;

    isXsOrSmaller: boolean;
    isSmOrSmaller: boolean;
    isMdOrSmaller: boolean;
    isLgOrSmaller: boolean;
    isXlOrSmaller: boolean;

    isXsOrLarger: boolean;
    isSmOrLarger: boolean;
    isMdOrLarger: boolean;
    isLgOrLarger: boolean;
    isXlOrLarger: boolean;
};

type CssStyleDisplay = "none" | "inline" | "inline-block" | "block" | "table";

const breakpointDisplays: Record<Exclude<Breakpoint, "unknown">, CssStyleDisplay> = {
    "xs": "none",
    "sm": "inline",
    "md": "inline-block",
    "lg": "block",
    "xl": "table"
};

const displayBreakpoints: Record<CssStyleDisplay, Exclude<Breakpoint, "unknown">> = {
    "none": "xs",
    "inline": "sm",
    "inline-block": "md",
    "block": "lg",
    "table": "xl"
};

/*
    06/24/26 - JMH

    The active breakpoint is read from Bootstrap instead of hard-coded pixel
    widths. A single hidden probe element carries the responsive display
    utilities (d-none d-sm-inline d-md-inline-block d-lg-block d-xl-table), so
    its computed `display` reflects whichever breakpoint Bootstrap's own media
    queries have activated. One probe and one resize listener are shared by
    every consumer, so this is safe to use in components rendered in large lists.

    Reason: One breakpoint source of truth (Bootstrap), observed once app-wide.
*/
let probeElement: HTMLElement | undefined;
const currentHelper = ref<BreakpointHelper>(createBreakpointHelper("unknown"));

function createBreakpointHelper(breakpoint: Breakpoint): BreakpointHelper {
    const partial: Omit<BreakpointHelper, "breakpoints"> = {
        breakpoint,

        isXs: breakpoint === "xs",
        isSm: breakpoint === "sm",
        isMd: breakpoint === "md",
        isLg: breakpoint === "lg",
        isXl: breakpoint === "xl",

        isXsOrSmaller: breakpoint === "xs",
        isSmOrSmaller: ["xs", "sm"].includes(breakpoint),
        isMdOrSmaller: ["xs", "sm", "md"].includes(breakpoint),
        isLgOrSmaller: ["xs", "sm", "md", "lg"].includes(breakpoint),
        isXlOrSmaller: true,

        isXsOrLarger: true,
        isSmOrLarger: ["sm", "md", "lg", "xl"].includes(breakpoint),
        isMdOrLarger: ["md", "lg", "xl"].includes(breakpoint),
        isLgOrLarger: ["lg", "xl"].includes(breakpoint),
        isXlOrLarger: breakpoint === "xl"
    };

    // Every tier at or below the active one, e.g. "xs sm md" while at md.
    const active = (["xs", "sm", "md", "lg", "xl"] as const)
        .filter(bp => partial[`is${bp.charAt(0).toUpperCase()}${bp.slice(1)}OrLarger` as keyof typeof partial] === true);

    return {
        ...partial,
        breakpoints: active.join(" ")
    };
}

function readBreakpoint(): void {
    if (!probeElement) {
        return;
    }

    const display = getComputedStyle(probeElement).display as CssStyleDisplay;
    const next = displayBreakpoints[display] ?? "unknown";

    if (next !== currentHelper.value.breakpoint) {
        currentHelper.value = createBreakpointHelper(next);
    }
}

function ensureProbe(): void {
    if (probeElement || typeof document === "undefined") {
        return;
    }

    const element = document.createElement("div");

    // Builds: "d-none d-sm-inline d-md-inline-block d-lg-block d-xl-table".
    element.className = (Object.keys(breakpointDisplays) as Array<Exclude<Breakpoint, "unknown">>)
        .map(bp => bp === "xs" ? `d-${breakpointDisplays[bp]}` : `d-${bp}-${breakpointDisplays[bp]}`)
        .join(" ");

    // Keep the probe out of layout and invisible while its display flips.
    element.style.setProperty("position", "absolute");
    element.style.setProperty("visibility", "collapse", "important");

    document.body.appendChild(element);
    probeElement = element;

    readBreakpoint();
    window.addEventListener("resize", readBreakpoint, { passive: true });
}

const readonlyHelper = computed<BreakpointHelper>(() => currentHelper.value);

/**
 * Gets the shared, reactive Bootstrap breakpoint helper. The first call lazily
 * creates the probe element and resize listener; every caller shares them.
 */
export function useBreakpointHelper(): ComputedRef<BreakpointHelper> {
    ensureProbe();
    return readonlyHelper;
}
