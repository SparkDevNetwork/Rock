<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="live-experience-body" :style="experienceStyles">
        <Alert v-if="isExperienceInactive" alertType="warning">
            This experience has ended.
        </Alert>

        <div v-if="isWelcomeContentVisible" class="welcome">
            <div class="welcome-header">
                <h1 v-if="config.style?.welcome?.title" class="welcome-title">{{ config.style.welcome.title }}</h1>
            </div>

            <div class="welcome-message">{{ config.style?.welcome?.message }}</div>
        </div>

        <div v-if="isNoActionContentVisible" class="no-action">
            <div class="no-action-header">
                <h1 v-if="config.style?.noAction?.title" class="no-action-title">{{ config.style.noAction.title }}</h1>
            </div>

            <div class="no-action-message">{{ config.style?.noAction?.message }}</div>
        </div>

        <component v-if="activeActionComponent"
                   :is="activeActionComponent"
                   :eventId="eventId"
                   :actionId="activeActionId"
                   :renderConfiguration="activeActionRenderConfiguration"
                   :realTimeTopic="realTimeTopic" />
    </div>
</template>

<!--
We might need to add this so that things look and behave right on mobile:

<meta name="viewport" content="initial-scale=1.0, user-scalable=no">

<style>
body {
    touch-action: none;
}
</style>
-->

<!-- Cannot use scoped here otherwise it becomes very difficult to override by custom CSS. -->
<style>
.live-experience-body {
    position: absolute;
    left: 0px;
    right: 0px;
    top: 0px;
    bottom: 0px;
    padding: 18px;
    background-color: var(--experience-action-bg-color, inherit);
    background-image: var(--experience-action-bg-image, initial);
    color: var(--experience-action-text-color, inherit);
}

.live-experience-body .btn-primary,
.live-experience-body .btn-primary:hover,
.live-experience-body .btn-primary:focus {
    background-color: var(--experience-action-primary-button-color);
    border-color: var(--experience-action-primary-button-color);
    color: var(--experience-action-primary-button-text-color);
    box-shadow: none;
}

.live-experience-body .btn-secondary,
.live-experience-body .btn-secondary:hover,
.live-experience-body .btn-secondary:focus {
    background-color: var(--experience-action-secondary-button-color);
    border-color: var(--experience-action-secondary-button-color);
    color: var(--experience-action-secondary-button-text-color);
}

.live-experience-body .welcome-header {
    background-image: var(--welcome-header-image, initial);
}
</style>

<script setup lang="ts">
    import ActionShortAnswer from "./LiveExperience/actionShortAnswer.partial.vue";
    import ActionPoll from "./LiveExperience/actionPoll.partial.vue";
    import Alert from "@Obsidian/Controls/alert.vue";
    import { Component as VueComponent, computed, ref } from "vue";
    import { getTopic, ITopic } from "@Obsidian/Utility/realTime";
    import { IParticipantTopic } from "./types";
    import { LiveExperienceInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/LiveExperience/liveExperienceInitializationBox";
    import { ActionRenderConfigurationBag } from "@Obsidian/ViewModels/Event/InteractiveExperiences/actionRenderConfigurationBag";
    import { onConfigurationValuesChanged, useConfigurationValues, useReloadBlock } from "@Obsidian/Utility/block";
    import { Guid } from "@Obsidian/Types";

    const config = useConfigurationValues<LiveExperienceInitializationBox>();

    const actionTypeLookup: Record<Guid, VueComponent> = {
        "5ffe1f8f-5f0b-4b34-9c3f-1706d9093210": ActionShortAnswer,
        "9256a5b7-480d-4ffa-86d1-03b8aefc254e": ActionPoll
    };

    // #region Values

    const eventId = ref<string | null>(null);
    const isReady = ref(false);
    const realTimeTopic = ref<ITopic<IParticipantTopic> | null>(null);
    const activeActionId = ref<string | null>(null);
    const activeActionComponent = ref<VueComponent | null>(null);
    const activeActionRenderConfiguration = ref<ActionRenderConfigurationBag | null>(null);
    const isExperienceInactive = ref(config.isExperienceInactive);
    const isWelcomeState = ref(!isExperienceInactive.value);
    const experienceStyles = getExperienceStyles();

    // #endregion

    // #region Computed Values

    const isWelcomeContentVisible = computed((): boolean => {
        return isReady.value && !isExperienceInactive.value && !!realTimeTopic.value && isWelcomeState.value;
    });

    const isNoActionContentVisible = computed((): boolean => {
        return isReady.value && !isExperienceInactive.value && !isWelcomeContentVisible.value && !activeActionComponent.value;
    });

    // #endregion

    // #region Functions

    async function startRealTime(): Promise<void> {
        const experienceToken = config.experienceToken;

        if (!experienceToken) {
            throw new Error("Missing experience token from server.");
        }

        const topic = await getTopic<IParticipantTopic>("Rock.RealTime.Topics.InteractiveExperienceParticipantTopic");

        topic.onReconnect(async server => {
            const response = await server.joinExperience(experienceToken);

            eventId.value = response.occurrenceIdKey ?? null;

            if (response.currentActionIdKey && response.currentActionConfiguration) {
                setupActionComponent(response.currentActionIdKey, response.currentActionConfiguration);
            }
        });

        const response = await topic.server.joinExperience(experienceToken);
        eventId.value = response.occurrenceIdKey ?? null;

        topic.on("showAction", onShowAction);
        topic.on("clearActions", onClearActions);

        realTimeTopic.value = topic;
        isReady.value = true;

        if (response.currentActionIdKey && response.currentActionConfiguration) {
            setupActionComponent(response.currentActionIdKey, response.currentActionConfiguration);
        }

        setTimeout(onPingTimer, config.keepAliveInterval * 1_000);
    }

    function setupActionComponent(actionId: string, renderConfiguration: ActionRenderConfigurationBag): void {
        if (!isReady.value || isExperienceInactive.value || !renderConfiguration.actionTypeGuid || !actionTypeLookup[renderConfiguration.actionTypeGuid]) {
            activeActionComponent.value = null;
            activeActionId.value = null;
            activeActionRenderConfiguration.value = null;
            isWelcomeState.value = false;
            return;
        }

        activeActionComponent.value = actionTypeLookup[renderConfiguration.actionTypeGuid];
        activeActionId.value = actionId;
        activeActionRenderConfiguration.value = renderConfiguration;
        isWelcomeState.value = false;
    }

    function getExperienceStyles(): Record<string, string> {
        const styles: Record<string, string> = {};

        if (config.style?.welcome?.headerImage) {
            styles["--welcome-header-image"] = config.style.welcome.headerImage;
        }

        if (config.style?.noAction?.headerImage) {
            styles["--no-action-header-image"] = config.style.noAction.headerImage;
        }

        if (config.style?.action?.backgroundColor) {
            styles["--experience-action-bg-color"] = config.style.action.backgroundColor;
        }

        if (config.style?.action?.backgroundImage) {
            styles["--experience-action-bg-image"] = `url('${config.style.action.backgroundImage}')`;
        }

        if (config.style?.action?.primaryButtonColor) {
            styles["--experience-action-primary-button-color"] = config.style.action.primaryButtonColor;
        }
        else {
            styles["--experience-action-primary-button-color"] = "var(--brand-primary)";
        }

        if (config.style?.action?.primaryButtonTextColor) {
            styles["--experience-action-primary-button-text-color"] = config.style.action.primaryButtonTextColor;
        }
        else {
            styles["--experience-action-primary-button-text-color"] = "#fff";
        }

        if (config.style?.action?.secondaryButtonColor) {
            styles["--experience-action-secondary-button-color"] = config.style.action.secondaryButtonColor;
        }
        else {
            styles["--experience-action-secondary-button-color"] = "var(--brand-info)";
        }

        if (config.style?.action?.secondaryButtonTextColor) {
            styles["--experience-action-secondary-button-text-color"] = config.style.action.secondaryButtonTextColor;
        }
        else {
            styles["--experience-action-secondary-button-text-color"] = "#fff";
        }

        if (config.style?.action?.textColor) {
            styles["--experience-action-text-color"] = config.style.action.textColor;
        }
        else {
            styles["--experience-action-text-color"] = "var(--text-color)";
        }

        return styles;
    }

    function setupCustomStyles(): void {
        if (config.style?.action?.customCss) {
            const styleNode = document.createElement("style");
            styleNode.textContent = config.style.action.customCss;
            document.head.appendChild(styleNode);
        }
    }

    // #endregion

    // #region Event Handlers

    function onShowAction(id: string, actionId: string, renderConfiguration: ActionRenderConfigurationBag): void {
        if (eventId.value === id) {
            setupActionComponent(actionId, renderConfiguration);
        }
    }

    function onClearActions(id: string): void {
        if (eventId.value === id) {
            activeActionComponent.value = null;
            activeActionId.value = null;
            activeActionRenderConfiguration.value = null;
        }
    }

    async function onPingTimer(): Promise<void> {
        try {
            if (realTimeTopic.value && eventId.value) {
                await realTimeTopic.value.server.pingExperience(eventId.value);
            }
        }
        finally {
            setTimeout(onPingTimer, config.keepAliveInterval * 1000);
        }
    }

    // #endregion

    onConfigurationValuesChanged(useReloadBlock());

    setupCustomStyles();
    startRealTime();
</script>
