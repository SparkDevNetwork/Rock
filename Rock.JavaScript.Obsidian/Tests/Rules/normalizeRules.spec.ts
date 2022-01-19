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
import { normalizeRules, ValidationRule, ValidationRuleFunction, ValidationRuleReference } from "../../Framework/Rules/index";

// This suite performs tests on normalizeRules function of the rules.
describe("normalizeRules Suite", () => {
    it("Single rule in single string produces one rule", () => {
        const rules = normalizeRules("required");

        assert.notStrictEqual(rules, null);
        assert.deepStrictEqual(rules, ["required"]);
    });

    it("Two rules in single string produces two rules", () => {
        const rules = normalizeRules("required|email");

        assert.notStrictEqual(rules, null);
        assert.deepStrictEqual(rules, ["required", "email"]);
    });

    it("Two rules in two strings produces two rules", () => {
        const rules = normalizeRules(["required", "email"]);

        assert.notStrictEqual(rules, null);
        assert.deepStrictEqual(rules, ["required", "email"]);
    });

    it("Three rules in two strings produces three rules", () => {
        const rules = normalizeRules(["required", "email|min"]);

        assert.notStrictEqual(rules, null);
        assert.deepStrictEqual(rules, ["required", "email", "min"]);
    });

    it("One string rule and one function rule produces two rules", () => {
        const ruleFn: ValidationRuleFunction = value => value !== null;

        const rules = normalizeRules(["required", ruleFn]);

        assert.notStrictEqual(rules, null);
        assert.deepStrictEqual(rules, ["required", ruleFn]);
    });

    it("One string rule and one reference rule produces two rules", () => {
        const ruleRef: ValidationRuleReference = {
            name: "email",
            params: []
        };

        const rules = normalizeRules(["required", ruleRef]);

        assert.notStrictEqual(rules, null);
        assert.deepStrictEqual(rules, ["required", ruleRef]);
    });

    it("Invalid rule type produces zero rules", () => {
        const rules = normalizeRules(<ValidationRule><unknown>42);

        assert.notStrictEqual(rules, null);
        assert.deepStrictEqual(rules, []);
    });
});
