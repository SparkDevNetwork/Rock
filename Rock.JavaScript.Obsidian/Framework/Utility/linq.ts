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

/**
 * A function that will select a value from the object.
 */
type ValueSelector<T> = (value: T) => string | number | boolean | null | undefined;

/**
 * A function that will perform testing on a value to see if it meets
 * a certain condition and return true or false.
 */
type PredicateFn<T> = (value: T, index: number) => boolean;

/**
 * A function that will compare two values to see which one should
 * be ordered first.
 */
type ValueComparer<T> = (a: T, b: T) => number;

const moreThanOneElement = "More than one element was found in collection.";

const noElementsFound = "No element was found in collection.";

/**
 * Compares the values of two objects given the selector function.
 *
 * For the purposes of a compare, null and undefined are always a lower
 * value - unless both values are null or undefined in which case they
 * are considered equal.
 *
 * @param keySelector The function that will select the value.
 * @param descending True if this comparison should be in descending order.
 */
function valueComparer<T>(keySelector: ValueSelector<T>, descending: boolean): ValueComparer<T> {
    return (a: T, b: T): number => {
        const valueA = keySelector(a);
        const valueB = keySelector(b);

        // If valueA is null or undefined then it will either be considered
        // lower than or equal to valueB.
        if (valueA === undefined || valueA === null) {
            // If valueB is also null or undefined then they are considered equal.
            if (valueB === undefined || valueB === null) {
                return 0;
            }

            return !descending ? -1 : 1;
        }

        // If valueB is undefined or null (but valueA is not) then it is considered
        // a lower value than valueA.
        if (valueB === undefined || valueB === null) {
            return !descending ? 1 : -1;
        }

        // Perform a normal comparison.
        if (valueA > valueB) {
            return !descending ? 1 : -1;
        }
        else if (valueA < valueB) {
            return !descending ? -1 : 1;
        }
        else {
            return 0;
        }
    };
}


/**
 * Provides LINQ style access to an array of elements.
 */
export class List<T> {
    /** The elements being tracked by this list. */
    protected elements: T[];

    // #region Constructors

    /**
     * Creates a new list with the given elements.
     *
     * @param elements The elements to be made available to LINQ queries.
     */
    constructor(elements?: T[]) {
        if (elements === undefined) {
            this.elements = [];
        }
        else {
            // Copy the array so if the caller makes changes it won't be reflected by us.
            this.elements = [...elements];
        }
    }

    /**
     * Creates a new List from the elements without copying to a new array.
     *
     * @param elements The elements to initialize the list with.
     * @returns A new list of elements.
     */
    public static fromArrayNoCopy<T>(elements: T[]): List<T> {
        const list = new List<T>();

        list.elements = elements;

        return list;
    }

    // #endregion

    /**
     * Returns a boolean that determines if the collection contains any elements.
     *
     * @returns true if the collection contains any elements; otherwise false.
     */
    public any(): boolean;

    /**
     * Filters the list by the predicate and then returns a boolean that determines
     * if the filtered collection contains any elements.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns true if the collection contains any elements; otherwise false.
     */
    public any(predicate: PredicateFn<T>): boolean;

    /**
     * Filters the list by the predicate and then returns a boolean that determines
     * if the filtered collection contains any elements.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns true if the collection contains any elements; otherwise false.
     */
    public any(predicate?: PredicateFn<T>): boolean {
        let elements = this.elements;

        if (predicate !== undefined) {
            elements = elements.filter(predicate);
        }

        return elements.length > 0;
    }

    /**
     * Returns the first element from the collection if there are any elements.
     * Otherwise will throw an exception.
     *
     * @returns The first element in the collection.
     */
    public first(): T;

    /**
     * Filters the list by the predicate and then returns the first element
     * in the collection if any remain. Otherwise throws an exception.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns The first element in the collection.
     */
    public first(predicate: PredicateFn<T>): T;

    /**
     * Filters the list by the predicate and then returns the first element
     * in the collection if any remain. Otherwise throws an exception.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns The first element in the collection.
     */
    public first(predicate?: PredicateFn<T>): T {
        let elements = this.elements;

        if (predicate !== undefined) {
            elements = elements.filter(predicate);
        }

        if (elements.length >= 1) {
            return elements[0];
        }
        else {
            throw noElementsFound;
        }
    }

    /**
     * Returns the first element found in the collection or undefined if the
     * collection contains no elements.
     *
     * @returns The first element in the collection or undefined.
     */
    public firstOrUndefined(): T | undefined;

    /**
     * Filters the list by the predicate and then returns the first element
     * found in the collection. If no elements remain then undefined is
     * returned instead.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns The first element in the filtered collection or undefined.
     */
    public firstOrUndefined(predicate: PredicateFn<T>): T | undefined;

    /**
     * Filters the list by the predicate and then returns the first element
     * found in the collection. If no elements remain then undefined is
     * returned instead.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns The first element in the filtered collection or undefined.
     */
    public firstOrUndefined(predicate?: PredicateFn<T>): T | undefined {
        let elements = this.elements;

        if (predicate !== undefined) {
            elements = elements.filter(predicate);
        }

        if (elements.length === 1) {
            return elements[0];
        }
        else {
            return undefined;
        }
    }

    /**
     * Returns the last element found in the collection or undefined if the
     * collection contains no elements.
     *
     * @returns The last element in the collection or undefined.
     */
    public lastOrUndefined(): T | undefined;

    /**
     * Filters the list by the predicate and then returns the last element
     * found in the collection. If no elements remain then undefined is
     * returned instead.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns The last element in the filtered collection or undefined.
     */
    public lastOrUndefined(predicate: PredicateFn<T>): T | undefined;

    /**
     * Filters the list by the predicate and then returns the last element
     * found in the collection. If no elements remain then undefined is
     * returned instead.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns The last element in the filtered collection or undefined.
     */
    public lastOrUndefined(predicate?: PredicateFn<T>): T | undefined {
        let elements = this.elements;

        if (predicate !== undefined) {
            elements = elements.filter(predicate);
        }

        if (elements.length) {
            return elements[elements.length - 1];
        }
        else {
            return undefined;
        }
    }

    /**
     * Returns a single element from the collection if there is a single
     * element. Otherwise will throw an exception.
     *
     * @returns An element.
     */
    public single(): T;

    /**
     * Filters the list by the predicate and then returns the single remaining
     * element from the collection. If more than one element remains then an
     * exception will be thrown.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns An element.
     */
    public single(predicate: PredicateFn<T>): T;

    /**
     * Filters the list by the predicate and then returns the single remaining
     * element from the collection. If more than one element remains then an
     * exception will be thrown.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns An element.
     */
    public single(predicate?: PredicateFn<T>): T {
        let elements = this.elements;

        if (predicate !== undefined) {
            elements = elements.filter(predicate);
        }

        if (elements.length === 1) {
            return elements[0];
        }
        else {
            throw moreThanOneElement;
        }
    }

    /**
     * Returns a single element from the collection if there is a single
     * element. If no elements are found then undefined is returned. More
     * than a single element will throw an exception.
     *
     * @returns An element or undefined.
     */
    public singleOrUndefined(): T | undefined;

    /**
     * Filters the list by the predicate and then returns the single element
     * from the collection if there is only one remaining. If no elements
     * remain then undefined is returned. More than a single element will throw
     * an exception.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns An element or undefined.
     */
    public singleOrUndefined(predicate: PredicateFn<T>): T | undefined;

    /**
     * Filters the list by the predicate and then returns the single element
     * from the collection if there is only one remaining. If no elements
     * remain then undefined is returned. More than a single element will throw
     * an exception.
     *
     * @param predicate The predicate to filter the elements by.
     *
     * @returns An element or undefined.
     */
    public singleOrUndefined(predicate?: PredicateFn<T>): T | undefined {
        let elements = this.elements;

        if (predicate !== undefined) {
            elements = elements.filter(predicate);
        }

        if (elements.length === 0) {
            return undefined;
        }
        else if (elements.length === 1) {
            return elements[0];
        }
        else {
            throw moreThanOneElement;
        }
    }

    /**
     * Orders the elements of the array and returns a new list of items
     * in that order.
     *
     * @param keySelector The selector for the key to be ordered by.
     * @returns A new ordered list of elements.
     */
    public orderBy(keySelector: ValueSelector<T>): OrderedList<T> {
        const comparer = valueComparer(keySelector, false);

        return new OrderedList(this.elements, comparer);
    }

    /**
     * Orders the elements of the array in descending order and returns a
     * new list of items in that order.
     *
     * @param keySelector The selector for the key to be ordered by.
     * @returns A new ordered list of elements.
     */
    public orderByDescending(keySelector: ValueSelector<T>): OrderedList<T> {
        const comparer = valueComparer(keySelector, true);

        return new OrderedList(this.elements, comparer);
    }

    /**
     * Filters the results and returns a new list containing only the elements
     * that match the predicate.
     *
     * @param predicate The predicate to filter elements with.
     *
     * @returns A new collection of elements that match the predicate.
     */
    public where(predicate: PredicateFn<T>): List<T> {
        return new List<T>(this.elements.filter(predicate));
    }

    /**
     * Get the elements of this list as a native array of items.
     *
     * @returns An array of items with all filters applied.
     */
    public toArray(): T[] {
        return [...this.elements];
    }
}

/**
 * A list of items that has ordering already applied.
 */
class OrderedList<T> extends List<T> {
    /** The base comparer to use when ordering. */
    private baseComparer!: ValueComparer<T>;

    // #region Constructors

    constructor(elements: T[], baseComparer: ValueComparer<T>) {
        super(elements);

        this.baseComparer = baseComparer;
        this.elements.sort(this.baseComparer);
    }

    // #endregion

    /**
     * Orders the elements of the array and returns a new list of items
     * in that order.
     *
     * @param keySelector The selector for the key to be ordered by.
     * @returns A new ordered list of elements.
     */
    public thenBy(keySelector: ValueSelector<T>): OrderedList<T> {
        const comparer = valueComparer(keySelector, false);

        return new OrderedList(this.elements, (a: T, b: T) => this.baseComparer(a, b) || comparer(a, b));
    }

    /**
     * Orders the elements of the array in descending order and returns a
     * new list of items in that order.
     *
     * @param keySelector The selector for the key to be ordered by.
     * @returns A new ordered list of elements.
     */
    public thenByDescending(keySelector: ValueSelector<T>): OrderedList<T> {
        const comparer = valueComparer(keySelector, true);

        return new OrderedList(this.elements, (a: T, b: T) => this.baseComparer(a, b) || comparer(a, b));
    }
}

/**
 * A utility class for working with iterables in a LINQ-like manner.
 */
export class Enumerable<T> {
    protected iterableFactory: () => Iterable<T>;

    /**
     * Creates an instance of Enumerable using a factory for the iterable.
     * @param iterableFactory - A factory function that produces an iterable.
     */
    constructor(iterableFactory: () => Iterable<T>) {
        this.iterableFactory = iterableFactory;
    }

    /**
     * Creates an Enumerable from a regular iterable (e.g., Array, Set).
     * @param iterable - An iterable to create the Enumerable from.
     * @returns A new Enumerable instance.
     */
    static from<T>(iterable: Iterable<T>): Enumerable<T>;

    /**
     * Creates an Enumerable from a generator function.
     * @param generator - A function that produces an IterableIterator.
     * @returns A new Enumerable instance.
     */
    static from<T>(generator: () => IterableIterator<T>): Enumerable<T>;

    /**
     * Creates an Enumerable from a regular iterable (e.g., Array, Set) or a generator function.
     * @param source - Either an iterable or a generator function.
     * @returns A new Enumerable instance.
     */
    static from<T>(source: Iterable<T> | (() => IterableIterator<T>)): Enumerable<T> {
        if (typeof source === "function") {
            return new Enumerable(source); // Handle generator factory
        }
        else {
            return new Enumerable(() => source); // Handle regular iterable
        }
    }

    /**
     * Returns an iterator for the current Enumerable.
     * @returns An iterator for the iterable.
     */
    *[Symbol.iterator](): Iterator<T> {
        // Regenerate the iterable.
        yield* this.iterableFactory();
    }

    /**
     * Filters the sequence to include only elements that satisfy the predicate.
     * @param predicate - A function to test each element for a condition.
     * @returns A new Enumerable containing the filtered elements.
     */
    where(predicate: (item: T) => boolean): Enumerable<T> {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        return new Enumerable(function* (): Generator<T, void, unknown> {
            for (const item of self) {
                if (predicate(item)) {
                    yield item;
                }
            }
        });
    }

    /**
     * Projects each element of the sequence into a new form.
     * @param selector - A function to project each element into a new form.
     * @returns A new Enumerable with the projected elements.
     */
    select<U>(selector: (item: T) => U): Enumerable<U> {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        return new Enumerable(function* (): Generator<U, void, unknown> {
            for (const item of self) {
                yield selector(item);
            }
        });
    }

    /**
     * Returns a new Enumerable that skips the first `count` elements of the sequence.
     * @param count - The number of elements to skip.
     * @returns A new Enumerable that skips the specified number of elements.
     */
    skip(count: number): Enumerable<T> {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        return new Enumerable(function* () {
            let skipped = 0;
            for (const item of self) {
                if (skipped++ >= count) {
                    yield item;
                }
            }
        });
    }

    /**
     * Returns a new Enumerable that contains the first `count` elements of the sequence.
     * @param count - The number of elements to take.
     * @returns A new Enumerable containing the taken elements.
     */
    take(count: number): Enumerable<T> {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        return new Enumerable(function* () {
            let i = 0;
            for (const item of self) {
                if (i++ < count) {
                    yield item;
                }
                else {
                    break;
                }
            }
        });
    }

    /**
     * Returns the first element of the sequence or a default value if the sequence is empty.
     * @param defaultValue - The default value to return if the sequence is empty.
     * @returns The first element of the sequence or the default value.
     */
    firstOrDefault(defaultValue?: T): T | undefined {
        for (const item of this) {
            return item;
        }

        return defaultValue;
    }

    /**
     * Returns the last element of the sequence, or a default value if the sequence is empty.
     * @param defaultValue - The default value to return if the sequence is empty.
     * @returns The last element of the sequence, or the provided default value.
     *
     * @example
     * const numbers = Enumerable.from([1, 2, 3]);
     * console.log(numbers.lastOrDefault()); // Outputs: 3
     *
     * @example
     * const empty = Enumerable.from<number>([]);
     * console.log(empty.lastOrDefault(0)); // Outputs: 0
     */
    lastOrDefault(defaultValue?: T): T | undefined {
        let last: T | undefined = defaultValue;

        for (const item of this) {
            // Update `last` for each element.
            last = item;
        }

        return last;
    }

    /**
     * Aggregates the elements of the sequence using a specified accumulator function and seed value.
     * @param accumulator - A function that accumulates each element.
     * @param seed - The initial value for the accumulation.
     * @returns The aggregated value.
     */
    aggregate<U>(accumulator: (acc: U, item: T, index: number) => U, seed: U): U {
        let result = seed;
        let index = 0;
        for (const item of this) {
            result = accumulator(result, item, index++);
        }
        return result;
    }

    /**
     * Determines whether any elements in the sequence satisfy a condition or if the sequence contains any elements.
     * @param predicate - An optional function to test each element for a condition.
     * @returns `true` if any elements satisfy the condition; otherwise, `false`.
     */
    any(predicate?: (item: T) => boolean): boolean {
        for (const item of this) {
            if (!predicate || predicate(item)) {
                return true;
            }
        }
        return false;
    }

    /**
     * Determines whether all elements in the sequence satisfy a condition or if the sequence contains any elements.
     * @param predicate - An optional function to test each element for a condition.
     * @returns `true` if all elements satisfy the condition; otherwise, `false`.
     */
    all(predicate: (item: T) => boolean): boolean {
        for (const item of this) {
            if (!predicate(item)) {
                return false;
            }
        }

        return true;
    }

    /**
     * Executes a specified action for each element in the sequence.
     * @param action - A function to execute for each element.
     */
    forEach(action: (item: T) => void): void {
        for (const item of this) {
            action(item);
        }
    }

    /**
     * Converts the sequence into an array.
     * @returns An array containing all elements in the sequence.
     */
    toArray(): T[] {
        return Array.from(this);
    }

    /**
     * Converts the sequence into a List.
     * @returns An List containing all elements in the sequence.
     */
    toList(): List<T> {
        return new List<T>(this.toArray());
    }

    /**
     * Sorts the elements of the sequence in ascending order.
     * @param keySelector - Function to extract the key for comparison.
     * @param comparer - Optional comparison function for the keys.
     * @returns An OrderedEnumerable sorted in ascending order.
     */
    orderBy<U>(
        keySelector: (item: T) => U,
        comparer: (a: U, b: U) => number = (a, b) => (a > b ? 1 : a < b ? -1 : 0)
    ): OrderedEnumerable<T> {
        return new OrderedEnumerable(
            this.iterableFactory,
            (a, b) => comparer(keySelector(a), keySelector(b))
        );
    }

    /**
     * Sorts the elements of the sequence in descending order.
     * @param keySelector - Function to extract the key for comparison.
     * @param comparer - Optional comparison function for the keys.
     * @returns An OrderedEnumerable sorted in descending order.
     */
    orderByDescending<U>(
        keySelector: (item: T) => U,
        comparer: (a: U, b: U) => number = (a, b) => (a > b ? 1 : a < b ? -1 : 0)
    ): OrderedEnumerable<T> {
        const descendingComparer = (a: U, b: U): number => -comparer(a, b);
        return this.orderBy(keySelector, descendingComparer);
    }

    /**
     * Returns a generator that yields each element of the sequence paired with its index.
     *
     * @generator
     * @yields {[T, number]} A tuple containing the element and its zero-based index.
     *
     * @example
     * // Example usage with a for...of loop:
     * const elements = Enumerable.from(['a', 'b', 'c']);
     * for (const [item, index] of elements.withIndex()) {
     *     console.log(`Index: ${index}, Item: ${item}`);
     * }
     * // Output:
     * // Index: 0, Item: a
     * // Index: 1, Item: b
     * // Index: 2, Item: c
     *
     * @example
     * // Example usage with chaining:
     * const indexed = Enumerable.from(['x', 'y', 'z'])
     *     .withIndex()
     *     .where(([item, index]) => index % 2 === 0)
     *     .toArray();
     * console.log(indexed);
     * // Output: [['x', 0], ['z', 2]]
     */
    *withIndex(): IterableIterator<[T, number]> {
        let index = 0;
        for (const item of this) {
            yield [item, index++];
        }
    }

    /**
 * Filters the sequence and returns only elements of the specified type.
 * @template U The target type to filter by.
 * @param typeCheck - A runtime check function to validate the type of each element.
 * @returns A new Enumerable containing elements of type `U`.
 *
 * @example
 * const mixed: Enumerable<unknown> = Enumerable.from([1, "hello", true, 42]);
 * const numbers = mixed.ofType<number>(item => typeof item === "number");
 * console.log(numbers.toArray()); // Outputs: [1, 42]
 *
 * @example
 * class Animal {}
 * class Dog extends Animal {}
 * const animals: Enumerable<Animal> = Enumerable.from([new Animal(), new Dog()]);
 * const dogs = animals.ofType<Dog>(item => item instanceof Dog);
 * console.log(dogs.toArray()); // Outputs: [Dog instance]
 */
    ofType<U extends T>(typeCheck: (item: T) => item is U): Enumerable<U> {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        return new Enumerable(function* () {
            for (const item of self) {
                if (typeCheck(item)) {
                    yield item;
                }
            }
        });
    }
}

class OrderedEnumerable<T> extends Enumerable<T> {
    private readonly sortComparers: ((a: T, b: T) => number)[];

    constructor(
        iterableFactory: () => Iterable<T>,
        initialComparer: (a: T, b: T) => number
    ) {
        super(iterableFactory);
        this.sortComparers = [initialComparer];
    }

    /**
     * Adds a secondary ascending order comparison to the current sort.
     * @param keySelector - Function to extract the key for comparison.
     * @param comparer - Optional comparison function for the keys.
     * @returns A new OrderedEnumerable with the additional ordering.
     */
    thenOrderBy<U>(
        keySelector: (item: T) => U,
        comparer: (a: U, b: U) => number = (a, b) => (a > b ? 1 : a < b ? -1 : 0)
    ): OrderedEnumerable<T> {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        return new OrderedEnumerable(
            this.iterableFactory,
            (a, b) => {
                for (const cmp of self.sortComparers) {
                    const result = cmp(a, b);
                    if (result !== 0) return result;
                }
                return comparer(keySelector(a), keySelector(b));
            }
        );
    }

    /**
     * Adds a secondary descending order comparison to the current sort.
     * @param keySelector - Function to extract the key for comparison.
     * @param comparer - Optional comparison function for the keys.
     * @returns A new OrderedEnumerable with the additional ordering.
     */
    thenOrderByDescending<U>(
        keySelector: (item: T) => U,
        comparer: (a: U, b: U) => number = (a, b) => (a > b ? 1 : a < b ? -1 : 0)
    ): OrderedEnumerable<T> {
        const descendingComparer = (a: U, b: U): number => -comparer(a, b);
        return this.thenOrderBy(keySelector, descendingComparer);
    }

    override toArray(): T[] {
        return Array.from(this);
    }

    /**
     * Sorts the sequence based on the defined comparers.
     * @returns A new Iterable with elements sorted.
     */
    override *[Symbol.iterator](): Iterator<T> {
        const array = Array.from(this.iterableFactory());
        array.sort((a, b) => {
            for (const comparer of this.sortComparers) {
                const result = comparer(a, b);
                if (result !== 0) return result;
            }
            return 0;
        });
        yield* array;
    }
}