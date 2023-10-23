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
import { defineComponent, computed, PropType, ref, watch  } from "vue";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import UrlLinkBox from "@Obsidian/Controls/urlLinkBox.obs";
import { asBoolean, asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { ConfigurationValueKey } from "./urlLinkField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";

export const EditComponent = defineComponent({
    name: "UrlLinkField.Edit",

    components: {
        UrlLinkBox
    },

    props: getFieldEditorProps(),

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const value = useVModelPassthrough(props, "modelValue", emit);

        const requiresTrailingSlash = computed(() => asBooleanOrNull(props.configurationValues.ShouldRequireTrailingForwardSlash) ?? false);

        return { value, requiresTrailingSlash };
    },
    template: `
<UrlLinkBox v-model="value" :requires-trailing-slash="requiresTrailingSlash" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "UrlLinkField.Configuration",

    components: {
        CheckBox,
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const shouldRequireTrailingForwardSlash = ref(false);
        const shouldAlwaysShowCondensed = ref(false);

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
            newValue[ConfigurationValueKey.ShouldAlwaysShowCondensed] = asTrueFalseOrNull(shouldAlwaysShowCondensed.value) ?? "False";
            newValue[ConfigurationValueKey.ShouldRequireTrailingForwardSlash] = asTrueFalseOrNull(shouldRequireTrailingForwardSlash.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ShouldAlwaysShowCondensed] !== (props.modelValue[ConfigurationValueKey.ShouldAlwaysShowCondensed] ?? "False")
                || newValue[ConfigurationValueKey.ShouldRequireTrailingForwardSlash] !== (props.modelValue[ConfigurationValueKey.ShouldRequireTrailingForwardSlash] ?? "False");

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
            shouldRequireTrailingForwardSlash.value = asBoolean(props.modelValue[ConfigurationValueKey.ShouldRequireTrailingForwardSlash]);
            shouldAlwaysShowCondensed.value = asBoolean(props.modelValue[ConfigurationValueKey.ShouldAlwaysShowCondensed]);
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
        watch(shouldRequireTrailingForwardSlash, () => maybeUpdateConfiguration(ConfigurationValueKey.ShouldRequireTrailingForwardSlash, asTrueFalseOrNull(shouldRequireTrailingForwardSlash.value) ?? "False"));
        watch(shouldAlwaysShowCondensed, () => maybeUpdateConfiguration(ConfigurationValueKey.ShouldAlwaysShowCondensed, asTrueFalseOrNull(shouldAlwaysShowCondensed.value) ?? "False"));

        return {
            shouldRequireTrailingForwardSlash,
            shouldAlwaysShowCondensed
        };
    },

    template: `
<div>
    <CheckBox v-model="shouldRequireTrailingForwardSlash" label="Ensure Trailing Forward Slash" text="Yes" help="When set, the URL must end with a forward slash (/) to be valid." />
    <CheckBox v-model="shouldAlwaysShowCondensed" label="Should always Show Condensed" text="Yes" help="When set, the URL will always be returned as a raw value." />
</div>
`
});
