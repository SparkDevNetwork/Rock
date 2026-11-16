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

// Loading more as the reader scrolls, the one way every list does it: a sentinel at the list's end
// asks for the next page as it comes into view.

import { onScopeDispose, Ref, watch } from "vue";

/** When a list may ask, and against what it scrolls. */
export type PagingTriggerOptions = {
    /** Whether a next page may be asked for now: one is left and none is out. */
    enabled?: () => boolean;

    /** The list's scroller, which the sentinel is watched against; the window when none is given. */
    root?: () => HTMLElement | null;

    /** How far ahead of the sentinel coming into view the page is asked for, as a CSS margin. */
    margin?: string;
};

/**
 * Asks for a list's next page as its sentinel comes into view: once for one arrival, never while
 * the list says no, and again at once when a page lands and leaves the sentinel still in view, as
 * a short page does, which the observer alone would never report.
 *
 * Called in a component's setup; it stops watching as the component goes. A browser with no
 * observer pages nothing by scrolling.
 *
 * @param sentinel The element at the list's end, while drawn.
 * @param loadMore Asks for the next page. A promise holds the next ask until it settles.
 * @param options When the list may ask, and against what it scrolls.
 */
export function usePagingTrigger(sentinel: Ref<HTMLElement | undefined>, loadMore: () => unknown, options: PagingTriggerOptions = {}): void {
    const enabled = options.enabled ?? ((): boolean => true);
    let observer: IntersectionObserver | null = null;
    let isInView = false;

    // Set from an ask until the list lets go of it: its promise settles, it says no and then yes
    // again, or the sentinel leaves view. Two reports of one arrival so ask once. An ask that
    // gave a promise is held until it settles, wherever the sentinel goes meanwhile.
    let isAsking = false;
    let isPromised = false;

    function ask(): void {
        if (isAsking || !isInView || !enabled()) {
            return;
        }

        isAsking = true;
        const asked = loadMore();
        if (asked instanceof Promise) {
            isPromised = true;
            void asked.finally(() => {
                isPromised = false;
                isAsking = false;
                ask();
            });
        }
    }

    function watchSentinel(element: HTMLElement | undefined): void {
        observer?.disconnect();
        observer = null;
        isInView = false;
        if (!element || typeof IntersectionObserver === "undefined") {
            return;
        }

        observer = new IntersectionObserver(entries => {
            isInView = entries.some(entry => entry.isIntersecting);
            if (!isInView) {
                isAsking = isPromised;
                return;
            }

            ask();
        }, { root: options.root?.() ?? null, rootMargin: options.margin });
        observer.observe(element);
    }

    watch(sentinel, watchSentinel, { immediate: true, flush: "post" });

    // The list saying no then yes again is a page that landed; a sentinel still in view asks again.
    watch(enabled, isEnabled => {
        if (isEnabled && !isPromised) {
            isAsking = false;
            ask();
        }
    }, { flush: "post" });

    onScopeDispose(() => {
        observer?.disconnect();
        observer = null;
    });
}
