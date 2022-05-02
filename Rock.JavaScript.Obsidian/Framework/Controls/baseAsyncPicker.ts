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
import { updateRefValue } from "@Obsidian/Utility/component";
import { isPromise } from "@Obsidian/Utility/promiseUtils";
import { useSuspense } from "@Obsidian/Utility/suspense";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ControlLazyMode, ControlLazyModeType } from "@Obsidian/Types/Controls/controlLazyMode";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import DropDownList from "./dropDownList";

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
        DropDownList
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

        showBlankItem: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        lazyMode: {
            type: String as PropType<ControlLazyModeType>,
            default: ControlLazyMode.OnDemand
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        items: {
            type: Object as PropType<ListItemBag[] | Promise<ListItemBag[]> | (() => ListItemBag[] | Promise<ListItemBag[]>) | null>,
            required: false
        }
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        // #region Values

        const internalValue = ref(modelValueToInternalValue(props.modelValue, props.multiple));
        const loadedItems = ref<ListItemBag[] | null>(null);
        const isLoading = ref(false);
        const hasPickerBeenOpened = ref(false);

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
        };

        // #endregion

        // #region Event Handlers

        /**
         * Called any time the drop down opens. Check if we need to lazy load
         * our items.
         */
        const onOpen = (): void => {
            hasPickerBeenOpened.value = true;
            // If we haven't loaded our dynamic options yet then start loading.
            if (loadedItems.value === null && !isLoading.value) {
                loadItems(true);
            }
        };

        // #endregion

        watch(() => props.items, () => {
            // Only perform eager loading if we are not on-demand loading or
            // if we have already been opened once.
            loadItems(props.lazyMode !== ControlLazyMode.OnDemand || hasPickerBeenOpened.value);
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

        if (props.lazyMode === ControlLazyMode.Lazy) {
            // Eager loading in this case means go ahead and load everything,
            // but we aren't going to wait around for it.
            loadItems(true);
        }
        else if (props.lazyMode === ControlLazyMode.Eager) {
            const suspense = useSuspense();

            if (suspense) {
                suspense.addOperation(loadItems(true));
            }
            else {
                loadItems(true);
            }
        }

        return {
            actualItems,
            internalValue,
            isLoading,
            onOpen
        };
    },

    template: `
<DropDownList v-model="internalValue"
    :enhanceForLongLists="enhanceForLongLists"
    :grouped="grouped"
    :loading="isLoading"
    :multiple="multiple"
    :items="actualItems"
    :showBlankItem="showBlankItem"
    @open="onOpen" />
`
});

