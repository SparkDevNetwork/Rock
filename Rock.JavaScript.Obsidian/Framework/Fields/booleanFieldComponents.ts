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
import { asTrueFalseOrNull, asBoolean } from "../Services/boolean";
import { ConfigurationValueKey } from "./booleanField";
import DropDownList from "../Elements/dropDownList";
import Toggle from "../Elements/toggle";
import CheckBox from "../Elements/checkBox";
import { ListItem } from "../ViewModels";

enum BooleanControlType {
    DropDown,
    Checkbox,
    Toggle
}

export const EditComponent = defineComponent({
    name: "BooleanField.Edit",
    components: {
        DropDownList,
        Toggle,
        CheckBox
    },
    props: getFieldEditorProps(),
    data() {
        return {
            internalBooleanValue: false,
            internalValue: ""
        };
    },
    computed: {
        booleanControlType(): BooleanControlType {
            const controlType = this.configurationValues[ConfigurationValueKey.BooleanControlType];

            switch (controlType) {
                case "1":
                    return BooleanControlType.Checkbox;
                case "2":
                    return BooleanControlType.Toggle;
                default:
                    return BooleanControlType.DropDown;
            }
        },
        trueText(): string {
            let trueText = "Yes";
            const trueConfig = this.configurationValues[ConfigurationValueKey.TrueText];

            if (trueConfig) {
                trueText = trueConfig;
            }

            return trueText || "Yes";
        },
        falseText(): string {
            let falseText = "No";
            const falseConfig = this.configurationValues[ConfigurationValueKey.FalseText];

            if (falseConfig) {
                falseText = falseConfig;
            }

            return falseText || "No";
        },
        isToggle(): boolean {
            return this.booleanControlType === BooleanControlType.Toggle;
        },
        isCheckBox(): boolean {
            return this.booleanControlType === BooleanControlType.Checkbox;
        },
        toggleOptions(): Record<string, unknown> {
            return {
                trueText: this.trueText,
                falseText: this.falseText
            };
        },
        dropDownListOptions(): ListItem[] {
            const trueVal = asTrueFalseOrNull(true);
            const falseVal = asTrueFalseOrNull(false);

            return [
                { text: this.falseText, value: falseVal },
                { text: this.trueText, value: trueVal }
            ] as ListItem[];
        }
    },
    watch: {
        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue);
        },
        internalBooleanValue(): void {
            const valueToEmit = asTrueFalseOrNull(this.internalBooleanValue) || "";
            this.$emit("update:modelValue", valueToEmit);
        },
        modelValue: {
            immediate: true,
            handler(): void {
                this.internalValue = asTrueFalseOrNull(this.modelValue) || "";
                this.internalBooleanValue = asBoolean(this.modelValue);
            }
        }
    },
    template: `
<Toggle v-if="isToggle" v-model="internalBooleanValue" v-bind="toggleOptions" />
<CheckBox v-else-if="isCheckBox" v-model="internalBooleanValue" />
<DropDownList v-else v-model="internalValue" :options="dropDownListOptions" />
`
});
