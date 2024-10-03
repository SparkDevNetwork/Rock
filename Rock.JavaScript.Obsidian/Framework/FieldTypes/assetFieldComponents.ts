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
import { getFieldEditorProps } from "./utils";
import AssetPicker from "@Obsidian/Controls/Internal/assetPicker.obs";
import { PickerDisplayStyle } from "@Obsidian/Enums/Controls/pickerDisplayStyle";
import { FileAsset } from "@Obsidian/ViewModels/Controls/fileAsset";


export const EditComponent = defineComponent({
    name: "AssetField.Edit",

    components: {
        AssetPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<Partial<FileAsset> | null>(null);
        const displayStyle = PickerDisplayStyle.Condensed;

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            const modelProp = JSON.parse(props.modelValue || "{}");

            internalValue.value = {
                assetStorageProviderId: modelProp?.AssetStorageProviderId,
                key: modelProp?.Key,
                iconPath: modelProp?.IconPath,
                name: modelProp?.Name,
                uri: modelProp?.Url
            };
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            if (internalValue.value && internalValue.value.key) {
                const value = {
                    AssetStorageProviderId: internalValue.value?.assetStorageProviderId?.toString(),
                    Key: internalValue.value?.key,
                    IconPath: internalValue.value?.iconPath,
                    Name: internalValue.value?.name,
                    Url: internalValue.value?.uri
                };
                emit("update:modelValue", JSON.stringify(value));
            }
            else {
                emit("update:modelValue", "");
            }
        });

        return {
            internalValue,
            displayStyle
        };
    },

    template: `<AssetPicker v-model="internalValue" />`
});

export const ConfigurationComponent = defineComponent({
    name: "AssetField.Configuration",

    template: ``
});