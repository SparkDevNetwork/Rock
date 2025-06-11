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
 * - gridProfileLinkColumn
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

import { Component, computed, defineComponent, onMounted, onUnmounted, ref } from "vue";
import { convertComponentName, getTemplateImportPath } from "./ControlGallery/common/utils.partial";
import { getSecurityGrant, provideSecurityGrant, useConfigurationValues, onConfigurationValuesChanged, useReloadBlock } from "@Obsidian/Utility/block";
import { ControlGalleryInitializationBox } from "@Obsidian/ViewModels/Blocks/Example/ControlGallery/controlGalleryInitializationBox";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { sleep } from "@Obsidian/Utility/promiseUtils";
import { upperCaseFirstCharacter } from "@Obsidian/Utility/stringUtils";
import GalleryAndResult from "./ControlGallery/common/galleryAndResult.partial.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import Panel from "@Obsidian/Controls/panel.obs";
import DetailBlock from "@Obsidian/Templates/detailBlock";
import SectionHeader from "@Obsidian/Controls/sectionHeader.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import DropDownMenuGallery from "./ControlGallery/dropDownMenuGallery.partial.obs";
import DropDownContentGallery from "./ControlGallery/dropDownContentGallery.partial.obs";
import ButtonDropDownListGallery from "./ControlGallery/buttonDropDownListGallery.partial.obs";
import CampusAccountAmountPickerGallery from "./ControlGallery/campusAccountAmountPickerGallery.partial.obs";
import PersonPickerGallery from "./ControlGallery/personPickerGallery.partial.obs";
import ImageEditorGallery from "./ControlGallery/imageEditorGallery.partial.obs";
import HighlightLabelGallery from "./ControlGallery/highlightLabelGallery.partial.obs";
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
import DatePartsPickerGallery from "./ControlGallery/datePartsPickerGallery.partial.obs";
import FirstNameTextBoxGallery from "./ControlGallery/firstNameTextBoxGallery.partial.obs";
import ImageUploaderGallery from "./ControlGallery/imageUploaderGallery.partial.obs";
import MediaPlayerGallery from "./ControlGallery/mediaPlayerGallery.partial.obs";
import RadioButtonGallery from "./ControlGallery/radioButtonGallery.partial.obs";
import BulletedListGallery from "./ControlGallery/bulletedListGallery.partial.obs";
import TermDescriptionGallery from "./ControlGallery/termDescriptionGallery.partial.obs";
import ValueFilterGallery from "./ControlGallery/valueFilterGallery.partial.obs";
import SecurityButtonGallery from "./ControlGallery/securityButtonGallery.partial.obs";
import MarkdownEditorGallery from "./ControlGallery/markdownEditorGallery.partial.obs";
import JsonFieldsBuilderGallery from "./ControlGallery/jsonFieldsBuilderGallery.partial.obs";
import HtmlEditorGallery from "./ControlGallery/htmlEditorGallery.partial.obs";
import TextBoxGallery from "./ControlGallery/textBoxGallery.partial.obs";
import FileAssetManagerGallery from "./ControlGallery/fileAssetManagerGallery.partial.obs";
import AssetPickerGallery from "./ControlGallery/assetPickerGallery.partial.obs";
import CustomSelectGallery from "./ControlGallery/customSelectGallery.partial.obs";
import TabbedModalGallery from "./ControlGallery/tabbedModalGallery.partial.obs";
import CategoryTreeGallery from "./ControlGallery/categoryTreeGallery.partial.obs";
import PageNavButtonsGallery from "./ControlGallery/pageNavButtonsGallery.partial.obs";
import SearchFieldGallery from "./ControlGallery/searchFieldGallery.partial.obs";
import AttributeValuesContainerGallery from "./ControlGallery/attributeValuesContainerGallery.partial.obs";
import SocialSecurityNumberBoxGallery from "./ControlGallery/socialSecurityNumberBoxGallery.partial.obs";
import FieldFilterEditorGallery from "./ControlGallery/fieldFilterEditorGallery.partial.obs";
import PhoneNumberBoxGallery from "./ControlGallery/phoneNumberBoxGallery.partial.obs";
import HelpBlockGallery from "./ControlGallery/helpBlockGallery.partial.obs";
import DropDownListGallery from "./ControlGallery/dropDownListGallery.partial.obs";
import RadioButtonListGallery from "./ControlGallery/radioButtonListGallery.partial.obs";
import CheckBoxGallery from "./ControlGallery/checkBoxGallery.partial.obs";
import InlineCheckBoxGallery from "./ControlGallery/inlineCheckBoxGallery.partial.obs";
import DialogGallery from "./ControlGallery/dialogGallery.partial.obs";
import CheckBoxListGallery from "./ControlGallery/checkBoxListGallery.partial.obs";
import MediaSelectorGallery from "./ControlGallery/mediaSelectorGallery.partial.obs";
import ListItemsGallery from "./ControlGallery/listItemsGallery.partial.obs";
import ListBoxGallery from "./ControlGallery/listBoxGallery.partial.obs";
import DatePickerGallery from "./ControlGallery/datePickerGallery.partial.obs";
import DateRangePickerGallery from "./ControlGallery/dateRangePickerGallery.partial.obs";
import DateTimePickerGallery from "./ControlGallery/dateTimePickerGallery.partial.obs";
import ColorPickerGallery from "./ControlGallery/colorPickerGallery.partial.obs";
import NumberBoxGallery from "./ControlGallery/numberBoxGallery.partial.obs";
import NumberRangeBoxGallery from "./ControlGallery/numberRangeBoxGallery.partial.obs";
import GenderPickerGallery from "./ControlGallery/genderPickerGallery.partial.obs";
import TimePickerGallery from "./ControlGallery/timePickerGallery.partial.obs";
import RatingGallery from "./ControlGallery/ratingGallery.partial.obs";
import SwitchGallery from "./ControlGallery/switchGallery.partial.obs";
import InlineSwitchGallery from "./ControlGallery/inlineSwitchGallery.partial.obs";
import EmailBoxGallery from "./ControlGallery/emailBoxGallery.partial.obs";
import NumberUpDownGallery from "./ControlGallery/numberUpDownGallery.partial.obs";
import AddressControlGallery from "./ControlGallery/addressControlGallery.partial.obs";
import StaticFormControlGallery from "./ControlGallery/staticFormControlGallery.partial.obs";
import ToggleGallery from "./ControlGallery/toggleGallery.partial.obs";
import ProgressTrackerGallery from "./ControlGallery/progressTrackerGallery.partial.obs";
import ItemsWithPreAndPostHtmlGallery from "./ControlGallery/itemsWithPreAndPostHtmlGallery.partial.obs";
import UrlLinkBoxGallery from "./ControlGallery/urlLinkBoxGallery.partial.obs";
import FullscreenGallery from "./ControlGallery/fullscreenGallery.partial.obs";
import PanelGallery from "./ControlGallery/panelGallery.partial.obs";
import FileUploaderGallery from "./ControlGallery/fileUploaderGallery.partial.obs";
import SlidingDateRangePickerGallery from "./ControlGallery/slidingDateRangePickerGallery.partial.obs";
import DefinedValuePickerGallery from "./ControlGallery/definedValuePickerGallery.partial.obs";
import EntityTypePickerGallery from "./ControlGallery/entityTypePickerGallery.partial.obs";
import AchievementTypePickerGallery from "./ControlGallery/achievementTypePickerGallery.partial.obs";
import BadgeComponentPickerGallery from "./ControlGallery/badgeComponentPickerGallery.partial.obs";
import SectionHeaderGallery from "./ControlGallery/sectionHeaderGallery.partial.obs";
import SectionContainerGallery from "./ControlGallery/sectionContainerGallery.partial.obs";
import CategoryPickerGallery from "./ControlGallery/categoryPickerGallery.partial.obs";
import LocationItemPickerGallery from "./ControlGallery/locationItemPickerGallery.partial.obs";
import ConnectionRequestPickerGallery from "./ControlGallery/connectionRequestPickerGallery.partial.obs";
import CopyButtonGallery from "./ControlGallery/copyButtonGallery.partial.obs";
import TagListGallery from "./ControlGallery/tagListGallery.partial.obs";
import FollowingGallery from "./ControlGallery/followingGallery.partial.obs";
import AssessmentTypePickerGallery from "./ControlGallery/assessmentTypePickerGallery.partial.obs";
import AssetStorageProviderPickerGallery from "./ControlGallery/assetStorageProviderPickerGallery.partial.obs";
import BinaryFileTypePickerGallery from "./ControlGallery/binaryFileTypePickerGallery.partial.obs";
import CampusPickerGallery from "./ControlGallery/campusPickerGallery.partial.obs";
import ScheduleBuilderGallery from "./ControlGallery/scheduleBuilderGallery.partial.obs";
import BinaryFilePickerGallery from "./ControlGallery/binaryFilePickerGallery.partial.obs";
import EventItemPickerGallery from "./ControlGallery/eventItemPickerGallery.partial.obs";
import DataViewPickerGallery from "./ControlGallery/dataViewPickerGallery.partial.obs";
import WorkflowTypePickerGallery from "./ControlGallery/workflowTypePickerGallery.partial.obs";
import FinancialGatewayPickerGallery from "./ControlGallery/financialGatewayPickerGallery.partial.obs";
import FinancialStatementTemplatePickerGallery from "./ControlGallery/financialStatementTemplatePickerGallery.partial.obs";
import FieldTypePickerGallery from "./ControlGallery/fieldTypePickerGallery.partial.obs";
import AuditDetailGallery from "./ControlGallery/auditDetailGallery.partial.obs";
import ModalGallery from "./ControlGallery/modalGallery.partial.obs";
import ComponentPickerGallery from "./ControlGallery/componentPickerGallery.partial.obs";
import GradePickerGallery from "./ControlGallery/gradePickerGallery.partial.obs";
import GroupMemberPickerGallery from "./ControlGallery/groupMemberPickerGallery.partial.obs";
import InteractionChannelPickerGallery from "./ControlGallery/interactionChannelPickerGallery.partial.obs";
import LavaCommandPickerGallery from "./ControlGallery/lavaCommandPickerGallery.partial.obs";
import RemoteAuthsPickerGallery from "./ControlGallery/remoteAuthsPickerGallery.partial.obs";
import StepProgramPickerGallery from "./ControlGallery/stepProgramPickerGallery.partial.obs";
import StepProgramStepTypePickerGallery from "./ControlGallery/stepProgramStepTypePickerGallery.partial.obs";
import StepProgramStepStatusPickerGallery from "./ControlGallery/stepProgramStepStatusPickerGallery.partial.obs";
import StepStatusPickerGallery from "./ControlGallery/stepStatusPickerGallery.partial.obs";
import StepTypePickerGallery from "./ControlGallery/stepTypePickerGallery.partial.obs";
import StreakTypePickerGallery from "./ControlGallery/streakTypePickerGallery.partial.obs";
import BadgePickerGallery from "./ControlGallery/badgePickerGallery.partial.obs";
import NotificationBoxGallery from "./ControlGallery/notificationBoxGallery.partial.obs";
import BadgeListGallery from "./ControlGallery/badgeListGallery.partial.obs";
import BasicTimePickerGallery from "./ControlGallery/basicTimePickerGallery.partial.obs";
import BirthdayPickerGallery from "./ControlGallery/birthdayPickerGallery.partial.obs";
import CountdownTimerGallery from "./ControlGallery/countdownTimerGallery.partial.obs";
import ElectronicSignatureGallery from "./ControlGallery/electronicSignatureGallery.partial.obs";
import FieldTypeEditorGallery from "./ControlGallery/fieldTypeEditorGallery.partial.obs";
import InlineRangeSliderGallery from "./ControlGallery/inlineRangeSliderGallery.partial.obs";
import JavaScriptAnchorGallery from "./ControlGallery/javascriptAnchorGallery.partial.obs";
import LoadingGallery from "./ControlGallery/loadingGallery.partial.obs";
import LoadingIndicatorGallery from "./ControlGallery/loadingIndicatorGallery.partial.obs";
import NumberUpDownGroupGallery from "./ControlGallery/numberUpDownGroupGallery.partial.obs";
import ProgressBarGallery from "./ControlGallery/progressBarGallery.partial.obs";
import RockButtonGallery from "./ControlGallery/rockButtonGallery.partial.obs";
import RockLabelGallery from "./ControlGallery/rockLabelGallery.partial.obs";
import RockValidationGallery from "./ControlGallery/rockValidationGallery.partial.obs";
import RangeSliderGallery from "./ControlGallery/rangeSliderGallery.partial.obs";
import TabbedBarGallery from "./ControlGallery/tabbedBarGallery.partial.obs";
import InteractionComponentPickerGallery from "./ControlGallery/interactionComponentPickerGallery.partial.obs";
import TabbedContentGallery from "./ControlGallery/tabbedContentGallery.partial.obs";
import TransitionVerticalCollapseGallery from "./ControlGallery/transitionVerticalCollapseGallery.partial.obs";
import ValueDetailListGallery from "./ControlGallery/valueDetailListGallery.partial.obs";
import CodeEditorGallery from "./ControlGallery/codeEditorGallery.partial.obs";
import PagePickerGallery from "./ControlGallery/pagePickerGallery.partial.obs";
import GroupPickerGallery from "./ControlGallery/groupPickerGallery.partial.obs";
import MergeTemplatePickerGallery from "./ControlGallery/mergeTemplatePickerGallery.partial.obs";
import MetricCategoryPickerGallery from "./ControlGallery/metricCategoryPickerGallery.partial.obs";
import MetricItemPickerGallery from "./ControlGallery/metricItemPickerGallery.partial.obs";
import RegistrationTemplatePickerGallery from "./ControlGallery/registrationTemplatePickerGallery.partial.obs";
import ReportPickerGallery from "./ControlGallery/reportPickerGallery.partial.obs";
import SchedulePickerGallery from "./ControlGallery/schedulePickerGallery.partial.obs";
import WorkflowActionTypePickerGallery from "./ControlGallery/workflowActionTypePickerGallery.partial.obs";
import DayOfWeekPickerGallery from "./ControlGallery/dayOfWeekPickerGallery.partial.obs";
import MonthDayPickerGallery from "./ControlGallery/monthDayPickerGallery.partial.obs";
import MonthYearPickerGallery from "./ControlGallery/monthYearPickerGallery.partial.obs";
import CacheabilityPickerGallery from "./ControlGallery/cacheabilityPickerGallery.partial.obs";
import ButtonGroupGallery from "./ControlGallery/buttonGroupGallery.partial.obs";
import IntervalPickerGallery from "./ControlGallery/intervalPickerGallery.partial.obs";
import GeoPickerGallery from "./ControlGallery/geoPickerGallery.partial.obs";
import ContentDropDownPickerGallery from "./ControlGallery/contentDropDownPickerGallery.partial.obs";
import WordCloudGallery from "./ControlGallery/wordCloudGallery.partial.obs";
import EventCalendarPickerGallery from "./ControlGallery/eventCalendarPickerGallery.partial.obs";
import GroupTypePickerGallery from "./ControlGallery/groupTypePickerGallery.partial.obs";
import LocationAddressPickerGallery from "./ControlGallery/locationAddressPickerGallery.partial.obs";
import LocationPickerGallery from "./ControlGallery/locationPickerGallery.partial.obs";
import LocationListGallery from "./ControlGallery/locationListGallery.partial.obs";
import EthnicityPickerGallery from "./ControlGallery/ethnicityPickerGallery.partial.obs";
import RacePickerGallery from "./ControlGallery/racePickerGallery.partial.obs";
import MediaElementPickerGallery from "./ControlGallery/mediaElementPickerGallery.partial.obs";
import MergeFieldPickerGallery from "./ControlGallery/mergeFieldPickerGallery.partial.obs";
import CategorizedValuePickerGallery from "./ControlGallery/categorizedValuePickerGallery.partial.obs";
import ReminderTypePickerGallery from "./ControlGallery/reminderTypePickerGallery.partial.obs";
import GroupRolePickerGallery from "./ControlGallery/groupRolePickerGallery.partial.obs";
import ModalAlertGallery from "./ControlGallery/modalAlertGallery.partial.obs";
import ContentChannelItemPickerGallery from "./ControlGallery/contentChannelItemPickerGallery.partial.obs";
import PersonLinkGallery from "./ControlGallery/personLinkGallery.partial.obs";
import PopOverGallery from "./ControlGallery/popOverGallery.partial.obs";
import RockLiteralGallery from "./ControlGallery/rockLiteralGallery.partial.obs";
import RegistryEntryGallery from "./ControlGallery/registryEntryGallery.partial.obs";
import GroupTypeGroupPickerGallery from "./ControlGallery/groupTypeGroupPickerGallery.partial.obs";
import GroupAndRolePickerGallery from "./ControlGallery/groupAndRolePickerGallery.partial.obs";
import AccountPickerGallery from "./ControlGallery/accountPickerGallery.partial.obs";
import NoteTextEditorGallery from "./ControlGallery/noteTextEditorGallery.partial.obs";
import StructuredContentEditorGallery from "./ControlGallery/structuredContentEditorGallery.partial.obs";
import RegistrationInstancePickerGallery from "./ControlGallery/registrationInstancePickerGallery.partial.obs";
import InteractionChannelInteractionComponentPickerGallery from "./ControlGallery/interactionChannelInteractionComponentPickerGallery.partial.obs";
import WorkflowPickerGallery from "./ControlGallery/workflowPickerGallery.partial.obs";
import ValueListGallery from "./ControlGallery/valueListGallery.partial.obs";
import BlockTemplatePickerGallery from "./ControlGallery/blockTemplatePickerGallery.partial.obs";
import AdaptiveMessagePickerGallery from "./ControlGallery/adaptiveMessagePickerGallery.partial.obs";
import EmailEditorGallery from "./ControlGallery/emailEditorGallery.partial.obs";
import KpiGallery from "./ControlGallery/kpiGallery.partial.obs";
import LearningClassPickerGallery from "./ControlGallery/learningClassPickerGallery.partial.obs";
import LearningClassActivityPickerGallery from "./ControlGallery/learningClassActivityPickerGallery.partial.obs";
import ConnectedListAddButtonGallery from "./ControlGallery/connectedListAddButtonGallery.partial.obs";
import ConnectedListButtonGallery from "./ControlGallery/connectedListButtonGallery.partial.obs";
import ConnectedListGallery from "./ControlGallery/connectedListGallery.partial.obs";
import IconPickerGallery from "./ControlGallery/iconPickerGallery.partial.obs";

const controlGalleryComponents: Record<string, Component> = [
    NotificationBoxGallery,
    AttributeValuesContainerGallery,
    BadgeListGallery,
    FieldFilterEditorGallery,
    DatePickerGallery,
    DateRangePickerGallery,
    DateTimePickerGallery,
    DatePartsPickerGallery,
    RadioButtonListGallery,
    DialogGallery,
    CheckBoxGallery,
    InlineCheckBoxGallery,
    SwitchGallery,
    InlineSwitchGallery,
    CheckBoxListGallery,
    MediaSelectorGallery,
    ListBoxGallery,
    ListItemsGallery,
    PhoneNumberBoxGallery,
    DropDownListGallery,
    HelpBlockGallery,
    ColorPickerGallery,
    NumberBoxGallery,
    NumberRangeBoxGallery,
    GenderPickerGallery,
    SocialSecurityNumberBoxGallery,
    TimePickerGallery,
    RatingGallery,
    CurrencyBoxGallery,
    EmailBoxGallery,
    NumberUpDownGallery,
    StaticFormControlGallery,
    AddressControlGallery,
    ToggleGallery,
    ProgressTrackerGallery,
    ItemsWithPreAndPostHtmlGallery,
    UrlLinkBoxGallery,
    FullscreenGallery,
    PanelGallery,
    PersonPickerGallery,
    FileUploaderGallery,
    ImageUploaderGallery,
    SlidingDateRangePickerGallery,
    DefinedValuePickerGallery,
    CampusPickerGallery,
    EntityTypePickerGallery,
    SectionHeaderGallery,
    SectionContainerGallery,
    CategoryPickerGallery,
    LocationItemPickerGallery,
    CopyButtonGallery,
    TagListGallery,
    FollowingGallery,
    AchievementTypePickerGallery,
    BadgeComponentPickerGallery,
    AssessmentTypePickerGallery,
    AssetStorageProviderPickerGallery,
    AuditDetailGallery,
    BinaryFileTypePickerGallery,
    BinaryFilePickerGallery,
    CodeEditorGallery,
    ModalGallery,
    EventItemPickerGallery,
    DataViewPickerGallery,
    WorkflowTypePickerGallery,
    ComponentPickerGallery,
    FinancialGatewayPickerGallery,
    FinancialStatementTemplatePickerGallery,
    FieldTypePickerGallery,
    GradePickerGallery,
    GroupMemberPickerGallery,
    InteractionChannelPickerGallery,
    InteractionComponentPickerGallery,
    LavaCommandPickerGallery,
    RemoteAuthsPickerGallery,
    StepProgramPickerGallery,
    StepProgramStepTypePickerGallery,
    StepProgramStepStatusPickerGallery,
    StepStatusPickerGallery,
    StepTypePickerGallery,
    StreakTypePickerGallery,
    BadgePickerGallery,
    BasicTimePickerGallery,
    BirthdayPickerGallery,
    CountdownTimerGallery,
    ElectronicSignatureGallery,
    FieldTypeEditorGallery,
    InlineRangeSliderGallery,
    JavaScriptAnchorGallery,
    KeyValueListGallery,
    LoadingGallery,
    LoadingIndicatorGallery,
    NumberUpDownGroupGallery,
    ProgressBarGallery,
    RockButtonGallery,
    RockLabelGallery,
    RockValidationGallery,
    RangeSliderGallery,
    TabbedBarGallery,
    TabbedContentGallery,
    TransitionVerticalCollapseGallery,
    ValueDetailListGallery,
    PagePickerGallery,
    ConnectionRequestPickerGallery,
    GroupPickerGallery,
    MergeTemplatePickerGallery,
    MetricCategoryPickerGallery,
    MetricItemPickerGallery,
    RegistrationTemplatePickerGallery,
    ReportPickerGallery,
    SchedulePickerGallery,
    WorkflowActionTypePickerGallery,
    DayOfWeekPickerGallery,
    MonthDayPickerGallery,
    MonthYearPickerGallery,
    CacheabilityPickerGallery,
    ButtonGroupGallery,
    IntervalPickerGallery,
    GeoPickerGallery,
    ContentDropDownPickerGallery,
    ScheduleBuilderGallery,
    WordCloudGallery,
    EventCalendarPickerGallery,
    GroupTypePickerGallery,
    LocationAddressPickerGallery,
    LocationPickerGallery,
    LocationListGallery,
    EthnicityPickerGallery,
    RacePickerGallery,
    MediaElementPickerGallery,
    MergeFieldPickerGallery,
    CategorizedValuePickerGallery,
    ReminderTypePickerGallery,
    GroupRolePickerGallery,
    ModalAlertGallery,
    ContentChannelItemPickerGallery,
    PersonLinkGallery,
    PopOverGallery,
    RockLiteralGallery,
    RegistryEntryGallery,
    GroupTypeGroupPickerGallery,
    GroupAndRolePickerGallery,
    AccountPickerGallery,
    NoteTextEditorGallery,
    StructuredContentEditorGallery,
    RegistrationInstancePickerGallery,
    InteractionChannelInteractionComponentPickerGallery,
    WorkflowPickerGallery,
    ValueListGallery,
    BlockTemplatePickerGallery,
    DropDownMenuGallery,
    DropDownContentGallery,
    ButtonDropDownListGallery,
    CampusAccountAmountPickerGallery,
    LightGridGallery,
    ImageEditorGallery,
    HighlightLabelGallery,
    PdfViewerGallery,
    ChartGallery,
    EntityPickerGallery,
    PersonBasicEditorGallery,
    AttributeMatrixEditorGallery,
    BadgeControlGallery,
    BadgeGallery,
    WarningBlockGallery,
    YearPickerGallery,
    FirstNameTextBoxGallery,
    MediaPlayerGallery,
    RadioButtonGallery,
    BulletedListGallery,
    TermDescriptionGallery,
    ValueFilterGallery,
    SecurityButtonGallery,
    MarkdownEditorGallery,
    JsonFieldsBuilderGallery,
    HtmlEditorGallery,
    TextBoxGallery,
    FileAssetManagerGallery,
    AssetPickerGallery,
    CustomSelectGallery,
    TabbedModalGallery,
    CategoryTreeGallery,
    PageNavButtonsGallery,
    SearchFieldGallery,
    AdaptiveMessagePickerGallery,
    EmailEditorGallery,
    KpiGallery,
    LearningClassPickerGallery,
    LearningClassActivityPickerGallery,
    ConnectedListAddButtonGallery,
    ConnectedListButtonGallery,
    ConnectedListGallery,
    IconPickerGallery,
]
    // Fix vue 3 SFC putting name in __name.
    .map(a => {
        a.name = upperCaseFirstCharacter((a.__name ?? a.name!).replace(/\.partial$/, ""));
        return a;
    })
    // Sort list by component name
    .sort((a, b) => a.name!.localeCompare(b.name!))
    // Convert list to an object where the key is the component name and the value is the component
    .reduce((newList, comp) => {
        newList[comp.name!] = comp;
        return newList;
    }, {});

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
            delayedHandler: async () => {
                await sleep(1000);
                return true;
            },
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
        :isTagsVisible="isTagsVisible"
        @save="delayedHandler"
        @edit="delayedHandler"
        @delete="delayedHandler">
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
    .map(a => {
        a.name = a.__name ?? a.name;
        return a;
    })
    .sort((a, b) => a.name!.localeCompare(b.name!))
    .reduce((newList, comp) => {
        newList[comp.name!] = comp;
        return newList;
    }, {});

const inputSizingGallery = defineComponent({
    name: "InputSizingRulesGallery",
    components: {
        GalleryAndResult,
        TextBox
    },
    setup() {
        return {
            exampleCode: `
<TextBox class="input-width-xs" label=".input-width-xs" />
<TextBox class="input-width-sm" label=".input-width-sm" />
<TextBox class="input-width-md" label=".input-width-md" />
<TextBox class="input-width-lg" label=".input-width-lg" />
<TextBox class="input-width-xl" label=".input-width-xl" />
<TextBox class="input-width-xxl" label=".input-width-xxl" />`
        };
    },
    template: `
<GalleryAndResult
    :exampleCode="exampleCode">
    <div class="alert alert-warning">
        <p><strong>Warning!</strong></p>
        In Bootstrap 3 inputs are meant to fill the width of their parent container (<a href="http://getbootstrap.com/css/#forms-control-sizes" class="alert-link">link</a>).  If a small input is desired they should
    be wrapped in a table grid.  This provides the best responsive solution.  In some rare cases it's beneficial to be able to fix the width of
    certain inputs to provide better context of what the input is for.  For instance a credit card CVV field makes more sense visually being
    fixed width to 3 characters.  To provide this capability we have added the following CSS classes to fix width inputs.  <em>Please use them
    sparingly.</em>
    </div>

    <div class="alert alert-danger">
        <p><strong>Alert</strong></p>
        Rock framework developers should get approval from the Core Team before using these styles.
    </div>

    <div>
        <TextBox class="input-width-xs" label=".input-width-xs" />
        <TextBox class="input-width-sm" label=".input-width-sm" />
        <TextBox class="input-width-md" label=".input-width-md" />
        <TextBox class="input-width-lg" label=".input-width-lg" />
        <TextBox class="input-width-xl" label=".input-width-xl" />
        <TextBox class="input-width-xxl" label=".input-width-xxl" />
    </div>

    <div class="alert alert-info">
        <p><strong>Note</strong></p>
        In Bootstrap 3 inputs are <em>display:block;</em>. If you need these sized controls to align horizontally, consider wrapping them with the <em>form-control-group</em> class.
    </div>
</GalleryAndResult>`
});

const horizontalFormsGallery = defineComponent({
    name: "HorizontalFormsGallery",
    components: {
        GalleryAndResult
    },
    setup() {
        return {
            exampleCode: `
<div class="form-horizontal label-sm">
    <div class="form-group">
        <label for="inputEmail4" class="control-label">Email</label>
        <div class="control-wrapper">
            <input type="email" class="form-control" id="inputEmail4" placeholder="Email">
        </div>
    </div>
</div>

<div class="form-horizontal label-md">
    <div class="form-group">
        <label for="inputEmail5" class="control-label">Email</label>
        <div class="control-wrapper">
            <input type="email" class="form-control" id="inputEmail5" placeholder="Email">
        </div>
    </div>
</div>

<div class="form-horizontal label-lg">
    <div class="form-group">
        <label for="inputEmail6" class="control-label">Email</label>
        <div class="control-wrapper">
            <input type="email" class="form-control" id="inputEmail6" placeholder="Email">
        </div>
    </div>
</div>

<div class="form-horizontal label-xl">
    <div class="form-group">
        <label for="inputEmail7" class="control-label">Email</label>
        <div class="control-wrapper">
            <input type="email" class="form-control" id="inputEmail7" placeholder="Email">
        </div>
    </div>
</div>

<div class="form-horizontal label-auto">
    <div class="form-group">
        <label for="inputEmail8" class="control-label">Email Email Email Email Email Email Email Email Email Email Email</label>
        <div class="control-wrapper">
            <input type="email" class="form-control" id="inputEmail8" placeholder="Email">
        </div>
    </div>
</div>`
        };
    },
    template: `
<GalleryAndResult
    :exampleCode="exampleCode">
    <div class="alert alert-info">
       <p><strong>Note</strong></p>
       In Bootstrap 3 inputs are <em>display:block;</em>. If you need these sized controls to align horizontally, consider wrapping them with the <em>form-control-group</em> class.
    </div>

    <h2 runat="server">Horizontal Forms</h2>
    <p>While Rock uses a similar approach to Bootstrap, we’ve made horizontal forms a bit easier to help facilitate their use when creating forms in workflows and event
        registrations. Below is the syntax for declaring a horizontal form.
    </p>
    <div>
        <div class="form-horizontal label-sm">
            <div class="form-group">
                <label for="inputEmail3" class="control-label">Email</label>
                <div class="control-wrapper">
                <input type="email" class="form-control" id="inputEmail3" placeholder="Email">
                </div>
            </div>
        </div>
    </div>

    <p>When using this in form generators you'll need to complete two steps. The first is adding a wrapping <code>&lt;div class=&quot;form-group &quot;&gt;</code> in your pre/post fields.</p>

    <p>The second is an additional class on the form-horizontal element that determines how wide the label column should be. Options include:</p>

    <ul>
        <li><strong>label-sm: </strong> Label column of 2, field column of 10</li>
        <li><strong>label-md: </strong> Label column of 4, field column of 8</li>
        <li><strong>label-lg: </strong> Label column of 6, field column of 6</li>
        <li><strong>label-xl: </strong> Label column of 8, field column of 4</li>
        <li><strong>label-auto: </strong> Label and field widths determined by contents</li>
    </ul>

    <div runat="server" class="r-example">
        <div class="form-horizontal label-sm">
            <div class="form-group">
                <label for="inputEmail4" class="control-label">Email</label>
                <div class="control-wrapper">
                    <input type="email" class="form-control" id="inputEmail4" placeholder="Email">
                </div>
            </div>
        </div>

        <div class="form-horizontal label-md">
            <div class="form-group">
                <label for="inputEmail5" class="control-label">Email</label>
                <div class="control-wrapper">
                    <input type="email" class="form-control" id="inputEmail5" placeholder="Email">
                </div>
            </div>
        </div>

        <div class="form-horizontal label-lg">
            <div class="form-group">
                <label for="inputEmail6" class="control-label">Email</label>
                <div class="control-wrapper">
                    <input type="email" class="form-control" id="inputEmail6" placeholder="Email">
                </div>
            </div>
        </div>

        <div class="form-horizontal label-xl">
            <div class="form-group">
                <label for="inputEmail7" class="control-label">Email</label>
                <div class="control-wrapper">
                    <input type="email" class="form-control" id="inputEmail7" placeholder="Email">
                </div>
            </div>
        </div>

        <div class="form-horizontal label-auto">
            <div class="form-group">
                <label for="inputEmail8" class="control-label">Email Email Email Email Email Email Email Email Email Email Email</label>
                <div class="control-wrapper">
                    <input type="email" class="form-control" id="inputEmail8" placeholder="Email">
                </div>
            </div>
        </div>

    </div>
</GalleryAndResult>`
});

const marginsAndPaddingGallery = defineComponent({
    name: "MarginsAndPaddingGallery",
    components: {
        GalleryAndResult,
        TextBox,
        NotificationBox
    },
    setup() {
        return {
            exampleCode: `
<div class="well">
    <TextBox class="margin-t-xl" label=".margin-t-xl" placeholder="Blah..."/>
</div>

<div class="well">
    <TextBox class="padding-h-lg" label=".padding-h-lg" placeholder="Blah..." />
</div>

<div class="well">
    <label class="control-label">.padding-all-xl .margin-all-lg</label>
    <NotificationBox class="padding-all-xl margin-all-lg" alertType="info" heading=".padding-all-xl .margin-all-md" text="For God so loved the world that he gave his one and only Son..." />
</div>`
        };
    },
    template: `
<GalleryAndResult
    :exampleCode="exampleCode">
    <h2>Margins and Padding</h2>

    <div class="alert alert-warning">
        <p><strong>Warning!</strong></p>
        If you think you need to control the margin or padding, you might be 'doing it wrong.'
        <em>These are for use in those cases when you know what you're doing.</em>
    </div>

    <h3>Format</h3>
    <p>
        The format is the type (padding or margin) followed by a dash then the position (v=vertical, h=horizontal, t=top, etc.)
        followed by a dash and then the sizing specifier (none, small, medium, etc).
    </p>
    <pre>.padding|margin - v|h|t|b|r|l|all - none|sm|md|lg|xl</pre>

    <div>
        <div class="well">
            <TextBox class="margin-t-xl" label=".margin-t-xl" placeholder="Blah..."/>
        </div>

        <div class="well">
            <TextBox class="padding-h-lg" label=".padding-h-lg" placeholder="Blah..." />
        </div>

        <div class="well">
            <label class="control-label">.padding-all-xl .margin-all-lg</label>
            <NotificationBox class="padding-all-xl margin-all-lg" alertType="info" heading=".padding-all-xl .margin-all-md" text="For God so loved the world that he gave his one and only Son..." />
        </div>
    </div>
</GalleryAndResult>`
});

const fieldLabelsGallery = defineComponent({
    name: "FieldLabelsGallery",
    components: {
        GalleryAndResult,
        TextBox,
        NotificationBox
    },
    setup() {
        return {
            exampleCode: `
<div class="form-group">
    <label class="control-label">Group</label>
    <div class="control-wrapper">
        <p>A/V Team</p>
    </div>
</div>`
        };
    },
    template: `
<GalleryAndResult
    :exampleCode="exampleCode">
    <h2 runat="server">Field Labels</h2>
    <p>When a field is not editable because it was pre-configured via configuration or block setting,
        it should still have a typical look by using the <code>form-group, control-label, control-wrapper</code>
        classes.
    </p>

    <div runat="server" class="r-example">
        <div class="form-group">
            <label class="control-label">Group</label>
            <div class="control-wrapper">
                <p>A/V Team</p>
            </div>
        </div>
    </div>
</GalleryAndResult>`
});

const generalInformationGalleryComponents = [
    inputSizingGallery,
    horizontalFormsGallery,
    marginsAndPaddingGallery,
    fieldLabelsGallery
]
    .map(a => {
        a.name = a.__name ?? a.name;
        return a;
    })
    .sort((a, b) => a.name!.localeCompare(b.name!))
    .reduce((newList, comp) => {
        newList[comp.name!] = comp;
        return newList;
    }, {});

// #endregion

export default defineComponent({
    name: "Example.ControlGallery",
    components: {
        Panel,
        SectionHeader,
        ...controlGalleryComponents,
        ...templateGalleryComponents,
        ...generalInformationGalleryComponents
    },

    setup() {
        const config = useConfigurationValues<ControlGalleryInitializationBox>();
        const securityGrant = getSecurityGrant(config.securityGrantToken);
        provideSecurityGrant(securityGrant);

        onConfigurationValuesChanged(useReloadBlock());

        const currentComponent = ref<Component>(Object.values(controlGalleryComponents)[0]);

        function getComponentFromHash(): void {
            const hashComponent = new URL(document.URL).hash.replace("#", "");

            if (!hashComponent) {
                return;
            }

            const component = controlGalleryComponents[hashComponent] ?? templateGalleryComponents[hashComponent] ?? generalInformationGalleryComponents[hashComponent];

            if (component) {
                currentComponent.value = component;
            }
        }

        getComponentFromHash();

        onMounted(() => {
            window.addEventListener("hashchange", getComponentFromHash);
        });

        onUnmounted(() => {
            window.removeEventListener("hashchange", getComponentFromHash);
        });

        return {
            currentComponent,
            convertComponentName,
            controlGalleryComponents,
            templateGalleryComponents,
            generalInformationGalleryComponents
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

@media only screen and (max-width: 480px) {
    .galleryContainer {
        flex-direction: column !important;
    }

    .gallerySidebar {
        max-height: 150px;
    }
}
</v-style>
<Panel type="block">
    <template #title>
        Obsidian Control Gallery
    </template>
    <template #default>
        <div class="panel-flex-fill-body flex-row galleryContainer">

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

                <h4 class="mt-3">General Information</h4>
                <ul class="list-unstyled mb-0">
                    <li v-for="(component, key) in generalInformationGalleryComponents" :key="key" :class="{current: currentComponent.name === component.name}">
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
