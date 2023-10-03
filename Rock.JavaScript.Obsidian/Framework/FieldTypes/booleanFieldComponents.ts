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
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { asTrueFalseOrNull, asBoolean } from "@Obsidian/Utility/booleanUtils";
import { ConfigurationValueKey } from "./booleanField.partial";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import Toggle from "@Obsidian/Controls/toggle.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";

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
        const internalBooleanValue = ref(asBoolean(props.modelValue));
        const internalValue = ref(asTrueFalseOrNull(props.modelValue) || "");

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
        const dropDownListOptions = computed((): ListItemBag[] => {
            const trueVal = asTrueFalseOrNull(true);
            const falseVal = asTrueFalseOrNull(false);

            return [
                { text: falseText.value, value: falseVal },
                { text: trueText.value, value: trueVal }
            ] as ListItemBag[];
        });

        // Sync internal values and modelValue
        watch(internalValue, () => {
            if (booleanControlType.value === BooleanControlType.DropDown) {
                emit("update:modelValue", internalValue.value);
            }
        });

        watch(internalBooleanValue, () => {
            if (booleanControlType.value !== BooleanControlType.DropDown) {
                emit("update:modelValue", asTrueFalseOrNull(internalBooleanValue.value) || "");
            }
        });

        watch(() => props.modelValue, () => {
            internalValue.value = asTrueFalseOrNull(props.modelValue) || "";
            internalBooleanValue.value = asBoolean(props.modelValue);
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
<DropDownList v-else v-model="internalValue" :items="dropDownListOptions" />
`
});

export const FilterComponent = defineComponent({
    name: "BooleanField.Filter",
    components: {
        DropDownList
    },
    props: getFieldEditorProps(),

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        // Internal values
        const internalValue = ref(asTrueFalseOrNull(props.modelValue) || "");

        // Sync internal values and modelValue
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        watch(() => props.modelValue, () => {
            internalValue.value = asTrueFalseOrNull(props.modelValue) || "";
        });

        // What labels does the user see for the true/false values
        const trueText = computed((): string => {
            const trueConfig = props.configurationValues[ConfigurationValueKey.TrueText];

            return trueConfig || "Yes";
        });

        const falseText = computed((): string => {
            const falseConfig = props.configurationValues[ConfigurationValueKey.FalseText];

            return falseConfig || "No";
        });

        // configuration for a dropdown control
        const dropDownListOptions = computed((): ListItemBag[] => {
            const trueVal = asTrueFalseOrNull(true);
            const falseVal = asTrueFalseOrNull(false);

            return [
                { text: falseText.value, value: falseVal },
                { text: trueText.value, value: trueVal }
            ] as ListItemBag[];
        });

        return {
            internalValue,
            dropDownListOptions
        };
    },
    template: `
<DropDownList v-model="internalValue" :items="dropDownListOptions" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "BooleanField.Configuration",

    components: { TextBox, DropDownList },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        const trueText = ref("Yes");
        const falseText = ref("No");
        const controlType = ref<BooleanControlType>(BooleanControlType.DropDown);

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.TrueText] = trueText.value ?? "Yes";
            newValue[ConfigurationValueKey.FalseText] = falseText.value ?? "No";
            newValue[ConfigurationValueKey.BooleanControlType] = controlType.value?.toString() ?? BooleanControlType.DropDown.toString();

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.TrueText] !== (props.modelValue[ConfigurationValueKey.TrueText] ?? "Yes")
                || newValue[ConfigurationValueKey.FalseText] !== (props.modelValue[ConfigurationValueKey.FalseText] ?? "No")
                || newValue[ConfigurationValueKey.BooleanControlType] !== (props.modelValue[ConfigurationValueKey.BooleanControlType] ?? BooleanControlType.DropDown);

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         *
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            trueText.value = props.modelValue[ConfigurationValueKey.TrueText] ?? "Yes";
            falseText.value = props.modelValue[ConfigurationValueKey.FalseText] ?? "No";
            controlType.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.BooleanControlType]) ?? 0;
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(trueText, () => maybeUpdateConfiguration(ConfigurationValueKey.TrueText, trueText.value ?? "Yes"));
        watch(falseText, () => maybeUpdateConfiguration(ConfigurationValueKey.FalseText, falseText.value ?? "No"));
        watch(controlType, () => maybeUpdateConfiguration(ConfigurationValueKey.BooleanControlType, controlType.value?.toString() ?? "0"));

        const controlTypeOptions = [
            { text: "Drop Down", value: BooleanControlType.DropDown },
            { text: "Checkbox", value: BooleanControlType.Checkbox },
            { text: "Toggle", value: BooleanControlType.Toggle }
        ];

        return { controlTypeOptions, trueText, falseText, controlType };
    },

    template: `
<div>
    <TextBox v-model="trueText" label="True Text" help="The text to display when value is true" />
    <TextBox v-model="falseText" label="False Text" help="The text to display when value is false" />
    <DropDownList v-model="controlType" label="Control Type" help="The type of control to use when editing the value" :items="controlTypeOptions" :show-blank-item="false" />
</div>
`
});
