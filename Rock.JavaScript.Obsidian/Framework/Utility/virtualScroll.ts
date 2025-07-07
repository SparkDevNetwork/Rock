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

import { computed, onBeforeUnmount, Ref, ref, watch } from "vue";

/**
 * A range of items that should be displayed, including the padding that should
 * be applied before and after the items so everything fits properly.
 */
export type Range = {
    /** The first item index to be displayed. */
    startIndex: number;

    /** The last item index to be displayed, this is inclusive. */
    endIndex: number;

    /** The padding, in pixels, that should preceed the first item. */
    padBefore: number;

    /** The padding, in pixels, that should follow the last item. */
    padAfter: number;
};

/**
 * The options to use when creating a new instance of {@link VirtualScroller}.
 */
export type VirtualOptions = {
    /**
     * The number of visible items expected to fit on screen.
     */
    visibleCount: number;

    /**
     * The height of each item.
     */
    rowHeight: number;

    /**
     * The number of items.
     */
    totalRowCount: number;
};

/**
 * Helper class to handle virtual scrolling. There is no DOM interaction here. This simply handles the logic of which
 * items to scroll and how much buffer space to prepend and append to the real list. This was originally based off
 * https://github.com/tangbc/vue-virtual-scroll-list like VirtualDataRows for the Grid, but there has been a lot of
 * cleanup to simplify and make it less generalized.
 */
export class VirtualScroller {
    // #region Properties

    /** The options we were initialized with. */
    private readonly options: VirtualOptions;

    /** The callback function to call when the range has changed. */
    private readonly rangeUpdated: (range: Range) => void;

    /** Contains the last known scroll offset for this scroller. */
    private offset: number = 0;

    /** The current range of items to be displayed. */
    private range: Range = { startIndex: 0, endIndex: 0, padBefore: 0, padAfter: 0 };

    // #endregion

    // #region Constructors

    /**
     * Creates a new instance of {@link VirtualScroller}.
     *
     * @param options The options to initialize the virtual scroller with.
     * @param rangeUpdated The callback for when the range value changes.
     */
    constructor(options: VirtualOptions, rangeUpdated: (range: Range) => void) {
        this.options = { ...options };
        this.rangeUpdated = rangeUpdated;

        this.checkRange(0, options.visibleCount - 1);
    }

    // #endregion

    // #region Public Functions

    /**
     * Gets the current calculated range for the scroller.
     *
     * @returns The current calculated range for the scroller.
     */
    public getRange(): Range {
        return {
            ...this.range
        };
    }

    /**
     * Informs the virtual scroller that the list of items to be displayed
     * has changed. This should be called anytime anything about the list or
     * order gets modified.
     *
     * @param totalRowCount The new total row count.
     */
    public dataSourceChanged(totalRowCount: number): void {
        this.options.totalRowCount = totalRowCount;

        this.handleScroll(this.offset);
    }

    /**
     * Informs the scroller that a scroll action has taken place. This is the
     * meat of the scroller and kicks off all logic to handle which items
     * need to be displayed.
     *
     * @param offset The offset from start in the virtual scroller it is positioned at.
     */
    public handleScroll(offset: number): void {
        this.offset = offset;

        const firstVisibleIndex = this.getFirstVisibleIndex();
        const lastVisibleIndex = firstVisibleIndex + this.options.visibleCount;

        let start = this.range.startIndex;
        let end = this.range.endIndex;

        if (
            firstVisibleIndex - start < this.options.visibleCount
            || end - lastVisibleIndex < this.options.visibleCount
        ) {
            start = Math.max(firstVisibleIndex - this.options.visibleCount, 0);
            end = Math.min(lastVisibleIndex + this.options.visibleCount, this.getLastIndex());
        }

        this.checkRange(start, end);
    }

    // #endregion

    // #region Private Functions

    /**
     * Determines the index of the first item that is at least partially visible.
     *
     * @returns The first item that is at least partially visible.
     */
    private getFirstVisibleIndex(): number {
        if (this.offset <= 0) {
            return 0;
        }

        return Math.floor(this.offset / this.options.rowHeight);
    }

    /**
     * Gets the index number of the last item.
     *
     * @returns The index of the last item.
     */
    private getLastIndex(): number {
        return this.options.totalRowCount - 1;
    }

    /**
     * Checks if the range is valid and then updates the range if it has
     * changed from the current values.
     *
     * @param start The requested starting index.
     * @param end The requested ending index (inclusive).
     */
    private checkRange(start: number, end: number): void {
        const total = this.options.totalRowCount;

        // Total number of items is less than what we expect to fit on
        // screen plus buffering, just render everything.
        if (total <= (this.options.visibleCount * 3)) {
            start = 0;
            end = this.getLastIndex();
        }

        if (this.range.startIndex !== start || this.range.endIndex !== end) {
            this.updateRange(start, end);
        }
    }

    /**
     * Updates the item range to match the supplied values.
     *
     * @param start The index of the first item to render.
     * @param end The index of the last item to render (inclusive).
     */
    private updateRange(start: number, end: number): void {
        this.range.startIndex = start;
        this.range.endIndex = end;
        this.range.padBefore = this.getPadBefore();
        this.range.padAfter = this.getPadAfter();
        this.rangeUpdated(this.getRange());
    }

    /**
     * Gets the padding to be applied before the first visible item in the list.
     *
     * @returns The number of pixels to use as padding before the first item.
     */
    private getPadBefore(): number {
       return this.options.rowHeight * this.range.startIndex;
    }

    /**
     * Gets the padding to be applied after the last visible item in the list.
     *
     * @returns The number of pixels to use as padding after the last item.
     */
    private getPadAfter(): number {
        const end = this.range.endIndex;
        const lastIndex = this.getLastIndex();

        return (lastIndex - end) * (this.options.rowHeight);
    }

    // #endregion
}


type VirtualScrollOptions<RowItem> = {
    /** The data for each row in the list */
    items: Ref<RowItem[]>
    /** The number of items that are in a row, if it is a 2D grid. If not specified, defaults to 1 */
    itemsPerRow?: number
    /**
     * The HTML element that wraps the list, marking the top and bottom of the list. This is only needed if the scroll
     * container is the body (not provided).
     */
    container?: Ref<HTMLElement | null | undefined>
    /** The HTML element that can scroll. Defaults to the body */
    scrollContainer?: Ref<HTMLElement | null | undefined>
    /** The px height of the scroll container. Will calculate if not provided. */
    scrollContainerHeight?: number
    /** The height of each item in the list */
    rowHeight: number,
    /** The number of items that are visible in the scroll container at any one time. Is calculated if not provided. */
    visibleRowCount?: number
};

type VirtualScrollReturns<RowItem> = {
    virtualItems: Ref<RowItem[]>
    beforePadStyle: Ref<{ height:string }>
    afterPadStyle: Ref<{ height:string }>
};

/**
 * A composable function that provides the actual items to render, along with the spacer sizes above and below the items,
 * given the options provided to produce a virtual scroller. This allows you to have a massive list of items, but only
 * render a small subset so there the pure number of DOM elements doesn't bog down the performance of the browser.
 *
 * This implementation only works with fixed height items to simplify the logic, but allows it to be used with a 2D grid
 * by allowing the number of items per row to be specified.
 */
export function useVirtualScroller<RowItem>(options: VirtualScrollOptions<RowItem>): VirtualScrollReturns<RowItem> {
    const scrollContainer = options.scrollContainer ?? ref(document.documentElement || document.body);
    const range = ref<Range>({ startIndex: 0, endIndex: 0, padBefore: 0, padAfter: 0 });
    let virtualScroller: VirtualScroller | undefined;
    const itemsPerRow = options.itemsPerRow ?? 1;


    /** Contains the set of virtual rows to be rendered in the DOM. */
    const virtualItems = computed((): RowItem[] => {
        const isInvalidRange = !range.value
            || range.value.startIndex >= options.items.value.length
            || range.value.endIndex >= options.items.value.length;

        if (isInvalidRange) {
            return [];
        }

        const actualStartIndex = range.value.startIndex * itemsPerRow;
        const actualEndIndex = Math.min((range.value.endIndex + 1) * itemsPerRow, options.items.value.length);

        return options.items.value.slice(actualStartIndex, actualEndIndex);
    });

    /** Contains the CSS styles for the padding item before the rows. */
    const beforePadStyle = computed(() => ({ height: `${range.value.padBefore}px` }));

    /** Contains the CSS styles for the padding item after the rows. */
    const afterPadStyle = computed(() => ({  height: `${range.value.padAfter}px` }));

    /**
     * Gets the current scroll offset.
     */
    function getScrollOffset(): number {
        return scrollContainer.value?.scrollTop ?? 0;
    }

    /**
     * Gets the height of the visible area in the scrollable element.
     */
    function getClientHeight(): number {
        return options.scrollContainerHeight ?? scrollContainer.value?.clientHeight ?? 0;
    }

    /**
     * Gets the total height of the scrollable content.
     */
    function getScrollHeight(): number {
        return scrollContainer.value?.scrollHeight ?? 0;
    }

    /**
     * Create the virtual scroller we will use to calculate which items to display.
     */
    function createVirtualScroller(): VirtualScroller {
        const scrollContainerHeight =  getClientHeight();
        const visibleCount = options.visibleRowCount ?? Math.ceil(scrollContainerHeight / options.rowHeight);

        return new VirtualScroller({
            visibleCount,
            rowHeight: options.rowHeight,
            totalRowCount: Math.ceil(options.items.value.length / itemsPerRow)
        }, range => onRangeChanged(range));
    }


    /**
     * Called when the range of the virtual scroller has changed.
     *
     * @param r The new range value.
     */
    function onRangeChanged(r: Range): void {
        range.value = r;
    }

    /**
     * Called whenever the element we are monitoring has scrolled.
     */
    function onScroll(): void {
        const offset = getScrollOffset();
        const clientSize = getClientHeight();
        const scrollSize = getScrollHeight();

        // If the scroll is outside the scrollable area, it is probably an
        // overscroll like iOS bounce back effect. Ignore it.
        if (offset < 0 || (offset + clientSize > scrollSize + 1) || !scrollSize) {
            return;
        }

        if (scrollContainer.value === document.documentElement || scrollContainer.value === document.body) {
            // Determine the offset inside the scrollable of our list. Meaning,
            // the grid probably doesn't start at the top of the page. So if the
            // grid starts 200 pixels down the page, our offset should be zero when
            // they have scrolled down by 200 pixels. That is, the top edge of the
            // grid is at the top edge of the scrollable.
            const scrollableOffset = options.container?.value
                ? options.container.value.getBoundingClientRect().top + window.scrollY
                : 0;

            virtualScroller?.handleScroll(Math.floor(Math.max(0, offset - scrollableOffset)));
        }
        else {
            // If the scroll container is not the body or html, then we can just use the offset directly.
            virtualScroller?.handleScroll(offset);
        }
    }


    watch(options.items, () => {
        virtualScroller?.dataSourceChanged(Math.ceil(options.items.value.length / itemsPerRow));
    });

    onBeforeUnmount(() => {
        scrollContainer!.value!.removeEventListener("scroll", onScroll);

        if (virtualScroller) {
            virtualScroller = undefined;
        }
    });

    watch(scrollContainer, () => {
        if (scrollContainer.value && !virtualScroller) {
            virtualScroller = createVirtualScroller();
            onScroll();
            scrollContainer.value.addEventListener("scroll", onScroll, { passive: false });
        }
    }, {immediate: true});


    return {
        virtualItems,
        beforePadStyle,
        afterPadStyle
    };
}