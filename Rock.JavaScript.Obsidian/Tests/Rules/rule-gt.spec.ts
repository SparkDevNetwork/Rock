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

const expectedGreaterThanErrorText = (value: unknown): string => `must be greater than ${value}`;

// This suite performs tests on gt rule.
describe("gt Rule", () => {
    it("Empty string > 0 produces no error", () => {
        const result = validateValue("", "gt:0");

        assert.deepStrictEqual(result, []);
    });

    it("Null > 0 produces no error", () => {
        const result = validateValue(null, "gt:0");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined > 0 produces no error", () => {
        const result = validateValue(undefined, "gt:0");

        assert.deepStrictEqual(result, []);
    });

    it("Empty string > -1 produces no error", () => {
        const result = validateValue("", "gt:-1");

        assert.deepStrictEqual(result, []);
    });

    it("Null > -1 produces no error", () => {
        const result = validateValue(null, "gt:-1");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined > -1 produces no error", () => {
        const result = validateValue(undefined, "gt:-1");

        assert.deepStrictEqual(result, []);
    });

    it("abc string > 0 produces error", () => {
        const result = validateValue("abc", "gt:0");

        assert.deepStrictEqual(result, [expectedGreaterThanErrorText("0")]);
    });

    it("5 string > undefined produces error", () => {
        const result = validateValue("5", "gt");

        assert.deepStrictEqual(result, [expectedGreaterThanErrorText(undefined)]);
    });

    it("1 string > 0 produces no error", () => {
        const result = validateValue("1", "gt:0");

        assert.deepStrictEqual(result, []);
    });

    it("1 number > 0 produces no error", () => {
        const result = validateValue(1, "gt:0");

        assert.deepStrictEqual(result, []);
    });

    it("1 string > 2 produces error", () => {
        const result = validateValue("1", "gt:2");

        assert.deepStrictEqual(result, [expectedGreaterThanErrorText("2")]);
    });

    it("1 number > 2 produces error", () => {
        const result = validateValue(1, "gt:2");

        assert.deepStrictEqual(result, [expectedGreaterThanErrorText("2")]);
    });

    it("2 string > 2 produces error", () => {
        const result = validateValue("2", "gt:2");

        assert.deepStrictEqual(result, [expectedGreaterThanErrorText("2")]);
    });

    it("2 number > 2 produces error", () => {
        const result = validateValue(2, "gt:2");

        assert.deepStrictEqual(result, [expectedGreaterThanErrorText("2")]);
    });
});
