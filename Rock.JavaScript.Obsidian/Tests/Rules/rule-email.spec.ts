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

const expectedEmailErrorText = "must be a valid email";

// This suite performs tests on the email rule.
describe("email Rule", () => {
    const validEmails = [
        "ted@rocksolidchurchdemo.com",
        "ted.decker@rocksolidchurchdemo.com",
        "ted-decker@rocksolidchurchdemo.com",
        "ted@subdomain.rocksolidchurchdemo.com"
    ];

    const invalidEmails = [
        "rocksolidchurchdemo.com",
        "@rocksolidchurchdemo.com",
        "ted@com",
        "ted@rocksolidchurchdemo",
        "ted@decker@rocksolidchurchdemo.com"
    ];

    it("Empty string produces no error", () => {
        const result = validateValue("", "email");

        assert.deepStrictEqual(result, []);
    });

    it("Null produces no error", () => {
        const result = validateValue(null, "email");

        assert.deepStrictEqual(result, []);
    });

    it("0 number produces error", () => {
        const result = validateValue(0, "email");

        assert.deepStrictEqual(result, [expectedEmailErrorText]);
    });

    it("Non-zero number produces error", () => {
        const result = validateValue(1, "email");

        assert.deepStrictEqual(result, [expectedEmailErrorText]);
    });

    it("True boolean produces error", () => {
        const result = validateValue(true, "email");

        assert.deepStrictEqual(result, [expectedEmailErrorText]);
    });

    it("False boolean produces error", () => {
        const result = validateValue(false, "email");

        assert.deepStrictEqual(result, [expectedEmailErrorText]);
    });

    validEmails.forEach(value => {
        it(`${value} produces no error`, () => {
            const result = validateValue(value, "email");

            assert.deepStrictEqual(result, []);
        });
    });

    invalidEmails.forEach(value => {
        it(`${value} produces error`, () => {
            const result = validateValue(value, "email");

            assert.deepStrictEqual(result, [expectedEmailErrorText]);
        });
    });
});
