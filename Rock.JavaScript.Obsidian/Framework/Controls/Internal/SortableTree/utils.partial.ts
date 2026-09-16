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

import { IDragAutoScrollOptions } from "./types.partial";

/**
 * Drives auto-scroll of the nearest scrollable ancestor while a drag is in
 * flight. Created via {@link useDragAutoScroll}; the consumer holds the
 * reference and calls `start` / `stop` from their drag-begin / drag-end hooks.
 */
class DragAutoScroll {
    private static readonly defaultEdgeZonePx = 50;
    private static readonly defaultMaxSpeedPxPerFrame = 12;

    /** Distance from the container's edge at which auto-scroll kicks in. */
    private readonly edgeZonePx: number;

    /**
     * Maximum scroll speed when the cursor is right at the edge. Speed ramps
     * linearly down to zero as the cursor moves away from the edge.
     */
    private readonly maxSpeedPxPerFrame: number;

    /** Whether the auto-scroll is currently active. */
    private active = false;

    /** The nearest scrollable ancestor of the dragged element. */
    private scrollContainer: HTMLElement | null = null;

    /** The current vertical position of the pointer. */
    private pointerY = 0;

    /** The ID of the current requestAnimationFrame callback. */
    private rafId: number | null = null;

    constructor(options?: IDragAutoScrollOptions) {
        this.edgeZonePx = options?.edgeZonePx ?? DragAutoScroll.defaultEdgeZonePx;
        this.maxSpeedPxPerFrame = options?.maxSpeedPxPerFrame ?? DragAutoScroll.defaultMaxSpeedPxPerFrame;
    }

    /**
     * Starts auto-scrolling the nearest scrollable ancestor of the dragged
     * element and tags the body with `sortable-tree-dragging` (used by CSS to
     * suppress per-node hover affordances during the drag).
     */
    public start(draggedEl: HTMLElement): void {
        if (this.active) {
            return;
        }

        document.body.classList.add("sortable-tree-dragging");

        this.scrollContainer = this.findScrollContainer(draggedEl);

        this.active = true;
        document.addEventListener("pointermove", this.onPointerMove, { passive: true });

        if (this.scrollContainer) {
            this.rafId = requestAnimationFrame(this.tick);
        }
    }

    /** Stops the auto-scroll loop and clears the body class. */
    public stop(): void {
        if (!this.active) {
            return;
        }

        this.active = false;
        this.scrollContainer = null;
        document.removeEventListener("pointermove", this.onPointerMove);
        document.body.classList.remove("sortable-tree-dragging");

        if (this.rafId !== null) {
            cancelAnimationFrame(this.rafId);
            this.rafId = null;
        }
    }

    /**
     * Walks up from `el` to find the nearest scrollable ancestor, defined as
     * the first element whose computed `overflow-y` is `auto` or `scroll`.
     * Returns null if none exists short of the document element.
     */
    private findScrollContainer(el: HTMLElement): HTMLElement | null {
        let parent = el.parentElement;
        while (parent) {
            const cs = getComputedStyle(parent);
            if (cs.overflowY === "auto" || cs.overflowY === "scroll") {
                return parent;
            }
            parent = parent.parentElement;
        }
        return null;
    }

    /**
     * Tracks the cursor's vertical position so the rAF tick can compute
     * distance to the scroll container's edges without recomputing on every
     * move. Bound as an arrow property so `this` is preserved when used as an
     * event listener and as the rAF callback.
     */
    private onPointerMove = (e: PointerEvent | MouseEvent): void => {
        this.pointerY = e.clientY;
    };

    /**
     * Runs every animation frame while a drag is active. Scrolls the container
     * by an amount proportional to how deep the cursor is into the edge zone,
     * so the scroll feels natural rather than stepping abruptly.
     */
    private tick = (): void => {
        if (!this.active || !this.scrollContainer) {
            return;
        }

        const rect = this.scrollContainer.getBoundingClientRect();
        const distFromTop = this.pointerY - rect.top;
        const distFromBottom = rect.bottom - this.pointerY;

        if (distFromTop < this.edgeZonePx) {
            const intensity = Math.max(0, this.edgeZonePx - distFromTop) / this.edgeZonePx;
            this.scrollContainer.scrollTop -= this.maxSpeedPxPerFrame * intensity;
        }
        else if (distFromBottom < this.edgeZonePx) {
            const intensity = Math.max(0, this.edgeZonePx - distFromBottom) / this.edgeZonePx;
            this.scrollContainer.scrollTop += this.maxSpeedPxPerFrame * intensity;
        }

        this.rafId = requestAnimationFrame(this.tick);
    };
}

/**
 * Returns a {@link DragAutoScroll} instance the consumer holds and drives from
 * its drag-begin / drag-end hooks. Each call returns a fresh instance.
 */
export function useDragAutoScroll(options?: IDragAutoScrollOptions): DragAutoScroll {
    return new DragAutoScroll(options);
}
