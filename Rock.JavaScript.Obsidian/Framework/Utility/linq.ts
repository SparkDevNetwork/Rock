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
        if (predicate !== undefined) {
            return this.elements.some(predicate);
        }
        else {
            return this.elements.length > 0;
        }
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
        if (predicate !== undefined) {
            return this.elements.find(predicate);
        }

        if (this.elements.length === 1) {
            return this.elements[0];
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
