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
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import UrlLinkBox from "@Obsidian/Controls/urlLinkBox.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import CodeEditor from "@Obsidian/Controls/codeEditor.obs";
import ColorPicker from "@Obsidian/Controls/colorPicker.obs";
import { ConfigurationValueKey } from "./socialMediaAccountField.partial";

export const EditComponent = defineComponent({
    name: "SocialMediaAccountField.Edit",

    components: {
        UrlLinkBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref("");

        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue || "";
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue
        };
    },

    template: `
    <UrlLinkBox label="URL" v-model="internalValue" />
`
});


export const ConfigurationComponent = defineComponent({
    name: "SocialMediaAccountField.Configuration",

    components: {
        TextBox,
        ColorPicker,
        CodeEditor,
        UrlLinkBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const name = ref("");
        const iconCssClass = ref("");
        const color = ref("");
        const textTemplate = ref("");
        const baseUrl = ref("");
        const baseUrlAliases = ref("");

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
            newValue[ConfigurationValueKey.Name] = name.value ?? "";
            newValue[ConfigurationValueKey.IconCssClass] = iconCssClass.value ?? "";
            newValue[ConfigurationValueKey.Color] = color.value ?? "";
            newValue[ConfigurationValueKey.TextTemplate] = textTemplate.value ?? "";
            newValue[ConfigurationValueKey.BaseUrl] = baseUrl.value ?? "";
            newValue[ConfigurationValueKey.BaseUrlAliases] = baseUrlAliases.value ?? "";

            const anyValueChanged = newValue[ConfigurationValueKey.Name] !== props.modelValue[ConfigurationValueKey.Name]
                || newValue[ConfigurationValueKey.IconCssClass] !== props.modelValue[ConfigurationValueKey.IconCssClass]
                || newValue[ConfigurationValueKey.Color] !== props.modelValue[ConfigurationValueKey.Color]
                || newValue[ConfigurationValueKey.TextTemplate] !== props.modelValue[ConfigurationValueKey.TextTemplate]
                || newValue[ConfigurationValueKey.BaseUrl] !== props.modelValue[ConfigurationValueKey.BaseUrl]
                || newValue[ConfigurationValueKey.BaseUrlAliases] !== props.modelValue[ConfigurationValueKey.BaseUrlAliases];

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
            name.value = props.modelValue[ConfigurationValueKey.Name] ?? "";
            iconCssClass.value = props.modelValue[ConfigurationValueKey.IconCssClass] ?? "";
            color.value = props.modelValue[ConfigurationValueKey.Color] ?? "";
            textTemplate.value = props.modelValue[ConfigurationValueKey.TextTemplate] ?? "";
            baseUrl.value = props.modelValue[ConfigurationValueKey.BaseUrl] ?? "";
            baseUrlAliases.value = props.modelValue[ConfigurationValueKey.BaseUrlAliases] ?? "";
        }, {
            immediate: true
        });

        watch(name, val => maybeUpdateConfiguration(ConfigurationValueKey.Name, val ?? ""));
        watch(iconCssClass, val => maybeUpdateConfiguration(ConfigurationValueKey.IconCssClass, val ?? ""));
        watch(color, val => maybeUpdateConfiguration(ConfigurationValueKey.Color, val ?? ""));
        watch(textTemplate, val => maybeUpdateConfiguration(ConfigurationValueKey.TextTemplate, val ?? ""));
        watch(baseUrl, val => maybeUpdateConfiguration(ConfigurationValueKey.BaseUrl, val ?? ""));
        watch(baseUrlAliases, val => maybeUpdateConfiguration(ConfigurationValueKey.BaseUrlAliases, val ?? ""));

        return {
            name,
            iconCssClass,
            color,
            textTemplate,
            baseUrl,
            baseUrlAliases
        };
    },

    template: `
    <TextBox label="Name" v-model="name" help="The name of the social media network." />
    <TextBox label="Icon CSS Class" v-model="iconCssClass" help="The icon that represents the social media network." />
    <ColorPicker label="Color" v-model="color" help="The color to use for making buttons for the social media network." />
    <CodeEditor label="Text Template" v-model="textTemplate" theme="rock" mode="text" :editorHeight="200" help="Lava template to use to create a formatted version for the link. Primarily used for making the link text." />
    <UrlLinkBox label="Base URL" v-model="baseUrl" help="The base URL for the social media network. If the entry does not have a URL in it this base URL will be prepended to the entered string." />
    <TextBox label="Base URL Aliases" v-model="baseUrlAliases" help="A comma-delimited list of URL prefixes that are considered valid aliases for the Base URL. If any of these values are detected in the input, they will be replaced by the Base URL." />
    `
});
