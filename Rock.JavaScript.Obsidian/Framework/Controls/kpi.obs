<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div ref="kpiElement" :class="kpiClass" :title="props.tooltip">
        <div v-if="iconCssClass" class="kpi-icon">
            <img class="svg-placeholder" src="data:image/svg+xml;utf8,&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'&gt;&lt;/svg&gt;" style="">
            <div class="kpi-content">
                <i :class="iconCssClass"></i>
            </div>
        </div>
        <div class="kpi-stat">
            <span class="kpi-value text-color">{{ calculatedValue }}</span>
            <span class="kpi-label">{{ label }}</span>
        </div>
    </div>
</template>

<style scoped>

</style>

<script setup lang="ts">
    import { asFormattedString } from "@Obsidian/Utility/numberUtils";
    import { tooltip } from "@Obsidian/Utility/tooltip";
    import { computed, nextTick, PropType, ref, watch } from "vue";

    const props = defineProps({
        /**
         * The value to display. If this is a number type then it will be
         * formatted with group separators, such as "," in the US.
         */
        value: {
            type: [String, Number] as PropType<string | number>,
            required: false
        },

        /** The short text label that describes what the value represents. */
        label: {
            type: String as PropType<string>,
            required: false
        },

        /** The tooltip to configure for the component that will be shown on hover. */
        tooltip: {
            type: String as PropType<string>,
            required: false
        },

        /** The CSS class to use for the icon. */
        iconCssClass: {
            type: String as PropType<string>,
            required: false
        },

        /** The base color to use for the text and border. */
        color: {
            type: String as PropType<"blue" | "gray" | "green" | "indigo" | "orange" | "pink" | "purple" | "red" | "teal" | "yellow">,
            default: "blue"
        },

        /** The shade of the color to use for the text and border. */
        colorShade: {
            type: Number as PropType<100 | 200 | 300 | 400 | 500 | 600 | 700 | 800 | 900>,
            default: 500
        },

        /** If true then a border will be drawn around the component. */
        isCard: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    });

    // #region Values

    const kpiElement = ref<HTMLElement | null>(null);

    // #endregion

    // #region Computed Values

    const kpiClass = computed((): string => {
        const classes = ["kpi", "has-icon-bg"];

        if (props.isCard) {
            classes.push("kpi-card");
        }

        classes.push(`text-${props.color}-${props.colorShade}`);

        if (props.colorShade < 300) {
            classes.push(`border-${props.color}-${props.colorShade}`);
        }
        else {
            classes.push(`border-${props.color}-${props.colorShade - 200}`);
        }

        return classes.join(" ");//text-blue-600 border-blue-400
    });

    const calculatedValue = computed((): string => {
        if (typeof props.value === "number") {
            return asFormattedString(props.value);
        }
        else if (typeof props.value === "string") {
            return props.value;
        }
        else {
            return "";
        }
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    // #endregion

    watch([kpiElement], () => {
        if (kpiElement.value) {
            tooltip(kpiElement.value);
        }
    });

    watch(() => props.tooltip, () => {
        // Next tick because at this moment the element hasn't been updated yet.
        nextTick(() => {
            if (kpiElement.value) {
                tooltip(kpiElement.value);
            }
        });
    });
</script>
