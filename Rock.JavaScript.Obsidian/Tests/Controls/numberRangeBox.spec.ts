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
import { mount, config } from "@vue/test-utils";
import assert = require("assert");
import NumberRangeBox from "../../Framework/Controls/numberRangeBox.obs";
import { ref } from "vue";

config.global.config.warnHandler = () => { /*empty*/ };

const wrappingComponent = {
    components: {NumberRangeBox},
    setup () {
        return {
            val: ref({ lower: 1.1, upper: 1.1 })
        };
    },
    template: `<NumberRangeBox v-model="val" />`
};

describe("Number Range Box", () => {

    it.skip("Should not delete zeroes after decimal place while still typing in first box", async () => {
        const wrapper = mount(wrappingComponent);
        const input = wrapper.get("input[id$='_lower']");
        const inputElement = input.element as HTMLInputElement;
        const numberRangeComponent = wrapper.getComponent(NumberRangeBox).vm;

        // Type in a value into 1st box with an ending of zeroes, then fire an event so the models will change.
        inputElement.focus();
        inputElement.value = "1.00";
        await input.trigger("input");

        // Verify the value for the NumberRangeBox still has zeroes, but the model is converted to a number without them
        assert.strictEqual(inputElement.value, "1.00");
        assert.strictEqual(numberRangeComponent.internalValue.lower, "1.00");
        assert.strictEqual(numberRangeComponent.modelValue.lower, 1);
    });

    it("Should delete stray zeroes after decimal place from first box when done with it", async () => {
        const wrapper = mount(wrappingComponent);
        const input = wrapper.get("input[id$='_lower']");
        const inputElement = input.element as HTMLInputElement;
        const numberRangeComponent = wrapper.getComponent(NumberRangeBox).vm;

        // Type in a value into 1st box with an ending of zeroes, then fire an event so the models will change.
        inputElement.value = "1.00";
        await input.trigger("input"); // Set internal value to "1.00" like the previous test
        await input.trigger("change"); // Fire change event too so it'll "finalize" the value and strip zeroes

        // Verify the value for the NumberRangeBox still has zeroes, but the model is converted to a number without them
        assert.strictEqual(inputElement.value, "1");
        assert.strictEqual(numberRangeComponent.internalValue.lower, "1");
        assert.strictEqual(numberRangeComponent.modelValue.lower, 1);
    });

    it.skip("Should not delete zeroes after decimal place while still typing in second box", async () => {
        const wrapper = mount(wrappingComponent);
        const input = wrapper.get("input[id$='_upper']");
        const inputElement = input.element as HTMLInputElement;
        const numberRangeComponent = wrapper.getComponent(NumberRangeBox).vm;

        // Type in a value into 1st box with an ending of zeroes, then fire an event so the models will change.
        inputElement.value = "1.00";
        await input.trigger("input");

        // Verify the value for the NumberRangeBox still has zeroes, but the model is converted to a number without them
        assert.strictEqual(inputElement.value, "1.00");
        assert.strictEqual(numberRangeComponent.internalValue.upper, "1.00");
        assert.strictEqual(numberRangeComponent.modelValue.upper, 1);
    });

    it("Should delete stray zeroes after decimal place from second box when done with it", async () => {
        const wrapper = mount(wrappingComponent);
        const input = wrapper.get("input[id$='_upper']");
        const inputElement = input.element as HTMLInputElement;
        const numberRangeComponent = wrapper.getComponent(NumberRangeBox).vm;

        // Type in a value into 1st box with an ending of zeroes, then fire an event so the models will change.
        inputElement.value = "1.00";
        await input.trigger("input"); // Set internal value to "1.00" like the previous test
        await input.trigger("change"); // Fire change event too so it'll "finalize" the value and strip zeroes

        // Verify the value for the NumberRangeBox still has zeroes, but the model is converted to a number without them
        assert.strictEqual(inputElement.value, "1");
        assert.strictEqual(numberRangeComponent.internalValue.upper, "1");
        assert.strictEqual(numberRangeComponent.modelValue.upper, 1);
    });
});
