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
import { getFieldEditorProps } from "./utils";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./remoteAuthsField.partial";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "RemoteAuthsField.Edit",

    components: {
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref<string[]>([]);

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        // Watch for changes from the parent component and update the client UI.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? props.modelValue.split(",") : []);
        }, {
            immediate: true
        });

        // Watch for changes from the client UI and update the parent component.
        watch(() => internalValue.value, () => {
            emit("update:modelValue", internalValue.value.join(","));
        });

        return {
            internalValue,
            options
        };
    },

    template: `
    <CheckBoxList v-model="internalValue" :items="options" :horizontal="false" />
`
});


export const ConfigurationComponent = defineComponent({
    name: "RemoteAuthsField.Configuration",

    template: ``
});