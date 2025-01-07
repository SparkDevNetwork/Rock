import assert = require("assert");
import { RockDateTime } from "../../Framework/Utility/rockDateTime";

describe("formatAspDate Suite", () => {
    /*
     * Seconds
     */
    it("Elapsed Seconds past the date includes 'Ago'", () => {
        assert.ok(RockDateTime.now().addSeconds(-5).toElapsedString().includes("Seconds Ago"));
    });

    it("Elapsed Seconds less than one past the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().toElapsedString(), "1 Second Ago");
    });

    it("Elapsed Seconds greater than one past the date produces plural noun", () => {
        assert.strictEqual(RockDateTime.now().addSeconds(-5).toElapsedString(), "5 Seconds Ago");
    });

    it("Elapsed Seconds from the date includes 'From Now'", () => {
        assert.ok(RockDateTime.now().addSeconds(5).toElapsedString().includes("Seconds From Now"));
    });

    it("Elapsed Seconds less than one from the date produces singular noun", () => {
        assert.ok(RockDateTime.now().addSeconds(1).toElapsedString().includes("Second From Now"));
    });

    it("Elapsed Seconds greater than from the date produces plural noun", () => {
        assert.ok(RockDateTime.now().addSeconds(5).toElapsedString().includes("Seconds From Now"));
    });

    /*
    * Minutes
    */
    it("Elapsed Minutes past the date includes 'Ago'", () => {
        assert.ok(RockDateTime.now().addMinutes(-5).toElapsedString().includes("Minutes Ago"));
    });

    it("Elapsed Minutes less than one past the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addMinutes(-1).toElapsedString(), "1 Minute Ago");
    });

    it("Elapsed Minutes greater than one past the date produces plural noun", () => {
        assert.strictEqual(RockDateTime.now().addMinutes(-5).toElapsedString(), "5 Minutes Ago");
    });

    it("Elapsed Minutes from the date includes 'From Now'", () => {
        assert.ok(RockDateTime.now().addMinutes(5).toElapsedString().includes("Minutes From Now"));
    });

    it("Elapsed Minutes less than one from the date produces singular noun", () => {
        assert.ok(RockDateTime.now().addMinutes(1).addSeconds(5).toElapsedString().includes("Minute From Now"));
    });

    it("Elapsed Minutes greater than from the date produces plural noun", () => {
        assert.ok(RockDateTime.now().addMinutes(5).toElapsedString().includes("Minutes From Now"));
    });

    /*
    * Hours
    */
    it("Elapsed Hours past the date includes 'Ago'", () => {
        assert.ok(RockDateTime.now().addHours(-5).toElapsedString().includes("Hours Ago"));
    });

    it("Elapsed Hours less than one past the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addHours(-1).toElapsedString(), "1 Hour Ago");
    });

    it("Elapsed Hours greater than one past the date produces plural noun", () => {
        assert.strictEqual(RockDateTime.now().addHours(-5).toElapsedString(), "5 Hours Ago");
    });

    it("Elapsed Hours from the date includes 'From Now'", () => {
        assert.ok(RockDateTime.now().addHours(5).toElapsedString().includes("Hours From Now"));
    });

    it("Elapsed Hours less than one from the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addHours(1).addSeconds(1).toElapsedString(), "1 Hour From Now");
    });

    it("Elapsed Hours greater than from the date produces plural noun", () => {
        assert.ok(RockDateTime.now().addHours(5).toElapsedString().includes("Hours From Now"));
    });

    /*
    * Days
    */
    it("Elapsed Days past the date includes 'Ago'", () => {
        assert.ok(RockDateTime.now().addDays(-5).toElapsedString().includes("Days Ago"));
    });

    it("Elapsed Days less than one past the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addDays(-1).toElapsedString(), "1 Day Ago");
    });

    it("Elapsed Days greater than one past the date produces plural noun", () => {
        assert.strictEqual(RockDateTime.now().addDays(-5).toElapsedString(), "5 Days Ago");
    });

    it("Elapsed Days from the date includes 'From Now'", () => {
        assert.ok(RockDateTime.now().addDays(5).toElapsedString().includes("Days From Now"));
    });

    it("Elapsed Days less than one from the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addDays(1.1).toElapsedString(), "1 Day From Now");
    });

    it("Elapsed Days greater than from the date produces plural noun", () => {
        assert.ok(RockDateTime.now().addDays(5).toElapsedString().includes("Days From Now"));
    });

    /*
    * Months
    */
    it("Elapsed Months past the date includes 'Ago'", () => {
        assert.ok(RockDateTime.now().addMonths(-5).toElapsedString().includes("Months Ago"));
    });

    it("Elapsed Months less than one past the date produces singular noun", () => {
        const nowDate = RockDateTime.fromParts(2024, 6, 30);
        const date = RockDateTime.fromParts(2024,5,14);
        assert.strictEqual(date.toElapsedString(nowDate), "1 Month Ago");
    });

    it("Elapsed Months greater than one past the date produces plural noun", () => {
        assert.strictEqual(RockDateTime.now().addMonths(-5).toElapsedString(), "5 Months Ago");
    });

    it("Elapsed Months from the date includes 'From Now'", () => {
        assert.ok(RockDateTime.now().addMonths(5).toElapsedString().includes("Months From Now"));
    });

    it("Elapsed Months less than one from the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addMonths(1).addDays(1).toElapsedString(), "1 Month From Now");
    });

    it("Elapsed Months greater than from the date produces plural noun", () => {
        assert.ok(RockDateTime.now().addMonths(5).toElapsedString().includes("Months From Now"));
    });

    /*
    * Years
    */
    it("Elapsed Years past the date includes 'Ago'", () => {
        assert.ok(RockDateTime.now().addYears(-5).toElapsedString().includes("Years Ago"));
    });

    it("Elapsed Years less than one past the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addYears(-1).addMonths(-7).toElapsedString(), "1 Year Ago");
    });

    it("Elapsed Years greater than one past the date produces plural noun", () => {
        assert.strictEqual(RockDateTime.now().addYears(-5).toElapsedString(), "5 Years Ago");
    });

    it("Elapsed Years from the date includes 'From Now'", () => {
        assert.strictEqual(RockDateTime.now().addYears(5).toElapsedString(), "5 Years From Now");
    });

    it("Elapsed Years less than one from the date produces singular noun", () => {
        assert.strictEqual(RockDateTime.now().addYears(1).addMonths(7).toElapsedString(), "1 Year From Now");
    });

    it("Elapsed Years greater than from the date produces plural noun", () => {
        assert.ok(RockDateTime.now().addYears(5).toElapsedString().includes("Years From Now"));
    });
});