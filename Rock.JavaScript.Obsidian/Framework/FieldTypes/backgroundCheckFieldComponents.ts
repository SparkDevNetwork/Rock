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
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import BinaryFileTypePicker from "@Obsidian/Controls/binaryFileTypePicker.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import FileUploader from "@Obsidian/Controls/fileUploader.obs";
import ComponentPicker from "@Obsidian/Controls/componentPicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./backgroundCheckField.partial";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
//import { splitCase } from "@Obsidian/Utility/stringUtils";

export const EditComponent = defineComponent({
    name: "BackgroundCheckField.Edit",

    components: {
        TextBox,
        FileUploader,
        ComponentPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const fileValue = ref<ListItemBag>({});
        const entityType = ref<ListItemBag>({});
        const textValue = ref<string>("");

        const isFile = computed((): boolean => {
            const entityTypeValue = entityType?.value?.value;
            return (entityTypeValue?.toLowerCase() !== EntityType.CheckrProvider.toLowerCase());
        });

        watch(() => props.modelValue, () => {

            if (!props.modelValue) {
                return;
            }

            const splitValues = props.modelValue.split(",");

            /*
                6/26/2025 - NA

                If the original stored attribute was:

                - a single GUID (used by ProtectMyMinistry), the props.modelValue format is:

                    {EntityType.Guid},{EntityType.FriendlyName},{BinaryFile.Guid},{filename(if any)}
                    0                  1                        2                 3

                - a comma-delimited string of <EntityTypeId>,<Key|Guid> (used by other providers),
                    the props.modelValue format is:
                    {EntityType.Guid},{EntityType.FriendlyName},{RecordKey|BinaryFile.Guid},{filename(if any)}
                    0                  1                         2                          3

                    splitValues[2] will be RecordKey for Checkr, and a BinaryFileGuid for everyone else (isFile=true)
            */

            // Stop if the provider entity type did not change.
            if (splitValues[0] == entityType?.value?.value) {
                return;
            }

            entityType.value = {
                text: splitValues[1], // EntityType.FriendlyName
                value: splitValues[0] // EntityType.Guid
            };

            if (isFile.value) {
                fileValue.value = {
                    text: splitValues[3], // filename (if any)
                    value: splitValues[2] // BinaryFile Guid
                };
            }
            else {
                textValue.value = splitValues[2]; // RecordKey
            }

        }, { immediate: true });

        watch(() => fileValue.value, () => {
            emit("update:modelValue", `${entityType?.value?.value},${entityType?.value?.text},${fileValue.value?.value ?? ""},${fileValue.value?.text ?? ""}`);
        }, { deep: true });

        watch(() => textValue.value, () => {
            emit("update:modelValue", `${entityType?.value?.value},${entityType?.value?.text},${textValue.value ?? ""}`);
        });

        // If the entity type changes emit the now empty values so the previous default value is overidden. This way if the default value is not set
        // the now empty value is saved.
        watch(() => entityType.value, () => {
            if (isFile.value) {
                emit("update:modelValue", `${entityType?.value?.value},${entityType?.value?.text},${fileValue.value?.value ?? ""},${fileValue.value?.text ?? ""}`);
            }
            else {
                emit("update:modelValue", `${entityType?.value?.value},${entityType?.value?.text},${textValue.value ?? ""}`);
            }
        });

        return {
            fileValue,
            entityType,
            isFile,
            textValue,
        };
    },

    template: `
    <ComponentPicker v-model="entityType" v-bind="$attrs" containerType="Rock.Security.BackgroundCheckContainer" showBlankItem includeInactive />
    <div v-if="entityType?.value">
        <FileUploader v-if="isFile"
            v-model="fileValue"
            uploadAsTemporary="true"
            uploadButtonText="Upload"
            showDeleteButton="true" />
        <TextBox v-else
            v-model="textValue"
            label="RecordKey"
            help="Unique key for the background check report."
            textMode="MultiLine" />
    </div>
`
});


export const ConfigurationComponent = defineComponent({
    name: "BackgroundCheckField.Configuration",

    components: {
        BinaryFileTypePicker
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
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
            binaryFileType.value = JSON.parse(props.modelValue[ConfigurationValueKey.BinaryFileType] || "{}");
        }, {
            immediate: true
        });

        watch(binaryFileType, val => maybeUpdateConfiguration(ConfigurationValueKey.BinaryFileType, JSON.stringify(val ?? "")));

        return {
            binaryFileType
        };
    },

    template: `
    <BinaryFileTypePicker label="File Type" v-model="binaryFileType" help="File type to use to store and retrieve the file. New file types can be configured under 'Admin Tools > General Settings > File Types'" />
    `
});