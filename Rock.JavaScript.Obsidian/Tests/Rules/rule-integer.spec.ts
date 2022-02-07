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

const expectedIntegerErrorText = "must be an integer value.";

// This suite performs tests on the integer rule.
describe("integer Rule", () => {
    it("Empty string produces no error", () => {
        const result = validateValue("", "integer");

        assert.deepStrictEqual(result, []);
    });

    it("Null produces no error", () => {
        const result = validateValue(null, "integer");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined produces no error", () => {
        const result = validateValue(undefined, "integer");

        assert.deepStrictEqual(result, []);
    });

    it("Integer as string produces no error", () => {
        const result = validateValue("42", "integer");

        assert.deepStrictEqual(result, []);
    });

    it("Integer produces no error", () => {
        const result = validateValue(42, "integer");

        assert.deepStrictEqual(result, []);
    });

    it("Decimal as string produces error", () => {
        const result = validateValue("42.3", "integer");

        assert.deepStrictEqual(result, [expectedIntegerErrorText]);
    });

    it("Decimal produces error", () => {
        const result = validateValue(42.3, "integer");

        assert.deepStrictEqual(result, [expectedIntegerErrorText]);
    });

    it("Text string produces error", () => {
        const result = validateValue("abc", "integer");

        assert.deepStrictEqual(result, [expectedIntegerErrorText]);
    });

    it("True as boolean produces error", () => {
        const result = validateValue(true, "integer");

        assert.deepStrictEqual(result, [expectedIntegerErrorText]);
    });

    it("False as boolean produces error", () => {
        const result = validateValue(false, "integer");

        assert.deepStrictEqual(result, [expectedIntegerErrorText]);
    });
});
