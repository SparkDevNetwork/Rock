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
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import ImageUploader from "@Obsidian/Controls/imageUploader.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import { ConfigurationKey, ConfigurationPropertyKey } from "./imageField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";
import { asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toGuidOrNull } from "@Obsidian/Utility/guid";
import { Guid } from "@Obsidian/Types";
import CodeEditor from "@Obsidian/Controls/codeEditor.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";

export const EditComponent = defineComponent({
    name: "ImageField.Edit",

    components: {
        ImageUploader
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag | null>(null);

        // Configuration attributes passed to the edit control.
        const binaryFileType = computed<Guid | null>(() => {
            return toGuidOrNull(props.configurationValues[ConfigurationKey.BinaryFileType]);
        });

        const enableCrop = computed<boolean>(() => {
            return asBooleanOrNull(props.configurationValues[ConfigurationKey.EnableCrop]) ?? false;
        });

        const targetWidth = computed<number>(() => {
            return toNumberOrNull(props.configurationValues[ConfigurationKey.TargetWidth]) ?? 0;
        });

        const targetHeight = computed<number>(() => {
            return toNumberOrNull(props.configurationValues[ConfigurationKey.TargetHeight]) ?? 0;
        });

        const minimumWidth = computed<number>(() => {
            return toNumberOrNull(props.configurationValues[ConfigurationKey.MinimumWidth]) ?? 0;
        });

        const minimumHeight = computed<number>(() => {
            return toNumberOrNull(props.configurationValues[ConfigurationKey.MinimumHeight]) ?? 0;
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
            enableCrop,
            targetWidth,
            targetHeight,
            minimumWidth,
            minimumHeight,
            internalValue
        };
    },

    template: `
<ImageUploader v-model="internalValue" :enableCrop="enableCrop" :targetWidth="targetWidth" :targetHeight="targetHeight" :minimumWidth="minimumWidth" :minimumHeight="minimumHeight" :binaryFileTypeGuid="binaryFileType" uploadAsTemporary />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ImageField.Configuration",

    components: {
        CheckBox,
        DropDownList,
        CodeEditor,
        NumberBox
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
        const formatAsLink = ref(false);
        const imageTagTemplate = ref("");
        const enableCrop = ref(false);
        const targetWidth = ref<number | null>(null);
        const targetHeight = ref<number | null>(null);
        const minimumWidth = ref<number | null>(null);
        const minimumHeight = ref<number | null>(null);

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
            newValue[ConfigurationKey.BinaryFileType] = fileType.value ? JSON.stringify({ text: "", value: fileType.value }) : "";
            newValue[ConfigurationKey.FormatAsLink] = asTrueFalseOrNull(formatAsLink.value) ?? "False";
            newValue[ConfigurationKey.ImageTagTemplate] = imageTagTemplate.value ?? "";
            newValue[ConfigurationKey.EnableCrop] = asTrueFalseOrNull(enableCrop.value) ?? "False";
            newValue[ConfigurationKey.TargetWidth] = targetWidth.value?.toString() ?? "";
            newValue[ConfigurationKey.TargetHeight] = targetHeight.value?.toString() ?? "";
            newValue[ConfigurationKey.MinimumWidth] = minimumWidth.value?.toString() ?? "";
            newValue[ConfigurationKey.MinimumHeight] = minimumHeight.value?.toString() ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationKey.BinaryFileType] !== (props.modelValue[ConfigurationKey.BinaryFileType] ?? "")
                || newValue[ConfigurationKey.FormatAsLink] !== (props.modelValue[ConfigurationKey.FormatAsLink] ?? "False")
                || newValue[ConfigurationKey.ImageTagTemplate] !== (props.modelValue[ConfigurationKey.ImageTagTemplate] ?? "")
                || newValue[ConfigurationKey.EnableCrop] !== (props.modelValue[ConfigurationKey.EnableCrop] ?? "False")
                || newValue[ConfigurationKey.TargetWidth] !== (props.modelValue[ConfigurationKey.TargetWidth] ?? "")
                || newValue[ConfigurationKey.TargetHeight] !== (props.modelValue[ConfigurationKey.TargetHeight] ?? "")
                || newValue[ConfigurationKey.MinimumWidth] !== (props.modelValue[ConfigurationKey.MinimumWidth] ?? "")
                || newValue[ConfigurationKey.MinimumHeight] !== (props.modelValue[ConfigurationKey.MinimumHeight] ?? "");

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
            fileType.value = JSON.parse(props.modelValue[ConfigurationKey.BinaryFileType] || "{}").value;
            formatAsLink.value = asBooleanOrNull(props.modelValue[ConfigurationKey.FormatAsLink]) ?? false;
            imageTagTemplate.value = props.modelValue[ConfigurationKey.ImageTagTemplate];
            enableCrop.value = asBooleanOrNull(props.modelValue[ConfigurationKey.EnableCrop]) ?? false;
            targetWidth.value = toNumberOrNull(props.modelValue[ConfigurationKey.TargetWidth] ?? "");
            targetHeight.value = toNumberOrNull(props.modelValue[ConfigurationKey.TargetHeight] ?? "");
            minimumWidth.value = toNumberOrNull(props.modelValue[ConfigurationKey.MinimumWidth] ?? "");
            minimumHeight.value = toNumberOrNull(props.modelValue[ConfigurationKey.MinimumHeight] ?? "");
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
        watch(fileType, () => maybeUpdateConfiguration(ConfigurationKey.BinaryFileType, fileType.value ? JSON.stringify({ text: "", value: fileType.value }) : ""));
        watch(formatAsLink, () => maybeUpdateConfiguration(ConfigurationKey.FormatAsLink, asTrueFalseOrNull(formatAsLink.value) ?? "False"));
        watch(imageTagTemplate, () => maybeUpdateConfiguration(ConfigurationKey.ImageTagTemplate, imageTagTemplate.value ?? ""));
        watch(enableCrop, () => maybeUpdateConfiguration(ConfigurationKey.EnableCrop, asTrueFalseOrNull(enableCrop.value) ?? "False"));
        watch(targetWidth, () => maybeUpdateConfiguration(ConfigurationKey.TargetWidth, targetWidth.value?.toString() ?? ""));
        watch(targetHeight, () => maybeUpdateConfiguration(ConfigurationKey.TargetHeight, targetHeight.value?.toString() ?? ""));
        watch(minimumWidth, () => maybeUpdateConfiguration(ConfigurationKey.MinimumWidth, minimumWidth.value?.toString() ?? ""));
        watch(minimumHeight, () => maybeUpdateConfiguration(ConfigurationKey.MinimumHeight, minimumHeight.value?.toString() ?? ""));

        return {
            fileType,
            fileTypeOptions,
            formatAsLink,
            imageTagTemplate,
            enableCrop,
            targetWidth,
            targetHeight,
            minimumWidth,
            minimumHeight
        };
    },

    template: `
<div>
    <DropDownList v-model="fileType"
        label="File Type"
        help="File type to use to store and retrieve the file. New file types can be configured under 'Admins Tools &gt; General Settings &gt; File Types'."
        :items="fileTypeOptions"
        enhanceForLongLists />

    <CheckBox v-model="formatAsLink"
        label="Format as Link"
        help="Enable this to navigate to a full size image when the image is clicked." />

    <CodeEditor v-model="imageTagTemplate"
        label="Image Tag Template"
        help="The Lava template to use when rendering as an html img tags."
        mode="lava"
        :editorHeight="100" />

    <CheckBox v-model="enableCrop"
        label="Enable Crop"
        help="Enable this to allow cropping of the image when uploading.<br/>Only supported by blocks using Obsidian." />

    <NumberBox v-model="targetWidth"
        label="Target Width"
        help="The width the image will be resized to when uploading.<br/>Only supported by blocks using Obsidian." />

    <NumberBox v-model="targetHeight"
        label="Target Height"
        help="The height the image will be resized to when uploading.<br/>Only supported by blocks using Obsidian." />

    <NumberBox v-model="minimumWidth"
        label="Minimum Width"
        help="The minimum width required for the image to be uploaded.<br/>Only supported by blocks using Obsidian." />

    <NumberBox v-model="minimumHeight"
        label="Minimum Height"
        help="The minimum height required for the image to be uploaded.<br/>Only supported by blocks using Obsidian." />
</div>
`
});
