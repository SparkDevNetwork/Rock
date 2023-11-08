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
import PagePicker from "@Obsidian/Controls/pagePicker.obs";
import { PageRouteValueBag } from "@Obsidian/ViewModels/Rest/Controls/pageRouteValueBag";

export const EditComponent = defineComponent({
    name: "PageReferenceField.Edit",

    components: {
        PagePicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the picker.
        const internalValue = ref<PageRouteValueBag>({});

        // Watch for changes from the parent component and update the picker.
        watch(() => props.modelValue, () => {
            internalValue.value = JSON.parse(props.modelValue || "{}");
        }, {
            immediate: true
        });

        // Watch for changes from the picker and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue
        };
    },

    template: `
    <PagePicker v-model="internalValue" :multiple="false" promptForPageRoute showSelectCurrentPage />
`
});

export const ConfigurationComponent = defineComponent({
    name: "PageReferenceField.Configuration",

    template: ``
});