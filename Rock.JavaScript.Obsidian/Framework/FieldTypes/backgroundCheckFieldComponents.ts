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
            const entityTypeValue = entityType.value.value;
            return entityTypeValue?.toLowerCase() == EntityType.ProtectMyMinistryProvider.toLowerCase();
        });

        watch(() => props.modelValue, () => {
            const splitValues = props.modelValue.split(",");

            entityType.value = {
                text: splitValues[1],
                value: splitValues[0]
            };

            if (isFile.value) {
                fileValue.value = {
                    text: splitValues[3],
                    value: splitValues[2]
                };
            }
            else {
                textValue.value = splitValues[2];
            }

        }, { immediate: true });

        watch(() => fileValue.value, () => {
            emit("update:modelValue", `${entityType.value.value},${entityType.value.text},${fileValue.value?.value ?? ""},${fileValue.value?.text ?? ""}`);
        },{ deep: true });

        watch(() => textValue.value, () => {
            emit("update:modelValue", `${entityType.value.value},${entityType.value.text},${textValue.value ?? ""}`);
        });

        // If the entity type changes emit the now empty values so the previous default value is overidden. This way if the default value is not set
        // the now empty value is saved.
        watch(() => entityType.value, () => {
            if(entityType.value?.value?.toLowerCase() == EntityType.ProtectMyMinistryProvider.toLowerCase()) {
                emit("update:modelValue", `${entityType.value.value},${entityType.value.text},${fileValue.value?.value ?? ""},${fileValue.value?.text ?? ""}`);
            }
            else{
                emit("update:modelValue", `${entityType.value.value},${entityType.value.text},${textValue.value ?? ""}`);
            }
        });

        return {
            fileValue,
            entityType,
            isFile,
            textValue
        };
    },

    template: `
    <ComponentPicker label="Component" v-model="entityType" containerType="Rock.Security.BackgroundCheckContainer" showBlankItem />
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