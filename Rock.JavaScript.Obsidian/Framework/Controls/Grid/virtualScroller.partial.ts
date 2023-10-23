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

// Originally from: https://github.com/tangbc/vue-virtual-scroll-list.
// This is a conversion to TypeScript and also to simplify a few things that
// we don't have a use for. It also reworks a few things to be a bit
// more friendly to TypeScript style of doing things.

/**
 * The type of size calculation type to be used.
 */
const enum CalcType {
    /** Initialization time, we have not determined what calculation type to use. */
    Init = "init",

    /** We have determined that fixed height is currently the best choice. */
    Fixed = "fixed",

    /** We have determined that dynamic height is the choice we must use. */
    Dynamic = "dynamic"
}

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
     * The number of "off-screen" items before and after the edge of the sceen
     * to keep rendered.
     */
    bufferCount: number;

    /**
     * The estimated height of each item.
     */
    estimatedHeight: number;

    /**
     * The list of all unique identifiers representing all items in the list.
     */
    uniqueIds: string[];
};

/**
 * Helper class to handle virtual scrolling. There is no DOM interaction here.
 * This simply handles the logic of which items to scroll and how much buffer
 * space to prepend and append to the real list.
 */
export default class VirtualScroller {
    // #region Properties

    /** The options we were initialized with. */
    private readonly options: VirtualOptions;

    /** The callback function to call when the range has changed. */
    private readonly rangeUpdated: (range: Range) => void;

    /** The hash table to map item identifiers to the known height. */
    private readonly heights: Map<string, number> = new Map<string, number>();

    /**
     * When not `undefined` this contains the total height of all known items.
     * It indicates that we need to keep woring to figure out an average height
     * until it becomes `undefined.
     */
    private firstRangeTotalHeight?: number = 0;

    /** Contains the calculated average height of each row. */
    private firstRangeAverageHeight: number = 0;

    /** Tracks the highest most index whose position has been calculated. */
    private highestCalculatedIndex: number = 0;

    /** The type of item height calculation we are using. */
    private calcType: CalcType = CalcType.Init;

    /**
     * Contains the currently known fixed height if {@link calcType} is
     * {@link CalcType.Fixed}. Will be set to `undefined` if we are in
     * dynamic mode.
     */
    private fixedHeightValue?: number = 0;

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

    /**
     * Performs any cleanup on the virtual scroller that is needed. This should
     * be called when the scroller will no longer be used.
     */
    destroy(): void {
        this.heights.clear();
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
     * @param uniqueIds The new list of unique identifiers in the list.
     */
    public dataSourceChanged(uniqueIds: string[]): void {
        this.heights.forEach((v, key) => {
            if (!uniqueIds.includes(key)) {
                this.heights.delete(key);
            }
        });

        this.options.uniqueIds = uniqueIds;

        this.handleScroll(this.offset);
    }

    /**
     * Informs the virtual scroller that the height of a single item has
     * changed.
     *
     * @param id The identifier of the item whose height changed.
     * @param height The new height of the item.
     */
    public updateHeight(id: string, height: number): void {
        this.heights.set(id, height);

        if (this.calcType === CalcType.Init) {
            // This is the first height we have. Assume fixed size.
            this.fixedHeightValue = height;
            this.calcType = CalcType.Fixed;
        }
        else if (this.calcType === CalcType.Fixed && this.fixedHeightValue !== height) {
            // We are currently fixed size, but we need to switch into
            // dynamic size because the sizes are different.
            this.calcType = CalcType.Dynamic;
            this.fixedHeightValue = undefined;
        }

        // If we we are dynamic height and have not yet calculated enough
        // heights to get a good average, then keep calculating.
        if (this.calcType !== CalcType.Fixed && this.firstRangeTotalHeight !== undefined) {
            if (this.heights.size < Math.min(this.options.visibleCount, this.options.uniqueIds.length)) {
                // We need more data to get a good average sampling.
                this.firstRangeTotalHeight = [...this.heights.values()].reduce((acc, val) => acc + val, 0);
                this.firstRangeAverageHeight = Math.round(this.firstRangeTotalHeight / this.heights.size);
            }
            else {
                // We got enough data, we can disable this logic now by
                // setting the value to undefined.
                this.firstRangeTotalHeight = undefined;
            }
        }
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

        if (firstVisibleIndex - start < this.options.bufferCount) {
            start = Math.max(firstVisibleIndex - (this.options.bufferCount * 2), 0);

            // Only update the end range if we have too much buffer.
            if (end - lastVisibleIndex > (this.options.bufferCount * 3)) {
                end = Math.min(lastVisibleIndex + (this.options.bufferCount * 2), this.getLastIndex());
            }
        }

        if (end - lastVisibleIndex < this.options.bufferCount) {
            end = Math.min(lastVisibleIndex + (this.options.bufferCount * 2), this.getLastIndex());

            // Only update start if we haven't already updated it and it has
            // too much buffer.
            if (start === this.range.startIndex && firstVisibleIndex - start > (this.options.bufferCount * 3)) {
                start = Math.max(firstVisibleIndex - (this.options.bufferCount * 2), 0);
            }
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

        // A fixed height makes this super easy to determine.
        if (this.isFixedType() && this.fixedHeightValue !== undefined) {
            return Math.floor(this.offset / this.fixedHeightValue);
        }

        // Perform a search to find the first visible index.
        let low = 0;
        let middle = 0;
        let high = this.options.uniqueIds.length;

        while (low <= high) {
            middle = low + Math.floor((high - low) / 2);
            const middleOffset = this.getIndexOffset(middle);

            if (middleOffset === this.offset) {
                return middle;
            }
            else if (middleOffset < this.offset) {
                low = middle + 1;
            }
            else if (middleOffset > this.offset) {
                high = middle - 1;
            }
        }

        return low > 0 ? --low : 0;
    }

    /**
     * Calculates and returns the offset position of the given item.
     *
     * @param givenIndex The index of the item whose offset we need to calcualate.
     *
     * @returns The offset of the start of the item at this index.
     */
    private getIndexOffset(givenIndex: number): number {
        if (givenIndex === 0) {
            return 0;
        }

        let offset = 0;
        for (let index = 0; index < givenIndex; index++) {
            const indexSize = this.heights.get(this.options.uniqueIds[index]);
            offset = offset + (typeof indexSize === "number" ? indexSize : this.getEstimatedItemHeight());
        }

        // Remember last calculated index.
        this.highestCalculatedIndex = Math.max(this.highestCalculatedIndex, givenIndex - 1);
        this.highestCalculatedIndex = Math.min(this.highestCalculatedIndex, this.getLastIndex());

        return offset;
    }

    /**
     * Checks if the current calculation type is a known fixed height.
     *
     * @returns `true` if the calculation type is currently a fixed height.
     */
    private isFixedType(): boolean {
        return this.calcType === CalcType.Fixed;
    }

    /**
     * Gets the index number of the last item.
     *
     * @returns The index of the last item.
     */
    private getLastIndex(): number {
        return this.options.uniqueIds.length - 1;
    }

    /**
     * Checks if the range is valid and then updates the range if it has
     * changed from the current values.
     *
     * @param start The requested starting index.
     * @param end The requested ending index (inclusive).
     */
    private checkRange(start: number, end: number): void {
        const total = this.options.uniqueIds.length;

        // Total number of items is less than what we expect to fit on
        // screen plus buffering, just render everything.
        if (total <= (this.options.visibleCount + this.options.bufferCount + this.options.bufferCount)) {
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
        if (this.isFixedType()) {
            return (this.fixedHeightValue ?? 0) * this.range.startIndex;
        }
        else {
            return this.getIndexOffset(this.range.startIndex);
        }
    }

    /**
     * Gets the padding to be applied after the last visible item in the list.
     *
     * @returns The number of pixels to use as padding after the last item.
     */
    private getPadAfter(): number {
        const end = this.range.endIndex;
        const lastIndex = this.getLastIndex();

        if (this.isFixedType()) {
            return (lastIndex - end) * (this.fixedHeightValue ?? 0);
        }

        // If it's all calculated, return the exact offset.
        if (this.highestCalculatedIndex === lastIndex) {
            return this.getIndexOffset(lastIndex) - this.getIndexOffset(end);
        }
        else {
            // If not, estimate the remaining space.
            return (lastIndex - end) * this.getEstimatedItemHeight();
        }
    }

    /**
     * Gets the estimated height of a single item.
     *
     * @returns The estimated height of a single item.
     */
    private getEstimatedItemHeight(): number {
        return this.isFixedType()
            ? this.fixedHeightValue ?? 0
            : (this.firstRangeAverageHeight || this.options.estimatedHeight);
    }

    // #endregion
}
