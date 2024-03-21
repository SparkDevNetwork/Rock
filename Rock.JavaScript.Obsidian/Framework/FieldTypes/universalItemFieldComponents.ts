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
import { getFieldConfigurationProps } from "./utils";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer.obs";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { deepEqual } from "@Obsidian/Utility/util";

export const ConfigurationComponent = defineComponent({
    name: "UniversalItemField.Configuration",

    components: {
        AttributeValuesContainer
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const internalValue = ref(props.modelValue);

        const attributes = computed((): Record<string, PublicAttributeBag> => {
            const attrs: Record<string, PublicAttributeBag> = {};

            try {
                const bags = JSON.parse(props.configurationProperties["Attributes"] ?? "[]") as PublicAttributeBag[];

                for (const bag of bags) {
                    if (bag.key) {
                        attrs[bag.key] = bag;
                    }
                }
            }
            catch {
                return {};
            }

            return attrs;
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
            let isChanged = !deepEqual(Object.keys(internalValue.value), Object.keys(props.modelValue), true);

            if (!isChanged) {
                for (const key of Object.keys(internalValue.value)) {
                    if (internalValue.value[key] !== (props.modelValue[key] ?? "")) {
                        isChanged = true;
                        break;
                    }
                }
            }

            // If any value changed then emit the new model value.
            if (isChanged) {
                emit("update:modelValue", internalValue.value);
                return true;
            }
            else {
                return false;
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(props.modelValue, () => {
            internalValue.value = { ...props.modelValue };
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch(internalValue, (newValue, oldValue) => {
            if (maybeUpdateModelValue()) {
                for (const key of Object.keys(newValue)) {
                    if (oldValue[key] === newValue[key]) {
                        continue;
                    }

                    emit("updateConfiguration");
                }
            }
        });

        // Watch for changes in properties that only require a local UI update.

        return {
            attributes,
            internalValue
        };
    },

    template: `
<div>
    <AttributeValuesContainer v-model="internalValue"
                              :attributes="attributes"
                              isEditMode />
</div>
`
});
