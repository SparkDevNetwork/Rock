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
import Panel from "@Obsidian/Controls/panel";
import RockForm from "@Obsidian/Controls/rockForm";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import AuditDetail from "@Obsidian/Controls/auditDetail";
import RockButton from "@Obsidian/Controls/rockButton";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { areEqual, emptyGuid } from "@Obsidian/Utility/guid";
import EditPanel from "./FormTemplateDetail/editPanel.partial";
import { FormTemplateDetailConfiguration, TemplateDetail, TemplateEditDetail } from "./FormTemplateDetail/types.partial";
import { provideSources } from "./FormTemplateDetail/utils.partial";
import ViewPanel from "./FormTemplateDetail/viewPanel.partial";

export default defineComponent({
    name: "WorkFlow.FormTemplateDetail",

    components: {
        NotificationBox,
        AuditDetail,
        EditPanel,
        Panel,
        RockButton,
        RockForm,
        ViewPanel
    },

    setup() {
        const config = useConfigurationValues<FormTemplateDetailConfiguration>();
        const invokeBlockAction = useInvokeBlockAction();

        const templateDetail = ref(config.template);
        const templateEditDetail = ref<TemplateEditDetail>({});
        const isEditable = ref(config.isEditable);

        // Start in edit mode if we have an empty guid.
        const isEditMode = ref(areEqual(config.templateGuid ?? "", emptyGuid));

        /** True if the template being viewed is inactive. */
        const isInactive = computed(() => !(templateDetail.value?.isActive ?? false));

        /**
         * True if we have a startup error that prevents us from displaying
         * the normal view or edit panels. It's an error if we don't have a
         * template or a templateGuid. That currently means they tried to view
         * an invalid template.
         */
        const isStartupError = !config.template && !config.templateGuid;

        /**
         * Gets the title that should currently be displayed in the panel title.
         * This takes into account the current display mode.
         */
        const blockTitle = computed((): string => {
            if (!isEditMode.value) {
                return templateDetail.value?.name ?? "";
            }
            else {
                return templateEditDetail.value.name || "Add Template";
            }
        });

        /**
         * Event handler for when the individual clicks the Edit button.
         */
        const onEditClick = async (): Promise<void> => {
            const result = await invokeBlockAction<TemplateEditDetail>("StartEdit", {
                guid: config.templateGuid ?? ""
            });

            if (result.isSuccess && result.data) {
                templateEditDetail.value = result.data;
                isEditMode.value = true;
            }
        };

        /**
         * Event handler for when the individual clicks the Cancel button
         * while in edit mode.
         */
        const onEditCancelClick = (): void => {
            if (config.parentUrl && areEqual(config.templateGuid ?? "", emptyGuid)) {
                window.location.href = config.parentUrl;
                return;
            }

            templateEditDetail.value = {};
            isEditMode.value = false;
        };

        /**
         * Event handler for when the form has been validated and is now ready
         * to be submitted to the server.
         */
        const onSubmit = async (): Promise<void> => {
            const result = await invokeBlockAction<TemplateDetail | string>("SaveTemplate", {
                guid: config.templateGuid ?? "",
                template: templateEditDetail.value
            });

            if (result.isSuccess && result.data) {
                if (result.statusCode === 200 && typeof result.data === "object") {
                    templateDetail.value = result.data;
                    templateEditDetail.value = {};
                    isEditMode.value = false;
                }
                else if (result.statusCode === 201 && typeof result.data === "string") {
                    window.location.href = result.data;
                }
            }
        };

        provideSources(config.sources ?? {});

        return {
            blockTitle,
            entityKey: config.templateGuid ?? "",
            entityTypeGuid: EntityType.WorkflowFormBuilderTemplate,
            isEditable,
            isInactive,
            isStartupError,
            isEditMode,
            onEditCancelClick,
            onEditClick,
            onSubmit,
            templateDetail,
            templateEditDetail
        };
    },

    template: `
<NotificationBox v-if="isStartupError" alertType="warning">
    Unable to view details of this template.
</NotificationBox>

<Panel v-else type="block" :title="blockTitle" titleIconCssClass="fa fa-align-left">
    <template v-if="!isEditMode" #headerActions>
        <span v-if="isInactive" class="label label-danger">Inactive</span>
    </template>

    <template v-if="!isEditMode" #drawer>
        <AuditDetail :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />
    </template>

    <div v-if="!isEditMode">
        <ViewPanel :modelValue="templateDetail" />

        <div class="actions">
            <RockButton v-if="isEditable" btnType="primary" accesskey="e" @click="onEditClick">Edit</RockButton>
        </div>
    </div>

    <div v-else>
        <RockForm @submit="onSubmit">
            <EditPanel v-model="templateEditDetail" />

            <div class="actions">
                <RockButton type="submit" btnType="primary">Save</RockButton>
                <RockButton btnType="link" @click="onEditCancelClick">Cancel</RockButton>
            </div>
        </RockForm>
    </div>
</Panel>
`
});
