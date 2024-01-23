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
import { toNumber, toNumberOrNull } from "./numberUtils";
import { SlidingDateRangeType as RangeType, SlidingDateRangeType } from "@Obsidian/Enums/Controls/slidingDateRangeType";
import { TimeUnitType as TimeUnit } from "@Obsidian/Enums/Controls/timeUnitType";
import { DayOfWeek, RockDateTime } from "./rockDateTime";

// This file contains helper functions and tooling required to work with sliding
// date ranges. A sliding date range is one that, generally, is anchored to whatever
// the current date and time is when the check is made. For example, "within the next
// 5 days" would be the english equivalent of a sliding date range.

/**
 * The enums have been moved to separate files in order to share with the back end. We import them
 * above (with the names used by the definitions that used to exist in this file) so they can be
 * used below and we export them here so that any files previously importing them from here
 * do not break.
 */
export { SlidingDateRangeType as RangeType } from "@Obsidian/Enums/Controls/slidingDateRangeType";
export { TimeUnitType as TimeUnit } from "@Obsidian/Enums/Controls/timeUnitType";

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
 * @param timeUnit The TimeUnit value to be represented.
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
    const rangeTypes = rangeTypeOptions.filter(o => (o.text ?? "").replace(" ", "").toLowerCase() === segments[0].toLowerCase() || o.value === segments[0]);
    const timeUnits = timeUnitOptions.filter(o => (o.text ?? "").toLowerCase() === segments[2].toLowerCase() || o.value === segments[2]);

    if (rangeTypes.length === 0) {
        return null;
    }

    const range: SlidingDateRange = {
        rangeType: toNumber(rangeTypes[0].value)
    };

    // If the range type is one that has time units then parse the time units.
    if (([RangeType.Current, RangeType.Last, RangeType.Next, RangeType.Previous, RangeType.Upcoming] as number[]).includes(range.rangeType)) {
        range.timeUnit = timeUnits.length > 0 ? toNumber(timeUnits[0].value) as TimeUnit : TimeUnit.Hour;

        // If the range type is one that has time values then parse the time value.
        if (([RangeType.Last, RangeType.Next, RangeType.Previous, RangeType.Upcoming] as number[]).includes(range.rangeType)) {
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
 * Formats the pipe delimited string.
 *
 * @param value The pipe delimited string that should be formatted.
 *
 * @returns A string that formats the sliding date range.
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

/**
 * Calculates the start and end dates in a sliding date range.
 *
 * @param value The sliding date range to use when calculating dates.
 * @param currentDateTime The date and time to use in any "now" calculations.
 *
 * @returns An object that contains the start and end dates and times.
 */
export function calculateSlidingDateRange(value: SlidingDateRange, currentDateTime: RockDateTime | null | undefined = undefined): { start: RockDateTime | null, end: RockDateTime | null } {
    const result: { start: RockDateTime | null, end: RockDateTime | null } = {
        start: null,
        end: null
    };

    if (!currentDateTime) {
        currentDateTime = RockDateTime.now();
    }

    if (value.rangeType === RangeType.Current) {
        if (value.timeUnit === TimeUnit.Hour) {
            result.start = RockDateTime.fromParts(currentDateTime.year, currentDateTime.month, currentDateTime.day, currentDateTime.hour, 0, 0);
            result.end = result.start?.addHours(1) ?? null;
        }
        else if (value.timeUnit === TimeUnit.Day) {
            result.start = currentDateTime.date;
            result.end = result.start.addDays(1);
        }
        else if (value.timeUnit === TimeUnit.Week) {
            // TODO: This needs to be updated to get the FirstDayOfWeek from server.
            let diff = currentDateTime.dayOfWeek - DayOfWeek.Monday;

            if (diff < 0) {
                diff += 7;
            }

            result.start = currentDateTime.addDays(-1 * diff).date;
            result.end = result.start.addDays(7);
        }
        else if (value.timeUnit === TimeUnit.Month) {
            result.start = RockDateTime.fromParts(currentDateTime.year, currentDateTime.month, 1);
            result.end = result.start?.addMonths(1) ?? null;
        }
        else if (value.timeUnit === TimeUnit.Year) {
            result.start = RockDateTime.fromParts(currentDateTime.year, 1, 1);
            result.end = RockDateTime.fromParts(currentDateTime.year + 1, 1, 1);
        }
    }
    else if (value.rangeType === RangeType.Last || value.rangeType === RangeType.Previous) {
        // The number of time units to adjust by.
        const count = value.timeValue ?? 1;

        // If we are getting "Last" then round up to include the
        // current day/week/month/year.
        const roundUpCount = value.rangeType === RangeType.Last ? 1 : 0;

        if (value.timeUnit === TimeUnit.Hour) {
            result.end = RockDateTime.fromParts(currentDateTime.year, currentDateTime.month, currentDateTime.day, currentDateTime.hour, 0, 0)
                ?.addHours(roundUpCount) ?? null;
            result.start = result.end?.addHours(-count) ?? null;
        }
        else if (value.timeUnit === TimeUnit.Day) {
            result.end = currentDateTime.date.addDays(roundUpCount);
            result.start = result.end?.addDays(-count) ?? null;
        }
        else if (value.timeUnit === TimeUnit.Week) {
            // TODO: This needs to be updated to get the FirstDayOfWeek from server.
            let diff = currentDateTime.dayOfWeek - DayOfWeek.Monday;

            if (diff < 0) {
                diff += 7;
            }

            result.end = currentDateTime.addDays(-1 * diff).date.addDays(7 * roundUpCount);
            result.start = result.end.addDays(-count * 7);
        }
        else if (value.timeUnit === TimeUnit.Month) {
            result.end = RockDateTime.fromParts(currentDateTime.year, currentDateTime.month, 1)?.addMonths(roundUpCount) ?? null;
            result.start = result.end?.addMonths(-count) ?? null;
        }
        else if (value.timeUnit === TimeUnit.Year) {
            result.end = RockDateTime.fromParts(currentDateTime.year, 1, 1)?.addYears(roundUpCount) ?? null;
            result.start = result.end?.addYears(-count) ?? null;
        }

        // don't let Last,Previous have any future dates
        const cutoffDate = currentDateTime.date.addDays(1);
        if (result.end && result.end.date > cutoffDate) {
            result.end = cutoffDate;
        }
    }
    else if (value.rangeType === RangeType.Next || value.rangeType === RangeType.Upcoming) {
        // The number of time units to adjust by.
        const count = value.timeValue ?? 1;

        // If we are getting "Upcoming" then round up to include the
        // current day/week/month/year.
        const roundUpCount = value.rangeType === RangeType.Upcoming ? 1 : 0;

        if (value.timeUnit === TimeUnit.Hour) {
            result.start = RockDateTime.fromParts(currentDateTime.year, currentDateTime.month, currentDateTime.day, currentDateTime.hour, 0, 0)
                ?.addHours(roundUpCount) ?? null;
            result.end = result.start?.addHours(count) ?? null;
        }
        else if (value.timeUnit === TimeUnit.Day) {
            result.start = currentDateTime.date.addDays(roundUpCount);
            result.end = result.start.addDays(count);
        }
        else if (value.timeUnit === TimeUnit.Week) {
            // TODO: This needs to be updated to get the FirstDayOfWeek from server.
            let diff = currentDateTime.dayOfWeek - DayOfWeek.Monday;

            if (diff < 0) {
                diff += 7;
            }

            result.start = currentDateTime.addDays(-1 * diff)
                .date.addDays(7 * roundUpCount);
            result.end = result.start.addDays(count * 7);
        }
        else if (value.timeUnit === TimeUnit.Month) {
            result.start = RockDateTime.fromParts(currentDateTime.year, currentDateTime.month, 1)
                ?.addMonths(roundUpCount) ?? null;
            result.end = result.start?.addMonths(count) ?? null;
        }
        else if (value.timeUnit === TimeUnit.Year) {
            result.start = RockDateTime.fromParts(currentDateTime.year, 1, 1)
                ?.addYears(roundUpCount) ?? null;
            result.end = result.start?.addYears(count) ?? null;
        }

        // don't let Next,Upcoming have any past dates
        if (result.start && result.start.date < currentDateTime.date) {
            result.start = currentDateTime.date;
        }
    }
    else if (value.rangeType === RangeType.DateRange) {
        result.start = RockDateTime.parseISO(value.lowerDate ?? "");
        result.end = RockDateTime.parseISO(value.upperDate ?? "");

        // Sliding date range does not use ISO dates (though might be changed
        // in the future). So if we can't parse as an ISO date then try a
        // natural parse.
        if (!result.start && value.lowerDate) {
            result.start = RockDateTime.fromJSDate(new Date(value.lowerDate));
        }

        if (!result.end && value.upperDate) {
            result.end = RockDateTime.fromJSDate(new Date(value.upperDate));
        }

        if (result.end) {
            // Add a day to the end so that we get the entire day when comparing.
            result.end = result.end.addDays(1);
        }
    }

    // To avoid confusion about the day or hour of the end of the date range,
    // subtract a millisecond off our 'less than' end date. For example, if our
    // end date is 2019-11-7, we actually want all the data less than 2019-11-8,
    // but if a developer does EndDate.DayOfWeek, they would want 2019-11-7 and
    // not 2019-11-8 So, to make sure we include all the data for 2019-11-7, but
    // avoid the confusion about what DayOfWeek of the end, we'll compromise by
    // subtracting a millisecond from the end date
    if (result.end && value.timeUnit != TimeUnit.Hour) {
        result.end = result.end.addMilliseconds(-1);
    }

    return result;
}
