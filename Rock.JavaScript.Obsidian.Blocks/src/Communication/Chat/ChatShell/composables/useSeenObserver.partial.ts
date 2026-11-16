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

// Marking messages seen, the one way the feed and a thread do it: a row counts as seen once any of
// it is on screen in a page the reader is looking at.

import { onScopeDispose } from "vue";

/** What a list asks of its seen watch. */
export type SeenObserver = {
    /** Watches every row under the container that matches the selector and is not watched yet. */
    observeAll: (container: HTMLElement, selector: string) => void;
};

/**
 * Watches a list's rows for being seen. Each row is watched once however often the list redraws,
 * marked data-observed once it is, and the reader's store keeps only the newest it is told of. A
 * browser with no observer marks nothing rather than failing.
 *
 * Called in a component's setup; it stops as the component goes.
 *
 * @param root The list's scroller, which a row must be on screen within.
 * @param onSeen Told of a row on screen.
 *
 * @returns The watch.
 */
export function useSeenObserver(root: () => HTMLElement | null, onSeen: (element: HTMLElement) => void): SeenObserver {
    let observer: IntersectionObserver | null = null;

    /** Made on the first rows, when the scroller is drawn to be watched within. */
    function observerFor(): IntersectionObserver | null {
        if (!observer && typeof IntersectionObserver !== "undefined") {
            observer = new IntersectionObserver(entries => {
                for (const entry of entries) {
                    if (entry.isIntersecting && document.visibilityState === "visible") {
                        onSeen(entry.target as HTMLElement);
                    }
                }
            }, { root: root() });
        }

        return observer;
    }

    onScopeDispose(() => {
        observer?.disconnect();
        observer = null;
    });

    return {
        observeAll(container: HTMLElement, selector: string): void {
            const watching = observerFor();
            if (!watching) {
                return;
            }

            container.querySelectorAll<HTMLElement>(selector).forEach(element => {
                if (element.dataset.observed !== "true") {
                    element.dataset.observed = "true";
                    watching.observe(element);
                }
            });
        }
    };
}
