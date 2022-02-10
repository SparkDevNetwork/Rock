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
import { validateValue, ValidationRuleReference } from "../../Framework/Rules/index";

// This suite performs tests on normalizeRulesToFunctions function of the rules.
describe("normalizeRulesToFunctions Suite", () => {
    it("String rule calls defined rule", () => {
        const result = validateValue("", "required");

        assert.deepStrictEqual(result, ["is required"]);
    });

    it("Reference rule calls defined rule", () => {
        const rule: ValidationRuleReference = {
            name: "required",
            params: []
        };
        const result = validateValue("", rule);

        assert.deepStrictEqual(result, ["is required"]);
    });

    it("Unknown string rule produces no error", () => {
        console.warn = jest.fn();

        const result = validateValue("", "XYZ-Does-Not-Exist");

        assert.deepStrictEqual(result, []);
        expect(console.warn).toHaveBeenCalledWith("Attempt to validate with unknown rule XYZ-Does-Not-Exist.");
    });

    it("Unknown reference rule produces no error", () => {
        console.warn = jest.fn();

        const rule: ValidationRuleReference = {
            name: "XYZ-Does-Not-Exist",
            params: []
        };
        const result = validateValue("", rule);

        assert.deepStrictEqual(result, []);
        expect(console.warn).toHaveBeenCalledWith("Attempt to validate with unknown rule XYZ-Does-Not-Exist.");
    });

    it("Unknown string rule produces console warning", () => {
        console.warn = jest.fn();

        validateValue("", "XYZ-Does-Not-Exist");

        expect(console.warn).toHaveBeenCalledWith("Attempt to validate with unknown rule XYZ-Does-Not-Exist.");
    });

    it("Unknown reference rule produces console warning", () => {
        console.warn = jest.fn();

        const rule: ValidationRuleReference = {
            name: "XYZ-Does-Not-Exist",
            params: []
        };
        validateValue("", rule);

        expect(console.warn).toHaveBeenCalledWith("Attempt to validate with unknown rule XYZ-Does-Not-Exist.");
    });
});
