type ItemWithKey<TItem, TKey> = {
    item: TItem;
    key: TKey;
};

type Collection<T> = {
    add(item: T): void;
    remove(): T | undefined;
    getLength(): number;
};

type FlattenOptions = {
    /** Determines whether the root items will be excluded in the flattened result (default `false`). */
    excludeRootItems: boolean;
};

const defaultFlattenOptions: FlattenOptions = {
    excludeRootItems: false
};

/**
 * Returns a new collection that implements a FIFO strategy for adding/removing items.
 */
function getFifoCollection<T>(): Collection<T> {
    const items: T[] = [];

    return {
        add(item: T): void {
            // Add item to the end of the array.
            items.push(item);
        },
        remove(): T | undefined {
            // Remove item from the front of the array.
            return items.shift();
        },
        getLength(): number {
            return items.length;
        }
    };
}

/**
 * Returns a new collection that implements a LIFO strategy for adding/removing items.
 */
function getLifoCollection<T>(): Collection<T> {
    const items: T[] = [];

    return {
        add(item: T): void {
            // Add item to the end of the array.
            items.push(item);
        },
        remove(): T | undefined {
            // Remove item from the end of the array.
            return items.pop();
        },
        getLength(): number {
            return items.length;
        }
    };
}

/**
 * Flattens a hierarchical data structure into a one-dimensional array
 * where the results are sorted in depth-first order.
 *
 * @param rootItems The root item(s) of the hierarchical data structure.
 * @param keySelector The key selector. The key is used to prevent duplicates in the flattened result.
 * @param childItemsSelector The selector that returns the child items of a given item.
 * @param options Flatten options.
 */
export function flattenDepthFirst<TItem, TKey>(rootItems: TItem | TItem[], keySelector: (item: TItem) => TKey, childItemsSelector: (item: TItem) => TItem[] | null | undefined, options?: FlattenOptions): TItem[] {
    return flatten<TItem, TKey>(getLifoCollection, Array.isArray(rootItems) ? rootItems : [rootItems], keySelector, childItemsSelector, options);
}

/**
 * Flattens a hierarchical data structure into a one-dimensional array
 * where the results are sorted in breadth-first order.
 *
 * @param rootItems The root item(s) of the hierarchical data structure.
 * @param keySelector The key selector. The key is used to prevent duplicates in the flattened result.
 * @param childItemsSelector The selector that returns the child items of a given item.
 * @param options Flatten options.
 */
export function flattenBreadthFirst<TItem, TKey>(rootItems: TItem | TItem[], keySelector: (item: TItem) => TKey, childItemsSelector: (item: TItem) => TItem[] | null | undefined, options?: FlattenOptions): TItem[] {
    return flatten<TItem, TKey>(getFifoCollection, Array.isArray(rootItems) ? rootItems : [rootItems], keySelector, childItemsSelector, options);
}

/**
 * Flattens a hierarchical data structure into a one-dimensional array
 * where the results are sorted in breadth-first order.
 *
 * @param useCollection The callback used to get the temporary collection used for flattening. Used to provide depth-first and breadth-first traversal.
 * @param rootItems The root items of the hierarchical data structure.
 * @param keySelector The key selector. The key is used to prevent duplicates in the flattened result.
 * @param childItemsSelector The selector that returns the child items of a given item.
 * @param options Flatten options.
 */
function flatten<TItem, TKey>(useCollection: () => Collection<ItemWithKey<TItem, TKey>>, rootItems: TItem[], keySelector: (item: TItem) => TKey, childItemsSelector: (item: TItem) => TItem[] | null | undefined, options: FlattenOptions = defaultFlattenOptions): TItem[] {
    // Create a map to prevent duplicates.
    const itemMap: Map<TKey, TItem> = new Map<TKey, TItem>();

    // Using a FIFO collection to avoid recursion to produce
    // a flattened array with breadth-first ordered results.
    const collection = useCollection();

    // Start with the direct descendant items of the root item(s).
    for (const rootItem of rootItems) {
        const childItems = childItemsSelector(rootItem);

        if (childItems) {
            for (const childItem of childItems) {
                const key = keySelector(childItem);

                // Only consider the child item if it has not been added.
                if (!itemMap.has(key)) {
                    collection.add({
                        item: childItem,
                        key
                    });
                }
            }
        }
    }

    // Process the remaining descendant items.
    while (collection.getLength()) {
        const itemAndKey = collection.remove();

        if (!itemAndKey?.item) {
            continue;
        }

        // Add the current item being evaluated.
        itemMap.set(itemAndKey.key, itemAndKey.item);

        // Add child items to evaluate.
        const childItems = childItemsSelector(itemAndKey.item);

        if (childItems?.length) {
            for (const childItem of childItems) {
                const key = keySelector(childItem);

                // Only consider the child item if it has not been added.
                if (!itemMap.has(key)) {
                    collection.add({
                        item: childItem,
                        key
                    });
                }
            }
        }
    }

    // Return the distinct flattened items.
    if (!options.excludeRootItems) {
        return [...rootItems, ...itemMap.values()];
    }
    else {
        return [...itemMap.values()];
    }
}