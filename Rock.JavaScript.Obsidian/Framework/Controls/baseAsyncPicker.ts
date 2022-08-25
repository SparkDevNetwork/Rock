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
import { standardAsyncPickerProps, updateRefValue, useStandardRockFormFieldProps } from "@Obsidian/Utility/component";
import { isPromise } from "@Obsidian/Utility/promiseUtils";
import { useSuspense } from "@Obsidian/Utility/suspense";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ControlLazyMode } from "@Obsidian/Types/Controls/controlLazyMode";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import CheckBoxList from "./checkBoxList";
import DropDownList from "./dropDownList";
import RadioButtonList from "./radioButtonList";
import { PickerDisplayStyle } from "@Obsidian/Types/Controls/pickerDisplayStyle";

/**
 * Convert a model value to the internal value. Basically, this extracts the
 * value from a ListItemBag and also ensures correct array/non-array state
 * depending on the isMultiple value.
 *
 * @param value The modelValue from the parent component.
 * @param isMultiple True if the output value should be an array; otherwise false.
 *
 * @returns The value trimmed down to just the actual selection value.
 */
function modelValueToInternalValue(value: ListItemBag | ListItemBag[] | undefined | null, isMultiple: boolean): string | string[] {
    if (value === undefined || value === null) {
        return isMultiple ? [] : "";
    }
    else if (Array.isArray(value)) {
        return value.map(v => v.value ?? "");
    }
    else {
        return value.value ?? "";
    }
}

export default defineComponent({
    name: "BaseAsyncPicker",

    components: {
        CheckBoxList,
        DropDownList,
        RadioButtonList
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        grouped: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        items: {
            type: Object as PropType<ListItemBag[] | Promise<ListItemBag[]> | (() => ListItemBag[] | Promise<ListItemBag[]>) | null>,
            required: false
        },

        ...standardAsyncPickerProps
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        // #region Values

        const internalValue = ref(modelValueToInternalValue(props.modelValue, props.multiple));
        const loadedItems = ref<ListItemBag[] | null>(null);
        const isLoading = ref(false);
        const hasLoadedItems = ref(false);
        const standardProps = useStandardRockFormFieldProps(props);

        // #endregion

        // #region Computed Values

        /**
         * Initial items is used to make sure that our currently selected value
         * is considered valid and not erased automatically before our real
         * list of items is available.
         */
        const initialItems = computed((): ListItemBag[] => {
            // Take the modelValue and force it to be an array. If we allow
            // multiple selection then just ensure its in an array format.
            // If we don't support multiple selection then also make sure
            // it doesn't contain more than one value.
            if (props.multiple) {
                if (Array.isArray(props.modelValue)) {
                    return props.modelValue;
                }
                else if (props.modelValue) {
                    return [props.modelValue];
                }
                else {
                    return [];
                }
            }
            else {
                if (Array.isArray(props.modelValue)) {
                    return [props.modelValue[0]];
                }
                else if (props.modelValue) {
                    return [props.modelValue];
                }
                else {
                    return [];
                }
            }
        });

        /**
         * The actual items that are available to the drop down. This is either
         * the loaded items or the initial items.
         */
        const actualItems = computed((): ListItemBag[] => {
            return loadedItems.value ?? initialItems.value;
        });

        const isDropDownListStyle = computed((): boolean => {
            return props.displayStyle === PickerDisplayStyle.Condensed || props.displayStyle === PickerDisplayStyle.Auto;
        });

        const isCheckBoxListStyle = computed((): boolean => {
            return props.displayStyle === PickerDisplayStyle.List && props.multiple;
        });

        const isRadioButtonListStyle = computed((): boolean => {
            return props.displayStyle === PickerDisplayStyle.List && !props.multiple;
        });

        const isHorizontal = computed((): boolean => {
            return props.columnCount != 1;
        });

        // #endregion

        // #region Functions

        /**
         * Loads the items from the propery and sets the loadedItems value. This
         * function handles the cases of items property being either a Promise or
         * a Function and all the necessary deconstruction.
         */
        const loadItems = async (eagerLoading: boolean): Promise<void> => {
            let items: ListItemBag[] | Promise<ListItemBag[]> | (() => ListItemBag[] | Promise<ListItemBag[]>) | null = props.items ?? null;

            if (items === null) {
                loadedItems.value = null;
                return;
            }

            if (typeof items === "function") {
                if (!eagerLoading) {
                    return;
                }

                items = items();
            }

            if (isPromise(items)) {
                isLoading.value = true;
                items = await items;
                isLoading.value = false;
            }

            loadedItems.value = items;
            hasLoadedItems.value = true;
        };

        // #endregion

        // #region Event Handlers

        /**
         * Called any time the drop down opens. Check if we need to lazy load
         * our items.
         */
        const onOpen = (): void => {
            // If we haven't loaded our dynamic options yet then start loading.
            if (loadedItems.value === null && !isLoading.value) {
                loadItems(true);
            }
        };

        // #endregion

        watch(() => props.items, () => {
            // Only perform eager loading if we are not on-demand loading or
            // if we have already been loaded items previously.
            loadItems(props.lazyMode !== ControlLazyMode.OnDemand || hasLoadedItems.value);
        });

        watch(() => props.displayStyle, () => {
            if (hasLoadedItems.value) {
                return;
            }

            if (isCheckBoxListStyle.value || isRadioButtonListStyle.value) {
                loadItems(true);
            }
        });

        watch([() => props.modelValue, () => props.multiple], () => {
            updateRefValue(internalValue, modelValueToInternalValue(props.modelValue, props.multiple));
        });

        watch(internalValue, () => {
            if (Array.isArray(internalValue.value)) {
                const selectedValues = internalValue.value;
                const newValue = actualItems.value.filter(o => selectedValues.some(v => v === o.value));

                emit("update:modelValue", newValue);
            }
            else {
                const selectedValue = internalValue.value;
                const newValue = actualItems.value.filter(o => selectedValue === o.value);

                emit("update:modelValue", newValue.length > 0 ? newValue[0] : null);
            }
        });

        if (Array.isArray(props.items)) {
            // If we have an array of items, then just load it because there
            // won't be any delay.
            loadItems(true);
        }
        else if (props.lazyMode === ControlLazyMode.Eager || !isDropDownListStyle.value) {
            // A radio list or checkbox list both require eager loading.
            const suspense = useSuspense();

            if (suspense) {
                suspense.addOperation(loadItems(true));
            }
            else {
                loadItems(true);
            }
        }
        else if (props.lazyMode === ControlLazyMode.Lazy) {
            // Eager loading in this case means go ahead and load everything,
            // but we aren't going to wait around for it.
            loadItems(true);
        }

        return {
            actualItems,
            internalValue,
            isCheckBoxListStyle,
            isDropDownListStyle,
            isHorizontal,
            isLoading,
            isRadioButtonListStyle,
            onOpen,
            standardProps
        };
    },

    template: `
<DropDownList v-if="isDropDownListStyle"
    v-model="internalValue"
    v-bind="standardProps"
    :grouped="grouped"
    :loading="isLoading"
    :items="actualItems"
    :multiple="multiple"
    :showBlankItem="showBlankItem"
    :enhanceForLongLists="enhanceForLongLists"
    :lazyMode="lazyMode"
    displayStyle="auto"
    @open="onOpen" />

<CheckBoxList v-if="isCheckBoxListStyle"
    v-model="internalValue"
    v-bind="standardProps"
    :horizontal="isHorizontal"
    :items="actualItems"
    :repeatColumns="columnCount" />

<RadioButtonList v-if="isRadioButtonListStyle"
    v-model="internalValue"
    v-bind="standardProps"
    :horizontal="isHorizontal"
    :items="actualItems"
    :repeatColumns="columnCount"
    :showBlankItem="showBlankItem" />
`
});
