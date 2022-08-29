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
import EditPanel from "./CampusDetail/editPanel.partial";
import ViewPanel from "./CampusDetail/viewPanel.partial";
import { getSecurityGrant, provideSecurityGrant, refreshDetailAttributes, useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { NavigationUrlKey } from "./CampusDetail/types";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { CampusBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusBag";
import { CampusDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusDetailOptionsBag";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { debounce } from "@Obsidian/Utility/util";

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
        const securityGrant = getSecurityGrant(config.securityGrantToken);

        // #region Values

        const blockError = ref("");
        const errorMessage = ref("");

        const campusViewBag = ref(config.entity);
        const campusEditBag = ref<CampusBag | null>(null);

        const panelMode = ref(DetailPanelMode.View);

        // The properties that are being edited in the UI. This is used to
        // inform the server which incoming values have valid data in them.
        const validProperties = [
            "attributeValues",
            "campusSchedules",
            "campusStatusValue",
            //"campusTopics",
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
        ];

        const refreshAttributesDebounce = debounce(() => refreshDetailAttributes(campusEditBag, validProperties, invokeBlockAction), undefined, true);

        // #endregion

        // #region Computed Values

        /**
         * The entity name to display in the block panel.
         */
        const panelName = computed((): string => {
            return campusViewBag.value?.name ?? "";
        });

        /**
         * The identifier key value for this entity.
         */
        const entityKey = computed((): string => {
            return campusViewBag.value?.idKey ?? "";
        });

        /**
         * Additional labels to display in the block panel.
         */
        const blockLabels = computed((): PanelAction[] | null => {
            const labels: PanelAction[] = [];

            if (panelMode.value !== DetailPanelMode.View) {
                return null;
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

        const options = computed((): CampusDetailOptionsBag => {
            return config.options ?? {
                isMultiTimeZoneSupported: false
            };
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for the Cancel button being clicked while in Edit mode.
         * Handles redirect to parent page if creating a new entity.
         *
         * @returns true if the panel should leave edit mode; false if it should stay in edit mode; or a string containing a redirect URL.
         */
        const onCancelEdit = async (): Promise<boolean | string> => {
            if (!campusEditBag.value?.idKey) {
                if (config.navigationUrls?.[NavigationUrlKey.ParentPage]) {
                    return config.navigationUrls[NavigationUrlKey.ParentPage];
                }

                return false;
            }

            return true;
        };

        /**
         * Event handler for the Delete button being clicked. Sends the
         * delete request to the server and then redirects to the target page.
         *
         * @returns false if it should stay on the page; or a string containing a redirect URL.
         */
        const onDelete = async (): Promise<false | string> => {
            errorMessage.value = "";

            const result = await invokeBlockAction<string>("Delete", {
                key: campusViewBag.value?.idKey
            });

            if (result.isSuccess && result.data) {
                return result.data;
            }
            else {
                errorMessage.value = result.errorMessage ?? "Unknown error while trying to delete campus.";

                return false;
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
                key: campusViewBag.value?.idKey
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
         * @returns true if the panel should leave edit mode; false if it should stay in edit mode; or a string containing a redirect URL.
         */
        const onSave = async (): Promise<boolean | string> => {
            errorMessage.value = "";

            const data: DetailBlockBox<CampusBag, CampusDetailOptionsBag> = {
                entity: campusEditBag.value,
                isEditable: true,
                validProperties: validProperties
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
                    return result.data;
                }
            }

            errorMessage.value = result.errorMessage ?? "Unknown error while trying to save campus.";

            return false;
        };

        // #endregion

        provideSecurityGrant(securityGrant);

        // Handle any initial error conditions or the need to go into edit mode.
        if (config.errorMessage) {
            blockError.value = config.errorMessage;
        }
        else if (!config.entity) {
            blockError.value = "The specified campus could not be viewed.";
        }
        else if (!config.entity.idKey) {
            campusEditBag.value = config.entity;
            panelMode.value = DetailPanelMode.Add;
        }

        return {
            campusViewBag,
            campusEditBag,
            blockError,
            blockLabels,
            entityKey,
            entityTypeGuid: EntityType.Campus,
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
<Alert v-if="blockError" alertType="warning">{{ blockError }}</Alert>

<Alert v-if="errorMessage" alertType="danger">{{ errorMessage }}</Alert>

<DetailBlock v-if="!blockError"
    v-model:mode="panelMode"
    :name="panelName"
    :labels="blockLabels"
    :entityKey="entityKey"
    :entityTypeGuid="entityTypeGuid"
    entityTypeName="Campus"
    :isAuditHidden="false"
    :isBadgesVisible="true"
    :isDeleteVisible="isEditable"
    :isEditVisible="isEditable"
    :isFollowVisible="true"
    :isSecurityHidden="false"
    @cancelEdit="onCancelEdit"
    @delete="onDelete"
    @edit="onEdit"
    @save="onSave">
    <template #view>
        <ViewPanel :modelValue="campusViewBag" :options="options" />
    </template>

    <template #edit>
        <EditPanel v-model="campusEditBag" :options="options" @propertyChanged="onPropertyChanged" />
    </template>
</DetailBlock>
`
});
