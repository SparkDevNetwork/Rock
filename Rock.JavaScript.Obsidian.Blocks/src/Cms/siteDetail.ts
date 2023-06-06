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
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
import DetailBlock from "@Obsidian/Templates/detailBlock";
import { DetailPanelMode } from "@Obsidian/Enums/Controls/detailPanelMode";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import EditPanel from "./SiteDetail/editPanel.partial";
import ViewPanel from "./SiteDetail/viewPanel.partial";
import { getSecurityGrant, provideSecurityGrant, refreshDetailAttributes, useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { debounce } from "@Obsidian/Utility/util";
import { NavigationUrlKey } from "./SiteDetail/types.partial";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { SiteBag } from "@Obsidian/ViewModels/Blocks/Cms/SiteDetail/siteBag";
import { SiteDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/SiteDetail/siteDetailOptionsBag";

export default defineComponent({
    name: "Cms.SiteDetail",

    components: {
        NotificationBox,
        EditPanel,
        DetailBlock,
        ViewPanel
    },

    setup() {
        const config = useConfigurationValues<DetailBlockBox<SiteBag, SiteDetailOptionsBag>>();
        const invokeBlockAction = useInvokeBlockAction();
        const securityGrant = getSecurityGrant(config.securityGrantToken);

        // #region Values

        const blockError = ref("");
        const errorMessage = ref("");

        const siteViewBag = ref(config.entity);
        const siteEditBag = ref<SiteBag | null>(null);

        const panelMode = ref<DetailPanelMode>(DetailPanelMode.View);

        // The properties that are being edited in the UI. This is used to
        // inform the server which incoming values have valid data in them.
        const validProperties = [
            "attributeValues",
            "allowedFrameDomains",
            "allowIndexing",
            "changePasswordPage",
            "changePasswordPageRoute",
            "communicationPage",
            "communicationPageRoute",
            "defaultPage",
            "defaultPageRoute",
            "description",
            "disablePredictableIds",
            "enabledForShortening",
            "enableExclusiveRoutes",
            "enableMobileRedirect",
            "enablePageViewGeoTracking",
            "enablePageViews",
            "enablePersonalization",
            "enableVisitorTracking",
            "errorPage",
            "externalUrl",
            "favIconBinaryFile",
            "googleAnalyticsCode",
            "indexStartingLocation",
            "isActive",
            "isIndexEnabled",
            "loginPage",
            "loginPageRoute",
            "mobilePage",
            "name",
            "pageHeaderContent",
            "pageNotFoundPage",
            "pageNotFoundPageRoute",
            "redirectTablets",
            "registrationPage",
            "registrationPageRoute",
            "requiresEncryption",
            "siteLogoBinaryFile",
            "theme",
            "siteDomains"
        ];

        const refreshAttributesDebounce = debounce(() => refreshDetailAttributes(siteEditBag, validProperties, invokeBlockAction), undefined, true);

        // #endregion

        // #region Computed Values

        /**
         * The entity name to display in the block panel.
         */
        const panelName = computed((): string => {
            return siteViewBag.value?.name ?? "";
        });

        /**
         * The identifier key value for this entity.
         */
        const entityKey = computed((): string => {
            return siteViewBag.value?.idKey ?? "";
        });

        /**
         * Additional labels to display in the block panel.
         */
        const blockLabels = computed((): PanelAction[] | null => {
            const labels: PanelAction[] = [];

            if (panelMode.value !== DetailPanelMode.View) {
                return null;
            }

            if (siteViewBag.value?.isActive === true) {
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

            if (siteViewBag.value?.isSystem) {
                labels.push({
                    title: "System",
                    type: "success"
                });
            }

            return labels;
        });

        const isEditable = computed((): boolean => {
            return config.isEditable === true && siteViewBag.value?.isSystem !== true;
        });

        const options = computed((): SiteDetailOptionsBag => {
            return config.options ?? { themes: [], siteAttributes: [] };
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
        async function onCancelEdit(): Promise<boolean | string> {
            if (!siteEditBag.value?.idKey) {
                if (config.navigationUrls?.[NavigationUrlKey.ParentPage]) {
                    return config.navigationUrls[NavigationUrlKey.ParentPage];
                }

                return false;
            }

            return true;
        }

        /**
         * Event handler for the Delete button being clicked. Sends the
         * delete request to the server and then redirects to the target page.
         *
         * @returns false if it should stay on the page; or a string containing a redirect URL.
         */
        async function onDelete(): Promise<false | string> {
            errorMessage.value = "";

            const result = await invokeBlockAction<string>("Delete", {
                key: siteViewBag.value?.idKey
            });

            if (result.isSuccess && result.data) {
                return result.data;
            }
            else {
                errorMessage.value = result.errorMessage ?? "Unknown error while trying to delete site.";

                return false;
            }
        }

        /**
         * Event handler for the Edit button being clicked. Request the edit
         * details from the server and then enter edit mode.
         *
         * @returns true if the panel should enter edit mode; otherwise false.
         */
        async function onEdit(): Promise<boolean> {
            const result = await invokeBlockAction<DetailBlockBox<SiteBag, SiteDetailOptionsBag>>("Edit", {
                key: siteViewBag.value?.idKey
            });

            if (result.isSuccess && result.data && result.data.entity) {
                siteEditBag.value = result.data.entity;
                config.options = result.data.options;
                return true;
            }
            else {
                return false;
            }
        }

        /**
         * Event handler for when a value has changed that has an associated
         * C# property name. This is used to detect changes to values that
         * might cause qualified attributes to either show up or not show up.
         *
         * @param propertyName The name of the C# property that was changed.
         */
        function onPropertyChanged(propertyName: string): void {
            // If we don't have any qualified attribute properties or this property
            // is not one of them then do nothing.
            if (!config.qualifiedAttributeProperties || !config.qualifiedAttributeProperties.some(n => n.toLowerCase() === propertyName.toLowerCase())) {
                return;
            }

            refreshAttributesDebounce();
        }

        /**
         * Event handler for the panel's Save event. Send the data to the server
         * to be saved and then leave edit mode or redirect to target page.
         *
         * @returns true if the panel should leave edit mode; false if it should stay in edit mode; or a string containing a redirect URL.
         */
        async function onSave(): Promise<boolean | string> {
            errorMessage.value = "";

            const data: DetailBlockBox<SiteBag, SiteDetailOptionsBag> = {
                entity: siteEditBag.value,
                isEditable: true,
                validProperties: validProperties,
                options: options.value
            };

            const result = await invokeBlockAction<SiteBag | string>("Save", {
                box: data
            });

            if (result.isSuccess && result.data) {
                if (result.statusCode === 200 && typeof result.data === "object") {
                    siteViewBag.value = result.data;

                    return true;
                }
                else if (result.statusCode === 201 && typeof result.data === "string") {
                    return result.data;
                }
            }

            errorMessage.value = result.errorMessage ?? "Unknown error while trying to save site.";

            return false;
        }

        // #endregion

        provideSecurityGrant(securityGrant);

        // Handle any initial error conditions or the need to go into edit mode.
        if (config.errorMessage) {
            blockError.value = config.errorMessage;
        }
        else if (!config.entity) {
            blockError.value = "The specified site could not be viewed.";
        }
        else if (!config.entity.idKey) {
            siteEditBag.value = config.entity;
            panelMode.value = DetailPanelMode.Add;
        }

        return {
            siteViewBag,
            siteEditBag,
            blockError,
            blockLabels,
            entityKey,
            entityTypeGuid: EntityType.Site,
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
<NotificationBox v-if="blockError" alertType="warning" v-text="blockError" />

<NotificationBox v-if="errorMessage" alertType="danger" v-text="errorMessage" />

<DetailBlock v-if="!blockError"
    v-model:mode="panelMode"
    :name="panelName"
    :labels="blockLabels"
    :entityKey="entityKey"
    :entityTypeGuid="entityTypeGuid"
    entityTypeName="Site"
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
        <ViewPanel :modelValue="siteViewBag" :options="options" />
    </template>

    <template #edit>
        <EditPanel v-model="siteEditBag" :options="options" @propertyChanged="onPropertyChanged" />
    </template>
</DetailBlock>
`
});
