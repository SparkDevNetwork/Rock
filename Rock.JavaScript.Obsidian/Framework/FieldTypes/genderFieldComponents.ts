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
import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";

const enum ConfigurationValueKey {
    HideUnknownGender = "hideUnknownGender"
}

export const EditComponent = defineComponent({

    name: "GenderField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    data() {

        return {
            internalValue: ""
        };
    },

    computed: {
        dropDownListOptions(): ListItemBag[] {
            const hideUnknownGenderConfig = this.configurationValues[ConfigurationValueKey.HideUnknownGender];
            const hideUnknownGender = hideUnknownGenderConfig?.toLowerCase() === "true";

            if (hideUnknownGender === false) {
                return [
                    { text: "Unknown", value: "0" },
                    { text: "Male", value: "1" },
                    { text: "Female", value: "2" }
                ] as ListItemBag[];
                }
            else {
                return [
                    { text: "Male", value: "1" },
                    { text: "Female", value: "2" }
                ] as ListItemBag[];
            }
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue || "";
            }
        }
    },

    template: `
<DropDownList v-model="internalValue" :items="dropDownListOptions" formControlClasses="input-width-md" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "GenderField.Configuration",

    components: { CheckBox },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const hideUnknownGender = ref(false);

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
            newValue[ConfigurationValueKey.HideUnknownGender] = asTrueFalseOrNull(hideUnknownGender.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.HideUnknownGender] !== (props.modelValue[ConfigurationValueKey.HideUnknownGender] ?? "False");

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
            hideUnknownGender.value = asBoolean(props.modelValue[ConfigurationValueKey.HideUnknownGender]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        // THIS IS JUST A PLACEHOLDER FOR COPYING TO NEW FIELDS THAT MIGHT NEED IT.
        // THIS FIELD DOES NOT NEED THIS
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(hideUnknownGender, () => maybeUpdateConfiguration(ConfigurationValueKey.HideUnknownGender, asTrueFalseOrNull(hideUnknownGender.value) ?? "False"));

        return { hideUnknownGender };
    },

    template: `
<div>
    <CheckBox v-model="hideUnknownGender" label="Hide Unknown Gender" help="When set, the 'Unknown' Option will not appear in the list of genders." />
</div>
`
});
