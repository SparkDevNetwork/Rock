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

const expectedUrlErrorText = (expected: unknown): string => `must end with "${expected}"`;

// This suite performs tests on the endswith rule.
describe("endswith Rule", () => {
    it("Empty string produces no error", () => {
        const result = validateValue("", "endswith");

        assert.deepStrictEqual(result, []);
    });

    it("Null produces no error", () => {
        const result = validateValue(null, "endswith");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined produces no error", () => {
        const result = validateValue(undefined, "endswith");

        assert.deepStrictEqual(result, []);
    });

    it("Passes if no parameter was given", () => {
        const result = validateValue("abc", "endswith:");

        assert.deepStrictEqual(result, []);
    });

    it("Is case sensitive", () => {
        const result = validateValue("testabc", "endswith:ABC");

        assert.deepStrictEqual(result, [expectedUrlErrorText("ABC")]);
    });

    it("Does not match substring", () => {
        const result = validateValue("testABCdef", "endswith:ABC");

        assert.deepStrictEqual(result, [expectedUrlErrorText("ABC")]);
    });

    it("Matches end of string", () => {
        const result = validateValue("testABC", "endswith:ABC");

        assert.deepStrictEqual(result, []);
    });
});
