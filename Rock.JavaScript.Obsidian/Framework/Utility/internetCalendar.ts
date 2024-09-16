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

import { DayOfWeek, RockDateTime } from "./rockDateTime";
import { newGuid } from "./guid";
import { toNumberOrNull } from "./numberUtils";
import { pluralConditional } from "./stringUtils";

type Frequency = "DAILY" | "WEEKLY" | "MONTHLY";

/**
 * The day of the week and an interval number for that particular day.
 */
export type WeekdayNumber = {
    /** The interval number for this day. */
    value: number;

    /** The day of the week. */
    day: DayOfWeek;
};

// Abbreviate nth lookup table.
const nthNamesAbbreviated: [number, string][] = [
    [1, "1st"],
    [2, "2nd"],
    [3, "3rd"],
    [4, "4th"],
    [-1, "last"]
];

// #region Internal Functions

/**
 * Converts the number to a string and pads the left with zeros to make up
 * the minimum required length.
 *
 * @param value The value to be converted to a string.
 * @param length The minimum required length of the final string.
 *
 * @returns A string that represents the value.
 */
function padZeroLeft(value: number, length: number): string {
    const str = value.toString();

    return "0".repeat(length - str.length) + str;
}

/**
 * Get a date-only string that can be used in the iCal format.
 *
 * @param date The date object to be converted to a string.
 *
 * @returns A string that represents only the date portion of the parameter.
 */
function getDateString(date: RockDateTime): string {
    const year = date.year;
    const month = date.month;
    const day = date.day;

    return `${year}${padZeroLeft(month, 2)}${padZeroLeft(day, 2)}`;
}

/**
 * Gets a time-only string that can be used in the iCal format.
 *
 * @param date The date object to be converted to a string.
 *
 * @returns A string that represents only the time portion of the parameter.
 */
function getTimeString(date: RockDateTime): string {
    const hour = date.hour;
    const minute = date.minute;
    const second = date.second;

    return `${padZeroLeft(hour, 2)}${padZeroLeft(minute, 2)}${padZeroLeft(second, 2)}`;
}

/**
 * Gets a date and time string that can be used in the iCal format.
 *
 * @param date The date object to be converted to a string.
 *
 * @returns A string that represents only the date and time of the parameter.
 */
function getDateTimeString(date: RockDateTime): string {
    return `${getDateString(date)}T${getTimeString(date)}`;
}

/**
 * Gets all the date objects from a range or period string value. This converts
 * from an iCal format into a set of date objects.
 *
 * @param value The string value in iCal format.
 *
 * @returns An array of date objects that represents the range or period value.
 */
function getDatesFromRangeOrPeriod(value: string): RockDateTime[] {
    const segments = value.split("/");

    if (segments.length === 0) {
        return [];
    }

    const startDate = getDateFromString(segments[0]);
    if (!startDate) {
        return [];
    }

    if (segments.length !== 2) {
        return [startDate];
    }

    const dates: RockDateTime[] = [];

    if (segments[1].startsWith("P")) {
        // Value is a period so we have a start date and then a period marker
        // to tell us how long that date extends.
        const days = getPeriodDurationInDays(segments[1]);

        for (let day = 0; day < days; day++) {
            const date = startDate.addDays(day);
            if (date) {
                dates.push(date);
            }
        }
    }
    else {
        // Value is a date range so we have a start date and then an end date
        // and we need to fill in the dates in between.
        const endDate = getDateFromString(segments[1]);

        if (endDate !== null) {
            let date = startDate;

            while (date <= endDate) {
                dates.push(date);
                date = date.addDays(1);
            }
        }
    }

    return dates;
}

/**
 * Get a date object that only has the date portion filled in from the iCal
 * date string. The time will be set to midnight.
 *
 * @param value An iCal date value.
 *
 * @returns A date object that represents the iCal date value.
 */
function getDateFromString(value: string): RockDateTime | null {
    if (value.length < 8) {
        return null;
    }

    const year = parseInt(value.substring(0, 4));
    const month = parseInt(value.substring(4, 6));
    const day = parseInt(value.substring(6, 8));

    return RockDateTime.fromParts(year, month, day);
}

/**
 * Get a date object that has both the date and time filled in from the iCal
 * date string.
 *
 * @param value An iCal date value.
 *
 * @returns A date object that represents the iCal date value.
 */
function getDateTimeFromString(value: string): RockDateTime | null {
    if (value.length < 15 || value[8] !== "T") {
        return null;
    }

    const year = parseInt(value.substring(0, 4));
    const month = parseInt(value.substring(4, 6));
    const day = parseInt(value.substring(6, 8));
    const hour = parseInt(value.substring(9, 11));
    const minute = parseInt(value.substring(11, 13));
    const second = parseInt(value.substring(13, 15));

    return RockDateTime.fromParts(year, month, day, hour, minute, second);
}

/**
 * Gets an iCal period duration in the number of days.
 *
 * @param period The iCal period definition.
 *
 * @returns The number of days found in the definition.
 */
function getPeriodDurationInDays(period: string): number {
    // These are in a format like P1D, P2W, etc.
    if (!period.startsWith("P")) {
        return 0;
    }

    if (period.endsWith("D")) {
        return parseInt(period.substring(1, period.length - 1));
    }
    else if (period.endsWith("W")) {
        return parseInt(period.substring(1, period.length - 1)) * 7;
    }

    return 0;
}

/**
 * Gets the specific recurrence dates from a RDATE iCal value string.
 *
 * @param attributes The attributes that were defined on the RDATE property.
 * @param value The value of the RDATE property.
 *
 * @returns An array of date objects found in the RDATE value.
 */
function getRecurrenceDates(attributes: Record<string, string>, value: string): RockDateTime[] {
    const recurrenceDates: RockDateTime[] = [];
    const valueParts = value.split(",");
    let valueType = attributes["VALUE"];

    for (const valuePart of valueParts) {
        if(!valueType) {
            // The value type is unspecified and it could be a PERIOD, DATE-TIME or a DATE.
            // Determine it based on the length and the contents of the valuePart string.

            const length = valuePart.length;

            if (length === 8) { // Eg: 20240117
                valueType = "DATE";
            }
            else if ((length === 15 || length === 16) && valuePart[8] === "T") { // Eg: 19980119T020000, 19970714T173000Z
                valueType = "DATE-TIME";
            }
            else { // Eg: 20240201/20240202, 20240118/P1D
                valueType = "PERIOD";
            }
        }


        if (valueType === "PERIOD") {
            // Values are stored in period format, such as "20221005/P1D".
            recurrenceDates.push(...getDatesFromRangeOrPeriod(valuePart));
        }
        else if (valueType === "DATE") {
            // Values are date-only values.
            const date = getDateFromString(valuePart);
            if (date) {
                recurrenceDates.push(date);
            }
        }
        else if (valueType === "DATE-TIME")  {
            // Values are date and time values.
            const date = getDateTimeFromString(valuePart);
            if (date) {
                recurrenceDates.push(date);
            }
        }
    }

    return recurrenceDates;
}

/**
 * Gets the name of the weekday from the iCal abbreviation.
 *
 * @param day The iCal day abbreviation.
 *
 * @returns A string that represents the day name.
 */
function getWeekdayName(day: DayOfWeek): "Sunday" | "Monday" | "Tuesday" | "Wednesday" | "Thursday" | "Friday" | "Saturday" | "Unknown" {
    if (day === DayOfWeek.Sunday) {
        return "Sunday";
    }
    else if (day === DayOfWeek.Monday) {
        return "Monday";
    }
    else if (day === DayOfWeek.Tuesday) {
        return "Tuesday";
    }
    else if (day === DayOfWeek.Wednesday) {
        return "Wednesday";
    }
    else if (day === DayOfWeek.Thursday) {
        return "Thursday";
    }
    else if (day === DayOfWeek.Friday) {
        return "Friday";
    }
    else if (day === DayOfWeek.Saturday) {
        return "Saturday";
    }
    else {
        return "Unknown";
    }
}

/**
 * Checks if the date matches one of the weekday options.
 *
 * @param rockDate The date that must match one of the weekday options.
 * @param days The array of weekdays that the date must match.
 *
 * @returns True if the date matches; otherwise false.
 */
function dateMatchesDays(rockDate: RockDateTime, days: DayOfWeek[]): boolean {
    for (const day of days) {
        if (rockDate.dayOfWeek === day) {
            return true;
        }
    }

    return false;
}

/**
 * Checks if the date matches the specifie day of week and also the offset into
 * the month for that day.
 *
 * @param rockDate The date object to be checked.
 * @param dayOfWeek The day of week the date must be on.
 * @param offsets The offset in week, such as 2 meaning the second 'dayOfWeek' or -1 meaning the last 'dayOfWeek'.
 *
 * @returns True if the date matches the options; otherwise false.
 */
function dateMatchesOffsetDayOfWeeks(rockDate: RockDateTime, dayOfWeek: DayOfWeek, offsets: number[]): boolean {
    if (!dateMatchesDays(rockDate, [dayOfWeek])) {
        return false;
    }

    const dayOfMonth = rockDate.day;

    for (const offset of offsets) {
        if (offset === 1 && dayOfMonth >= 1 && dayOfMonth <= 7) {
            return true;
        }
        else if (offset === 2 && dayOfMonth >= 8 && dayOfMonth <= 14) {
            return true;
        }
        else if (offset === 3 && dayOfMonth >= 15 && dayOfMonth <= 21) {
            return true;
        }
        else if (offset === 4 && dayOfMonth >= 22 && dayOfMonth <= 28) {
            return true;
        }
        else if (offset === -1) {
            const lastDayOfMonth = rockDate.addDays(-(rockDate.day - 1)).addMonths(1).addDays(-1).day;

            if (dayOfMonth >= (lastDayOfMonth - 7) && dayOfMonth <= lastDayOfMonth) {
                return true;
            }
        }
    }

    return false;
}

/**
 * Gets the DayOfWeek value that corresponds to the iCal formatted weekday.
 *
 * @param day The day of the week to be parsed.
 *
 * @returns A DayOfWeek value that represents the day.
 */
function getDayOfWeekFromIcalDay(day: "SU" | "MO" | "TU" | "WE" | "TH" | "FR" | "SA"): DayOfWeek {
    switch (day) {
        case "SU":
            return DayOfWeek.Sunday;

        case "MO":
            return DayOfWeek.Monday;
        case "TU":
            return DayOfWeek.Tuesday;

        case "WE":
            return DayOfWeek.Wednesday;

        case "TH":
            return DayOfWeek.Thursday;

        case "FR":
            return DayOfWeek.Friday;

        case "SA":
            return DayOfWeek.Saturday;
    }
}

/**
 * Gets the iCal abbreviation for the day of the week.
 *
 * @param day The day of the week to be converted to iCal format.
 *
 * @returns An iCal representation of the day of week.
 */
function getiCalDay(day: DayOfWeek): "SU" | "MO" | "TU" | "WE" | "TH" | "FR" | "SA" {
    switch (day) {
        case DayOfWeek.Sunday:
            return "SU";

        case DayOfWeek.Monday:
            return "MO";

        case DayOfWeek.Tuesday:
            return "TU";

        case DayOfWeek.Wednesday:
            return "WE";

        case DayOfWeek.Thursday:
            return "TH";

        case DayOfWeek.Friday:
            return "FR";

        case DayOfWeek.Saturday:
            return "SA";
    }
}

/**
 * Normalizes line length so that none of the individual lines exceed the
 * maximum length of 75 charactes from the RFC.
 *
 * @param lines The array of lines to be normalized.
 *
 * @returns A new array with the lines normalized for length.
 */
function normalizeLineLength(lines: string[]): string[] {
    const newLines: string[] = [...lines];

    for (let lineNumber = 0; lineNumber < newLines.length; lineNumber++) {
        // Spec does not allow lines longer than 75 characters.
        if (newLines[lineNumber].length > 75) {
            const currentLine = newLines[lineNumber].substring(0, 75);
            const newLine = " " + newLines[lineNumber].substring(75);

            newLines.splice(lineNumber, 1, currentLine, newLine);
        }
    }

    return newLines;
}

/**
 * Denormalizes line length so that any continuation lines are appending
 * to the previous line for proper parsing.
 *
 * @param lines The array of lines to be denormalized.
 *
 * @returns A new array with the lines denormalized.
 */
function denormalizeLineLength(lines: string[]): string[] {
    const newLines: string[] = [...lines];

    for (let lineNumber = 1; lineNumber < newLines.length;) {
        if (newLines[lineNumber][0] === " ") {
            newLines[lineNumber - 1] += newLines[lineNumber].substring(1);
            newLines.splice(lineNumber, 1);
        }
        else {
            lineNumber++;
        }
    }

    return newLines;
}

// #endregion

/**
 * Helper utility to feed lines into ICS parsers.
 */
class LineFeeder {
    // #region Properties

    /**
     * The denormalzied lines that represent the ICS data.
     */
    private lines: string[];

    // #endregion

    // #region Constructors

    /**
     * Creates a new LineFeeder with the given content.
     *
     * @param content A string that represents raw ICS data.
     */
    constructor(content: string) {
        const lines = content.split(/\r\n|\n|\r/);

        this.lines = denormalizeLineLength(lines);
    }

    // #endregion

    // #region Functions

    /**
     * Peek at the next line to be read from the feeder.
     *
     * @returns The next line to be read or null if no more lines remain.
     */
    public peek(): string | null {
        if (this.lines.length === 0) {
            return null;
        }

        return this.lines[0];
    }

    /**
     * Pops the next line from the feeder, removing it.
     *
     * @returns The line that was removed from the feeder or null if no lines remain.
     */
    public pop(): string | null {
        if (this.lines.length === 0) {
            return null;
        }

        return this.lines.splice(0, 1)[0];
    }

    // #endregion
}

/**
 * Logic and structure for a rule that defines when an even recurs on
 * different dates.
 */
export class RecurrenceRule {
    // #region Properties

    /**
     * The frequency of this recurrence. Only Daily, Weekly and Monthly
     * are supported.
     */
    public frequency?: Frequency;

    /**
     * The date at which no more event dates will be generated. This is
     * an exclusive date, meaning if an event date lands on this date
     * then it will not be included in the list of dates.
     */
    public endDate?: RockDateTime;

    /**
     * The maximum number of dates, including the original date, that
     * should be generated.
     */
    public count?: number;

    /**
     * The interval between dates based on the frequency. If this value is
     * 2 and frequency is Weekly, then you are asking for "every other week".
     */
    public interval: number = 1;

    /**
     * The days of the month the event should recur on. Only a single value
     * is supported currently.
     */
    public byMonthDay: number[] = [];

    /**
     * The days of the week the event shoudl recur on.
     */
    public byDay: WeekdayNumber[] = [];

    // #endregion

    // #region Constructors

    /**
     * Creates a new recurrence rule that can be used to define or adjust the
     * recurrence pattern of an event.
     *
     * @param rule An existing RRULE string from an iCal file.
     *
     * @returns A new instance that can be used to adjust or define the rule.
     */
    public constructor(rule: string | undefined = undefined) {
        if (!rule) {
            return;
        }

        // Rule has a format like "FREQ=DAILY;COUNT=5" so we split by semicolon
        // first and then sub-split by equals character and then stuff everything
        // into this values object.
        const values: Record<string, string> = {};

        for (const attr of rule.split(";")) {
            const attrParts = attr.split("=");
            if (attrParts.length === 2) {
                values[attrParts[0]] = attrParts[1];
            }
        }

        // Make sure the values we have are valid.
        if (values["UNTIL"] !== undefined && values["COUNT"] !== undefined) {
            throw new Error(`Recurrence rule '${rule}' cannot specify both UNTIL and COUNT.`);
        }

        if (values["FREQ"] !== "DAILY" && values["FREQ"] !== "WEEKLY" && values["FREQ"] !== "MONTHLY") {
            throw new Error(`Invalid frequence for recurrence rule '${rule}'.`);
        }

        this.frequency = values["FREQ"];

        if (values["UNTIL"]?.length === 8) {
            this.endDate = getDateFromString(values["UNTIL"]) ?? undefined;
        }
        else if (values["UNTIL"]?.length >= 15) {
            this.endDate = getDateTimeFromString(values["UNTIL"]) ?? undefined;
        }

        if (values["COUNT"] !== undefined) {
            this.count = toNumberOrNull(values["COUNT"]) ?? undefined;
        }

        if (values["INTERVAL"] !== undefined) {
            this.interval = toNumberOrNull(values["INTERVAL"]) ?? 1;
        }

        if (values["BYMONTHDAY"] !== undefined && values["BYMONTHDAY"].length > 0) {
            this.byMonthDay = [];

            for (const v of values["BYMONTHDAY"].split(",")) {
                const num = toNumberOrNull(v);
                if (num !== null) {
                    this.byMonthDay.push(num);
                }
            }
        }

        if (values["BYDAY"] !== undefined && values["BYDAY"].length > 0) {
            this.byDay = [];

            for (const v of values["BYDAY"].split(",")) {
                if (v.length < 2) {
                    continue;
                }

                const num = v.length > 2 ? toNumberOrNull(v.substring(0, v.length - 2)) : 1;
                const day = v.substring(v.length - 2);

                if (num === null) {
                    continue;
                }

                if (day === "SU" || day === "MO" || day === "TU" || day == "WE" || day == "TH" || day == "FR" || day == "SA") {
                    this.byDay.push({
                        value: num,
                        day: getDayOfWeekFromIcalDay(day)
                    });
                }
            }
        }
    }

    // #endregion

    // #region Functions

    /**
     * Builds and returns the RRULE value for an iCal file export.
     *
     * @returns A RRULE value that represents the recurrence rule.
     */
    public build(): string {
        const attributes: string[] = [];

        attributes.push(`FREQ=${this.frequency}`);

        if (this.count !== undefined) {
            attributes.push(`COUNT=${this.count}`);
        }
        else if (this.endDate) {
            attributes.push(`UNTIL=${getDateTimeString(this.endDate)}`);
        }

        if (this.interval > 1) {
            attributes.push(`INTERVAL=${this.interval}`);
        }

        if (this.byMonthDay.length > 0) {
            const monthDayValues = this.byMonthDay.map(md => md.toString()).join(",");
            attributes.push(`BYMONTHDAY=${monthDayValues}`);
        }

        if (this.frequency === "MONTHLY" && this.byDay.length > 0) {
            const dayValues = this.byDay.map(d => `${d.value}${getiCalDay(d.day)}`);
            attributes.push(`BYDAY=${dayValues}`);
        }
        else if (this.byDay.length > 0) {
            const dayValues = this.byDay.map(d => d.value !== 1 ? `${d.value}${getiCalDay(d.day)}` : getiCalDay(d.day));
            attributes.push(`BYDAY=${dayValues}`);
        }

        return attributes.join(";");
    }

    /**
     * Gets all the dates within the range that match the recurrence rule. A
     * maximum of 100,000 dates will be returned by this function.
     *
     * @param eventStartDateTime The start date and time of the primary event this rule is for.
     * @param startDateTime The inclusive starting date and time that events should be returned for.
     * @param endDateTime The exclusive ending date and time that events should be returned for.
     *
     * @returns An array of date objects that represent the additional dates and times for the event.
     */
    public getDates(eventStartDateTime: RockDateTime, startDateTime: RockDateTime, endDateTime: RockDateTime): RockDateTime[] {
        const dates: RockDateTime[] = [];
        let rockDate = eventStartDateTime;
        let dateCount = 0;

        if (!rockDate) {
            return [];
        }

        if (this.endDate && this.endDate < endDateTime) {
            endDateTime = this.endDate;
        }

        while (rockDate < endDateTime && dateCount < 100_000) {
            if (this.count && dateCount >= this.count) {
                break;
            }

            dateCount++;

            if (rockDate >= startDateTime) {
                dates.push(rockDate);
            }

            const nextDate = this.nextDateAfter(rockDate);

            if (nextDate === null) {
                break;
            }
            else {
                rockDate = nextDate;
            }
        }

        return dates;
    }

    /**
     * Gets the next valid date after the specified date based on our recurrence
     * rules.
     *
     * @param rockDate The reference date that should be used when calculation the next date.
     *
     * @returns The next date after the reference date or null if one cannot be determined.
     */
    private nextDateAfter(rockDate: RockDateTime): RockDateTime | null {
        if (this.frequency === "DAILY") {
            return rockDate.addDays(this.interval);
        }
        else if (this.frequency === "WEEKLY" && this.byDay.length > 0) {
            let nextDate = rockDate;

            if (nextDate.dayOfWeek === DayOfWeek.Saturday) {
                // On saturday process any skip intervals to move past the next n weeks.
                nextDate = nextDate.addDays(1 + ((this.interval - 1) * 7));
            }
            else {
                nextDate = nextDate.addDays(1);
            }

            while (!dateMatchesDays(nextDate, this.byDay.map(d => d.day))) {
                if (nextDate.dayOfWeek === DayOfWeek.Saturday) {
                    // On saturday process any skip intervals to move past the next n weeks.
                    nextDate = nextDate.addDays(1 + ((this.interval - 1) * 7));
                }
                else {
                    nextDate = nextDate.addDays(1);
                }
            }

            return nextDate;
        }
        else if (this.frequency === "MONTHLY") {
            if (this.byMonthDay.length > 0) {
                let nextDate = rockDate.addDays(-(rockDate.day - 1));

                if (rockDate.day >= this.byMonthDay[0]) {
                    nextDate = nextDate.addMonths(this.interval);
                }

                let lastDayOfMonth = nextDate.addMonths(1).addDays(-1).day;
                let loopCount = 0;

                // Skip any months that don't have this day number.
                while (lastDayOfMonth < this.byMonthDay[0]) {
                    nextDate = nextDate.addMonths(this.interval);

                    lastDayOfMonth = nextDate.addMonths(1).addDays(-1).day;

                    // Fail-safe check so we don't get stuck looping forever
                    // if the rule is one that can't be determined. Such as a
                    // rule for the 30th day of the month every 12 months
                    // starting in February.
                    if (loopCount++ >= 100) {
                        return null;
                    }
                }

                nextDate = nextDate.addDays(this.byMonthDay[0] - 1);

                return nextDate;
            }
            else if (this.byDay.length > 0) {
                const dayOfWeek = this.byDay[0].day;
                const offsets = this.byDay.map(d => d.value);

                let nextDate = rockDate.addDays(1);

                while (!dateMatchesOffsetDayOfWeeks(nextDate, dayOfWeek, offsets)) {
                    nextDate = nextDate.addDays(1);
                }

                return nextDate;
            }
        }

        return null;
    }

    // #endregion
}

/**
 * A single event inside a calendar.
 */
export class Event {
    // #region Properties

    /**
     * The unique identifier for this schedule used in the scheduled event.
     */
    public uid?: string;

    /**
     * The first date and time that the event occurs on. This must be provided
     * before the schedule can be built.
     */
    public startDateTime?: RockDateTime;

    /**
     * The end date and time for the event. This must be provided before
     * this schedule can be built.
     */
    public endDateTime?: RockDateTime;

    /**
     * An array of dates to be excluded from the recurrence rules.
     */
    public excludedDates: RockDateTime[] = [];

    /**
     * An array of specific dates that this schedule will recur on. This is
     * only valid if recurrenceRules contains no rules.
     */
    public recurrenceDates: RockDateTime[] = [];

    /**
     * The rules that define when this schedule recurs on for additional dates.
     * Only the first rule is currently supported.
     */
    public recurrenceRules: RecurrenceRule[] = [];

    // #endregion

    // #region Constructors

    /**
     * Creates a new internet calendar event.
     *
     * @param icsContent The content from the ICS file that represents this single event.
     *
     * @returns A new Event instance.
     */
    public constructor(icsContent: string | LineFeeder | undefined = undefined) {
        if (icsContent === undefined) {
            this.uid = newGuid();
            return;
        }

        let feeder: LineFeeder;

        if (typeof icsContent === "string") {
            feeder = new LineFeeder(icsContent);
        }
        else {
            feeder = icsContent;
        }

        this.parse(feeder);
    }

    // #endregion

    // #region Functions

    /**
     * Build the event as a list of individual lines that make up the event in
     * the ICS file format.
     *
     * @returns An array of lines to be inserted into an ICS file.
     */
    public buildLines(): string[] {
        if (!this.startDateTime || !this.endDateTime) {
            return [];
        }

        const lines: string[] = [];

        lines.push("BEGIN:VEVENT");
        lines.push(`DTEND:${getDateTimeString(this.endDateTime)}`);
        lines.push(`DTSTAMP:${getDateTimeString(RockDateTime.now())}`);
        lines.push(`DTSTART:${getDateTimeString(this.startDateTime)}`);

        if (this.excludedDates.length > 0) {
            lines.push(`EXDATE:${this.excludedDates.map(d => getDateString(d) + "/P1D").join(",")}`);
        }

        if (this.recurrenceDates.length > 0) {
            const recurrenceDates: string[] = [];
            for (const date of this.recurrenceDates) {
                const rDate = RockDateTime.fromParts(date.year, date.month, date.day, this.startDateTime.hour, this.startDateTime.minute, this.startDateTime.second);
                if (rDate) {
                    recurrenceDates.push(getDateTimeString(rDate));
                }
            }

            lines.push(`RDATE:${recurrenceDates.join(",")}`);
        }
        else if (this.recurrenceRules.length > 0) {
            for (const rrule of this.recurrenceRules) {
                lines.push(`RRULE:${rrule.build()}`);
            }
        }

        lines.push("SEQUENCE:0");
        lines.push(`UID:${this.uid}`);
        lines.push("END:VEVENT");

        return lines;
    }

    /**
     * Builds the event into a string that conforms to ICS format.
     *
     * @returns An ICS formatted string that represents the event data.
     */
     public build(): string | null {
        const lines = this.buildLines();

        if (lines.length === 0) {
            return null;
        }

        return normalizeLineLength(lines).join("\r\n");
    }

    /**
     * Parse data from an existing event and store it on this instance.
     *
     * @param feeder The feeder that will provide the line data for parsing.
     */
    private parse(feeder: LineFeeder): void {
        let duration: string | null = null;
        let line: string | null;

        // Verify this is an event.
        if (feeder.peek() !== "BEGIN:VEVENT") {
            throw new Error("Invalid event.");
        }

        feeder.pop();

        // Parse the line until we run out of lines or see an END line.
        while ((line = feeder.pop()) !== null) {
            if (line === "END:VEVENT") {
                break;
            }

            const splitAt = line.indexOf(":");
            if (splitAt < 0) {
                continue;
            }

            let key = line.substring(0, splitAt);
            const value = line.substring(splitAt + 1);

            const keyAttributes: Record<string, string> = {};
            const keySegments = key.split(";");
            if (keySegments.length > 1) {
                key = keySegments[0];
                keySegments.splice(0, 1);

                for (const attr of keySegments) {
                    const attrSegments = attr.split("=");
                    if (attr.length === 2) {
                        keyAttributes[attrSegments[0]] = attrSegments[1];
                    }
                }
            }

            if (key === "DTSTART") {
                this.startDateTime = getDateTimeFromString(value) ?? undefined;
            }
            else if (key === "DTEND") {
                this.endDateTime = getDateTimeFromString(value) ?? undefined;
            }
            else if (key === "RRULE") {
                this.recurrenceRules.push(new RecurrenceRule(value));
            }
            else if (key === "RDATE") {
                this.recurrenceDates = getRecurrenceDates(keyAttributes, value);
            }
            else if (key === "UID") {
                this.uid = value;
            }
            else if (key === "DURATION") {
                duration = value;
            }
            else if (key === "EXDATE") {
                const dateValues = value.split(",");
                for (const dateValue of dateValues) {
                    const dates = getDatesFromRangeOrPeriod(dateValue);
                    this.excludedDates.push(...dates);
                }
            }
        }

        if (duration !== null) {
            // TODO: Calculate number of seconds and add to startDate.
        }
    }

    /**
     * Determines if the date is listed in one of the excluded dates. This
     * currently only checks the excludedDates but in the future might also
     * check the excluded rules.
     *
     * @param rockDate The date to be checked to see if it is excluded.
     *
     * @returns True if the date is excluded; otherwise false.
     */
    private isDateExcluded(rockDate: RockDateTime): boolean {
        const rockDateOnly = rockDate.date;

        for (const excludedDate of this.excludedDates) {
            if (excludedDate.date.isEqualTo(rockDateOnly)) {
                return true;
            }
        }

        return false;
    }

    /**
     * Get all the dates for this event that fall within the specified date range.
     *
     * @param startDateTime The inclusive starting date to use when filtering event dates.
     * @param endDateTime The exclusive endign date to use when filtering event dates.
     *
     * @returns An array of dates that fall between startDateTime and endDateTime.
     */
    public getDates(startDateTime: RockDateTime, endDateTime: RockDateTime): RockDateTime[] {
        if (!this.startDateTime) {
            return [];
        }

        // If the schedule has a startDateTime that is later than the requested
        // startDateTime then use ours instead.
        if (this.startDateTime > startDateTime) {
            startDateTime = this.startDateTime;
        }

        if (this.recurrenceDates.length > 0) {
            const dates: RockDateTime[] = [];
            const recurrenceDates: RockDateTime[] = [this.startDateTime, ...this.recurrenceDates];

            for (const date of recurrenceDates) {
                if (date >= startDateTime && date < endDateTime) {
                    dates.push(date);
                }
            }

            return dates;
        }
        else if (this.recurrenceRules.length > 0) {
            const rrule = this.recurrenceRules[0];

            return rrule.getDates(this.startDateTime, startDateTime, endDateTime)
                .filter(d => !this.isDateExcluded(d));
        }
        else {
            if (this.startDateTime >= startDateTime && this.startDateTime < endDateTime) {
                return [this.startDateTime];
            }

            return [];
        }
    }

    /**
     * Get the friendly text string that represents this event. This will be a
     * plain text string with no formatting applied.
     *
     * @returns A string that represents the event in a human friendly manner.
     */
    public toFriendlyText(): string {
        return this.toFriendlyString(false);
    }

    /**
     * Get the friendly HTML string that represents this event. This will be
     * formatted with HTML to make the information easier to read.
     *
     * @returns A string that represents the event in a human friendly manner.
     */
    public toFriendlyHtml(): string {
        return this.toFriendlyString(true);
    }

    /**
     * Get the friendly string that can be easily understood by a human.
     *
     * @param html If true then the string can contain HTML content to make things easier to read.
     *
     * @returns A string that represents the event in a human friendly manner.
     */
    private toFriendlyString(html: boolean): string {
        if (!this.startDateTime) {
            return "";
        }

        const startTimeText = this.startDateTime.toLocaleString({ hour: "numeric", minute: "2-digit", hour12: true });

        if (this.recurrenceRules.length > 0) {
            const rrule = this.recurrenceRules[0];

            if (rrule.frequency === "DAILY") {
                let result = "Daily";

                if (rrule.interval > 1) {
                    result += ` every ${rrule.interval} ${pluralConditional(rrule.interval, "day", "days")}`;
                }

                result += ` at ${startTimeText}`;

                return result;
            }
            else if (rrule.frequency === "WEEKLY") {
                if (rrule.byDay.length === 0) {
                    return "No Scheduled Days";
                }

                let result = rrule.byDay.map(d => getWeekdayName(d.day) + "s").join(",");

                if (rrule.interval > 1) {
                    result = `Every ${rrule.interval} weeks: ${result}`;
                }
                else {
                    result = `Weekly: ${result}`;
                }

                return `${result} at ${startTimeText}`;
            }
            else if (rrule.frequency === "MONTHLY") {
                if (rrule.byMonthDay.length > 0) {
                    let result = `Day ${rrule.byMonthDay[0]} of every `;

                    if (rrule.interval > 1) {
                        result += `${rrule.interval} months`;
                    }
                    else {
                        result += "month";
                    }

                    return `${result} at ${startTimeText}`;
                }
                else if (rrule.byDay.length > 0) {
                    const byDay = rrule.byDay[0];
                    const offsetNames = nthNamesAbbreviated.filter(n => rrule.byDay.some(d => d.value == n[0])).map(n => n[1]);
                    let result = "";

                    if (offsetNames.length > 0) {
                        let nameText: string;

                        if (offsetNames.length > 2) {
                            nameText = `${offsetNames.slice(0, offsetNames.length - 1).join(", ")} and ${offsetNames[offsetNames.length - 1]}`;
                        }
                        else {
                            nameText = offsetNames.join(" and ");
                        }
                        result = `The ${nameText} ${getWeekdayName(byDay.day)} of every month`;
                    }
                    else {
                        return "";
                    }

                    return `${result} at ${startTimeText}`;
                }
                else {
                    return "";
                }
            }
            else {
                return "";
            }
        }
        else {
            const dates: RockDateTime[] = [this.startDateTime, ...this.recurrenceDates];

            if (dates.length === 1) {
                return `Once at ${this.startDateTime.toASPString("g")}`;
            }
            else if (!html || dates.length > 99) {
                const firstDate = dates[0];
                const lastDate = dates[dates.length - 1];

                if (firstDate && lastDate) {
                    return `Multiple dates between ${firstDate.toASPString("g")} and ${lastDate.toASPString("g")}`;
                }
                else {
                    return "";
                }
            }
            else if (dates.length > 1) {
                let listHtml = `<ul class="list-unstyled">`;

                for (const date of dates) {
                    listHtml += `<li>${date.toASPString("g")}</li>`;
                }

                listHtml += "</li>";

                return listHtml;
            }
            else {
                return "No Schedule";
            }
        }
    }

    // #endregion
}

/**
 * A recurring schedule allows schedules to be built and customized from the iCal
 * format used in ics files.
 */
export class Calendar {
    // #region Properties

    /**
     * The events that exist for this calendar.
     */
    public events: Event[] = [];

    // #endregion

    // #region Constructors

    /**
     * Creates a new Calendar instance.
     *
     * @param icsContent The content from an ICS file to initialize the calendar with.
     *
     * @returns A new Calendar instance.
     */
    public constructor(icsContent: string | undefined = undefined) {
        if (icsContent === undefined) {
            return;
        }

        const feeder = new LineFeeder(icsContent);

        this.parse(feeder);
    }

    // #endregion

    // #region Functions

    /**
     * Builds the calendar into a string that conforms to ICS format.
     *
     * @returns An ICS formatted string that represents the calendar data.
     */
    public build(): string | null {
        const lines: string[] = [];

        lines.push("BEGIN:VCALENDAR");
        lines.push("PRODID:-//github.com/SparkDevNetwork/Rock//NONSGML Rock//EN");
        lines.push("VERSION:2.0");

        for (const event of this.events) {
            lines.push(...event.buildLines());
        }

        lines.push("END:VCALENDAR");

        return denormalizeLineLength(lines).join("\r\n");
    }

    /**
     * Parses the ICS data from a line feeder and constructs the calendar
     * from that data.
     *
     * @param feeder The feeder that provides the individual lines.
     */
    private parse(feeder: LineFeeder): void {
        let line: string | null;

        // Parse the line data.
        while ((line = feeder.peek()) !== null) {
            if (line === "BEGIN:VEVENT") {
                const event = new Event(feeder);

                this.events.push(event);
            }
            else {
                feeder.pop();
            }
        }
    }

    // #endregion
}
