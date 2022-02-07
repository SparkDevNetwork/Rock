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
import { validateValue, ValidationRuleFunction } from "../../Framework/Rules/index";

// This suite performs tests on normalizeRuleResult function of the rules.
describe("normalizeRuleResult Suite", () => {
    it("Empty string produces no error", () => {
        const fn: ValidationRuleFunction = _value => "";

        const result = validateValue("test value", fn);

        assert.deepStrictEqual(result, []);
    });

    it("Non-empty string produces error", () => {
        const fn: ValidationRuleFunction = _value => "some error";

        const result = validateValue("test value", fn);

        assert.deepStrictEqual(result, ["some error"]);
    });

    it("True boolean produces no error", () => {
        const fn: ValidationRuleFunction = _value => true;

        const result = validateValue("test value", fn);

        assert.deepStrictEqual(result, []);
    });

    it("False boolean produces generic error", () => {
        const fn: ValidationRuleFunction = _value => false;

        const result = validateValue("test value", fn);

        assert.deepStrictEqual(result, ["failed validation"]);
    });
});
