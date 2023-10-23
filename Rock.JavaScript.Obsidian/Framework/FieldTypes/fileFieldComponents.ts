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
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import FileUploader from "@Obsidian/Controls/fileUploader.obs";
import { ConfigurationValueKey, ConfigurationPropertyKey } from "./fileField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "FileField.Edit",

    components: {
        FileUploader
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag | null>(null);

        // Configuration attributes passed to the edit control.
        const binaryFileType = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.BinaryFileType] ?? "";
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            try {
                updateRefValue(internalValue, JSON.parse(props.modelValue ?? "") as ListItemBag);
            }
            catch {
                internalValue.value = null;
            }
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value ? JSON.stringify(internalValue.value) : "");
        });

        return {
            binaryFileType,
            internalValue
        };
    },

    template: `
<FileUploader v-model="internalValue" :binaryFileTypeGuid="binaryFileType" uploadAsTemporary />
`
});

export const ConfigurationComponent = defineComponent({
    name: "FileField.Configuration",

    components: {
        DropDownList
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const fileType = ref("");

        /** The binary file types the individual can select from. */
        const fileTypeOptions = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationProperties[ConfigurationPropertyKey.BinaryFileTypes] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

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
            newValue[ConfigurationValueKey.BinaryFileType] = fileType.value ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.BinaryFileType] !== (props.modelValue[ConfigurationValueKey.BinaryFileType] ?? "");

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
            fileType.value = props.modelValue[ConfigurationValueKey.BinaryFileType];
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
        watch(fileType, () => maybeUpdateConfiguration(ConfigurationValueKey.BinaryFileType, fileType.value ?? ""));

        return {
            fileType,
            fileTypeOptions
        };
    },

    template: `
<div>
    <DropDownList v-model="fileType"
        label="File Type"
        help="File type to use to store and retrieve the file. New file types can be configured under 'Admins Tools &gt; General Settings &gt; File Types'."
        :items="fileTypeOptions" />
</div>
`
});
