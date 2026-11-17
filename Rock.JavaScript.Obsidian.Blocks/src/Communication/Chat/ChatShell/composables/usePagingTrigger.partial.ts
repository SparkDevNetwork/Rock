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

export type PagingTriggerOptions = {
    enabled?: () => boolean;

    /** The list's scroller; the window when none is given. */
    root?: () => HTMLElement | null;

    margin?: string;
};

/**
 * Asks once for one arrival, never while the list says no, and again at once when a page lands
 * and leaves the sentinel in view, as a short page does, which the observer would never report.
 *
 * @param sentinel The element at the list's end, while drawn.
 * @param loadMore Asks for the next page. Its promise holds the next ask and lets a list whose
 *     `enabled` does not change around each page ask again after a short one; a failed page is
 *     asked again only on a new arrival, since at once it would only fail again.
 * @param options When the list may ask, and against what it scrolls.
 */
export function usePagingTrigger(sentinel: Ref<HTMLElement | undefined>, loadMore: () => unknown, options: PagingTriggerOptions = {}): void {
    const enabled = options.enabled ?? ((): boolean => true);
    let observer: IntersectionObserver | null = null;
    let isInView = false;

    // Held from an ask until the list lets go of it, as the observer reports one arrival twice.
    let isAsking = false;
    let isPromised = false;

    // Set once the list has gone, so a page settling after it, as one asked before a switch to
    // another conversation does, asks nothing more of a loader that now reads the new one.
    let isDisposed = false;

    function ask(): void {
        if (isDisposed || isAsking || !isInView || !enabled()) {
            return;
        }

        isAsking = true;
        const asked = loadMore();
        if (asked instanceof Promise) {
            isPromised = true;
            asked.then(() => {
                isPromised = false;
                isAsking = false;
                ask();
            }, () => {
                isPromised = false;
                isAsking = isInView;
            });
        }
    }

    function watchSentinel(element: HTMLElement | undefined): void {
        observer?.disconnect();
        observer = null;
        isInView = false;

        // a new sentinel is a new arrival, unless an ask is still out
        isAsking = isPromised;
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
        isDisposed = true;
        observer?.disconnect();
        observer = null;
    });
}
