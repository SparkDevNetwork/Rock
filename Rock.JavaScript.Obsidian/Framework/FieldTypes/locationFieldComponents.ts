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
import { defineComponent, PropType, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import LocationPicker from "@Obsidian/Controls/locationPicker.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import { ConfigurationValueKey } from "./locationField.partial";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { asListItemBagOrNull } from "@Obsidian/Utility/listItemBag";
import { Guid } from "@Obsidian/Types";
import { AddressControlBag } from "@Obsidian/ViewModels/Controls/addressControlBag";

export const EditComponent = defineComponent({
    name: "LocationField.Edit",

    components: {
        LocationPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref<AddressControlBag | ListItemBag | string | null>(null);
        const allowedPickerModes = ref<string[]>([]);

        watch(() => props.modelValue, () => {
            try {
                internalValue.value = JSON.parse(props.modelValue || "null");
            }
            catch {
                internalValue.value = null;
            }
        }, { immediate: true });

        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        watch(() => props.configurationValues, () => {
            allowedPickerModes.value = (props.configurationValues[ConfigurationValueKey.AllowedPickerModes]?.split(",") ?? []).filter(s => s !== "");
        }, { immediate: true });

        return {
            internalValue,
        };
    },

    template: `
<LocationPicker v-model="internalValue" label="Location" />`
});


export const ConfigurationComponent = defineComponent({
    name: "LocationField.Configuration",

    components: {
        CheckBoxList
    },

    props: {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },

    emits: {
        "update:modelValue": (_v: Record<string, string>) => true,
        "updateConfigurationValue": (_key: string, _value: string) => true,
        "updateConfiguration": () => true
    },

    setup(props, { emit }) {
        console.debug("CONFIG", props.modelValue);

        const options = [{
            text: "Location",
            value: "0"
        }, {
            text: "Address",
            value: "1"
        }, {
            text: "Point",
            value: "2"
        }, {
            text: "Geo-fence",
            value: "3"
        }];

        const availableLocationTypes = ref<string[]>([]);

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {
                ...props.modelValue
            };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.AllowedPickerModes] = availableLocationTypes.value.join(",");

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.AllowedPickerModes] !== props.modelValue[ConfigurationValueKey.AllowedPickerModes];

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

        // Watch for changes coming in from the parent component and update our data to match the new information.
        watch(() => props.modelValue, () => {
            availableLocationTypes.value = (props.modelValue[ConfigurationValueKey.AllowedPickerModes]?.split(",") ?? []).filter(s => s !== "");
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(availableLocationTypes, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowedPickerModes, JSON.stringify(availableLocationTypes.value)));

        return {
            availableLocationTypes,
            options
        };
    },

    template: `
    <CheckBoxList v-model="availableLocationTypes"
        label="Available Location Types"
        help="Select the location types that can be used by the Location Picker."
        :items="options"
        horizontal />
`
});
