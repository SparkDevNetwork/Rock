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
import { computed, defineComponent, PropType, ref, watch } from "vue";
import VueSelect from "vue-select";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import RockFormField from "./rockFormField";
import { isPromise } from "@Obsidian/Utility/promiseUtils";
import { deepEqual } from "@Obsidian/Utility/util";

const specialGroupValueTag = "THIS IS A GROUP AND NOT AN OPTION";

type OptionGroup = {
    text: string;

    options: ListItemBag[];
};

const deselectComponent = defineComponent({
    template: `
<i class="fa fa-times"></i>
`
});

export default defineComponent({
    name: "DropDownList",

    components: {
        RockFormField,
        VueSelect
    },

    props: {
        modelValue: {
            type: Object as PropType<string | string[]>,
            required: true
        },

        options: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        optionsSource: {
            type: Function as PropType<(() => ListItemBag[]) | (() => Promise<ListItemBag[]>)>,
            required: false
        },

        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        blankValue: {
            type: String as PropType<string>,
            default: ""
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        formGroupClasses: {
            type: String as PropType<string>,
            default: ""
        },

        /** No longer used. */
        formControlClasses: {
            type: String as PropType<string>,
            default: ""
        },

        enhanceForLongLists: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        grouped: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue ? props.modelValue : null);
        const isLoading = ref(false);
        const loadedOptions = ref(props.options);

        const computedShowBlankItem = computed((): boolean => {
            return !props.multiple && props.showBlankItem;
        });

        const computedOptions = computed((): ListItemBag[] => {
            if (props.grouped) {
                const groupedOptions: ListItemBag[] = [];
                const groups: OptionGroup[] = [];

                for (const o of loadedOptions.value) {
                    if (!o.category) {
                        groupedOptions.push(o);
                        continue;
                    }

                    const matchedGroups = groups.filter(g => g.text == o.category);

                    if (matchedGroups.length >= 1) {
                        matchedGroups[0].options.push(o);
                    }
                    else {
                        groups.push({
                            text: o.category,
                            options: [o]
                        });
                    }
                }

                for (const g of groups) {
                    groupedOptions.push({
                        value: specialGroupValueTag,
                        text: g.text
                    });

                    groupedOptions.push(...g.options);
                }

                return groupedOptions;
            }

            return loadedOptions.value;
        });

        const isClearable = computed((): boolean => {
            return computedShowBlankItem.value && !isLoading.value;
        });

        const isDisabled = computed((): boolean => {
            return isLoading.value;
        });

        /**
         * Synchronizes our internal value with the modelValue and current
         * component property values.
         */
        const syncInternalValue = (): void => {
            let value: string | string[] | null = props.modelValue;

            // Note: Even though we are converting between single and multiple
            // value types, this is only for our benefit on initial load. When
            // the multiple flag changes, the vue-select component clears the
            // current selection anyway.
            if (props.multiple) {
                if (!Array.isArray(value)) {
                    value = value === "" ? [] : [value];
                }

                value = value.filter(v => !!loadedOptions.value.find(o => o.value === v));
            }
            else {
                if (Array.isArray(value)) {
                    value = value.length === 0 ? null : value[0];
                }

                if (value === null) {
                    value = computedShowBlankItem.value
                        ? props.blankValue
                        : (loadedOptions.value[0]?.value || props.blankValue);
                }

                const selectedOption = loadedOptions.value.find(o => o.value === value) || null;

                if (!selectedOption) {
                    value = computedShowBlankItem.value
                        ? props.blankValue
                        : (loadedOptions.value[0]?.value || props.blankValue);
                }
            }

            if (!deepEqual(value, internalValue.value, true)) {
                internalValue.value = value;
            }
        };

        const isItemSelectable = (item: ListItemBag): boolean => {
            return !(props.grouped && item.value === specialGroupValueTag);
        };

        const filterItem = (item: ListItemBag, label: string | undefined | null, search: string): boolean => {
            if (props.grouped && item.value === specialGroupValueTag) {
                return false;
            }

            return (label || "").toLocaleLowerCase().indexOf(search.toLocaleLowerCase()) > -1;
        };

        const loadOptionsFromSource = async (): Promise<void> => {
            if (props.optionsSource) {
                isLoading.value = true;

                try {
                    let options = props.optionsSource();

                    if (isPromise(options)) {
                        options = await options;
                    }

                    loadedOptions.value = options;
                }
                finally {
                    isLoading.value = false;
                }
            }
        };

        watch([loadedOptions, () => props.modelValue, computedShowBlankItem, () => props.multiple], () => {
            syncInternalValue();
        });

        watch(() => props.options, () => {
            if (!props.optionsSource) {
                loadedOptions.value = props.options;
            }
        });

        watch(internalValue, () => {
            let newValue = internalValue.value;

            // Note: Even though we are converting between single and multiple
            // value types, this is only for our benefit on initial load. When
            // the multiple flag changes, the vue-select component clears the
            // current selection anyway.
            if (props.multiple) {
                if (!Array.isArray(newValue)) {
                    newValue = newValue === null ? [] : [newValue];
                }
            }
            else {
                if (Array.isArray(newValue)) {
                    newValue = newValue.length === 0 ? null : newValue[0];
                }

                if (newValue === null) {
                    newValue = computedShowBlankItem.value
                        ? props.blankValue
                        : (loadedOptions.value[0]?.value || props.blankValue);
                }
            }

            if (!deepEqual(props.modelValue, newValue, true)) {
                emit("update:modelValue", newValue);
            }
        });

        if (props.optionsSource) {
            loadOptionsFromSource();
        }
        else {
            syncInternalValue();
        }

        return {
            computedOptions,
            filterItem,
            internalValue,
            isClearable,
            isDisabled,
            isItemSelectable,
            isLoading,
            reduceItem: (item: ListItemBag) => item.value,
            selectComponents: {
                Deselect: deselectComponent
            }
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    :formGroupClasses="'rock-drop-down-list ' + formGroupClasses"
    name="dropdownlist">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <VueSelect :inputId="uniqueId"
                v-model="internalValue"
                v-bind="field"
                class="form-control"
                label="text"
                :multiple="multiple"
                :options="computedOptions"
                :reduce="reduceItem"
                :clearable="isClearable"
                :searchable="enhanceForLongLists"
                :selectable="isItemSelectable"
                :filterBy="filterItem"
                :disabled="isDisabled"
                :loading="isLoading"
                :components="selectComponents">
                <template #open-indicator>
                    <i class="fa fa-caret-down"></i>
                </template>
            </VueSelect>
        </div>
    </template>
</RockFormField>`
});
