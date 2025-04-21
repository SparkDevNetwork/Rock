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
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { ConfigurationValueKey } from "./siteField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";

export const EditComponent = defineComponent({

    name: "SiteField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string>("");
        // The Site Type options.
        const options = computed(() => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            options
        };
    },

    template: `
        <DropDownList v-model="internalValue" :items="options" />
    `
});

export const ConfigurationComponent = defineComponent({
    name: "SiteField.Configuration",

    components: { CheckBox },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const shorteningSitesOnly = ref(false);

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
            newValue[ConfigurationValueKey.ShorteningSitesOnly] = asTrueOrFalseString(shorteningSitesOnly.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ShorteningSitesOnly] !== (asTrueOrFalseString(props.modelValue[ConfigurationValueKey.ShorteningSitesOnly]));

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            shorteningSitesOnly.value = asBoolean(props.modelValue[ConfigurationValueKey.ShorteningSitesOnly]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([shorteningSitesOnly], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });


        return { shorteningSitesOnly };
    },

    template: `
<div>
    <CheckBox v-model="shorteningSitesOnly" label="Shortening Enabled Sites Only" help="Should only sites that are enabled for shortening be displayed." />
</div>
`
});
