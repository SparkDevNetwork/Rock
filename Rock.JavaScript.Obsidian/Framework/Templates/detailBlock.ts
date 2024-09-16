// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
import { computed, defineComponent, PropType, ref, watch } from "vue";
import Panel from "@Obsidian/Controls/panel.obs";
import Modal from "@Obsidian/Controls/modal.obs";
import { Guid } from "@Obsidian/Types";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { DetailPanelMode } from "@Obsidian/Enums/Controls/detailPanelMode";
import { isPromise, PromiseCompletionSource } from "@Obsidian/Utility/promiseUtils";
import { FollowingGetFollowingOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/followingGetFollowingOptionsBag";
import { FollowingGetFollowingResponseBag } from "@Obsidian/ViewModels/Rest/Controls/followingGetFollowingResponseBag";
import { FollowingSetFollowingOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/followingSetFollowingOptionsBag";
import AuditDetail from "@Obsidian/Controls/auditDetail.obs";
import BadgeList from "@Obsidian/Controls/badgeList.obs";
import EntityTagList from "@Obsidian/Controls/tagList.obs";
import RockButton from "@Obsidian/Controls/rockButton.obs";
import RockForm from "@Obsidian/Controls/rockForm.obs";
import RockSuspense from "@Obsidian/Controls/rockSuspense.obs";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { alert, confirmDelete, showSecurity } from "@Obsidian/Utility/dialogs";
import { useHttp } from "@Obsidian/Utility/http";
import { makeUrlRedirectSafe } from "@Obsidian/Utility/url";
import { asBooleanOrNull } from "@Obsidian/Utility/booleanUtils";
import { splitCase } from "@Obsidian/Utility/stringUtils";
import { areEqual, emptyGuid } from "@Obsidian/Utility/guid";
import { useBlockBrowserBus, useEntityTypeGuid, useEntityTypeName } from "@Obsidian/Utility/block";
import { BlockMessages } from "@Obsidian/Utility/browserBus";

/** Provides a pattern for entity detail blocks. */
export default defineComponent({
    name: "DetailBlock",

    components: {
        AuditDetail,
        EntityTagList,
        Modal,
        Panel,
        RockButton,
        RockForm,
        RockSuspense,
        BadgeList
    },

    props: {
        /** The name of the entity. This will be used to construct the panel title. */
        name: {
            type: String as PropType<string>,
            required: false
        },

        /**
         * The full title to use for the panel. This will override the default
         * logic for setting the title.
         */
        title: {
            type: String as PropType<string>,
            required: false
        },

        /** The unique identifier of the entity type that this detail block represents. */
        entityTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        /** The friendly name of the entity type that this block represents. */
        entityTypeName: {
            type: String as PropType<string>,
            required: false
        },

        /** The identifier key of the entity being displayed by this block. */
        entityKey: {
            type: String as PropType<string | null>,
            required: false
        },

        /** If true then entity tags will be displayed in view mode. */
        isTagsVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** If true then the following action will be displayed in view mode. */
        isFollowVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** If true then badges for the entity will be displayed in view mode. */
        isBadgesVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** If true then the entity audit information will not be available. */
        isAuditHidden: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** If true then the entity security button will not be visible. */
        isSecurityHidden: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** If true then the individual will be able to enter edit mode. */
        isEditVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * If true then the delete button will be visible and emit the delete
         * event when clicked.
         */
        isDeleteVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * If true then the individual will be able to switch to fullscreen mode.
         */
        isFullScreenVisible: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        /** The current display mode for the detail panel. */
        mode: {
            type: Number as PropType<DetailPanelMode>,
            default: DetailPanelMode.View
        },

        /** Additional actions that should be displayed in the panel header. */
        headerActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        /**
         * Additional actions that should be displayed in the secondary
         * panel header zone. These are currently placed inside the ellipsis.
         */
        headerSecondaryActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        /**
         * Custom labels to display that will provide additional context
         * information about the entity. These are only shown in view mode.
         */
        labels: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        /**
         * Additional actions to display in the footer of the panel. These are
         * currently displayed as full buttons on the left of the footer.
         */
        footerActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        /**
         * Additional secondary actions to display in the footer of the panel.
         * These are currently displayed as icon buttons in the right of the footer.
         */
        footerSecondaryActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        /**
         * A function to be called when the individual clicks the Cancel button
         * while in edit mode. If provided, the function must return true to
         * allow the panel to switch back to view mode. If it returns false then
         * the panel will stay in edit mode. If it returns a string then the
         * person will be redirected to that URL.
         */
        onCancelEdit: {
            type: Function as PropType<() => boolean | string | PromiseLike<boolean | string>>,
            required: false
        },

        /**
         * A function to be called when the individual clicks the Edit button
         * while in view mode. If provided, the function must return true to
         * allow the panel to switch into edit mode. If it returns false then the
         * panel will stay in view mode.
         */
        onEdit: {
            type: Function as PropType<() => boolean | PromiseLike<boolean>>,
            required: false
        },

        /**
         * A function to be called when the individual clicks the Save button
         * while in edit mode. If provided, the function must return true to
         * allow the panel to switch back to view mode. If it returns false then
         * the panel will stay in edit mode. Return a string to redirect the
         * person to the URL contained in the string. Your logic to save the
         * entity to the server should be placed in this function.
         */
        onSave: {
            type: Function as PropType<() => boolean | string | PromiseLike<boolean | string>>,
            required: false
        },

        /**
         * A function to be called when the individual clicks the Delete button
         * while in view mode. Your logic to delete the entity should be placed
         * in this function. If the person should be redirected to another page
         * then return the URL. If the delete was aborted, return false.
         */
        onDelete: {
            type: Function as PropType<() => false | string | PromiseLike<false | string>>,
            required: false
        },

        /**
         * An optional string that may be displayed along with the confirmation
         * question in the modal after the delete is clicked on the entity.
         */
        additionalDeleteMessage: {
            type: String as PropType<string | null>,
            required: false
        }
    },

    emits: {
        "update:mode": (_value: DetailPanelMode) => true
    },

    setup(props, { emit }) {
        // #region Values

        const http = useHttp();
        const internalMode = useVModelPassthrough(props, "mode", emit);
        const isFormSubmitting = ref(false);
        const isEditModeLoading = ref(false);
        const isEntityFollowed = ref<boolean | null>(null);
        const showAuditDetailsModal = ref(false);
        const isPanelVisible = ref(true);
        const providedEntityTypeName = useEntityTypeName();
        const providedEntityTypeGuid = useEntityTypeGuid();
        const browserBus = useBlockBrowserBus();

        let formSubmissionSource: PromiseCompletionSource | null = null;
        let editModeReadyCompletionSource: PromiseCompletionSource | null = null;

        // AutoEditMode means we go directly into edit mode and, usually, have a
        // custom return URL to use when leaving edit mode. This can be used
        // in cases where it doesn't make sense for the detail block to show
        // a read-only view.
        const params = new URLSearchParams(window.location.search);
        const isAutoEditMode = ref(asBooleanOrNull(params.get("autoEdit")) ?? false);
        const autoEditReturnUrl = params.get("returnUrl");

        // #endregion

        // #region Computed Values

        /**
         * The entity type name for this block.
         */
        const entityTypeName = computed((): string => {
            return props.entityTypeName ?? providedEntityTypeName ?? "EntityTypeNotConfigured";
        });

        /**
         * The entity type unique identifier for this block.
         */
        const entityTypeGuid = computed((): Guid => {
            return props.entityTypeGuid ?? providedEntityTypeGuid ?? emptyGuid;
        });

        /**
         * Contains the title to be displayed in the panel depending on the
         * property values and the current state of the panel.
         */
        const panelTitle = computed((): string => {
            if (props.title) {
                return props.title;
            }

            switch (internalMode.value) {
                // If we are in view mode then we should be display the entity name.
                // If not, fall back on the entity type name.
                case DetailPanelMode.View:
                    return props.name ?? splitCase(entityTypeName.value);

                // If we are in Add mode then display "Add {Entity Type Name}"
                case DetailPanelMode.Add:
                    return `Add ${splitCase(entityTypeName.value)}`;

                // If we are in edit mode then we should be displaying the entity name.
                // If not, fall back on the entity type name.
                case DetailPanelMode.Edit:
                    return props.name ?? splitCase(entityTypeName.value);

                default:
                    return splitCase(entityTypeName.value);
            }
        });

        /** The CSS icon to display before the text in the panel header. */
        const panelTitleIconCssClass = computed((): string => {
            switch (internalMode.value) {
                // If we are in edit mode show an icon to indicate that to the individual.
                case DetailPanelMode.Edit:
                    return "fa fa-pencil";

                // If we are in add mode show an icon to indicate that to the individual.
                case DetailPanelMode.Add:
                    return "fa fa-plus";

                case DetailPanelMode.View:
                default:
                    return "";
            }
        });

        /** The secondary actions to show in the ellipsis of the panel header. */
        const internalHeaderSecondaryActions = computed((): PanelAction[] => {
            const actions: PanelAction[] = [];

            if (!props.isAuditHidden) {
                actions.push({
                    type: "default",
                    title: "Audit Details",
                    handler: onAuditClick
                });
            }

            // If the block has their own actions, add them in.
            if (props.headerSecondaryActions) {
                for (const action of props.headerSecondaryActions) {
                    actions.push(action);
                }
            }

            return actions;
        });

        /** The secondary footer actions to show in the block. */
        const internalFooterSecondaryActions = computed((): PanelAction[] => {
            const actions: PanelAction[] = [];

            // If the security button should be visible, we are in view mode and
            // we have a valid entity then show it.
            if (!props.isSecurityHidden && isViewMode.value && props.entityKey) {
                actions.push({
                    iconCssClass: "fa fa-lock",
                    title: "Edit Security",
                    type: "default",
                    handler: onSecurityClick
                });
            }

            // If the block has their own actions, add them in.
            if (props.footerSecondaryActions) {
                for (const action of props.footerSecondaryActions) {
                    actions.push(action);
                }
            }

            return actions;
        });

        /** True when we are in view mode. */
        const isViewMode = computed((): boolean => {
            return internalMode.value === DetailPanelMode.View && !isAutoEditMode.value;
        });

        /** True when we are in one of the edit modes (edit or add). */
        const isEditMode = computed((): boolean => {
            return internalMode.value === DetailPanelMode.Edit || internalMode.value === DetailPanelMode.Add;
        });

        /** True when the edit button should be visible. */
        const isEditModeVisible = computed((): boolean => {
            return isEditMode.value || isEditModeLoading.value;
        });

        /** True if the panel should be shown on screen or False if it should be in the DOM but hidden. */
        const isPanelShown = computed((): boolean => {
            return !isAutoEditMode.value || isEditMode.value;
        });

        /** True if we have any labels to display. */
        const hasLabels = computed((): boolean => {
            return !!props.labels && props.labels.length > 0;
        });

        /** The header actions that should be displayed in the panel title area. */
        const headerActions = computed((): PanelAction[] => {
            const actions = [...props.headerActions ?? []];

            // Add in the follow action if we are in view mode and it has been requested.
            if (props.isFollowVisible && isViewMode.value) {
                actions.push({
                    type: isEntityFollowed.value ? "primary" : "default",
                    iconCssClass: isEntityFollowed.value ? "fa fa-star" : "fa fa-star-o",
                    handler: onFollowClick,
                    title: isEntityFollowed.value ? `You are currently following ${props.name}.` : `Click to follow ${props.name}.`
                });
            }

            return actions;
        });

        // #endregion

        // #region Functions

        /**
         * Gets the CSS class to use for an action when it is displayed
         * as a plain icon.
         *
         * @param action The action to be displayed.
         *
         * @returns A string that contains the CSS classes to apply to the DOM element.
         */
        const getClassForIconAction = (action: PanelAction): string => {
            let cssClass = action.handler ? "action clickable" : "action";

            if (action.type !== "default" && action.type !== "link") {
                cssClass += ` text-${action.type}`;
            }

            return cssClass;
        };

        /**
         * Gets the CSS class to use for an action when it is displayed
         * as a label.
         *
         * @param action The action to be displayed.
         *
         * @returns A string that contains the CSS classes to apply to the DOM element.
         */
        const getClassForLabelAction = (action: PanelAction): string => {
            let cssClass = action.handler ? "label clickable" : "label";

            if (action.type === "link") {
                cssClass += " label-default";
            }
            else {
                cssClass += ` label-${action.type}`;
            }

            return cssClass;
        };

        /**
         * Get the icon CSS class to use for the action when it is displayed
         * as a plain icon.
         *
         * @param action The action to be displayed.
         *
         * @returns A string that contains the CSS classes to apply to the DOM element.
         */
        const getActionIconCssClass = (action: PanelAction): string => {
            // Provide a default value if they didn't give us one.
            return action.iconCssClass || "fa fa-square";
        };

        /**
         * Get the current followed state for the entity we are displaying in
         * this detail block.
         */
        const getEntityFollowedState = async (): Promise<void> => {
            // If we don't have an entity then mark the state as "unknown".
            if (areEqual(entityTypeGuid.value, emptyGuid)
                || !props.entityKey) {
                isEntityFollowed.value = null;
                return;
            }

            const data: FollowingGetFollowingOptionsBag = {
                entityTypeGuid: entityTypeGuid.value,
                entityKey: props.entityKey
            };

            const response = await http.post<FollowingGetFollowingResponseBag>("/api/v2/Controls/FollowingGetFollowing", undefined, data);

            isEntityFollowed.value = response.isSuccess && response.data && response.data.isFollowing;
        };

        // #endregion

        // #region Event Handlers

        /**
         * Called when the security button has been clicked.
         * Shows the edit security modal.
         *
         * @param event The event that triggered this handler.
         */
        const onSecurityClick = (): void => {
            if (props.entityKey) {
                showSecurity(entityTypeGuid.value, props.entityKey, props.entityTypeName);
            }
        };

        /**
         * Called when the cancel button has been clicked while in edit mode.
         * Check with the block if we should return to view mode or stay
         * in edit mode.
         */
        const onEditCancelClick = async (): Promise<void> => {
            if (props.onCancelEdit) {
                let result = props.onCancelEdit();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result === false) {
                    return;
                }

                if (isAutoEditMode.value) {
                    isAutoEditMode.value = false;

                    if (autoEditReturnUrl) {
                        window.location.href = makeUrlRedirectSafe(autoEditReturnUrl);

                        // Don't switch back to view mode.
                        return;
                    }
                }

                if (typeof result === "string") {
                    window.location.href = makeUrlRedirectSafe(result);

                    // Don't switch back to view mode.
                    return;
                }
            }

            internalMode.value = DetailPanelMode.View;
            browserBus.publish(BlockMessages.EndEdit);
        };

        /**
         * Called when the edit button has been clicked. Check with the block
         * if we should switch to edit mode or stay in view mode.
         */
        const onEditClick = async (): Promise<boolean> => {
            if (props.onEdit) {
                let result = props.onEdit();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result !== true) {
                    return false;
                }
            }

            // If we are in auto edit mode, the panel is currently hidden. Show it.
            if (isAutoEditMode.value) {
                isPanelVisible.value = true;
            }

            // Block has given go ahead for edit mode, note that we are currently
            // switching to edit mode and waiting for the view to load.
            isEditModeLoading.value = true;

            // Wait for the RockSuspense control to indicate that the view is
            // fully loaded and ready to display.
            editModeReadyCompletionSource = new PromiseCompletionSource();
            await editModeReadyCompletionSource.promise;

            // Perform the final switch into edit mode.
            browserBus.publish(BlockMessages.BeginEdit);
            internalMode.value = props.entityKey ? DetailPanelMode.Edit : DetailPanelMode.Add;
            isEditModeLoading.value = false;
            editModeReadyCompletionSource = null;

            return true;
        };

        /**
         * Called when the RockSuspense control for the Edit panel has detected
         * that it is fully loaded and ready to display.
         */
        const onEditSuspenseReady = (): void => {
            editModeReadyCompletionSource?.resolve();
        };

        /**
         * Called when the Save button has been clicked. Trigger the submit
         * operation on the form to perform validation.
         */
        const onSaveClick = async (): Promise<void> => {
            // Trigger the form to begin processing and then wait for it to
            // fully complete. This makes sure the Save button stays disabled
            // until the action is complete so they can't double click.
            formSubmissionSource = new PromiseCompletionSource();
            isFormSubmitting.value = true;
            await formSubmissionSource.promise;
            isFormSubmitting.value = false;
        };

        /**
         * Called when the form submission has been validated and the entity
         * should now be saved. Allow the block a chance to save the data and
         * then switch back to edit mode if we are allowed to do so.
         */
        const onSaveSubmit = async (): Promise<void> => {
            // Do everything in a try-finally block in case the block code
            // throws an exception. That way we don't get stuck in a state
            // where the Save button is forever disabled.
            try {
                // Give the block a chance to perform the actual save as well
                // as abort the operation so we stay in edit mode.
                if (props.onSave) {
                    let result = props.onSave();

                    if (isPromise(result)) {
                        result = await result;
                    }

                    if (result === false) {
                        return;
                    }

                    if (isAutoEditMode.value) {
                        isAutoEditMode.value = false;

                        if (autoEditReturnUrl) {
                            window.location.href = makeUrlRedirectSafe(autoEditReturnUrl);

                            // Don't switch back to view mode.
                            return;
                        }
                    }

                    if (typeof result === "string") {
                        window.location.href = makeUrlRedirectSafe(result);

                        // Don't switch back to view mode.
                        return;
                    }
                }

                internalMode.value = DetailPanelMode.View;
                browserBus.publish(BlockMessages.EndEdit);
            }
            finally {
                if (formSubmissionSource !== null) {
                    formSubmissionSource.resolve();
                    formSubmissionSource = null;
                }
            }
        };

        /**
         * Called when the delete button has been clicked. Give the block
         * a chance to perform the deletion and redirect.
         */
        const onDeleteClick = async (): Promise<void> => {
            if (props.onDelete) {
                if (!await confirmDelete(entityTypeName.value, props.additionalDeleteMessage ?? "")) {
                    return;
                }

                let result = props.onDelete();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result === false) {
                    return;
                }

                if (typeof result === "string") {
                    window.location.href = makeUrlRedirectSafe(result);
                }
            }
        };

        /**
         * Called when any of the panel actions have been clicked. If there is
         * an event handler attached then trigger the handler.
         *
         * @param action The action that was clicked.
         * @param event The DOM event that triggered the click.
         */
        const onActionClick = (action: PanelAction, event: Event): void => {
            if (action.handler && !action.disabled) {
                action.handler(event);
            }
        };

        /**
         * Called when the follow panel action has been clicked. Attempt to
         * toggle the followed state of the entity.
         */
        const onFollowClick = async (): Promise<void> => {
            // Shouldn't really happen, but just make sure we have everything.
            if (isEntityFollowed.value === null
                || areEqual(entityTypeGuid.value, emptyGuid)
                || !props.entityKey) {
                return;
            }

            const data: FollowingSetFollowingOptionsBag = {
                entityTypeGuid: entityTypeGuid.value,
                entityKey: props.entityKey,
                isFollowing: !isEntityFollowed.value
            };

            const response = await http.post("/api/v2/Controls/FollowingSetFollowing", undefined, data);

            // If we got a 200 OK response then we can toggle our internal state.
            if (response.isSuccess) {
                isEntityFollowed.value = !isEntityFollowed.value;
            }
            else {
                await alert("Unable to update followed state.");
            }
        };

        const onAuditClick = async (): Promise<void> => {
            showAuditDetailsModal.value = true;
        };

        // #endregion

        // Watch for the RockForm component to toggle the isFormSubmitting value
        // back off. This indicates it has finished submitting the form.
        watch(isFormSubmitting, () => {
            if (isFormSubmitting.value === false && formSubmissionSource !== null) {
                formSubmissionSource.resolve();
                formSubmissionSource = null;
            }
        });

        // Watch for the isFollowVisible value to change, and if we haven't loaded
        // the initial followed state yet then begin loading it.
        watch(() => props.isFollowVisible, () => {
            if (props.isFollowVisible && isEntityFollowed.value === null) {
                getEntityFollowedState();
            }
        });

        // If the following icon is visible then immediately get the followed state.
        if (props.isFollowVisible) {
            getEntityFollowedState();
        }

        if (isAutoEditMode.value) {
            isPanelVisible.value = false;

            onEditClick();
        }

        return {
            entityTypeName,
            entityTypeGuid,
            hasLabels,
            internalFooterSecondaryActions,
            internalHeaderSecondaryActions,
            panelTitle,
            panelTitleIconCssClass,
            getActionIconCssClass,
            getClassForIconAction,
            getClassForLabelAction,
            headerActions,
            isEditMode,
            isEditModeVisible,
            isFormSubmitting,
            isPanelShown,
            isPanelVisible,
            isViewMode,
            onActionClick,
            onDeleteClick,
            onEditCancelClick,
            onEditClick,
            onEditSuspenseReady,
            onSaveClick,
            onSaveSubmit,
            showAuditDetailsModal
        };
    },

    template: `
<Panel v-if="isPanelVisible"
    v-show="isPanelShown"
    type="block"
    :title="panelTitle"
    :titleIconCssClass="panelTitleIconCssClass"
    :hasFullscreen="isFullScreenVisible"
    :headerSecondaryActions="internalHeaderSecondaryActions">

    <template #headerActions>
        <span v-for="action in headerActions" :class="getClassForIconAction(action)" :title="action.title" @click="onActionClick(action, $event)">
            <i :class="getActionIconCssClass(action)"></i>
        </span>
    </template>

    <template v-if="!isEditMode && (hasLabels || isTagsVisible)" #subheaderLeft>
        <div class="d-flex">
            <div v-if="hasLabels" class="label-group">
                <span v-for="action in labels" :class="getClassForLabelAction(action)" @click="onActionClick(action, $event)">
                    <template v-if="action.title">{{ action.title }}</template>
                    <i v-else :class="action.iconCssClass"></i>
                </span>
            </div>

            <div v-if="isTagsVisible && hasLabels" style="width: 2px; background-color: #eaedf0; margin: 0px 12px;"></div>

            <div v-if="isTagsVisible" class="flex-grow-1">
                <EntityTagList :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />
            </div>
        </div>
    </template>

    <template v-if="!isEditMode && isBadgesVisible" #subheaderRight>
        <BadgeList :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />
    </template>

    <template v-if="$slots.helpContent" #helpContent>
        <slot name="helpContent" />
    </template>

    <template #footerActions>
        <template v-if="isEditMode">
            <RockButton btnType="primary" autoDisable @click="onSaveClick" shortcutKey="s">Save</RockButton>
            <RockButton btnType="link" @click="onEditCancelClick" shortcutKey="c">Cancel</RockButton>
        </template>

        <template v-else>
            <RockButton v-if="isEditVisible" btnType="primary" @click="onEditClick" autoDisable shortcutKey="e">Edit</RockButton>
            <RockButton v-if="isDeleteVisible" btnType="link" @click="onDeleteClick" autoDisable>Delete</RockButton>
        </template>

        <RockButton v-for="action in footerActions" :btnType="action.type" @click="onActionClick(action, $event)">
            <template v-if="action.title">{{ action.title }}</template>
            <i v-else :class="action.iconCssClass"></i>
        </RockButton>
    </template>

    <template #footerSecondaryActions>
        <RockButton v-for="action in internalFooterSecondaryActions" :btnType="action.type" btnSize="sm" :title="action.title" @click="onActionClick(action, $event)" :disabled="action.disabled">
            <i :class="getActionIconCssClass(action)"></i>
        </RockButton>
    </template>

    <template #default>
        <v-style>
            .panel-flex .label-group > .label + * {
                margin-left: 8px;
            }
        </v-style>

        <RockForm v-if="isEditModeVisible" v-show="isEditMode" @submit="onSaveSubmit" v-model:submit="isFormSubmitting">
            <RockSuspense @ready="onEditSuspenseReady">
                <slot name="edit" />
            </RockSuspense>
        </RockForm>

        <slot v-if="isViewMode" name="view" />
    </template>
</Panel>

<Modal v-model="showAuditDetailsModal" title="Audit Details">
    <AuditDetail :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />
</Modal>
`
});
