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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import { ConfigurationValueKey } from "./persistedDatasetField.partial";
import DropDownList  from "@Obsidian/Controls/dropDownList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "PersistedDatasetField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the dropdown list.
        const internalValue = ref("");

        // PersistedDatasets from server to choose from.
        const options = computed((): ListItemBag[] => {
            const options = JSON.parse(props.configurationValues[ConfigurationValueKey.Values] || "[]") as ListItemBag[];
            return options;
        });

        // Watch for changes from the parent component and update the dropdown list.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        }, {
            immediate: true
        });

        // Watch for changes from the dropdown list and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            options
        };
    },

    template: `
    <DropDownList v-model="internalValue" :items="options" showBlankItem />
`
});

export const ConfigurationComponent = defineComponent({
    name: "PersistedDatasetField.Configuration",

    template: ``
});
