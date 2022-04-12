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
import Alert from "../../Elements/alert.vue";
import PaneledDetailBlockTemplate from "../../Templates/paneledDetailBlockTemplate";
import { useConfigurationValues, useInvokeBlockAction } from "../../Util/block";
import { emptyGuid } from "../../Util/guid";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import EditPanel from "./CampusDetail/editPanel";
import { CampusDetailOptionsBag, CampusBag, DetailBlockBox, NavigationUrlKey } from "./CampusDetail/types";
import ViewPanel from "./CampusDetail/viewPanel";

export default defineComponent({
    name: "Core.CampusDetail",

    components: {
        Alert,
        EditPanel,
        PaneledDetailBlockTemplate,
        ViewPanel
    },

    setup() {
        const config = useConfigurationValues<DetailBlockBox<CampusBag, CampusDetailOptionsBag>>();
        const invokeBlockAction = useInvokeBlockAction();

        // #region Values

        const blockError = ref("");
        const errorMessage = ref("");

        const campusViewBag = ref(config.entity);
        const campusEditBag = ref<CampusBag | null>(null);

        const isEditMode = ref(false);

        // #endregion

        // #region Computed Values

        /**
         * The title to display in the block panel depending on the current state.
         */
        const blockTitle = computed((): string => {
            if (campusViewBag.value?.guid === emptyGuid) {
                return "Add Campus";
            }
            else if (!isEditMode.value) {
                return campusViewBag.value?.name ?? "";
            }
            else if (campusEditBag.value?.name) {
                return `Edit ${campusEditBag.value.name}`;
            }
            else {
                return "Edit Campus";
            }
        });

        /**
         * Additional labels to display in the block panel.
         */
        const blockLabels = computed((): ListItemBag[] => {
            const labels: ListItemBag[] = [];

            if (isEditMode.value) {
                return labels;
            }

            if (campusViewBag.value?.isActive === true) {
                labels.push({ value: "success", text: "Active" });
            }
            else {
                labels.push({ value: "danger", text: "Inactive" });
            }

            return labels;
        });

        const isEditable = computed((): boolean => {
            return config.isEditable === true && campusViewBag.value?.isSystem !== true;
        });

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
                saveCrate: data
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
            isEditMode.value = true;
        }

        return {
            blockError,
            blockLabels,
            blockTitle,
            campusViewBag,
            campusEditBag,
            errorMessage,
            isEditable,
            isEditMode,
            onCancelEdit,
            onDelete,
            onEdit,
            onSave,
            options
        };
    },

    template: `
<Alert alertType="warning">
    This is an experimental block and should not be used in production.
</Alert>

<Alert v-if="blockError" alertType="warning">
    {{ blockError }}
</Alert>

<Alert v-if="errorMessage" alertType="danger">
    {{ errorMessage }}
</Alert>

<PaneledDetailBlockTemplate v-if="!blockError"
    v-model:isEditMode="isEditMode"
    :title="blockTitle"
    iconClass="fa fa-building-o"
    :labels="blockLabels"
    entityTitle="Campus"
    :isEditAllowed="isEditable"
    :isDeleteAllowed="isEditable"
    @cancelEdit="onCancelEdit"
    @delete="onDelete"
    @edit="onEdit"
    @save="onSave">
    <EditPanel v-if="isEditMode" v-model="campusEditBag" :options="options" />
    <ViewPanel v-else :modelValue="campusViewBag" :options="options" />
</PaneledDetailBlockTemplate>
`
});
