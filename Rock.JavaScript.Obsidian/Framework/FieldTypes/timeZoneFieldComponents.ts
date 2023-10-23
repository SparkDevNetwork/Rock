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
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./timeZoneField.partial";

export const EditComponent = defineComponent({
    name: "TimeZoneField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref("");

        const timeZones = computed((): ListItemBag[] => {
            return JSON.parse(props.configurationValues[ConfigurationValueKey.TimeZones] || "[]");
        });

        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue || "";
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            timeZones
        };
    },

    template: `
    <DropDownList v-model="internalValue" :items="timeZones" :showBlankItem="true" />
`
});


export const ConfigurationComponent = defineComponent({
    name: "TimeZoneField.Configuration",

    template: ``
});