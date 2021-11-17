System.register(["../Util/linq", "../Services/string"], function (exports_1, context_1) {
    "use strict";
    var linq_1, string_1, englishDayNames, englishMonthNames, dateFormatters, dateFormatterKeys, standardDateFormats;
    var __moduleName = context_1 && context_1.id;
    function blankIfZero(value) {
        return parseInt(value) === 0 ? "" : value;
    }
    function get12HourValue(hour) {
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
    function formatAspCustomDate(date, format) {
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
    function formatAspStandardDate(date, format) {
        if (standardDateFormats[format] !== undefined) {
            return standardDateFormats[format](date);
        }
        return format;
    }
    function formatAspDate(date, format) {
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
    exports_1("formatAspDate", formatAspDate);
    return {
        setters: [
            function (linq_1_1) {
                linq_1 = linq_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            }
        ],
        execute: function () {
            englishDayNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
            englishMonthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
            dateFormatters = {
                "yyyyy": date => string_1.padLeft(date.year.toString(), 5, "0"),
                "yyyy": date => string_1.padLeft(date.year.toString(), 4, "0"),
                "yyy": date => string_1.padLeft(date.year.toString(), 3, "0"),
                "yy": date => string_1.padLeft((date.year % 100).toString(), 2, "0"),
                "y": date => (date.year % 100).toString(),
                "MMMM": date => englishMonthNames[date.month - 1],
                "MMM": date => englishMonthNames[date.month - 1].substr(0, 3),
                "MM": date => string_1.padLeft(date.month.toString(), 2, "0"),
                "M": date => date.month.toString(),
                "dddd": date => englishDayNames[date.dayOfWeek],
                "ddd": date => englishDayNames[date.dayOfWeek].substr(0, 3),
                "dd": date => string_1.padLeft(date.day.toString(), 2, "0"),
                "d": date => date.day.toString(),
                "fffffff": date => string_1.padRight((date.millisecond * 10000).toString(), 7, "0"),
                "ffffff": date => string_1.padRight((date.millisecond * 1000).toString(), 6, "0"),
                "fffff": date => string_1.padRight((date.millisecond * 100).toString(), 5, "0"),
                "ffff": date => string_1.padRight((date.millisecond * 10).toString(), 4, "0"),
                "fff": date => string_1.padRight(date.millisecond.toString(), 3, "0"),
                "ff": date => string_1.padRight(Math.floor(date.millisecond / 10).toString(), 2, "0"),
                "f": date => string_1.padRight(Math.floor(date.millisecond / 100).toString(), 1, "0"),
                "FFFFFFF": date => blankIfZero(string_1.padRight((date.millisecond * 10000).toString(), 7, "0")),
                "FFFFFF": date => blankIfZero(string_1.padRight((date.millisecond * 1000).toString(), 6, "0")),
                "FFFFF": date => blankIfZero(string_1.padRight((date.millisecond * 100).toString(), 5, "0")),
                "FFFF": date => blankIfZero(string_1.padRight((date.millisecond * 10).toString(), 4, "0")),
                "FFF": date => blankIfZero(string_1.padRight(date.millisecond.toString(), 3, "0")),
                "FF": date => blankIfZero(string_1.padRight(Math.floor(date.millisecond / 10).toString(), 2, "0")),
                "F": date => blankIfZero(string_1.padRight(Math.floor(date.millisecond / 100).toString(), 1, "0")),
                "g": date => date.year < 0 ? "B.C." : "A.D.",
                "gg": date => date.year < 0 ? "B.C." : "A.D.",
                "hh": date => string_1.padLeft(get12HourValue(date.hour).toString(), 2, "0"),
                "h": date => get12HourValue(date.hour).toString(),
                "HH": date => string_1.padLeft(date.hour.toString(), 2, "0"),
                "H": date => date.hour.toString(),
                "mm": date => string_1.padLeft(date.minute.toString(), 2, "0"),
                "m": date => date.minute.toString(),
                "ss": date => string_1.padLeft(date.second.toString(), 2, "0"),
                "s": date => date.second.toString(),
                "K": date => {
                    const offset = date.offset;
                    const offsetHour = Math.abs(Math.floor(offset / 60));
                    const offsetMinute = Math.abs(offset % 60);
                    return `${offset >= 0 ? "+" : "-"}${string_1.padLeft(offsetHour.toString(), 2, "0")}:${string_1.padLeft(offsetMinute.toString(), 2, "0")}`;
                },
                "tt": date => date.hour >= 12 ? "PM" : "AM",
                "t": date => date.hour >= 12 ? "P" : "A",
                "zzz": date => {
                    const offset = date.offset;
                    const offsetHour = Math.abs(Math.floor(offset / 60));
                    const offsetMinute = Math.abs(offset % 60);
                    return `${offset >= 0 ? "+" : "-"}${string_1.padLeft(offsetHour.toString(), 2, "0")}:${string_1.padLeft(offsetMinute.toString(), 2, "0")}`;
                },
                "zz": date => {
                    const offset = date.offset;
                    const offsetHour = Math.abs(Math.floor(offset / 60));
                    return `${offset >= 0 ? "+" : "-"}${string_1.padLeft(offsetHour.toString(), 2, "0")}`;
                },
                "z": date => {
                    const offset = date.offset;
                    const offsetHour = Math.abs(Math.floor(offset / 60));
                    return `${offset >= 0 ? "+" : "-"}${offsetHour}`;
                },
                ":": () => ":",
                "/": () => "/"
            };
            dateFormatterKeys = new linq_1.List(Object.keys(dateFormatters))
                .orderByDescending(k => k.length)
                .toArray();
            standardDateFormats = {
                "d": date => formatAspDate(date, "M/dd/yyyy"),
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
        }
    };
});
//# sourceMappingURL=aspDateFormat.js.map