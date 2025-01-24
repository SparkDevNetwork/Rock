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

import { Component, computed, defineComponent, onMounted, onUnmounted, ref, watch, watchEffect } from "vue";
import { convertComponentName, getSfcControlImportPath, getTemplateImportPath, displayStyleItems } from "./ControlGallery/common/utils.partial";
import { getSecurityGrant, provideSecurityGrant, useConfigurationValues, onConfigurationValuesChanged, useReloadBlock } from "@Obsidian/Utility/block";
import { ControlGalleryInitializationBox } from "@Obsidian/ViewModels/Blocks/Example/ControlGallery/controlGalleryInitializationBox";
import GalleryAndResult from "./ControlGallery/common/galleryAndResult.partial.obs";
import { BtnType } from "@Obsidian/Enums/Controls/btnType";
import { BtnSize } from "@Obsidian/Enums/Controls/btnSize";
import { Guid } from "@Obsidian/Types";
import TextBox from "@Obsidian/Controls/textBox.obs";
import NumberUpDown from "@Obsidian/Controls/numberUpDown.obs";
import Toggle from "@Obsidian/Controls/toggle.obs";
import RockForm from "@Obsidian/Controls/rockForm.obs";
import RockButton from "@Obsidian/Controls/rockButton.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import InlineCheckBox from "@Obsidian/Controls/inlineCheckBox.obs";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import Panel from "@Obsidian/Controls/panel.obs";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker.obs";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker.obs";
import DetailBlock from "@Obsidian/Templates/detailBlock";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { toGuidOrNull } from "@Obsidian/Utility/guid";
import { DefinedType } from "@Obsidian/SystemGuids/definedType";
import { DefinedValue } from "@Obsidian/SystemGuids/definedValue";
import { EntityType } from "@Obsidian/SystemGuids/entityType";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { sleep } from "@Obsidian/Utility/promiseUtils";
import { upperCaseFirstCharacter } from "@Obsidian/Utility/stringUtils";
import SectionHeader from "@Obsidian/Controls/sectionHeader.obs";
import { PickerDisplayStyle } from "@Obsidian/Enums/Controls/pickerDisplayStyle";
import WorkflowTypePicker from "@Obsidian/Controls/workflowTypePicker.obs";
import InteractionChannelPicker from "@Obsidian/Controls/interactionChannelPicker.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import RegistrationTemplatePicker from "@Obsidian/Controls/registrationTemplatePicker.obs";
import { RockCacheabilityBag } from "@Obsidian/ViewModels/Controls/rockCacheabilityBag";
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
import attributeValuesContainerGallery from "./ControlGallery/attributeValuesContainerGallery.partial.obs";
import socialSecurityNumberBoxGallery from "./ControlGallery/socialSecurityNumberBoxGallery.partial.obs";
import fieldFilterEditorGallery from "./ControlGallery/fieldFilterEditorGallery.partial.obs";
import phoneNumberBoxGallery from "./ControlGallery/phoneNumberBoxGallery.partial.obs";
import helpBlockGallery from "./ControlGallery/helpBlockGallery.partial.obs";
import dropDownListGallery from "./ControlGallery/dropDownListGallery.partial.obs";
import radioButtonListGallery from "./ControlGallery/radioButtonListGallery.partial.obs";
import checkBoxGallery from "./ControlGallery/checkBoxGallery.partial.obs";
import inlineCheckBoxGallery from "./ControlGallery/inlineCheckBoxGallery.partial.obs";
import dialogGallery from "./ControlGallery/dialogGallery.partial.obs";
import checkBoxListGallery from "./ControlGallery/checkBoxListGallery.partial.obs";
import mediaSelectorGallery from "./ControlGallery/mediaSelectorGallery.partial.obs";
import listItemsGallery from "./ControlGallery/listItemsGallery.partial.obs";
import listBoxGallery from "./ControlGallery/listBoxGallery.partial.obs";
import datePickerGallery from "./ControlGallery/datePickerGallery.partial.obs";
import dateRangePickerGallery from "./ControlGallery/dateRangePickerGallery.partial.obs";
import dateTimePickerGallery from "./ControlGallery/dateTimePickerGallery.partial.obs";
import colorPickerGallery from "./ControlGallery/colorPickerGallery.partial.obs";
import numberBoxGallery from "./ControlGallery/numberBoxGallery.partial.obs";
import numberRangeBoxGallery from "./ControlGallery/numberRangeBoxGallery.partial.obs";
import genderPickerGallery from "./ControlGallery/genderPickerGallery.partial.obs";
import timePickerGallery from "./ControlGallery/timePickerGallery.partial.obs";
import ratingGallery from "./ControlGallery/ratingGallery.partial.obs";
import switchGallery from "./ControlGallery/switchGallery.partial.obs";
import inlineSwitchGallery from "./ControlGallery/inlineSwitchGallery.partial.obs";
import emailBoxGallery from "./ControlGallery/emailBoxGallery.partial.obs";
import numberUpDownGallery from "./ControlGallery/numberUpDownGallery.partial.obs";
import addressControlGallery from "./ControlGallery/addressControlGallery.partial.obs";
import staticFormControlGallery from "./ControlGallery/staticFormControlGallery.partial.obs";
import toggleGallery from "./ControlGallery/toggleGallery.partial.obs";
import progressTrackerGallery from "./ControlGallery/progressTrackerGallery.partial.obs";
import itemsWithPreAndPostHtmlGallery from "./ControlGallery/itemsWithPreAndPostHtmlGallery.partial.obs";
import urlLinkBoxGallery from "./ControlGallery/urlLinkBoxGallery.partial.obs";
import fullscreenGallery from "./ControlGallery/fullscreenGallery.partial.obs";
import panelGallery from "./ControlGallery/panelGallery.partial.obs";
import fileUploaderGallery from "./ControlGallery/fileUploaderGallery.partial.obs";
import slidingDateRangePickerGallery from "./ControlGallery/slidingDateRangePickerGallery.partial.obs";
import definedValuePickerGallery from "./ControlGallery/definedValuePickerGallery.partial.obs";
import entityTypePickerGallery from "./ControlGallery/entityTypePickerGallery.partial.obs";
import achievementTypePickerGallery from "./ControlGallery/achievementTypePickerGallery.partial.obs";
import badgeComponentPickerGallery from "./ControlGallery/badgeComponentPickerGallery.partial.obs";
import sectionHeaderGallery from "./ControlGallery/sectionHeaderGallery.partial.obs";
import sectionContainerGallery from "./ControlGallery/sectionContainerGallery.partial.obs";
import categoryPickerGallery from "./ControlGallery/categoryPickerGallery.partial.obs";
import locationItemPickerGallery from "./ControlGallery/locationItemPickerGallery.partial.obs";
import connectionRequestPickerGallery from "./ControlGallery/connectionRequestPickerGallery.partial.obs";
import copyButtonGallery from "./ControlGallery/copyButtonGallery.partial.obs";
import tagListGallery from "./ControlGallery/tagListGallery.partial.obs";
import followingGallery from "./ControlGallery/followingGallery.partial.obs";
import assessmentTypePickerGallery from "./ControlGallery/assessmentTypePickerGallery.partial.obs";
import assetStorageProviderPickerGallery from "./ControlGallery/assetStorageProviderPickerGallery.partial.obs";
import binaryFileTypePickerGallery from "./ControlGallery/binaryFileTypePickerGallery.partial.obs";
import campusPickerGallery from "./ControlGallery/campusPickerGallery.partial.obs";
import scheduleBuilderGallery from "./ControlGallery/scheduleBuilderGallery.partial.obs";
import binaryFilePickerGallery from "./ControlGallery/binaryFilePickerGallery.partial.obs";
import eventItemPickerGallery from "./ControlGallery/eventItemPickerGallery.partial.obs";
import dataViewPickerGallery from "./ControlGallery/dataViewPickerGallery.partial.obs";
import workflowTypePickerGallery from "./ControlGallery/workflowTypePickerGallery.partial.obs";
import financialGatewayPickerGallery from "./ControlGallery/financialGatewayPickerGallery.partial.obs";
import financialStatementTemplatePickerGallery from "./ControlGallery/financialStatementTemplatePickerGallery.partial.obs";
import fieldTypePickerGallery from "./ControlGallery/fieldTypePickerGallery.partial.obs";
import auditDetailGallery from "./ControlGallery/auditDetailGallery.partial.obs";
import modalGallery from "./ControlGallery/modalGallery.partial.obs";
import componentPickerGallery from "./ControlGallery/componentPickerGallery.partial.obs";
import gradePickerGallery from "./ControlGallery/gradePickerGallery.partial.obs";
import groupMemberPickerGallery from "./ControlGallery/groupMemberPickerGallery.partial.obs";
import interactionChannelPickerGallery from "./ControlGallery/interactionChannelPickerGallery.partial.obs";
import lavaCommandPickerGallery from "./ControlGallery/lavaCommandPickerGallery.partial.obs";
import remoteAuthsPickerGallery from "./ControlGallery/remoteAuthsPickerGallery.partial.obs";
import stepProgramPickerGallery from "./ControlGallery/stepProgramPickerGallery.partial.obs";
import stepProgramStepTypePickerGallery from "./ControlGallery/stepProgramStepTypePickerGallery.partial.obs";
import stepProgramStepStatusPickerGallery from "./ControlGallery/stepProgramStepStatusPickerGallery.partial.obs";
import stepStatusPickerGallery from "./ControlGallery/stepStatusPickerGallery.partial.obs";
import stepTypePickerGallery from "./ControlGallery/stepTypePickerGallery.partial.obs";
import streakTypePickerGallery from "./ControlGallery/streakTypePickerGallery.partial.obs";
import badgePickerGallery from "./ControlGallery/badgePickerGallery.partial.obs";
import notificationBoxGallery from "./ControlGallery/notificationBoxGallery.partial.obs";
import badgeListGallery from "./ControlGallery/badgeListGallery.partial.obs";
import basicTimePickerGallery from "./ControlGallery/basicTimePickerGallery.partial.obs";
import birthdayPickerGallery from "./ControlGallery/birthdayPickerGallery.partial.obs";
import countdownTimerGallery from "./ControlGallery/countdownTimerGallery.partial.obs";
import electronicSignatureGallery from "./ControlGallery/electronicSignatureGallery.partial.obs";
import fieldTypeEditorGallery from "./ControlGallery/fieldTypeEditorGallery.partial.obs";
import inlineRangeSliderGallery from "./ControlGallery/inlineRangeSliderGallery.partial.obs";
import javaScriptAnchorGallery from "./ControlGallery/javaScriptAnchorGallery.partial.obs";
import loadingGallery from "./ControlGallery/loadingGallery.partial.obs";
import loadingIndicatorGallery from "./ControlGallery/loadingIndicatorGallery.partial.obs";
import numberUpDownGroupGallery from "./ControlGallery/numberUpDownGroupGallery.partial.obs";
import progressBarGallery from "./ControlGallery/progressBarGallery.partial.obs";
import rockButtonGallery from "./ControlGallery/rockButtonGallery.partial.obs";
import rockLabelGallery from "./ControlGallery/rockLabelGallery.partial.obs";
import rockValidationGallery from "./ControlGallery/rockValidationGallery.partial.obs";
import rangeSliderGallery from "./ControlGallery/rangeSliderGallery.partial.obs";
import tabbedBarGallery from "./ControlGallery/tabbedBarGallery.partial.obs";
import interactionComponentPickerGallery from "./ControlGallery/interactionComponentPickerGallery.partial.obs";
import tabbedContentGallery from "./ControlGallery/tabbedContentGallery.partial.obs";
import transitionVerticalCollapseGallery from "./ControlGallery/transitionVerticalCollapseGallery.partial.obs";
import valueDetailListGallery from "./ControlGallery/valueDetailListGallery.partial.obs";
import codeEditorGallery from "./ControlGallery/codeEditorGallery.partial.obs";
import pagePickerGallery from "./ControlGallery/pagePickerGallery.partial.obs";
import groupPickerGallery from "./ControlGallery/groupPickerGallery.partial.obs";
import mergeTemplatePickerGallery from "./ControlGallery/mergeTemplatePickerGallery.partial.obs";
import metricCategoryPickerGallery from "./ControlGallery/metricCategoryPickerGallery.partial.obs";
import metricItemPickerGallery from "./ControlGallery/metricItemPickerGallery.partial.obs";
import registrationTemplatePickerGallery from "./ControlGallery/registrationTemplatePickerGallery.partial.obs";
import reportPickerGallery from "./ControlGallery/reportPickerGallery.partial.obs";
import schedulePickerGallery from "./ControlGallery/schedulePickerGallery.partial.obs";
import workflowActionTypePickerGallery from "./ControlGallery/workflowActionTypePickerGallery.partial.obs";
import dayOfWeekPickerGallery from "./ControlGallery/dayOfWeekPickerGallery.partial.obs";
import monthDayPickerGallery from "./ControlGallery/monthDayPickerGallery.partial.obs";
import monthYearPickerGallery from "./ControlGallery/monthYearPickerGallery.partial.obs";

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
            value: ref<RockCacheabilityBag | null>(null),
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
    @clearButtonClicked="clear"
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
            importCode: getSfcControlImportPath("wordCloud"),
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
        LocationPicker,
        CheckBoxList
    },
    setup() {
        const options = [{
            text: "Location",
            value: "2"
        }, {
            text: "Address",
            value: "1"
        }, {
            text: "Point",
            value: "4"
        }, {
            text: "Geo-fence",
            value: "8"
        }];

        const selectedOptions = ref(["1", "2", "4", "8"]);

        const selectedAsNumber = computed(() => {
            if (selectedOptions.value.length === 0) {
                return undefined;
            }

            return selectedOptions.value.reduce((total, option) => {
                return total + parseInt(option, 10);
            }, 0);
        });

        return {
            value: ref(undefined),
            currentPickerMode: ref(2),
            options,
            selectedOptions,
            selectedAsNumber,
            importCode: getSfcControlImportPath("locationPicker"),
            exampleCode: `<LocationPicker label="Location" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="{value, currentPickerMode}"
    :importCode="importCode"
    :exampleCode="exampleCode"
    hasMultipleValues
    enableReflection >

    <LocationPicker label="Location" v-model="value" v-model:currentPickerMode="currentPickerMode" :allowedPickerModes="selectedAsNumber" />

    <template #settings>
        <div class="row">
            <div class="col-md-6">
                <CheckBoxList v-model="selectedOptions" :items="options" label="Allowed Modes" horizontal />
            </div>
        </div>
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
            locationType: ref<Guid | null>(null),
            parentLocation: ref(""),
            showCityState: ref(false),
            multiple: ref(false),
            allowAdd: ref(false),
            showBlankItem: ref(false),
            isAddressRequired: ref(false),
            parentLocationGuid: ref<Guid | null>("e0545b4d-4f97-43b0-971f-94b593ae2134"),
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
            importCode: getSfcControlImportPath("ethnicityPicker"),
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
            importCode: getSfcControlImportPath("racePicker"),
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
            enhanceForLongLists: ref(false),
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
    <MergeFieldPicker label="Merge Field" v-model="value" :multiple="multiple" :enhanceForLongLists="enhanceForLongLists" :additionalFields="additionalFields" />

    <template #settings>
        <div class="row">
            <div class="col-md-4">
                <CheckBox label="Multiple" v-model="multiple" />
            </div>
            <div class="col-md-4">
                <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
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
            definedType: ref<Guid>(DefinedType.CampusType),
            importCode: getSfcControlImportPath("categorizedValuePicker"),
            exampleCode: `<CategorizedValuePicker label="Categorized Defined Value" v-model="value" :definedTypeGuid="DefinedType.CampusType" />`
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
            <div class="col-md-4">
                <TextBox label="Defined Type" v-model="definedType" />
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
            entityTypeGuid: ref<Guid | null>(null),
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
        :entityTypeGuid="entityTypeGuid"
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
    This is the content that shows up in the popOver
    <template #activator="props">
        <strong v-bind="props">Hover Me</strong>
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
            This is the content that shows up in the popOver
            <template #activator="props">
                <strong v-bind="props">Hover Me</strong>
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
        AccountPicker,
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

    <NoteTextEditor v-model="value" :avatar="avatar" label="Leave a Note" />

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
        const toolsGuid = computed<Guid | null>(() => toGuidOrNull(toolsItemBag.value?.value));
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
            registrationTemplateGuid: ref<Guid | null>(null),
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
        :registrationTemplateGuid="registrationTemplateGuid"
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
            interactionChannelGuid: ref<Guid | null>(null),
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
            workflowTypeGuid: ref<Guid | null>(null),
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
        :workflowTypeGuid="workflowTypeGuid"
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
    datePickerGallery,
    dateRangePickerGallery,
    dateTimePickerGallery,
    DatePartsPickerGallery,
    radioButtonListGallery,
    dialogGallery,
    checkBoxGallery,
    inlineCheckBoxGallery,
    switchGallery,
    inlineSwitchGallery,
    checkBoxListGallery,
    mediaSelectorGallery,
    listBoxGallery,
    listItemsGallery,
    phoneNumberBoxGallery,
    dropDownListGallery,
    helpBlockGallery,
    colorPickerGallery,
    numberBoxGallery,
    numberRangeBoxGallery,
    genderPickerGallery,
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
    ImageUploaderGallery,
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
    KeyValueListGallery,
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
