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
import HtmlEditor from "@Obsidian/Controls/htmlEditor.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { PickerDisplayStyle } from "@Obsidian/Enums/Controls/pickerDisplayStyle";
import { FileAsset } from "@Obsidian/ViewModels/Controls/fileAsset";
import { ConfigurationValueKey } from "./htmlField.partial";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "HtmlField.Edit",

    components: {
        HtmlEditor
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref("");
        const refreshKey = ref(0);

        const toolbar = computed(() => {
            return (props.configurationValues[ConfigurationValueKey.Toolbar] ?? "Light").toLocaleLowerCase();
        });

        const documentFolderRoot = computed(() => {
            return props.configurationValues[ConfigurationValueKey.EncryptedDocumentFolderRoot] ?? "";
        });

        const imageFolderRoot = computed(() => {
            return props.configurationValues[ConfigurationValueKey.EncryptedImageFolderRoot] ?? "";
        });

        const userSpecificRoot = computed(() => {
            return props.configurationValues[ConfigurationValueKey.UserSpecificRoot] == "True";
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue ?? "";
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value ?? "");
        });

        // HTML Editor can't change the toolbar after it changes, so we need to re-render it on change.
        watch(toolbar, () => {
            refreshKey.value++;
        });

        return {
            internalValue,
            toolbar,
            documentFolderRoot,
            imageFolderRoot,
            userSpecificRoot,
            refreshKey
        };
    },

    template: `
<HtmlEditor v-model="internalValue"
            :key="refreshKey"
            editorHeight="200px"
            :toolbar="toolbar"
            :encryptedDocumentRootFolder="documentFolderRoot"
            :encryptedImageRootFolder="imageFolderRoot"
            :userSpecificRoot="userSpecificRoot" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "HtmlField.Configuration",
    components: {
        CheckBox,
        DropDownList,
        TextBox
    },

    props: getFieldConfigurationProps(),

    emit: {
        "update:modelValue": (_v: Record<string, string>) => true,
        "updateConfigurationValue": (_k: string, _v: string) => true,
        "updateConfiguration": () => true
    },

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const toolbar = ref("");
        const documentFolderRoot = ref("");
        const imageFolderRoot = ref("");
        const userSpecificRoot = ref(false);

        const toolbarOptions = ref<ListItemBag[]>([
            { text: "Light", value: "Light" },
            { text: "Full", value: "Full" }
        ]);

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
            newValue[ConfigurationValueKey.Toolbar] = toolbar.value?.toString() || "Light";
            newValue[ConfigurationValueKey.DocumentFolderRoot] = documentFolderRoot.value?.toString() ?? "";
            newValue[ConfigurationValueKey.ImageFolderRoot] = imageFolderRoot.value?.toString() ?? "";
            newValue[ConfigurationValueKey.UserSpecificRoot] = asTrueFalseOrNull(userSpecificRoot.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.Toolbar] !== (props.modelValue[ConfigurationValueKey.Toolbar] ?? "Light")
                || newValue[ConfigurationValueKey.DocumentFolderRoot] !== (props.modelValue[ConfigurationValueKey.DocumentFolderRoot] ?? "")
                || newValue[ConfigurationValueKey.ImageFolderRoot] !== (props.modelValue[ConfigurationValueKey.ImageFolderRoot] ?? "")
                || newValue[ConfigurationValueKey.UserSpecificRoot] !== (props.modelValue[ConfigurationValueKey.ImageFolderRoot] ?? "False");

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
            toolbar.value = props.modelValue[ConfigurationValueKey.Toolbar] ?? "Light";
            documentFolderRoot.value = props.modelValue[ConfigurationValueKey.DocumentFolderRoot] ?? "";
            imageFolderRoot.value = props.modelValue[ConfigurationValueKey.ImageFolderRoot] ?? "";
            userSpecificRoot.value = asBoolean(props.modelValue[ConfigurationValueKey.UserSpecificRoot]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([documentFolderRoot, imageFolderRoot], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(toolbar, () => maybeUpdateConfiguration(ConfigurationValueKey.Toolbar, toolbar.value ?? "Light"));
        watch(documentFolderRoot, () => maybeUpdateConfiguration(ConfigurationValueKey.DocumentFolderRoot, documentFolderRoot.value?.toString() ?? ""));
        watch(imageFolderRoot, () => maybeUpdateConfiguration(ConfigurationValueKey.ImageFolderRoot, imageFolderRoot.value?.toString() ?? ""));
        watch(userSpecificRoot, () => maybeUpdateConfiguration(ConfigurationValueKey.UserSpecificRoot, asTrueFalseOrNull(userSpecificRoot.value) ?? "False"));

        return {
            toolbar,
            toolbarOptions,
            documentFolderRoot,
            imageFolderRoot,
            userSpecificRoot
        };
    },

    template: `
<div>
    <DropDownList v-model="toolbar"
        :items="toolbarOptions"
        label="Toolbar Type"
        help="The type of toolbar to display on editor." />

    <TextBox v-model.lazy="documentFolderRoot"
        label="Document Root Folder"
        help="The folder to use as the root when browsing or uploading documents ( e.g. ~/Content )." />

    <TextBox v-model.lazy="imageFolderRoot"
        label="Image Root Folder"
        help="The folder to use as the root when browsing or uploading images ( e.g. ~/Content )." />

    <CheckBox v-model="userSpecificRoot"
        label="User Specific Folders"
        help="Should the root folders be specific to current user?" />
</div>
`
});