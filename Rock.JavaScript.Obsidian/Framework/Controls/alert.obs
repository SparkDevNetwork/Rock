<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div :class="typeClass">
        <button v-if="dismissible" type="button" class="close" @click="onDismiss" aria-label="Hide Alert">
            <span aria-hidden="true">&times;</span>
        </button>
        <slot />
    </div>
</template>

<script setup lang="ts">
    import { PropType, computed } from "vue";
    import { AlertType } from "@Obsidian/Enums/Controls/alertType";
    import { LiteralUnion } from "@Obsidian/Types/Utility/support";

    const props = defineProps({
        dismissible: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        alertType: {
            type: String as PropType<LiteralUnion<AlertType>>,
            default: AlertType.Default
        }
    });

    const emit = defineEmits<{
        (e: "dismiss"): void
    }>();

    function onDismiss(): void {
        emit("dismiss");
    }

    const typeClass = computed(() => `alert alert-${props.alertType}`);
</script>