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
import { mount } from "@vue/test-utils";
import assert = require("assert");
import RockButton from "../../Framework/Controls/rockButton.obs";


describe("RockButton", () => {

    it("Has classes applied to button", () => {
        const wrapper = mount(RockButton, {
            attrs: {
                class: "foo"
            }
        });

        const button = wrapper.get("button");
        assert.strictEqual(button.classes("foo"), true);
    });
});
