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
import BaseAsyncPicker from "@Obsidian/Controls/baseAsyncPicker.obs";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue, useVModelPassthrough } from "@Obsidian/Utility/component";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { PickerDisplayStyle } from "@Obsidian/Enums/Controls/pickerDisplayStyle";
import { UniversalItemValuePickerDisplayStyle } from "@Obsidian/Enums/Controls/universalItemValuePickerDisplayStyle";
import { DataEntryMode } from "@Obsidian/Utility/fieldTypes";

export const EditComponent = defineComponent({
    name: "UniversalItemPickerField.Edit",

    components: {
        BaseAsyncPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(getModelValue());
        const isRequired = inject<boolean>("isRequired") ?? false;

        /** The options to choose from in the drop down list */
        const items = computed((): ListItemBag[] => {
            try {
                const providedOptions = JSON.parse(props.configurationValues["items"] ?? "[]") as ListItemBag[];

                if (!isMultiple && !isRequired) {
                    providedOptions.unshift({
                        text: "None",
                        value: ""
                    });
                }

                return providedOptions;
            }
            catch {
                return [];
            }
        });

        const displayStyle = computed((): PickerDisplayStyle => {
            const mode = toNumberOrNull(props.configurationValues["displayStyle"]) ?? 0;

            if (mode === UniversalItemValuePickerDisplayStyle.List) {
                return PickerDisplayStyle.List;
            }
            else if (mode === UniversalItemValuePickerDisplayStyle.Condensed) {
                return PickerDisplayStyle.Condensed;
            }
            else {
                return PickerDisplayStyle.Auto;
            }
        });

        const enhanceForLongLists = computed((): boolean => {
            return asBoolean(props.configurationValues["enhanceForLongLists"]);
        });

        const columnCount = computed((): number => {
            return toNumberOrNull(props.configurationValues["columnCount"]) ?? 0;
        });

        const isMultiple = computed((): boolean => {
            return asBoolean(props.configurationValues["isMultiple"]);
        });

        function getModelValue(): ListItemBag | ListItemBag[] | null {
            try {
                return JSON.parse(props.modelValue) as ListItemBag | ListItemBag[];
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
            columnCount,
            displayStyle,
            enhanceForLongLists,
            internalValue,
            isMultiple,
            isRequired,
            items,
        };
    },

    template: `
<BaseAsyncPicker v-model="internalValue"
                 :items="items"
                 :isRequired="isRequired"
                 :multiple="isMultiple"
                 :enhanceForLongLists="enhanceForLongLists"
                 :columnCount="columnCount"
                 :displayStyle="displayStyle"
                 showBlankItem />
`
});

export const FilterComponent = defineComponent({
    name: "UniversalItemPickerField.Filter",

    components: {
        EditComponent
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);
        const dataEntryMode = computed((): DataEntryMode => props.dataEntryMode);

        const configurationValues = computed((): Record<string, string> => {
            const values = {...props.configurationValues};

            // Invert the multiple state for the filter component.
            if (asBoolean(values["isMultiple"])) {
                values["isMultiple"] = "false";
            }
            else {
                values["isMultiple"] = "true";
            }

            return values;
        });

        return {
            configurationValues,
            dataEntryMode,
            internalValue
        };
    },

    template: `
<EditComponent v-model="internalValue" :configurationValues="configurationValues" :dataEntryMode="dataEntryMode" />
`
});
