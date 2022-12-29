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

import { Guid } from "@Obsidian/Types";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import CheckBox from "./checkBox";
import TextBox from "./textBox";
import { FieldTypeEditorUpdateAttributeConfigurationOptionsBag } from "@Obsidian/ViewModels/Controls/fieldTypeEditorUpdateAttributeConfigurationOptionsBag";
import { PublicEditableAttributeBag } from "@Obsidian/ViewModels/Utility/publicEditableAttributeBag";
import CategoryPicker from "./categoryPicker";
import FieldTypeEditor from "./fieldTypeEditor";
import StaticFormControl from "./staticFormControl";
import PanelWidget from "./panelWidget";
import { EntityType } from "@Obsidian/SystemGuids/entityType";

export default defineComponent({
    name: "AttributeEditor",

    components: {
        CategoryPicker,
        CheckBox,
        FieldTypeEditor,
        PanelWidget,
        StaticFormControl,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<PublicEditableAttributeBag | null>,
            default: null
        },

        /** test */
        attributeEntityTypeGuid: {
            type: String as PropType<Guid>,
            default: ""
        },

        isAnalyticsVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isShowInGridVisible: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        isShowOnBulkVisible: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        isAllowSearchVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isIndexingEnabledVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        reservedKeyNames: {
            type: Array as PropType<string[]>,
            default: []
        }
    },

    setup(props, { emit }) {
        const attributeName = ref(props.modelValue?.name ?? "");
        const abbreviatedName = ref(props.modelValue?.abbreviatedName ?? "");
        const attributeKey = ref(props.modelValue?.key ?? "");
        const description = ref(props.modelValue?.description ?? "");
        const isSystem = ref(props.modelValue?.isSystem ?? false);
        const isActive = ref(props.modelValue?.isActive ?? true);
        const isPublic = ref(props.modelValue?.isPublic ?? false);
        const isRequired = ref(props.modelValue?.isRequired ?? false);
        const isShowOnBulk = ref(props.modelValue?.isShowOnBulk ?? false);
        const isShowInGrid = ref(props.modelValue?.isShowInGrid ?? false);
        const isHistoryEnabled = ref(props.modelValue?.isEnableHistory ?? false);
        const isAllowSearch = ref(props.modelValue?.isAllowSearch ?? false);
        const isIndexingEnabled = ref(props.modelValue?.isIndexEnabled ?? false);
        const isAnalyticsEnabled = ref(props.modelValue?.isAnalytic ?? false);
        const isAnalyticsHistoryEnabled = ref(props.modelValue?.isAnalyticHistory ?? false);
        const preHtml = ref(props.modelValue?.preHtml ?? "");
        const postHtml = ref(props.modelValue?.postHtml ?? "");
        const categories = ref([...(props.modelValue?.categories ?? [])]);
        const fieldTypeValue = ref<FieldTypeEditorUpdateAttributeConfigurationOptionsBag>({
            fieldTypeGuid: props.modelValue?.fieldTypeGuid ?? "",
            configurationValues: { ...(props.modelValue?.configurationValues ?? {}) },
            defaultValue: props.modelValue?.defaultValue ?? ""
        });

        const categoryQualifierValue = computed((): string => {
            if (props.attributeEntityTypeGuid) {
                return `{EL:${EntityType.EntityType}:${props.attributeEntityTypeGuid}}`;
            }
            else {
                return "";
            }
        });

        const isFieldTypeReadOnly = computed((): boolean => !!props.modelValue?.guid);

        watch([
            attributeName,
            abbreviatedName,
            attributeKey,
            description,
            isActive,
            isPublic,
            isRequired,
            isShowOnBulk,
            isShowInGrid,
            isAllowSearch,
            isAnalyticsHistoryEnabled,
            isAnalyticsHistoryEnabled,
            isHistoryEnabled,
            isIndexingEnabled,
            preHtml,
            postHtml,
            categories,
            fieldTypeValue],
            () => {
                const newModelValue: PublicEditableAttributeBag = {
                    ...(props.modelValue ?? { isSystem: false }),
                    name: attributeName.value,
                    abbreviatedName: abbreviatedName.value,
                    key: attributeKey.value,
                    description: description.value,
                    isActive: isActive.value,
                    isPublic: isPublic.value,
                    isRequired: isRequired.value,
                    isShowOnBulk: isShowOnBulk.value,
                    isShowInGrid: isShowInGrid.value,
                    isAllowSearch: isAllowSearch.value,
                    isAnalytic: isAnalyticsEnabled.value,
                    isAnalyticHistory: isAnalyticsHistoryEnabled.value,
                    isEnableHistory: isHistoryEnabled.value,
                    isIndexEnabled: isIndexingEnabled.value,
                    preHtml: preHtml.value,
                    postHtml: postHtml.value,
                    categories: [...categories.value],
                    fieldTypeGuid: fieldTypeValue.value.fieldTypeGuid,
                    configurationValues: { ...fieldTypeValue.value.configurationValues },
                    defaultValue: fieldTypeValue.value.defaultValue
                };

                emit("update:modelValue", newModelValue);
            });

        return {
            abbreviatedName,
            attributeEntityTypeGuid: EntityType.Attribute,
            attributeName,
            attributeKey,
            categoryQualifierValue,
            description,
            categories,
            fieldTypeValue,
            isActive,
            isAllowSearch,
            isAnalyticsEnabled,
            isAnalyticsHistoryEnabled,
            isFieldTypeReadOnly,
            isHistoryEnabled,
            isIndexingEnabled,
            isPublic,
            isRequired,
            isShowInGrid,
            isShowOnBulk,
            isSystem,
            preHtml,
            postHtml
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="attributeName"
                label="Name"
                rules="required" />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active"
                help="Set to Inactive to exclude this attribute from Edit and Display UIs."
                text="Yes" />
        </div>
    </div>

    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="abbreviatedName"
                label="Abbreviated Name" />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isPublic"
                label="Public"
                help="Set to public if you want this attribute to be displayed in public contexts."
                text="Yes" />
        </div>
    </div>

    <TextBox v-model="description"
        label="Description"
        textMode="multiline" />

    <div class="row">
        <div class="col-md-6">
            <CategoryPicker v-model="categories"
                label="Categories"
                :entityTypeGuid="attributeEntityTypeGuid"
                entityTypeQualifierColumn="EntityTypeId"
                :entityTypeQualifierValue="categoryQualifierValue"
                multiple />

            <StaticFormControl v-if="isSystem" v-model="attributeKey" label="Key" />
            <TextBox v-else v-model="attributeKey" label="Key" rules="required" :disabled="keyDisabledAttr" />

            <div class="row">
                <div class="col-sm-6">
                    <CheckBox v-model="isRequired"
                        label="Required"
                        text="Yes" />
                </div>

                <div class="col-sm-6">
                    <CheckBox v-if="isShowOnBulkVisible"
                        v-model="isShowOnBulk"
                        label="Show on Bulk"
                        help="If selected, this attribute will be shown with bulk update attributes."
                        text="Yes" />
                </div>

                <div class="col-sm-6">
                    <CheckBox v-if="isShowInGridVisible"
                        v-model="isShowInGrid"
                        label="Show in Grid"
                        help="If selected, this attribute will be included in a grid."
                        text="Yes" />
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <FieldTypeEditor v-model="fieldTypeValue" :isFieldTypeReadOnly="isFieldTypeReadOnly" />
        </div>
    </div>

    <PanelWidget>
        <template #header>Advanced Settings</template>
        <div class="row">
            <div class="col-md-6">
                <CheckBox label="Enable History"
                    v-model="isHistoryEnabled"
                    help="If selected, changes to the value of this attribute will be stored in attribute value history."
                    text="Yes" />

                <CheckBox v-if="isAllowSearchVisible"
                    label="Allow Search"
                    v-model="isAllowSearch"
                    help="If selected, this attribute can be searched on."
                    text="Yes" />

                <CheckBox v-if="isIndexingEnabledVisible"
                    label="Indexing Enabled"
                    v-model="isIndexingEnabled"
                    help="If selected, this attribute can be used when indexing for universal search."
                    text="Yes" />
            </div>

            <div class="col-md-6">
                <CheckBox v-if="isAnalyticsVisible"
                    label="Analytics Enabled"
                    v-model="isAnalyticHistory"
                    help="If selected, this attribute will be made available as an Analytic."
                    text="Yes" />

                <CheckBox v-if="isAnalyticsVisible"
                    label="Analytics History Enabled"
                    v-model="isAnalyticsHistoryEnabled"
                    help="If selected, changes to the value of this attribute will cause Analytics to create a history record. Note that this requires that 'Analytics Enabled' is also enabled."
                    text="Yes" />
            </div>
        </div>

        <TextBox v-model="preHtml"
            label="Pre-HTML"
            help="HTML that should be rendered before teh attribute's edit control."
            textMode="multiline" />

        <TextBox v-model="postHtml"
            label="Post-HTML"
            help="HTML that should be rendered before teh attribute's edit control."
            textMode="multiline" />
    </PanelWidget>
</fieldset>
`
});
