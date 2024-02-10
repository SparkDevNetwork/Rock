import { DateTime, FixedOffsetZone, Zone } from "luxon";
import { formatAspDate } from "./aspDateFormat";
import { DayOfWeek } from "@Obsidian/Enums/Controls/dayOfWeek";

/**
 * The days of the week that are used by RockDateTime.
 */
export { DayOfWeek } from "@Obsidian/Enums/Controls/dayOfWeek";

/**
 * The various date and time formats supported by the formatting methods.
 */
export const DateTimeFormat: Record<string, Intl.DateTimeFormatOptions> = {
    DateFull: {
        year: "numeric",
        month: "long",
        day: "numeric"
    },

    DateMedium: {
        year: "numeric",
        month: "short",
        day: "numeric"
    },

    DateShort: {
        year: "numeric",
        month: "numeric",
        day: "numeric"
    },

    TimeShort: {
        hour: "numeric",
        minute: "numeric",
    },

    TimeWithSeconds: {
        hour: "numeric",
        minute: "numeric",
        second: "numeric"
    },

    DateTimeShort: {
        year: "numeric",
        month: "numeric",
        day: "numeric",
        hour: "numeric",
        minute: "numeric"
    },

    DateTimeShortWithSeconds: {
        year: "numeric",
        month: "numeric",
        day: "numeric",
        hour: "numeric",
        minute: "numeric",
        second: "numeric"
    },

    DateTimeMedium: {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "numeric",
        minute: "numeric"
    },

    DateTimeMediumWithSeconds: {
        year: "numeric",
        month: "short",
        day: "numeric",
        hour: "numeric",
        minute: "numeric",
        second: "numeric"
    },

    DateTimeFull: {
        year: "numeric",
        month: "long",
        day: "numeric",
        hour: "numeric",
        minute: "numeric"
    },

    DateTimeFullWithSeconds: {
        year: "numeric",
        month: "long",
        day: "numeric",
        hour: "numeric",
        minute: "numeric",
        second: "numeric"
    }
};

/**
 * A date and time object that handles time zones and formatting. This class is
 * immutable and cannot be modified. All modifications are performed by returning
 * a new RockDateTime instance.
 */
export class RockDateTime {
    /** The internal DateTime object that holds our date information. */
    private dateTime: DateTime;

    // #region Constructors

    /**
     * Creates a new instance of RockDateTime.
     *
     * @param dateTime The Luxon DateTime object that is used to track the internal state.
     */
    private constructor(dateTime: DateTime) {
        this.dateTime = dateTime;
    }

    /**
     * Creates a new instance of RockDateTime from the given date and time parts.
     *
     * @param year The year of the new date.
     * @param month The month of the new date (1-12).
     * @param day The day of month of the new date.
     * @param hour The hour of the day.
     * @param minute The minute of the hour.
     * @param second The second of the minute.
     * @param millisecond The millisecond of the second.
     * @param zone The time zone offset to construct the date in.
     *
     * @returns A RockDateTime instance or null if the requested date was not valid.
     */
    public static fromParts(year: number, month: number, day: number, hour?: number, minute?: number, second?: number, millisecond?: number, zone?: number | string): RockDateTime | null {
        let luxonZone: Zone | string | undefined;

        if (zone !== undefined) {
            if (typeof zone === "number") {
                luxonZone = FixedOffsetZone.instance(zone);
            }
            else {
                luxonZone = zone;
            }
        }

        const dateTime = DateTime.fromObject({
            year,
            month,
            day,
            hour,
            minute,
            second,
            millisecond
        }, {
            zone: luxonZone
        });

        if (!dateTime.isValid) {
            return null;
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new instance of RockDateTime that represents the time specified
     * as the Javascript milliseconds value. The time zone is set to the browser
     * time zone.
     *
     * @param milliseconds The time in milliseconds since the epoch.
     *
     * @returns A new RockDateTime instance or null if the specified date was not valid.
     */
    public static fromMilliseconds(milliseconds: number): RockDateTime | null {
        const dateTime = DateTime.fromMillis(milliseconds);

        if (!dateTime.isValid) {
            return null;
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Construct a new RockDateTime instance from a Javascript Date object.
     *
     * @param date The Javascript date object that contains the date information.
     *
     * @returns A RockDateTime instance or null if the date was not valid.
     */
    public static fromJSDate(date: Date): RockDateTime | null {
        const dateTime = DateTime.fromJSDate(date);

        if (!dateTime.isValid) {
            return null;
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Constructs a new RockDateTime instance by parsing the given string from
     * ISO 8601 format.
     *
     * @param dateString The string that contains the ISO 8601 formatted text.
     *
     * @returns A new RockDateTime instance or null if the date was not valid.
     */
    public static parseISO(dateString: string): RockDateTime | null {
        const dateTime = DateTime.fromISO(dateString, { setZone: true });

        if (!dateTime.isValid) {
            return null;
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Constructs a new RockDateTime instance by parsing the given string from
     * RFC 1123 format. This is common in HTTP headers.
     *
     * @param dateString The string that contains the RFC 1123 formatted text.
     *
     * @returns A new RockDateTime instance or null if the date was not valid.
     */
    public static parseHTTP(dateString: string): RockDateTime | null {
        const dateTime = DateTime.fromHTTP(dateString, { setZone: true });

        if (!dateTime.isValid) {
            return null;
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the current date and time.
     *
     * @returns A RockDateTime instance.
     */
    public static now(): RockDateTime {
        return new RockDateTime(DateTime.now());
    }

    /**
     * Creates a new RockDateTime instance that represents the current time in UTC.
     *
     * @returns A new RockDateTime instance in the UTC time zone.
     */
    public static utcNow(): RockDateTime {
        return new RockDateTime(DateTime.now().toUTC());
    }

    // #endregion

    // #region Properties

    /**
     * The Date portion of this RockDateTime instance. All time properties of
     * the returned instance will be set to 0.
     */
    public get date(): RockDateTime {
        const date = RockDateTime.fromParts(this.year, this.month, this.day, 0, 0, 0, 0, this.offset);

        if (date === null) {
            throw "Could not convert to date instance.";
        }

        return date;
    }

    /**
     * The day of the month represented by this instance.
     */
    public get day(): number {
        return this.dateTime.day;
    }

    /**
     * The day of the week represented by this instance.
     */
    public get dayOfWeek(): DayOfWeek {
        switch (this.dateTime.weekday) {
            case 1:
                return DayOfWeek.Monday;

            case 2:
                return DayOfWeek.Tuesday;

            case 3:
                return DayOfWeek.Wednesday;

            case 4:
                return DayOfWeek.Thursday;

            case 5:
                return DayOfWeek.Friday;

            case 6:
                return DayOfWeek.Saturday;

            case 7:
                return DayOfWeek.Sunday;
        }

        throw "Could not determine day of week.";
    }

    /**
     * The day of the year represented by this instance.
     */
    public get dayOfYear(): number {
        return this.dateTime.ordinal;
    }

    /**
     * The hour of the day represented by this instance.
     */
    public get hour(): number {
        return this.dateTime.hour;
    }

    /**
     * The millisecond of the second represented by this instance.
     */
    public get millisecond(): number {
        return this.dateTime.millisecond;
    }

    /**
     * The minute of the hour represented by this instance.
     */
    public get minute(): number {
        return this.dateTime.minute;
    }

    /**
     * The month of the year represented by this instance (1-12).
     */
    public get month(): number {
        return this.dateTime.month;
    }

    /**
     * The offset from UTC represented by this instance. If the timezone of this
     * instance is UTC-7 then the value returned is -420.
     */
    public get offset(): number {
        return this.dateTime.offset;
    }

    /**
     * The second of the minute represented by this instance.
     */
    public get second(): number {
        return this.dateTime.second;
    }

    /**
     * The year represented by this instance.
     */
    public get year(): number {
        return this.dateTime.year;
    }

    /**
     * Creates a new RockDateTime instance that represents the same point in
     * time represented in the local browser time zone.
     */
    public get localDateTime(): RockDateTime {
        return new RockDateTime(this.dateTime.toLocal());
    }

    /**
     * Creates a new RockDateTime instance that represents the same point in
     * time represented in the organization time zone.
     */
    public get organizationDateTime(): RockDateTime {
        throw "Not Implemented";
    }

    /**
     * Creates a new RockDateTime instance that represents the same point in
     * time represented in UTC.
     */
    public get universalDateTime(): RockDateTime {
        return new RockDateTime(this.dateTime.toUTC());
    }

    // #endregion

    // #region Methods

    /**
     * Creates a new RockDateTime instance that represents the date and time
     * after adding the number of days to this instance.
     *
     * @param days The number of days to add.
     *
     * @returns A new instance of RockDateTime that represents the new date and time.
     */
    public addDays(days: number): RockDateTime {
        const dateTime = this.dateTime.plus({ days: days });

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the end of the month
     * for this instance.
     */
    public endOfMonth(): RockDateTime {
        debugger;
        const dateTime = this.dateTime.endOf("month");

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the date and time
     * after adding the number of hours to this instance.
     *
     * @param days The number of hours to add.
     *
     * @returns A new instance of RockDateTime that represents the new date and time.
     */
    public addHours(hours: number): RockDateTime {
        const dateTime = this.dateTime.plus({ hours: hours });

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the date and time
     * after adding the number of milliseconds to this instance.
     *
     * @param days The number of milliseconds to add.
     *
     * @returns A new instance of RockDateTime that represents the new date and time.
     */
    public addMilliseconds(milliseconds: number): RockDateTime {
        const dateTime = this.dateTime.plus({ milliseconds: milliseconds });

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the date and time
     * after adding the number of minutes to this instance.
     *
     * @param days The number of minutes to add.
     *
     * @returns A new instance of RockDateTime that represents the new date and time.
     */
    public addMinutes(minutes: number): RockDateTime {
        const dateTime = this.dateTime.plus({ minutes: minutes });

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the date and time
     * after adding the number of months to this instance.
     *
     * @param days The number of months to add.
     *
     * @returns A new instance of RockDateTime that represents the new date and time.
     */
    public addMonths(months: number): RockDateTime {
        const dateTime = this.dateTime.plus({ months: months });

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the date and time
     * after adding the number of seconds to this instance.
     *
     * @param days The number of seconds to add.
     *
     * @returns A new instance of RockDateTime that represents the new date and time.
     */
    public addSeconds(seconds: number): RockDateTime {
        const dateTime = this.dateTime.plus({ seconds: seconds });

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Creates a new RockDateTime instance that represents the date and time
     * after adding the number of years to this instance.
     *
     * @param days The number of years to add.
     *
     * @returns A new instance of RockDateTime that represents the new date and time.
     */
    public addYears(years: number): RockDateTime {
        const dateTime = this.dateTime.plus({ years: years });

        if (!dateTime.isValid) {
            throw "Operation produced an invalid date.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Converts the date time representation into the number of milliseconds
     * that have elapsed since the epoch (1970-01-01T00:00:00Z).
     *
     * @returns The number of milliseconds since the epoch.
     */
    public toMilliseconds(): number {
        return this.dateTime.toMillis();
    }

    /**
     * Creates a new instance of RockDateTime that represents the same point
     * in time as represented by the specified time zone offset.
     *
     * @param zone The time zone offset as a number or string such as "UTC+4".
     *
     * @returns A new RockDateTime instance that represents the specified time zone.
     */
    public toOffset(zone: number | string): RockDateTime {
        let dateTime: DateTime;

        if (typeof zone === "number") {
            dateTime = this.dateTime.setZone(FixedOffsetZone.instance(zone));
        }
        else {
            dateTime = this.dateTime.setZone(zone);
        }

        if (!dateTime.isValid) {
            throw "Invalid time zone specified.";
        }

        return new RockDateTime(dateTime);
    }

    /**
     * Formats this instance according to C# formatting rules.
     *
     * @param format The string that specifies the format to use.
     *
     * @returns A string representing this instance in the given format.
     */
    public toASPString(format: string): string {
        return formatAspDate(this, format);
    }

    /**
     * Creates a string representation of this instance in ISO8601 format.
     *
     * @returns An ISO8601 formatted string.
     */
    public toISOString(): string {
        return this.dateTime.toISO();
    }

    /**
     * Formats this instance using standard locale formatting rules to display
     * a date and time in the browsers specified locale.
     *
     * @param format The format to use when generating the string.
     *
     * @returns A string that represents the date and time in then specified format.
     */
    public toLocaleString(format: Intl.DateTimeFormatOptions): string {
        return this.dateTime.toLocaleString(format);
    }

    /**
     * Transforms the date into a human friendly elapsed time string.
     *
     * @example
     * // Returns "21yrs"
     * RockDateTime.fromParts(2000, 3, 4).toElapsedString();
     *
     * @returns A string that represents the amount of time that has elapsed.
     */
    public toElapsedString(): string {
        const now = RockDateTime.now();
        const msPerHour = 1000 * 60 * 60;
        const hoursPerDay = 24;
        const daysPerMonth = 30.4167;
        const daysPerYear = 365.25;

        const totalMs = Math.abs(now.toMilliseconds() - this.toMilliseconds());
        const totalHours = totalMs / msPerHour;
        const totalDays = totalHours / hoursPerDay;

        if (totalDays < 2) {
            return "1day";
        }

        if (totalDays < 31) {
            return `${Math.floor(totalDays)}days`;
        }

        const totalMonths = totalDays / daysPerMonth;

        if (totalMonths <= 1) {
            return "1mon";
        }

        if (totalMonths <= 18) {
            return `${Math.round(totalMonths)}mon`;
        }

        const totalYears = totalDays / daysPerYear;

        if (totalYears <= 1) {
            return "1yr";
        }

        return `${Math.round(totalYears)}yrs`;
    }

    /**
     * Formats this instance as a string that can be used in HTTP headers and
     * cookies.
     *
     * @returns A new string that conforms to RFC 1123
     */
    public toHTTPString(): string {
        return this.dateTime.toHTTP();
    }

    /**
     * Get the value of the date and time in a format that can be used in
     * comparisons.
     *
     * @returns A number that represents the date and time.
     */
    public valueOf(): number {
        return this.dateTime.valueOf();
    }

    /**
     * Creates a standard string representation of the date and time.
     *
     * @returns A string representation of the date and time.
     */
    public toString(): string {
        return this.toLocaleString(DateTimeFormat.DateTimeFull);
    }

    /**
     * Checks if this instance is equal to another RockDateTime instance. This
     * will return true if the two instances represent the same point in time,
     * even if they have been associated with different time zones. In other
     * words "2021-09-08 12:00:00 Z" == "2021-09-08 14:00:00 UTC+2".
     *
     * @param otherDateTime The other RockDateTime to be compared against.
     *
     * @returns True if the two instances represent the same point in time.
     */
    public isEqualTo(otherDateTime: RockDateTime): boolean {
        return this.dateTime.toMillis() === otherDateTime.dateTime.toMillis();
    }

    /**
     * Checks if this instance is later than another RockDateTime instance.
     *
     * @param otherDateTime The other RockDateTime to be compared against.
     *
     * @returns True if this instance represents a point in time that occurred after another point in time, regardless of time zone.
     */
    public isLaterThan(otherDateTime: RockDateTime): boolean {
        return this.dateTime.toMillis() > otherDateTime.dateTime.toMillis();
    }

    /**
     * Checks if this instance is earlier than another RockDateTime instance.
     *
     * @param otherDateTime The other RockDateTime to be compared against.
     *
     * @returns True if this instance represents a point in time that occurred before another point in time, regardless of time zone.
     */
    public isEarlierThan(otherDateTime: RockDateTime): boolean {
        return this.dateTime.toMillis() < otherDateTime.dateTime.toMillis();
    }

    /**
     * Calculates the elapsed time between this date and the reference date and
     * returns that difference in a human friendly way.
     *
     * @param otherDateTime The reference date and time. If not specified then 'now' is used.
     *
     * @returns A string that represents the elapsed time.
     */
    public humanizeElapsed(otherDateTime?: RockDateTime): string {
        otherDateTime = otherDateTime ?? RockDateTime.now();

        const totalSeconds = Math.floor((otherDateTime.dateTime.toMillis() - this.dateTime.toMillis()) / 1000);

        if (totalSeconds <= 1) {
            return "right now";
        }
        else if (totalSeconds < 60) { // 1 minute
            return `${totalSeconds} seconds ago`;
        }
        else if (totalSeconds < 3600) { // 1 hour
            return `${Math.floor(totalSeconds / 60)} minutes ago`;
        }
        else if (totalSeconds < 86400) { // 1 day
            return `${Math.floor(totalSeconds / 3600)} hours ago`;
        }
        else if (totalSeconds < 31536000) { // 1 year
            return `${Math.floor(totalSeconds / 86400)} days ago`;
        }
        else {
            return `${Math.floor(totalSeconds / 31536000)} years ago`;
        }
    }

    // #endregion
}
