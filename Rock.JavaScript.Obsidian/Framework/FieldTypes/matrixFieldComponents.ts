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
import { defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { ConfigurationValueKey, ConfigurationPropertyKey } from "./matrixField.partial";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import AttributeMatrixEditor from "@Obsidian/Controls/Internal/attributeMatrixEditor.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { MatrixFieldDataBag } from "@Obsidian/ViewModels/Rest/Controls/matrixFieldDataBag";

export const EditComponent = defineComponent({
    name: "MatrixField.Edit",
    components: {
        AttributeMatrixEditor
    },
    props: getFieldEditorProps(),

    emits: ["update:modelValue"],

    setup(props, { emit }) {
        const internalValue = ref(tryParseModel());

        watch(internalValue, () => emit("update:modelValue", JSON.stringify(internalValue.value)));

        watch(() => props.modelValue, () => internalValue.value = tryParseModel());

        function tryParseModel() :MatrixFieldDataBag {
            try{
                return JSON.parse(props.modelValue) as MatrixFieldDataBag;
            }
            catch(e) {
                return {
                    matrixItems: [],
                    attributes: {}
                } as MatrixFieldDataBag;
            }
        }

        return {
            val: internalValue,
        };
    },
    template: `
<AttributeMatrixEditor v-model="val.matrixItems" :attributes="val.attributes" :defaultAttributeValues="val.defaultAttributeValues" :minRows="val.minRows" :maxRows="val.maxRows" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "MatrixField.Configuration",

    components: { DropDownList },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        const template = ref<string>("");
        const templateOptions =ref<ListItemBag[]>([]);

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
            newValue[ConfigurationValueKey.AttributeMatrixTemplate] = template.value ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.AttributeMatrixTemplate] !== (props.modelValue[ConfigurationValueKey.AttributeMatrixTemplate] ?? "");

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
            const templates = props.configurationProperties[ConfigurationPropertyKey.Templates];

            templateOptions.value = templates ? JSON.parse(templates) as ListItemBag[] : [];
            template.value = props.modelValue[ConfigurationValueKey.AttributeMatrixTemplate] ?? "";
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(template, () => maybeUpdateConfiguration(ConfigurationValueKey.AttributeMatrixTemplate, template.value ?? ""));

        return { templateOptions, template };
    },

    template: `
<div>
    <DropDownList v-model="template"
                  label="Attribute Matrix Template"
                  help="The Attribute Matrix Template that defines this matrix attribute"
                  :items="templateOptions"
                  show-blank-item
                  rules="required" />
</div>
`
});
