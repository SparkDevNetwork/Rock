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
import MobileNavigationActionEditor from "@Obsidian/Controls/Internal/mobileNavigationActionEditor.obs";
import { getFieldEditorProps } from "./utils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "MobileNavigationAction.Edit",

    components: {
        MobileNavigationActionEditor
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as ListItemBag);

        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, { immediate: true });

        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue
        };
    },

    template: `
<MobileNavigationActionEditor v-model="internalValue" />
`
});


// Empty Configuration
export const ConfigurationComponent = defineComponent({
    name: "MobileNavigationAction.Configuration",
    template: ``
});
