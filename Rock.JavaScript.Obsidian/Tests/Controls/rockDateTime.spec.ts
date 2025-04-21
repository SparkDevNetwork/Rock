import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import assert = require("assert");

describe("humanizeElapsed function", () => {
    it("should return 59 seconds ago for time interval of 59 seconds", () => {
        const before = RockDateTime.fromParts(2024, 6, 24, 13, 37, 0);
        const after = RockDateTime.fromParts(2024, 6, 24, 13, 37, 59);

        assert.equal(before.humanizeElapsed(after), "59 seconds ago");
    });

    it("should return 1 minute ago for time interval of 60 seconds", () => {
        const before = RockDateTime.fromParts(2024, 6, 24, 13, 37, 0);
        const after = RockDateTime.fromParts(2024, 6, 24, 13, 38, 0);

        assert.equal(before.humanizeElapsed(after), "1 minute ago");
    });

    it("should return 1 minute ago for time interval of more than 60 seconds but less than 119 seconds", () => {
        const before = RockDateTime.fromParts(2024, 6, 24, 13, 37, 0);
        const after = RockDateTime.fromParts(2024, 6, 24, 13, 38, 1);

        assert.equal(before.humanizeElapsed(after), "1 minute ago");
    });

    it("should return 2 minutes ago for time interval of 120 minutes", () => {
        const before = RockDateTime.fromParts(2024, 6, 24, 13, 37, 0);
        const after = RockDateTime.fromParts(2024, 6, 24, 13, 39, 0);

        assert.equal(before.humanizeElapsed(after), "2 minutes ago");
    });
});