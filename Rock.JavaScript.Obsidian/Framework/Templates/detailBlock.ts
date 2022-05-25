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
import Panel from "@Obsidian/Controls/panel";
import { Guid } from "@Obsidian/Types";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { DetailPanelMode } from "@Obsidian/Types/Controls/detailPanelMode";
import { isPromise, PromiseCompletionSource } from "@Obsidian/Utility/promiseUtils";
import BadgeList from "@Obsidian/Controls/badgeList";
import EntityTagList from "@Obsidian/Controls/entityTagList";
import RockButton from "@Obsidian/Controls/rockButton";
import RockForm from "@Obsidian/Controls/rockForm";
import RockSuspense from "@Obsidian/Controls/rockSuspense";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { confirmDelete } from "@Obsidian/Utility/dialogs";

// Define jQuery and Rock for showing security modal.
declare function $(value: unknown): unknown;
// eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/naming-convention
declare const Rock: any;

/** Provides a pattern for entity detail blocks. */
export default defineComponent({
    name: "DetailBlock",

    components: {
        EntityTagList,
        Panel,
        RockButton,
        RockForm,
        RockSuspense,
        BadgeList
    },

    props: {
        name: {
            type: String as PropType<string>,
            required: false
        },

        title: {
            type: String as PropType<string>,
            required: false
        },

        entityTypeGuid: {
            type: String as PropType<Guid>,
            required: true
        },

        entityTypeName: {
            type: String as PropType<string>,
            required: true
        },

        entityKey: {
            type: String as PropType<string | null>,
            required: false
        },

        isTagsVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isFollowVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isBadgesVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isAuditHidden: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isSecurityHidden: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isEditVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isDeleteVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        mode: {
            type: Number as PropType<DetailPanelMode>,
            default: DetailPanelMode.View
        },

        headerActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        headerSecondaryActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        labels: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        footerActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        footerSecondaryActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
        },

        onCancelEdit: {
            type: Function as PropType<() => boolean | PromiseLike<boolean>>,
            required: false
        },

        onEdit: {
            type: Function as PropType<() => boolean | PromiseLike<boolean>>,
            required: false
        },

        onSave: {
            type: Function as PropType<() => boolean | PromiseLike<boolean>>,
            required: false
        },

        onDelete: {
            type: Function as PropType<() => void | PromiseLike<void>>,
            required: false
        }
    },

    emits: {
        "update:mode": (_value: DetailPanelMode) => true
    },

    setup(props, { emit }) {
        // #region Values

        const internalMode = useVModelPassthrough(props, "mode", emit);
        const isFormSubmitting = ref(false);
        const isEditModeLoading = ref(false);

        let formSubmissionSource: PromiseCompletionSource | null = null;
        let editModeReadyCompletionSource: PromiseCompletionSource | null = null;

        // #endregion

        // #region Computed Values

        const panelTitle = computed((): string => {
            switch (internalMode.value) {
                case DetailPanelMode.View:
                    return props.name ?? props.entityTypeName;

                case DetailPanelMode.Edit:
                case DetailPanelMode.Add:
                default:
                    return props.entityTypeName;
            }
        });

        const panelTitleIconCssClass = computed((): string => {
            switch (internalMode.value) {
                case DetailPanelMode.Edit:
                    return "fa fa-pencil";

                case DetailPanelMode.Add:
                    return "fa fa-plus";

                case DetailPanelMode.View:
                default:
                    return "";
            }
        });

        const internalHeaderSecondaryActions = computed((): PanelAction[] => {
            const actions: PanelAction[] = [];

            if (props.headerSecondaryActions) {
                for (const action of props.headerSecondaryActions) {
                    actions.push(action);
                }
            }

            return actions;
        });

        const internalFooterSecondaryActions = computed((): PanelAction[] => {
            const actions: PanelAction[] = [];

            if (!props.isSecurityHidden && props.entityKey) {
                actions.push({
                    iconCssClass: "fa fa-lock",
                    title: "Edit Security",
                    type: "default",
                    handler: onSecurityClick
                });
            }

            if (props.footerSecondaryActions) {
                for (const action of props.footerSecondaryActions) {
                    actions.push(action);
                }
            }

            return actions;
        });

        const isViewMode = computed((): boolean => {
            return internalMode.value === DetailPanelMode.View;
        });

        const isEditMode = computed((): boolean => {
            return internalMode.value === DetailPanelMode.Edit || internalMode.value === DetailPanelMode.Add;
        });

        const isEditModeVisible = computed((): boolean => {
            return isEditMode.value || isEditModeLoading.value;
        });

        const hasLabels = computed((): boolean => {
            return !!props.labels && props.labels.length > 0;
        });

        // #endregion

        // #region Functions

        const getClassForIconAction = (action: PanelAction): string => {
            let cssClass = action.handler ? "action clickable" : "action";

            if (action.type !== "default" && action.type !== "link") {
                cssClass += ` text-${action.type}`;
            }

            return cssClass;
        };

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

        const getActionIconCssClass = (action: PanelAction): string => {
            return action.iconCssClass || "fa fa-square";
        };

        // #endregion

        // #region Event Handlers

        const onSecurityClick = (event: Event): void => {
            Rock.controls.modal.show($(event.target), `/Secure/${props.entityTypeGuid}/${props.entityKey}?t=Secure ${props.entityTypeName}&pb=&sb=Done`);
        };

        const onEditCancelClick = async (): Promise<void> => {
            if (props.onCancelEdit) {
                let result = props.onCancelEdit();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result === false) {
                    return;
                }
            }

            internalMode.value = DetailPanelMode.View;
        };

        const onEditClick = async (): Promise<void> => {
            if (props.onEdit) {
                let result = props.onEdit();

                if (isPromise(result)) {
                    result = await result;
                }

                if (result !== true) {
                    return;
                }
            }

            isEditModeLoading.value = true;

            editModeReadyCompletionSource = new PromiseCompletionSource();
            await editModeReadyCompletionSource.promise;

            internalMode.value = props.entityKey ? DetailPanelMode.Edit : DetailPanelMode.Add;
            isEditModeLoading.value = false;
            editModeReadyCompletionSource = null;
        };

        const onEditSuspenseReady = (): void => {
            editModeReadyCompletionSource?.resolve();
        };

        const onSaveClick = async (): Promise<void> => {
            formSubmissionSource = new PromiseCompletionSource();
            isFormSubmitting.value = true;
            await formSubmissionSource.promise;
        };

        const onSaveSubmit = async (): Promise<void> => {
            try {
                if (props.onSave) {
                    let result = props.onSave();

                    if (isPromise(result)) {
                        result = await result;
                    }

                    if (result !== true) {
                        return;
                    }
                }
            }
            finally {
                if (formSubmissionSource !== null) {
                    formSubmissionSource.resolve();
                }
            }

            internalMode.value = DetailPanelMode.View;
        };

        const onDeleteClick = async (): Promise<void> => {
            if (props.onDelete) {
                if (!await confirmDelete(props.entityTypeName)) {
                    return;
                }

                const result = props.onDelete();

                if (isPromise(result)) {
                    await result;
                }
            }
        };

        const onActionClick = (action: PanelAction, event: Event): void => {
            if (action.handler) {
                action.handler(event);
            }
        };

        // #endregion

        watch(isFormSubmitting, () => {
            if (isFormSubmitting.value === false && formSubmissionSource !== null) {
                formSubmissionSource.resolve();
            }
        });

        return {
            hasLabels,
            internalFooterSecondaryActions,
            internalHeaderSecondaryActions,
            panelTitle,
            panelTitleIconCssClass,
            getActionIconCssClass,
            getClassForIconAction,
            getClassForLabelAction,
            isEditMode,
            isEditModeVisible,
            isFormSubmitting,
            isViewMode,
            onActionClick,
            onDeleteClick,
            onEditCancelClick,
            onEditClick,
            onEditSuspenseReady,
            onSaveClick,
            onSaveSubmit
        };
    },

    template: `
<Panel type="block"
    :title="panelTitle"
    :titleIconCssClass="panelTitleIconCssClass"
    :hasFullscreen="true"
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
            <RockButton btnType="primary" autoDisable @click="onSaveClick">Save</RockButton>
            <RockButton btnType="link" @click="onEditCancelClick">Cancel</RockButton>
        </template>

        <template v-else>
            <RockButton v-if="isEditVisible" btnType="primary" @click="onEditClick" autoDisable>Edit</RockButton>
            <RockButton v-if="isDeleteVisible" btnType="link" @click="onDeleteClick" autoDisable>Delete</RockButton>
        </template>

        <RockButton v-for="action in footerActions" :btnType="action.type" @click="onActionClick(action, $event)">
            <template v-if="action.title">{{ action.title }}</template>
            <i v-else :class="action.iconCssClass"></i>
        </RockButton>
    </template>

    <template #footerSecondaryActions>
        <RockButton v-for="action in internalFooterSecondaryActions" :btnType="action.type" btnSize="sm" :title="action.title" @click="onActionClick(action, $event)">
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
`
});
