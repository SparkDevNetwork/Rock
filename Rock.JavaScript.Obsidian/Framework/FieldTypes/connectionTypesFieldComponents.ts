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
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./noteTypesField.partial";
import { getFieldEditorProps } from "./utils";

export const EditComponent = defineComponent({
    name: "ConnectionTypesField.Edit",

    components: {
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
            internalValue: this.modelValue ? this.modelValue.split(",") : []
        };
    },

    computed: {
        options(): ListItemBag[] {
            try {
                const valuesConfig = JSON.parse(this.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
                return valuesConfig.map(v => {
                    return {
                        text: v.text,
                        value: v.value
                    } as ListItemBag;
                });
            }
            catch {
                return [];
            }
        },

        repeatColumns(): number {
            return Number(this.configurationValues[ConfigurationValueKey.RepeatColumns]) ?? 1;
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue.join(","));
        }
    },

    template: `
<CheckBoxList v-model="internalValue" :items="options" :repeatColumns="repeatColumns" horizontal />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ConnectionTypeField.Configuration",
    template: `
`
});
