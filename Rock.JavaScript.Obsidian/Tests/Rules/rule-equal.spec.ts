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

const expectedEqualErrorText = (value: unknown): string => `must equal ${value}`;

// This suite performs tests on equal rule.
describe("equal Rule", () => {
    it("Empty string == empty string produces no error", () => {
        const result = validateValue("", "equal:");

        assert.deepStrictEqual(result, []);
    });

    it("Null == empty string produces error", () => {
        const result = validateValue(null, "equal:");

        assert.deepStrictEqual(result, [expectedEqualErrorText("")]);
    });

    it("Undefined == empty string produces error", () => {
        const result = validateValue(undefined, "equal:");

        assert.deepStrictEqual(result, [expectedEqualErrorText("")]);
    });

    it("Empty string == undefined string produces error", () => {
        const result = validateValue("", "equal");

        assert.deepStrictEqual(result, [expectedEqualErrorText(undefined)]);
    });
    
    it("Null == undefined string produces error", () => {
        const result = validateValue(null, "equal");

        assert.deepStrictEqual(result, [expectedEqualErrorText(undefined)]);
    });
    
    it("undefined == undefined string produces no error", () => {
        const result = validateValue(undefined, "equal");

        assert.deepStrictEqual(result, []);
    });
    
    it("abc string == abc string produces no error", () => {
        const result = validateValue("abc", "equal:abc");

        assert.deepStrictEqual(result, []);
    });

    it("abc string == ABC string produces error", () => {
        const result = validateValue("abc", "equal:ABC");

        assert.deepStrictEqual(result, [expectedEqualErrorText("ABC")]);
    });

    it("0 number == 0 string produces no error", () => {
        const result = validateValue(0, "equal:0");

        assert.deepStrictEqual(result, []);
    });

    it("0 number == 1 string produces error", () => {
        const result = validateValue(0, "equal:1");

        assert.deepStrictEqual(result, [expectedEqualErrorText("1")]);
    });

    it("Zero string == 0 string produces no error", () => {
        const result = validateValue("0", "equal:0");

        assert.deepStrictEqual(result, []);
    });

    it("Zero string == 1 string produces error", () => {
        const result = validateValue("0", "equal:1");

        assert.deepStrictEqual(result, [expectedEqualErrorText("1")]);
    });

    it("True boolean == true string produces no error", () => {
        const result = validateValue(true, "equal:true");

        assert.deepStrictEqual(result, []);
    });

    it("False boolean == false string produces no error", () => {
        const result = validateValue(false, "equal:false");

        assert.deepStrictEqual(result, []);
    });

    it("False boolean == true string produces error", () => {
        const result = validateValue(false, "equal:true");

        assert.deepStrictEqual(result, [expectedEqualErrorText("true")]);
    });

    it("True boolean == false string produces error", () => {
        const result = validateValue(true, "equal:false");

        assert.deepStrictEqual(result, [expectedEqualErrorText("false")]);
    });
});
