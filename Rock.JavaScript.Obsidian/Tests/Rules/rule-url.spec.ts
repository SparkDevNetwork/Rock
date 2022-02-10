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

const expectedUrlErrorText = "must be a valid URL";

// This suite performs tests on the url rule.
describe("url Rule", () => {
    it("Empty string produces no error", () => {
        const result = validateValue("", "url");

        assert.deepStrictEqual(result, []);
    });

    it("Null produces no error", () => {
        const result = validateValue(null, "url");

        assert.deepStrictEqual(result, []);
    });

    it("Undefined produces no error", () => {
        const result = validateValue(undefined, "url");

        assert.deepStrictEqual(result, []);
    });

    it("TLD only produces no error", () => {
        const result = validateValue("com", "url");

        // This is not an error so that "localhost" works.
        assert.deepStrictEqual(result, []);
    });

    it("TLD with period produces no error", () => {
        const result = validateValue(".com", "url");

        // This really should probably be an error.
        assert.deepStrictEqual(result, []);
    });

    it("Full domain with extra period produces no error", () => {
        const result = validateValue(".rocksolidchurchdemo.com", "url");

        // This really should probably be an error.
        assert.deepStrictEqual(result, []);
    });

    it("Rock URL string produces no error", () => {
        const rockUrls = [
            "http://localhost:6229/Person/1/Edit",
            "https://rock.rocksolidchurchdemo.com/page/12"
        ];

        rockUrls.forEach(value => {
            const result = validateValue("ftp://rocksolidchurchdemo.com", "url");

            assert.deepStrictEqual(result, []);
        });
    });

    it("Space in string produces error", () => {
        const result = validateValue("https://www.rock solidchurchdemo.com", "url");

        assert.deepStrictEqual(result, [expectedUrlErrorText]);
    });
});
