<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="live-event">
        <div class="row">
            <div class="col-lg-3 col-md-4 col-sm-6">
                <Kpi class="ml-0"
                     color="blue"
                     :colorShade="600"
                     :value="participantCount"
                     label="Current Participants"
                     iconCssClass="fa fa-user"
                     isCard
                     tooltip="The number of individuals that are currently participating in the experience." />
            </div>
        </div>

        <Alert v-if="isExperienceInactive" alertType="warning">
            This experience event has ended.
        </Alert>

        <div class="experience-body" :class="{ inactive: isExperienceInactive }">
            <div class="experience-actions-panel">
                <div class="experience-actions-panel-header">
                    <span class="title">Experience Actions</span>
                    <a v-if="isNotificationAvailable" href="#" :class="notificationStateClass" @click.prevent="onNotificationStateClick">
                        <i :class="notificationStateIconClass"></i>
                    </a>
                </div>

                <div class="experience-actions-panel-body">
                    <ExperienceActionButtons :modelValue="activeAction"
                                             :actions="experienceActions"
                                             @update:modelValue="onUpdateActiveAction" />
                </div>
            </div>

            <div class="preview-panel">
                <iframe v-if="previewPageUrl" class="invisible" :src="previewPageUrl" @load="onPreviewLoad" />
                <Alert v-else alertType="info">
                    Live experience preview has not been configured.
                </Alert>

                <div>
                    <RockLabel>Visualizer</RockLabel>

                    <div class="d-flex">
                        <div class="mr-3">
                            <InlineCheckBox v-model="isVisualizerAutomatic" label="Automatic" />
                        </div>

                        <div class="flex-grow-1">
                            <DropDownList :modelValue="activeVisualizer"
                                          @update:modelValue="onUpdateActiveVisualizer"
                                          :items="visualizerItems"
                                          showBlankItem
                                          :disabled="isVisualizerAutomatic" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.experience-body {
    display: flex;
    flex-wrap: wrap;
    margin: 0px -18px;
}

.experience-body.inactive > * {
    opacity: 0.5;
    cursor: not-allowed;
}

.experience-body.inactive > * > * {
    pointer-events: none;
}

.experience-actions-panel {
    flex-grow: 1;
    min-width: 320px;
    min-height: 480px;
    margin: 0px 18px 18px 18px;
    background-color: var(--panel-heading-bg);
    border: 1px solid #c4c4c4;
    border-radius: var(--border-radius-base);
}

.experience-actions-panel-header {
    display: flex;
    padding: 8px 12px;
    border-bottom: 1px solid #c4c4c4;
    align-items: center;
}

.experience-actions-panel-header > .title {
    flex-grow: 1;
}

.experience-actions-panel-body {
    padding: 12px;
}

.preview-panel {
    display: flex;
    flex-direction: column;
    flex-grow: 1;
    margin: 0px 18px 18px 18px;
    min-width: 320px;
    max-width: 640px;
    min-height: 480px;
}

.preview-panel iframe {
    flex-grow: 1;
    border: 1px solid #c4c4c4;
    border-radius: var(--border-radius-base);
    overflow: hidden;
}
</style>

<script setup lang="ts">
    import Alert from "@Obsidian/Controls/alert.vue";
    import DropDownList from "@Obsidian/Controls/dropDownList";
    import Kpi from "@Obsidian/Controls/kpi.vue";
    import ExperienceActionButtons from "./experienceActionButtons.partial.vue";
    import InlineCheckBox from "@Obsidian/Controls/inlineCheckBox";
    import RockLabel from "@Obsidian/Controls/rockLabel";
    import { computed, PropType, ref, watch } from "vue";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { ITopic } from "@Obsidian/Utility/realTime";
    import { IParticipantTopic } from "../types";
    import { ExperienceManagerInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/ExperienceManager/experienceManagerInitializationBox";
    import { NavigationUrlKey } from "./types";

    const props = defineProps({
        /** The identifier of the occurrence we have joined. */
        occurrenceIdKey: {
            type: String as PropType<string | null>,
            default: null
        },

        /** The topic proxy for the experience we have joined. */
        realTimeTopic: {
            type: Object as PropType<ITopic<IParticipantTopic> | null>,
            default: null
        },

        /** The block configuration data. */
        configuration: {
            type: Object as PropType<ExperienceManagerInitializationBox>,
            required: true
        },

        /** The identifier of the initial action being displayed, read-only. */
        initialActionIdKey: {
            type: String as PropType<string | null>,
            required: true
        },

        /** The identifier of the initial action visualizer being displayed, read-only. */
        initialVisualizerActionIdKey: {
            type: String as PropType<string | null>,
            required: true
        }
    });

    // const emit = defineEmits<{
    // }>();

    // #region Values

    const isNotificationsEnabled = ref(false);
    const activeAction = ref<string | null>(props.initialActionIdKey ?? null);
    const participantCount = ref(props.configuration.participantCount);
    const isExperienceInactive = ref(props.configuration.isExperienceInactive);

    const experienceActions: ListItemBag[] = props.configuration.actions ?? [];
    const isNotificationAvailable = props.configuration.isNotificationAvailable;

    const activeVisualizer = ref(props.initialVisualizerActionIdKey ?? "");
    const isVisualizerAutomatic = ref(true);

    // #endregion

    // #region Computed Values

    const notificationStateClass = computed((): string => {
        return isNotificationsEnabled.value ? "btn btn-info btn-xs" : "btn btn-default btn-xs";
    });

    const notificationStateIconClass = computed((): string => {
        return isNotificationsEnabled.value ? "fa fa-fw fa-bell" : "fa fa-fw fa-bell-slash";
    });

    const previewPageUrl = computed((): string => {
        return props.configuration.navigationUrls?.[NavigationUrlKey.LiveExperiencePage] ?? "";
    });

    const visualizerItems = computed((): ListItemBag[] => {
        return (props.configuration.actions ?? []).map((a, idx) => ({
            value: a.value,
            text: `${idx + 1}. ${a.text}`
        }));
    });

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    /**
     * Event handler for when the notification bell is clicked. Toggle the
     * flag which determines if we are sending push notifications.
     */
    function onNotificationStateClick(ev: Event): void {
        isNotificationsEnabled.value = !isNotificationsEnabled.value;

        if (ev.target instanceof HTMLElement) {
            ev.target.blur();
        }
    }

    /**
     * Called when the preview IFrame has loaded. Hide the admin bar and then
     * show the frame.
     *
     * @param ev The event.
     */
    function onPreviewLoad(ev: Event): void {
        if (ev.target instanceof HTMLIFrameElement) {
            const adminFooter = ev.target.contentDocument?.querySelector("#cms-admin-footer") as HTMLElement;

            if (adminFooter) {
                adminFooter.style.display = "none";
            }

            ev.target.classList.remove("invisible");
        }
    }

    /**
     * Event handler for when the individual manually changes selection of the
     * activeAction value. Notify the server of the change.
     *
     * @param value The new value to set activeAction to.
     */
    async function onUpdateActiveAction(value: string | null): Promise<void> {
        activeAction.value = value;

        if (!props.realTimeTopic || !props.occurrenceIdKey) {
            return;
        }

        if (activeAction.value) {
            await props.realTimeTopic.server.showAction(props.occurrenceIdKey, activeAction.value, isNotificationsEnabled.value);

            if (isVisualizerAutomatic.value) {
                await props.realTimeTopic.server.showVisualizer(props.occurrenceIdKey, activeAction.value);
            }
        }
        else {
            await props.realTimeTopic.server.clearActions(props.occurrenceIdKey);

            if (isVisualizerAutomatic.value) {
                await props.realTimeTopic.server.clearVisualizer(props.occurrenceIdKey);
            }
        }
    }

    /**
     * Event handler for when the server notifies us that an action should
     * be shown.
     *
     * @param idKey The occurrence identifier key.
     * @param actionIdKey The action identifier that should be shown.
     */
    function onShowAction(idKey: string, actionIdKey: string): void {
        if (idKey === props.occurrenceIdKey) {
            activeAction.value = actionIdKey;
        }
    }

    /**
     * Event handler for when the server notifies us that all actions
     * should be cleared.
     *
     * @param idKey The occurrence identifier key.
     */
    function onClearActions(idKey: string): void {
        if (idKey === props.occurrenceIdKey) {
            activeAction.value = null;
        }
    }

    /**
     * Event handler for when the server notifies us that a visualizer should
     * be shown.
     *
     * @param idKey The occurrence identifier key.
     * @param actionIdKey The action identifier that should be shown.
     */
    function onShowVisualizer(idKey: string, actionIdKey: string): void {
        if (idKey === props.occurrenceIdKey) {
            activeVisualizer.value = actionIdKey;
        }
    }

    /**
     * Event handler for when the server notifies us that the visualizer
     * should be cleared.
     *
     * @param idKey The occurrence identifier key.
     */
    function onClearVisualizer(idKey: string): void {
        if (idKey === props.occurrenceIdKey) {
            activeVisualizer.value = "";
        }
    }

    async function onUpdateActiveVisualizer(value: string | string[]): Promise<void> {
        activeVisualizer.value = typeof value === "string" ? value : "";

        if (props.realTimeTopic && props.occurrenceIdKey) {
            if (activeVisualizer.value) {
                await props.realTimeTopic.server.showVisualizer(props.occurrenceIdKey, activeVisualizer.value);
            }
            else {
                await props.realTimeTopic.server.clearVisualizer(props.occurrenceIdKey);
            }
        }
    }

    /**
     * Event handler for a timer that causes us to update the participant
     * count in the UI.
     */
    async function onUpdateParticipantCountTimer(): Promise<void> {
        try {
            if (props.realTimeTopic && props.occurrenceIdKey) {
                participantCount.value = await props.realTimeTopic.server.getParticipantCount(props.occurrenceIdKey);
            }
        }
        finally {
            setTimeout(onUpdateParticipantCountTimer, props.configuration.participantCountUpdateInterval * 1_000);
        }
    }

    // #endregion

    watch(() => props.realTimeTopic, () => {
        if (props.realTimeTopic) {
            props.realTimeTopic.on("showAction", onShowAction);
            props.realTimeTopic.on("clearActions", onClearActions);
            props.realTimeTopic.on("showVisualizer", onShowVisualizer);
            props.realTimeTopic.on("clearVisualizer", onClearVisualizer);
        }
    });

    watch(() => props.initialActionIdKey, () => {
        activeAction.value = props.initialActionIdKey;
    });

    watch(() => props.initialVisualizerActionIdKey, () => {
        activeVisualizer.value = props.initialVisualizerActionIdKey ?? "";
    });

    setTimeout(onUpdateParticipantCountTimer, props.configuration.participantCountUpdateInterval * 1_000);
</script>
