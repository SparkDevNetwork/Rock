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
// it is on screen in a page the reader is looking at. Beside it, where one mark stands against
// the reader's view, as the feed's New divider tells its unread strip whether to point up or down.

import { onMounted, onScopeDispose, readonly, Ref, ref } from "vue";

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

/** Where one mark stands against the reader's view. */
export type PlaceInView = {
    /** Whether the mark is out of view, as the browser last reported. */
    isOffScreen: Readonly<Ref<boolean>>;

    /** Whether the mark, out of view, is above it rather than below. */
    isAbove: Readonly<Ref<boolean>>;

    /** Watches the mark as it is drawn, in place of the one before; null while none is drawn. */
    watch: (element: Element | null) => void;
};

/**
 * Watches one mark for where it stands against the reader's view: in it, above it or below it. A
 * mark drawn before the list is mounted is watched from the mount, when the scroller it is placed
 * against is drawn. A browser with no observer reports the mark in view.
 *
 * Called in a component's setup; it stops as the component goes.
 *
 * @param root The list's scroller, the view the mark is placed against.
 *
 * @returns The mark's place, and how to tell it which element the mark is.
 */
export function usePlaceInView(root: () => HTMLElement | null): PlaceInView {
    const isOffScreen = ref(false);
    const isAbove = ref(false);
    let mark: Element | null = null;
    let observer: IntersectionObserver | null = null;

    onMounted(() => {
        if (typeof IntersectionObserver === "undefined") {
            return;
        }

        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                isOffScreen.value = !entry.isIntersecting;
                isAbove.value = !!entry.rootBounds && entry.boundingClientRect.bottom < entry.rootBounds.top;
            }
        }, { root: root() });

        if (mark) {
            observer.observe(mark);
        }
    });

    onScopeDispose(() => {
        observer?.disconnect();
        observer = null;
    });

    return {
        isOffScreen: readonly(isOffScreen),
        isAbove: readonly(isAbove),
        watch(element: Element | null): void {
            if (element === mark) {
                return;
            }

            if (mark) {
                observer?.unobserve(mark);
            }

            mark = element;
            if (element) {
                observer?.observe(element);
            }
        }
    };
}
