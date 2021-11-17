System.register(["luxon", "./aspDateFormat"], function (exports_1, context_1) {
    "use strict";
    var luxon_1, aspDateFormat_1, DateTimeFormat, RockDateTime;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (luxon_1_1) {
                luxon_1 = luxon_1_1;
            },
            function (aspDateFormat_1_1) {
                aspDateFormat_1 = aspDateFormat_1_1;
            }
        ],
        execute: function () {
            exports_1("DateTimeFormat", DateTimeFormat = {
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
            });
            RockDateTime = class RockDateTime {
                constructor(dateTime) {
                    this.dateTime = dateTime;
                }
                static fromParts(year, month, day, hour, minute, second, millisecond, zone) {
                    let luxonZone;
                    if (zone !== undefined) {
                        if (typeof zone === "number") {
                            luxonZone = luxon_1.FixedOffsetZone.instance(zone);
                        }
                        else {
                            luxonZone = zone;
                        }
                    }
                    const dateTime = luxon_1.DateTime.fromObject({
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
                static fromMilliseconds(milliseconds) {
                    const dateTime = luxon_1.DateTime.fromMillis(milliseconds);
                    if (!dateTime.isValid) {
                        return null;
                    }
                    return new RockDateTime(dateTime);
                }
                static fromJSDate(date) {
                    const dateTime = luxon_1.DateTime.fromJSDate(date);
                    if (!dateTime.isValid) {
                        return null;
                    }
                    return new RockDateTime(dateTime);
                }
                static parseISO(dateString) {
                    const dateTime = luxon_1.DateTime.fromISO(dateString, { setZone: true });
                    if (!dateTime.isValid) {
                        return null;
                    }
                    return new RockDateTime(dateTime);
                }
                static parseHTTP(dateString) {
                    const dateTime = luxon_1.DateTime.fromHTTP(dateString, { setZone: true });
                    if (!dateTime.isValid) {
                        return null;
                    }
                    return new RockDateTime(dateTime);
                }
                static now() {
                    return new RockDateTime(luxon_1.DateTime.now());
                }
                static utcNow() {
                    return new RockDateTime(luxon_1.DateTime.now().toUTC());
                }
                get date() {
                    const date = RockDateTime.fromParts(this.year, this.month, this.day, 0, 0, 0, 0, this.offset);
                    if (date === null) {
                        throw "Could not convert to date instance.";
                    }
                    return date;
                }
                get day() {
                    return this.dateTime.day;
                }
                get dayOfWeek() {
                    switch (this.dateTime.weekday) {
                        case 1:
                            return 1;
                        case 2:
                            return 2;
                        case 3:
                            return 3;
                        case 4:
                            return 4;
                        case 5:
                            return 5;
                        case 6:
                            return 6;
                        case 7:
                            return 0;
                    }
                    throw "Could not determine day of week.";
                }
                get dayOfYear() {
                    return this.dateTime.year;
                }
                get hour() {
                    return this.dateTime.hour;
                }
                get millisecond() {
                    return this.dateTime.millisecond;
                }
                get minute() {
                    return this.dateTime.minute;
                }
                get month() {
                    return this.dateTime.month;
                }
                get offset() {
                    return this.dateTime.offset;
                }
                get second() {
                    return this.dateTime.second;
                }
                get year() {
                    return this.dateTime.year;
                }
                get localDateTime() {
                    return new RockDateTime(this.dateTime.toLocal());
                }
                get organizationDateTime() {
                    throw "Not Implemented";
                }
                get universalDateTime() {
                    return new RockDateTime(this.dateTime.toUTC());
                }
                addDays(days) {
                    const dateTime = this.dateTime.plus({ days: days });
                    if (!dateTime.isValid) {
                        throw "Operation produced an invalid date.";
                    }
                    return new RockDateTime(dateTime);
                }
                addHours(hours) {
                    const dateTime = this.dateTime.plus({ hours: hours });
                    if (!dateTime.isValid) {
                        throw "Operation produced an invalid date.";
                    }
                    return new RockDateTime(dateTime);
                }
                addMilliseconds(milliseconds) {
                    const dateTime = this.dateTime.plus({ milliseconds: milliseconds });
                    if (!dateTime.isValid) {
                        throw "Operation produced an invalid date.";
                    }
                    return new RockDateTime(dateTime);
                }
                addMinutes(minutes) {
                    const dateTime = this.dateTime.plus({ minutes: minutes });
                    if (!dateTime.isValid) {
                        throw "Operation produced an invalid date.";
                    }
                    return new RockDateTime(dateTime);
                }
                addMonths(months) {
                    const dateTime = this.dateTime.plus({ months: months });
                    if (!dateTime.isValid) {
                        throw "Operation produced an invalid date.";
                    }
                    return new RockDateTime(dateTime);
                }
                addSeconds(seconds) {
                    const dateTime = this.dateTime.plus({ seconds: seconds });
                    if (!dateTime.isValid) {
                        throw "Operation produced an invalid date.";
                    }
                    return new RockDateTime(dateTime);
                }
                addYears(years) {
                    const dateTime = this.dateTime.plus({ years: years });
                    if (!dateTime.isValid) {
                        throw "Operation produced an invalid date.";
                    }
                    return new RockDateTime(dateTime);
                }
                toMilliseconds() {
                    return this.dateTime.toMillis();
                }
                toOffset(zone) {
                    let dateTime;
                    if (typeof zone === "number") {
                        dateTime = this.dateTime.setZone(luxon_1.FixedOffsetZone.instance(zone));
                    }
                    else {
                        dateTime = this.dateTime.setZone(zone);
                    }
                    if (!dateTime.isValid) {
                        throw "Invalid time zone specified.";
                    }
                    return new RockDateTime(dateTime);
                }
                toASPString(format) {
                    return aspDateFormat_1.formatAspDate(this, format);
                }
                toISOString() {
                    return this.dateTime.toISO();
                }
                toLocaleString(format) {
                    return this.dateTime.toLocaleString(format);
                }
                toElapsedString() {
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
                toHTTPString() {
                    return this.dateTime.toHTTP();
                }
                valueOf() {
                    return this.dateTime.valueOf();
                }
                toString() {
                    return this.toLocaleString(DateTimeFormat.DateTimeFull);
                }
                isEqualTo(otherDateTime) {
                    return this.dateTime.toMillis() === otherDateTime.dateTime.toMillis();
                }
            };
            exports_1("RockDateTime", RockDateTime);
        }
    };
});
//# sourceMappingURL=rockDateTime.js.map