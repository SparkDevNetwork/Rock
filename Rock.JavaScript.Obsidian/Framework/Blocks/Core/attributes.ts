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

import { defineComponent, ref, watch } from "vue";
import Alert from "../../Elements/alert";
import AttributeEditor from "../../Controls/attributeEditor";
import DropDownList from "../../Elements/dropDownList";
import Modal from "../../Controls/modal";
import RockButton from "../../Elements/rockButton";
import RockForm from "../../Controls/rockForm";
import RockField from "../../Controls/rockField";
import PaneledBlockTemplate from "../../Templates/paneledBlockTemplate";
import TextBox from "../../Elements/textBox";
import { useConfigurationValues, useInvokeBlockAction } from "../../Util/block";
import { Guid, normalize as normalizeGuid } from "../../Util/guid";
import { ClientAttributeValue, ClientEditableAttributeValue, ListItem } from "../../ViewModels";
import { getFieldType } from "../../Fields/index";
import { truncate } from "../../Services/string";
import { computed } from "vue";
import { PublicEditableAttributeViewModel } from "../../ViewModels/publicEditableAttribute";
import { FieldType } from "../../SystemGuids";
import { alert, confirmDelete } from "../../Util/dialogs";

type BlockConfiguration = {
    attributeEntityTypeId: number;

    entityTypeGuid?: Guid;

    entityTypes?: ListItem[];

    hideColumns: boolean;

    enableShowInGrid: boolean;

    allowSettingOfValues: boolean;

    attributes: GridRow[];
};

type EditAttribute = {
    entityTypeQualifierColumn?: string | null;

    entityTypeQualifierValue?: string | null;

    attribute: PublicEditableAttributeViewModel;
};

type GridRow = {
    guid: Guid;

    id: number;

    name: string;

    qualifier: string;

    categories: string;

    isActive: boolean;

    value: ClientAttributeValue;

    isDeleteEnabled: boolean;

    isSecurityEnabled: boolean;
};

export default defineComponent({
    name: "Core.Attributes",

    components: {
        Alert,
        AttributeEditor,
        DropDownList,
        Modal,
        PaneledBlockTemplate,
        RockButton,
        RockField,
        RockForm,
        TextBox
    },

    setup() {
        const config = useConfigurationValues<BlockConfiguration>();
        const invokeBlockAction = useInvokeBlockAction();

        /** True if the entity type picker should be visible on the page. */
        const showEntityTypePicker = computed(() => !config.entityTypeGuid);

        /** The currently selected entity type by the user. */
        const entityTypeGuid = ref("");

        /** The list of entity types to show in the entity type picker. */
        const entityTypeOptions = computed(() => config.entityTypes ?? []);

        /** True if we have a valid entity type selected or via configuration. */
        const entityTypeSelectionIsValid = computed(() => !!config.entityTypeGuid || entityTypeGuid.value !== "");

        /** True if the entity type qualifier text boxes should be shown. */
        const showEntityTypeQualifier = computed(() => !config.entityTypeGuid);

        /** The current value of the entity type qualifier column. */
        const entityTypeQualifierColumn = ref("");

        /** The current value of the entity type qualifier value. */
        const entityTypeQualifierValue = ref("");

        /** The attributes currently displayed in the pseudo-grid. */
        const attributes = ref<GridRow[]>(config.attributes);

        // #region Attribute Editing

        /** The current attribute in an editable format. */
        const editableAttribute = ref<PublicEditableAttributeViewModel | null>(null);

        /** True if the edit attribute modal should be visible. */
        const showEditAttributeModal = ref<boolean>(false);

        /** True if the edit attribute form should attempt to submit it's data. */
        const submitEditAttribute = ref<boolean>(false);

        /** The title to display in the edit attribute modal. */
        const editAttributeModalTitle = computed((): string => {
            if (editableAttribute.value) {
                return `Edit ${editableAttribute.value.name}`;
            }

            return "";
        });

        /**
         * Start editing an attribute on the given row.
         * 
         * @param row The row that represents the attribute.
         */
        const editAttribute = async (row: GridRow): Promise<void> => {
            const result = await invokeBlockAction<EditAttribute>("GetEditAttribute", {
                attributeGuid: row.guid
            });

            if (!result.isSuccess || !result.data) {
                return alert(result.errorMessage ?? "Unable to edit attribute.");
            }

            entityTypeQualifierColumn.value = result.data.entityTypeQualifierColumn ?? "";
            entityTypeQualifierValue.value = result.data.entityTypeQualifierValue ?? "";
            editableAttribute.value = result.data.attribute;
            showEditAttributeModal.value = true;
        };

        /**
         * Start the save operation by requesting the edit attribute form to
         * validation and then trigger the submit event.
         */
        const startSaveEditAttribute = (): void => {
            submitEditAttribute.value = true;
        };

        /**
         * Save the attribute information to the server.
         */
        const saveEditAttribute = async (): Promise<void> => {
            const result = await invokeBlockAction<GridRow>("SaveEditAttribute", {
                entityTypeGuid: entityTypeGuid.value,
                entityTypeQualifierColumn: entityTypeQualifierColumn.value,
                entityTypeQualifierValue: entityTypeQualifierValue.value,
                attribute: editableAttribute.value
            });

            if (!result.isSuccess || !result.data) {
                return alert(result.errorMessage ?? "Unable to save attribute.");
            }

            const index = attributes.value.findIndex(a => a.guid === result.data?.guid);

            if (index !== -1) {
                attributes.value.splice(index, 1, result.data);
            }
            else {
                attributes.value.push(result.data);
            }

            editableAttribute.value = null;
            showEditAttributeModal.value = false;
        };

        /**
         * Event handler for when the add attribute button is clicked.
         */
        const onAddAttribute = (): void => {
            editableAttribute.value = {
                isActive: true,
                fieldTypeGuid: normalizeGuid(FieldType.Text)
            };
            showEditAttributeModal.value = true;
            entityTypeQualifierColumn.value = "";
            entityTypeQualifierValue.value = "";
        };

        /**
         * Event handler for when a delete button on a row is clicked.
         * 
         * @param row The row on which the delete button was clicked.
         */
        const onDeleteAttribute = async (row: GridRow): Promise<void> => {
            const status = await confirmDelete("Attribute");

            if (!status) {
                return;
            }

            const result = await invokeBlockAction<GridRow>("DeleteAttribute", {
                attributeGuid: row.guid
            });

            if (!result.isSuccess) {
                return alert(result.errorMessage || "Unable to delete attribute.");
            }

            const index = attributes.value.findIndex(a => a.guid === row.guid);

            if (index !== -1) {
                attributes.value.splice(index, 1);
            }
        };

        // #endregion

        // #region Attribute Value Editing

        /** The current attribute value in an editable format. */
        const editableAttributeValue = ref<ClientEditableAttributeValue | null>(null);

        /** True if the edit attribute value modal should be visible. */
        const showEditAttributeValueModal = ref<boolean>(false);

        /** True if the edit attribute value form should attempt to submit it's data. */
        const submitEditAttributeValue = ref<boolean>(false);

        /** The title to display in the edit attribute value modal. */
        const editAttributeValueModalTitle = computed((): string => {
            if (editableAttributeValue.value) {
                return `${editableAttributeValue.value.name} Value`;
            }

            return "";
        });

        /**
         * Begins editing an attribute's value.
         * 
         * @param row The row that initiated the action.
         */
        const editAttributeValue = async (row: GridRow): Promise<void> => {
            if (!config.allowSettingOfValues) {
                return;
            }

            const result = await invokeBlockAction<ClientEditableAttributeValue>("GetEditAttributeValue", {
                attributeGuid: row.guid
            });

            if (!result.isSuccess || !result.data) {
                return alert(result.errorMessage ?? "Unable to edit attribute value.");
            }

            editableAttributeValue.value = result.data;
            showEditAttributeValueModal.value = true;
        };

        /**
         * Request that the edit value form attempt to validate and submit.
         */
        const startSaveEditAttributeValue = (): void => {
            submitEditAttributeValue.value = true;
        };

        /**
         * Performs the save operation for editing an attribute value.
         */
        const saveEditAttributeValue = async (): Promise<void> => {
            const result = await invokeBlockAction<GridRow>("SaveEditAttributeValue", {
                attributeGuid: editableAttributeValue.value?.attributeGuid,
                value: editableAttributeValue.value?.value
            });

            if (!result.isSuccess || !result.data) {
                return alert(result.errorMessage ?? "Unable to save attribute value.");
            }

            const index = attributes.value.findIndex(a => a.guid === result.data?.guid);

            if (index !== -1) {
                attributes.value.splice(index, 1, result.data);
            }

            editableAttributeValue.value = null;
            showEditAttributeValueModal.value = false;
        };

        // #endregion

        /**
         * Get the condensed value to display in a row.
         * 
         * @param value The value representation to display.
         *
         * @returns A string to display in the grid row for the value.
         */
        const getCondensedValue = (value: ClientAttributeValue): string => {
            // TODO: This should probably be updated to have a new method called
            // getFieldTypeOrDefault() that will return the Text field type.
            const fieldType = getFieldType(value.fieldTypeGuid);

            if (!fieldType) {
                return truncate(value.textValue ?? "", 100);
            }
            else {
                return fieldType.getCondensedTextValue(value);
            }
        };

        /**
         * Gets the CSS classes to be applied to the delete button.
         * 
         * @param row The row containing the delete button.
         *
         * @returns An array of class names.
         */
        const getDeleteButtonClass = (row: GridRow): string[] => {
            const classes: string[] = ["btn", "btn-danger", "btn-sm", "grid-delete-button"];

            if (!row.isDeleteEnabled) {
                classes.push("disabled");
            }

            return classes;
        };

        /**
         * Gets the CSS classes to be applied to the data cell of a row.
         *
         * @param _row The row containing the data cell.
         *
         * @returns An array of class names.
         */
        const getDataCellClass = (_row: GridRow): string[] => {
            if (config.allowSettingOfValues) {
                return ["grid-select-cell"];
            }
            else {
                return ["grid-cell"];
            }
        };

        // Watch for changes to the user-selection of the entity type and update
        // the list of attributes in the grid.
        watch(entityTypeGuid, async () => {
            if (entityTypeGuid.value === "") {
                attributes.value = [];
                return;
            }

            const result = await invokeBlockAction<GridRow[]>("GetAttributes", {
                entityTypeGuid: entityTypeGuid.value,
            });

            if (!result.isSuccess || !result.data) {
                return;
            }

            attributes.value = result.data;
        });

        return {
            attributes,
            editableAttribute,
            editableAttributeValue,
            editAttribute,
            editAttributeValue,
            editAttributeModalTitle,
            editAttributeValueModalTitle,
            entityTypeGuid,
            entityTypeOptions,
            entityTypeQualifierColumn,
            entityTypeQualifierValue,
            getCondensedValue,
            getDataCellClass,
            getDeleteButtonClass,
            saveEditAttribute,
            saveEditAttributeValue,
            entityTypeSelectionIsValid,
            onAddAttribute,
            onDeleteAttribute,
            onIgnore: () => { /* Intentionally blank */ },
            showEditAttributeModal,
            showEditAttributeValueModal,
            showEntityTypeQualifier,
            showEntityTypePicker,
            startSaveEditAttribute,
            startSaveEditAttributeValue,
            submitEditAttribute,
            submitEditAttributeValue
        };
    },

    template: `
<Alert alertType="warning">
    This is an experimental block and should not be used in production.
</Alert>

<PaneledBlockTemplate>
    <template v-slot:title>Attribute List</template>
    <template v-slot:titleAside>
        <div v-if="showEntityTypePicker" class="form-inline panel-labels">
            <DropDownList v-model="entityTypeGuid"
                label="Entity Type"
                grouped
                :enhanceForLongLists="false"
                :options="entityTypeOptions" />
        </div>
    </template>

    <template v-slot:default>
        <div v-if="entityTypeSelectionIsValid" class="grid grid-panel">
            <div class="grid-actions" style="border-bottom: 1px solid #dfe0e1;">
                <RockButton class="btn-add btn-grid-action" btnType="default" btnSize="sm" @click="onAddAttribute"><i class="fa fa-plus-circle fa-fw"></i></RockButton>
            </div>

            <div class="table-responsive">
                <table class="grid-table table table-bordered table-striped table-hover">
                    <thead>
                        <tr align="left">
                            <th data-priority="1" scope="col" align="right">Id</th>
                            <th data-priority="1" scope="col">Qualifier</th>
                            <th data-priority="1" scope="col">Name</th>
                            <th data-priority="1" scope="col">Categories</th>
                            <th data-priority="1" scope="col">Value</th>
                            <th class="grid-columncommand" data-priority="1" scope="col">&nbsp;</th>
                            <th class="grid-columncommand" data-priority="1" scope="col">&nbsp;</th>
                            <th class="grid-columncommand" data-priority="1" scope="col">&nbsp;</th>
                        </tr>
                    </thead>

                    <tbody>
                        <tr v-for="attribute in attributes" :key="attribute.id" align="left" @click.stop="editAttributeValue(attribute)">
                            <td :class="getDataCellClass(attribute)" data-priority="1" style="white-space: nowrap;" align="right">{{ attribute.id }}</td>
                            <td :class="getDataCellClass(attribute)" data-priority="1" style="white-space: nowrap;">{{ attribute.qualifier }}</td>
                            <td :class="getDataCellClass(attribute)" data-priority="1">{{ attribute.name }}</td>
                            <td :class="getDataCellClass(attribute)" data-priority="1">{{ attribute.categories }}</td>
                            <td :class="getDataCellClass(attribute)" data-priority="1">{{ getCondensedValue(attribute.value) }}</td>
                            <td class="grid-columncommand" data-priority="1" align="center" @click.stop="onIgnore">
                                <a title="Edit" class="btn btn-default btn-sm" @click.prevent.stop="editAttribute(attribute)"><i class="fa fa-pencil"></i></a>
                            </td>
                            <td class="grid-columncommand" data-priority="1" align="center" @click.stop="onIgnore">
                                <a title="Security" class="btn btn-security btn-sm disabled"><i class="fa fa-lock"></i></a>
                            </td>
                            <td class="grid-columncommand" data-priority="1" align="center" @click.stop="onIgnore">
                                <a title="Delete" :class="getDeleteButtonClass(attribute)" @click.prevent.stop="onDeleteAttribute(attribute)"><i class="fa fa-times"></i></a>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <Alert v-else alertType="warning">
            Please select an entity to display attributes for.
        </Alert>
    </template>

</PaneledBlockTemplate>

<Modal v-model="showEditAttributeValueModal" :title="editAttributeValueModalTitle">
    <RockForm v-model:submit="submitEditAttributeValue" @submit="saveEditAttributeValue">
        <RockField v-model:attributeValue="editableAttributeValue" isEditMode />
    </RockForm>

    <template #customButtons>
        <RockButton btnType="primary" @click="startSaveEditAttributeValue">Save</RockButton>
    </template>
</Modal>

<Modal v-model="showEditAttributeModal" :title="editAttributeModalTitle">
    <RockForm v-model:submit="submitEditAttribute" @submit="saveEditAttribute">
        <div v-if="showEntityTypeQualifier" class="well">
            <div class="row">
                <div class="col-md-6">
                    <TextBox v-model="entityTypeQualifierColumn" label="Qualifier Field" />
                </div>

                <div class="col-md-6">
                    <TextBox v-model="entityTypeQualifierValue" label="Qualifier Value" />
                </div>
            </div>
        </div>

        <AttributeEditor v-model="editableAttribute" />
    </RockForm>

    <template #customButtons>
        <RockButton btnType="primary" @click="startSaveEditAttribute">Save</RockButton>
    </template>
</Modal>
`
});
