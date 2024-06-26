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
import { computed, defineComponent, inject, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import UniversalItemSearchPicker from "@Obsidian/Controls/Internal/universalItemSearchPicker.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";

export const EditComponent = defineComponent({
    name: "UniversalItemSearchPickerField.Edit",

    components: {
        UniversalItemSearchPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(getModelValue());
        const isRequired = inject<boolean>("isRequired") ?? false;

        const areDetailsAlwaysVisible = computed((): boolean => {
            return asBoolean(props.configurationValues["areDetailsAlwaysVisible"]);
        });

        const isIncludeInactiveVisible = computed((): boolean => {
            return asBoolean(props.configurationValues["isIncludeInactiveVisible"]);
        });

        const iconCssClass = computed((): string | undefined => {
            return props.configurationValues["iconCssClass"] ?? undefined;
        });

        const searchUrl = computed((): string => {
            return props.configurationValues["searchUrl"];
        });

        function getModelValue(): ListItemBag | null {
            try {
                return JSON.parse(props.modelValue) as ListItemBag;
            }
            catch {
                return null;
            }
        }

        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, getModelValue());
        });

        return {
            areDetailsAlwaysVisible,
            iconCssClass,
            isIncludeInactiveVisible,
            internalValue,
            isRequired,
            searchUrl
        };
    },

    template: `
<UniversalItemSearchPicker v-model="internalValue"
                           :areDetailsAlwaysVisible="areDetailsAlwaysVisible"
                           :iconCssClass="iconCssClass"
                           :isIncludeInactiveVisible="isIncludeInactiveVisible"
                           :isRequired="isRequired"
                           :searchUrl="searchUrl" />
`
});

export const FilterComponent = EditComponent;