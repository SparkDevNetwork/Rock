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

const expectedDecimalErrorText = "must be a decimal value.";

// This suite performs tests on the decimal rule.
describe("decimal Rule", () => {
    it("Empty string produces no error", () => {
        const result = validateValue("", "decimal");

        assert.deepStrictEqual(result, []);
    });

    it("Null produces no error", () => {
        const result = validateValue(null, "decimal");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined produces no error", () => {
        const result = validateValue(undefined, "decimal");

        assert.deepStrictEqual(result, []);
    });

    it("Integer as string produces no error", () => {
        const result = validateValue("42", "decimal");

        assert.deepStrictEqual(result, []);
    });

    it("Integer produces no error", () => {
        const result = validateValue(42, "decimal");

        assert.deepStrictEqual(result, []);
    });

    it("Decimal as string produces no error", () => {
        const result = validateValue("42.3", "decimal");

        assert.deepStrictEqual(result, []);
    });

    it("Decimal produces no error", () => {
        const result = validateValue(42.3, "decimal");

        assert.deepStrictEqual(result, []);
    });

    it("Text string produces error", () => {
        const result = validateValue("abc", "decimal");

        assert.deepStrictEqual(result, [expectedDecimalErrorText]);
    });

    it("True as boolean produces error", () => {
        const result = validateValue(true, "decimal");

        assert.deepStrictEqual(result, [expectedDecimalErrorText]);
    });

    it("False as boolean produces error", () => {
        const result = validateValue(false, "decimal");

        assert.deepStrictEqual(result, [expectedDecimalErrorText]);
    });
});
