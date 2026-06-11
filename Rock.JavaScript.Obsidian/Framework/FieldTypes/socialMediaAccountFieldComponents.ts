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
import { ConfigurationKey } from "./socialMediaAccountField.partial";

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
            newValue[ConfigurationKey.Name] = name.value ?? "";
            newValue[ConfigurationKey.IconCssClass] = iconCssClass.value ?? "";
            newValue[ConfigurationKey.Color] = color.value ?? "";
            newValue[ConfigurationKey.TextTemplate] = textTemplate.value ?? "";
            newValue[ConfigurationKey.BaseUrl] = baseUrl.value ?? "";
            newValue[ConfigurationKey.BaseUrlAliases] = baseUrlAliases.value ?? "";

            const anyValueChanged = newValue[ConfigurationKey.Name] !== props.modelValue[ConfigurationKey.Name]
                || newValue[ConfigurationKey.IconCssClass] !== props.modelValue[ConfigurationKey.IconCssClass]
                || newValue[ConfigurationKey.Color] !== props.modelValue[ConfigurationKey.Color]
                || newValue[ConfigurationKey.TextTemplate] !== props.modelValue[ConfigurationKey.TextTemplate]
                || newValue[ConfigurationKey.BaseUrl] !== props.modelValue[ConfigurationKey.BaseUrl]
                || newValue[ConfigurationKey.BaseUrlAliases] !== props.modelValue[ConfigurationKey.BaseUrlAliases];

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
            name.value = props.modelValue[ConfigurationKey.Name] ?? "";
            iconCssClass.value = props.modelValue[ConfigurationKey.IconCssClass] ?? "";
            color.value = props.modelValue[ConfigurationKey.Color] ?? "";
            textTemplate.value = props.modelValue[ConfigurationKey.TextTemplate] ?? "";
            baseUrl.value = props.modelValue[ConfigurationKey.BaseUrl] ?? "";
            baseUrlAliases.value = props.modelValue[ConfigurationKey.BaseUrlAliases] ?? "";
        }, {
            immediate: true
        });

        watch(name, val => maybeUpdateConfiguration(ConfigurationKey.Name, val ?? ""));
        watch(iconCssClass, val => maybeUpdateConfiguration(ConfigurationKey.IconCssClass, val ?? ""));
        watch(color, val => maybeUpdateConfiguration(ConfigurationKey.Color, val ?? ""));
        watch(textTemplate, val => maybeUpdateConfiguration(ConfigurationKey.TextTemplate, val ?? ""));
        watch(baseUrl, val => maybeUpdateConfiguration(ConfigurationKey.BaseUrl, val ?? ""));
        watch(baseUrlAliases, val => maybeUpdateConfiguration(ConfigurationKey.BaseUrlAliases, val ?? ""));

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
    <CodeEditor label="Text Template" v-model="textTemplate" mode="text" :editorHeight="200" help="Lava template to use to create a formatted version for the link. Primarily used for making the link text." />
    <UrlLinkBox label="Base URL" v-model="baseUrl" help="The base URL for the social media network. If the entry does not have a URL in it this base URL will be prepended to the entered string." />
    <TextBox label="Base URL Aliases" v-model="baseUrlAliases" help="A comma-delimited list of URL prefixes that are considered valid aliases for the Base URL. If any of these values are detected in the input, they will be replaced by the Base URL." />
    `
});
