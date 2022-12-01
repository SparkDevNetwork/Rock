<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <Panel title="Actions">
        <SectionHeader :title="actionHeaderTitle"
                       :description="actionHeaderDescription">
            <template #actions>
                <a class="btn btn-default btn-sm" href="#" @click.prevent="onAddActionClick">
                    <i class="fa fa-plus"></i>
                </a>
            </template>
        </SectionHeader>

        <div v-dragSource="dragOptions"
             v-dragTarget="dragOptions"
             class="actions-list">
            <div v-for="(action, index) in internalValue" class="action-item" :key="action.guid!">
                <div>{{ index + 1 }}</div>
                <div>
                    <span class="reorder-handle">
                        <i class="fa fa-bars"></i>
                    </span>
                </div>
                <div class="action-item-icon">
                    <span class="icon">
                        <i :class="getActionTypeIconClass(action)"></i>
                    </span>
                </div>
                <div class="action-item-content">
                    <div class="title text-lg">{{ action.title }}</div>
                    <div class="subtitle text-sm text-muted">Subtitle: {{ getActionTypeName(action) }}</div>
                </div>
                <div>
                    <a href="#" class="action-icon" @click.prevent="onEditActionClick(action)">
                        <i class="fa fa-pencil"></i>
                    </a>
                </div>
                <div>
                    <a href="#" class="action-icon action-icon-danger" @click.prevent="onActionRemoveClick(action)">
                        <i class="fa fa-times"></i>
                    </a>
                </div>
            </div>
        </div>
    </Panel>

    <Modal v-model="isModalVisible"
           :title="modalTitle"
           saveText="Save"
           @save="onActionSave">
        <Alert v-if="modalErrorMessage"
               alertType="warning">
            {{ modalErrorMessage }}
        </Alert>

        <DropDownList v-model="actionType"
                      label="Action Type"
                      rules="required"
                      :items="actionTypeItems" />

        <AttributeValuesContainer v-model="attributeValues"
                                  :attributes="actionAttributes"
                                  isEditMode
                                  :showCategoryLabel="false" />

        <div class="row">
            <div v-if="isRequiresModerationVisible" class="col-md-4">
                <CheckBox v-model="requiresModeration"
                          label="Requires Moderation" />
            </div>

            <div v-if="isMultipleSubmissionsVisible" class="col-md-4">
                <CheckBox v-model="allowMultipleSubmissions"
                          label="Allow Multiple Submissions" />
            </div>

            <div class="col-md-4">
                <CheckBox v-model="anonymousResponses"
                          label="Anonymous Responses" />
            </div>
        </div>

        <DropDownList v-model="responseVisual"
                      label="Response Visual"
                      :items="responseVisualItems" />

        <AttributeValuesContainer v-model="attributeValues"
                                  :attributes="visualizerAttributes"
                                  isEditMode
                                  :showCategoryLabel="false" />

    </Modal>
</template>

<style scoped>
.action-item {
    display: flex;
    align-items: stretch;
    margin-bottom: 12px;
}

.action-item > * {
    display: flex;
    align-items: center;
    align-self: stretch;
    padding: 8px 12px;
    border-top: 1px solid #c4c4c4;
    border-bottom: 1px solid #c4c4c4;
}

.action-item > *:first-child {
    border-left: 1px solid #c4c4c4;
    border-radius: 8px 0px 0px 8px;
    background-color: var(--brand-info);
    color: white;
    justify-content: center;
    padding: 8px 0px;
    min-width: 35px;
}

.action-item > *:last-child {
    border-right: 1px solid #c4c4c4;
    border-radius: 0px 8px 8px 0px;
    padding-right: 16px;
}

.action-item > .action-item-icon {
    padding-left: 0px;
}

.action-item > .action-item-icon > .icon {
    background-color: var(--brand-info);
    color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 35px;
    height: 35px;
    border-radius: 50%;
}

.action-item > .action-item-content {
    flex: 1 0;
    flex-direction: column;
    align-items: stretch;
    justify-content: center;
    padding-left: 0px;
}

.action-item .reorder-handle {
    cursor: grab;
}

.action-item .action-icon {
    color: var(--text-color);
    display: none;
}

.action-item:hover .action-icon {
    display: initial;
}

.action-item:hover .action-icon.action-icon-danger {
    color: var(--brand-danger);
}
</style>

<script setup lang="ts">
    import Alert from "@Obsidian/Controls/alert.vue";
    import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
    import CheckBox from "@Obsidian/Controls/checkBox";
    import DropDownList from "@Obsidian/Controls/dropDownList";
    import Modal from "@Obsidian/Controls/modal";
    import Panel from "@Obsidian/Controls/panel";
    import SectionHeader from "@Obsidian/Controls/sectionHeader";
    import { DragSource as vDragSource, DragTarget as vDragTarget, useDragReorder } from "@Obsidian/Directives/dragDrop";
    import { useVModelPassthrough } from "@Obsidian/Utility/component";
    import { setPropertiesBoxValue, useInvokeBlockAction } from "@Obsidian/Utility/block";
    import { areEqual, newGuid } from "@Obsidian/Utility/guid";
    import { ValidPropertiesBox } from "@Obsidian/ViewModels/Utility/validPropertiesBox";
    import { InteractiveExperienceActionBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceActionBag";
    import { InteractiveExperienceActionTypeBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceActionTypeBag";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { computed, PropType, ref, watch } from "vue";
    import { Guid } from "@Obsidian/Types";
    import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
    import { alert, confirmDelete } from "@Obsidian/Utility/dialogs";
    import { InteractiveExperienceVisualizerTypeBag } from "@Obsidian/ViewModels/Blocks/Event/InteractiveExperiences/InteractiveExperienceDetail/interactiveExperienceVisualizerTypeBag";

    const props = defineProps({
        /** An array of actions that are currently configured. */
        modelValue: {
            type: Array as PropType<InteractiveExperienceActionBag[]>,
            required: true
        },

        /** The name of the experience currently displayed. */
        name: {
            type: String as PropType<string>,
            required: true
        },

        /** The identifier key of the interactive experience these actions are for. */
        interactiveExperienceIdKey: {
            type: String as PropType<string>,
            required: true
        },

        /** The action types that are supported by the server. */
        actionTypes: {
            type: Array as PropType<InteractiveExperienceActionTypeBag[]>,
            default: []
        },

        /** The visualizer types that are supported by the server. */
        visualizerTypes: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: InteractiveExperienceActionBag): void
    }>();

    // #region Values

    const invokeBlockAction = useInvokeBlockAction();
    const internalValue = useVModelPassthrough(props, "modelValue", emit);

    const isModalVisible = ref(false);
    const modalTitle = ref("");
    const modalErrorMessage = ref("");
    const existingActionGuid = ref<Guid | null>(null);
    const actionType = ref("");
    const requiresModeration = ref(false);
    const allowMultipleSubmissions = ref(false);
    const anonymousResponses = ref(false);
    const responseVisual = ref("");
    const attributeValues = ref<Record<string, string>>({});

    // #endregion

    // #region Computed Values

    const actionHeaderTitle = computed((): string => {
        return `Actions for ${props.name}`;
    });

    const actionHeaderDescription = computed((): string => {
        return `The actions below are configured for the ${props.name} experience.`;
    });

    const actionTypeItems = computed((): ListItemBag[] => {
        return props.actionTypes.map(at => ({
            value: at.value,
            text: at.text
        }));
    });

    const responseVisualItems = computed((): ListItemBag[] => {
        return props.visualizerTypes;
    });

    const selectedActionType = computed((): InteractiveExperienceActionTypeBag | null => {
        return props.actionTypes.find(at => areEqual(at.value, actionType.value)) ?? null;
    });

    const actionAttributes = computed((): Record<string, PublicAttributeBag> => {
        return selectedActionType.value?.attributes ?? {};
    });

    const selectedVisualizerType = computed((): InteractiveExperienceVisualizerTypeBag | null => {
        return props.visualizerTypes.find(at => areEqual(at.value, responseVisual.value)) ?? null;
    });

    const visualizerAttributes = computed((): Record<string, PublicAttributeBag> => {
        return selectedVisualizerType.value?.attributes ?? {};
    });

    const isRequiresModerationVisible = computed((): boolean => {
        return selectedActionType.value?.isModerationSupported === true;
    });

    const isMultipleSubmissionsVisible = computed((): boolean => {
        return selectedActionType.value?.isMultipleSubmissionSupported === true;
    });

    // #endregion

    // #region Functions

    /**
     * Gets the name of the action type that the action is using.
     *
     * @param action The action whose type name is being requested.
     *
     * @returns The name of the action type or an empty string if not found.
     */
    function getActionTypeName(action: InteractiveExperienceActionBag): string {
        return props.actionTypes.find(at => areEqual(at.value, action.actionType?.value))?.text ?? "";
    }

    /**
     * Gets the icon CSS class to use for the requested action.
     *
     * @param action The aciton whose icon class is being requested.
     *
     * @returns The CSS class value to use to display the icon.
     */
    function getActionTypeIconClass(action: InteractiveExperienceActionBag): string {
        return props.actionTypes.find(at => areEqual(at.value, action.actionType?.value))?.iconCssClass ?? "";
    }

    /**
     * Updates the attribute values with any missing default values from the
     * currently selected action type and visualizer type.
     */
    function updateDefaultAttributeValues(): void {
        const newValues = { ...attributeValues.value };
        let isChanged = false;

        if (selectedActionType.value && selectedActionType.value.defaultAttributeValues) {
            for (const key in selectedActionType.value.defaultAttributeValues) {
                if (!newValues[key]) {
                    newValues[key] = selectedActionType.value.defaultAttributeValues[key];
                    isChanged = true;
                }
            }
        }

        if (selectedVisualizerType.value && selectedVisualizerType.value.defaultAttributeValues) {
            for (const key in selectedVisualizerType.value.defaultAttributeValues) {
                if (!newValues[key]) {
                    newValues[key] = selectedVisualizerType.value.defaultAttributeValues[key];
                    isChanged = true;
                }
            }
        }

        if (isChanged) {
            attributeValues.value = newValues;
        }
    }

    // #endregion

    // #region Event Handlers

    /**
     * Event handler for when the Add button is clicked. Begin the process of
     * adding a new action to the experience.
     */
    function onAddActionClick(): void {
        actionType.value = "";
        requiresModeration.value = false;
        allowMultipleSubmissions.value = false;
        anonymousResponses.value = false;
        responseVisual.value = "";
        attributeValues.value = {};

        existingActionGuid.value = null;
        modalTitle.value = "Add Action";
        isModalVisible.value = true;
    }

    /**
     * Event handler for when an existing action's edit button has been clicked.
     * Begin the process of editing the action.
     *
     * @param action The action that should be edited.
     */
    function onEditActionClick(action: InteractiveExperienceActionBag): void {
        actionType.value = action.actionType?.value ?? "";
        requiresModeration.value = action.isModerationRequired;
        allowMultipleSubmissions.value = action.isMultipleSubmissionsAllowed;
        anonymousResponses.value = action.isResponseAnonymous;
        responseVisual.value = action.responseVisualizer?.value ?? "";
        attributeValues.value = action.attributeValues ?? {};

        existingActionGuid.value = action.guid ?? null;
        modalTitle.value = "Edit Action";
        isModalVisible.value = true;
    }

    /**
     * Event handler for when the Save button in the modal is clicked and the
     * action should be saved. Send the request to the server and then update
     * the list of known actions.
     */
    async function onActionSave(): Promise<void> {
        modalErrorMessage.value = "";

        const box: ValidPropertiesBox<InteractiveExperienceActionBag> = {};

        setPropertiesBoxValue(box, "guid", existingActionGuid.value ?? newGuid());
        setPropertiesBoxValue(box, "actionType", { value: actionType.value });
        setPropertiesBoxValue(box, "isModerationRequired", requiresModeration.value);
        setPropertiesBoxValue(box, "isMultipleSubmissionsAllowed", allowMultipleSubmissions.value);
        setPropertiesBoxValue(box, "isResponseAnonymous", anonymousResponses.value);
        setPropertiesBoxValue(box, "attributeValues", attributeValues.value);
        setPropertiesBoxValue(box, "responseVisualizer", responseVisual.value ? { value: responseVisual.value } : null);

        const result = await invokeBlockAction<InteractiveExperienceActionBag>("SaveAction", {
            idKey: props.interactiveExperienceIdKey,
            box: box
        });

        if (!result.isSuccess || !result.data) {
            modalErrorMessage.value = result.errorMessage ?? "Unknown error while trying to save action.";

            return;
        }

        const action = result.data;
        const existingActionIndex = internalValue.value.findIndex(a => areEqual(a.guid, action.guid));

        if (existingActionIndex !== -1) {
            const newValue = [...internalValue.value];

            newValue.splice(existingActionIndex, 1, action);

            internalValue.value = newValue;
        }
        else {
            internalValue.value = [...internalValue.value, action];
        }

        isModalVisible.value = false;
    }

    /**
     * Event handler for when the remove button for an action is clicked.
     * Confirm with the individual that they really wanted to remove the
     * action and then remove it from the server.
     *
     * @param action The action that is to be removed.
     */
    async function onActionRemoveClick(action: InteractiveExperienceActionBag): Promise<void> {
        if (!await confirmDelete("Action")) {
            return;
        }

        const result = await invokeBlockAction<void>("DeleteAction", {
            idKey: props.interactiveExperienceIdKey,
            actionGuid: action.guid
        });

        if (!result.isSuccess) {
            alert(result.errorMessage || "Unable to delete the action.");
        }
        else {
            const index = internalValue.value.findIndex(a => areEqual(a.guid, action.guid));

            if (index !== -1) {
                const newValue = [...internalValue.value];

                newValue.splice(index, 1);

                internalValue.value = newValue;
            }
        }
    }

    /**
     * Event handler for when the person has dragged one of the actions to
     * reorder it in the action list.
     *
     * @param action The action that was dragged.
     * @param beforeAction The action it should be placed before, or null if at the end.
     */
    async function onActionReorder(action: InteractiveExperienceActionBag, beforeAction: InteractiveExperienceActionBag | null): Promise<void> {
        // Force an update so the detail block updates the value.
        internalValue.value = [...internalValue.value];

        const result = await invokeBlockAction<void>("ReorderAction", {
            idKey: props.interactiveExperienceIdKey,
            actionGuid: action.guid,
            beforeActionGuid: beforeAction?.guid ?? null
        });

        if (!result.isSuccess) {
            alert(result.errorMessage || "Unable to re-order actions, you might need to reload the page.");
            return;
        }
    }

    // #endregion

    watch([actionType, responseVisual], () => {
        updateDefaultAttributeValues();
    });

    const dragOptions = useDragReorder(internalValue, onActionReorder);

    updateDefaultAttributeValues();
</script>
