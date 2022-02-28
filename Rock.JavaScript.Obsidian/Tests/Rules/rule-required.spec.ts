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

const expectedRequiredErrorText = "is required";

// This suite performs tests on the required rule.
describe("required Rule", () => {
    it("Empty string produces error", () => {
        const result = validateValue("", "required");

        assert.deepStrictEqual(result, [expectedRequiredErrorText]);
    });

    it("Null string produces error", () => {
        const result = validateValue(null, "required");

        assert.deepStrictEqual(result, [expectedRequiredErrorText]);
    });

    it("Undefined string produces error", () => {
        const result = validateValue(undefined, "required");

        assert.deepStrictEqual(result, [expectedRequiredErrorText]);
    });

    it("Non-empty string produces no error", () => {
        const result = validateValue("text", "required");

        assert.deepStrictEqual(result, []);
    });

    it("Zero string produces no error", () => {
        const result = validateValue("0", "required");

        assert.deepStrictEqual(result, []);
    });

    it("Zero number produces error", () => {
        const result = validateValue(0, "required");

        assert.deepStrictEqual(result, [expectedRequiredErrorText]);
    });

    it("Non-zero number produces no error", () => {
        const result = validateValue(1, "required");

        assert.deepStrictEqual(result, []);
    });

    it("True boolean produces no error", () => {
        const result = validateValue(true, "required");

        assert.deepStrictEqual(result, []);
    });

    it("False boolean produces no error", () => {
        const result = validateValue(false, "required");

        assert.deepStrictEqual(result, []);
    });
});
