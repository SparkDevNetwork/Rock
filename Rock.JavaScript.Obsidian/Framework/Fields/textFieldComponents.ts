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
import { defineComponent } from "vue";
import { getFieldEditorProps } from "./utils";
import TextBox from "../Elements/textBox";
import { asBooleanOrNull } from "../Services/boolean";

enum ConfigurationValueKey {
    IsPassword = "ispassword",
    MaxCharacters = "maxcharacters",
    ShowCountDown = "showcountdown"
}

export const EditComponent = defineComponent({
    name: "TextField.Edit",
    components: {
        TextBox
    },
    props: getFieldEditorProps(),
    data() {
        return {
            internalValue: ""
        };
    },
    computed: {
        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};

            const maxCharsConfig = this.configurationValues[ConfigurationValueKey.MaxCharacters];
            if (maxCharsConfig) {
                const maxCharsValue = Number(maxCharsConfig);

                if (maxCharsValue) {
                    attributes.maxLength = maxCharsValue;
                }
            }

            const showCountDownConfig = this.configurationValues[ConfigurationValueKey.ShowCountDown];
            if (showCountDownConfig && showCountDownConfig) {
                const showCountDownValue = asBooleanOrNull(showCountDownConfig) || false;

                if (showCountDownValue) {
                    attributes.showCountDown = showCountDownValue;
                }
            }

            return attributes;
        },
        isPassword(): boolean {
            const isPasswordConfig = this.configurationValues[ConfigurationValueKey.IsPassword];
            return asBooleanOrNull(isPasswordConfig) || false;
        }
    },
    watch: {
        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler(): void {
                this.internalValue = this.modelValue || "";
            }
        }
    },
    template: `
<TextBox v-model="internalValue" v-bind="configAttributes" :type="isPassword ? 'password' : ''" />
`
});
