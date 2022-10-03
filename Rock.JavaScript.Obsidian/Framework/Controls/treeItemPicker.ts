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
import RockButton from "./rockButton";
import RockFormField from "./rockFormField";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ITreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
import { TreeItemBag } from "@Obsidian/ViewModels/Utility/treeItemBag";
import TreeList from "./treeList";
import { updateRefValue } from "@Obsidian/Utility/component";

/**
 * Helper function to flatten an array of items that contains child items
 * of the same type.
 *
 * @param source The source array of items to the flattened.
 * @param childrenSource A callback function that retrieves the child items.
 *
 * @returns An array of all items and descendants.
 */
function flatten<T>(source: T[], childrenSource: (value: T) => T[]): T[] {
    let stack = [...source];
    const flatArray: T[] = [];

    for (let i = 0; i < stack.length; i++) {
        const item = stack[i];

        flatArray.push(item);

        stack = stack.concat(childrenSource(item));
    }

    return flatArray;
}

/**
 * Convert a single item to an array of one item. If the value is already an
 * array then it is just returned as is.
 *
 * @param value The value from the parent component.
 *
 * @returns The value trimmed down to just the actual selection value.
 */
function forceToArray(value: ListItemBag | ListItemBag[] | undefined | null, multiple: boolean): ListItemBag[] {
    if (value === undefined || value === null) {
        return [];
    }
    else if (Array.isArray(value)) {
        if (!multiple && value.length > 1) {
            return [value[0]];
        }
        else {
            return value;
        }
    }
    else {
        return [value];
    }
}

export default defineComponent({
    name: "TreeItemPicker",

    components: {
        RockButton,
        RockFormField,
        TreeList
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        items: {
            type: Array as PropType<TreeItemBag[]>
        },

        provider: {
            type: Object as PropType<ITreeItemProvider>
        },

        iconCssClass: {
            type: String as PropType<string>,
            default: "fa fa-folder-open"
        },

        disableFolderSelection: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * Automatically expand parents who have (sub)children that are selected
         */
        autoExpand: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true,
        "valueSelected": () => true
    },

    setup(props, { emit }) {
        /**
         * Our internal list of selected values. This must be kept seperate
         * because we don't actually emit the new values until the user clicks
         * the select button.
         */
        const internalValues = ref(forceToArray(props.modelValue, props.multiple).map(v => v.value ?? ""));

        /**
         * A flat array of items from the tree. This is used to quickly filter
         * to just the selected items.
         */
        const flatItems = ref<TreeItemBag[]>(flatten(props.items ?? [], i => i.children ?? []));

        /** Will contain the value true if the popup tree list should be shown. */
        const showPopup = ref(false);

        /** Determines if the clear button should be shown. */
        const showClear = computed((): boolean => {
            // Use modelValue since internalValues is used for the in-process
            // popup, not the actual stored value.
            return forceToArray(props.modelValue, props.multiple).length > 0;
        });

        /**
         * Determines the names of the currently selected items. This shows the
         * names of the "save safe" items, meaning it is updated after the user
         * clicks the select button. It does not update on the fly as they are
         * selecting items.
         */
        const selectedNames = computed((): string => {
            return forceToArray(props.modelValue, true).map(v => v.text).join(", ");
        });

        /** The CSS class to use for the picker icon. */
        const pickerIconClass = computed((): string => `${props.iconCssClass} fa-fw`);

        /** Updates the model value from our internal value. */
        const updateModelValue = (): void => {
            // Create a new set of selected items to emit.
            const newModelValue = forceToArray(props.modelValue, true)
                .filter(v => internalValues.value.includes(v.value ?? ""));

            // Helpful list of the values already in the new model value.
            const knownValues = newModelValue.map(v => v.value);

            // Determine which values need to be added from the tree list.
            const additionalValues = internalValues.value
                .filter(v => !knownValues.includes(v));

            // Go through each additional value and find it in the tree list
            // and add it to the new model value.
            for (const v of additionalValues) {
                const items = flatItems.value.filter(i => i.value === v);

                if (items.length > 0 && items[0].value && items[0].text) {
                    newModelValue.push({
                        value: items[0].value,
                        text: items[0].text
                    });
                }
            }

            // Emit the new value and close the popup.
            if (props.multiple) {
                emit("update:modelValue", newModelValue);
            }
            else {
                emit("update:modelValue", newModelValue.length > 0 ? newModelValue[0] : null);
            }
        };

        /**
         * Event handler for when the list of items in the tree list has been
         * updated.
         *
         * @param newItems The new root items being used by the tree list.
         */
        const onUpdateItems = (newItems: TreeItemBag[]): void => {
            // Update our flatItems array with the list of new items.
            flatItems.value = flatten(newItems ?? [], i => i.children ?? []);
        };

        /**
         * Event handler for when the clear button is clicked by the user.
         */
        const onClear = (): void => {
            emit("update:modelValue", props.multiple ? [] : null);
        };

        /**
         * Event handler for when the user clicks on the picker. Show/hide the
         * popup.
         */
        const onPickerClick = (): void => {
            showPopup.value = !showPopup.value;
        };

        /**
         * Event handler for when the user clicks the cancel button. Hide the
         * popup.
         */
        const onCancel = (): void => {
            showPopup.value = false;
        };

        /**
         * Event handler for when the user clicks the select button. Save the
         * current selection and close the popup.
         */
        const onSelect = (): void => {
            updateModelValue();
            showPopup.value = false;
            emit("valueSelected"); // inform that a value was selected, even if it didn't change
        };

        // Watch for changes to the selected values from the parent control and
        // update our internal values to match.
        watch([() => props.modelValue, () => props.multiple], (oldValues, newValues) => {
            updateRefValue(internalValues, forceToArray(props.modelValue, props.multiple).map(v => v.value ?? ""));

            // If the "multiple" property changed, force update the model value.
            if (newValues[1] !== oldValues[1]) {
                updateModelValue();
            }
        });

        return {
            internalValues,
            onCancel,
            onClear,
            onPickerClick,
            onSelect,
            onUpdateItems,
            pickerIconClass,
            selectedNames,
            showClear,
            showPopup
        };
    },

    template: `
<RockFormField
    :modelValue="modelValue"
    formGroupClasses="category-picker"
    name="itempicker">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <v-style>
                .scrollbar-thin {
                    scrollbar-width: thin;
                }
                .scrollbar-thin::-webkit-scrollbar {
                    width: 8px;
                    background-color: #bbb;
                }
            </v-style>

            <div class="picker picker-select rollover-container">
                <a class="picker-label" href="#" @click.prevent.stop="onPickerClick">
                    <i :class="pickerIconClass"></i>
                    <span class="selected-names">{{ selectedNames }}</span>
                    <b class="fa fa-caret-down pull-right"></b>
                </a>

                <a v-if="showClear" class="picker-select-none" @click.prevent.stop="onClear">
                    <i class="fa fa-times"></i>
                </a>

                <div v-show="showPopup" class="picker-menu dropdown-menu" style="display: block;">
                    <div class="scrollbar-thin" style="height: 200px; overflow-y: scroll; overflow-x: hidden;">
                        <TreeList v-model="internalValues" :multiple="multiple" :items="items" :provider="provider" @update:items="onUpdateItems" :disableFolderSelection="disableFolderSelection" :autoExpand="autoExpand" />
                    </div>

                    <div class="picker-actions">
                        <a class="btn btn-xs btn-primary picker-btn" @click.prevent.stop="onSelect">Select</a>
                        <a class="btn btn-xs btn-link picker-cancel" @click.prevent.stop="onCancel">Cancel</a>
                        <div v-if="$slots.customPickerActions" class="pull-right">
                            <slot name="customPickerActions" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </template>
</RockFormField>
`
});
