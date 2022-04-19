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

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toNumber, toNumberOrNull } from "./number";

// This file contains helper functions and tooling required to work with sliding
// date ranges. A sliding date range is one that, generally, is anchored to whatever
// the current date and time is when the check is made. For example, "within the next
// 5 days" would be the english equivalent of a sliding date range.

/**
 * The possible enumeration values for the sliding date range type.
 */
export const enum RangeType {
    /**
     * The last X hours,days,etc (inclusive of current hour,day,etc) but
     * cuts off so it doesn't include future dates.
     */
    Last = 0,

    /** The current hour,day,etc. */
    Current = 1,

    /** A specific date range between two dates, inclusive of both dates. */
    DateRange = 2,

    /** The previous X hours,days,etc (excludes current hour,day,etc). */
    Previous = 4,

    /**
     * The next X hours,days,etc (inclusive of current hour,day,etc), but
     * cuts off so it doesn't include past dates.
     */
    Next = 8,

    /** The upcoming X hours,days,etc (excludes current hour,day,etc). */
    Upcoming = 16
}

/**
 * The unit of time a corresponding numeric value represents.
 */
export const enum TimeUnit {
    /** The numeric value represents a number of hours. */
    Hour = 0,

    /** The numeric value represents a number of days. */
    Day = 1,

    /** The numeric value represents a number of weeks. */
    Week = 2,

    /** The numeric value represents a number of months. */
    Month = 3,

    /** The numeric value represents a number of years. */
    Year = 4
}

/**
 * Specifies the information required to track a sliding date range.
 */
export type SlidingDateRange = {
    /** The type of sliding date range represented by this instance. */
    rangeType: RangeType;

    /** The unit of time represented by the timeValue property. */
    timeUnit?: TimeUnit;

    /** The number of time units used when calculating the date range. */
    timeValue?: number;

    /** The lower value of a specific date range. */
    lowerDate?: string;

    /** The upper value of a specific date range. */
    upperDate?: string;
};

/**
 * The sliding date range types represented as an array of ListItemBag objects.
 * These are ordered correctly and can be used in pickers.
 */
export const rangeTypeOptions: ListItemBag[] = [
    {
        value: RangeType.Current.toString(),
        text: "Current"
    },
    {
        value: RangeType.Previous.toString(),
        text: "Previous"
    },
    {
        value: RangeType.Last.toString(),
        text: "Last"
    },
    {
        value: RangeType.Next.toString(),
        text: "Next"
    },
    {
        value: RangeType.Upcoming.toString(),
        text: "Upcoming"
    },
    {
        value: RangeType.DateRange.toString(),
        text: "Date Range"
    }
];

/**
 * The sliding date range time units represented as an array of ListItemBag objects.
 * These are ordered correctly and can be used in pickers.
 */
export const timeUnitOptions: ListItemBag[] = [
    {
        value: TimeUnit.Hour.toString(),
        text: "Hour"
    },
    {
        value: TimeUnit.Day.toString(),
        text: "Day"
    },
    {
        value: TimeUnit.Week.toString(),
        text: "Week"
    },
    {
        value: TimeUnit.Month.toString(),
        text: "Month"
    },
    {
        value: TimeUnit.Year.toString(),
        text: "Year"
    },
];

/**
 * Helper function to get the text from a ListItemBag that matches the value.
 * 
 * @param value The value to be searched for.
 * @param options The ListItemBag options to be searched.
 *
 * @returns The text value of the ListItemBag or an empty string if not found.
 */
function getTextForValue(value: string, options: ListItemBag[]): string {
    const matches = options.filter(v => v.value === value);

    return matches.length > 0 ? matches[0].text ?? "" : "";
}

/**
 * Gets the user friendly text that represents the RangeType value.
 * 
 * @param rangeType The RangeType value to be represented.
 *
 * @returns A human readable string that represents the RangeType value.
 */
export function getRangeTypeText(rangeType: RangeType): string {
    const rangeTypes = rangeTypeOptions.filter(o => o.value === rangeType.toString());

    return rangeTypes.length > 0 ? rangeTypes[0].text ?? "" : "";
}

/**
 * Gets the user friendly text that represents the TimeUnit value.
 *
 * @param rangeType The TimeUnit value to be represented.
 *
 * @returns A human readable string that represents the TimeUnit value.
 */
export function getTimeUnitText(timeUnit: TimeUnit): string {
    const timeUnits = timeUnitOptions.filter(o => o.value === timeUnit.toString());

    return timeUnits.length > 0 ? timeUnits[0].text ?? "" : "";
}

/**
 * Parses a pipe delimited string into a SlidingDateRange native object. The
 * delimited string is a format used by attribute values and other places.
 * 
 * @param value The pipe delimited string that should be parsed.
 *
 * @returns A SlidingDaterange object or null if the string could not be parsed.
 */
export function parseSlidingDateRangeString(value: string): SlidingDateRange | null {
    const segments = value.split("|");

    if (segments.length < 3) {
        return null;
    }

    // Find the matching range types and time units (should be 0 or 1) that
    // match the values in the string.
    const rangeTypes = rangeTypeOptions.filter(o => (o.text ?? "").replace(" ", "").toLowerCase() === segments[0].toLowerCase());
    const timeUnits = timeUnitOptions.filter(o => (o.text ?? "").toLowerCase() === segments[2].toLowerCase());

    if (rangeTypes.length === 0) {
        return null;
    }

    const range: SlidingDateRange = {
        rangeType: toNumber(rangeTypes[0].value)
    };

    // If the range type is one that has time units then parse the time units.
    if ([RangeType.Current, RangeType.Last, RangeType.Next, RangeType.Previous, RangeType.Upcoming].includes(range.rangeType)) {
        range.timeUnit = timeUnits.length > 0 ? toNumber(timeUnits[0].value) : TimeUnit.Hour;

        // If the range type is one that has time values then parse the time value.
        if ([RangeType.Last, RangeType.Next, RangeType.Previous, RangeType.Upcoming].includes(range.rangeType)) {
            range.timeValue = toNumberOrNull(segments[1]) ?? 1;
        }
    }

    // Parse the lower and upper dates if our range type is a DateRange.
    if (range.rangeType === RangeType.DateRange) {
        if (segments.length > 3) {
            range.lowerDate = segments[3];
        }

        if (segments.length > 4) {
            range.upperDate = segments[4];
        }
    }

    return range;
}

/**
 * Convert a SlidingDateRange object into a pipe delimited string that represents
 * the object. This string representation is used in attribute values as well as
 * other places in Rock.
 * 
 * @param value The SlidingDateRange object to be represented as a string.
 *
 * @returns A string that represents the SlidingDateRange object.
 */
export function slidingDateRangeToString(value: SlidingDateRange): string {
    switch (value.rangeType) {
        case RangeType.Current:
            return `Current||${getTextForValue(value.timeUnit?.toString() ?? "", timeUnitOptions)}||`;

        case RangeType.DateRange:
            return `DateRange|||${value.lowerDate ?? ""}|${value.upperDate ?? ""}`;

        default:
            return `${getTextForValue(value.rangeType.toString(), rangeTypeOptions)}|${value.timeValue ?? ""}|${getTextForValue(value.timeUnit?.toString() ?? "", timeUnitOptions)}||`;
    }
}
