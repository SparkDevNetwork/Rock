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

import { defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { useHttp } from "@Obsidian/Utility/http";
import { debounce } from "@Obsidian/Utility/util";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import { ComponentPickerGetComponentsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/ComponentPickerGetComponentsOptionsBag";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { ConfigurationValueKey } from "./componentsField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";

const http = useHttp();

export const EditComponent = defineComponent({
    name: "ComponentField.Edit",

    components: {
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string[]>([]);
        // The selected container type configuration value.
        const containerType = ref("");
        const componentOptions = ref<ListItemBag[]>([]);
        const loadOptionsDebounce = debounce(() => loadOptions(), 500, true);

        /**
        * Loads the components from the server.
        */
        const loadOptions = async (): Promise<void> => {

            if (!containerType.value) {
                componentOptions.value = [];
            }
            else {
                const options: Partial<ComponentPickerGetComponentsOptionsBag> = {
                    containerType: containerType.value
                };
                const result = await http.post<ListItemBag[]>("/api/v2/Controls/ComponentPickerGetComponents", undefined, options);

                if (result.isSuccess && result.data) {
                    updateRefValue(componentOptions, result.data);
                    // Internal value set here and not immediately after watch because CheckBoxList will wipe the viewmodel value
                    // if no options are available. So we set the internal value after we get our options.
                    updateRefValue(internalValue, props.modelValue ? props.modelValue.split("|") : []);
                }
                else {
                    console.error(result.errorMessage ?? "Unknown error while loading components.");
                    componentOptions.value = [];
                }
            }
        };

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? props.modelValue.toUpperCase().split("|") : []);
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value.join("|"));
        });

        watch(() => props.configurationValues, () => {
            updateRefValue(containerType, props.configurationValues[ConfigurationValueKey.Container] ?? "");
            loadOptionsDebounce();
        }, {
            immediate: true
        });

        return {
            internalValue,
            componentOptions
        };
    },

    template: `
    <CheckBoxList v-model="internalValue" :items="componentOptions" :horizontal="false" :repeatColumns="1" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ComponentField.Configuration",

    components: {
        TextBox,
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const containerType = ref<string>("");

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
            newValue[ConfigurationValueKey.Container] = containerType.value;

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.Container] !== (props.modelValue[ConfigurationValueKey.Container]);

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
            containerType.value = props.modelValue[ConfigurationValueKey.Container];
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(containerType, () => maybeUpdateConfiguration(ConfigurationValueKey.Container, containerType.value));

        return {
            containerType,
        };
    },

    template: `
    <TextBox label="Container Assembly Name" v-model="containerType" help="The assembly name of the MEF container to show components of" />
`
});
