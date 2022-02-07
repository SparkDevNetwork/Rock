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
import { defineRule, validateValue, ValidationRuleFunction } from "../../Framework/Rules/index";

// This suite performs tests on normalizeRuleResult function of the rules.
describe("defineRule Suite", () => {
    it("New rule is registered and called", () => {
        const fn: ValidationRuleFunction = _value => "custom rule error";
        defineRule("Mock-Test-Rule", fn);

        const result = validateValue("test value", "Mock-Test-Rule");

        assert.deepStrictEqual(result, ["custom rule error"]);
    });

    it("Duplicate rule does not replace original rule", () => {
        const fn: ValidationRuleFunction = _value => "custom rule error";
        defineRule("required", fn);

        const result = validateValue("", "required");

        assert.deepStrictEqual(result, ["is required"]);
    });

    it("Duplicate rule produces console warning", () => {
        console.warn = jest.fn();

        const fn: ValidationRuleFunction = _value => "custom rule error";
        defineRule("required", fn);

        expect(console.warn).toHaveBeenCalledWith("Attempt to redefine validation rule required.");
    });
});
