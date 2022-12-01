<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="experience-action experience-action-type-5ffe1f8f-5f0b-4b34-9c3f-1706d9093210">
        <div class="question">
            {{ questionText }}
        </div>

        <div class="answer">
            <input v-model="answerText" class="form-control" type="text"  />
        </div>

        <div class="submit">
            <RockButton :btnType="primaryButton"
                        :disabled="isButtonDisabled"
                        @click="onSubmitClick">
                Submit
            </RockButton>
        </div>
    </div>
</template>

<!-- Cannot use scoped here otherwise it becomes very difficult to override by custom CSS. -->
<style>
.experience-action-type-5ffe1f8f-5f0b-4b34-9c3f-1706d9093210 .question::before {
    content: 'Q:';
    display: block;
    font-size: 3em;
}

.experience-action-type-5ffe1f8f-5f0b-4b34-9c3f-1706d9093210 .answer::before {
    content: 'A:';
    display: block;
    font-size: 3em;
}

.experience-action-type-5ffe1f8f-5f0b-4b34-9c3f-1706d9093210 .submit {
    margin-top: 18px;
}
</style>

<script setup lang="ts">
    import RockButton from "@Obsidian/Controls/rockButton";
    import { BtnType } from "@Obsidian/Enums/Controls/buttonOptions";
    import { computed, ref } from "vue";
    import { actionProps } from "./util.partial";

    const props = defineProps(actionProps);

    // #region Values

    const answerText = ref("");
    const primaryButton = BtnType.Primary;

    // #endregion

    // #region Computed Values

    const questionText = computed((): string => {
        return props.renderConfiguration.configurationValues?.["question"] ?? "";
    });

    const isButtonDisabled = computed((): boolean => {
        return !answerText.value;
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    async function onSubmitClick(): Promise<void> {
        await props.realTimeTopic.server.postResponse(props.eventId, props.actionId, answerText.value);

        answerText.value = "";
    }

    // #endregion
</script>
