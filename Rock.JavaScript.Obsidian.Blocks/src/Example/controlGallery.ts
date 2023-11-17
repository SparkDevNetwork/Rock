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

/**
 * The following controls are not included for various reasons (e.g. only used internally or are not finalized)
 *
 * - attributeEditor
 * - blockActionSourceGrid
 * - categorizedValuePickerDropDownLevel
 * - componentFromUrl
 * - fieldFilterContainer
 * - fieldFilterRuleRow
 * - gatewayControl
 * - geoPickerMap
 * - grid
 * - gridColumn
 * - gridProfileLInkColumn
 * - gridRow
 * - gridSelectColumn
 * - myWellGatewayControl
 * - nmiGatewayControl
 * - pageDebugTimings
 * - primaryBlock
 * - rockAttributeFilter
 * - rockField
 * - rockForm
 * - rockFormField
 * - rockSuspense
 * - saveFinancialAccountForm
 * - secondaryBlock
 * - testGatewayControl
 * - timeIntervalPicker
 */

import { Component, computed, defineComponent, onMounted, onUnmounted, ref, watch } from "vue";
import { buildExampleCode, convertComponentName, getControlImportPath, getSfcControlImportPath, getTemplateImportPath, displayStyleItems } from "./ControlGallery/utils.partial";
import GalleryAndResult from "./ControlGallery/galleryAndResult.partial.obs";
import { BtnType } from "@Obsidian/Enums/Controls/btnType";
import { BtnSize } from "@Obsidian/Enums/Controls/btnSize";
import FieldFilterEditor from "@Obsidian/Controls/fieldFilterEditor.obs";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import EmailBox from "@Obsidian/Controls/emailBox.obs";
import CodeEditor from "@Obsidian/Controls/codeEditor.obs";
import DatePicker from "@Obsidian/Controls/datePicker.obs";
import DateRangePicker from "@Obsidian/Controls/dateRangePicker.obs";
import DateTimePicker from "@Obsidian/Controls/dateTimePicker.obs";
import ListBox from "@Obsidian/Controls/listBox.obs";
import BirthdayPicker from "@Obsidian/Controls/birthdayPicker.obs";
import NumberUpDown from "@Obsidian/Controls/numberUpDown.obs";
import AddressControl from "@Obsidian/Controls/addressControl.obs";
import InlineSwitch from "@Obsidian/Controls/inlineSwitch.obs";
import Switch from "@Obsidian/Controls/switch.obs";
import Toggle from "@Obsidian/Controls/toggle.obs";
import ItemsWithPreAndPostHtml from "@Obsidian/Controls/itemsWithPreAndPostHtml.obs";
import { ItemWithPreAndPostHtml } from "@Obsidian/Types/Controls/itemsWithPreAndPostHtml";
import StaticFormControl from "@Obsidian/Controls/staticFormControl.obs";
import ProgressTracker from "@Obsidian/Controls/progressTracker.obs";
import { ProgressTrackerItem } from "@Obsidian/Types/Controls/progressTracker";
import RockForm from "@Obsidian/Controls/rockForm.obs";
import RockButton from "@Obsidian/Controls/rockButton.obs";
import RadioButtonList from "@Obsidian/Controls/radioButtonList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import Dialog from "@Obsidian/Controls/dialog.obs";
import InlineCheckBox from "@Obsidian/Controls/inlineCheckBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import PhoneNumberBox from "@Obsidian/Controls/phoneNumberBox.obs";
import HelpBlock from "@Obsidian/Controls/helpBlock.obs";
import DatePartsPicker from "@Obsidian/Controls/datePartsPicker.obs";
import { DatePartsPickerValue } from "@Obsidian/Types/Controls/datePartsPicker";
import ColorPicker from "@Obsidian/Controls/colorPicker.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import NumberRangeBox from "@Obsidian/Controls/numberRangeBox.obs";
import GenderDropDownList from "@Obsidian/Controls/genderDropDownList.obs";
import SocialSecurityNumberBox from "@Obsidian/Controls/socialSecurityNumberBox.obs";
import TimePicker from "@Obsidian/Controls/timePicker.obs";
import UrlLinkBox from "@Obsidian/Controls/urlLinkBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import Rating from "@Obsidian/Controls/rating.obs";
import Fullscreen from "@Obsidian/Controls/fullscreen.obs";
import Panel from "@Obsidian/Controls/panel.obs";
import PersonPicker from "@Obsidian/Controls/personPicker.obs";
import FileUploader from "@Obsidian/Controls/fileUploader.obs";
import ImageUploader from "@Obsidian/Controls/imageUploader.obs";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker.obs";
import AchievementTypePicker from "@Obsidian/Controls/achievementTypePicker.obs";
import AssessmentTypePicker from "@Obsidian/Controls/assessmentTypePicker.obs";
import AssetStorageProviderPicker from "@Obsidian/Controls/assetStorageProviderPicker.obs";
import BinaryFileTypePicker from "@Obsidian/Controls/binaryFileTypePicker.obs";
import BinaryFilePicker from "@Obsidian/Controls/binaryFilePicker.obs";
import SlidingDateRangePicker from "@Obsidian/Controls/slidingDateRangePicker.obs";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker.obs";
import CategoryPicker from "@Obsidian/Controls/categoryPicker.obs";
import LocationItemPicker from "@Obsidian/Controls/locationItemPicker.obs";
import ConnectionRequestPicker from "@Obsidian/Controls/connectionRequestPicker.obs";
import CopyButton from "@Obsidian/Controls/copyButton.obs";
import TagList from "@Obsidian/Controls/tagList.obs";
import Following from "@Obsidian/Controls/following.obs";
import AuditDetail from "@Obsidian/Controls/auditDetail.obs";
import CampusPicker from "@Obsidian/Controls/campusPicker.obs";
import DetailBlock from "@Obsidian/Templates/detailBlock";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { newGuid } from "@Obsidian/Utility/guid";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { AssessmentType } from "@Obsidian/SystemGuids/assessmentType";
import { BinaryFiletype } from "@Obsidian/SystemGuids/binaryFiletype";
import { DefinedType } from "@Obsidian/SystemGuids/definedType";
import { DefinedValue } from "@Obsidian/SystemGuids/definedValue";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
import { FieldType } from "@Obsidian/SystemGuids/fieldType";
import { SlidingDateRange, rangeTypeOptions } from "@Obsidian/Utility/slidingDateRange";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { sleep } from "@Obsidian/Utility/promiseUtils";
import { upperCaseFirstCharacter } from "@Obsidian/Utility/stringUtils";
import TransitionVerticalCollapse from "@Obsidian/Controls/transitionVerticalCollapse.obs";
import SectionContainer from "@Obsidian/Controls/sectionContainer.obs";
import SectionHeader from "@Obsidian/Controls/sectionHeader.obs";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { PickerDisplayStyle } from "@Obsidian/Enums/Controls/pickerDisplayStyle";
import { useStore } from "@Obsidian/PageState";
import BadgeComponentPicker from "@Obsidian/Controls/badgeComponentPicker.obs";
import ComponentPicker from "@Obsidian/Controls/componentPicker.obs";
import Modal from "@Obsidian/Controls/modal.obs";
import EventItemPicker from "@Obsidian/Controls/eventItemPicker.obs";
import DataViewPicker from "@Obsidian/Controls/dataViewPicker.obs";
import WorkflowTypePicker from "@Obsidian/Controls/workflowTypePicker.obs";
import FinancialGatewayPicker from "@Obsidian/Controls/financialGatewayPicker.obs";
import FinancialStatementTemplatePicker from "@Obsidian/Controls/financialStatementTemplatePicker.obs";
import FieldTypePicker from "@Obsidian/Controls/fieldTypePicker.obs";
import GradePicker from "@Obsidian/Controls/gradePicker.obs";
import ScheduleBuilder from "@Obsidian/Controls/scheduleBuilder.obs";
import GroupMemberPicker from "@Obsidian/Controls/groupMemberPicker.obs";
import InteractionChannelPicker from "@Obsidian/Controls/interactionChannelPicker.obs";
import InteractionComponentPicker from "@Obsidian/Controls/interactionComponentPicker.obs";
import LavaCommandPicker from "@Obsidian/Controls/lavaCommandPicker.obs";
import RemoteAuthsPicker from "@Obsidian/Controls/remoteAuthsPicker.obs";
import StepProgramPicker from "@Obsidian/Controls/stepProgramPicker.obs";
import StepProgramStepTypePicker from "@Obsidian/Controls/stepProgramStepTypePicker.obs";
import StepProgramStepStatusPicker from "@Obsidian/Controls/stepProgramStepStatusPicker.obs";
import StepStatusPicker from "@Obsidian/Controls/stepStatusPicker.obs";
import StepTypePicker from "@Obsidian/Controls/stepTypePicker.obs";
import StreakTypePicker from "@Obsidian/Controls/streakTypePicker.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { AlertType } from "@Obsidian/Enums/Controls/alertType";
import BadgeList from "@Obsidian/Controls/badgeList.obs";
import BadgePicker from "@Obsidian/Controls/badgePicker.obs";
import BasicTimePicker from "@Obsidian/Controls/basicTimePicker.obs";
import CountdownTimer from "@Obsidian/Controls/countdownTimer.obs";
import MediaSelector from "@Obsidian/Controls/mediaSelector.obs";
import ElectronicSignature from "@Obsidian/Controls/electronicSignature.obs";
import { FieldTypeEditorUpdateAttributeConfigurationOptionsBag } from "@Obsidian/ViewModels/Controls/fieldTypeEditorUpdateAttributeConfigurationOptionsBag";
import FieldTypeEditor from "@Obsidian/Controls/fieldTypeEditor.obs";
import InlineRangeSlider from "@Obsidian/Controls/inlineRangeSlider.obs";
import RangeSlider from "@Obsidian/Controls/rangeSlider.obs";
import JavaScriptAnchor from "@Obsidian/Controls/javaScriptAnchor.obs";
import KeyValueList from "@Obsidian/Controls/keyValueList.obs";
import Loading from "@Obsidian/Controls/loading.obs";
import LoadingIndicator from "@Obsidian/Controls/loadingIndicator.obs";
import NumberUpDownGroup from "@Obsidian/Controls/numberUpDownGroup.obs";
import { NumberUpDownGroupOption } from "@Obsidian/Types/Controls/numberUpDownGroup";
import ProgressBar from "@Obsidian/Controls/progressBar.obs";
import RockLabel from "@Obsidian/Controls/rockLabel.obs";
import RockValidation from "@Obsidian/Controls/rockValidation.obs";
import TabbedBar from "@Obsidian/Controls/tabbedBar.obs";
import TabbedContent from "@Obsidian/Controls/tabbedContent.obs";
import ValueDetailList from "@Obsidian/Controls/valueDetailList.obs";
import PagePicker from "@Obsidian/Controls/pagePicker.obs";
import GroupPicker from "@Obsidian/Controls/groupPicker.obs";
import MergeTemplatePicker from "@Obsidian/Controls/mergeTemplatePicker.obs";
import { MergeTemplateOwnership } from "@Obsidian/Enums/Controls/mergeTemplateOwnership";
import MetricCategoryPicker from "@Obsidian/Controls/metricCategoryPicker.obs";
import MetricItemPicker from "@Obsidian/Controls/metricItemPicker.obs";
import RegistrationTemplatePicker from "@Obsidian/Controls/registrationTemplatePicker.obs";
import ReportPicker from "@Obsidian/Controls/reportPicker.obs";
import SchedulePicker from "@Obsidian/Controls/schedulePicker.obs";
import WorkflowActionTypePicker from "@Obsidian/Controls/workflowActionTypePicker.obs";
import DayOfWeekPicker from "@Obsidian/Controls/dayOfWeekPicker.obs";
import MonthDayPicker from "@Obsidian/Controls/monthDayPicker.obs";
import MonthYearPicker from "@Obsidian/Controls/monthYearPicker.obs";
import { RockCacheability } from "@Obsidian/ViewModels/Controls/rockCacheability";
import CacheabilityPicker from "@Obsidian/Controls/cacheabilityPicker.obs";
import ButtonGroup from "@Obsidian/Controls/buttonGroup.obs";
import IntervalPicker from "@Obsidian/Controls/intervalPicker.obs";
import GeoPicker from "@Obsidian/Controls/geoPicker.obs";
import ContentDropDownPicker from "@Obsidian/Controls/contentDropDownPicker.obs";
import WordCloud from "@Obsidian/Controls/wordCloud.obs";
import EventCalendarPicker from "@Obsidian/Controls/eventCalendarPicker.obs";
import GroupTypePicker from "@Obsidian/Controls/groupTypePicker.obs";
import LocationAddressPicker from "@Obsidian/Controls/locationAddressPicker.obs";
import LocationPicker from "@Obsidian/Controls/locationPicker.obs";
import LocationList from "@Obsidian/Controls/locationList.obs";
import EthnicityPicker from "@Obsidian/Controls/ethnicityPicker.obs";
import RacePicker from "@Obsidian/Controls/racePicker.obs";
import MediaElementPicker from "@Obsidian/Controls/mediaElementPicker.obs";
import MergeFieldPicker from "@Obsidian/Controls/mergeFieldPicker.obs";
import CategorizedValuePicker from "@Obsidian/Controls/categorizedValuePicker.obs";
import ReminderTypePicker from "@Obsidian/Controls/reminderTypePicker.obs";
import GroupRolePicker from "@Obsidian/Controls/groupRolePicker.obs";
import ModalAlert from "@Obsidian/Controls/modalAlert.obs";
import { ModalAlertType } from "@Obsidian/Enums/Controls/modalAlertType";
import ContentChannelItemPicker from "@Obsidian/Controls/contentChannelItemPicker.obs";
import PersonLink from "@Obsidian/Controls/personLink.obs";
import PopOver from "@Obsidian/Controls/popOver.obs";
import RockLiteral from "@Obsidian/Controls/rockLiteral.obs";
import RegistryEntry from "@Obsidian/Controls/registryEntry.obs";
import GroupTypeGroupPicker from "@Obsidian/Controls/groupTypeGroupPicker.obs";
import GroupAndRolePicker from "@Obsidian/Controls/groupAndRolePicker.obs";
import AccountPicker from "@Obsidian/Controls/accountPicker.obs";
import NoteTextEditor from "@Obsidian/Controls/noteTextEditor.obs";
import StructuredContentEditor from "@Obsidian/Controls/structuredContentEditor.obs";
import RegistrationInstancePicker from "@Obsidian/Controls/registrationInstancePicker.obs";
import InteractionChannelInteractionComponentPicker from "@Obsidian/Controls/interactionChannelInteractionComponentPicker.obs";
import WorkflowPicker from "@Obsidian/Controls/workflowPicker.obs";
import ValueList from "@Obsidian/Controls/valueList.obs";
import BlockTemplatePicker from "@Obsidian/Controls/blockTemplatePicker.obs";
import ButtonDropDownList from "@Obsidian/Controls/buttonDropDownList.obs";
import DropDownMenuGallery from "./ControlGallery/dropDownMenuGallery.partial.obs";
import DropDownContentGallery from "./ControlGallery/dropDownContentGallery.partial.obs";
import ButtonDropDownListGallery from "./ControlGallery/buttonDropDownListGallery.partial.obs";
import CampusAccountAmountPickerGallery from "./ControlGallery/campusAccountAmountPickerGallery.partial.obs";
import PersonPickerGallery from "./ControlGallery/personPickerGallery.partial.obs";
import ImageEditorGallery from "./ControlGallery/imageEditorGallery.partial.obs";
import HighlightLabelGallery from "./ControlGallery/highlightLabelGallery.partial.obs";
import { MediaSelectorMode } from "@Obsidian/Enums/Controls/mediaSelectorMode";
import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";
import LightGridGallery from "./ControlGallery/lightGridGallery.partial.obs";
import PdfViewerGallery from "./ControlGallery/pdfViewerGallery.partial.obs";
import ChartGallery from "./ControlGallery/chartGallery.partial.obs";
import EntityPickerGallery from "./ControlGallery/entityPickerGallery.partial.obs";
import PersonBasicEditorGallery from "./ControlGallery/personBasicEditorGallery.partial.obs";
import AttributeMatrixEditorGallery from "./ControlGallery/attributeMatrixEditorGallery.partial.obs";
import BadgeControlGallery from "./ControlGallery/badgeControlGallery.partial.obs";
import BadgeGallery from "./ControlGallery/badgeGallery.partial.obs";
import WarningBlockGallery from "./ControlGallery/warningBlockGallery.partial.obs";
import KeyValueListGallery from "./ControlGallery/keyValueListGallery.partial.obs";
import YearPickerGallery from "./ControlGallery/yearPickerGallery.partial.obs";
import CurrencyBoxGallery from "./ControlGallery/currencyBoxGallery.partial.obs";


// #region Control Gallery

/** Demonstrates an attribute values container. */
const attributeValuesContainerGallery = defineComponent({
    name: "AttributeValuesContainerGallery",
    components: {
        GalleryAndResult,
        AttributeValuesContainer,
        CheckBox,
        NumberBox,
        TextBox
    },
    setup() {
        const isEditMode = ref(false);
        const showAbbreviatedName = ref(false);
        const showEmptyValues = ref(true);
        const displayAsTabs = ref(false);
        const showCategoryLabel = ref(true);
        const numberOfColumns = ref(2);
        const entityName = ref("Foo Entity");

        const categories = [{
            guid: newGuid(),
            name: "Cat A",
            order: 1
        },
        {
            guid: newGuid(),
            name: "Cat B",
            order: 2
        },
        {
            guid: newGuid(),
            name: "Cat C",
            order: 3
        }];

        const attributes = ref<Record<string, PublicAttributeBag>>({
            text: {
                attributeGuid: newGuid(),
                categories: [categories[0]],
                description: "A text attribute.",
                fieldTypeGuid: FieldType.Text,
                isRequired: false,
                key: "text",
                name: "Text Attribute",
                order: 2,
                configurationValues: {},
                preHtml: "<div class='bg-primary p-3'>"
            },
            color: {
                attributeGuid: newGuid(),
                categories: [categories[0], categories[2]],
                description: "Favorite color? Or just a good one?",
                fieldTypeGuid: FieldType.Color,
                isRequired: true,
                key: "color",
                name: "Random Color",
                order: 4,
                configurationValues: {},
                postHtml: "</div>"
            },
            bool: {
                attributeGuid: newGuid(),
                categories: [categories[2]],
                description: "Are you foo?",
                fieldTypeGuid: FieldType.Boolean,
                isRequired: false,
                key: "bool",
                name: "Boolean Attribute",
                order: 3,
                configurationValues: {}
            },
            textagain: {
                attributeGuid: newGuid(),
                categories: [categories[1]],
                description: "Another text attribute.",
                fieldTypeGuid: FieldType.Text,
                isRequired: false,
                key: "textAgain",
                name: "Some Text",
                order: 5,
                configurationValues: {},
                preHtml: "<h5>PRE HTML!</h5>"
            },
            single: {
                attributeGuid: newGuid(),
                categories: [],
                description: "A single select attribute.",
                fieldTypeGuid: FieldType.SingleSelect,
                isRequired: false,
                key: "single",
                name: "Single Select",
                order: 1,
                configurationValues: {
                    values: JSON.stringify([{ value: "1", text: "One" }, { value: "2", text: "Two" }, { value: "3", text: "Three" }])
                }
            }
        });

        const attributeValues = ref<Record<string, string>>({
            "text": "Default text value",
            "color": "#336699",
            "bool": "N",
            "textAgain": "",
            single: "1"
        });

        return {
            attributes,
            attributeValues,
            isEditMode,
            showAbbreviatedName,
            showEmptyValues,
            displayAsTabs,
            showCategoryLabel,
            numberOfColumns,
            entityName,
            showPrePost: ref(false),
            importCode: getControlImportPath("attributeValuesContainer"),
            exampleCode: `<AttributeValuesContainer v-model="attributeValues" :attributes="attributes" :isEditMode="false" :showAbbreviatedName="false" :showEmptyValues="true" :displayAsTabs="false" :showCategoryLabel="true" :numberOfColumns="1" :entityTypeName="entityName" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{ attributes, modelValue: attributeValues }"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <AttributeValuesContainer
        v-model="attributeValues"
        :attributes="attributes"
        :isEditMode="isEditMode"
        :showAbbreviatedName="showAbbreviatedName"
        :showEmptyValues="showEmptyValues"
        :displayAsTabs="displayAsTabs"
        :showCategoryLabel="showCategoryLabel"
        :numberOfColumns="numberOfColumns"
        :entityTypeName="entityName" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-4" v-model="isEditMode" label="Edit Mode" text="Enable" help="Default: false" />
            <CheckBox formGroupClasses="col-sm-4" v-model="showAbbreviatedName" label="Abbreviated Name" text="Show" help="Default: false" />
            <CheckBox formGroupClasses="col-sm-4" v-model="showEmptyValues" label="Empty Values" text="Show" help="Default: true; Only applies if not in edit mode" />
        </div>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-4" v-model="displayAsTabs" label="Category Tabs" text="Show" help="Default: false; If any attributes are in a category, display each category as a tab. Not applicable while editing." />
            <CheckBox formGroupClasses="col-sm-4" v-model="showCategoryLabel" label="Category Labels" text="Show" help="Default: false; Only applies when not displaying tabs." />
            <CheckBox formGroupClasses="col-sm-4" v-model="showPrePost" label="Render Pre/Post HTML" text="Show" help="Default: true" />
        </div>
        <div class="row">
            <NumberBox formGroupClasses="col-sm-6" v-model="numberOfColumns" label="Number of Columns" help="Default: 1; Only applies when not displaying tabs." />
            <TextBox formGroupClasses="col-sm-6" v-model="entityName" label="Entity Type" help="Default: ''; Appears in the heading when category labels are showing." />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a field visibility rules editor. */
const fieldFilterEditorGallery = defineComponent({
    name: "FieldFilterEditorGallery",
    components: {
        GalleryAndResult,
        FieldFilterEditor,
        CheckBox,
        TextBox
    },
    setup() {

        const sources: FieldFilterSourceBag[] = [
            {
                guid: "2a50d342-3a0b-4da3-83c1-25839c75615c",
                type: 0,
                attribute: {
                    attributeGuid: "4eb1eb34-988b-4212-8c93-844fae61b43c",
                    fieldTypeGuid: "9C204CD0-1233-41C5-818A-C5DA439445AA",
                    name: "Text Field",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {
                        maxcharacters: "10"
                    }
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c0",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b0",
                    fieldTypeGuid: "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                    name: "Integer Field",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {}
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c1",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b1",
                    fieldTypeGuid: "D747E6AE-C383-4E22-8846-71518E3DD06F",
                    name: "Color",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {
                        selectiontype: "Color Picker"
                    }
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c2",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b2",
                    fieldTypeGuid: "3EE69CBC-35CE-4496-88CC-8327A447603F",
                    name: "Currency",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {}
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c3",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b3",
                    fieldTypeGuid: "9C7D431C-875C-4792-9E76-93F3A32BB850",
                    name: "Date Range",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {}
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c4",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b4",
                    fieldTypeGuid: "7EDFA2DE-FDD3-4AC1-B356-1F5BFC231DAE",
                    name: "Day of Week",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {}
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c5",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b5",
                    fieldTypeGuid: "3D045CAE-EA72-4A04-B7BE-7FD1D6214217",
                    name: "Email",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {}
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c6",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b6",
                    fieldTypeGuid: "2E28779B-4C76-4142-AE8D-49EA31DDB503",
                    name: "Gender",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {
                        hideUnknownGender: "True"
                    }
                }
            },
            {
                guid: "6dbb47c4-5816-4110-8a52-92880d4d05c7",
                type: 0,
                attribute: {
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b7",
                    fieldTypeGuid: "C28C7BF3-A552-4D77-9408-DEDCF760CED0",
                    name: "Memo",
                    description: "",
                    order: 0,
                    isRequired: false,
                    configurationValues: {
                        numberofrows: "4",
                        allowhtml: "True",
                        maxcharacters: "5",
                        showcountdown: "True"
                    }
                }
            }
        ];

        const prefilled = (): FieldFilterGroupBag => ({
            guid: newGuid(),
            expressionType: 4,
            rules: [
                {
                    guid: "a81c3ef9-72a9-476b-8b88-b52f513d92e6",
                    comparisonType: 128,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b0",
                    value: "50"
                },
                {
                    guid: "74d34117-4cc6-4cea-92c5-8297aa693ba5",
                    comparisonType: 2,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b1",
                    value: "BlanchedAlmond"
                },
                {
                    guid: "0fa2b6ea-bc86-4fae-b0da-02e48fed8d96",
                    comparisonType: 8,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b5",
                    value: "@gmail.com"
                },
                {
                    guid: "434107e6-6c0c-4698-90ef-d615b1c2de4b",
                    comparisonType: 2,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b6",
                    value: "2"
                },
                {
                    guid: "706179b9-7518-4a74-8e0f-8a48016aec04",
                    comparisonType: 16,
                    sourceType: 0,
                    attributeGuid: "4eb1eb34-988b-4212-8c93-844fae61b43c",
                    value: "text"
                },
                {
                    guid: "4564eac2-15d9-48d9-b618-563523285af0",
                    comparisonType: 512,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b2",
                    value: "999"
                },
                {
                    guid: "e6c56d4c-7f63-44f9-8f07-1ea0860b605d",
                    comparisonType: 1,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b3",
                    value: "2022-02-01,2022-02-28"
                },
                {
                    guid: "0c27507f-9fb7-4f37-8026-70933bbf1398",
                    comparisonType: 0,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b4",
                    value: "3"
                },
                {
                    guid: "4f68fa2c-0942-4084-bb4d-3c045cef4551",
                    comparisonType: 8,
                    sourceType: 0,
                    attributeGuid: "c41817d8-be26-460c-9f89-a7059ae6a9b7",
                    value: "more text than I want to deal with...."
                }
            ]
        });

        const clean = (): FieldFilterGroupBag => ({
            guid: newGuid(),
            expressionType: 1,
            rules: []
        });

        const usePrefilled = ref(false);
        const value = ref(clean());

        watch(usePrefilled, () => {
            value.value = usePrefilled.value ? prefilled() : clean();
        });

        const title = ref("TEST PROPERTY");

        return {
            sources,
            value,
            title,
            usePrefilled,
            importCode: getControlImportPath("fieldFilterEditor"),
            exampleCode: `<FieldFilterEditor :sources="sources" v-model="value" :title="title" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{ 'output:modelValue':value, 'input:sources':sources }"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <FieldFilterEditor :sources="sources" v-model="value" :title="title" />

    <template #settings>
        <TextBox v-model="title" label="Attribute Name" />
        <CheckBox v-model="usePrefilled" text="Use prefilled data" />
    </template>
</GalleryAndResult>`
});

/** Demonstrates a phone number box */
const phoneNumberBoxGallery = defineComponent({
    name: "PhoneNumberBoxGallery",
    components: {
        GalleryAndResult,
        PhoneNumberBox,
        RockForm,
        RockButton
    },
    setup() {
        return {
            phoneNumber: ref(null),
            submit: ref(false),
            importCode: getSfcControlImportPath("phoneNumberBox"),
            exampleCode: `<PhoneNumberBox label="Phone Number" v-model="phoneNumber" />`
        };
    },
    template: `
<GalleryAndResult
    :value="phoneNumber"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RockForm v-model:submit="submit">
        <PhoneNumberBox label="Phone Number" v-model="phoneNumber" />
        <RockButton @click="submit=true">Validate</RockButton>
    </RockForm>

    <template #settings>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a help block */
const helpBlockGallery = defineComponent({
    name: "HelpBlockGallery",
    components: {
        GalleryAndResult,
        HelpBlock,
        TextBox
    },
    setup() {
        return {
            text: ref("This is some helpful text that explains something."),
            importCode: getControlImportPath("helpBlock"),
            exampleCode: `<HelpBlock text="text" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <HelpBlock :text="text" />
    Hover over the symbol to the left to view HelpBlock in action

    <template #settings>
        <TextBox label="Text" v-model="text" help="The text for the help tooltip to display" rules="required" />
    </template>
</GalleryAndResult>`
});

/** Demonstrates a drop down list */
const dropDownListGallery = defineComponent({
    name: "DropDownListGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList
    },
    setup() {
        const options: ListItemBag[] = [
            { text: "A Text", value: "a", category: "First" },
            { text: "B Text", value: "b", category: "First" },
            { text: "C Text", value: "c", category: "Second" },
            { text: "D Text", value: "d", category: "Second", disabled: true }
        ];

        // This function can be used to demonstrate lazy loading of items.
        const loadOptionsAsync = async (): Promise<ListItemBag[]> => {
            await sleep(5000);

            return options;
        };

        return {
            enhanceForLongLists: ref(false),
            loadOptionsAsync,
            showBlankItem: ref(true),
            grouped: ref(false),
            multiple: ref(false),
            value: ref(null),
            options,
            importCode: getControlImportPath("dropDownList"),
            exampleCode: `<DropDownList label="Select" v-model="value" :items="options" :showBlankItem="true" :enhanceForLongLists="false" :grouped="false" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{'output:modelValue': value, 'input:items': options}"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <DropDownList label="Select" v-model="value" :items="options" :showBlankItem="showBlankItem" :enhanceForLongLists="enhanceForLongLists" :grouped="grouped" :multiple="multiple" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-4" label="Show Blank Item" v-model="showBlankItem" />
            <CheckBox formGroupClasses="col-sm-4" label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            <CheckBox formGroupClasses="col-sm-4" label="Grouped" v-model="grouped" />
            <CheckBox formGroupClasses="col-sm-4" label="Multiple" v-model="multiple" />
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a radio button list */
const radioButtonListGallery = defineComponent({
    name: "RadioButtonListGallery",
    components: {
        GalleryAndResult,
        RadioButtonList,
        Toggle,
        NumberUpDown
    },
    setup() {
        return {
            value: ref("a"),
            isHorizontal: ref(false),
            repeatColumns: ref(0),
            options: [
                { text: "A Text", value: "a" },
                { text: "B Text", value: "b" },
                { text: "C Text", value: "c" },
                { text: "D Text", value: "d" },
                { text: "E Text", value: "e" },
                { text: "F Text", value: "f" },
                { text: "G Text", value: "g" }
            ] as ListItemBag[],
            importCode: getControlImportPath("radioButtonList"),
            exampleCode: `<RadioButtonList label="Radio List" v-model="value" :items="options" :horizontal="false" :repeatColumns="0" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{'output:modelValue': value, 'input:items': options}"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <RadioButtonList label="Radio List" v-model="value" :items="options" :horizontal="isHorizontal" :repeatColumns="repeatColumns" />

    <template #settings>
        <div class="row">
            <NumberUpDown formGroupClasses="col-sm-6" label="Horizontal Columns" v-model="repeatColumns" :min="0" />
            <Toggle formGroupClasses="col-sm-6" label="Horizontal" v-model="isHorizontal" />
        </div>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a checkbox */
const checkBoxGallery = defineComponent({
    name: "CheckBoxGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        TextBox
    },
    setup() {
        return {
            isChecked: ref(false),
            importCode: getControlImportPath("checkBox"),
            exampleCode: `<CheckBox label="Check Box" text="Enable" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="isChecked"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <CheckBox label="Check Box" text="Enable" v-model="isChecked" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates an inline checkbox */
const inlineCheckBoxGallery = defineComponent({
    name: "InlineCheckBoxGallery",
    components: {
        GalleryAndResult,
        InlineCheckBox
    },
    data() {
        return {
            isChecked: false,
            inline: true,
            importCode: getControlImportPath("checkBox"),
            exampleCode: `<CheckBox label="Check Box" text="Enable" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="isChecked"
    :importCode="importCode"
    :exampleCode="exampleCode"
    description="Check Box with label that is displayed beside it instead of above it"
    enableReflection >
    <InlineCheckBox label="Inline Label" v-model="isChecked" />
</GalleryAndResult>`
});

/** Demonstrates a modal / dialog / pop-up */
const dialogGallery = defineComponent({
    name: "DialogGallery",
    components: {
        GalleryAndResult,
        RockButton,
        Dialog,
        CheckBox
    },
    setup() {
        return {
            isDialogVisible: ref(false),
            isDismissible: ref(true),
            importCode: getControlImportPath("dialog"),
            exampleCode: `<Dialog v-model="isDialogVisible" :dismissible="true">
    <template #header>
        <h4>Dialog Header</h4>
    </template>
    <template #default>
        <p>Dialog Main Content</p>
    </template>
    <template #footer>
        <p>Dialog Footer (usually for buttons)</p>
    </template>
</Dialog>`
        };
    },
    template: `
<GalleryAndResult
    :value="isDialogVisible"
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <RockButton @click="isDialogVisible = true">Show</RockButton>

    <Dialog v-model="isDialogVisible" :dismissible="isDismissible">
        <template #header>
            <h4>Romans 11:33-36</h4>
        </template>
        <template #default>
            <p>
                Oh, the depth of the riches<br />
                and the wisdom and the knowledge of God!<br />
                How unsearchable his judgments<br />
                and untraceable his ways!<br />
                For who has known the mind of the Lord?<br />
                Or who has been his counselor?<br />
                And who has ever given to God,<br />
                that he should be repaid?<br />
                For from him and through him<br />
                and to him are all things.<br />
                To him be the glory forever. Amen.
            </p>
        </template>
        <template #footer>
            <RockButton @click="isDialogVisible = false" btnType="primary">OK</RockButton>
            <RockButton @click="isDialogVisible = false" btnType="default">Cancel</RockButton>
        </template>
    </Dialog>

    <template #settings>
        <CheckBox label="Dismissible" text="Show the close button" v-model="isDismissible" />
    </template>
</GalleryAndResult>`
});

/** Demonstrates check box list */
const checkBoxListGallery = defineComponent({
    name: "CheckBoxListGallery",
    components: {
        GalleryAndResult,
        CheckBoxList,
        NumberUpDown,
        Toggle
    },
    setup() {
        return {
            items: ref(["green"]),
            options: [
                { value: "red", text: "Red" },
                { value: "green", text: "Green" },
                { value: "blue", text: "Blue" }
            ] as ListItemBag[],
            isHorizontal: ref(false),
            repeatColumns: ref(0),
            importCode: getControlImportPath("checkBoxList"),
            exampleCode: `<CheckBoxList label="CheckBoxList" v-model="value" :items="options" :horizontal="false" :repeatColumns="0" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{'output:modelValue': items, 'input:items': options}"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <CheckBoxList label="CheckBoxList" v-model="items" :items="options" :horizontal="isHorizontal" :repeatColumns="repeatColumns" />

    <template #settings>
        <div class="row">
            <NumberUpDown formGroupClasses="col-sm-6" label="Horizontal Columns" v-model="repeatColumns" :min="0" />
            <Toggle formGroupClasses="col-sm-6" label="Horizontal" v-model="isHorizontal" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates media selector list */
const mediaSelectorGallery = defineComponent({
    name: "mediaSelectorGallery",
    components: {
        GalleryAndResult,
        MediaSelector,
        KeyValueList,
        DropDownList
    },
    setup() {
        return {
            items: ref([""]),
            mediaItems: [
            ] as KeyValueItem[],
            modeOptions: [
                {
                    text: "Image",
                    value: "0"
                },
                {
                    text: "Audio",
                    value: "1"
                }
            ] as ListItemBag[],
            mode: ref(MediaSelectorMode.Image),
            itemWidth: "100px",
            importCode: getControlImportPath("mediaSelector"),
            exampleCode: `<MediaSelector label="MediaSelector" v-model="value" :mediaItems="mediaItems" :itemWidth="itemWidth" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{'output:modelValue': items, 'input:items': mediaItems}"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <MediaSelector label="MediaSelector" v-model="items" :mediaItems="mediaItems" :mode="mode"/>

    <template #settings>
        <div class="row">
            <KeyValueList label="Media Items" v-model="mediaItems" />
            <DropDownList label="Mode" v-model="mode" :items="modeOptions" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a list box */
const listBoxGallery = defineComponent({
    name: "ListBoxGallery",
    components: {
        GalleryAndResult,
        ListBox,
        InlineCheckBox
    },
    setup() {
        return {
            value: ref(["a"]),
            options: [
                { text: "A Text", value: "a" },
                { text: "B Text", value: "b" },
                { text: "C Text", value: "c" },
                { text: "D Text", value: "d" }
            ] as ListItemBag[],
            enhanced: ref(false),
            importCode: getControlImportPath("listBox"),
            exampleCode: `<ListBox label="Select" v-model="value" :items="options" :enhanceForLongLists="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{'output:modelValue': value, 'input:items': options}"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <ListBox label="Select" v-model="value" :items="options" :enhanceForLongLists="enhanced" />

    <template #settings>
        <InlineCheckBox v-model="enhanced" label="Use Enhanced Functionality" />
    </template>
</GalleryAndResult>`
});

/** Demonstrates date pickers */
const datePickerGallery = defineComponent({
    name: "DatePickerGallery",
    components: {
        GalleryAndResult,
        DatePicker,
        InlineCheckBox
    },
    setup() {
        return {
            date: ref<string | null>(null),
            displayCurrentOption: ref(false),
            isCurrentDateOffset: ref(false),
            disableForceParse: ref(false),
            disableShowOnFocus: ref(false),
            disableHighlightToday: ref(false),
            disallowFutureDateSelection: ref(false),
            disallowPastDateSelection: ref(false),
            isDisabled: ref(false),
            startView: ref(0),
            viewOptions: [{ value: 0, text: "Month" }, { value: 1, text: "Year" }, { value: 2, text: "Decade" }],
            importCode: getControlImportPath("datePicker"),
            exampleCode: `<DatePicker label="Date" v-model="date"
    :displayCurrentOption="false"
    :isCurrentDateOffset="false"
    :disableForceParse="false"
    :disableShowOnFocus="false"
    :disableHighlightToday="false"
    :disallowFutureDateSelection="false"
    :disallowPastDateSelection="false"
    :startView="startView"
/>`
        };
    },
    template: `
<GalleryAndResult
    :value="date"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <DatePicker label="Date" v-model="date" :displayCurrentOption="displayCurrentOption" :isCurrentDateOffset="isCurrentDateOffset" :disabled="isDisabled" />

    <template #settings>
        <div class="row">
            <div class="col-sm-4">
                <InlineCheckBox v-model="displayCurrentOption" label="Display Current Option" />
            </div>
            <div class="col-sm-4">
                <InlineCheckBox v-model="isCurrentDateOffset" label="Is Current Date Offset" />
            </div>
            <div class="col-sm-4">
                <InlineCheckBox v-model="isDisabled" label="Disable" />
            </div>
        </div>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates date range pickers */
const dateRangePickerGallery = defineComponent({
    name: "DateRangePickerGallery",
    components: {
        GalleryAndResult,
        DateRangePicker
    },
    setup() {
        return {
            date: ref({}),
            disallowPastDateSelection: ref(false),
            importCode: getControlImportPath("dateRangePicker"),
            exampleCode: `<DateRangePicker label="Date Range" v-model="date"
    :disallowPastDateSelection="false"
/>`
        };
    },
    template: `
<GalleryAndResult
    :value="date"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <DateRangePicker label="Date Range" v-model="date" />

    <template #settings>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates date time pickers */
const dateTimePickerGallery = defineComponent({
    name: "DateTimePickerGallery",
    components: {
        GalleryAndResult,
        DateTimePicker,
        InlineCheckBox
    },
    setup() {
        return {
            date: ref<string | null>(null),
            displayCurrentOption: ref(false),
            isCurrentDateOffset: ref(false),
            disabled: ref(false),
            importCode: getControlImportPath("dateTimePicker"),
            exampleCode: `<DateTimePicker label="Date and Time" v-model="date" :displayCurrentOption="false" :isCurrentDateOffset="false" :disabled="disabled" />`
        };
    },
    template: `
<GalleryAndResult
    :value="date"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <DateTimePicker label="Date and Time" v-model="date" :displayCurrentOption="displayCurrentOption" :isCurrentDateOffset="isCurrentDateOffset" :disabled="disabled" />

    <template #settings>
        <div class="row">
            <div class="col-sm-4">
                <InlineCheckBox v-model="displayCurrentOption" label="Display Current Option" />
            </div>
            <div class="col-sm-4">
                <InlineCheckBox v-model="isCurrentDateOffset" label="Is Current Date Offset" />
            </div>
            <div class="col-sm-4">
                <InlineCheckBox v-model="disabled" label="Is Disabled" />
            </div>
        </div>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates date part pickers */
const datePartsPickerGallery = defineComponent({
    name: "DatePartsPickerGallery",
    components: {
        GalleryAndResult,
        Toggle,
        DatePartsPicker
    },
    setup() {
        return {
            showYear: ref(true),
            datePartsModel: ref<DatePartsPickerValue>({
                month: 1,
                day: 1,
                year: 2020
            }),
            importCode: getControlImportPath("datePartsPicker"),
            exampleCode: `<DatePartsPicker label="Date" v-model="date" :requireYear="true" :showYear="true" :allowFutureDates="true" :futureYearCount="50" :startYear="1900" />`
        };
    },
    template: `
<GalleryAndResult
    :value="datePartsModel"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <DatePartsPicker label="Date" v-model="datePartsModel" :showYear="showYear" />

    <template #settings>
        <Toggle label="Show Year" v-model="showYear" />
        <p class="mt-4 mb-4">The <a href="#BirthdayPickerGallery">Birthday Picker</a> simply wraps this control and sets <code>allowFutureDates</code> and <code>requireYear</code> to <code>false</code>.</p>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a textbox */
const textBoxGallery = defineComponent({
    name: "TextBoxGallery",
    components: {
        GalleryAndResult,
        TextBox
    },
    data() {
        return {
            text: "Some two-way bound text",
            importCode: getControlImportPath("textBox"),
            exampleCode: `<TextBox label="Text 1" v-model="text" :maxLength="50" showCountDown />`
        };
    },
    template: `
<GalleryAndResult
    :value="text"
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <TextBox label="Text 1" v-model="text" :maxLength="50" showCountDown />
    <TextBox label="Text 2" v-model="text" />
    <TextBox label="Memo" v-model="text" textMode="MultiLine" :rows="10" :maxLength="100" showCountDown />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a color picker */
const colorPickerGallery = defineComponent({
    name: "ColorPickerGallery",
    components: {
        GalleryAndResult,
        ColorPicker
    },
    setup() {
        return {
            value: ref("#ee7725"),
            importCode: getControlImportPath("colorPicker"),
            exampleCode: `<ColorPicker label="Color" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <ColorPicker label="Color" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a number box */
const numberBoxGallery = defineComponent({
    name: "NumberBoxGallery",
    components: {
        GalleryAndResult,
        RockForm,
        RockButton,
        TextBox,
        NumberBox
    },
    setup() {
        const minimumValue = ref("0");
        const maximumValue = ref("1");
        const value = ref(42);

        const numericMinimumValue = computed((): number => toNumber(minimumValue.value));
        const numericMaximumValue = computed((): number => toNumber(maximumValue.value));

        return {
            minimumValue,
            maximumValue,
            numericMinimumValue,
            numericMaximumValue,
            value,
            importCode: getControlImportPath("numberBox"),
            exampleCode: `<NumberBox label="Number" v-model="value" :minimumValue="minimumValue" :maximumValue="maximumValue" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <RockForm>
        <NumberBox label="Number" v-model="value" :minimumValue="numericMinimumValue" :maximumValue="numericMaximumValue" />
        <RockButton btnType="primary" type="submit">Test Validation</RockButton>
    </RockForm>

    <template #settings>
        <TextBox label="Minimum Value" v-model="minimumValue" />
        <TextBox label="Maximum Value" v-model="maximumValue" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a number box */
const numberRangeBoxGallery = defineComponent({
    name: "NumberRangeBoxGallery",
    components: {
        GalleryAndResult,
        NumberRangeBox
    },
    setup() {
        return {
            value: ref({ lower: 0, upper: 100 }),
            importCode: getControlImportPath("numberRangeBox"),
            exampleCode: `<NumberRangeBox label="Number Range" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <NumberRangeBox label="Number Range" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a gender picker */
const genderDropDownListGallery = defineComponent({
    name: "GenderDropDownListGallery",
    components: {
        GalleryAndResult,
        GenderDropDownList
    },
    setup() {
        return {
            value: ref("1"),
            importCode: getControlImportPath("genderDropDownList"),
            exampleCode: `<GenderDropDownList label="Your Gender" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <GenderDropDownList label="Your Gender" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code> and <code>Drop Down List</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a social security number box */
const socialSecurityNumberBoxGallery = defineComponent({
    name: "SocialSecurityNumberBoxGallery",
    components: {
        GalleryAndResult,
        SocialSecurityNumberBox
    },
    setup() {
        return {
            value: ref("123456789"),
            importCode: getControlImportPath("socialSecurityNumberBox"),
            exampleCode: `<SocialSecurityNumberBox label="SSN" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <SocialSecurityNumberBox label="SSN" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code> and <code>Drop Down List</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a time picker */
const timePickerGallery = defineComponent({
    name: "TimePickerGallery",
    components: {
        GalleryAndResult,
        TimePicker,
        CheckBox
    },
    setup() {
        return {
            value: ref({ hour: 14, minute: 15 }),
            disabled: ref(false),
            importCode: getSfcControlImportPath("timePicker"),
            exampleCode: `<TimePicker label="Time" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <TimePicker label="Time" v-model="value" :disabled="disabled" />

    <template #settings>
        <div>
            <CheckBox v-model="disabled" label="Disabled" />
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code> and <code>Drop Down List</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a rating picker */
const ratingGallery = defineComponent({
    name: "RatingGallery",
    components: {
        GalleryAndResult,
        NumberBox,
        Rating
    },
    setup() {
        return {
            value: ref(3),
            maximumValue: ref(5),
            importCode: getControlImportPath("rating"),
            exampleCode: `<Rating label="Rating" v-model="value" :maxRating="5" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <Rating label="How Would You Rate God?" v-model="value" :maxRating="maximumValue || 5" />

    <template #settings>
        <NumberBox label="Maximum Rating" v-model="maximumValue" />
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a switch */
const switchGallery = defineComponent({
    name: "SwitchGallery",
    components: {
        GalleryAndResult,
        Switch
    },
    setup() {
        return {
            isChecked: ref(false),
            importCode: getControlImportPath("switch"),
            exampleCode: `<Switch text="Switch" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="isChecked"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <Switch text="Switch" v-model="isChecked" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates an inline switch */
const inlineSwitchGallery = defineComponent({
    name: "InlineSwitchGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        InlineSwitch
    },
    setup() {
        return {
            isBold: ref(false),
            isChecked: ref(false),
            importCode: getControlImportPath("inlineSwitch"),
            exampleCode: `<InlineSwitch label="Inline Switch" v-model="value" :isBold="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="isChecked"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <InlineSwitch label="Inline Switch" v-model="isChecked" :isBold="isBold" />

    <template #settings>
        <CheckBox label="Is Bold" v-model="isBold" />
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates an email box */
const emailBoxGallery = defineComponent({
    name: "EmailBoxGallery",
    components: {
        GalleryAndResult,
        EmailBox
    },
    setup() {
        return {
            value: ref("ted@rocksolidchurchdemo.com"),
            importCode: getControlImportPath("emailBox"),
            exampleCode: `<EmailBox label="Email" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <EmailBox label="Email" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a number up down control */
const numberUpDownGallery = defineComponent({
    name: "NumberUpDownGallery",
    components: {
        GalleryAndResult,
        NumberUpDown
    },
    setup() {
        return {
            value: ref(1),
            importCode: getControlImportPath("numberUpDown"),
            exampleCode: `<NumberUpDown label="Number" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <NumberUpDown label="Number" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a static form control */
const staticFormControlGallery = defineComponent({
    name: "StaticFormControlGallery",
    components: {
        GalleryAndResult,
        StaticFormControl
    },
    setup() {
        return {
            value: ref("This is a static value"),
            importCode: getControlImportPath("staticFormControl"),
            exampleCode: `<StaticFormControl label="Static Value" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <StaticFormControl label="Static Value" v-model="value" />
</GalleryAndResult>`
});

/** Demonstrates an address control */
const addressControlGallery = defineComponent({
    name: "AddressControlGallery",
    components: {
        GalleryAndResult,
        RockForm,
        RockButton,
        CheckBox,
        AddressControl,
        ButtonDropDownList
    },
    setup() {
        const showCountrySelected = ref("default");
        const showCountry = computed(() => {
            return showCountrySelected.value == "true" ? true :
                showCountrySelected.value == "false" ? false : null;
        });

        return {
            value: ref({}),
            submit: ref(false),
            required: ref(false),
            partial: ref(false),
            showCountry,
            showCountrySelected,
            showCountryOptions: [
                {text: "Default", value: "default"},
                {text: "Yes", value: "true"},
                {text: "No", value: "false"},
            ],
            importCode: getSfcControlImportPath("addressControl"),
            exampleCode: `<AddressControl label="Address" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RockForm v-model:submit="submit">
    <AddressControl label="Address" v-model="value" :rules="required ? 'required' : ''" :partialAddressIsAllowed="partial" :showCountry="showCountry" />

    <RockButton @click="submit=true">Validate</RockButton>
    </RockForm>

    <template #settings>
        <div class="row">
            <div class="col-sm-4">
                <CheckBox label="Required" v-model="required" />
            </div>
            <div class="col-sm-4">
                <CheckBox label="Allow Partial Addresses" v-model="partial" />
            </div>
            <div class="col-sm-4">
                <ButtonDropDownList label="Show Country" v-model="showCountrySelected" :items="showCountryOptions" help="If no value is passed in, the visibility of the Country field will depend on the 'Support International Addresses' Global Attribute setting." />
            </div>
        </div>
        <p>All props match that of a <code>Rock Form Field</code></p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a toggle button */
const toggleGallery = defineComponent({
    name: "ToggleGallery",
    components: {
        GalleryAndResult,
        TextBox,
        DropDownList,
        Toggle
    },
    setup() {
        return {
            trueText: ref("On"),
            falseText: ref("Off"),
            btnSize: ref("sm"),
            sizeOptions: [
                { value: "lg", text: "Large" },
                { value: "md", text: "Medium" },
                { value: "sm", text: "Small" },
                { value: "xs", text: "Extra Small" },
            ],
            value: ref(false),
            importCode: getControlImportPath("toggle"),
            exampleCode: `<Toggle label="Toggle" v-model="value" trueText="On" falseText="Off" :btnSize="btnSize" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <Toggle label="Toggle" v-model="value" :trueText="trueText" :falseText="falseText" :btnSize="btnSize" />

    <template #settings>
        <TextBox label="True Text" v-model="trueText" />
        <TextBox label="False Text" v-model="falseText" />
        <DropDownList label="Button Size" v-model="btnSize" :items="sizeOptions" />

        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a progress tracker */
const progressTrackerGallery = defineComponent({
    name: "ProgressTrackerGallery",
    components: {
        GalleryAndResult,
        NumberUpDown,
        ProgressTracker
    },
    setup() {
        return {
            value: ref(0),
            items: [
                { key: "S", title: "Start", subtitle: "The beginning" },
                { key: "1", title: "Step 1", subtitle: "The first step" },
                { key: "2", title: "Step 2", subtitle: "The second step" },
                { key: "3", title: "Step 3", subtitle: "The third step" },
                { key: "4", title: "Step 4", subtitle: "The fourth step" },
                { key: "5", title: "Step 5", subtitle: "The fifth step" },
                { key: "6", title: "Step 6", subtitle: "The sixth step" },
                { key: "7", title: "Step 7", subtitle: "The seventh step" },
                { key: "8", title: "Step 8", subtitle: "The eighth step" },
                { key: "F", title: "Finish", subtitle: "The finish" }
            ] as ProgressTrackerItem[],
            importCode: getControlImportPath("progressTracker"),
            exampleCode: `<ProgressTracker :items="items" :currentIndex="0" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <ProgressTracker :items="items" :currentIndex="value" />

    <template #settings>
        <NumberUpDown label="Index" v-model="value" :min="0" :max="10" />
    </template>
</GalleryAndResult>`
});

/** Demonstrates an items with pre and post html control */
const itemsWithPreAndPostHtmlGallery = defineComponent({
    name: "ItemsWithPreAndPostHtmlGallery",
    components: {
        GalleryAndResult,
        TextBox,
        ItemsWithPreAndPostHtml
    },
    setup() {
        return {
            value: ref<ItemWithPreAndPostHtml[]>([
                { preHtml: '<div class="row"><div class="col-sm-6">', postHtml: "</div>", slotName: "item1" },
                { preHtml: '<div class="col-sm-6">', postHtml: "</div></div>", slotName: "item2" }
            ]),
            importCode: getControlImportPath("itemsWithPreAndPostHtml"),
            exampleCode: `<ItemsWithPreAndPostHtml :items="value">
    <template #item1>
        <div>This is item 1</div>
    </template>
    <template #item2>
        <div>This is item 2</div>
    </template>
</ItemsWithPreAndPostHtml>`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <ItemsWithPreAndPostHtml :items="value">
        <template #item1>
            <div class="padding-all-sm text-center bg-primary">This is item 1</div>
        </template>
        <template #item2>
            <div class="padding-all-sm text-center bg-primary">This is item 2</div>
        </template>
    </ItemsWithPreAndPostHtml>

    <template #settings>
        <TextBox label="Item 1 - Pre Html" v-model="value[0].preHtml" />
        <TextBox label="Item 1 - Post Html" v-model="value[0].postHtml" />
        <TextBox label="Item 2 - Pre Html" v-model="value[1].preHtml" />
        <TextBox label="Item 2 - Post Html" v-model="value[1].postHtml" />
    </template>
</GalleryAndResult>`
});

/** Demonstrates a URL link box */
const urlLinkBoxGallery = defineComponent({
    name: "UrlLinkBoxGallery",
    components: {
        UrlLinkBox,
        GalleryAndResult
    },
    setup() {
        return {
            value: ref("/home/"),
            importCode: getControlImportPath("urlLinkBox"),
            exampleCode: `<UrlLinkBox label="URL" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <UrlLinkBox label="URL" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates the fullscreen component. */
const fullscreenGallery = defineComponent({
    name: "FullscreenGallery",
    components: {
        GalleryAndResult,
        InlineSwitch,
        CheckBox,
        Fullscreen
    },
    setup() {
        return {
            pageOnly: ref(true),
            value: ref(false),
            importCode: getControlImportPath("fullscreen"),
            exampleCode: `<Fullscreen v-model="value" :isPageOnly="true">
    <p>Content to make full screen</p>
</Fullscreen>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <Fullscreen v-model="value" :isPageOnly="pageOnly">
        <div class="bg-info padding-all-md" style="width:100%; height: 100%; min-height: 300px; display: grid; place-content: center;">
            <InlineSwitch v-model="value" label="Fullscreen" :isBold="true" />
        </div>
    </Fullscreen>

    <template #settings>
        <CheckBox v-model="pageOnly" label="Is Page Only" help="If true, fills content window. If false, hides the browser chrome and fills entire screen." />
    </template>
</GalleryAndResult>`
});

/** Demonstrates the panel component. */
const panelGallery = defineComponent({
    name: "PanelGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        CheckBoxList,
        Panel,
        RockButton
    },

    setup() {
        const simulateValues = ref<string[]>([]);

        const headerSecondaryActions = computed((): PanelAction[] => {
            if (!simulateValues.value.includes("headerSecondaryActions")) {
                return [];
            }

            return [
                {
                    iconCssClass: "fa fa-user",
                    title: "Action 1",
                    type: "default",
                    handler: () => alert("Action 1 selected.")
                },
                {
                    iconCssClass: "fa fa-group",
                    title: "Action 2",
                    type: "default",
                    handler: () => alert("Action 2 selected.")
                }
            ];
        });

        return {
            colors: Array.apply(0, Array(256)).map((_: unknown, index: number) => `rgb(${index}, ${index}, ${index})`),
            collapsibleValue: ref(true),
            drawerValue: ref(false),
            hasFullscreen: ref(false),
            hasZoom: ref(false),
            headerSecondaryActions,
            simulateValues,
            simulateOptions: [
                {
                    value: "drawer",
                    text: "Drawer"
                },
                {
                    value: "headerActions",
                    text: "Header Actions"
                },
                {
                    value: "headerSecondaryActions",
                    text: "Header Secondary Actions"
                },
                {
                    value: "subheaderLeft",
                    text: "Subheader Left",
                },
                {
                    value: "subheaderRight",
                    text: "Subheader Right"
                },
                {
                    value: "footerActions",
                    text: "Footer Actions"
                },
                {
                    value: "footerSecondaryActions",
                    text: "Footer Secondary Actions"
                },
                {
                    value: "helpContent",
                    text: "Help Content"
                },
                {
                    value: "largeBody",
                    text: "Large Body"
                }
            ],
            simulateDrawer: computed((): boolean => simulateValues.value.includes("drawer")),
            simulateHeaderActions: computed((): boolean => simulateValues.value.includes("headerActions")),
            simulateSubheaderLeft: computed((): boolean => simulateValues.value.includes("subheaderLeft")),
            simulateSubheaderRight: computed((): boolean => simulateValues.value.includes("subheaderRight")),
            simulateFooterActions: computed((): boolean => simulateValues.value.includes("footerActions")),
            simulateFooterSecondaryActions: computed((): boolean => simulateValues.value.includes("footerSecondaryActions")),
            simulateLargeBody: computed((): boolean => simulateValues.value.includes("largeBody")),
            simulateHelp: computed((): boolean => simulateValues.value.includes("helpContent")),
            isFullscreenPageOnly: ref(true),
            value: ref(true),
            importCode: getSfcControlImportPath("panel"),
            exampleCode: `<Panel v-model="isExanded" v-model:isDrawerOpen="false" title="Panel Title" :hasCollapse="true" :hasFullscreen="false" :isFullscreenPageOnly="true" :headerSecondaryActions="false">
    <template #helpContent>Help Content</template>
    <template #drawer>Drawer Content</template>
    <template #headerActions>Header Actions</template>
    <template #subheaderLeft>Sub Header Left</template>
    <template #subheaderRight>Sub Header Right</template>
    <template #footerActions>Footer Actions</template>
    <template #footerSecondaryActions>Footer Secondary Actions</template>

    Main Panel Content
</Panel>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <Panel v-model="value" v-model:isDrawerOpen="drawerValue" :hasCollapse="collapsibleValue" :hasZoom="hasZoom" :hasFullscreen="hasFullscreen" :isFullscreenPageOnly="isFullscreenPageOnly" title="Panel Title" :headerSecondaryActions="headerSecondaryActions">
        <template v-if="simulateHelp" #helpContent>
            This is some help text.
        </template>

        <template v-if="simulateDrawer" #drawer>
            <div style="text-align: center;">Drawer Content</div>
        </template>

        <template v-if="simulateHeaderActions" #headerActions>
            <span class="action">
                <i class="fa fa-star-o"></i>
            </span>

            <span class="action">
                <i class="fa fa-user"></i>
            </span>
        </template>

        <template v-if="simulateSubheaderLeft" #subheaderLeft>
            <span class="label label-warning">Warning</span>&nbsp;
            <span class="label label-default">Default</span>
        </template>

        <template v-if="simulateSubheaderRight" #subheaderRight>
            <span class="label label-info">Info</span>&nbsp;
            <span class="label label-default">Default</span>
        </template>

        <template v-if="simulateFooterActions" #footerActions>
            <RockButton btnType="primary">Action 1</RockButton>
            <RockButton btnType="primary">Action 2</RockButton>
        </template>

        <template v-if="simulateFooterSecondaryActions" #footerSecondaryActions>
            <RockButton btnType="default"><i class="fa fa-lock"></i></RockButton>
            <RockButton btnType="default"><i class="fa fa-unlock"></i></RockButton>
        </template>


        <h4>Romans 11:33-36</h4>
        <p>
            Oh, the depth of the riches<br />
            and the wisdom and the knowledge of God!<br />
            How unsearchable his judgments<br />
            and untraceable his ways!<br />
            For who has known the mind of the Lord?<br />
            Or who has been his counselor?<br />
            And who has ever given to God,<br />
            that he should be repaid?<br />
            For from him and through him<br />
            and to him are all things.<br />
            To him be the glory forever. Amen.
        </p>
    </Panel>

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-3" v-model="collapsibleValue" label="Collapsible" />
            <CheckBox formGroupClasses="col-sm-3" v-model="value" label="Panel Open" />
            <CheckBox formGroupClasses="col-sm-3" v-model="hasFullscreen" label="Has Fullscreen" />
            <CheckBox formGroupClasses="col-sm-3" v-model="isFullscreenPageOnly" label="Page Only Fullscreen" />
            <CheckBox formGroupClasses="col-sm-3" v-model="hasZoom" label="Has Zoom" />
        </div>
        <CheckBoxList v-model="simulateValues" label="Simulate" :items="simulateOptions" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates the file uploader component. */
const fileUploaderGallery = defineComponent({
    name: "FileUploaderGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        FileUploader,
        TextBox
    },
    setup() {
        return {
            binaryFileTypeGuid: ref(BinaryFiletype.Default),
            showDeleteButton: ref(true),
            uploadAsTemporary: ref(true),
            uploadButtonText: ref("Upload"),
            value: ref(null),
            importCode: getControlImportPath("fileUploader"),
            exampleCode: `<FileUploader v-model="value" label="File Uploader" :uploadAsTemporary="true" :binaryFileTypeGuid="BinaryFiletype.Default" uploadButtonText="Upload" :showDeleteButton="true" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <FileUploader v-model="value"
        label="File Uploader"
        :uploadAsTemporary="uploadAsTemporary"
        :binaryFileTypeGuid="binaryFileTypeGuid"
        :uploadButtonText="uploadButtonText"
        :showDeleteButton="showDeleteButton" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-4" v-model="uploadAsTemporary" label="Upload As Temporary" />
            <TextBox formGroupClasses="col-sm-8" v-model="binaryFileTypeGuid" label="Binary File Type Guid" />
        </div>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-4" v-model="showDeleteButton" label="Show Delete Button" />
            <TextBox formGroupClasses="col-sm-8" v-model="uploadButtonText" label="Upload Button Text" />
        </div>

        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates the image uploader component. */
const imageUploaderGallery = defineComponent({
    name: "ImageUploaderGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        ImageUploader,
        TextBox
    },
    setup() {
        return {
            binaryFileTypeGuid: ref(BinaryFiletype.Default),
            showDeleteButton: ref(true),
            uploadAsTemporary: ref(true),
            uploadButtonText: ref("Upload"),
            value: ref(null),
            importCode: getControlImportPath("imageUploader"),
            exampleCode: `<ImageUploader v-model="value" label="Image Uploader" :uploadAsTemporary="true" :binaryFileTypeGuid="BinaryFiletype.Default" uploadButtonText="Upload" :showDeleteButton="true" />`
        };
    },
    template: `
    <GalleryAndResult
        :value="value"
        :importCode="importCode"
        :exampleCode="exampleCode"
        enableReflection >
        <ImageUploader v-model="value"
            label="Image Uploader"
            :uploadAsTemporary="uploadAsTemporary"
            :binaryFileTypeGuid="binaryFileTypeGuid"
            :uploadButtonText="uploadButtonText"
            :showDeleteButton="showDeleteButton" />

        <template #settings>
            <div class="row">
                <CheckBox formGroupClasses="col-sm-4" v-model="uploadAsTemporary" label="Upload As Temporary" />
                <TextBox formGroupClasses="col-sm-8" v-model="binaryFileTypeGuid" label="Binary File Type Guid" />
            </div>
            <div class="row">
                <CheckBox formGroupClasses="col-sm-4" v-model="showDeleteButton" label="Show Delete Button" />
                <TextBox formGroupClasses="col-sm-8" v-model="uploadButtonText" label="Upload Button Text" />
            </div>

            <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
        </template>
    </GalleryAndResult>`
});

/** Demonstrates a sliding date range picker */
const slidingDateRangePickerGallery = defineComponent({
    name: "SlidingDateRangePickerGallery",
    components: {
        GalleryAndResult,
        SlidingDateRangePicker,
        DropDownList
    },
    setup() {
        const value = ref<SlidingDateRange | null>(null);

        return {
            value,
            rangeTypeOptions: rangeTypeOptions,
            rangeTypes: ref(null),
            previewLocation: ref("Right"),
            previewLocationOptions: [
                {
                    text: "Right (Default)",
                    value: "Right"
                },
                {
                    text: "Top",
                    value: "Top"
                },
                {
                    text: "None",
                    value: "None"
                },
            ],
            importCode: getSfcControlImportPath("slidingDateRangePicker") +
                "\n// If Customizing Date Range Types" +
                "\nimport { RangeType } from \"@Obsidian/Utility/slidingDateRange\";",
            exampleCode: `<SlidingDateRangePicker v-model="value" label="Sliding Date Range" :enabledSlidingDateRangeUnits="[RangeType.Current, RangeType.Previous, RangeType.Next]" previewLocation="Right" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <SlidingDateRangePicker
        v-model="value"
        label="Sliding Date Range"
        :enabledSlidingDateRangeUnits="rangeTypes"
        :previewLocation="previewLocation" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <DropDownList v-model="rangeTypes" :items="rangeTypeOptions" multiple showBlankItem label="Available Range Types" />
            </div>
            <div class="col-md-4">
                <DropDownList v-model="previewLocation" :items="previewLocationOptions" showBlankItem label="Date Preview Location" />
            </div>
        </div>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates defined value picker */
const definedValuePickerGallery = defineComponent({
    name: "DefinedValuePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DefinedValuePicker,
        TextBox
    },
    setup() {

        function onsubmit(): void {
            alert("control gallery form submitted");
        }

        const multiple = ref(false);
        const enhanceForLongLists = ref(false);
        const displayStyle = computed(() => (multiple.value && !enhanceForLongLists.value) ? PickerDisplayStyle.List : PickerDisplayStyle.Auto);

        return {
            onsubmit,
            allowAdd: ref(false),
            definedTypeGuid: ref(DefinedType.PersonConnectionStatus),
            enhanceForLongLists,
            multiple,
            displayStyle,
            value: ref(null),
            importCode: getControlImportPath("definedValuePicker"),
            exampleCode: `<DefinedValuePicker label="Defined Value" v-model="value" :definedTypeGuid="definedTypeGuid" :multiple="false" :enhanceForLongLists="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <DefinedValuePicker label="Defined Value" v-model="value" :definedTypeGuid="definedTypeGuid" :multiple="multiple" :enhanceForLongLists="enhanceForLongLists" :allowAdd="allowAdd" :displayStyle="displayStyle" />

    <template #settings>
        <div class="row">
            <TextBox formGroupClasses="col-md-4" label="Defined Type" v-model="definedTypeGuid" />
            <CheckBox formGroupClasses="col-md-2" label="Multiple" v-model="multiple" />
            <CheckBox formGroupClasses="col-md-3" label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            <CheckBox formGroupClasses="col-md-3" label="Allow Adding Values" v-model="allowAdd" />
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates entity type picker */
const entityTypePickerGallery = defineComponent({
    name: "EntityTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        EntityTypePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            includeGlobalOption: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({ value: EntityType.Person, text: "Default Person" }),
            importCode: getControlImportPath("entityTypePicker"),
            exampleCode: `<EntityTypePicker label="Entity Type" v-model="value" :multiple="false" :includeGlobalOption="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <EntityTypePicker label="Entity Type"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :includeGlobalOption="includeGlobalOption"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-3">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Include Global Option" v-model="includeGlobalOption" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-3">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-3">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Button</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Achievement type picker */
const achievementTypePickerGallery = defineComponent({
    name: "AchievementTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        AchievementTypePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("achievementTypePicker"),
            exampleCode: `<AchievementTypePicker label="Achievement Type" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <AchievementTypePicker label="Achievement Type"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-3">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-3">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-3">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Badge Component picker */
const badgeComponentPickerGallery = defineComponent({
    name: "BadgeComponentPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        BadgeComponentPicker,
        NumberUpDown,
        EntityTypePicker
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            entityTypeGuid: ref(null),
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("badgeComponentPicker"),
            exampleCode: `<BadgeComponentPicker label="Badge Component" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <BadgeComponentPicker label="Badge Component"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :entityTypeGuid="entityTypeGuid?.value" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-4">
                <EntityTypePicker label="For Entity Type" v-model="entityTypeGuid" enhanceForLongLists showBlankItem />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates the SectionHeader component */
const sectionHeaderGallery = defineComponent({
    name: "SectionHeaderGallery",
    components: {
        GalleryAndResult,
        SectionHeader,
        CheckBox
    },
    setup() {
        const showSeparator = ref(true);
        const showDescription = ref(true);
        const showActionBar = ref(true);
        const showContent = ref(true);

        const description = computed(() => {
            return showDescription.value
                ? "You can use a Section Header to put a title and description above some content."
                : "";
        });

        return {
            showSeparator,
            showDescription,
            showActionBar,
            showContent,
            description,
            importCode: getControlImportPath("sectionHeader"),
            exampleCode: `<SectionHeader title="This is a SectionHeader" description="A Description" :isSeparatorHidden="false">
    <template #actions>Action Buttons</template>
</SectionHeader>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <SectionHeader
        title="This is a SectionHeader"
        :description="description"
        :isSeparatorHidden="!showSeparator" >
        <template v-if="showActionBar" #actions>
            <a class="btn btn-default btn-xs btn-square"><i class="fa fa-lock"></i></a>
            <a class="btn btn-default btn-xs btn-square"><i class="fa fa-pencil"></i></a>
            <a class="btn btn-danger btn-xs btn-square"><i class="fa fa-trash-alt"></i></a>
        </template>
    </SectionHeader>

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-xs-4" v-model="showSeparator" label="Show Separator" />
            <CheckBox formGroupClasses="col-xs-4" v-model="showDescription" label="Show Description" />
            <CheckBox formGroupClasses="col-xs-4" v-model="showActionBar" label="Show Action Bar" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates the SectionContainer component */
const sectionContainerGallery = defineComponent({
    name: "SectionContainerGallery",
    components: {
        GalleryAndResult,
        SectionContainer,
        CheckBox
    },
    setup() {
        const showDescription = ref(true);
        const showActionBar = ref(true);
        const showContentToggle = ref(false);
        const showContent = ref(true);

        const description = computed(() => {
            return showDescription.value
                ? "The Section Container has a Section Header and a collapsible content section below it."
                : "";
        });

        return {
            showDescription,
            showActionBar,
            showContentToggle,
            showContent,
            description,
            importCode: getControlImportPath("sectionContainer"),
            exampleCode: `<SectionContainer title="This is a Section Container" description="A Description" v-model="showContent" toggleText="Show">
    <template #actions>Action Buttons</template>
    Main Content
</SectionContainer>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <SectionContainer
        title="This is a Section Container"
        :description="description"
        v-model="showContent"
        :toggleText="showContentToggle ? 'Show' : ''" >
        <template v-if="showActionBar" #actions>
            <a class="btn btn-default btn-xs btn-square"><i class="fa fa-lock"></i></a>
            <a class="btn btn-default btn-xs btn-square"><i class="fa fa-pencil"></i></a>
            <a class="btn btn-danger btn-xs btn-square"><i class="fa fa-trash-alt"></i></a>
        </template>
        Here's some content to put in here.
    </SectionContainer>

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-xs-4" v-model="showDescription" label="Show Description" />
            <CheckBox formGroupClasses="col-xs-4" v-model="showActionBar" label="Show Action Bar" />
            <CheckBox formGroupClasses="col-xs-4" v-model="showContentToggle" label="Show Content Toggle" />
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates category picker */
const categoryPickerGallery = defineComponent({
    name: "CategoryPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        CategoryPicker,
        TextBox,
        EntityTypePicker
    },
    setup() {
        const entityType = ref<ListItemBag | null>(null);
        const entityTypeGuid = computed(() => {
            if (entityType?.value?.value) {
                return entityType.value.value;
            }

            return null;
        });

        return {
            entityType,
            entityTypeGuid,
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("categoryPicker"),
            exampleCode: `<CategoryPicker label="Category Picker" v-model="value" :multiple="false" :entityTypeGuid="entityTypeGuid" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <CategoryPicker label="Category Picker" v-model="value" :multiple="multiple" :entityTypeGuid="entityTypeGuid" />

    <template #settings>

        <div class="row">
            <div class="col-md-6">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-6">
                <EntityTypePicker label="For Entity Type" v-model="entityType" enhanceForLongLists showBlankItem />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates location item picker */
const locationItemPickerGallery = defineComponent({
    name: "LocationItemPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        LocationItemPicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("locationItemPicker"),
            exampleCode: `<LocationItemPicker label="Location" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <LocationItemPicker label="Location" v-model="value" :multiple="multiple" />

    <template #settings>
        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates connection request picker */
const connectionRequestPickerGallery = defineComponent({
    name: "ConnectionRequestPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        ConnectionRequestPicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("connectionRequestPicker"),
            exampleCode: `<ConnectionRequestPicker label="ConnectionRequest" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <ConnectionRequestPicker label="ConnectionRequest" v-model="value" :multiple="multiple" />

    <template #settings>
        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates copy button */
const copyButtonGallery = defineComponent({
    name: "CopyButtonGallery",
    components: {
        GalleryAndResult,
        TextBox,
        DropDownList,
        CopyButton,
    },
    setup() {
        return {
            tooltip: ref("Copy"),
            value: ref("To God Be The Glory"),
            buttonSize: ref("md"),
            sizeOptions: [
                { value: "lg", text: "Large" },
                { value: "md", text: "Medium" },
                { value: "sm", text: "Small" },
                { value: "xs", text: "Extra Small" },
            ],
            importCode: getControlImportPath("copyButton"),
            exampleCode: `<CopyButton :value="value" tooltip="Copy" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >
    <CopyButton :value="value" :tooltip="tooltip" :btnSize="buttonSize" />

    <template #settings>
        <div class="row">
            <TextBox formGroupClasses="col-sm-4" v-model="value" label="Value to Copy to Clipboard" />
            <TextBox formGroupClasses="col-sm-4" v-model="tooltip" label="Tooltip" />
            <DropDownList formGroupClasses="col-sm-4" label="Button Size" v-model="buttonSize" :items="sizeOptions" />
        </div>

        <p>Additional props extend and are passed to the underlying <code>Rock Button</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates entity tag list */
const tagListGallery = defineComponent({
    name: "TagListGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        RockButton,
        TagList
    },
    setup() {
        const store = useStore();

        return {
            control: ref(null),
            disabled: ref(false),
            delaySave: ref(false),
            showInactive: ref(false),
            disallowNewTags: ref(false),
            entityTypeGuid: EntityType.Person,
            entityKey: store.state.currentPerson?.idKey ?? "",
            btnType: BtnType.Primary,
            importCode: getSfcControlImportPath("tagList"),
            exampleCode: `<TagList :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode">

    <TagList
        :entityTypeGuid="entityTypeGuid"
        :entityKey="entityKey"
        :disabled="disabled"
        :showInactiveTags="showInactive"
        :disallowNewTags="disallowNewTags"
        :delaySave="delaySave"
        ref="control" />

    <template #settings>
        <div class="row">
            <div class="col-md-3">
                <CheckBox label="Disabled" v-model="disabled" help="Makes it read-only. You can't add or remove tags if it's disabled." />
            </div>
            <div class="col-md-3">
                <CheckBox label="Delay Saving Value" v-model="delaySave" help="If checked, creating new tags, adding tags and removing tags is not saved to the server until the component's <code>saveTagValues</code> method is called." />
                <RockButton v-if="delaySave" :btnType="btnType" type="button" @click="control.saveTagValues()"><i class="fa fa-save" /> Save Values</RockButton>
            </div>
            <div class="col-md-3">
                <CheckBox label="Disallow New Tags" v-model="disallowNewTags" help="If checked, no new tags can be created, though you can still add existing tags" />
            </div>
            <div class="col-md-3">
                <CheckBox label="Show Inactive Tags" v-model="showInactive" />
            </div>
        </div>
        <p>
            This control takes multiple props for filtering the tags to show and giving specifiers about what it tags. Below is a list of those props:
        </p>
        <table class="table" style="max-width:450px;">
            <tr>
                <th scope="col">Prop</th>
                <th scope="col">Type</th>
                <th scope="col" class="text-center">Required</th>
            </tr>
            <tr>
                <th scope="row"><code>entityTypeGuid</code></th>
                <td>GUID String</td>
                <td class="text-center"><i class="fa fa-check text-success"></i></td>
            </tr>
            <tr>
                <th scope="row"><code>entityKey</code></th>
                <td>String</td>
                <td class="text-center"><i class="fa fa-check text-success"></i></td>
            </tr>
            <tr>
                <th scope="row"><code>categoryGuid</code></th>
                <td>GUID String</td>
                <td class="text-center"><i class="fa fa-ban text-danger"></i></td>
            </tr>
            <tr>
                <th scope="row"><code>entityQualifierColumn</code></th>
                <td>String</td>
                <td class="text-center"><i class="fa fa-ban text-danger"></i></td>
            </tr>
            <tr>
                <th scope="row"><code>entityQualifierValue</code></th>
                <td>String</td>
                <td class="text-center"><i class="fa fa-ban text-danger"></i></td>
            </tr>
        </table>
    </template>
</GalleryAndResult>`
});

/** Demonstrates following control. */
const followingGallery = defineComponent({
    name: "FollowingGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        Following,
        TextBox
    },
    setup() {
        const store = useStore();

        return {
            disabled: ref(false),
            entityTypeGuid: ref(EntityType.Person),
            entityKey: ref(store.state.currentPerson?.idKey ?? ""),
            importCode: getControlImportPath("following"),
            exampleCode: `<Following :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode">
    <Following :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" :disabled="disabled" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Disabled" v-model="disabled" />
            </div>

            <div class="col-md-4">
                <TextBox label="Entity Type Guid" v-model="entityTypeGuid" />
            </div>

            <div class="col-md-4">
                <TextBox label="Entity Key" v-model="entityKey" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates assessment type picker */
const assessmentTypePickerGallery = defineComponent({
    name: "AssessmentTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        AssessmentTypePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            isInactiveIncluded: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({ value: AssessmentType.Disc, text: "DISC" }),
            importCode: getControlImportPath("assessmentTypePicker"),
            exampleCode: `<AssessmentTypePicker label="Assessment Type" v-model="value" :isInactiveIncluded="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <AssessmentTypePicker label="Assessment Type"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :isInactiveIncluded="isInactiveIncluded"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Include Inactive" v-model="isInactiveIncluded" help="When set, inactive assessments will be included in the list." />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Button</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates Asset Storage Provider picker */
const assetStorageProviderPickerGallery = defineComponent({
    name: "AssetStorageProviderPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        AssetStorageProviderPicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref(null),
            importCode: getControlImportPath("assetStorageProviderPicker"),
            exampleCode: `<AssetStorageProviderPicker label="Asset Storage Provider" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <AssetStorageProviderPicker label="Asset Storage Provider"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates Binary File type picker */
const binaryFileTypePickerGallery = defineComponent({
    name: "BinaryFileTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        BinaryFileTypePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("binaryFileTypePicker"),
            exampleCode: `<BinaryFileTypePicker label="Binary File Type" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <BinaryFileTypePicker label="Binary File Type"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});


/** Demonstrates Campus picker */
const campusPickerGallery = defineComponent({
    name: "CampusPickerGallery",
    components: {
        GalleryAndResult,
        CampusPicker,
        CheckBox,
        DefinedValuePicker,
        DropDownList,
        NumberUpDown,
        TextBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(true),
            value: ref({}),
            forceVisible: ref(false),
            includeInactive: ref(false),
            campusStatusFilter: ref(null),
            campusTypeFilter: ref(null),
            campusStatusDefinedTypeGuid: DefinedType.CampusStatus,
            campusTypeDefinedTypeGuid: DefinedType.CampusType,
            importCode: getControlImportPath("campusPicker"),
            exampleCode: `<CampusPicker label="Campus" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <CampusPicker label="Campus"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :forceVisible="forceVisible"
        :includeInactive="includeInactive"
        :campusStatusFilter="campusStatusFilter?.value"
        :campusTypeFilter="campusTypeFilter?.value" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Force Visible" v-model="forceVisible" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Include Inactive" v-model="includeInactive" />
            </div>

            <div class="col-md-4">
                <DefinedValuePicker label="Campus Type Filter" v-model="campusTypeFilter" :definedTypeGuid="campusTypeDefinedTypeGuid" showBlankItem />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DefinedValuePicker label="Campus Status Filter" v-model="campusStatusFilter" :definedTypeGuid="campusStatusDefinedTypeGuid" showBlankItem />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});


/** Demonstrates Schedule Builder */
const scheduleBuilderGallery = defineComponent({
    name: "ScheduleBuilderGallery",
    components: {
        GalleryAndResult,
        ScheduleBuilder
    },
    setup() {
        return {
            value: ref(""),
            importCode: getControlImportPath("scheduleBuilder"),
            exampleCode: `<ScheduleBuilder label="Schedule Builder" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection
    displayAsRaw>
    <ScheduleBuilder label="Schedule Builder"
        v-model="value" />

    <template #settings>
    </template>
</GalleryAndResult>`
});


/** Demonstrates BinaryFile  picker */
const binaryFilePickerGallery = defineComponent({
    name: "BinaryFilePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        BinaryFilePicker,
        BinaryFileTypePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            binaryFileType: ref({
                "value": BinaryFiletype.Default
            }),
            value: ref({}),
            importCode: getControlImportPath("binaryFilePicker"),
            exampleCode: `<BinaryFilePicker label="Binary File" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <BinaryFilePicker label="Binary File"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :binaryFileTypeGuid="binaryFileType.value" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>

            <div class="col-md-4">
                <BinaryFileTypePicker label="Binary File Type" v-model="binaryFileType" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Event Item picker */
const eventItemPickerGallery = defineComponent({
    name: "EventItemPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        EventItemPicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            includeInactive: ref(false),
            value: ref({}),
            importCode: getControlImportPath("eventItemPicker"),
            exampleCode: `<EventItemPicker label="Event Item" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <EventItemPicker label="Event Item"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :includeInactive="includeInactive" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Include Inactive Items" v-model="includeInactive" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});


/** Demonstrates data views picker */
const dataViewPickerGallery = defineComponent({
    name: "DataViewPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        DataViewPicker,
        NumberUpDown,
        EntityTypePicker
    },
    setup() {
        return {
            entityTypeGuid: ref(null),
            multiple: ref(false),
            value: ref(null),
            displayPersistedOnly: ref(false),
            importCode: getControlImportPath("dataViewPicker"),
            exampleCode: `<DataViewPicker label="Data View" v-model="value" :displayOnlyPersisted="true"/>`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <DataViewPicker label="Data Views"
        v-model="value"
        :multiple="multiple"
        :displayPersistedOnly="displayPersistedOnly"
        :showBlankItem="showBlankItem"
        :entityTypeGuid="entityTypeGuid?.value" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <EntityTypePicker label="For Entity Type" v-model="entityTypeGuid" enhanceForLongLists showBlankItem />
            </div>
            <div class="col-md-4">
                <CheckBox label="Display Only Persisted" v-model="displayPersistedOnly" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});


/** Demonstrates workflow type picker */
const workflowTypePickerGallery = defineComponent({
    name: "WorkflowTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        WorkflowTypePicker,
        NumberUpDown,
        EntityTypePicker
    },
    setup() {
        return {
            includeInactiveItems: ref(false),
            multiple: ref(false),
            value: ref(null),
            importCode: getSfcControlImportPath("workflowTypePicker"),
            exampleCode: `<WorkflowTypePicker label="Data View" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <WorkflowTypePicker label="Data Views"
        v-model="value"
        :multiple="multiple"
        :showBlankItem="showBlankItem"
        :includeInactiveItems="includeInactiveItems" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Include Inactive Items" v-model="includeInactiveItems" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Financial Gateway picker */
const financialGatewayPickerGallery = defineComponent({
    name: "FinancialGatewayPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        FinancialGatewayPicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            includeInactive: ref(false),
            showAllGatewayComponents: ref(false),
            value: ref({}),
            importCode: getControlImportPath("financialGatewayPicker"),
            exampleCode: `<FinancialGatewayPicker label="Financial Gateway" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <FinancialGatewayPicker label="Financial Gateway"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :includeInactive="includeInactive"
        :showAllGatewayComponents="showAllGatewayComponents" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>

            <div class="col-md-2">
                <CheckBox label="Show Inactive Gateways" v-model="includeInactive" />
            </div>

            <div class="col-md-2">
                <CheckBox label="Show All Gateway Components" v-model="showAllGatewayComponents" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Financial Statement Template picker */
const financialStatementTemplatePickerGallery = defineComponent({
    name: "FinancialStatementTemplatePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        FinancialStatementTemplatePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("financialStatementTemplatePicker"),
            exampleCode: `<FinancialStatementTemplatePicker label="Financial Statement Template" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <FinancialStatementTemplatePicker label="Financial Statement Template"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Field Type picker */
const fieldTypePickerGallery = defineComponent({
    name: "FieldTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        FieldTypePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("fieldTypePicker"),
            exampleCode: `<FieldTypePicker label="Field Type" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <FieldTypePicker label="Field Type"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates audit detail. */
const auditDetailGallery = defineComponent({
    name: "AuditDetailGallery",
    components: {
        GalleryAndResult,
        AuditDetail,
        TextBox
    },
    setup() {
        const store = useStore();

        return {
            entityTypeGuid: ref(EntityType.Person),
            entityKey: ref(store.state.currentPerson?.idKey ?? ""),
            importCode: getControlImportPath("auditDetail"),
            exampleCode: `<AuditDetail :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode">
    <AuditDetail :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <TextBox label="Entity Type Guid" v-model="entityTypeGuid" />
            </div>

            <div class="col-md-4">
                <TextBox label="Entity Key" v-model="entityKey" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates modal. */
const modalGallery = defineComponent({
    name: "ModalGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        Modal,
        TextBox
    },
    setup() {
        return {
            isOpen: ref(false),
            saveText: ref<string>("Save"),
            cancelText: ref<string>("Cancel"),
            isFooterHidden: ref(false),
            isSaveButtonDisabled: ref(false),
            isCloseButtonHidden: ref(false),
            clickBackdropToClose: ref(false),
            value: "",
            importCode: getControlImportPath("modal"),
            exampleCode: `<Modal v-model="isOpen" title="Modal Dialog Title" saveText="Save" @save="isOpen = false">
    <TextBox label="Required Value" v-model="value" rules="required" />
</Modal>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode">
    <CheckBox label="Is Open" v-model="isOpen" />

    <Modal v-model="isOpen"
           title="Modal Dialog Title"
           :saveText="saveText"
           :cancelText="cancelText"
           :isFooterHidden="isFooterHidden"
           :isSaveButtonDisabled="isSaveButtonDisabled"
           :isCloseButtonHidden="isCloseButtonHidden"
           :clickBackdropToClose="clickBackdropToClose"
           @save="isOpen = false">
        <TextBox label="Required Value" v-model="value" rules="required" />
    </Modal>

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <TextBox label="Save Text" v-model="saveText" help="If an empty string is provided, the Save button will be hidden." />
            </div>
            <div class="col-md-4">
                <TextBox label="Cancel Text" v-model="cancelText" help="If an empty string is provided, the Cancel button will be hidden." />
            </div>
            <div class="col-md-4">
                <CheckBox label="Is Footer Hidden" v-model="isFooterHidden" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Is Save Button Disabled" v-model="isSaveButtonDisabled" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Is Close Button Hidden" v-model="isCloseButtonHidden" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Click Backdrop to Close" v-model="clickBackdropToClose" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates  Component picker */
const componentPickerGallery = defineComponent({
    name: "ComponentPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        ComponentPicker,
        NumberUpDown,
        TextBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            containerType: ref("Rock.Badge.BadgeContainer"),
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("componentPicker"),
            exampleCode: `<ComponentPicker label="Component" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <ComponentPicker label="Component"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :containerType="containerType" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-3">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-5">
                <TextBox label="Container Assembly Name" v-model="containerType" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Grade Picker */
const gradePickerGallery = defineComponent({
    name: "GradePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        GradePicker,
        NumberUpDown,
        TextBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            useAbbreviation: ref(false),
            useGuidAsValue: ref(false),
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("gradePicker"),
            exampleCode: `<GradePicker label="Grade" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <GradePicker label="Grade"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :useAbbreviation="useAbbreviation"
        :useGuidAsValue="useGuidAsValue" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-3">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-3">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-3">
                <CheckBox label="Use Abbreviations" v-model="useAbbreviation" />
            </div>
            <div class="col-md-3">
                <CheckBox label="Use GUID Value" v-model="useGuidAsValue" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Group Member Picker */
const groupMemberPickerGallery = defineComponent({
    name: "GroupMemberPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        GroupMemberPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            groupGuid: ref("62DC3753-01D5-48B5-B22D-D2825D92900B"), // use a groupPicker eventually...
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("groupMemberPicker"),
            exampleCode: `<GroupMemberPicker label="Group Member" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <GroupMemberPicker label="Group Member"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :groupGuid="groupGuid" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-4">
                <NumberBox label="Group ID" v-model="groupGuid" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Interaction Channel Picker */
const interactionChannelPickerGallery = defineComponent({
    name: "InteractionChannelPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        InteractionChannelPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getSfcControlImportPath("interactionChannelPicker"),
            exampleCode: `<InteractionChannelPicker label="Interaction Channel" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <InteractionChannelPicker label="Interaction Channel"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Interaction Component Picker */
const interactionComponentPickerGallery = defineComponent({
    name: "InteractionComponentPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        InteractionComponentPicker,
        InteractionChannelPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            interactionChannelGuid: ref(null),
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getSfcControlImportPath("interactionComponentPicker"),
            exampleCode: `<InteractionComponentPicker label="Interaction Component" v-model="value" :interactionChannelGuid="interactionChannelGuid" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <InteractionComponentPicker label="Interaction Component"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :interactionChannelGuid="interactionChannelGuid?.value" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-4">
                <InteractionChannelPicker label="Interaction Channel" v-model="interactionChannelGuid" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Lava Command Picker */
const lavaCommandPickerGallery = defineComponent({
    name: "LavaCommandPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        LavaCommandPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("lavaCommandPicker"),
            exampleCode: `<LavaCommandPicker label="Lava Command" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <LavaCommandPicker label="Lava Command"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Remote Auths Picker */
const remoteAuthsPickerGallery = defineComponent({
    name: "RemoteAuthsPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        RemoteAuthsPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("remoteAuthsPicker"),
            exampleCode: `<RemoteAuthsPicker label="Remote Auths" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <RemoteAuthsPicker label="Remote Auths"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Step Program Picker */
const stepProgramPickerGallery = defineComponent({
    name: "StepProgramPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        StepProgramPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getSfcControlImportPath("stepProgramPicker"),
            exampleCode: `<StepProgramPicker label="Step Program" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <StepProgramPicker label="Step Program"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Step Program Step Type Picker */
const stepProgramStepTypePickerGallery = defineComponent({
    name: "StepProgramStepTypePickerGallery",
    components: {
        GalleryAndResult,
        StepProgramPicker,
        StepProgramStepTypePicker,
        CheckBox,
    },
    setup() {
        return {
            value: ref({}),
            stepProgram: ref({}),
            defaultProgramGuid: ref(""),
            required: ref(false),
            disabled: ref(false),
            importCode: getSfcControlImportPath("stepProgramStepTypePicker"),
            exampleCode: `<StepProgramStepTypePicker label="Step Program > Step Type" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{value,stepProgram}"
    :importCode="importCode"
    :exampleCode="exampleCode"
    hasMultipleValues
    enableReflection >

    <StepProgramStepTypePicker label="Step Program > Step Type"
        v-model="value"
        v-model:stepProgram="stepProgram"
        :defaultStepProgramGuid="defaultProgramGuid?.value"
        :rules="required ? 'required' : ''"
        :disabled="disabled" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <StepProgramPicker label="Default Step Program" v-model="defaultProgramGuid" showBlankItem help="If this defaultStepProgramGuid prop is set, the Step Program selector will not be shown and the Step Types will be based on that Program." />
            </div>
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Disabled" v-model="disabled" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Step Program Step Status Picker */
const stepProgramStepStatusPickerGallery = defineComponent({
    name: "StepProgramStepStatusPickerGallery",
    components: {
        GalleryAndResult,
        StepProgramPicker,
        StepProgramStepStatusPicker,
        CheckBox,
    },
    setup() {
        return {
            value: ref({}),
            stepProgram: ref({}),
            defaultProgramGuid: ref(""),
            required: ref(false),
            disabled: ref(false),
            importCode: getSfcControlImportPath("stepProgramStepStatusPicker"),
            exampleCode: `<StepProgramStepStatusPicker label="Step Program > Step Status" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{value,stepProgram}"
    :importCode="importCode"
    :exampleCode="exampleCode"
    hasMultipleValues
    enableReflection >

    <StepProgramStepStatusPicker label="Step Program > Step Status"
        v-model="value"
        v-model:stepProgram="stepProgram"
        :defaultStepProgramGuid="defaultProgramGuid?.value"
        :rules="required ? 'required' : ''"
        :disabled="disabled" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <StepProgramPicker label="Default Step Program" v-model="defaultProgramGuid" showBlankItem help="If this defaultStepProgramGuid prop is set, the Step Program selector will not be shown and the Step Types will be based on that Program." />
            </div>
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Disabled" v-model="disabled" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Step Status Picker */
const stepStatusPickerGallery = defineComponent({
    name: "StepStatusPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        StepStatusPicker,
        StepProgramPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            stepProgramGuid: ref(null),
            value: ref({}),
            importCode: getSfcControlImportPath("stepStatusPicker"),
            exampleCode: `<StepStatusPicker label="Step Status" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <StepStatusPicker label="Step Status"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :stepProgramGuid="stepProgramGuid?.value" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-4">
                <StepProgramPicker label="Step Program" v-model="stepProgramGuid" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Step Type Picker */
const stepTypePickerGallery = defineComponent({
    name: "StepTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        StepTypePicker,
        StepProgramPicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            stepProgramGuid: ref(null),
            value: ref({}),
            importCode: getSfcControlImportPath("stepTypePicker"),
            exampleCode: `<StepTypePicker label="Step Type" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <StepTypePicker label="Step Type"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem"
        :stepProgramGuid="stepProgramGuid?.value" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-4">
                <StepProgramPicker label="Step Program" v-model="stepProgramGuid" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Streak Type Picker */
const streakTypePickerGallery = defineComponent({
    name: "StreakTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        StreakTypePicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("streakTypePicker"),
            exampleCode: `<StreakTypePicker label="Streak Type" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <StreakTypePicker label="Streak Type"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />
    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Badge Picker */
const badgePickerGallery = defineComponent({
    name: "BadgePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        BadgePicker,
        NumberUpDown,
        TextBox,
        NumberBox
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("badgePicker"),
            exampleCode: `<BadgePicker label="Badge" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <BadgePicker label="Badge"
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates an notification box */
const notificationBoxGallery = defineComponent({
    name: "NotificationBoxGallery",
    components: {
        GalleryAndResult,
        NotificationBox,
        DropDownList,
        CheckBox,
        TextBox
    },
    setup() {
        const options: ListItemBag[] = ["default", "success", "info", "danger", "warning", "primary", "validation"].map(key => ({ text: upperCaseFirstCharacter(key), value: key }));
        return {
            isDismissible: ref(false),
            heading: ref(""),
            details: ref("Here's a place where you can place details that show up when you click \"Show Details\"."),
            onDismiss: () => alert('"dismiss" event fired. Parents are responsible for hiding the component.'),
            options,
            alertType: ref(AlertType.Default),
            importCode: getSfcControlImportPath("notificationBox"),
            exampleCode: `<NotificationBox dismissable alertType="AlertType.Info" @dismiss="onDismiss" heading="Heading Text">
    This is an alert!
    <template #details>
        Here's a place where you can place details that show up when you click "Show Details".
    </template>
</NotificationBox>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <NotificationBox :dismissible="isDismissible" :alertType="alertType" @dismiss="onDismiss" :heading="heading">
        This is an alert!
        <template #details v-if="details">
            {{details}}
        </template>
    </NotificationBox>

    <template #settings>
        <div class="row">
            <div class="col-md-3">
                <DropDownList label="Alert Type" v-model="alertType" :items="options" :showBlankItem="false" />
            </div>
            <div class="col-md-3">
                <TextBox v-model="heading" label="Heading Text" />
            </div>
            <div class="col-md-3">
                <TextBox v-model="details" label="Details Text" />
            </div>
            <div class="col-md-3">
                <CheckBox label="Dismissable" v-model="isDismissible" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a badge list */
const badgeListGallery = defineComponent({
    name: "BadgeListGallery",
    components: {
        GalleryAndResult,
        BadgeList,
        EntityTypePicker,
        TextBox,
        CheckBox,
        BadgePicker
    },
    setup() {
        const entityType = ref({ text: "Person", value: EntityType.Person });
        const entityTypeGuid = computed(() => entityType?.value);

        const badgeTypes = ref<ListItemBag[]>([]);
        const badgeTypeGuids = computed(() => badgeTypes.value.map(b => b.value));

        const store = useStore();

        return {
            entityType,
            entityTypeGuid,
            badgeTypes,
            badgeTypeGuids,
            entityKey: ref(store.state.currentPerson?.idKey ?? ""),
            importCode: getControlImportPath("badgeList"),
            exampleCode: `<BadgeList :entityTypeGuid="entityTypeGuid?.value" :entityKey="entityKey" :badgeTypeGuids="badgeTypeGuids" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <BadgeList :entityTypeGuid="entityTypeGuid?.value" :entityKey="entityKey" :badgeTypeGuids="badgeTypeGuids" />

    <template #settings>
        <div class="row">
            <EntityTypePicker formGroupClasses="col-md-4" label="Entity Type" v-model="entityType" enhanceForLongLists />
            <TextBox formGroupClasses="col-md-4" label="Entity Key" v-model="entityKey" />
            <BadgePicker formGroupClasses="col-md-4" label="Badge Type" v-model="badgeTypes" showBlankItem multiple />
        </div>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a basic time picker */
const basicTimePickerGallery = defineComponent({
    name: "BasicTimePickerGallery",
    components: {
        GalleryAndResult,
        BasicTimePicker,
        CheckBox
    },
    setup() {
        return {
            value: ref({}),
            disabled: ref(false),
            importCode: getControlImportPath("basicTimePicker"),
            exampleCode: `<BasicTimePicker label="Time" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <BasicTimePicker label="Time" v-model="value" :disabled="disabled" />

    <template #settings>
        <div>
            <CheckBox v-model="disabled" label="Disabled" />
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code> and <code>Drop Down List</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates birthday picker */
const birthdayPickerGallery = defineComponent({
    name: "BirthdayPickerGallery",
    components: {
        GalleryAndResult,
        Toggle,
        BirthdayPicker
    },
    setup() {
        return {
            showYear: ref(true),
            datePartsModel: ref<Partial<DatePartsPickerValue>>({
                month: 1,
                day: 1,
                year: 1970
            }),
            importCode: getControlImportPath("birthdayPicker"),
            exampleCode: `<BirthdayPicker label="Birthday" v-model="date" />`
        };
    },
    template: `
<GalleryAndResult
    :value="datePartsModel"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection>

    <BirthdayPicker label="Birthday" v-model="datePartsModel" :showYear="showYear" />

    <template #settings>
        <Toggle label="Show Year" v-model="showYear" />
        <p class="mt-4 mb-4">This simply wraps the <a href="#DatePartsPickerGallery">Date Parts Picker</a> and sets <code>allowFutureDates</code> and <code>requireYear</code> to <code>false</code>.</p>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates countdown timer */
const countdownTimerGallery = defineComponent({
    name: "CountdownTimerGallery",
    components: {
        GalleryAndResult,
        CountdownTimer,
        TextBox,
        RockButton
    },
    setup() {
        const seconds = ref(300);
        const setToSeconds = ref(300);

        return {
            reset: () => seconds.value = setToSeconds.value,
            setToSeconds,
            seconds,
            importCode: getControlImportPath("countdownTimer"),
            exampleCode: `<CountdownTimer v-model="seconds" />`
        };
    },
    template: `
<GalleryAndResult
    :value="seconds"
    :importCode="importCode"
    :exampleCode="exampleCode">

    Counting down:
    <CountdownTimer v-model="seconds" />

    <template #settings>
        <form class="form-inline" @submit.prevent="reset">
            <TextBox label="Reset Timer to (seconds)" v-model="setToSeconds" />
            <RockButton type="submit">Set Timer</RockButton>
        </form>
    </template>
</GalleryAndResult>`
});

/** Demonstrates electronic signature */
const electronicSignatureGallery = defineComponent({
    name: "ElectronicSignatureGallery",
    components: {
        GalleryAndResult,
        ElectronicSignature,
        Toggle,
        TextBox
    },
    setup() {
        return {
            signature: ref(null),
            isDrawn: ref(false),
            term: ref("document"),
            importCode: getControlImportPath("electronicSignature"),
            exampleCode: `<ElectronicSignature v-model="signature" :isDrawn="isDrawn" documentTerm="document" />`
        };
    },
    template: `
<GalleryAndResult
    :value="signature"
    :importCode="importCode"
    :exampleCode="exampleCode">

    <ElectronicSignature v-model="signature" :isDrawn="isDrawn" :documentTerm="term" />

    <template #settings>
        <div class="row">
            <Toggle formGroupClasses="col-md-4" label="Signature Type" trueText="Drawn" falseText="Typed" v-model="isDrawn" />
            <TextBox formGroupClasses="col-md-4" label="Document Type Term" v-model="term" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates field type editor */
const fieldTypeEditorGallery = defineComponent({
    name: "FieldTypeEditorGallery",
    components: {
        GalleryAndResult,
        FieldTypeEditor,
        CheckBox,
        TextBox
    },
    setup() {
        return {
            value: ref<FieldTypeEditorUpdateAttributeConfigurationOptionsBag>({
                configurationValues: {
                    truetext: "Yup",
                    falsetext: "Nah",
                    BooleanControlType: "2"
                },
                defaultValue: "True",
                fieldTypeGuid: FieldType.Boolean
            }),
            readOnly: ref(false),
            importCode: getControlImportPath("fieldTypeEditor"),
            exampleCode: `<FieldTypeEditor v-model="value" :isFieldTypeReadOnly="readOnly" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <FieldTypeEditor v-model="value" :isFieldTypeReadOnly="readOnly" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-4" label="Read Only Field Type" v-model="readOnly" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates inline range slider */
const inlineRangeSliderGallery = defineComponent({
    name: "InlineRangeSliderGallery",
    components: {
        GalleryAndResult,
        InlineRangeSlider,
        CheckBox,
        NumberBox
    },
    setup() {
        return {
            value: ref(10),
            step: ref(0),
            min: ref(0),
            max: ref(100),
            showValue: ref(false),
            importCode: getSfcControlImportPath("inlineRangeSlider"),
            exampleCode: `<InlineRangeSlider v-model="value" :step="1" :min="min" :max="max" :showValueBar="showValue" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <InlineRangeSlider v-model="value" :step="step" :min="min" :max="max" :showValueBar="showValue" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Show Value" v-model="showValue" />
            <NumberBox formGroupClasses="col-md-3" label="Step Value" v-model="step" help="Leave blank or set to zero to have no step" />
            <NumberBox formGroupClasses="col-md-3" label="Minimum Value" v-model="min" />
            <NumberBox formGroupClasses="col-md-3" label="Maximum Value" v-model="max" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates javascript anchor */
const javaScriptAnchorGallery = defineComponent({
    name: "JavascriptAnchorGallery",
    components: {
        GalleryAndResult,
        JavaScriptAnchor,
        CheckBox,
        NumberBox
    },
    setup() {
        return {
            onClick: () => alert("Link Clicked"),
            importCode: getControlImportPath("javaScriptAnchor"),
            exampleCode: `<JavaScriptAnchor @click="onClick">Link Text</JavaScriptAnchor>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <JavaScriptAnchor @click="onClick">This link can run code, but does not link to a page.</JavaScriptAnchor>
</GalleryAndResult>`
});

/** Demonstrates javascript anchor */
const keyValueListGallery = defineComponent({
    name: "KeyValueListGallery",
    components: {
        GalleryAndResult,
        KeyValueList,
        CheckBox,
        TextBox
    },
    setup() {
        const limitValues = ref(false);
        const displayValueFirst = ref(false);
        const options: ListItemBag[] = [
            {
                text: "Option 1",
                value: "1"
            },
            {
                text: "Option 2",
                value: "2"
            },
            {
                text: "Option 3",
                value: "3"
            },
        ];

        const valueOptions = computed(() => limitValues.value ? options : null);

        return {
            limitValues,
            displayValueFirst,
            valueOptions,
            value: ref(null),
            fullWidth: ref(false),
            keyPlaceholder: ref("Key"),
            valuePlaceholder: ref("Value"),
            importCode: getControlImportPath("keyValueList"),
            exampleCode: `<KeyValueList label="Keys and Values" v-model="value" :valueOptions="valueOptions" :displayValueFirst="displayValueFirst" :keyPlaceholder="keyPlaceholder" :valuePlaceholder="valuePlaceholder" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{ 'output:modelValue':value, 'input:valueOptions':valueOptions }"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <KeyValueList label="Keys and Values" v-model="value" :valueOptions="valueOptions" :displayValueFirst="displayValueFirst" :keyPlaceholder="keyPlaceholder" :valuePlaceholder="valuePlaceholder" :fullWidth="fullWidth" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-4" label="Limit Possible Values" v-model="limitValues" />
            <CheckBox formGroupClasses="col-md-4" label="Show Value First" v-model="displayValueFirst" />
            <CheckBox formGroupClasses="col-md-4" label="Full Width" v-model="fullWidth" />
        </div>
        <div class="row">
            <TextBox formGroupClasses="col-md-4" label="Placeholder for Key Field" v-model="keyPlaceholder" />
            <TextBox formGroupClasses="col-md-4" label="Placeholder for Value Field" v-model="valuePlaceholder" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates loading component */
const loadingGallery = defineComponent({
    name: "LoadingGallery",
    components: {
        GalleryAndResult,
        Loading,
        CheckBox
    },
    setup() {
        return {
            isLoading: ref(false),
            importCode: getControlImportPath("loading"),
            exampleCode: `<Loading :isLoading="isLoading">Content to show when not loading</Loading>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <Loading :isLoading="isLoading">Check the box below to start loading</Loading>

    <template #settings>
        <div class="row mb-3">
            <CheckBox formGroupClasses="col-md-3" label="Is Loading" v-model="isLoading" />
        </div>
        <p>Internally, this uses the <a href="#LoadingIndicatorGallery">LoadingIndicator</a> component.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates loading indicator component */
const loadingIndicatorGallery = defineComponent({
    name: "LoadingIndicatorGallery",
    components: {
        GalleryAndResult,
        LoadingIndicator
    },
    setup() {
        return {
            importCode: getControlImportPath("loadingIndicator"),
            exampleCode: `<LoadingIndicator />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <LoadingIndicator />

    <template #settings>
        <p>It's best to use the <a href="#LoadingGallery">Loading</a> component instead of using this one directly.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates number up down group */
const numberUpDownGroupGallery = defineComponent({
    name: "NumberUpDownGroupGallery",
    components: {
        GalleryAndResult,
        NumberUpDownGroup,
        CheckBox,
        NumberBox
    },
    setup() {
        return {
            value: ref({ prop1: 30, prop2: 30, prop3: 30 }),
            options: [
                { key: "prop1", label: "Prop 1", min: 0, max: 50 },
                { key: "prop2", label: "Prop 2", min: 10, max: 60 },
                { key: "prop3", label: "Prop 3", min: 20, max: 70 }
            ] as NumberUpDownGroupOption[],
            importCode: getControlImportPath("numberUpDownGroup"),
            exampleCode: `<NumberUpDownGroup v-model="value" :options="options" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{ 'output:modelValue':value, 'input:options':options }"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <NumberUpDownGroup v-model="value" :options="options" />
</GalleryAndResult>`
});

/** Demonstrates progress bar */
const progressBarGallery = defineComponent({
    name: "ProgressBarGallery",
    components: {
        GalleryAndResult,
        ProgressBar,
        RangeSlider
    },
    setup() {
        return {
            value: ref(10),
            importCode: getControlImportPath("progressBar"),
            exampleCode: `<ProgressBar :percent="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <ProgressBar :percent="value" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <RangeSlider label="Percent Done" v-model="value" showValueBar :step="1" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Rock Button */
const rockButtonGallery = defineComponent({
    name: "RockButtonGallery",
    components: {
        GalleryAndResult,
        RockButton,
        DropDownList,
        CheckBox,
        TextBox
    },
    setup() {
        const sizeOptions: ListItemBag[] = [
            { text: "Default", value: BtnSize.Default },
            { text: "ExtraSmall", value: BtnSize.ExtraSmall },
            { text: "Small", value: BtnSize.Small },
            { text: "Large", value: BtnSize.Large }
        ];

        const typeOptions: ListItemBag[] = [
            { text: "Default", value: BtnType.Default },
            { text: "Primary", value: BtnType.Primary },
            { text: "Danger", value: BtnType.Danger },
            { text: "Warning", value: BtnType.Warning },
            { text: "Success", value: BtnType.Success },
            { text: "Info", value: BtnType.Info },
            { text: "Link", value: BtnType.Link },
        ];

        return {
            sizeOptions,
            typeOptions,
            btnSize: ref(BtnSize.Default),
            btnType: ref(BtnType.Default),
            onClick: () => new Promise((res) => setTimeout(() => {
                res(true); alert("done");
            }, 3000)),
            autoLoading: ref(false),
            autoDisable: ref(false),
            isLoading: ref(false),
            isSquare: ref(false),
            loadingText: ref("Loading..."),
            importCode: `import RockButton, { BtnType, BtnSize } from "@Obsidian/Controls/rockButton.obs";`,
            exampleCode: `<RockButton
    :btnSize="BtnSize.Default"
    :btnType="BtnType.Default"
    @click="onClick"
    :isLoading="isLoading"
    :autoLoading="autoLoading"
    :autoDisable="autoDisable"
    :loadingText="loadingText">
    Button Text
</RockButton>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RockButton :btnSize="btnSize" :btnType="btnType" @click="onClick" :isLoading="isLoading" :autoLoading="autoLoading" :autoDisable="autoDisable" :loadingText="loadingText" :isSquare="isSquare">
        <i class="fa fa-cross" v-if="isSquare"></i>
        <template v-else>Click Here to Fire Async Operation</template>
    </RockButton>

    <template #settings>
        <div class="row">
            <DropDownList formGroupClasses="col-md-3" label="Button Size" v-model="btnSize" :items="sizeOptions" :showBlankItem="false" />
            <DropDownList formGroupClasses="col-md-3" label="Button Type" v-model="btnType" :items="typeOptions" :showBlankItem="false" />
            <CheckBox formGroupClasses="col-md-3" label="Auto Loading Indicator" v-model="autoLoading" />
            <CheckBox formGroupClasses="col-md-3" label="Auto Disable" v-model="autoDisable" />
        </div>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Force Loading" v-model="isLoading" />
            <CheckBox formGroupClasses="col-md-3" label="Square" v-model="isSquare" />
            <TextBox formGroupClasses="col-md-3" label="Loading Text" v-model="loadingText" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a rock form */
const rockLabelGallery = defineComponent({
    name: "RockLabelGallery",
    components: {
        GalleryAndResult,
        RockLabel
    },
    setup() {
        return {
            importCode: getControlImportPath("rockLabel"),
            exampleCode: `<RockLabel help="More Info">A Label</RockLabel>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <RockLabel help="This is the help message">This is a Rock Label. Hover icon for help.</RockLabel>
</GalleryAndResult>`
});

/** Demonstrates a rock validation */
const rockValidationGallery = defineComponent({
    name: "RockValidationGallery",
    components: {
        GalleryAndResult,
        RockValidation
    },
    setup() {
        return {
            errors: [
                { name: "Error Name", text: "Error text describing the validation error." },
                { name: "Not Good", text: "This is invalid because it is sinful." },
                { name: "Trust God", text: "Didn't trust God. Turn to Him." }
            ],
            importCode: getControlImportPath("rockValidation"),
            exampleCode: `<RockValidation :errors="[{ name:'Error Name', text:'Error Description' }]" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <RockValidation :errors="errors" />

    <template #settings>
        <p>The <code>errors</code> parameter takes an array of <code>FormError</code> objects. <code>FormError</code> type is defined in <code>@Obsidian/Utility/form</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates range slider */
const rangeSliderGallery = defineComponent({
    name: "RangeSliderGallery",
    components: {
        GalleryAndResult,
        RangeSlider,
        CheckBox,
        NumberBox
    },
    setup() {
        return {
            value: ref(10),
            step: ref(1),
            min: ref(0),
            max: ref(100),
            showValue: ref(false),
            importCode: getSfcControlImportPath("slider"),
            exampleCode: `<RangeSlider label="Range Slider" v-model="value" :step="1" :min="min" :max="max" :showValueBar="showValue" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RangeSlider v-model="value" label="Range Slider Value" :step="step" :min="min" :max="max" :showValueBar="showValue" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Show Value" v-model="showValue" />
            <NumberBox formGroupClasses="col-md-3" label="Step Value" v-model="step" help="Set to zero to have no step" />
            <NumberBox formGroupClasses="col-md-3" label="Minimum Value" v-model="min" />
            <NumberBox formGroupClasses="col-md-3" label="Maximum Value" v-model="max" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates tabbed bar */
const tabbedBarGallery = defineComponent({
    name: "TabbedBarGallery",
    components: {
        GalleryAndResult,
        TabbedBar,
        DropDownList
    },
    setup() {
        return {
            list: ["Matthew", "Mark", "Luke", "John", "Acts", "Romans", "1 Corinthians", "2 Corinthians", "Galatians", "Ephesians", "Philippians", "Colossians"],
            selectedTab: ref(""),
            type: ref("tabs"),
            typeItems: [{ value: "tabs", text: "Tabs" }, { value: "pills", text: "Pills" }],
            importCode: getSfcControlImportPath("tabbedBar"),
            exampleCode: `<TabbedBar v-model="selectedTab" :tabs="arrayOfItems" :type="type" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <TabbedBar v-model="selectedTab" :tabs="list" :type="type" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Type" v-model="type" :items="typeItems" :showBlankItem="false" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates tabbed content */
const tabbedContentGallery = defineComponent({
    name: "TabbedContentGallery",
    components: {
        GalleryAndResult,
        TabbedContent,
        CheckBox,
        NumberBox
    },
    setup() {
        return {
            list: ["Matthew", "Mark", "Luke", "John"],
            importCode: getSfcControlImportPath("tabbedContent"),
            exampleCode: `<TabbedContent :tabs="arrayOfItems">
    <template #tabpane="{item}">
        This is the content for {{item}}.
    </template>
</TabbedContent>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <TabbedContent :tabs="list">
        <template #tabpane="{item}">
            This is the content for {{item}}.
        </template>
    </TabbedContent>
</GalleryAndResult>`
});

/** Demonstrates vertical collapse transition */
const transitionVerticalCollapseGallery = defineComponent({
    name: "TransitionVerticalCollapseGallery",
    components: {
        GalleryAndResult,
        TransitionVerticalCollapse,
        RockButton
    },
    setup() {
        return {
            showContent: ref(false),
            importCode: getControlImportPath("transitionVerticalCollapse"),
            exampleCode: `<TransitionVerticalCollapse>
    <div v-if="showContent">Content to transition in</div>
</TransitionVerticalCollapse>`
        };
    },
    template: `
<GalleryAndResult :importCode="importCode" :exampleCode="exampleCode">
    <RockButton btnType="primary" class="mb-3" @click="showContent = !showContent">Show Content</RockButton>
    <TransitionVerticalCollapse>
        <div v-if="showContent">God so loved the world...</div>
    </TransitionVerticalCollapse>
</GalleryAndResult>`
});

/** Demonstrates a value detail list */
const valueDetailListGallery = defineComponent({
    name: "ValueDetailListGallery",
    components: {
        GalleryAndResult,
        ValueDetailList
    },
    setup() {
        return {
            modelValue: [
                { title: "Title", textValue: "A text description of this item." },
                { title: "Something", htmlValue: "This description has <i>some</i> <code>HTML</code> mixed in." }
            ],
            importCode: getControlImportPath("valueDetailList"),
            exampleCode: `<ValueDetailList :modelValue="[{ name:'Error Name', text:'Error Description' }]" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <ValueDetailList :modelValue="modelValue" />

    <template #settings>
        <p>The <code>modelValue</code> parameter takes an array of <code>ValueDetailListItem</code> objects. <code>ValueDetailListItem</code> type is defined in <code>@Obsidian/Types/Controls/valueDetailListItem</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates code editor. */
const codeEditorGallery = defineComponent({
    name: "CodeEditorGallery",
    components: {
        GalleryAndResult,
        CodeEditor,
        DropDownList,
        NumberBox
    },
    setup() {
        const themeItems: ListItemBag[] = [
            { value: "rock", text: "rock" },
            { value: "chrome", text: "chrome" },
            { value: "crimson_editor", text: "crimson_editor" },
            { value: "dawn", text: "dawn" },
            { value: "dreamweaver", text: "dreamweaver" },
            { value: "eclipse", text: "eclipse" },
            { value: "solarized_light", text: "solarized_light" },
            { value: "textmate", text: "textmate" },
            { value: "tomorrow", text: "tomorrow" },
            { value: "xcode", text: "xcode" },
            { value: "github", text: "github" },
            { value: "ambiance", text: "ambiance" },
            { value: "chaos", text: "chaos" },
            { value: "clouds_midnight", text: "clouds_midnight" },
            { value: "cobalt", text: "cobalt" },
            { value: "idle_fingers", text: "idle_fingers" },
            { value: "kr_theme", text: "kr_theme" },
            { value: "merbivore", text: "merbivore" },
            { value: "merbivore_soft", text: "merbivore_soft" },
            { value: "mono_industrial", text: "mono_industrial" },
            { value: "monokai", text: "monokai" },
            { value: "pastel_on_dark", text: "pastel_on_dark" },
            { value: "solarized_dark", text: "solarized_dark" },
            { value: "terminal", text: "terminal" },
            { value: "tomorrow_night", text: "tomorrow_night" },
            { value: "tomorrow_night_blue", text: "tomorrow_night_blue" },
            { value: "tomorrow_night_bright", text: "tomorrow_night_bright" },
            { value: "tomorrow_night_eighties", text: "tomorrow_night_eighties" },
            { value: "twilight", text: "twilight" },
            { value: "vibrant_ink", text: "vibrant_ink" }
        ].sort((a, b) => a.text.localeCompare(b.text));

        const modeItems: ListItemBag[] = [
            { value: "text", text: "text" },
            { value: "css", text: "css" },
            { value: "html", text: "html" },
            { value: "lava", text: "lava" },
            { value: "javascript", text: "javascript" },
            { value: "less", text: "less" },
            { value: "powershell", text: "powershell" },
            { value: "sql", text: "sql" },
            { value: "typescript", text: "typescript" },
            { value: "csharp", text: "csharp" },
            { value: "markdown", text: "markdown" },
            { value: "xml", text: "xml" },
        ].sort((a, b) => a.text.localeCompare(b.text));

        const theme = ref("rock");
        const mode = ref("text");
        const editorHeight = ref(200);

        const exampleCode = computed((): string => {
            return buildExampleCode("CodeEditor", {
                theme,
                mode,
                editorHeight
            });
        });

        return {
            theme,
            themeItems,
            mode,
            modeItems,
            editorHeight,
            importCode: getControlImportPath("codeEditor"),
            exampleCode
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode">
    <CodeEditor :theme="theme" :mode="mode" :editorHeight="editorHeight" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <DropDownList label="Theme" v-model="theme" :items="themeItems" />
            </div>

            <div class="col-md-4">
                <DropDownList label="Mode" v-model="mode" :items="modeItems" />
            </div>

            <div class="col-md-4">
                <NumberBox label="Editor Height" v-model="editorHeight" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});


/** Demonstrates page picker */
const pagePickerGallery = defineComponent({
    name: "PagePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        PagePicker
    },
    setup() {
        return {
            multiple: ref(false),
            showSelectCurrentPage: ref(false),
            promptForPageRoute: ref(false),
            value: ref({
                "page": {
                    value: "b07f30b3-95c4-40a5-9cf6-455399bef67a",
                    text: "Universal Search"
                }
            }),
            importCode: getSfcControlImportPath("pagePicker"),
            exampleCode: `<PagePicker label="Page" v-model="value" :multiple="false" promptForPageRoute showSelectCurrentPage />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <PagePicker label="Page" v-model="value" :multiple="multiple" :promptForPageRoute="promptForPageRoute" :showSelectCurrentPage="showSelectCurrentPage" />

    <template #settings>

        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show 'Select Current Page' Button" v-model="showSelectCurrentPage" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Prompt for Route" v-model="promptForPageRoute" help="Only works if not selecting multiple values" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates group picker */
const groupPickerGallery = defineComponent({
    name: "GroupPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        GroupPicker
    },
    setup() {
        return {
            multiple: ref(false),
            limitToSchedulingEnabled: ref(false),
            limitToRSVPEnabled: ref(false),
            value: ref(null),
            importCode: getControlImportPath("groupPicker"),
            exampleCode: `<GroupPicker label="Group" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <GroupPicker label="Group"
        v-model="value"
        :multiple="multiple"
        :limitToSchedulingEnabled="limitToSchedulingEnabled"
        :limitToRSVPEnabled="limitToRSVPEnabled" />

    <template #settings>

    <div class="row">
        <div class="col-md-4">
            <CheckBox label="Multiple" v-model="multiple" />
        </div>
        <div class="col-md-4">
            <CheckBox label="Limit to Scheduling Enabled" v-model="limitToSchedulingEnabled" />
        </div>
        <div class="col-md-4">
            <CheckBox label="Limit to RSVP Enabled" v-model="limitToRSVPEnabled" />
        </div>
    </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates merge template picker */
const mergeTemplatePickerGallery = defineComponent({
    name: "MergeTemplatePickerGallery",
    components: {
        GalleryAndResult,
        DropDownList,
        CheckBox,
        MergeTemplatePicker
    },
    setup() {
        const ownershipOptions = [
            { text: "Global", value: MergeTemplateOwnership.Global },
            { text: "Personal", value: MergeTemplateOwnership.Personal },
            { text: "Both", value: MergeTemplateOwnership.PersonalAndGlobal },
        ];

        return {
            ownershipOptions,
            ownership: ref(MergeTemplateOwnership.Global),
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("mergeTemplatePicker"),
            exampleCode: `<MergeTemplatePicker label="Merge Template" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <MergeTemplatePicker label="Merge Template"
        v-model="value"
        :multiple="multiple"
        :mergeTemplateOwnership="ownership" />

    <template #settings>

    <div class="row">
        <div class="col-md-4">
            <CheckBox label="Multiple" v-model="multiple" />
        </div>
        <div class="col-md-4">
            <DropDownList label="Ownership" v-model="ownership" :items="ownershipOptions" />
        </div>
    </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates metric category picker */
const metricCategoryPickerGallery = defineComponent({
    name: "MetricCategoryPickerGallery",
    components: {
        GalleryAndResult,
        DropDownList,
        CheckBox,
        MetricCategoryPicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("metricCategoryPicker"),
            exampleCode: `<MetricCategoryPicker label="Metric Category" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <MetricCategoryPicker label="Metric Category"
        v-model="value"
        :multiple="multiple" />

    <template #settings>
        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates metric item picker */
const metricItemPickerGallery = defineComponent({
    name: "MetricItemPickerGallery",
    components: {
        GalleryAndResult,
        DropDownList,
        CheckBox,
        MetricItemPicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("metricItemPicker"),
            exampleCode: `<MetricItemPicker label="Metric Item" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <MetricItemPicker label="Metric Item"
        v-model="value"
        :multiple="multiple" />

    <template #settings>
        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates registration template picker */
const registrationTemplatePickerGallery = defineComponent({
    name: "RegistrationTemplatePickerGallery",
    components: {
        GalleryAndResult,
        DropDownList,
        CheckBox,
        RegistrationTemplatePicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("registrationTemplatePicker"),
            exampleCode: `<RegistrationTemplatePicker label="Registration Template" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RegistrationTemplatePicker label="Registration Template"
        v-model="value"
        :multiple="multiple"
        :mergeTemplateOwnership="ownership" />

    <template #settings>

        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates report picker */
const reportPickerGallery = defineComponent({
    name: "ReportPickerGallery",
    components: {
        GalleryAndResult,
        DropDownList,
        CheckBox,
        ReportPicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("reportPicker"),
            exampleCode: `<ReportPicker label="Report" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <ReportPicker label="Report"
        v-model="value"
        :multiple="multiple" />

    <template #settings>

        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates schedule picker */
const schedulePickerGallery = defineComponent({
    name: "SchedulePickerGallery",
    components: {
        GalleryAndResult,
        DropDownList,
        CheckBox,
        SchedulePicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("schedulePicker"),
            exampleCode: `<SchedulePicker label="Schedule" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <SchedulePicker label="Schedule"
        v-model="value"
        :multiple="multiple" />

    <template #settings>

        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates workflow action type picker */
const workflowActionTypePickerGallery = defineComponent({
    name: "WorkflowActionTypePickerGallery",
    components: {
        GalleryAndResult,
        DropDownList,
        CheckBox,
        WorkflowActionTypePicker
    },
    setup() {
        return {
            multiple: ref(false),
            value: ref(null),
            importCode: getSfcControlImportPath("workflowActionTypePicker"),
            exampleCode: `<WorkflowActionTypePicker label="Workflow Action Type" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <WorkflowActionTypePicker label="Workflow Action Type"
        v-model="value"
        :multiple="multiple" />

    <template #settings>

        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a day of week picker */
const dayOfWeekPickerGallery = defineComponent({
    name: "DayOfWeekPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        NumberUpDown,
        DayOfWeekPicker
    },
    setup() {
        return {
            showBlankItem: ref(false),
            multiple: ref(false),
            columns: ref(1),
            value: ref(null),
            importCode: getSfcControlImportPath("dayOfWeekPicker"),
            exampleCode: `<DayOfWeekPicker label="Day of the Week" v-model="value" :showBlankItem="false" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <DayOfWeekPicker label="Day of the Week" v-model="value" :showBlankItem="showBlankItem" :multiple="multiple" :repeatColumns="columns" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-4" label="Show Blank Item" v-model="showBlankItem" />
            <CheckBox formGroupClasses="col-sm-4" label="Multiple" v-model="multiple" />
            <NumberUpDown v-if="multiple" formGroupClasses="col-sm-4" label="Columns" v-model="columns" />
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a month/day picker */
const monthDayPickerGallery = defineComponent({
    name: "MonthDayPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        NumberUpDown,
        MonthDayPicker
    },
    setup() {
        return {
            value: ref({ month: 0, day: 0 }),
            importCode: getSfcControlImportPath("monthDayPicker"),
            exampleCode: `<MonthDayPicker label="Month and Day" v-model="value" :showBlankItem="false" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <MonthDayPicker label="Month and Day" v-model="value" :showBlankItem="showBlankItem" :multiple="multiple" :repeatColumns="columns" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a month/year picker */
const monthYearPickerGallery = defineComponent({
    name: "MonthYearPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        NumberUpDown,
        MonthYearPicker
    },
    setup() {
        return {
            value: ref({ month: 0, year: 0 }),
            importCode: getSfcControlImportPath("monthYearPicker"),
            exampleCode: `<MonthYearPicker label="Month and Year" v-model="value" :showBlankItem="false" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <MonthYearPicker label="Month and Year" v-model="value" :showBlankItem="showBlankItem" :multiple="multiple" :repeatColumns="columns" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a cacheability picker */
const cacheabilityPickerGallery = defineComponent({
    name: "CacheabilityPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        NumberUpDown,
        CacheabilityPicker
    },
    setup() {
        return {
            value: ref<RockCacheability | null>(null),
            importCode: getSfcControlImportPath("cacheabilityPicker"),
            exampleCode: `<CacheabilityPicker v-model="value" :showBlankItem="false" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <CacheabilityPicker label="Cacheability" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Button Group */
const buttonGroupGallery = defineComponent({
    name: "ButtonGroupGallery",
    components: {
        GalleryAndResult,
        ButtonGroup,
        DropDownList,
        CheckBox,
        TextBox
    },
    setup() {
        const sizeOptions: ListItemBag[] = [
            { text: "Default", value: BtnSize.Default },
            { text: "ExtraSmall", value: BtnSize.ExtraSmall },
            { text: "Small", value: BtnSize.Small },
            { text: "Large", value: BtnSize.Large }
        ];

        const typeOptions: ListItemBag[] = [
            { text: "Default", value: BtnType.Default },
            { text: "Primary", value: BtnType.Primary },
            { text: "Danger", value: BtnType.Danger },
            { text: "Warning", value: BtnType.Warning },
            { text: "Success", value: BtnType.Success },
            { text: "Info", value: BtnType.Info },
            { text: "Link", value: BtnType.Link },
        ];

        const buttonOptions: ListItemBag[] = [
            { text: "Mins", value: "1" },
            { text: "Hours", value: "2" },
            { text: "Days", value: "3" },
        ];

        return {
            sizeOptions,
            typeOptions,
            buttonOptions,
            btnSize: ref(BtnSize.Default),
            sbtnType: ref(BtnType.Primary),
            ubtnType: ref(BtnType.Default),
            value: ref("1"),
            importCode: getSfcControlImportPath("buttonGroup"),
            exampleCode: `<ButtonGroup :btnSize="BtnSize.Default" :btnType="BtnType.Default" :items="items" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <ButtonGroup v-model="value" :btnSize="btnSize" :selectedBtnType="sbtnType" :unselectedBtnType="ubtnType" :items="buttonOptions" />

    <template #settings>
        <div class="row">
            <DropDownList formGroupClasses="col-md-4" label="Button Size" v-model="btnSize" :items="sizeOptions" :showBlankItem="false" />
            <DropDownList formGroupClasses="col-md-4" label="Selected Button Type" v-model="sbtnType" :items="typeOptions" :showBlankItem="false" />
            <DropDownList formGroupClasses="col-md-4" label="Unselected Button Type" v-model="ubtnType" :items="typeOptions" :showBlankItem="false" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Interval Picker */
const intervalPickerGallery = defineComponent({
    name: "IntervalPickerGallery",
    components: {
        GalleryAndResult,
        IntervalPicker,
        DropDownList,
        CheckBox,
        TextBox
    },
    setup() {
        const typeOptions: ListItemBag[] = [
            { text: "Default", value: BtnType.Default },
            { text: "Primary", value: BtnType.Primary },
            { text: "Danger", value: BtnType.Danger },
            { text: "Warning", value: BtnType.Warning },
            { text: "Success", value: BtnType.Success },
            { text: "Info", value: BtnType.Info },
            { text: "Link", value: BtnType.Link },
        ];

        return {
            typeOptions,
            sbtnType: ref(BtnType.Primary),
            ubtnType: ref(BtnType.Default),
            value: ref(null),
            importCode: getSfcControlImportPath("intervalPicker"),
            exampleCode: `<IntervalPicker v-model="value" label="Interval" :selectedBtnType="sbtnType" :unselectedBtnType="ubtnType" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <IntervalPicker v-model="value" label="Interval" :selectedBtnType="sbtnType" :unselectedBtnType="ubtnType" />

    <template #settings>
        <div class="row">
            <DropDownList formGroupClasses="col-md-4" label="Selected Button Type" v-model="sbtnType" :items="typeOptions" :showBlankItem="false" />
            <DropDownList formGroupClasses="col-md-4" label="Unselected Button Type" v-model="ubtnType" :items="typeOptions" :showBlankItem="false" />
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Geo Picker */
const geoPickerGallery = defineComponent({
    name: "GeoPickerGallery",
    components: {
        GalleryAndResult,
        GeoPicker,
        Toggle
    },
    setup() {
        const toggleValue = ref(false);
        const drawingMode = computed(() => toggleValue.value ? "Point" : "Polygon");

        return {
            value: ref("POLYGON((35.1945 31.813, 35.2345 31.813, 35.2345 31.783, 35.2745 31.783, 35.2745 31.753, 35.2345 31.753, 35.2345 31.693, 35.1945 31.693, 35.1945 31.753, 35.1545 31.753, 35.1545 31.783, 35.1945 31.783, 35.1945 31.813))"),
            toggleValue,
            drawingMode,
            importCode: getSfcControlImportPath("geoPicker"),
            exampleCode: `<GeoPicker :drawingMode="drawingMode" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <GeoPicker label="Geo Picker" :drawingMode="drawingMode" v-model="value" />

    <template #settings>
        <div class="row">
            <Toggle formGroupClasses="col-md-3" v-model="toggleValue" label="Drawing Mode" trueText="Point" falseText="Polygon" help="This will not update while the picker is open. Re-open picker to see change. You may also need to clear the value" />
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Content Drop Down Picker */
const contentDropDownPickerGallery = defineComponent({
    name: "ContentDropDownPickerGallery",
    components: {
        GalleryAndResult,
        ContentDropDownPicker,
        InlineCheckBox,
        TextBox
    },
    setup() {
        const value = ref<string>("");
        const innerLabel = computed<string>(() => value.value || "No Value Selected");
        const showPopup = ref(false);
        const isFullscreen = ref(false);

        function onSelect(): void {
            value.value = "A Value";
        }
        function onClear(): void {
            value.value = "";
        }

        return {
            value,
            innerLabel,
            onSelect,
            onClear,
            primaryButtonLabel: ref("<i class='fa fa-save'></i> Save"),
            secondaryButtonLabel: ref("Close"),
            showPopup,
            isFullscreen,
            showClearButton: ref(false),
            importCode: getSfcControlImportPath("contentDropDownPicker"),
            exampleCode: `<ContentDropDownPicker
    label="Your Custom Picker"
    @primaryButtonClicked="selectValue"
    @clearButtonClicked="clear"S
    :innerLabel="innerLabel"
    :showClear="!!value"
    iconCssClass="fa fa-cross" >
    You can place anything you want in here. Click the Save button to select a value or Cancel to close this box.
</ContentDropDownPicker>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <ContentDropDownPicker
        label="Your Custom Picker"
        @primaryButtonClicked="onSelect"
        @clearButtonClicked="onClear"
        v-model:showPopup="showPopup"
        v-model:isFullscreen="isFullscreen"
        :innerLabel="innerLabel"
        :showClear="showClearButton"
        pickerContentBoxHeight="auto"
        disablePickerContentBoxScroll
        iconCssClass="fa fa-cross"
        rules="required"
         >

        <p>You can place anything you want in here. Click the Save button to select a value or Cancel to close this box.
        The actions are completely customizable via event handlers (though they always close the popup), or you can
        completely override them via the <code>mainPickerActions</code> slot. You can also add additional custom buttons
        to the right via the <code>customPickerActions</code> slot.</p>

        <p><strong>Note</strong>: you are in control of:</p>

        <ul>
            <li>Selecting a value when the primary button is clicked. This control does not touch actual values at all
            except to pass them to <code>&lt;RockFormField&gt;</code> for validation.</li>
            <li>Determining the text inside the select box via the <code>innerLabel</code> prop, since this control does
            not look at the values or know how to format them</li>
            <li>Determining when the clear button should show up via the <code>showClear</code> prop, once again because
            this control doesn't mess with selected values.</li>
        </ul>

        <template #primaryButtonLabel><span v-html="primaryButtonLabel"></span></template>

        <template #secondaryButtonLabel><span v-html="secondaryButtonLabel"></span></template>


        <template #customPickerActions>
            Custom Actions Here
        </template>
    </ContentDropDownPicker>

    <template #settings>
        <div class="row">
            <TextBox formGroupClasses="col-md-3" label="Primary Button Label" v-model="primaryButtonLabel" />
            <TextBox formGroupClasses="col-md-3" label="Secondary Button Label" v-model="secondaryButtonLabel" />
            <div class="col-md-3"><InlineCheckBox label="Show Popup" v-model="showPopup" /></div>
            <div class="col-md-3"><InlineCheckBox label="Show Clear Button" v-model="showClearButton" /></div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});



/** Demonstrates a wordcloud */
const wordCloudGallery = defineComponent({
    name: "WordCloudGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        NumberBox,
        TextBox,
        WordCloud
    },
    setup() {
        const wordsText = ref("Hello, Hello, Hello, from, from, Chip");
        const colorsText = ref("#0193B9, #F2C852, #1DB82B, #2B515D, #ED3223");

        const words = computed((): string[] => {
            return wordsText.value.split(",").map(v => v.trim()).filter(v => v.length > 0);
        });

        const colors = computed((): string[] => {
            return colorsText.value.split(",").map(v => v.trim()).filter(v => v.length > 0);
        });

        return {
            animationDuration: ref(350),
            angleCount: ref(5),
            autoClear: ref(false),
            colors,
            colorsText,
            fontName: ref("Impact"),
            minimumAngle: ref(-90),
            minimumFontSize: ref(10),
            maximumAngle: ref(90),
            maximumFontSize: ref(96),
            wordPadding: ref(5),
            words,
            wordsText,
            importCode: getControlImportPath("wordCloud"),
            exampleCode: `<WordCloud :words="['Hello', 'Hello', 'Goodbye']" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode">
    <WordCloud width="100%"
        :words="words"
        :animationDuration="animationDuration"
        :angleCount="angleCount"
        :autoClear="autoClear"
        :colors="colors"
        :fontName="fontName"
        :minimumAngle="minimumAngle"
        :minimumFontSize="minimumFontSize"
        :maximumAngle="maximumAngle"
        :maximumFontSize="maximumFontSize"
        :wordPadding="wordPadding" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <TextBox v-model="wordsText" label="Words" />
            </div>

            <div class="col-md-4">
                <TextBox v-model="colorsText" label="Colors" />
            </div>

            <div class="col-md-4">
                <NumberBox v-model="wordPadding" label="Word Padding" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <TextBox v-model="fontName" label="Font Name" />
            </div>

            <div class="col-md-4">
                <NumberBox v-model="minimumFontSize" label="Minimum Font Size" />
            </div>

            <div class="col-md-4">
                <NumberBox v-model="maximumFontSize" label="Maximum Font Size" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <NumberBox v-model="angleCount" label="Angle Count" />
            </div>

            <div class="col-md-4">
                <NumberBox v-model="minimumAngle" label="Minimum Angle" />
            </div>

            <div class="col-md-4">
                <NumberBox v-model="maximumAngle" label="Maximum Angle" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <CheckBox v-model="autoClear" label="Auto Clear" />
            </div>

            <div class="col-md-4">
                <NumberBox v-model="animationDuration" label="Animation Duration" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Event Calendar Picker */
const eventCalendarPickerGallery = defineComponent({
    name: "EventCalendarPickerGallery",
    components: {
        GalleryAndResult,
        EventCalendarPicker
    },
    setup() {

        return {
            value: ref(null),
            importCode: getSfcControlImportPath("eventCalendarPicker"),
            exampleCode: `<EventCalendarPicker label="Event Calendar" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <EventCalendarPicker label="Event Calendar" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Group Type Picker */
const groupTypePickerGallery = defineComponent({
    name: "GroupTypePickerGallery",
    components: {
        GalleryAndResult,
        GroupTypePicker,
        CheckBox
    },
    setup() {

        return {
            value: ref(null),
            isSortedByName: ref(false),
            multiple: ref(false),
            importCode: getSfcControlImportPath("groupTypePicker"),
            exampleCode: `<GroupTypePicker label="Group Type" v-model="value" :groupTypes="[...groupTypeGuids]" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <GroupTypePicker label="Group Type" v-model="value" :isSortedByName="isSortedByName" :multiple="multiple" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox v-model="isSortedByName" label="Sort by Name" />
            </div>
            <div class="col-md-4">
                <CheckBox v-model="multiple" label="Multiple" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates Location Address Picker */
const locationAddressPickerGallery = defineComponent({
    name: "LocationAddressPickerGallery",
    components: {
        GalleryAndResult,
        LocationAddressPicker,
        DropDownList,
        CheckBox,
        TextBox,
        Toggle
    },
    setup() {
        return {
            value: ref({}),
            importCode: getSfcControlImportPath("locationAddressPicker"),
            exampleCode: `<LocationAddressPicker v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <LocationAddressPicker label="Location Address Picker" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates location picker */
const locationPickerGallery = defineComponent({
    name: "LocationPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        LocationPicker
    },
    setup() {
        return {
            value: ref(null),
            importCode: getSfcControlImportPath("locationPicker"),
            exampleCode: `<LocationPicker label="Location" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <LocationPicker label="Location" v-model="value" :multiple="multiple" />

    <template #settings>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates location list */
const locationListGallery = defineComponent({
    name: "LocationListGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        TextBox,
        DefinedValuePicker,
        LocationList
    },
    setup() {
        return {
            value: ref(null),
            locationType: ref(""),
            parentLocation: ref(""),
            showCityState: ref(false),
            multiple: ref(false),
            allowAdd: ref(false),
            showBlankItem: ref(false),
            isAddressRequired: ref(false),
            parentLocationGuid: ref("e0545b4d-4f97-43b0-971f-94b593ae2134"),
            importCode: getSfcControlImportPath("locationList"),
            exampleCode: `<LocationList label="Location" v-model="value" :multiple="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <LocationList label="Location" v-model="value" :multiple="multiple" :locationTypeValueGuid="locationType?.value" :allowAdd="allowAdd" :showCityState="showCityState" :showBlankItem="showBlankItem" :isAddressRequired="isAddressRequired" :parentLocationGuid="parentLocationGuid" />

    <template #settings>
        <div class="row">
            <div class="col-md-3">
                <CheckBox v-model="showCityState" label="Show City/State" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="multiple" label="Multiple" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="allowAdd" label="Allow Adding Values" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="showBlankItem" label="Show Blank Item" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="isAddressRequired" label="Require Address" help="Only applies when adding a new location." />
            </div>
            <div class="col-md-3">
                <TextBox v-model="parentLocationGuid" label="Parent Location Guid" />
            </div>
            <div class="col-md-3">
                <DefinedValuePicker v-model="locationType" label="Location Type" definedTypeGuid="3285DCEF-FAA4-43B9-9338-983F4A384ABA" showBlankItem />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates ethnicity picker */
const ethnicityPickerGallery = defineComponent({
    name: "EthnicityPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        EthnicityPicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("ethnicityPicker"),
            exampleCode: `<EthnicityPicker v-model="value" :multiple="false" :showBlankItem="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <EthnicityPicker
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row mb-3">
            <div class="col-md-3">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>

            <div class="col-md-3">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-3">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates race picker */
const racePickerGallery = defineComponent({
    name: "RacePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DropDownList,
        RacePicker,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref({}),
            importCode: getControlImportPath("racePicker"),
            exampleCode: `<RacePicker v-model="value" :multiple="false" :showBlankItem="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RacePicker
        v-model="value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row mb-3">
            <div class="col-md-3">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>

            <div class="col-md-3">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
            <div class="col-md-3">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>

            <div class="col-md-3">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates media element picker */
const mediaElementPickerGallery = defineComponent({
    name: "MediaElementPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        TextBox,
        DropDownList,
        MediaElementPicker
    },
    setup() {
        return {
            value: ref(null),
            account: ref(null),
            folder: ref(null),
            multiple: ref(false),
            showBlankItem: ref(false),
            hideRefresh: ref(false),
            required: ref(false),
            hideAccountPicker: ref(false),
            hideFolderPicker: ref(false),
            hideMediaPicker: ref(false),
            importCode: getSfcControlImportPath("mediaElementPicker"),
            exampleCode: `<MediaElementPicker label="Media" v-model="value" :isRefreshDisallowed="false" :hideAccountPicker="hideAccountPicker" :hideFolderPicker="hideFolderPicker" :hideMediaPicker="hideMediaPicker" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{account, folder, modelValue: value}"
    hasMultipleValues
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <MediaElementPicker label="Media Element"
        v-model="value"
        v-model:account="account"
        v-model:folder="folder"
        :multiple="multiple"
        :showBlankItem="showBlankItem"
        :hideRefreshButtons="hideRefresh"
        :rules="required ? 'required' : ''"
        :hideAccountPicker="hideAccountPicker"
        :hideFolderPicker="hideFolderPicker"
        :hideMediaPicker="hideMediaPicker"
    />

    <template #settings>
        <div class="row">
            <div class="col-md-3">
                <CheckBox v-model="multiple" label="Multiple" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="hideRefresh" label="Hide Refresh Buttons" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="required" label="Required" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="hideAccountPicker" label="Hide Account Picker" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="hideFolderPicker" label="Hide Folder Picker" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="hideMediaPicker" label="Hide Media Picker" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates merge field picker */
const mergeFieldPickerGallery = defineComponent({
    name: "MergeFieldPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        MergeFieldPicker,
        TextBox
    },
    setup() {
        const value = ref([
            {
                "value": "Rock.Model.Group|ArchivedByPersonAlias|Person|Aliases|AliasedDateTime",
                "text": "Aliased Date Time"
            },
            {
                "value": "Rock.Model.Person|ConnectionStatusValue|Category|CreatedByPersonAliasId",
                "text": "Created By Person Alias Id"
            }
        ]);

        return {
            multiple: ref(true),
            value,
            additionalFields: ref("GlobalAttribute,Rock.Model.Person,Rock.Model.Group"),
            importCode: getSfcControlImportPath("mergeFieldPicker"),
            exampleCode: `<MergeFieldPicker label="Merge Field" v-model="value" :multiple="false" additionalFields="GlobalAttribute,Rock.Model.Person,Rock.Model.Group" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <MergeFieldPicker label="Merge Field" v-model="value" :multiple="multiple" :additionalFields="additionalFields" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <TextBox label="Root Merge Fields" v-model="additionalFields" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates categorized value picker */
const categorizedValuePickerGallery = defineComponent({
    name: "CategorizedValuePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        CategorizedValuePicker,
        TextBox
    },
    setup() {
        return {
            multiple: ref(true),
            value: ref(null),
            required: ref(false),
            definedType: ref(DefinedType.PowerbiAccounts),
            importCode: getSfcControlImportPath("categorizedValuePicker"),
            exampleCode: `<CategorizedValuePicker label="Categorized Defined Value" v-model="value" :definedTypeGuid="DefinedType.PowerbiAccounts" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <CategorizedValuePicker label="Categorized Defined Value" v-model="value" :definedTypeGuid="definedType" :rules="required ? 'required' : ''" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
        </div>
        <p class="my-4">
            <strong>NOTE:</strong> This picker will be empty unless you specify a defined type that has
            categorized values. By default, there aren't any, so you may need to configure a defined type to
            have categories and add values to those categories in order to see what this control can do.
        </p>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates reminder type picker */
const reminderTypePickerGallery = defineComponent({
    name: "ReminderTypePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        ReminderTypePicker,
        DropDownList,
        EntityTypePicker,
        TextBox,
        NumberUpDown
    },
    setup() {
        return {
            columnCount: ref(0),
            displayStyle: ref(PickerDisplayStyle.Auto),
            displayStyleItems,
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            showBlankItem: ref(false),
            value: ref(null),
            required: ref(false),
            entityTypeGuid: ref(null),
            importCode: getSfcControlImportPath("reminderTypePicker"),
            exampleCode: `<ReminderTypePicker label="Reminder Type" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <ReminderTypePicker
        label="Reminder Type"
        v-model="value"
        :entityTypeGuid="entityTypeGuid?.value"
        :multiple="multiple"
        :columnCount="columnCount"
        :enhanceForLongLists="enhanceForLongLists"
        :displayStyle="displayStyle"
        :showBlankItem="showBlankItem" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Show Blank Item" v-model="showBlankItem" />
            </div>
            <div class="col-md-4">
                <DropDownList label="Display Style" :showBlankItem="false" v-model="displayStyle" :items="displayStyleItems" />
            </div>
            <div class="col-md-4">
                <NumberUpDown label="Column Count" v-model="columnCount" :min="0" />
            </div>
            <div class="col-md-4">
                <EntityTypePicker label="For Entity Type" v-model="entityTypeGuid" enhanceForLongLists showBlankItem />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates group role picker */
const groupRolePickerGallery = defineComponent({
    name: "GroupRolePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        GroupRolePicker,
        TextBox
    },
    setup() {
        return {
            value: ref(null),
            required: ref(false),
            importCode: getSfcControlImportPath("groupRolePicker"),
            exampleCode: `<GroupRolePicker label="Group Type and Role" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <GroupRolePicker label="Group Type and Role" v-model="value" :rules="required ? 'required' : ''" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates modal alert */
const modalAlertGallery = defineComponent({
    name: "ModalAlertGallery",
    components: {
        GalleryAndResult,
        RockButton,
        ModalAlert,
        TextBox,
        DropDownList
    },
    setup() {
        const types = [
            {
                text: ModalAlertType.Alert,
                value: ModalAlertType.Alert
            },
            {
                text: ModalAlertType.Information,
                value: ModalAlertType.Information
            },
            {
                text: ModalAlertType.Warning,
                value: ModalAlertType.Warning
            },
            {
                text: ModalAlertType.None,
                value: ModalAlertType.None
            }
        ];

        return {
            types,
            type: ref("Alert"),
            isShowing: ref(false),
            message: ref("Message I want to alert you to."),
            importCode: getSfcControlImportPath("modalAlert"),
            exampleCode: `<ModalAlert v-model="isShowing" type="ModalAlertType.Alert">Message I want to alert you to.</ModalAlert>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <ModalAlert v-model="isShowing" :type="type">{{message}}</ModalAlert>

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <RockButton @click="isShowing = true">Show</RockButton>
            </div>
            <div class="col-md-4">
                <TextBox label="Message" v-model="message" />
            </div>
            <div class="col-md-4">
                <DropDownList label="Alert Type" v-model="type" :items="types" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates content channel item picker */
const contentChannelItemPickerGallery = defineComponent({
    name: "ContentChannelItemPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        ContentChannelItemPicker,
        TextBox
    },
    setup() {
        return {
            value: ref({
                "value": "d6d4a292-f794-4d0c-bd29-420631a858b3",
                "text": "Miracles in Luke",
                "category": null
            }),
            required: ref(false),
            importCode: getSfcControlImportPath("contentChannelItemPicker"),
            exampleCode: `<ContentChannelItemPicker label="Content Channel Item" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <ContentChannelItemPicker label="Choose A Content Channel Item" v-model="value" :rules="required ? 'required' : ''" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates person link */
const personLinkGallery = defineComponent({
    name: "PersonLinkGallery",
    components: {
        GalleryAndResult,
        PersonLink,
        DropDownList,
        TextBox
    },
    setup() {
        const placement = ref("right");
        const textAlign = computed(() => {
            if (placement.value == "right") {
                return "left";
            }

            if (placement.value == "left") {
                return "right";
            }

            return "center";
        });

        return {
            placementOptions: [
                { text: "Top", value: "top" },
                { text: "Right (Default)", value: "right" },
                { text: "Bottom", value: "bottom" },
                { text: "Left", value: "left" },
            ],
            placement,
            textAlign,
            personName: ref("Ted Decker"),
            role: ref("Member"),
            photoId: ref(""),
            personId: ref("1"),
            importCode: getSfcControlImportPath("personLink"),
            exampleCode: `<PersonLink :personId="56" personName="Ted Decker" role="Member" popOverPlacement="right" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <div :style="{textAlign, 'margin-top': placement == 'top' ? '150px' : '0'}">
        <PersonLink :personId="personId" :personName="personName" :photoId="photoId" :role="role" :popOverPlacement="placement" />
    </div>
    <div class="mt-5 text-center" v-if="textAlign != 'left'"><strong>Note:</strong> The link has been moved to demonstrate the placement position of the pop over better. Changing the pop over placement does not normally move PersonLink around, just the position of the pop over.</div>

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <TextBox v-model="personName" label="Person Name" />
            </div>
            <div class="col-md-4">
                <TextBox v-model="role" label="Role" />
            </div>
            <div class="col-md-4">
                <TextBox v-model="photoId" label="Photo ID" help="NOTE: Providing a photo ID only adds a dot. Currently, this does nothing else and the value does not matter, as long as a value is provided." />
            </div>
            <div class="col-md-4">
                <TextBox v-model="personId" label="Person ID" />
            </div>
            <div class="col-md-4">
                <DropDownList v-model="placement" :items="placementOptions" label="Pop Over Placement" :showBlankItem="false" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates popOver */
const popOverGallery = defineComponent({
    name: "PopOverGallery",
    components: {
        GalleryAndResult,
        PopOver,
        DropDownList,
        CheckBox
    },
    setup() {
        const placement = ref("right");
        const triggerUpdate = ref(false);

        watch(placement, () => {
            triggerUpdate.value = true;
        });

        return {
            placementOptions: [
                { text: "Top", value: "top" },
                { text: "Right (Default)", value: "right" },
                { text: "Bottom", value: "bottom" },
                { text: "Left", value: "left" },
            ],
            placement,
            triggerUpdate,
            show: ref(false),
            importCode: getSfcControlImportPath("popOver"),
            exampleCode: `<PopOver v-model:isVisible="isVisible" placement="right">
    <template #activator="props">
        <strong v-bind="props">Hover Me</strong>
    </template>
    <template #popOverContent>
        This is the content that shows up in the popOver
    </template>
</PopOver>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <div class="text-center">
        <PopOver v-model:isVisible="show" :placement="placement" v-model:triggerUpdate="triggerUpdate">
            <template #activator="props">
                <strong v-bind="props">Hover Me</strong>
            </template>
            <template #popOverContent>
                This is the content that shows up in the popOver
            </template>
        </PopOver>
    </div>

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox v-model="show" label="Show PopOver" />
            </div>
            <div class="col-md-4">
                <DropDownList v-model="placement" :items="placementOptions" label="Pop Over Placement" :showBlankItem="false" />
            </div>
        </div>
    </template>

    <template #syntaxNotes>
        <p class="font-italic"><strong>Important Notes:</strong> The <code>activator</code> slot's contents must be an HTML element. Putting a component there will not work. Also,
        you must bind the activator slot's props to that element. This allows the popOver to attach the event listeners so it can detect if
        it is being hovered.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates rockLiteral */
const rockLiteralGallery = defineComponent({
    name: "RockLiteralGallery",
    components: {
        GalleryAndResult,
        RockLiteral,
        TextBox,
        CheckBox
    },
    setup() {
        return {
            label: ref("Romans 11:33"),
            labelClass: ref(""),
            content: ref("<p>Oh, the depth of the riches and the wisdom and the knowledge of God!<br> How unsearchable his judgments and untraceable his ways!"),
            useLabelSlot: ref(false),
            importCode: getSfcControlImportPath("rockLiteral"),
            exampleCode: `// Simple Label
<RockLiteral label="Label Text" labelCssClass="text-primary">
    My content beneath the label.
</RockLiteral>

// Advanced Label with Slot
<RockLiteral labelCssClass="text-primary">
    <template #label><i class="fa fa-cross"></i> <strong>My Custom Label</strong></template>
    My content beneath the label.
</RockLiteral>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <RockLiteral :labelCssClass="labelClass" :label="label">
        <template #label v-if="useLabelSlot"><span v-html="label"></span></template>
        <div v-html="content"></div>
    </RockLiteral>

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <TextBox v-model="label" label="Label Text" textMode="multiline" />
            </div>
            <div class="col-md-4">
                <CheckBox v-model="useLabelSlot" label="Use Label Slot" help="Instead of using the prop. This allows you to pass in HTML or a component for the label instead of plain text." />
            </div>
            <div class="col-md-4">
                <TextBox v-model="labelClass" label="Label Class" help="Try something like <code>text-primary</code> to change the color" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-4">
                <TextBox v-model="content" label="Content HTML" textMode="multiline" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a registry entry */
const registryEntryGallery = defineComponent({
    name: "RegistryEntryGallery",
    components: {
        GalleryAndResult,
        RegistryEntry,
        RockForm,
        RockButton,
        CheckBox
    },
    setup() {
        return {
            entry: ref(null),
            submit: ref(false),
            isRequired: ref(false),
            importCode: getSfcControlImportPath("registryEntry"),
            exampleCode: `<RegistryEntry label="Registry Entry" v-model="phoneNumber" />`
        };
    },
    template: `
<GalleryAndResult
    :value="entry"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RockForm v-model:submit="submit">
        <RegistryEntry label="Registry Entry" v-model="entry" :rules="isRequired ? 'required' : ''" class="text-primary" />
        <RockButton @click="submit=true">Validate</RockButton>
    </RockForm>

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox v-model="isRequired" label="Required" />
            </div>
        </div>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates group type group picker */
const groupTypeGroupPickerGallery = defineComponent({
    name: "GroupTypeGroupPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        GroupTypeGroupPicker,
        TextBox,
        RockButton
    },
    setup() {
        return {
            value: ref(null),
            groupType: ref(null),
            required: ref(false),
            glabel: ref("Group"),
            importCode: getSfcControlImportPath("groupTypeGroupPicker"),
            exampleCode: `<GroupTypeGroupPicker label="Group Type and Group" groupLabel="Group" v-model="value" v-model:groupType="groupType" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{value, groupType}"
    :importCode="importCode"
    :exampleCode="exampleCode"
    hasMultipleValues
    enableReflection >

    <GroupTypeGroupPicker label="Group Type and Group" :groupLabel="glabel" v-model="value" v-model:groupType="groupType" :rules="required ? 'required' : ''" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <TextBox label="Group Label" v-model="glabel" help="The label for the 2nd dropdown. The label for the first dropdown is not customizable" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates group and role picker */
const groupAndRolePickerGallery = defineComponent({
    name: "GroupAndRolePickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        GroupAndRolePicker,
        TextBox,
        RockButton,
        RockForm
    },
    setup() {
        return {
            value: ref(null),
            groupType: ref(null),
            group: ref(null),
            required: ref(false),
            disabled: ref(false),
            glabel: ref("Group"),
            submit: ref(false),
            importCode: getSfcControlImportPath("groupAndRolePicker"),
            exampleCode: `<GroupAndRolePicker label="Group and Role" groupLabel="Group" v-model="value" v-model:groupType="groupType" v-model:group="group" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{groupType, group, value}"
    :importCode="importCode"
    :exampleCode="exampleCode"
    hasMultipleValues
    enableReflection >

<RockForm v-model:submit="submit">
    <GroupAndRolePicker label="Group and Role" :groupLabel="glabel" v-model="value" v-model:groupType="groupType" v-model:group="group" :rules="required ? 'required' : ''" :disabled="disabled" />
    <RockButton @click="submit = true">Submit</RockButton>
</RockForm>

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <TextBox label="Group Label" v-model="glabel" help="The label for the 2nd dropdown. The main label is also customizable, but the group type and role labels are not." />
            </div>
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Disabled" v-model="disabled" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates account picker */
const accountPickerGallery = defineComponent({
    name: "AccountPickerGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        AccountPicker, https://configurelaptop.eu/cat/custom-laptop/
            TextBox,
        RockButton
    },
    setup() {
        return {
            value: ref(null),
            activeOnly: ref(false),
            displayPublic: ref(false),
            multiple: ref(false),
            enhance: ref(false),
            displayChildItemCountLabel: ref(false),
            importCode: getSfcControlImportPath("accountPicker"),
            exampleCode: `<AccountPicker label="Financial Account" v-model="value" enhanceForLongLists activeOnly displayPublicName multiple displayChildItemCountLabel />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <AccountPicker label="Financial Account" v-model="value" :enhanceForLongLists="enhance" :activeOnly="activeOnly" :displayPublicName="displayPublic" :multiple="multiple" :displayChildItemCountLabel="displayChildItemCountLabel" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhance" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Active Only" v-model="activeOnly" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Display Public Names" v-model="displayPublic" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Select Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Display Child Count" v-model="displayChildItemCountLabel" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates new editor */
const noteTextEditorGallery = defineComponent({
    name: "NoteTextEditorGallery",
    components: {
        GalleryAndResult,
        NoteTextEditor,
        CheckBox
    },
    setup() {
        return {
            value: ref(""),
            importCode: getSfcControlImportPath("noteTextEditor"),
            exampleCode: `<NoteTextEditor v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection>

    <NoteTextEditor v-model="value" :avatar="avatar" />

    <template #settings>
    </template>
</GalleryAndResult>`
});

/** Demonstrates structured content editor */
const structuredContentEditorGallery = defineComponent({
    name: "StructuredContentEditorGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        DefinedValuePicker,
        StructuredContentEditor
    },
    setup() {
        const required = ref(false);
        const toolsItemBag = ref<ListItemBag | undefined>({
            value: DefinedValue.StructureContentEditorDefault
        });
        const toolsGuid = computed(() => toolsItemBag.value?.value);
        const toolsTypeGuid = DefinedType.StructuredContentEditorTools;

        return {
            value: ref("{}"),
            required,
            importCode: getSfcControlImportPath("structuredContentEditor"),
            exampleCode: computed(() => `<StructuredContentEditor v-model="value" label="StructuredContent Editor" :toolsGuid="${toolsGuid.value}" ${required.value ? 'rules="required" ' : ""}/>`),
            toolsGuid,
            toolsItemBag,
            toolsTypeGuid
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <StructuredContentEditor
        v-model="value"
        label="Structured Content Editor"
        :toolsGuid="toolsGuid"
        :rules="required ? 'required' : ''" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
            <div class="col-md-4">
                <DefinedValuePicker v-model="toolsItemBag" :definedTypeGuid="toolsTypeGuid" label="Structured Content Editor Tools Value" lazyMode="eager" :multiple="false" />
            </div>
        </div>
        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates registrationInstance picker */
const registrationInstancePickerGallery = defineComponent({
    name: "RegistrationInstancePickerGallery",
    components: {
        GalleryAndResult,
        RegistrationInstancePicker,
        RegistrationTemplatePicker,
        CheckBox
    },
    setup() {
        return {
            registrationTemplateGuid: ref(null),
            value: ref({
                "value": "eefe4ad9-bfa9-405c-b732-ccb4d857ab73",
                "text": "Joe's Test Registration",
                "category": null
            }),
            required: ref(false),
            disabled: ref(false),
            importCode: getSfcControlImportPath("registrationInstancePicker"),
            exampleCode: `<RegistrationInstancePicker label="Registration Instance" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RegistrationInstancePicker
        v-model="value"
        label="Registration Instance"
        :registrationTemplateGuid="registrationTemplateGuid?.value"
        :disabled="disabled"
        :rules="required ? 'required' : ''" />

    <template #settings>
        <div class="row mb-3">
            <div class="col-md-3">
                <RegistrationTemplatePicker label="Default Registration Template" v-model="registrationTemplateGuid" showBlankItem />
            </div>
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Disabled" v-model="disabled" />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates interactionChannelInteractionComponent picker */
const interactionChannelInteractionComponentPickerGallery = defineComponent({
    name: "InteractionChannelInteractionComponentPickerGallery",
    components: {
        GalleryAndResult,
        InteractionChannelInteractionComponentPicker,
        InteractionChannelPicker,
        DropDownList,
        NumberUpDown
    },
    setup() {
        return {
            interactionChannelGuid: ref(null),
            value: ref({
                "value": "1d6d3e3c-131c-4ed9-befe-b34f3c3da7d3",
                "text": "Calendar",
                "category": null
            }),
            importCode: getSfcControlImportPath("interactionChannelInteractionComponentPicker"),
            exampleCode: `<InteractionChannelInteractionComponentPicker label="Interaction Channel > Interaction Component" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <InteractionChannelInteractionComponentPicker
        v-model="value"
        label="Interaction Channel > Interaction Component"
        :defaultInteractionChannelGuid="interactionChannelGuid?.value" />

    <template #settings>
        <div class="row mb-3">
            <div class="col-md-3">
                <InteractionChannelPicker label="Default Interaction Channel" v-model="interactionChannelGuid" showBlankItem />
            </div>
        </div>

        <p class="text-semibold font-italic">Not all settings are demonstrated in this gallery.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
    </template>
</GalleryAndResult>`
});


/** Demonstrates Workflow Picker */
const workflowPickerGallery = defineComponent({
    name: "WorkflowPickerGallery",
    components: {
        GalleryAndResult,
        WorkflowTypePicker,
        WorkflowPicker,
        CheckBox,
    },
    setup() {
        return {
            value: ref({
                "value": "969b09e5-d830-46b7-86ab-2f0fbd12cf51",
                "text": "New Request",
                "category": null
            }),
            workflowType: ref({}),
            workflowTypeGuid: ref(""),
            required: ref(false),
            disabled: ref(false),
            importCode: getSfcControlImportPath("workflowPicker"),
            exampleCode: `<WorkflowPicker label="Choose a Workflow" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <WorkflowPicker label="Choose a Workflow"
        v-model="value"
        :workflowTypeGuid="workflowTypeGuid?.value"
        :rules="required ? 'required' : ''"
        :disabled="disabled" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <WorkflowTypePicker label="Workflow Type" v-model="workflowTypeGuid" showBlankItem help="If this workflowTypeGuid prop is set, the Workflow Type selector will not be shown and the Workflows will be based on that type." />
            </div>
            <div class="col-md-4">
                <CheckBox label="Required" v-model="required" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Disabled" v-model="disabled" />
            </div>
        </div>
    </template>
</GalleryAndResult>`
});

/** Demonstrates value list component */
const valueListGallery = defineComponent({
    name: "ValueListGallery",
    components: {
        GalleryAndResult,
        ValueList,
        CheckBox,
        TextBox
    },
    setup() {
        const usePredefinedValues = ref(false);
        const displayValueFirst = ref(false);
        const options: ListItemBag[] = [
            {
                text: "Option 1",
                value: "1"
            },
            {
                text: "Option 2",
                value: "2"
            },
            {
                text: "Option 3",
                value: "3"
            },
        ];

        const customValues = computed(() => usePredefinedValues.value ? options : null);

        return {
            usePredefinedValues: usePredefinedValues,
            displayValueFirst,
            customValues,
            fullWidth: ref(false),
            useDefinedType: ref(false),
            value: ref(null),
            definedTypeGuid: DefinedType.PersonConnectionStatus,
            valuePrompt: ref("Value"),
            importCode: getSfcControlImportPath("valueList"),
            exampleCode: `<ValueList label="List of Values" v-model="value" :customValues="customValues" :valuePrompt="valuePrompt" :definedTypeGuid="definedTypeGuid" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <ValueList label="List of Values" v-model="value" :customValues="customValues" :valuePrompt="valuePrompt" :fullWidth="fullWidth" :definedTypeGuid="useDefinedType ? definedTypeGuid : null" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Use Predefined Values" v-model="usePredefinedValues" help="Enabling this will pass a pre-made <code>ListItemBag[]</code> of options to the ValueList component via the <code>customValues</code> prop." :disabled="useDefinedType" />
            <CheckBox formGroupClasses="col-md-3" label="Use Defined Type" v-model="useDefinedType" help="Enabling this will pass the Connection Status Defined Type's GUID to the ValueList component via the <code>definedTypeGuid</code> prop." :disabled="usePredefinedValues" />
            <CheckBox formGroupClasses="col-md-3" label="Full Width" v-model="fullWidth" />
            <TextBox formGroupClasses="col-md-3" label="Placeholder for Value Field" v-model="valuePrompt" />
        </div>
        <p>
            There are 2 different props that control what options users can choose/enter.
            The <code>definedTypeGuid</code> prop takes a GUID string and will limit users to choosing values from a list of defined values of that type.
            The <code>customValues</code> option allows you to pass a <code>ListItemBag</code> array in as a list of options that the user can choose from a dropdown.
            If both of those props are specified, the <code>definedTypeGuid</code> prop will take precedence.
            If neither option is used, a text box is shown, allowing users to manually type in any values.
        </p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates block template picker component */
const blockTemplatePickerGallery = defineComponent({
    name: "BlockTemplatePickerGallery",
    components: {
        GalleryAndResult,
        BlockTemplatePicker,
        DefinedValuePicker
    },
    setup() {
        return {
            value: ref(null),
            templateKey: ref(null),
            definedTypeGuid: DefinedType.TemplateBlock,
            templateBlockGuid: ref(null),
            importCode: getSfcControlImportPath("blockTemplatePicker"),
            exampleCode: `<BlockTemplatePicker label="Select a Template" v-model="value" :templateBlockValueGuid="templateBlockValueGuid" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{value, templateKey}"
    :importCode="importCode"
    :exampleCode="exampleCode"
    hasMultipleValues
    enableReflection >

    <BlockTemplatePicker label="Select a Template" v-model="value" v-model:templateKey="templateKey" :templateBlockValueGuid="templateBlockGuid?.value" />

    <template #settings>
        <div class="row">
            <DefinedValuePicker label="Template Block" formGroupClasses="col-md-4" v-model="templateBlockGuid" :definedTypeGuid="definedTypeGuid" showBlankItem />
        </div>
    </template>
</GalleryAndResult>`
});


const controlGalleryComponents: Record<string, Component> = [
    notificationBoxGallery,
    attributeValuesContainerGallery,
    badgeListGallery,
    fieldFilterEditorGallery,
    textBoxGallery,
    datePickerGallery,
    dateRangePickerGallery,
    dateTimePickerGallery,
    datePartsPickerGallery,
    radioButtonListGallery,
    dialogGallery,
    checkBoxGallery,
    inlineCheckBoxGallery,
    switchGallery,
    inlineSwitchGallery,
    checkBoxListGallery,
    mediaSelectorGallery,
    listBoxGallery,
    phoneNumberBoxGallery,
    dropDownListGallery,
    helpBlockGallery,
    colorPickerGallery,
    numberBoxGallery,
    numberRangeBoxGallery,
    genderDropDownListGallery,
    socialSecurityNumberBoxGallery,
    timePickerGallery,
    ratingGallery,
    CurrencyBoxGallery,
    emailBoxGallery,
    numberUpDownGallery,
    staticFormControlGallery,
    addressControlGallery,
    toggleGallery,
    progressTrackerGallery,
    itemsWithPreAndPostHtmlGallery,
    urlLinkBoxGallery,
    fullscreenGallery,
    panelGallery,
    PersonPickerGallery,
    fileUploaderGallery,
    imageUploaderGallery,
    slidingDateRangePickerGallery,
    definedValuePickerGallery,
    campusPickerGallery,
    entityTypePickerGallery,
    sectionHeaderGallery,
    sectionContainerGallery,
    categoryPickerGallery,
    locationItemPickerGallery,
    copyButtonGallery,
    tagListGallery,
    followingGallery,
    achievementTypePickerGallery,
    badgeComponentPickerGallery,
    assessmentTypePickerGallery,
    assetStorageProviderPickerGallery,
    auditDetailGallery,
    binaryFileTypePickerGallery,
    binaryFilePickerGallery,
    codeEditorGallery,
    modalGallery,
    eventItemPickerGallery,
    dataViewPickerGallery,
    workflowTypePickerGallery,
    componentPickerGallery,
    financialGatewayPickerGallery,
    financialStatementTemplatePickerGallery,
    fieldTypePickerGallery,
    gradePickerGallery,
    groupMemberPickerGallery,
    interactionChannelPickerGallery,
    interactionComponentPickerGallery,
    lavaCommandPickerGallery,
    remoteAuthsPickerGallery,
    stepProgramPickerGallery,
    stepProgramStepTypePickerGallery,
    stepProgramStepStatusPickerGallery,
    stepStatusPickerGallery,
    stepTypePickerGallery,
    streakTypePickerGallery,
    badgePickerGallery,
    basicTimePickerGallery,
    birthdayPickerGallery,
    countdownTimerGallery,
    electronicSignatureGallery,
    fieldTypeEditorGallery,
    inlineRangeSliderGallery,
    javaScriptAnchorGallery,
    keyValueListGallery,
    loadingGallery,
    loadingIndicatorGallery,
    numberUpDownGroupGallery,
    progressBarGallery,
    rockButtonGallery,
    rockLabelGallery,
    rockValidationGallery,
    rangeSliderGallery,
    tabbedBarGallery,
    tabbedContentGallery,
    transitionVerticalCollapseGallery,
    valueDetailListGallery,
    pagePickerGallery,
    connectionRequestPickerGallery,
    groupPickerGallery,
    mergeTemplatePickerGallery,
    metricCategoryPickerGallery,
    metricItemPickerGallery,
    registrationTemplatePickerGallery,
    reportPickerGallery,
    schedulePickerGallery,
    workflowActionTypePickerGallery,
    dayOfWeekPickerGallery,
    monthDayPickerGallery,
    monthYearPickerGallery,
    cacheabilityPickerGallery,
    buttonGroupGallery,
    intervalPickerGallery,
    geoPickerGallery,
    contentDropDownPickerGallery,
    scheduleBuilderGallery,
    wordCloudGallery,
    eventCalendarPickerGallery,
    groupTypePickerGallery,
    locationAddressPickerGallery,
    locationPickerGallery,
    locationListGallery,
    ethnicityPickerGallery,
    racePickerGallery,
    mediaElementPickerGallery,
    mergeFieldPickerGallery,
    categorizedValuePickerGallery,
    reminderTypePickerGallery,
    groupRolePickerGallery,
    modalAlertGallery,
    contentChannelItemPickerGallery,
    personLinkGallery,
    popOverGallery,
    rockLiteralGallery,
    registryEntryGallery,
    groupTypeGroupPickerGallery,
    groupAndRolePickerGallery,
    accountPickerGallery,
    noteTextEditorGallery,
    structuredContentEditorGallery,
    registrationInstancePickerGallery,
    interactionChannelInteractionComponentPickerGallery,
    workflowPickerGallery,
    valueListGallery,
    blockTemplatePickerGallery,
    DropDownMenuGallery,
    DropDownContentGallery,
    ButtonDropDownListGallery,
    CampusAccountAmountPickerGallery,
    LightGridGallery,
    ImageEditorGallery,
]
    // Sort list by component name
    .sort((a, b) => a.name.localeCompare(b.name))
    // Convert list to an object where the key is the component name and the value is the component
    .reduce((newList, comp) => {
        newList[comp.name] = comp;
        return newList;
    }, {});

// #endregion

// #region Template Gallery

/** Demonstrates the detailPanel component. */
const detailBlockGallery = defineComponent({
    name: "DetailBlockGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        CheckBoxList,
        DetailBlock
    },

    setup() {
        const simulateValues = ref<string[]>([]);

        const headerActions = computed((): PanelAction[] => {
            if (!simulateValues.value.includes("headerActions")) {
                return [];
            }

            return [
                {
                    iconCssClass: "fa fa-user",
                    title: "Action 1",
                    type: "default",
                    handler: () => alert("Action 1 selected.")
                },
                {
                    iconCssClass: "fa fa-group",
                    title: "Action 2",
                    type: "success",
                    handler: () => alert("Action 2 selected.")
                }
            ];
        });

        const labels = computed((): PanelAction[] => {
            if (!simulateValues.value.includes("labels")) {
                return [];
            }

            return [
                {
                    iconCssClass: "fa fa-user",
                    title: "Action 1",
                    type: "info",
                    handler: () => alert("Action 1 selected.")
                },
                {
                    iconCssClass: "fa fa-group",
                    title: "Action 2",
                    type: "success",
                    handler: () => alert("Action 2 selected.")
                }
            ];
        });

        const headerSecondaryActions = computed((): PanelAction[] => {
            if (!simulateValues.value.includes("headerSecondaryActions")) {
                return [];
            }

            return [
                {
                    iconCssClass: "fa fa-user",
                    title: "Action 1",
                    type: "default",
                    handler: () => alert("Action 1 selected.")
                },
                {
                    iconCssClass: "fa fa-group",
                    title: "Action 2",
                    type: "success",
                    handler: () => alert("Action 2 selected.")
                }
            ];
        });

        const footerActions = computed((): PanelAction[] => {
            if (!simulateValues.value.includes("footerActions")) {
                return [];
            }

            return [
                {
                    iconCssClass: "fa fa-user",
                    title: "Action 1",
                    type: "default",
                    handler: () => alert("Action 1 selected.")
                },
                {
                    iconCssClass: "fa fa-group",
                    title: "Action 2",
                    type: "success",
                    handler: () => alert("Action 2 selected.")
                }
            ];
        });

        const footerSecondaryActions = computed((): PanelAction[] => {
            if (!simulateValues.value.includes("footerSecondaryActions")) {
                return [];
            }

            return [
                {
                    iconCssClass: "fa fa-user",
                    title: "Action 1",
                    type: "default",
                    handler: () => alert("Action 1 selected.")
                },
                {
                    iconCssClass: "fa fa-group",
                    title: "Action 2",
                    type: "success",
                    handler: () => alert("Action 2 selected.")
                }
            ];
        });

        return {
            colors: Array.apply(0, Array(256)).map((_: unknown, index: number) => `rgb(${index}, ${index}, ${index})`),
            entityTypeGuid: EntityType.Group,
            footerActions,
            footerSecondaryActions,
            headerActions,
            headerSecondaryActions,
            isAuditHidden: ref(false),
            isBadgesVisible: ref(true),
            isDeleteVisible: ref(true),
            isEditVisible: ref(true),
            isFollowVisible: ref(true),
            isSecurityHidden: ref(false),
            isTagsVisible: ref(false),
            labels,
            simulateValues,
            simulateOptions: [
                {
                    value: "headerActions",
                    text: "Header Actions"
                },
                {
                    value: "headerSecondaryActions",
                    text: "Header Secondary Actions"
                },
                {
                    value: "labels",
                    text: "Labels",
                },
                {
                    value: "footerActions",
                    text: "Footer Actions"
                },
                {
                    value: "footerSecondaryActions",
                    text: "Footer Secondary Actions"
                },
                {
                    value: "helpContent",
                    text: "Help Content"
                }
            ],
            simulateHelp: computed((): boolean => simulateValues.value.includes("helpContent")),
            importCode: getTemplateImportPath("detailBlock"),
            exampleCode: `<DetailBlock name="Sample Entity" :entityTypeGuid="entityTypeGuid" entityTypeName="Entity Type" entityKey="57dc00a3-ff88-4d4c-9878-30ae309117e2" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode">
    <DetailBlock name="Sample Entity"
        :entityTypeGuid="entityTypeGuid"
        entityTypeName="Entity Type"
        entityKey="57dc00a3-ff88-4d4c-9878-30ae309117e2"
        :headerActions="headerActions"
        :headerSecondaryActions="headerSecondaryActions"
        :labels="labels"
        :footerActions="footerActions"
        :footerSecondaryActions="footerSecondaryActions"
        :isAuditHidden="isAuditHidden"
        :isEditVisible="isEditVisible"
        :isDeleteVisible="isDeleteVisible"
        :isFollowVisible="isFollowVisible"
        :isBadgesVisible="isBadgesVisible"
        :isSecurityHidden="isSecurityHidden"
        :isTagsVisible="isTagsVisible">
        <template v-if="simulateHelp" #helpContent>
            This is some help text.
        </template>
        <div v-for="c in colors" :style="{ background: c, height: '1px' }"></div>
    </DetailBlock>

    <template #settings>
        <div class="row">
            <div class="col-md-3">
                <CheckBox v-model="isAuditHidden" label="Is Audit Hidden" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="isBadgesVisible" label="Is Badges Visible" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="isDeleteVisible" label="Is Delete Visible" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="isEditVisible" label="Is Edit Visible" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="isFollowVisible" label="Is Follow Visible" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="isSecurityHidden" label="Is Security Hidden" />
            </div>
            <div class="col-md-3">
                <CheckBox v-model="isTagsVisible" label="Is Tags Visible" />
            </div>
        </div>

        <CheckBoxList v-model="simulateValues" label="Simulate" :items="simulateOptions" horizontal />
    </template>
</GalleryAndResult>`
});

const templateGalleryComponents = [
    detailBlockGallery
]
    .sort((a, b) => a.name.localeCompare(b.name))
    .reduce((newList, comp) => {
        newList[comp.name] = comp;
        return newList;
    }, {});

// #endregion

export default defineComponent({
    name: "Example.ControlGallery",
    components: {
        Panel,
        SectionHeader,
        ...controlGalleryComponents,
        ...templateGalleryComponents
    },

    setup() {
        const currentComponent = ref<Component>(Object.values(controlGalleryComponents)[0]);

        function getComponentFromHash(): void {
            const hashComponent = new URL(document.URL).hash.replace("#", "");
            const component = controlGalleryComponents[hashComponent] ?? templateGalleryComponents[hashComponent];

            if (component) {
                currentComponent.value = component;
            }
        }

        onMounted(() => {
            getComponentFromHash();

            window.addEventListener("hashchange", getComponentFromHash);
        });

        onUnmounted(() => {
            window.removeEventListener("hashchange", getComponentFromHash);
        });

        return {
            currentComponent,
            convertComponentName,
            controlGalleryComponents,
            templateGalleryComponents
        };
    },

    template: `
<v-style>
.gallerySidebar {
    border-radius: 0;
    margin: -1px 0 -1px -1px;
    overflow-y: auto;
    flex-shrink: 0;
}

.gallerySidebar li.current {
    font-weight: 700;
}

.galleryContent {
    flex-grow: 1;
    overflow-x: clip;
    overflow-y: auto;
    padding: 20px;
}

.galleryContent > .rock-header hr {
    margin-left: -20px;
    margin-right: -20px;
}
</v-style>
<Panel type="block">
    <template #title>
        Obsidian Control Gallery
    </template>
    <template #default>
        <div class="panel-flex-fill-body flex-row">

            <div class="gallerySidebar well">
                <h4>Components</h4>

                <ul class="list-unstyled mb-0">
                    <li v-for="(component, key) in controlGalleryComponents" :key="key" :class="{current: currentComponent.name === component.name}">
                        <a :href="'#' + key" @click="currentComponent = component">{{ convertComponentName(component.name) }}</a>
                    </li>
                </ul>

                <h4 class="mt-3">Templates</h4>

                <ul class="list-unstyled mb-0">
                    <li v-for="(component, key) in templateGalleryComponents" :key="key" :class="{current: currentComponent.name === component.name}">
                        <a :href="'#' + key" @click="currentComponent = component">{{ convertComponentName(component.name) }}</a>
                    </li>
                </ul>
            </div>

            <div class="galleryContent">
                <component :is="currentComponent" />
            </div>

        </div>
    </template>
</Panel>`
});
