<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div v-for="(action, index) in actions" class="action-item" :class="getActionItemClass(action)"
         href="#"
         @click.prevent="onActionClick(action)">
        <div class="action-item-icon">
            <span>{{ index + 1 }}</span>
            <span class="icon">
                <i :class="action.category"></i>
            </span>
        </div>

        <div class="action-item-content">
            {{ action.text }}
        </div>

        <div class="action-item-selected-icon">
            <i :class="getActionSelectedIconClass(action)"></i>
        </div>
    </div>
</template>

<style scoped>
.action-item {
    display: flex;
    align-items: stretch;
    margin-bottom: 12px;
    cursor: pointer;
}

.action-item > * {
    display: flex;
    align-items: center;
    align-self: stretch;
    padding: 12px;
    color: var(--text-color);
    background-color: #fff;
    border-top: 1px solid #c4c4c4;
    border-bottom: 1px solid #c4c4c4;
    transition: background-color .25s ease-in-out, border-color .25s ease-in-out;
}

.action-item > *:first-child {
    justify-content: center;
    min-width: 64px;
    padding-right: 8px;
    padding-left: 8px;
    border-right: 1px solid #c4c4c4;
    border-left: 1px solid #c4c4c4;
    border-radius: 8px 0 0 8px;
}

.action-item > *:last-child {
    padding-right: 16px;
    border-right: 1px solid #c4c4c4;
    border-radius: 0 8px 8px 0;
}

.action-item > .action-item-icon,
.action-item > .action-item-selected-icon {
    color: #777;
}

.action-item > .action-item-icon > .icon {
    margin-left: 8px;
}

.action-item > .action-item-content {
    flex: 1 0;
}

.action-item:hover > * {
    background-color: rgba(85, 150, 230, .1);
}

.action-item.selected > * {
    color: #0079b0;
    background-color: #d9f2fe;
    border-color: #009ce3;
}

.action-item.selected > .action-item-selected-icon {
    color: #009ce3;
}
</style>

<script setup lang="ts">
    import { useVModelPassthrough } from "@Obsidian/Utility/component";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { PropType } from "vue";

    const props = defineProps({
        modelValue: {
            type: String as PropType<string | null>,
            default: null
        },

        actions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: string | null): void
    }>();

    // #region Values

    const internalValue = useVModelPassthrough(props, "modelValue", emit);

    // #endregion

    // #region Computed Values

    // #endregion

    // #region Functions

    function getActionItemClass(action: ListItemBag): string {
        return action.value === internalValue.value ? "selected" : "";
    }

    function getActionSelectedIconClass(action: ListItemBag): string {
        return action.value === internalValue.value ? "fa fa-check-circle" : "fa fa-check-circle-o";
    }

    // #endregion

    // #region Event Handlers

    function onActionClick(action: ListItemBag): void {
        if (internalValue.value === action.value) {
            internalValue.value = null;
        }
        else {
            internalValue.value = action.value ?? null;
        }
    }

    // #endregion
</script>
