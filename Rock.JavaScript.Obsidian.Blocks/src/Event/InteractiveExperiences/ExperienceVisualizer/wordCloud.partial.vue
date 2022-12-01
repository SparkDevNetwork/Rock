<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="experience-visualizer experience-visualizer-type-dc35f0f7-83e5-47d8-aa27-b448962b60dd" :class="additionalVisualizerClasses">
        <div v-if="renderConfiguration.title" class="visualizer-title">{{ renderConfiguration.title }}</div>
        <WordCloud :words="responses"
                   :angleCount="angleCount"
                   :fontName="fontName"
                   :colors="colors"
                   width="100%"
                   height="100%" />
    </div>
</template>

<!-- Cannot use scoped here otherwise it becomes very difficult to override by custom CSS. -->
<style>
.experience-visualizer-type-dc35f0f7-83e5-47d8-aa27-b448962b60dd {
    display: flex;
    flex-direction: column;
}

.experience-visualizer-type-dc35f0f7-83e5-47d8-aa27-b448962b60dd .visualizer-title {
    color: var(--experience-visualizer-primary-color);
    text-align: center;
    font-size: 36px;
    margin-bottom: 12px;
}

.experience-visualizer-type-dc35f0f7-83e5-47d8-aa27-b448962b60dd .rock-word-cloud {
    flex-grow: 1;
}
</style>

<script setup lang="ts">
    import WordCloud from "@Obsidian/Controls/wordCloud.vue";
    import { toNumber } from "@Obsidian/Utility/numberUtils";
    import { computed, ref, watch } from "vue";
    import { visualizerProps } from "./util.partial";

    const props = defineProps(visualizerProps);

    // #region Values

    const responses = ref<string[]>([]);

    // #endregion

    // #region Computed Values

    const additionalVisualizerClasses = computed((): string => {
        return `experience-visualizer-${props.renderConfiguration.actionGuid}`;
    });

    const colors = computed((): string[] | undefined => {
        const colorStrings = props.renderConfiguration.configurationValues
            ?.colors
            ?.split(",")
            .map(c => c.trim())
            .filter(c => c !== "")
            ?? [];

        return colorStrings.length > 0 ? colorStrings : undefined;
    });

    const angleCount = computed((): number => {
        const value = toNumber(props.renderConfiguration.configurationValues?.angleCount || "5");

        return Math.min(100, Math.max(1, Math.floor(value)));
    });

    const fontName = computed((): string | undefined => {
        return props.renderConfiguration.configurationValues?.fontName || undefined;
    });

    // #endregion

    // #region Functions

    function updateResponses(): void {
        responses.value = props.responses.filter(r => r.response).map(r => r.response as string);
    }

    // #endregion

    // #region Event Handlers

    // #endregion

    watch(() => props.responses, updateResponses);

    updateResponses();
</script>
