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
import { ComponentObjectPropsOptions } from "@vue/runtime-core";

/**
 * Performs standard tests on controls that implement rockFieldType.
 * 
 * @param component The component to be tested.
 */
function formFieldTests(component: ComponentObjectPropsOptions): void {
    it("Does not render label when not passed", () => {
        const wrapper = mount(component, {
            props: {
                modelValue: "",
                label: ""
            }
        });

        const labels = wrapper.findAll("label");
        assert.strictEqual(labels.length, 0);
    });

    it("Renders label when passed", () => {
        const labelText = "This is the label";

        const wrapper = mount(component, {
            props: {
                modelValue: "",
                label: labelText
            }
        });

        const labels = wrapper.findAll("label");

        assert.strictEqual(labels.length, 1);
        assert.strictEqual(labels[0].text(), labelText);
    });
}

export { formFieldTests };
