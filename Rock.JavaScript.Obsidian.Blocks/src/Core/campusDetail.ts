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
import DetailBlock from "@Obsidian/Templates/detailBlock";
import { useConfigurationValues, useInvokeBlockAction, useSecurityGrantToken } from "@Obsidian/Utility/block";
import { emptyGuid } from "@Obsidian/Utility/guid";
import EditPanel from "./CampusDetail/editPanel";
import { CampusDetailOptionsBag, CampusBag, DetailBlockBox, NavigationUrlKey } from "./CampusDetail/types";
import ViewPanel from "./CampusDetail/viewPanel";
import { DetailPanelMode } from "@Obsidian/Types/Controls/detailPanelMode";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";

export default defineComponent({
    name: "Core.CampusDetail",

    components: {
        Alert,
        EditPanel,
        DetailBlock,
        ViewPanel
    },

    setup() {
        const config = useConfigurationValues<DetailBlockBox<CampusBag, CampusDetailOptionsBag>>();
        const invokeBlockAction = useInvokeBlockAction();
        const securityGrant = useSecurityGrantToken(config.securityGrantToken);

        // #region Values

        const blockError = ref("");
        const errorMessage = ref("");

        const campusViewBag = ref(config.entity);
        const campusEditBag = ref<CampusBag | null>(null);

        const panelMode = ref(DetailPanelMode.View);

        // #endregion

        // #region Computed Values

        /**
         * The name to display in the panel title.
         */
        const panelName = computed((): string => campusViewBag.value?.name ?? "");

        /**
         * Additional labels to display in the block panel.
         */
        const panelLabels = computed((): PanelAction[] => {
            const labels: PanelAction[] = [];

            if (isEditMode.value) {
                return labels;
            }

            if (campusViewBag.value?.isActive === true) {
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
            return config.isEditable === true && campusViewBag.value?.isSystem !== true;
        });

        const isEditMode = computed((): boolean => panelMode.value === DetailPanelMode.Edit || panelMode.value === DetailPanelMode.Add);

        const options = computed((): CampusDetailOptionsBag => {
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
            if (campusEditBag.value?.guid === emptyGuid) {
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
                guid: campusViewBag.value?.guid
            });

            if (result.isSuccess && result.data) {
                window.location.href = result.data;
            }
            else {
                errorMessage.value = result.errorMessage ?? "Unknown error while trying to delete campus.";
            }
        };

        /**
         * Event handler for the Edit button being clicked. Request the edit
         * details from the server and then enter edit mode.
         *
         * @returns true if the panel should enter edit mode; otherwise false.
         */
        const onEdit = async (): Promise<boolean> => {
            const result = await invokeBlockAction<DetailBlockBox<CampusBag, CampusDetailOptionsBag>>("Edit", {
                guid: campusViewBag.value?.guid
            });

            if (result.isSuccess && result.data && result.data.entity) {
                campusEditBag.value = result.data.entity;

                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Event handler for the panel's Save event. Send the data to the server
         * to be saved and then leave edit mode or redirect to target page.
         *
         * @returns true if the panel should leave edit mode; otherwise false.
         */
        const onSave = async (): Promise<boolean> => {
            errorMessage.value = "";

            const data: DetailBlockBox<CampusBag, CampusDetailOptionsBag> = {
                entity: campusEditBag.value,
                validProperties: [
                    "attributeValues",
                    "campusSchedules",
                    "campusStatusValue",
                    "campusTypeValue",
                    "description",
                    "isActive",
                    "leaderPersonAlias",
                    "location",
                    "name",
                    "phoneNumber",
                    "serviceTimes",
                    "shortCode",
                    "timeZoneId",
                    "url"
                ]
            };

            const result = await invokeBlockAction<CampusBag | string>("Save", {
                box: data
            });

            if (result.isSuccess && result.data) {
                if (result.statusCode === 200 && typeof result.data === "object") {
                    campusViewBag.value = result.data;

                    return true;
                }
                else if (result.statusCode === 201 && typeof result.data === "string") {
                    window.location.href = result.data;

                    return false;
                }
            }

            errorMessage.value = result.errorMessage ?? "Unknown error while trying to save campus.";

            return false;
        };

        // #endregion

        // Handle any initial error conditions or the need to go into edit mode.
        if (config.errorMessage) {
            blockError.value = config.errorMessage;
        }
        else if (!config.entity) {
            blockError.value = "The specified campus could not be viewed.";
        }
        else if (config.entity.guid === emptyGuid) {
            campusEditBag.value = config.entity;
            panelMode.value = DetailPanelMode.Add;
        }

        return {
            blockError,
            campusViewBag,
            campusEditBag,
            errorMessage,
            isEditable,
            isEditMode,
            onCancelEdit,
            onDelete,
            onEdit,
            onSave,
            options,
            panelLabels,
            panelMode,
            panelName
        };
    },

    template: `
<Alert v-if="blockError" alertType="warning">
    {{ blockError }}
</Alert>

<Alert v-if="errorMessage" alertType="danger">
    {{ errorMessage }}
</Alert>

<DetailBlock v-if="!blockError"
    v-model:mode="panelMode"
    :name="panelName"
    :labels="panelLabels"
    entityTypeName="Campus"
    :isEditVisible="isEditable"
    :isDeleteVisible="isEditable"
    @cancelEdit="onCancelEdit"
    @delete="onDelete"
    @edit="onEdit"
    @save="onSave">
    <EditPanel v-if="isEditMode" v-model="campusEditBag" :options="options" />
    <ViewPanel v-else :modelValue="campusViewBag" :options="options" />
</DetailBlock>
`
});
