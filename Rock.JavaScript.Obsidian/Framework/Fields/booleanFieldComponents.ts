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
import { defineComponent, ref, computed, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import { asTrueFalseOrNull, asBoolean } from "../Services/boolean";
import { ConfigurationValueKey } from "./booleanField";
import DropDownList from "../Elements/dropDownList";
import Toggle from "../Elements/toggle";
import CheckBox from "../Elements/checkBox";
import TextBox from "../Elements/textBox";
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

    emits: ["update:modelValue"],

    setup(props, { emit }) {
        // Internal values
        const internalBooleanValue = ref(false);
        const internalValue = ref("");

        // Sync internal values and modelValue
        watch(
            internalValue,
            () => emit("update:modelValue", internalValue.value)
        );

        watch(
            internalBooleanValue,
            () => emit("update:modelValue", asTrueFalseOrNull(internalBooleanValue.value) || "")
        );

        watch(
            () => props.modelValue,
            () => {
                internalValue.value = asTrueFalseOrNull(props.modelValue) || "";
                internalBooleanValue.value = asBoolean(props.modelValue);
            },
            { immediate: true }
        );

        // Which control type should be used for value selection
        const booleanControlType = computed((): BooleanControlType => {
            const controlType = props.configurationValues[ConfigurationValueKey.BooleanControlType];

            switch (controlType) {
                case "1":
                    return BooleanControlType.Checkbox;
                case "2":
                    return BooleanControlType.Toggle;
                default:
                    return BooleanControlType.DropDown;
            }
        });

        // Helpers to determine control type in the template
        const isToggle = computed((): boolean => booleanControlType.value === BooleanControlType.Toggle);
        const isCheckBox = computed((): boolean => booleanControlType.value === BooleanControlType.Checkbox);

        // What labels does the user see for the true/false values
        const trueText = computed((): string => {
            let trueText = "Yes";
            const trueConfig = props.configurationValues[ConfigurationValueKey.TrueText];

            if (trueConfig) {
                trueText = trueConfig;
            }

            return trueText || "Yes";
        });

        const falseText = computed((): string => {
            let falseText = "No";
            const falseConfig = props.configurationValues[ConfigurationValueKey.FalseText];

            if (falseConfig) {
                falseText = falseConfig;
            }

            return falseText || "No";
        });
        
        // configuration for a toggle button
        const toggleOptions = computed((): Record<string, unknown> => ({
                trueText: trueText.value,
                falseText: falseText.value
        }));

        // configuration for a dropdown control
        const dropDownListOptions = computed((): ListItem[] => {
            const trueVal = asTrueFalseOrNull(true);
            const falseVal = asTrueFalseOrNull(false);

            return [
                { text: falseText.value, value: falseVal },
                { text: trueText.value, value: trueVal }
            ] as ListItem[];
        });

        return {
            internalBooleanValue,
            internalValue,
            booleanControlType,
            isToggle,
            isCheckBox,
            toggleOptions,
            dropDownListOptions
        };
    },
    template: `
<Toggle v-if="isToggle" v-model="internalBooleanValue" v-bind="toggleOptions" />
<CheckBox v-else-if="isCheckBox" v-model="internalBooleanValue" />
<DropDownList v-else v-model="internalValue" :options="dropDownListOptions" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "BooleanField.Configuration",

});
