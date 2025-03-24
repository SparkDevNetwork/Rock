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

import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationValueKey } from "./noteTypeField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";


export const EditComponent = defineComponent({
    name: "StepProgramField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

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
        }
    },

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string>("");

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue || "";
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue
        };
    },

    template: `<DropDownList v-model="internalValue" :items="options" />`
});

export const ConfigurationComponent = defineComponent({
    name: "StepProgramField.Configuration",
    template: ``
});