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
import { defineComponent, inject } from "vue";
import { getFieldEditorProps } from "./utils";
import ListBox from "../Elements/listBox";
import CheckBoxList from "../Elements/checkBoxList";
import { toNumberOrNull } from "../Services/number";
import { ConfigurationValueKey } from "./multiSelectField";
import { ListItem } from "../ViewModels";
import { asBoolean } from "../Services/boolean";

export const EditComponent = defineComponent({
    name: "MultiSelectField.Edit",

    components: {
        ListBox,
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup() {
        return {
            isRequired: inject("isRequired") as boolean
        };
    },

    data() {
        return {
            internalValue: [] as string[]
        };
    },

    computed: {
        /** The options to choose from */
        options(): ListItem[] {
            try {
                const valuesConfig = JSON.parse(this.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItem[];

                return valuesConfig.map(v => {
                    return {
                        text: v.text,
                        value: v.value
                    } as ListItem;
                });
            }
            catch {
                return [];
            }
        },

        /** Any additional attributes that will be assigned to the list box control */
        listBoxConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const enhancedSelection = this.configurationValues[ConfigurationValueKey.EnhancedSelection];

            if (asBoolean(enhancedSelection)) {
                attributes.enhanceForLongLists = true;
            }

            return attributes;
        },

        /** Any additional attributes that will be assigned to the check box list control */
        checkBoxListConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const repeatColumnsConfig = this.configurationValues[ConfigurationValueKey.RepeatColumns];
            const repeatDirection = this.configurationValues[ConfigurationValueKey.RepeatDirection];

            if (repeatColumnsConfig) {
                attributes["repeatColumns"] = toNumberOrNull(repeatColumnsConfig) || 0;
            }

            if (repeatDirection !== "Vertical") {
                attributes["horizontal"] = true;
            }

            return attributes;
        },

        /** Is the control going to be list box? */
        isListBox(): boolean {
            const enhancedSelection = this.configurationValues[ConfigurationValueKey.EnhancedSelection];

            return asBoolean(enhancedSelection);
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue.join(","));
        },

        modelValue: {
            immediate: true,
            handler() {
                const value = this.modelValue || "";

                this.internalValue = value !== "" ? value.split(",") : [];
            }
        }
    },

    template: `
<ListBox v-if="isListBox" v-model="internalValue" v-bind="listBoxConfigAttributes" :options="options" />
<CheckBoxList v-else v-model="internalValue" v-bind="checkBoxListConfigAttributes" :options="options" />
`
});
