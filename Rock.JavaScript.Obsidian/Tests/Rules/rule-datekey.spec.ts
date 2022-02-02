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

import assert = require("assert");
import exp = require("constants");
import { validateValue } from "../../Framework/Rules/index";

// This suite performs tests on the datekey rule.
describe("datekey Rule", () => {
    it("Empty string produces error", () => {
        const result = validateValue("", "datekey");

        assert.strictEqual(result.length, 1);
    });

    it("Value under 8 digits produces error", () => {
        ["2", "20", "202", "2021", "20210", "202104", "2021041"].forEach(value => {
            const result = validateValue(value, "datekey");

            assert.strictEqual(result.length, 1);
        });
    });

    it("8 digit value produces no error", () => {
        const result = validateValue("20210413", "datekey");

        assert.deepStrictEqual(result, []);
    });

    it("Missing year produces error", () => {
        const result = validateValue("202", "datekey");

        assert.deepStrictEqual(result, ["must have a year"]);
    });

    it("Missing month produces error", () => {
        const result = validateValue("2021", "datekey");

        // Error message is always "must have a year"
        assert.deepStrictEqual(result, ["must have a year"]);
    });

    it("Missing day produces error", () => {
        const result = validateValue("202104", "datekey");

        // Error message is always "must have a year"
        assert.deepStrictEqual(result, ["must have a year"]);
    });

    it("Zero year produces error", () => {
        const result = validateValue("00000413", "datekey");

        assert.deepStrictEqual(result, ["must have a year"]);
    });

    it("Zero month produces error", () => {
        const result = validateValue("20210013", "datekey");

        assert.deepStrictEqual(result, ["must have a month"]);
    });

    it("Zero day produces error", () => {
        const result = validateValue("20210400", "datekey");

        assert.deepStrictEqual(result, ["must have a day"]);
    });
});
