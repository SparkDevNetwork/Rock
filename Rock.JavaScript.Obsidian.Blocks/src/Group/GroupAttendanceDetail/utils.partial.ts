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

import { AttendanceSortByDelegate, ModalOptionsBag, Switch, SwitchPosition } from "./types.partial";
import { GroupAttendanceDetailAttendanceBag } from "@Obsidian/ViewModels/Blocks/Group/GroupAttendanceDetail/groupAttendanceDetailAttendanceBag";
import { isPromise } from "@Obsidian/Utility/promiseUtils";

//#region Filter Utilities

/**
 * Binds `this` to the `attendanceFilter` instance for all its methods.
 *
 * This allows each method to be executed as a function instead of an instance method while still retaining the original execution context (`this` reference). (see example)
 *
 * @example
 * // Assign the filter method to a variable.
 * const f = attendanceFilter.filter;
 *
 * // If the original `attendanceFilter.filter()` method definition contained a `this` keyword,
 * // then `this` will still reference the original `attendanceFilter` instance when invoked with:
 * f(something);
 */
function bindThis(attendanceFilter: IAttendanceFilter): void {
    attendanceFilter.filter = attendanceFilter.filter.bind(attendanceFilter);
    attendanceFilter.hasFilter = attendanceFilter.hasFilter.bind(attendanceFilter);
    attendanceFilter.isFilter = attendanceFilter.isFilter.bind(attendanceFilter);
}

/**
 * Represents an attendance filter.
 */
export interface IAttendanceFilter {
    /**
     * Filters an `attendance` record.
     *
     * @returns `true` if the `attendance` matches the filter; otherwise, `false`.
     */
    filter(attendance: GroupAttendanceDetailAttendanceBag): boolean;
    /**
     * Returns `true` if this filter is the same instance as `attendanceFilter`; otherwise, `false`.
     *
     * Useful if you want to check if this is a specific type of filter.
     */
    isFilter(attendanceFilter: IAttendanceFilter): boolean;
    /**
     * Returns `true` if this filter is the same instance as `attendanceFilter` OR if `attendanceFilter` is one of its own aggregate filters; otherwise, `false`.
     *
     * Useful if you want to check if this filter is or has a specific type of filter.
     */
    hasFilter(attendanceFilter: IAttendanceFilter): boolean;
}

/**
 * Represents an attendance filter based on multiple filters.
 */
export interface IAggregateAttendanceFilter extends IAttendanceFilter {
    /**
     * The aggregate child filters.
     */
    filters: IAttendanceFilter[];
}

/**
 * Creates a new `IAttendanceFilter` object using the specified filter function.
 */
export function createFilter(filter: (attendance: GroupAttendanceDetailAttendanceBag) => boolean): IAttendanceFilter {
    const attendanceFilter: IAttendanceFilter = {
        filter,
        hasFilter(filter: IAttendanceFilter): boolean {
            return hasSameFilter(this, filter);
        },
        isFilter(filter: IAttendanceFilter): boolean {
            return isSameFilter(this, filter);
        }
    };

    // Bind the `this` keyword to the `attendanceFilter` instance.
    bindThis(attendanceFilter);

    return attendanceFilter;
}

/**
 * Creates a new `IAggregateAttendanceFilter` object using the specified filters and filter function.
 */
export function createAggregateFilter(filters: IAttendanceFilter[], filter: (filters: IAttendanceFilter[], attendance: GroupAttendanceDetailAttendanceBag) => boolean): IAggregateAttendanceFilter {
    const aggregateAttendanceFilter: IAggregateAttendanceFilter = {
        hasFilter(filter: IAttendanceFilter): boolean {
            return hasSameFilter(this, filter);
        },
        isFilter(filter: IAttendanceFilter): boolean {
            return isSameFilter(this, filter);
        },
        filters: filters,
        filter(attendance: GroupAttendanceDetailAttendanceBag): boolean {
            return filter(this.filters, attendance);
        }
    };

    // Bind the `this` keyword to the `attendanceFilter` instance.
    bindThis(aggregateAttendanceFilter);

    return aggregateAttendanceFilter;
}

/**
 * A filter that returns `true` for any attendance record.
 */
export const NoFilter = createFilter(_ => true);

/**
 * A filter that returns `true` if `attendance.didAttend == true`.
 */
export const DidAttendFilter = createFilter(attendance => attendance.didAttend);

// Cache "last name starts with" filters.
const lastNameStartsWithFilters: Record<string, IAttendanceFilter> = {};

/**
 * Creates a new (or returns an existing) `IAttendanceFilter` object whose `filter` method that returns `true` when the `attendance.lastName` starts with the specified initial.
 */
export function getOrCreateLastNameStartsWithFilter(lastNameInitial: string): IAttendanceFilter {
    let lastNameStartsWithFilter = lastNameStartsWithFilters[lastNameInitial];

    if (lastNameStartsWithFilter) {
        return lastNameStartsWithFilter;
    }

    lastNameStartsWithFilter = createFilter(attendance => attendance.lastName?.startsWith(lastNameInitial) === true);
    lastNameStartsWithFilters[lastNameInitial] = lastNameStartsWithFilter;

    return lastNameStartsWithFilter;
}

// Cache "first name starts with" filters.
const firstNameStartsWithFilters: Record<string, IAttendanceFilter> = {};

/**
 * Creates a new (or returns an existing) `IAttendanceFilter` object whose `filter` method that returns `true` when the `attendance.nickName` starts with the specified initial.
 */
export function getOrCreateFirstNameStartsWithFilter(firstNameInitial: string): IAttendanceFilter {
    let firstNameStartsWithFilter = firstNameStartsWithFilters[firstNameInitial];

    if (firstNameStartsWithFilter) {
        return firstNameStartsWithFilter;
    }

    firstNameStartsWithFilter = createFilter(attendance => attendance.nickName?.startsWith(firstNameInitial) === true);
    firstNameStartsWithFilters[firstNameInitial] = firstNameStartsWithFilter;

    return firstNameStartsWithFilter;
}

/**
 * Creates a filter that will return `true` if any of the specified `attendanceFilters` returns `true`; otherwise, `false`.
 *
 * The aggregate filters can be modified via the `filters` property of the returned object.
 */
export function createSomeFilter(...attendanceFilters: IAttendanceFilter[]): IAggregateAttendanceFilter {
    return createAggregateFilter(attendanceFilters, (filters, attendance) => filters.some(filter => filter.filter(attendance)));
}

/**
 * Creates a filter that will return `true` if all of the specified `attendanceFilters` returns `true`; otherwise, `false`.
 *
 * The aggregate filters can be modified via the `filters` property of the returned object.
 */
export function createEveryFilter(...attendanceFilters: IAttendanceFilter[]): IAggregateAttendanceFilter {
    return createAggregateFilter(attendanceFilters, (filters, attendance) => filters.every(filter => filter.filter(attendance)));
}

/**
 * Returns `true` if `attendanceFilter1` is the same instance as `attendanceFilter2`; otherwise, `false`.
 */
function isSameFilter(attendanceFilter1: IAttendanceFilter, attendanceFilter2: IAttendanceFilter): boolean {
    return attendanceFilter1?.filter === attendanceFilter2?.filter;
}

/**
 * Returns `true` if `attendanceFilter1` is the same instance as `attendanceFilter2` OR if `attendanceFilter2` is one of `attendanceFilter1`'s own aggregate filters; otherwise, `false`
 */
function hasSameFilter(attendanceFilter1: IAttendanceFilter, attendanceFilter2: IAttendanceFilter): boolean {
    if (attendanceFilter1?.filter === attendanceFilter2?.filter) {
        return true;
    }

    if (isAggregateAttendanceFilter(attendanceFilter1)) {
        return attendanceFilter1.filters.some(f => isSameFilter(f, attendanceFilter2));
    }

    return false;
}

/**
 * Determines if an `IAttendanceFilter` instance is an instance of `IAggregateAttendanceFilter`.
 */
function isAggregateAttendanceFilter(attendanceFilter: IAttendanceFilter): attendanceFilter is IAggregateAttendanceFilter {
    return !!(attendanceFilter as IAggregateAttendanceFilter)?.filters;
}

//#endregion Filter Utilities

//#region Sort Utilities

/**
 * Creates a "sort by" delegate that sorts attendance records by `firstBy` then sorts by the `thenBys`, if defined.
 */
export function createSortBy(firstBy: AttendanceSortByDelegate, ...thenBys: AttendanceSortByDelegate[]): AttendanceSortByDelegate {
    return (attendance1: GroupAttendanceDetailAttendanceBag, attendance2: GroupAttendanceDetailAttendanceBag): number => {
        let comparison = firstBy(attendance1, attendance2);

        // If attendance1 and attendance2 match, then run the additional `thenBy` comparisons.
        let thenByIndex = 0;
        let thenBy = thenBys ? thenBys[thenByIndex++] : null;
        while (comparison === 0 && thenBy) {
            comparison = thenBy(attendance1, attendance2);
            thenBy = thenBys[thenByIndex++];
        }

        return comparison;
    };
}

/**
 * A "sort by" delegate that sorts attendance records by `attendance.nickName`.
 */
export const sortByFirstName: AttendanceSortByDelegate = (attendance1: GroupAttendanceDetailAttendanceBag, attendance2: GroupAttendanceDetailAttendanceBag): number => {
    return compareStrings(attendance1.nickName, attendance2.nickName);
};

/**
 * A "sort by" delegate that sorts attendance records by `attendance.lastName`.
 */
export const sortByLastName: AttendanceSortByDelegate = (attendance1: GroupAttendanceDetailAttendanceBag, attendance2: GroupAttendanceDetailAttendanceBag): number => {
    return compareStrings(attendance1.lastName, attendance2.lastName);
};

/**
 * Returns the comparison value between two strings.
 *
 * -1: str1 < str2
 *  0: str1 == str2
 *  1: str1 > str2
 */
function compareStrings(str1: string | null | undefined, str2: string | null | undefined): number {
    return (str1 ?? "").localeCompare(str2 ?? "");
}

//#endregion Sort Utilities

//#region Switch Utilities

/**
 * Creates a switch that can be turned on and off.
 */
export function createSwitch(): Switch {
    let position: SwitchPosition;
    let isDisabled: boolean;

    return {
        get isOn(): boolean {
            return !this.isDisabled && this.position === "on";
        },
        get isDisabled(): boolean {
            return isDisabled;
        },
        get position(): SwitchPosition {
            return position;
        },
        enable(): void {
            isDisabled = false;
        },
        disable(): void {
            isDisabled = true;
        },
        on(): void {
            if (position === "on") {
                return;
            }

            position = "on";
        },
        off(): void {
            if (position === "off") {
                return;
            }

            position = "off";
        },
        connectToFunc<Request>(func: (request: Request) => Promise<void>): typeof func {
            return async (r: Request): Promise<void> => {
                if (!this.isOn) {
                    return;
                }

                return await func(r);
            };
        }
    };
}

//#endregion Switch Utilities

//#region Modal Utilities


/**
 * Creates options that can be bound to a Modal component.
 */
export function createModalOptions<T extends Partial<ModalOptionsBag> & { onCancel?(): void | PromiseLike<void> }>(options: T): ModalOptionsBag {
    let isOpen: boolean = true;
    let isCanceled: boolean = true;

    return {
        cancelText: options.cancelText,
        saveText: options.saveText,
        text: options.text,
        get isOpen(): boolean {
            return isOpen;
        },
        set isOpen(newIsOpen: boolean) {
            const wasOpen = isOpen;

            // Call cancel callback if closed without saving.
            if (isCanceled && wasOpen && !newIsOpen) {
                if (options.onCancel) {
                    const result = options.onCancel();
                    if (isPromise(result)) {
                        result.then(() => {
                            isOpen = newIsOpen;
                        });
                    }
                    else {
                        isOpen = newIsOpen;
                    }
                    return;
                }
            }

            isOpen = newIsOpen;
        },
        async onSave() {
            isCanceled = false;

            if (options.onSave) {
                const result = options.onSave();
                if (isPromise(result)) {
                    await result;
                }
            }

            this.isOpen = false;
        }
    };
}

/**
 * Gets the full name for an attendee.
 */
export function getAttendanceFullName(attendance: GroupAttendanceDetailAttendanceBag): string {
    return `${attendance.nickName} ${attendance.lastName}`;
}

//#endregion
