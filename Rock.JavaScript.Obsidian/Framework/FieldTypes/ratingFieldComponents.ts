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
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import Rating from "@Obsidian/Controls/rating.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import { ConfigurationValueKey, RatingValue } from "./ratingField.partial";

export const EditComponent = defineComponent({
    name: "RatingField.Edit",

    components: {
        Rating
    },

    props: getFieldEditorProps(),

    data() {
        return {
            /** The current rating value. */
            internalValue: 0
        };
    },

    computed: {
        maxRating(): number {
            const maxRatingConfig = this.configurationValues[ConfigurationValueKey.MaxRating];

            return toNumberOrNull(maxRatingConfig) || 5;
        },

    },

    watch: {
        /**
         * Watch for changes to internalValue and emit the new value out to
         * the consuming component.
         */
        internalValue(): void {
            const ratingValue: RatingValue = {
                value: this.internalValue,
                maxValue: this.maxRating
            };

            this.$emit("update:modelValue", JSON.stringify(ratingValue));
        },

        /**
         * Watch for changes to modelValue which indicate the component
         * using us has given us a new value to work with.
         */
        modelValue: {
            immediate: true,
            handler(): void {
                try {
                    const ratingValue = JSON.parse(this.modelValue ?? "") as RatingValue;
                    this.internalValue = ratingValue.value ?? 0;
                }
                catch {
                    this.internalValue = 0;
                }
            }
        }
    },
    template: `
<Rating v-model="internalValue" :maxRating="maxRating" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "TextField.Configuration",

    components: {
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
        const maxRating = ref<number | null>(null);

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
            newValue[ConfigurationValueKey.MaxRating] = maxRating.value?.toString() ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.MaxRating] !== (props.modelValue[ConfigurationValueKey.MaxRating] ?? "");

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
            maxRating.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.MaxRating]);
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
        watch(maxRating, () => maybeUpdateConfiguration(ConfigurationValueKey.MaxRating, maxRating.value?.toString() ?? ""));

        return {
            maxRating
        };
    },

    template: `
<div>
    <NumberBox v-model="maxRating" label="Max Rating" help="The number of stars (max rating) that should be displayed" />
</div>
`
});
