<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="panel-navigation-bar">
        <div class="panel-toolbar panel-toolbar-shadow">
            <ul class="nav nav-pills nav-sm">
                <li v-for="item in items"
                    role="presentation"
                    :class="getItemClass(item)">
                    <a href="#" @click.prevent="onItemClicked(item)">{{ item.text }}</a>
                </li>
            </ul>
        </div>
    </div>
</template>

<style scoped>
.panel-navigation-bar {
    overflow-x: clip;
    background-color: var(--panel-bg);
}
</style>

<script setup lang="ts">
    import { PropType } from "vue";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { useVModelPassthrough } from "@Obsidian/Utility/component";

    const props = defineProps({
        modelValue: {
            type: String as PropType<string | null>,
            required: false
        },

        items: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: string): void
    }>();

    // #region Values

    const internalValue = useVModelPassthrough(props, "modelValue", emit);

    // #endregion

    // #region Computed Values

    // #endregion

    // #region Functions

    function getItemClass(item: ListItemBag): string {
        if (internalValue.value === item.value) {
            return "active";
        }

        return "";
    }

    // #endregion

    // #region Event Handlers

    function onItemClicked(item: ListItemBag): void {
        internalValue.value = item.value;
    }

    // #endregion
</script>
