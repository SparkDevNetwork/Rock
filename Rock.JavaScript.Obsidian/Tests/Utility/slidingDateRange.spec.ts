import assert = require("assert");
import { RockDateTime } from "../../Framework/Utility/rockDateTime";
import { calculateSlidingDateRange, parseSlidingDateRangeString } from "../../Framework/Utility/slidingDateRange";

jest.mock("@Obsidian/Enums/Controls/timeUnitType", () => ({
    TimeUnitType: { Hour: 0, Day: 1, Week: 2, Month: 3, Year: 4 },
}));
jest.mock("@Obsidian/Enums/Controls/slidingDateRangeType", () => ({
    SlidingDateRangeType: { All: -1, Last: 0, Current: 1, DateRange: 2, Previous: 4, Next: 8, Upcoming: 16 }
}));

function dateEqual(actual: RockDateTime, expected: RockDateTime): void {
    assert.strictEqual(actual.year, expected.year);
    assert.strictEqual(actual.month, expected.month);
    assert.strictEqual(actual.day, expected.day);
    assert.strictEqual(actual.hour, expected.hour);
    assert.strictEqual(actual.minute, expected.minute);
    assert.strictEqual(actual.second, expected.second);
    assert.strictEqual(actual.millisecond, expected.millisecond);
}

describe("calculateSlidingDateRange Suite", () => {
    const standardReferenceDateTime = RockDateTime.fromParts(2022, 11, 2, 9, 10, 24);

    const testData = [
        // Current Hour
        ["Current||Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 9, 0, 0), RockDateTime.fromParts(2022, 11, 2, 10, 0, 0)],
        // Current Day
        ["Current||Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Current Week
        ["Current||Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 31, 0, 0, 0), RockDateTime.fromParts(2022, 11, 6, 23, 59, 59, 999)],
        // Current Month
        ["Current||Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 1, 0, 0, 0), RockDateTime.fromParts(2022, 11, 30, 23, 59, 59, 999)],
        // Current Year
        ["Current||Year||", standardReferenceDateTime, RockDateTime.fromParts(2022, 1, 1, 0, 0, 0), RockDateTime.fromParts(2022, 12, 31, 23, 59, 59, 999)],

        // Previous 1 Hours
        ["Previous|1|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 8, 0, 0), RockDateTime.fromParts(2022, 11, 2, 9, 0, 0)],
        // Previous 1 Days
        ["Previous|1|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 1, 0, 0, 0), RockDateTime.fromParts(2022, 11, 1, 23, 59, 59, 999)],
        // Previous 1 Weeks
        ["Previous|1|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 24, 0, 0, 0), RockDateTime.fromParts(2022, 10, 30, 23, 59, 59, 999)],
        // Previous 1 Months
        ["Previous|1|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 1, 0, 0, 0), RockDateTime.fromParts(2022, 10, 31, 23, 59, 59, 999)],
        // Previous 1 Years
        ["Previous|1|Year||", standardReferenceDateTime, RockDateTime.fromParts(2021, 1, 1, 0, 0, 0), RockDateTime.fromParts(2021, 12, 31, 23, 59, 59, 999)],

        // Previous 3 Hours
        ["Previous|3|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 6, 0, 0), RockDateTime.fromParts(2022, 11, 2, 9, 0, 0)],
        // Previous 3 Days
        ["Previous|3|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 30, 0, 0, 0), RockDateTime.fromParts(2022, 11, 1, 23, 59, 59, 999)],
        // Previous 3 Weeks
        ["Previous|3|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 10, 0, 0, 0), RockDateTime.fromParts(2022, 10, 30, 23, 59, 59, 999)],
        // Previous 3 Months
        ["Previous|3|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 8, 1, 0, 0, 0), RockDateTime.fromParts(2022, 10, 31, 23, 59, 59, 999)],
        // Previous 3 Years
        ["Previous|3|Year||", standardReferenceDateTime, RockDateTime.fromParts(2019, 1, 1, 0, 0, 0), RockDateTime.fromParts(2021, 12, 31, 23, 59, 59, 999)],

        // Last 1 Hours
        ["Last|1|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 9, 0, 0), RockDateTime.fromParts(2022, 11, 2, 10, 0, 0)],
        // Last 1 Days
        ["Last|1|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Last 1 Weeks
        ["Last|1|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 31, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Last 1 Months
        ["Last|1|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 1, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Last 1 Years
        ["Last|1|Year||", standardReferenceDateTime, RockDateTime.fromParts(2022, 1, 1, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],

        // Last 3 Hours
        ["Last|3|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 7, 0, 0), RockDateTime.fromParts(2022, 11, 2, 10, 0, 0)],
        // Last 3 Days
        ["Last|3|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 31, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Last 3 Weeks
        ["Last|3|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 17, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Last 3 Months
        ["Last|3|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 9, 1, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Last 3 Years
        ["Last|3|Year||", standardReferenceDateTime, RockDateTime.fromParts(2020, 1, 1, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],

        // Next 1 Hours
        ["Next|1|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 9, 0, 0), RockDateTime.fromParts(2022, 11, 2, 10, 0, 0)],
        // Next 1 Days
        ["Next|1|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 11, 2, 23, 59, 59, 999)],
        // Next 1 Weeks
        ["Next|1|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 11, 6, 23, 59, 59, 999)],
        // Next 1 Months
        ["Next|1|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 11, 30, 23, 59, 59, 999)],
        // Next 1 Years
        ["Next|1|Year||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 12, 31, 23, 59, 59, 999)],

        // Next 3 Hours
        ["Next|3|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 9, 0, 0), RockDateTime.fromParts(2022, 11, 2, 12, 0, 0)],
        // Next 3 Days
        ["Next|3|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 11, 4, 23, 59, 59, 999)],
        // Next 3 Weeks
        ["Next|3|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2022, 11, 20, 23, 59, 59, 999)],
        // Next 3 Months
        ["Next|3|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2023, 1, 31, 23, 59, 59, 999)],
        // Next 3 Years
        ["Next|3|Year||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 0, 0, 0), RockDateTime.fromParts(2024, 12, 31, 23, 59, 59, 999)],

        // Upcoming 1 Hours
        ["Upcoming|1|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 10, 0, 0), RockDateTime.fromParts(2022, 11, 2, 11, 0, 0)],
        // Upcoming 1 Days
        ["Upcoming|1|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 3, 0, 0, 0), RockDateTime.fromParts(2022, 11, 3, 23, 59, 59, 999)],
        // Upcoming 1 Weeks
        ["Upcoming|1|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 7, 0, 0, 0), RockDateTime.fromParts(2022, 11, 13, 23, 59, 59, 999)],
        // Upcoming 1 Months
        ["Upcoming|1|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 12, 1, 0, 0, 0), RockDateTime.fromParts(2022, 12, 31, 23, 59, 59, 999)],
        // Upcoming 1 Years
        ["Upcoming|1|Year||", standardReferenceDateTime, RockDateTime.fromParts(2023, 1, 1, 0, 0, 0), RockDateTime.fromParts(2023, 12, 31, 23, 59, 59, 999)],

        // Upcoming 3 Hours
        ["Upcoming|3|Hour||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 2, 10, 0, 0), RockDateTime.fromParts(2022, 11, 2, 13, 0, 0)],
        // Upcoming 3 Days
        ["Upcoming|3|Day||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 3, 0, 0, 0), RockDateTime.fromParts(2022, 11, 5, 23, 59, 59, 999)],
        // Upcoming 3 Weeks
        ["Upcoming|3|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 7, 0, 0, 0), RockDateTime.fromParts(2022, 11, 27, 23, 59, 59, 999)],
        // Upcoming 3 Months
        ["Upcoming|3|Month||", standardReferenceDateTime, RockDateTime.fromParts(2022, 12, 1, 0, 0, 0), RockDateTime.fromParts(2023, 2, 28, 23, 59, 59, 999)],
        // Upcoming 3 Years
        ["Upcoming|3|Year||", standardReferenceDateTime, RockDateTime.fromParts(2023, 1, 1, 0, 0, 0), RockDateTime.fromParts(2025, 12, 31, 23, 59, 59, 999)],

        // Between
        ["DateRange|||11/3/2022 12:00:00 AM|11/22/2022 12:00:00 AM", standardReferenceDateTime, RockDateTime.fromParts(2022, 11, 3, 0, 0, 0), RockDateTime.fromParts(2022, 11, 22, 23, 59, 59, 999)],

        // Alternative format: Current Week.
        ["Current|1|Week||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 31, 0, 0, 0), RockDateTime.fromParts(2022, 11, 6, 23, 59, 59, 999)],
        // Alternative format: Current Week.
        ["1|1|2||", standardReferenceDateTime, RockDateTime.fromParts(2022, 10, 31, 0, 0, 0), RockDateTime.fromParts(2022, 11, 6, 23, 59, 59, 999)],
    ];

    it.each(testData)("Parses '%s' into the expected start and end dates", (rangeString: string, currentDateTime: RockDateTime, expectedStart: RockDateTime, expectedEnd: RockDateTime) => {
        const rangeValue = parseSlidingDateRangeString(rangeString);
        const range = calculateSlidingDateRange(rangeValue, currentDateTime);

        dateEqual(range.start, expectedStart);
        dateEqual(range.end, expectedEnd);
    });
});
