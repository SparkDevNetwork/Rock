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
import { ITreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
import { TreeItemBag } from "@Obsidian/ViewModels/Utility/treeItemBag";
import { isPromise } from "@Obsidian/Utility/promiseUtils";

/**
 * Internal helper component that renders individual tree items.
 */
const treeItem = defineComponent({
    name: "TreeList.Item",

    props: {
        modelValue: {
            type: Array as PropType<string[]>,
            default: []
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        item: {
            type: Object as PropType<TreeItemBag>,
            default: {}
        },

        disableFolderSelection: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * Automatically open to show children if a selected value is a child of this item
         */
        autoExpand: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "treeitem-expanded",
        "update:modelValue"
    ],

    setup(props, { emit }) {
        /** The list of child items to this item. */
        const children = computed((): TreeItemBag[] => props.item.children ?? []);

        /** Determines if we currently have any children to display. */
        const hasChildren = computed((): boolean => children.value.length > 0);

        /** Determines if this item is a folder that can contain children. */
        const isFolder = computed((): boolean => props.item.isFolder || props.item.hasChildren);

        /** The display name of the current item. */
        const itemName = computed((): string => props.item.text ?? "");

        /** Contains a value that indicates if the children should be shown. */
        const showChildren = ref(false);

        /** The CSS class value to use for the list-item element. */
        const listItemClass = computed((): string => {
            return isFolder.value ? "rocktree-item rocktree-folder" : "rocktree-item rocktree-leaf";
        });

        /** The CSS class value to use for the folder icon. */
        const folderClass = computed((): string => {
            return showChildren.value
                ? "rocktree-icon icon-fw fa fa-fw fa-chevron-down"
                : "rocktree-icon icon-fw fa fa-fw fa-chevron-right";
        });

        /** The CSS class value to use for the item icon. */
        const itemIconClass = computed((): string => {
            return `icon-fw ${props.item.iconCssClass}`;
        });

        /** The CSS class value to use for the item name. */
        const itemNameClass = computed((): string => {
            const classes = ["rocktree-name"];

            if (props.item.value && props.modelValue.includes(props.item.value)) {
                classes.push("selected");
            }

            if (!props.item.isActive) {
                classes.push("is-inactive");
            }

            return classes.join(" ");
        });

        // Automatically expand to show selected value deep in the tree
        watch(() => [props.item, props.modelValue], () => {
            if (!props.autoExpand || hasChildren.value == false) {
                return;
            }

            if (hasSelectedChild(props.item, props.modelValue)) {
                showChildren.value = true;
            }
            else if (children.value.length == 0) {
                showChildren.value = false;
            }
        }, { immediate: true });

        /**
         * Determine if a child item is a selected value
         *
         * @param item The item potentially with children that are selected
         * @param values The selected values
         *
         * @return Whether or not a child is selected
         */
        function hasSelectedChild(item: TreeItemBag, values: string[]): boolean {
            const children = item.children;

            if (children && children.length > 0) {
                for (const child of children) {
                    if (values.includes(child.value ?? "")) {
                        return true;
                    }

                    if (child.children && child.children.length > 0 && hasSelectedChild(child, values)) {
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Event handler for when the folder arrow is clicked.
         */
        const onExpand = (): void => {
            showChildren.value = !showChildren.value;

            if (showChildren.value) {
                emit("treeitem-expanded", props.item);
            }
        };

        /**
         * If something changes to make existing children no longer exist in the list,
         * unexpand this item
         */
        watch(hasChildren, () => {
            if (!hasChildren.value && showChildren.value) {
                onExpand();
            }
        });

        /**
         * Event handler for when a child item is expanded.
         *
         * @param item The item that was expanded.
         */
        const onChildItemExpanded = (item: TreeItemBag): void => {
            emit("treeitem-expanded", item);
        };

        /**
         * Event handler for when this item is selected or deselected.
         */
        const onSelect = (): void => {
            if (isFolder.value && props.disableFolderSelection) {
                return;
            }

            if (props.multiple) {
                if (props.item.value && !props.modelValue.includes(props.item.value)) {
                    emit("update:modelValue", [...props.modelValue, props.item.value]);
                }
                else if (props.item.value && props.modelValue.includes(props.item.value)) {
                    emit("update:modelValue", [...props.modelValue.filter(v => v !== props.item.value)]);
                }
            }
            else {
                if (props.item.value && !props.modelValue.includes(props.item.value)) {
                    emit("update:modelValue", [props.item.value]);
                }
                else if (props.item.value && props.modelValue.includes(props.item.value)) {
                    emit("update:modelValue", []);
                }
            }
        };

        /**
         * Event handler for when a child item has modified the selected values.
         *
         * @param values The new selected values.
         */
        const onUpdateSelectedValues = (values: string[]): void => {
            emit("update:modelValue", values);
        };

        return {
            children,
            hasChildren,
            folderClass,
            isFolder,
            itemIconClass,
            itemName,
            itemNameClass,
            listItemClass,
            onChildItemExpanded,
            onExpand,
            onSelect,
            onUpdateSelectedValues,
            showChildren
        };
    },

    template: `
<li :class="listItemClass">
    <i v-if="isFolder" :class="folderClass" @click.prevent.stop="onExpand"></i>
    <span :class="itemNameClass" :title="itemName" @click.prevent.stop="onSelect">
        <i :class="itemIconClass"></i>
        {{ itemName }}
    </span>
    <ul v-if="hasChildren" v-show="showChildren" class="rocktree-children" v-for="child in children">
        <TreeList.Item :modelValue="modelValue" @update:modelValue="onUpdateSelectedValues" @treeitem-expanded="onChildItemExpanded" :item="child" :multiple="multiple" :disableFolderSelection="disableFolderSelection" :autoExpand="autoExpand" />
    </ul>
</li>
`
});

export default defineComponent({
    name: "TreeList",

    components: {
        TreeItem: treeItem
    },

    props: {
        modelValue: {
            type: Array as PropType<string[]>,
            default: []
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        items: {
            type: Array as PropType<TreeItemBag[]>,
            default: []
        },

        provider: {
            type: Object as PropType<ITreeItemProvider>
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

    emits: [
        "update:modelValue",
        "update:items",
        "treeitem-expanded"
    ],

    setup(props, { emit }) {
        /** The list of items currently being displayed in the tree list. */
        const internalItems = ref<TreeItemBag[]>(props.items ?? []);

        /**
         * Get the root items from the provider as an asynchronous operation.
         */
        const getRootItems = async (): Promise<void> => {
            if (props.provider) {
                const result = props.provider.getRootItems();
                const rootItems = isPromise(result) ? await result : result;

                internalItems.value = JSON.parse(JSON.stringify(rootItems)) as TreeItemBag[];

                emit("update:items", internalItems.value);
            }
        };

        /**
         * Event handler for when a child item has updated the selected values.
         *
         * @param values The new selected values.
         */
        const onUpdateSelectedValues = (values: string[]): void => {
            // Pass the event up to the parent so it knows about the new selection.
            if (props.multiple) {
                emit("update:modelValue", values);
            }
            else {
                emit("update:modelValue", values.length > 0 ? [values[0]] : []);
            }
        };

        /**
         * Event handler for when an item has been expanded.
         *
         * @param item The item that was expanded.
         */
        const onItemExpanded = async (item: TreeItemBag): Promise<void> => {
            if (props.provider) {
                // We have a provider, check if the item needs it's children
                // loaded still.
                if (item.hasChildren && item.children === null) {
                    const result = props.provider.getChildItems(item);
                    const children = isPromise(result) ? await result : result;

                    item.children = JSON.parse(JSON.stringify(children)) as TreeItemBag[];

                    emit("update:items", internalItems.value);
                }
            }
            else {
                // No provider, simply pass the event to the parent in case it
                // wants to make any changes to the tree.
                emit("treeitem-expanded", item);
            }
        };

        // Watch for a change in our passed items and update our internal list.
        watch(() => props.items, () => {
            // Only update if we don't have a provider.
            if (!props.provider) {
                internalItems.value = props.items ?? [];
            }
        });

        // If we have a provider, then request the root items.
        if (props.provider) {
            getRootItems();
        }

        watch(() => props.provider, () => {
            if (props.provider) {
                getRootItems();
            }
        });

        return {
            internalItems,
            onItemExpanded,
            onUpdateSelectedValues
        };
    },

    template: `
<div>
    <ul class="rocktree">
        <TreeItem v-for="child in internalItems" :modelValue="modelValue" @update:modelValue="onUpdateSelectedValues" @treeitem-expanded="onItemExpanded" :item="child" :multiple="multiple" :disableFolderSelection="disableFolderSelection" :autoExpand="autoExpand" />
    </ul>
</div>
`
});
