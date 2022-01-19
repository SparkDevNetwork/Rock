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
import DropDownList from "../Elements/dropDownList";
import RadioButtonList from "../Elements/radioButtonList";
import { toNumberOrNull } from "../Services/number";
import { ConfigurationValueKey } from "./singleSelectField";
import { ListItem } from "../ViewModels";

export const EditComponent = defineComponent({
    name: "SingleSelectField.Edit",

    components: {
        DropDownList,
        RadioButtonList
    },

    props: getFieldEditorProps(),

    setup() {
        return {
            isRequired: inject("isRequired") as boolean
        };
    },

    data() {
        return {
            internalValue: ""
        };
    },

    computed: {
        /** The options to choose from in the drop down list */
        options(): ListItem[] {
            try {
                const providedOptions = JSON.parse(this.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItem[];

                if (this.isRadioButtons && !this.isRequired) {
                    providedOptions.unshift({
                        text: "None",
                        value: ""
                    });
                }

                return providedOptions;
            }
            catch {
                return [];
            }
        },

        /** Any additional attributes that will be assigned to the drop down list control */
        ddlConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const fieldTypeConfig = this.configurationValues[ConfigurationValueKey.FieldType];

            if (fieldTypeConfig === "ddl_enhanced") {
                attributes.enhanceForLongLists = true;
            }

            return attributes;
        },

        /** Any additional attributes that will be assigned to the radio button control */
        rbConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const repeatColumnsConfig = this.configurationValues[ConfigurationValueKey.RepeatColumns];

            if (repeatColumnsConfig) {
                attributes["repeatColumns"] = toNumberOrNull(repeatColumnsConfig) || 0;
            }

            return attributes;
        },

        /** Is the control going to be radio buttons? */
        isRadioButtons(): boolean {
            const fieldTypeConfig = this.configurationValues[ConfigurationValueKey.FieldType];
            return fieldTypeConfig === "rb";
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },

        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue || "";
            }
        }
    },

    template: `
<RadioButtonList v-if="isRadioButtons" v-model="internalValue" v-bind="rbConfigAttributes" :options="options" horizontal />
<DropDownList v-else v-model="internalValue" v-bind="ddlConfigAttributes" :options="options" />
`
});
