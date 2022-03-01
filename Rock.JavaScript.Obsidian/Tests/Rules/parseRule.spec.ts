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
import { parseRule } from "../../Framework/Rules/index";

// This suite performs tests on parseRule function of the rules.
describe("parseRule Suite", () => {
    it("Parses string with no parameters", () => {
        const rule = parseRule("required");

        assert.notStrictEqual(rule, null);
        assert.strictEqual(rule?.name, "required");
        assert.deepStrictEqual(rule?.params, []);
    });

    it("Parses string with one parameter", () => {
        const rule = parseRule("required:1");

        assert.notStrictEqual(rule, null);
        assert.strictEqual(rule?.name, "required");
        assert.deepStrictEqual(rule?.params, ["1"]);
    });

    it("Parses string with two parameters", () => {
        const rule = parseRule("required:1,b");

        assert.notStrictEqual(rule, null);
        assert.strictEqual(rule?.name, "required");
        assert.deepStrictEqual(rule?.params, ["1", "b"]);
    });
});
