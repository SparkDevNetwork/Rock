import { List } from "./linq";
import { padLeft, padRight } from "./stringUtils";
import { RockDateTime } from "./rockDateTime";

/**
 * Returns a blank string if the string value is 0.
 *
 * @param value The value to check and return.
 * @returns The value passed in or an empty string if it equates to zero.
 */
function blankIfZero(value: string): string {
    return parseInt(value) === 0 ? "" : value;
}

/**
 * Gets the 12 hour value of the given 24-hour number.
 *
 * @param hour The hour in a 24-hour format.
 * @returns The hour in a 12-hour format.
 */
function get12HourValue(hour: number): number {
    if (hour == 0) {
        return 12;
    }
    else if (hour < 13) {
        return hour;
    }
    else {
        return hour - 12;
    }
}
type DateFormatterCommand = (date: RockDateTime) => string;

const englishDayNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
const englishMonthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

const dateFormatters: Record<string, DateFormatterCommand> = {
    "yyyyy": date => padLeft(date.year.toString(), 5, "0"),
    "yyyy": date => padLeft(date.year.toString(), 4, "0"),
    "yyy": date => padLeft(date.year.toString(), 3, "0"),
    "yy": date => padLeft((date.year % 100).toString(), 2, "0"),
    "y": date => (date.year % 100).toString(),

    "MMMM": date => englishMonthNames[date.month - 1],
    "MMM": date => englishMonthNames[date.month - 1].substr(0, 3),
    "MM": date => padLeft(date.month.toString(), 2, "0"),
    "M": date => date.month.toString(),

    "dddd": date => englishDayNames[date.dayOfWeek],
    "ddd": date => englishDayNames[date.dayOfWeek].substr(0, 3),
    "dd": date => padLeft(date.day.toString(), 2, "0"),
    "d": date => date.day.toString(),

    "fffffff": date => padRight((date.millisecond * 10000).toString(), 7, "0"),
    "ffffff": date => padRight((date.millisecond * 1000).toString(), 6, "0"),
    "fffff": date => padRight((date.millisecond * 100).toString(), 5, "0"),
    "ffff": date => padRight((date.millisecond * 10).toString(), 4, "0"),
    "fff": date => padRight(date.millisecond.toString(), 3, "0"),
    "ff": date => padRight(Math.floor(date.millisecond / 10).toString(), 2, "0"),
    "f": date => padRight(Math.floor(date.millisecond / 100).toString(), 1, "0"),

    "FFFFFFF": date => blankIfZero(padRight((date.millisecond * 10000).toString(), 7, "0")),
    "FFFFFF": date => blankIfZero(padRight((date.millisecond * 1000).toString(), 6, "0")),
    "FFFFF": date => blankIfZero(padRight((date.millisecond * 100).toString(), 5, "0")),
    "FFFF": date => blankIfZero(padRight((date.millisecond * 10).toString(), 4, "0")),
    "FFF": date => blankIfZero(padRight(date.millisecond.toString(), 3, "0")),
    "FF": date => blankIfZero(padRight(Math.floor(date.millisecond / 10).toString(), 2, "0")),
    "F": date => blankIfZero(padRight(Math.floor(date.millisecond / 100).toString(), 1, "0")),

    "g": date => date.year < 0 ? "B.C." : "A.D.",
    "gg": date => date.year < 0 ? "B.C." : "A.D.",

    "hh": date => padLeft(get12HourValue(date.hour).toString(), 2, "0"),
    "h": date => get12HourValue(date.hour).toString(),

    "HH": date => padLeft(date.hour.toString(), 2, "0"),
    "H": date => date.hour.toString(),

    "mm": date => padLeft(date.minute.toString(), 2, "0"),
    "m": date => date.minute.toString(),

    "ss": date => padLeft(date.second.toString(), 2, "0"),
    "s": date => date.second.toString(),

    "K": date => {
        const offset = date.offset;
        const offsetHour = Math.abs(Math.floor(offset / 60));
        const offsetMinute = Math.abs(offset % 60);
        return `${offset >= 0 ? "+" : "-"}${padLeft(offsetHour.toString(), 2, "0")}:${padLeft(offsetMinute.toString(), 2, "0")}`;
    },

    "tt": date => date.hour >= 12 ? "PM" : "AM",
    "t": date => date.hour >= 12 ? "P" : "A",

    "zzz": date => {
        const offset = date.offset;
        const offsetHour = Math.abs(Math.floor(offset / 60));
        const offsetMinute = Math.abs(offset % 60);
        return `${offset >= 0 ? "+" : "-"}${padLeft(offsetHour.toString(), 2, "0")}:${padLeft(offsetMinute.toString(), 2, "0")}`;
    },
    "zz": date => {
        const offset = date.offset;
        const offsetHour = Math.abs(Math.floor(offset / 60));
        return `${offset >= 0 ? "+" : "-"}${padLeft(offsetHour.toString(), 2, "0")}`;
    },
    "z": date => {
        const offset = date.offset;
        const offsetHour = Math.abs(Math.floor(offset / 60));
        return `${offset >= 0 ? "+" : "-"}${offsetHour}`;
    },

    ":": () => ":",
    "/": () => "/"
};

const dateFormatterKeys = new List<string>(Object.keys(dateFormatters))
    .orderByDescending(k => k.length)
    .toArray();

const standardDateFormats: Record<string, DateFormatterCommand> = {
    "d": date => formatAspDate(date, getLocalDateFormatString()),
    "D": date => formatAspDate(date, "dddd, MMMM dd, yyyy"),
    "t": date => formatAspDate(date, "h:mm tt"),
    "T": date => formatAspDate(date, "h:mm:ss tt"),
    "M": date => formatAspDate(date, "MMMM dd"),
    "m": date => formatAspDate(date, "MMMM dd"),
    "Y": date => formatAspDate(date, "yyyy MMMM"),
    "y": date => formatAspDate(date, "yyyy MMMM"),
    "f": date => `${formatAspDate(date, "D")} ${formatAspDate(date, "t")}`,
    "F": date => `${formatAspDate(date, "D")} ${formatAspDate(date, "T")}`,
    "g": date => `${formatAspDate(date, "d")} ${formatAspDate(date, "t")}`,
    "G": date => `${formatAspDate(date, "d")} ${formatAspDate(date, "T")}`,
    "o": date => formatAspDate(date, `yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz`),
    "O": date => formatAspDate(date, `yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz`),
    "r": date => formatAspDate(date, `ddd, dd MMM yyyy HH':'mm':'ss 'GMT'`),
    "R": date => formatAspDate(date, `ddd, dd MMM yyyy HH':'mm':'ss 'GMT'`),
    "s": date => formatAspDate(date, `yyyy'-'MM'-'dd'T'HH':'mm':'ss`),
    "u": date => formatAspDate(date, `yyyy'-'MM'-'dd HH':'mm':'ss'Z'`),
    "U": date => {
        return formatAspDate(date.universalDateTime, `F`);
    },
};

/**
 * Formats the Date object using custom format specifiers.
 *
 * @param date The date object to be formatted.
 * @param format The custom format string.
 * @returns A string that represents the date in the specified format.
 */
function formatAspCustomDate(date: RockDateTime, format: string): string {
    let result = "";

    for (let i = 0; i < format.length;) {
        let matchFound = false;

        for (const k of dateFormatterKeys) {
            if (format.substr(i, k.length) === k) {
                result += dateFormatters[k](date);
                matchFound = true;
                i += k.length;
                break;
            }
        }

        if (matchFound) {
            continue;
        }

        if (format[i] === "\\") {
            i++;
            if (i < format.length) {
                result += format[i++];
            }
        }
        else if (format[i] === "'") {
            i++;
            for (; i < format.length && format[i] !== "'"; i++) {
                result += format[i];
            }
            i++;
        }
        else if (format[i] === '"') {
            i++;
            for (; i < format.length && format[i] !== '"'; i++) {
                result += format[i];
            }
            i++;
        }
        else {
            result += format[i++];
        }
    }

    return result;
}

/**
 * Formats the Date object using a standard format string.
 *
 * @param date The date object to be formatted.
 * @param format The standard format specifier.
 * @returns A string that represents the date in the specified format.
 */
function formatAspStandardDate(date: RockDateTime, format: string): string {
    if (standardDateFormats[format] !== undefined) {
        return standardDateFormats[format](date);
    }

    return format;
}

/**
 * Formats the given Date object using nearly the same rules as the ASP C#
 * format methods.
 *
 * @param date The date object to be formatted.
 * @param format The format string to use.
 */
export function formatAspDate(date: RockDateTime, format: string): string {
    if (format.length === 1) {
        return formatAspStandardDate(date, format);
    }
    else if (format.length === 2 && format[0] === "%") {
        return formatAspCustomDate(date, format[1]);
    }
    else {
        return formatAspCustomDate(date, format);
    }
}

 /**
  * Gets a format string that matches the current browser locale settings.
  *
  * @returns A string that represents the date format of the client browser.
  */
function getLocalDateFormatString(): string {

    // Create an arbitrary date with recognizable numeric parts, format the date using the current locale settings
    // then replace the numeric parts with date format placeholders to get the local date format string.
    // Note that the month is specified as an index in the Date constructor, so "9" represents month "10".
    const refDate = new Date(2000,9,20);

    return refDate.toLocaleDateString(undefined, { year:"numeric", month:"2-digit", day:"2-digit"  })
    .replace("20","d")
    .replace("10","M")
    .replace("2000","yyyy");
}
