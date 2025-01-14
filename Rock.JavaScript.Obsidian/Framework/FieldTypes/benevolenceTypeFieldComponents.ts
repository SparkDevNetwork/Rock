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
import { defineComponent, computed } from "vue";
import { getFieldEditorProps } from "./utils";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "BenevolenceTypeField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    emit: {
        "update:modelValue": (_value: string) => true
    },

    setup(props, { emit }) {
        const internalValue = computed<string>({
            get() {
                return props.modelValue;
            },
            set(newVal: string) {
                emit("update:modelValue", newVal ?? "");
            }
        });

        const items = computed(() => {
            try {
                return JSON.parse(props.configurationValues.values ?? "") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        return { internalValue, items };
    },

    template: `<DropDownList v-model="internalValue" :items="items" :showBlankItem="true" />`
});

export const ConfigurationComponent = defineComponent({
    name: "BenevolenceTypeField.Configuration",

    template: ``
});