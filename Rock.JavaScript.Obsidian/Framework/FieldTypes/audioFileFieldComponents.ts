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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import FileUploader from "@Obsidian/Controls/fileUploader.obs";
import BinaryFileTypePicker from "@Obsidian/Controls/binaryFileTypePicker.obs";
import { ConfigurationValueKey } from "./audioFileField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { BinaryFiletype } from "@Obsidian/SystemGuids/binaryFiletype";

export const EditComponent = defineComponent({
    name: "AudioFileField.Edit",

    components: {
        FileUploader
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag>({});

        // The selected binary file type configuration value.
        const binaryFileType = computed((): string => {
            const fileType = JSON.parse(props.configurationValues[ConfigurationValueKey.BinaryFileType] || "{}") as ListItemBag;
            return fileType.value ?? BinaryFiletype.Default;
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value ?? ""));
        });

        return {
            internalValue,
            binaryFileType
        };
    },

    template: `
<FileUploader v-model="internalValue" :uploadAsTemporary="true" :binaryFileTypeGuid="binaryFileType" uploadButtonText="Upload" :showDeleteButton="true" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "AudioFileField.Configuration",

    components: {
        BinaryFileTypePicker,
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const binaryFileType = ref<ListItemBag>({});

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
            newValue[ConfigurationValueKey.BinaryFileType] = JSON.stringify(binaryFileType.value ?? "");

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.BinaryFileType] !== (props.modelValue[ConfigurationValueKey.BinaryFileType]);

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
            binaryFileType.value = JSON.parse(props.modelValue[ConfigurationValueKey.BinaryFileType] || "{}");
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
        watch(binaryFileType, () => maybeUpdateConfiguration(ConfigurationValueKey.BinaryFileType, JSON.stringify(binaryFileType.value ?? "")));

        return {
            binaryFileType,
        };
    },

    template: `
<div>
    <BinaryFileTypePicker label="File Type" v-model="binaryFileType" help="File type to use to store and retrieve the file. New file types can be configured under 'Admin Tools > General Settings > File Types'" />
</div>
`
});