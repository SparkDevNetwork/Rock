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
import TextBox from "../../Framework/Controls/textBox.obs";
import { formFieldTests } from "./rockFormFieldHelper";


describe("TextBox", () => {
    formFieldTests(TextBox);

    it("Shows a countdown", () => {
        const text = "This is some text";
        const maxLength = 20;
        const charsRemaining = maxLength - text.length;

        const wrapper = mount(TextBox, {
            props: {
                modelValue: text,
                label: "",
                showCountDown: true,
                maxLength
            }
        });

        const countdownElements = wrapper.findAll("em.badge");
        assert.strictEqual(countdownElements.length, 1);
        assert.strictEqual(countdownElements[0].text(), charsRemaining.toString());
    });
});
