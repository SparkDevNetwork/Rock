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
import LavaCommandPicker from "@Obsidian/Controls/lavaCommandPicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "LavaCommandsField.Edit",

    components: {
        LavaCommandPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the picker.
        const internalValue = ref<ListItemBag[]>([]);

        // Watch for changes from the parent component and update the picker.
        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "[]") as ListItemBag[];
        }, {
            immediate: true
        });

        // Watch for changes from the picker and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
        };
    },

    template: `
    <LavaCommandPicker label="Lava Command" v-model="internalValue" multiple />
`
});

export const ConfigurationComponent = defineComponent({
    name: "LavaCommandsField.Configuration",

    template: ``
});