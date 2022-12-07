<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="experience-action experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e" :class="additionalActionClasses">
        <div class="question">
            {{ questionText }}
        </div>

        <div class="answers">
            <RadioButtonList v-model="answerText" :items="answerItems" />
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
.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .question::before,
.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers::before {
    display: block;
    margin-bottom: 4px;
    font-size: 3em;
    line-height: 1.2;
    content: 'Q:';
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers::before {
    content: 'A:';
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers {
    margin-top: 12px;
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers .radio {
    padding-left: 0;
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers .radio .label-text::before,
.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers .radio .label-text::after {
    display: none;
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers label {
    display: block;
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers input {
    display: none;
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers .label-text {
    display: block;
    padding: 6px 12px;
    border: 2px solid var(--experience-action-secondary-btn-bg);
    border-radius: var(--border-radius-base);
    transition: background-color .25s ease-in-out, color .25s ease-in-out;
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .answers input:checked + .label-text {
    color: var(--experience-action-secondary-btn-color);
    background-color: var(--experience-action-secondary-btn-bg);
}

.experience-action-type-9256a5b7-480d-4ffa-86d1-03b8aefc254e .submit {
    margin-top: 18px;
}
</style>

<script setup lang="ts">
    import RockButton from "@Obsidian/Controls/rockButton";
    import RadioButtonList from "@Obsidian/Controls/radioButtonList";
    import { BtnType } from "@Obsidian/Enums/Controls/buttonOptions";
    import { computed, ref } from "vue";
    import { actionProps } from "./util.partial";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

    const props = defineProps(actionProps);

    // #region Values

    const answerText = ref("");
    const answers: string[] = JSON.parse(props.renderConfiguration.configurationValues?.answers ?? "[]") ?? [];
    const answerItems: ListItemBag[] = answers.map(a => ({ value: a, text: a }));
    const primaryButton = BtnType.Primary;

    // #endregion

    // #region Computed Values

    const additionalActionClasses = computed((): string => {
        return `experience-action-${props.renderConfiguration.actionId}`;
    });

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
