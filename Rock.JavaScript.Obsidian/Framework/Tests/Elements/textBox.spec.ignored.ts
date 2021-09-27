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
import TextBox from "../../Elements/TextBox";

describe("TextBox", () => {
    it("Does not render label when not passed", async () => {
        const wrapper = mount(TextBox);
        await wrapper.setProps({
            modelValue: "",
            label: ""
        });

        const labels = wrapper.findAll("label");

        assert.equal(labels.length, 0);
    });

    it("Renders label when passed", async () => {
        const labelText = "This is the label";

        const wrapper = mount(TextBox);
        await wrapper.setProps({
            modelValue: "",
            label: labelText
        });

        const labels = wrapper.findAll("label");
        assert.equal(labels.length, 1);
        assert.equal(labels[0].text(), labelText);
    });

    it("Shows a countdown", async () => {
        const text = "This is some text";
        const maxLength = 20;
        const charsRemaining = maxLength - text.length;

        const wrapper = mount(TextBox);
        await wrapper.setProps({
          modelValue: text,
          label: "",
          showCountDown: true,
          maxLength
        });

        const countdownElements = wrapper.findAll("em.badge");
        assert.equal(countdownElements.length, 1);
        assert.equal(countdownElements[0].text(), charsRemaining.toString());
    });
});
