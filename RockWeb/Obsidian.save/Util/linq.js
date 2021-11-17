System.register([], function (exports_1, context_1) {
    "use strict";
    var moreThanOneElement, noElementsFound, List, OrderedList;
    var __moduleName = context_1 && context_1.id;
    function valueComparer(keySelector, descending) {
        return (a, b) => {
            const valueA = keySelector(a);
            const valueB = keySelector(b);
            if (valueA === undefined || valueA === null) {
                if (valueB === undefined || valueB === null) {
                    return 0;
                }
                return !descending ? -1 : 1;
            }
            if (valueB === undefined || valueB === null) {
                return !descending ? 1 : -1;
            }
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
    return {
        setters: [],
        execute: function () {
            moreThanOneElement = "More than one element was found in collection.";
            noElementsFound = "No element was found in collection.";
            List = class List {
                constructor(elements) {
                    if (elements === undefined) {
                        this.elements = [];
                    }
                    else {
                        this.elements = [...elements];
                    }
                }
                static fromArrayNoCopy(elements) {
                    const list = new List();
                    list.elements = elements;
                    return list;
                }
                any(predicate) {
                    let elements = this.elements;
                    if (predicate !== undefined) {
                        elements = elements.filter(predicate);
                    }
                    return elements.length > 0;
                }
                first(predicate) {
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
                firstOrUndefined(predicate) {
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
                single(predicate) {
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
                singleOrUndefined(predicate) {
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
                orderBy(keySelector) {
                    const comparer = valueComparer(keySelector, false);
                    return new OrderedList(this.elements, comparer);
                }
                orderByDescending(keySelector) {
                    const comparer = valueComparer(keySelector, true);
                    return new OrderedList(this.elements, comparer);
                }
                where(predicate) {
                    return new List(this.elements.filter(predicate));
                }
                toArray() {
                    return [...this.elements];
                }
            };
            exports_1("List", List);
            OrderedList = class OrderedList extends List {
                constructor(elements, baseComparer) {
                    super(elements);
                    this.baseComparer = baseComparer;
                    this.elements.sort(this.baseComparer);
                }
                thenBy(keySelector) {
                    const comparer = valueComparer(keySelector, false);
                    return new OrderedList(this.elements, (a, b) => this.baseComparer(a, b) || comparer(a, b));
                }
                thenByDescending(keySelector) {
                    const comparer = valueComparer(keySelector, true);
                    return new OrderedList(this.elements, (a, b) => this.baseComparer(a, b) || comparer(a, b));
                }
            };
        }
    };
});
//# sourceMappingURL=linq.js.map