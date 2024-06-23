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
import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import LocationPicker from "@Obsidian/Controls/locationPicker.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import RadioButtonList from "@Obsidian/Controls/radioButtonList.obs";
import { ConfigurationValueKey } from "./locationField.partial";
import { AddressControlBag } from "@Obsidian/ViewModels/Controls/addressControlBag";

export const EditComponent = defineComponent({
    name: "LocationField.Edit",

    components: {
        LocationPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref<AddressControlBag | ListItemBag | string | null>(null);

        const selectedAsNumber = computed(() => {
            const allowedPickerModes = (props.configurationValues[ConfigurationValueKey.AllowedPickerModes]?.split(",") ?? []).filter(s => s !== "");
            if (allowedPickerModes.length === 0) {
                return undefined;
            }

            return allowedPickerModes.reduce((total, option) => {
                return total + parseInt(option, 10);
            }, 0);
        });

        const currentPickerMode = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.CurrentPickerMode];
        });

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

        return {
            currentPickerMode,
            selectedAsNumber,
            internalValue,
        };
    },

    template: `
<LocationPicker v-model="internalValue" label="Location" v-model:currentPickerMode="currentPickerMode" :allowedPickerModes="selectedAsNumber"  />`
});


export const ConfigurationComponent = defineComponent({
    name: "LocationField.Configuration",

    components: {
        CheckBoxList,
        RadioButtonList
    },

    props: getFieldConfigurationProps(),

    emits: {
        "update:modelValue": (_v: Record<string, string>) => true,
        "updateConfigurationValue": (_key: string, _value: string) => true,
        "updateConfiguration": () => true
    },

    setup(props, { emit }) {
        console.debug("CONFIG", props.modelValue);

        const options = [{
            text: "Location",
            value: "2"
        }, {
            text: "Address",
            value: "1"
        }, {
            text: "Point",
            value: "4"
        }, {
            text: "Geo-fence",
            value: "8"
        }];

        const availableLocationTypes = ref<string[]>([]);
        const currentPickerMode = ref("");

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
            newValue[ConfigurationValueKey.CurrentPickerMode] = currentPickerMode.value;

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.AllowedPickerModes] !== props.modelValue[ConfigurationValueKey.AllowedPickerModes] ||
                        newValue[ConfigurationValueKey.CurrentPickerMode] !== props.modelValue[ConfigurationValueKey.CurrentPickerMode];

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
            availableLocationTypes.value = (props.modelValue[ConfigurationValueKey.AllowedPickerModes]?.split(",") ?? []).filter(s => s !== "");
            currentPickerMode.value = (props.modelValue[ConfigurationValueKey.CurrentPickerMode] ?? "");
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([availableLocationTypes,currentPickerMode], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(availableLocationTypes, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowedPickerModes, availableLocationTypes.value.join(",")));
        watch(currentPickerMode, () => maybeUpdateConfiguration(ConfigurationValueKey.CurrentPickerMode, currentPickerMode.value));

        return {
            currentPickerMode,
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

    <RadioButtonList v-model="currentPickerMode"
        label="Default Location Type"
        help="Select the location type that is initially displayed."
        :items="options"
        horizontal />
`
});
