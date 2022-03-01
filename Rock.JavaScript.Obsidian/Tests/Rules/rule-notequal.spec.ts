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

const expectedNotEqualErrorText = (value: unknown): string => `must not equal ${value}`;

// This suite performs tests on the notequal rule.
describe("notequal Rule", () => {
    it("Empty string != empty string produces error", () => {
        const result = validateValue("", "notequal:");

        assert.deepStrictEqual(result, [expectedNotEqualErrorText("")]);
    });

    it("Null != empty string produces no error", () => {
        const result = validateValue(null, "notequal:");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined != empty string produces no error", () => {
        const result = validateValue(undefined, "notequal:");

        assert.deepStrictEqual(result, []);
    });

    it("Empty string != undefined string produces no error", () => {
        const result = validateValue("", "notequal");

        assert.deepStrictEqual(result, []);
    });
    
    it("Null != undefined string produces no error", () => {
        const result = validateValue(null, "notequal");

        assert.deepStrictEqual(result, []);
    });
    
    it("undefined != undefined string produces error", () => {
        const result = validateValue(undefined, "notequal");

        assert.deepStrictEqual(result, [expectedNotEqualErrorText(undefined)]);
    });
    
    it("abc string != abc string produces error", () => {
        const result = validateValue("abc", "notequal:abc");

        assert.deepStrictEqual(result, [expectedNotEqualErrorText("abc")]);
    });

    it("abc string != ABC string produces no error", () => {
        const result = validateValue("abc", "notequal:ABC");

        assert.deepStrictEqual(result, []);
    });

    it("0 number != 0 string produces error", () => {
        const result = validateValue(0, "notequal:0");

        assert.deepStrictEqual(result, [expectedNotEqualErrorText("0")]);
    });

    it("0 number != 1 string produces no error", () => {
        const result = validateValue(0, "notequal:1");

        assert.deepStrictEqual(result, []);
    });

    it("Zero string != 0 string produces error", () => {
        const result = validateValue("0", "notequal:0");

        assert.deepStrictEqual(result, [expectedNotEqualErrorText("0")]);
    });

    it("Zero string != 1 string produces no error", () => {
        const result = validateValue("0", "notequal:1");

        assert.deepStrictEqual(result, []);
    });

    it("True boolean != true string produces error", () => {
        const result = validateValue(true, "notequal:true");

        assert.deepStrictEqual(result, [expectedNotEqualErrorText("true")]);
    });

    it("False boolean != false string produces error", () => {
        const result = validateValue(false, "notequal:false");

        assert.deepStrictEqual(result, [expectedNotEqualErrorText("false")]);
    });

    it("False boolean != true string produces no error", () => {
        const result = validateValue(false, "notequal:true");

        assert.deepStrictEqual(result, []);
    });

    it("True boolean != false string produces no error", () => {
        const result = validateValue(true, "notequal:false");

        assert.deepStrictEqual(result, []);
    });
});
