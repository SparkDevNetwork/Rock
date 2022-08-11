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

import { computed, defineComponent, ref } from "vue";
import Alert from "@Obsidian/Controls/alert";
import { EntityType } from "@Obsidian/SystemGuids";
import DetailBlock from "@Obsidian/Templates/detailBlock";
import { DetailPanelMode } from "@Obsidian/Types/Controls/detailPanelMode";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import EditPanel from "./AssessmentTypeDetail/editPanel.partial";
import ViewPanel from "./AssessmentTypeDetail/viewPanel.partial";
import { getSecurityGrant, provideSecurityGrant, refreshDetailAttributes, useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { NavigationUrlKey } from "./AssessmentTypeDetail/types";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { AssessmentTypeBag } from "@Obsidian/ViewModels/Blocks/Crm/AssessmentTypeDetail/assessmentTypeBag";
import { AssessmentTypeDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Crm/AssessmentTypeDetail/assessmentTypeDetailOptionsBag";
import { debounce } from "@Obsidian/Utility/util";

export default defineComponent({
    name: "Crm.AssessmentTypeDetail",

    components: {
        Alert,
        EditPanel,
        DetailBlock,
        ViewPanel
    },

    setup() {
        const config = useConfigurationValues<DetailBlockBox<AssessmentTypeBag, AssessmentTypeDetailOptionsBag>>();
        const invokeBlockAction = useInvokeBlockAction();
        const securityGrant = getSecurityGrant(config.securityGrantToken);

        // #region Values

        const blockError = ref("");
        const errorMessage = ref("");

        const assessmentTypeViewBag = ref(config.entity);
        const assessmentTypeEditBag = ref<AssessmentTypeBag | null>(null);

        const panelMode = ref(DetailPanelMode.View);

        // The properties that are being edited in the UI. This is used to
        // inform the server which incoming values have valid data in them.
        const validProperties = [
            "assessmentPath",
            "assessmentResultsPath",
            "description",
            "isActive",
            "minimumDaysToRetake",
            "requiresRequest",
            "title"
        ];

        const refreshAttributesDebounce = debounce(() => refreshDetailAttributes(assessmentTypeEditBag, validProperties, invokeBlockAction), undefined, true);

        // #endregion

        // #region Computed Values

        /**
         * The entity name to display in the block panel.
         */
        const panelName = computed((): string => {
            return assessmentTypeViewBag.value?.title ?? "";
        });

        /**
         * The identifier key value for this entity.
         */
        const entityKey = computed((): string => {
            return assessmentTypeViewBag.value?.idKey ?? "";
        });

        /**
         * Additional labels to display in the block panel.
         */
        const blockLabels = computed((): PanelAction[] | null => {
            const labels: PanelAction[] = [];

            if (panelMode.value !== DetailPanelMode.View) {
                return null;
            }

            if (assessmentTypeViewBag.value?.isActive === true) {
                labels.push({
                    iconCssClass: "fa fa-lightbulb",
                    title: "Active",
                    type: "success"
                });
            }
            else {
                labels.push({
                    iconCssClass: "far fa-lightbulb",
                    title: "Inactive",
                    type: "danger"
                });
            }

            return labels;
        });

        const isEditable = computed((): boolean => {
            return config.isEditable === true;
        });

        const options = computed((): AssessmentTypeDetailOptionsBag => {
            return config.options ?? {};
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for the Cancel button being clicked while in Edit mode.
         * Handles redirect to parent page if creating a new entity.
         *
         * @returns true if the panel should leave edit mode; otherwise false.
         */
        const onCancelEdit = async (): Promise<boolean> => {
            if (!assessmentTypeEditBag.value?.idKey) {
                if (config.navigationUrls?.[NavigationUrlKey.ParentPage]) {
                    window.location.href = config.navigationUrls[NavigationUrlKey.ParentPage];
                }

                return false;
            }

            return true;
        };

        /**
         * Event handler for the Delete button being clicked. Sends the
         * delete request to the server and then redirects to the target page.
         */
        const onDelete = async (): Promise<void> => {
            errorMessage.value = "";

            const result = await invokeBlockAction<string>("Delete", {
                key: assessmentTypeViewBag.value?.idKey
            });

            if (result.isSuccess && result.data) {
                window.location.href = result.data;
            }
            else {
                errorMessage.value = result.errorMessage ?? "Unknown error while trying to delete assessment type.";
            }
        };

        /**
         * Event handler for the Edit button being clicked. Request the edit
         * details from the server and then enter edit mode.
         *
         * @returns true if the panel should enter edit mode; otherwise false.
         */
        const onEdit = async (): Promise<boolean> => {
            const result = await invokeBlockAction<DetailBlockBox<AssessmentTypeBag, AssessmentTypeDetailOptionsBag>>("Edit", {
                key: assessmentTypeViewBag.value?.idKey
            });

            if (result.isSuccess && result.data && result.data.entity) {
                assessmentTypeEditBag.value = result.data.entity;

                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Event handler for when a value has changed that has an associated
         * C# property name. This is used to detect changes to values that
         * might cause qualified attributes to either show up or not show up.
         * 
         * @param propertyName The name of the C# property that was changed.
         */
        const onPropertyChanged = (propertyName: string): void => {
            // If we don't have any qualified attribute properties or this property
            // is not one of them then do nothing.
            if (!config.qualifiedAttributeProperties || !config.qualifiedAttributeProperties.some(n => n.toLowerCase() === propertyName.toLowerCase())) {
                return;
            }

            refreshAttributesDebounce();
        };

        /**
         * Event handler for the panel's Save event. Send the data to the server
         * to be saved and then leave edit mode or redirect to target page.
         *
         * @returns true if the panel should leave edit mode; otherwise false.
         */
        const onSave = async (): Promise<boolean> => {
            errorMessage.value = "";

            const data: DetailBlockBox<AssessmentTypeBag, AssessmentTypeDetailOptionsBag> = {
                entity: assessmentTypeEditBag.value,
                isEditable: true,
                validProperties: validProperties
            };

            const result = await invokeBlockAction<AssessmentTypeBag | string>("Save", {
                box: data
            });

            if (result.isSuccess && result.data) {
                if (result.statusCode === 200 && typeof result.data === "object") {
                    assessmentTypeViewBag.value = result.data;

                    return true;
                }
                else if (result.statusCode === 201 && typeof result.data === "string") {
                    window.location.href = result.data;

                    return false;
                }
            }

            errorMessage.value = result.errorMessage ?? "Unknown error while trying to save assessment type.";

            return false;
        };

        // #endregion

        provideSecurityGrant(securityGrant);

        // Handle any initial error conditions or the need to go into edit mode.
        if (config.errorMessage) {
            blockError.value = config.errorMessage;
        }
        else if (!config.entity) {
            blockError.value = "The specified assessment type could not be viewed.";
        }
        else if (!config.entity.idKey) {
            assessmentTypeEditBag.value = config.entity;
            panelMode.value = DetailPanelMode.Add;
        }

        return {
            assessmentTypeViewBag,
            assessmentTypeEditBag,
            blockError,
            blockLabels,
            entityKey,
            entityTypeGuid: EntityType.AssessmentType,
            errorMessage,
            isEditable,
            onCancelEdit,
            onDelete,
            onEdit,
            onPropertyChanged,
            onSave,
            options,
            panelMode,
            panelName
        };
    },

    template: `
<Alert v-if="blockError" alertType="warning" v-text="blockError" />

<Alert v-if="errorMessage" alertType="danger" v-text="errorMessage" />

<DetailBlock v-if="!blockError"
    v-model:mode="panelMode"
    :name="panelName"
    :labels="blockLabels"
    :entityKey="entityKey"
    :entityTypeGuid="entityTypeGuid"
    entityTypeName="AssessmentType"
    :isAuditHidden="false"
    :isBadgesVisible="true"
    :isDeleteVisible="isEditable"
    :isEditVisible="isEditable"
    :isFollowVisible="false"
    :isSecurityHidden="false"
    @cancelEdit="onCancelEdit"
    @delete="onDelete"
    @edit="onEdit"
    @save="onSave">
    <template #view>
        <ViewPanel :modelValue="assessmentTypeViewBag" :options="options" />
    </template>

    <template #edit>
        <EditPanel v-model="assessmentTypeEditBag" :options="options" @propertyChanged="onPropertyChanged" />
    </template>
</DetailBlock>
`
});
