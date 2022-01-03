import assert = require("assert");
import { RockDateTime } from "../../Framework/Util/rockDateTime";

describe("formatAspDate Suite", () => {
    it("Short Date", () => {
        const date = RockDateTime.fromParts(2021, 4, 7);

        assert.strictEqual(date?.toASPString("M-dd-yyyy"), "4-07-2021");
    });

    it("Long Date", () => {
        const date = RockDateTime.fromParts(2021, 4, 7);

        assert.strictEqual(date?.toASPString("MMMM d, yyyy"), "April 7, 2021");
    });

    /*
     * Seconds: s
     */
    it("Seconds under 10 produces single digit with 's'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:30:09")?.toASPString("%s"), "9");
    });

    it("Seconds over 10 produces double digit with 's'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:30:14")?.toASPString("%s"), "14");
    });

    /*
     * Seconds: ss
     */
    it("Seconds under 10 produces double digits with 'ss'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:30:09")?.toASPString("ss"), "09");
    });

    it("Seconds over 10 produces double digits with 'ss'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:30:14")?.toASPString("ss"), "14");
    });

    /*
     * Minutes: m
     */
    it("Minutes under 10 produces single digit with 'm'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:09:00")?.toASPString("%m"), "9");
    });

    it("Minutes over 10 produces double digit with 'm'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:14:00")?.toASPString("%m"), "14");
    });

    /*
     * Minutes: mm
     */
    it("Minutes under 10 produces double digits with 'mm'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:09:00")?.toASPString("mm"), "09");
    });

    it("Minutes over 10 produces double digits with 'mm'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:14:00")?.toASPString("mm"), "14");
    });

    /*
     * Hours: h
     */
    it("Hours under 10 produces single digit with 'h'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T09:00:00")?.toASPString("%h"), "9");
    });

    it("Hours over 10 produces double digit with 'h'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T11:00:00")?.toASPString("%h"), "11");
    });

    it("Hours over 12 and under 20 produces single digit with 'h'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T14:00:00")?.toASPString("%h"), "2");
    });

    it("Hours over 20 produces double digit with 'h'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T22:00:00")?.toASPString("%h"), "10");
    });

    /*
     * Hours: hh
     */
    it("Hours under 10 produces double digits with 'hh'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T09:00:00")?.toASPString("hh"), "09");
    });

    it("Hours over 10 produces double digits with 'hh'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T11:00:00")?.toASPString("hh"), "11");
    });

    it("Hours over 12 and under 20 produces double digits with 'hh'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T14:00:00")?.toASPString("hh"), "02");
    });

    it("Hours over 20 produces double digit with 'hh'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T22:00:00")?.toASPString("hh"), "10");
    });

    /*
     * Hours: H
     */
    it("Hours under 10 produces single digit with 'H'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T09:00:00")?.toASPString("%H"), "9");
    });

    it("Hours over 10 produces double digit with 'H'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T11:00:00")?.toASPString("%H"), "11");
    });

    it("Hours over 12 produces double digit with 'H'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T14:00:00")?.toASPString("%H"), "14");
    });

    /*
     * Hours: HH
     */
    it("Hours under 10 produces double digit with 'HH'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T09:00:00")?.toASPString("HH"), "09");
    });

    it("Hours over 10 produces double digit with 'HH'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T11:00:00")?.toASPString("HH"), "11");
    });

    it("Hours over 12 produces double digit with 'HH'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T14:00:00")?.toASPString("HH"), "14");
    });

    /*
     * Milliseconds
     */
    it("Millisecond Values", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("%f"), "6");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.05")?.toASPString("%f"), "0");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("ff"), "61");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.005")?.toASPString("ff"), "00");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("fff"), "617");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.0005")?.toASPString("fff"), "000");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175000")?.toASPString("ffff"), "6170");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.00005")?.toASPString("ffff"), "0000");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175400")?.toASPString("fffff"), "61700");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.000005")?.toASPString("fffff"), "00000");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175420")?.toASPString("ffffff"), "617000");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.0000005")?.toASPString("ffffff"), "000000");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175425")?.toASPString("fffffff"), "6170000");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.0001150")?.toASPString("fffffff"), "0000000");
    });

    it("Millisecond Optional Values", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("%F"), "6");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.05")?.toASPString("%F"), "");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("FF"), "61");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.005")?.toASPString("FF"), "");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("FFF"), "617");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.0005")?.toASPString("FFF"), "");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175000")?.toASPString("FFFF"), "6170");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.00005")?.toASPString("FFFF"), "");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175400")?.toASPString("FFFFF"), "61700");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.000005")?.toASPString("FFFFF"), "");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175420")?.toASPString("FFFFFF"), "617000");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.0000005")?.toASPString("FFFFFF"), "");

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6175425")?.toASPString("FFFFFFF"), "6170000");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.0001150")?.toASPString("FFFFFFF"), "");
    });

    /*
     * Days: d
     */
    it("Days under 10 produces single digit with 'd'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-04T09:00:00")?.toASPString("%d"), "4");
    });

    it("Days over 10 produces double digit with 'd'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-14T09:00:00")?.toASPString("%d"), "14");
    });

    /*
     * Days: dd
     */
    it("Days under 10 produces double digit with 'dd'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-04T09:00:00")?.toASPString("dd"), "04");
    });

    it("Days over 10 produces double digit with 'dd'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-14T09:00:00")?.toASPString("dd"), "14");
    });

    /*
     * Days: ddd
     */
    it("Days produces abbreviated name with 'ddd'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-12T09:00:00")?.toASPString("ddd"), "Fri");
    });

    /*
     * Days: dddd
     */
    it("Days produces full name with 'dddd'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-12T09:00:00")?.toASPString("dddd"), "Friday");
    });

    /*
     * Months: M
     */
    it("Months under 10 produces single digit with 'M'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-04T09:00:00")?.toASPString("%M"), "6");
    });

    it("Months over 10 produces double digit with 'M'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-11-14T09:00:00")?.toASPString("%M"), "11");
    });

    /*
     * Months: MM
     */
    it("Months under 10 produces double digit with 'MM'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-04T09:00:00")?.toASPString("MM"), "06");
    });

    it("Months over 10 produces double digit with 'MM'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-11-14T09:00:00")?.toASPString("MM"), "11");
    });

    /*
     * Months: MMM
     */
    it("Months produces abbreviated name with 'MMM'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-12T09:00:00")?.toASPString("MMM"), "Jun");
    });

    /*
     * Months: MMMM
     */
    it("Months produces full name with 'MMMM'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-12T09:00:00")?.toASPString("MMMM"), "June");
    });

    /*
     * Years: y
     */
    it("Years produces expected values with 'y'", () => {
        assert.strictEqual(RockDateTime.parseISO("0001-01-01T00:00:00")?.toASPString("%y"), "1");
        assert.strictEqual(RockDateTime.parseISO("0900-01-01T00:00:00")?.toASPString("%y"), "0");
        assert.strictEqual(RockDateTime.parseISO("1900-01-01T00:00:00")?.toASPString("%y"), "0");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30")?.toASPString("%y"), "9");
        assert.strictEqual(RockDateTime.parseISO("2019-06-15T13:45:30")?.toASPString("%y"), "19");
    });

    /*
     * Years: yy
     */
    it("Years produces expected values with 'yy'", () => {
        assert.strictEqual(RockDateTime.parseISO("0001-01-01T00:00:00")?.toASPString("yy"), "01");
        assert.strictEqual(RockDateTime.parseISO("0900-01-01T00:00:00")?.toASPString("yy"), "00");
        assert.strictEqual(RockDateTime.parseISO("1900-01-01T00:00:00")?.toASPString("yy"), "00");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30")?.toASPString("yy"), "09");
        assert.strictEqual(RockDateTime.parseISO("2019-06-15T13:45:30")?.toASPString("yy"), "19");
    });

    /*
     * Years: yyy
     */
    it("Years produces expected values with 'yyy'", () => {
        assert.strictEqual(RockDateTime.parseISO("0001-01-01T00:00:00")?.toASPString("yyy"), "001");
        assert.strictEqual(RockDateTime.parseISO("0900-01-01T00:00:00")?.toASPString("yyy"), "900");
        assert.strictEqual(RockDateTime.parseISO("1900-01-01T00:00:00")?.toASPString("yyy"), "1900");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30")?.toASPString("yyy"), "2009");
        assert.strictEqual(RockDateTime.parseISO("2019-06-15T13:45:30")?.toASPString("yyy"), "2019");
    });

    /*
     * Years: yyyy
     */
    it("Years produces expected values with 'yyyy'", () => {
        assert.strictEqual(RockDateTime.parseISO("0001-01-01T00:00:00")?.toASPString("yyyy"), "0001");
        assert.strictEqual(RockDateTime.parseISO("0900-01-01T00:00:00")?.toASPString("yyyy"), "0900");
        assert.strictEqual(RockDateTime.parseISO("1900-01-01T00:00:00")?.toASPString("yyyy"), "1900");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30")?.toASPString("yyyy"), "2009");
        assert.strictEqual(RockDateTime.parseISO("2019-06-15T13:45:30")?.toASPString("yyyy"), "2019");
    });

    /*
     * Years: yyyyy
     */
    it("Years produces expected values with 'yyyyy'", () => {
        assert.strictEqual(RockDateTime.parseISO("0001-01-01T00:00:00")?.toASPString("yyyyy"), "00001");
        assert.strictEqual(RockDateTime.parseISO("0900-01-01T00:00:00")?.toASPString("yyyyy"), "00900");
        assert.strictEqual(RockDateTime.parseISO("1900-01-01T00:00:00")?.toASPString("yyyyy"), "01900");
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30")?.toASPString("yyyyy"), "02009");
        assert.strictEqual(RockDateTime.parseISO("2019-06-15T13:45:30")?.toASPString("yyyyy"), "02019");
    });

    /*
     * Period: g
     */
    it("Year > 0 produces 'A.D.' with 'g'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("%g"), "A.D.");
    });

    it("Year < 0 produces 'B.C.' with 'g'", () => {
        assert.strictEqual(RockDateTime.fromParts(-4, 6, 15)?.toASPString("%g"), "B.C.");
    });

    /*
     * Period: gg
     */
    it("Year > 0 produces 'A.D.' with 'gg'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("gg"), "A.D.");
    });

    it("Year < 0 produces 'B.C.' with 'gg'", () => {
        assert.strictEqual(RockDateTime.fromParts(-4, 6, 15)?.toASPString("gg"), "B.C.");
    });

    /*
     * Time Zone: K
     */
    it("TimeZone produces local time zone with 'K'", () => {
        const offset = new Date().getTimezoneOffset();
        const offsetHour = Math.floor(offset / 60);
        const offsetMinute = offset % 60;
        const expected = `${offset > 0 ? "-" : "+"}${offsetHour < 10 ? "0" : ""}${offsetHour}:${offsetMinute < 10 ? "0" : ""}${offsetMinute}`;

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("%K"), expected);
    });

    /*
     * Time Zone: z
     */
    it("TimeZone produces local time zone with 'z'", () => {
        const offset = new Date().getTimezoneOffset();
        const offsetHour = Math.floor(offset / 60);
        const expected = `${offset > 0 ? "-" : "+"}${offsetHour}`;

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("%z"), expected);
    });

    /*
     * Time Zone: zz
     */
    it("TimeZone produces local time zone with 'zz'", () => {
        const offset = new Date().getTimezoneOffset();
        const offsetHour = Math.floor(offset / 60);
        const expected = `${offset > 0 ? "-" : "+"}${offsetHour < 10 ? "0" : ""}${offsetHour}`;

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("zz"), expected);
    });

    /*
     * Time Zone: zzz
     */
    it("TimeZone produces local time zone with 'zzz'", () => {
        const offset = new Date().getTimezoneOffset();
        const offsetHour = Math.floor(offset / 60);
        const offsetMinute = offset % 60;
        const expected = `${offset > 0 ? "-" : "+"}${offsetHour < 10 ? "0" : ""}${offsetHour}:${offsetMinute < 10 ? "0" : ""}${offsetMinute}`;

        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("zzz"), expected);
    });

    /*
     * Date Separator: /
     */
    it("Date Separator produces US slash with '/'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("M/d/yy"), "6/15/09");
    });

    /*
     * Time Separator: :
     */
    it("Time Separator produces US colon with ':'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("h:m"), "1:45");
    });

    /*
     * Escape sequence: \
     */
    it("Escape sequence produced literal character with '\\'", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("h:\\m"), "1:m");
    });

    /*
     * Escape sequence: '
     */
    it("Escape sequence produced literal character with single-quote", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString("h'h:m'"), "1h:m");
    });

    /*
     * Escape sequence: "
     */
    it("Escape sequence produced literal character with double-quote", () => {
        assert.strictEqual(RockDateTime.parseISO("2009-06-15T13:45:30.6170000")?.toASPString(`h"h:m"`), "1h:m");
    });

    /*
     * Standard date format: F
     */
    it("Standard date format 'F' produces expected value", () => {
        assert.strictEqual(RockDateTime.parseISO("2008-04-10T06:30:00")?.toASPString("F"), "Thursday, April 10, 2008 6:30:00 AM");
    });
});
