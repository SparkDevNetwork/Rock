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
import { validateValue } from "../../Framework/Rules/index";

const expectedSsnErrorText = "must be a valid social security number";

// This suite performs tests on the ssn rule.
describe("ssn Rule", () => {
    it("Empty string produces no error", () => {
        const result = validateValue("", "ssn");

        assert.deepStrictEqual(result, []);
    });

    it("Null produces no error", () => {
        const result = validateValue(null, "ssn");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined produces no error", () => {
        const result = validateValue(undefined, "ssn");

        assert.deepStrictEqual(result, []);
    });

    it("9 digit number produces no error", () => {
        const result = validateValue(111223333, "ssn");

        assert.deepStrictEqual(result, []);
    });

    it("9 digit string produces no error", () => {
        const result = validateValue("111223333", "ssn");

        assert.deepStrictEqual(result, []);
    });

    it("9 digit string with dashes produces no error", () => {
        const result = validateValue("111-22-3333", "ssn");

        assert.deepStrictEqual(result, []);
    });

    it("9 digit string with single dash produces error", () => {
        const result = validateValue("11122-3333", "ssn");

        assert.deepStrictEqual(result, [expectedSsnErrorText]);
    });

    it("9 digit string with incorrect dash produces error", () => {
        const result = validateValue("11-1-22-3333", "ssn");

        assert.deepStrictEqual(result, [expectedSsnErrorText]);
    });

    it("Under 9 digit numbers produce error", () => {
        [1, 11, 111, 1112, 11122, 111223, 1112233, 11122333].forEach(value => {
            const result = validateValue(value, "ssn");

            assert.deepStrictEqual(result, [expectedSsnErrorText]);
        });
    });

    it("Under 9 digit strings produce error", () => {
        ["1", "11", "111", "1112", "11122", "111223", "1112233", "11122333"].forEach(value => {
            const result = validateValue(value, "ssn");

            assert.deepStrictEqual(result, [expectedSsnErrorText]);
        });
    });

    it("Over 9 digit number produces error", () => {
        const result = validateValue(1112233334, "ssn");

        assert.deepStrictEqual(result, [expectedSsnErrorText]);
    });

    it("Over 9 digit string produces error", () => {
        const result = validateValue("1112233334", "ssn");

        assert.deepStrictEqual(result, [expectedSsnErrorText]);
    });
});
