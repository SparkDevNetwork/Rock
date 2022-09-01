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
 * - componentFromUrl
 * - fieldFilterContainer
 * - fieldFilterRuleRow
 * - gatewayControl
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
 */

import { Component, computed, defineComponent, getCurrentInstance, isRef, onMounted, onUnmounted, PropType, Ref, ref, watch } from "vue";
import { ObjectUtils } from "@Obsidian/Utility";
import HighlightJs from "@Obsidian/Libs/highlightJs";
import FieldFilterEditor from "@Obsidian/Controls/fieldFilterEditor";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import TextBox from "@Obsidian/Controls/textBox";
import EmailBox from "@Obsidian/Controls/emailBox";
import CodeEditor from "@Obsidian/Controls/codeEditor";
import CurrencyBox from "@Obsidian/Controls/currencyBox";
import DatePicker from "@Obsidian/Controls/datePicker";
import DateRangePicker from "@Obsidian/Controls/dateRangePicker";
import DateTimePicker from "@Obsidian/Controls/dateTimePicker";
import ListBox from "@Obsidian/Controls/listBox";
import BirthdayPicker from "@Obsidian/Controls/birthdayPicker";
import NumberUpDown from "@Obsidian/Controls/numberUpDown";
import AddressControl, { getDefaultAddressControlModel } from "@Obsidian/Controls/addressControl";
import InlineSwitch from "@Obsidian/Controls/inlineSwitch";
import Switch from "@Obsidian/Controls/switch";
import Toggle from "@Obsidian/Controls/toggle";
import ItemsWithPreAndPostHtml, { ItemWithPreAndPostHtml } from "@Obsidian/Controls/itemsWithPreAndPostHtml";
import StaticFormControl from "@Obsidian/Controls/staticFormControl";
import ProgressTracker, { ProgressTrackerItem } from "@Obsidian/Controls/progressTracker";
import RockForm from "@Obsidian/Controls/rockForm";
import RockButton, { BtnSize, BtnType } from "@Obsidian/Controls/rockButton";
import RadioButtonList from "@Obsidian/Controls/radioButtonList";
import DropDownList from "@Obsidian/Controls/dropDownList";
import Dialog from "@Obsidian/Controls/dialog";
import InlineCheckBox from "@Obsidian/Controls/inlineCheckBox";
import CheckBox from "@Obsidian/Controls/checkBox";
import PhoneNumberBox from "@Obsidian/Controls/phoneNumberBox";
import HelpBlock from "@Obsidian/Controls/helpBlock";
import DatePartsPicker, { DatePartsPickerValue } from "@Obsidian/Controls/datePartsPicker";
import ColorPicker from "@Obsidian/Controls/colorPicker";
import NumberBox from "@Obsidian/Controls/numberBox";
import NumberRangeBox from "@Obsidian/Controls/numberRangeBox";
import GenderDropDownList from "@Obsidian/Controls/genderDropDownList";
import SocialSecurityNumberBox from "@Obsidian/Controls/socialSecurityNumberBox";
import TimePicker from "@Obsidian/Controls/timePicker";
import UrlLinkBox from "@Obsidian/Controls/urlLinkBox";
import CheckBoxList from "@Obsidian/Controls/checkBoxList";
import Rating from "@Obsidian/Controls/rating";
import Fullscreen from "@Obsidian/Controls/fullscreen";
import Panel from "@Obsidian/Controls/panel";
import PersonPicker from "@Obsidian/Controls/personPicker";
import FileUploader from "@Obsidian/Controls/fileUploader";
import ImageUploader from "@Obsidian/Controls/imageUploader";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker";
import AchievementTypePicker from "@Obsidian/Controls/achievementTypePicker";
import AssessmentTypePicker from "@Obsidian/Controls/assessmentTypePicker";
import AssetStorageProviderPicker from "@Obsidian/Controls/assetStorageProviderPicker";
import BinaryFileTypePicker from "@Obsidian/Controls/binaryFileTypePicker";
import BinaryFilePicker from "@Obsidian/Controls/binaryFilePicker";
import SlidingDateRangePicker from "@Obsidian/Controls/slidingDateRangePicker";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker";
import CategoryPicker from "@Obsidian/Controls/categoryPicker";
import LocationPicker from "@Obsidian/Controls/locationPicker";
import ConnectionRequestPicker from "@Obsidian/Controls/connectionRequestPicker";
import CopyButton from "@Obsidian/Controls/copyButton";
import EntityTagList from "@Obsidian/Controls/entityTagList";
import Following from "@Obsidian/Controls/following";
import AuditDetail from "@Obsidian/Controls/auditDetail";
import DetailBlock from "@Obsidian/Templates/detailBlock";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { newGuid } from "@Obsidian/Utility/guid";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { BinaryFiletype, DefinedType, EntityType, FieldType, AssessmentType } from "@Obsidian/SystemGuids";
import { SlidingDateRange, slidingDateRangeToString } from "@Obsidian/Utility/slidingDateRange";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { sleep } from "@Obsidian/Utility/promiseUtils";
import { upperCaseFirstCharacter } from "@Obsidian/Utility/stringUtils";
import TransitionVerticalCollapse from "@Obsidian/Controls/transitionVerticalCollapse";
import SectionContainer from "@Obsidian/Controls/sectionContainer";
import SectionHeader from "@Obsidian/Controls/sectionHeader";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { PickerDisplayStyle } from "@Obsidian/Types/Controls/pickerDisplayStyle";
import { useStore } from "@Obsidian/PageState";
import BadgeComponentPicker from "@Obsidian/Controls/badgeComponentPicker";
import ComponentPicker from "@Obsidian/Controls/componentPicker";
import Modal from "@Obsidian/Controls/modal";
import EventItemPicker from "@Obsidian/Controls/eventItemPicker";
import DataViewPicker from "@Obsidian/Controls/dataViewPicker";
import WorkflowTypePicker from "@Obsidian/Controls/workflowTypePicker";
import FinancialGatewayPicker from "@Obsidian/Controls/financialGatewayPicker";
import FinancialStatementTemplatePicker from "@Obsidian/Controls/financialStatementTemplatePicker";
import FieldTypePicker from "@Obsidian/Controls/fieldTypePicker";
import GradePicker from "@Obsidian/Controls/gradePicker";
import GroupMemberPicker from "@Obsidian/Controls/groupMemberPicker";
import InteractionChannelPicker from "@Obsidian/Controls/interactionChannelPicker";
import InteractionComponentPicker from "@Obsidian/Controls/interactionComponentPicker";
import LavaCommandPicker from "@Obsidian/Controls/lavaCommandPicker";
import RemoteAuthsPicker from "@Obsidian/Controls/remoteAuthsPicker";
import StepProgramPicker from "@Obsidian/Controls/stepProgramPicker";
import StepStatusPicker from "@Obsidian/Controls/stepStatusPicker";
import StepTypePicker from "@Obsidian/Controls/stepTypePicker";
import StreakTypePicker from "@Obsidian/Controls/streakTypePicker";
import Alert, { AlertType } from "@Obsidian/Controls/alert";
import BadgeList from "@Obsidian/Controls/badgeList";
import BadgePicker from "@Obsidian/Controls/badgePicker";
import BasicTimePicker from "@Obsidian/Controls/basicTimePicker";
import CountdownTimer from "@Obsidian/Controls/countdownTimer";
import ElectronicSignature from "@Obsidian/Controls/electronicSignature";
import FieldTypeEditor from "@Obsidian/Controls/fieldTypeEditor";
import InlineSlider from "@Obsidian/Controls/inlineSlider";
import Slider from "@Obsidian/Controls/slider";
import JavaScriptAnchor from "@Obsidian/Controls/javaScriptAnchor";
import KeyValueList from "@Obsidian/Controls/keyValueList";
import Loading from "@Obsidian/Controls/loading";
import LoadingIndicator from "@Obsidian/Controls/loadingIndicator";
import NumberUpDownGroup, { NumberUpDownGroupOption } from "@Obsidian/Controls/numberUpDownGroup";
import PanelWidget from "@Obsidian/Controls/panelWidget";
import ProgressBar from "@Obsidian/Controls/progressBar";
import RockLabel from "@Obsidian/Controls/rockLabel";
import RockValidation from "@Obsidian/Controls/rockValidation";
import TabbedContent from "@Obsidian/Controls/tabbedContent";
import ValueDetailList from "@Obsidian/Controls/valueDetailList";
import PagePicker from "@Obsidian/Controls/pagePicker";
import GroupPicker from "@Obsidian/Controls/groupPicker";
import MergeTemplatePicker from "@Obsidian/Controls/mergeTemplatePicker";
import { MergeTemplateOwnership } from "@Obsidian/Enums/Controls/mergeTemplateOwnership";
import MetricCategoryPicker from "@Obsidian/Controls/metricCategoryPicker";
import MetricItemPicker from "@Obsidian/Controls/metricItemPicker";

// #region Gallery Support

const displayStyleItems: ListItemBag[] = [
    {
        value: PickerDisplayStyle.Auto,
        text: "Auto"
    },
    {
        value: PickerDisplayStyle.List,
        text: "List"
    },
    {
        value: PickerDisplayStyle.Condensed,
        text: "Condensed"
    }
];

/**
 * Takes a gallery component's name and converts it to a name that is useful for the header and
 * sidebar by adding spaces and stripping out the "Gallery" suffix
 *
 * @param name Name of the control
 * @returns A string of code that can be used to import the given control file
 */
function convertComponentName(name: string | undefined | null): string {
    if (!name) {
        return "Unknown Component";
    }

    return name.replace(/[A-Z]/g, " $&").replace(/Gallery$/, "").trim();
}

/**
 * Takes an element name and a collection of attribute keys and values and
 * constructs the example code. This can be used inside a computed call to
 * have the example code dynamically match the selected settings.
 *
 * @param elementName The name of the element to use in the example code.
 * @param attributes The attribute names and values to append to the element name.
 *
 * @returns A string of valid HTML content for how to use the component.
 */
function buildExampleCode(elementName: string, attributes: Record<string, Ref<unknown> | unknown>): string {
    const attrs: string[] = [];

    for (const attr in attributes) {
        let value = attributes[attr];
        console.log("attributes", attr, value);

        if (isRef(value)) {
            value = value.value;
        }

        if (typeof value === "string") {
            attrs.push(`${attr}="${value}"`);
        }
        else if (typeof value === "number") {
            attrs.push(`:${attr}="${value}"`);
        }
        else if (typeof value === "boolean") {
            attrs.push(`:${attr}="${value ? "true" : "false"}"`);
        }
        else if (value === undefined || value === null) {
            /* Do nothing */
        }
    }

    console.log(attrs);

    return `<${elementName} ${attrs.join(" ")} />`;
}

/**
 * A wrapper component that describes the template used for each of the controls
 * within this control gallery
 */
// eslint-disable-next-line @typescript-eslint/naming-convention
export const GalleryAndResult = defineComponent({
    name: "GalleryAndResult",
    inheritAttrs: false,
    components: {
        Switch,
        SectionHeader,
        TransitionVerticalCollapse,
        CopyButton
    },
    props: {
        // The value passed into/controlled by the component, if any
        value: {
            required: false
        },
        // If true, the provided value is a map of multiple values
        hasMultipleValues: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        // Show another copy of the component so you can see that the value is reflected across them
        enableReflection: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        // Code snippet showing how to import the component
        importCode: {
            type: String as PropType<string>
        },
        // Code snippet of the component being used
        exampleCode: {
            type: String as PropType<string>
        },
        // Describe what this component is/does
        description: {
            type: String as PropType<string>,
            default: ""
        }
    },

    setup(props) {
        // Calculate a header based on the name of the component, adding spaces and stripping out the "Gallery" suffix
        const componentName = convertComponentName(getCurrentInstance()?.parent?.type?.name);

        const formattedValue = computed(() => {
            if (!props.hasMultipleValues) {
                return JSON.stringify(props.value, null, 4);
            }
            else {
                // Convert each property's value to a JSON string.
                return ObjectUtils.fromEntries(
                    Object.entries(props.value as Record<string, unknown>).map(([key, val]) => {
                        return [
                            key,
                            JSON.stringify(val, null, 4)
                        ];
                    })
                );
            }
        });

        const styledImportCode = computed((): string | undefined => {
            if (!props.importCode) {
                return undefined;
            }

            return HighlightJs.highlight(props.importCode, {
                language: "typescript"
            })?.value;
        });

        const styledExampleCode = computed((): string | undefined => {
            if (!props.exampleCode) {
                return undefined;
            }

            return HighlightJs.highlight(props.exampleCode, {
                language: "html"
            })?.value;
        });

        const showReflection = ref(false);

        return {
            componentName,
            formattedValue,
            showReflection,
            styledExampleCode,
            styledImportCode,
        };
    },

    template: `
<v-style>
.galleryContent-mainRow > div.well {
    overflow-x: auto;
}

.galleryContent-reflectionToggle {
    display: flex;
    justify-content: flex-end;
}

.galleryContent-valueBox {
    max-height: 300px;
    overflow: auto;
}

.galleryContent-codeSampleWrapper {
    position: relative;
}

.galleryContent-codeSample {
    padding-right: 3rem;
    overflow-x: auto;
}

.galleryContent-codeCopyButton {
    position: absolute;
    top: 1.4rem;
    transform: translateY(-50%);
    right: .5rem;
    z-index: 1;
}

.galleryContent-codeCopyButton::before {
    content: "";
    position: absolute;
    top: -0.3rem;
    right: -0.5rem;
    bottom: -0.3rem;
    left: -0.5rem;
    background: linear-gradient(to left, #f5f5f4, #f5f5f4 80%, #f5f5f500);
    z-index: -1;
}
</v-style>

<SectionHeader :title="componentName" :description="description" />
<div class="galleryContent-mainRow mb-5 row">
    <div v-if="$slots.default" :class="value === void 0 ? 'col-sm-12' : 'col-sm-6'">
        <h4 class="mt-0">Test Control</h4>
        <slot name="default" />

        <div v-if="enableReflection" class="mt-3">
            <div class="mb-3 galleryContent-reflectionToggle">
                <Switch v-model="showReflection" text="Show Reflection" />
            </div>
            <TransitionVerticalCollapse>
                <div v-if="showReflection">
                    <h4 class="mt-0">Control Reflection</h4>
                    <slot name="default" />
                </div>
            </TransitionVerticalCollapse>
        </div>
    </div>
    <div v-if="value !== void 0" class="col-sm-6">
        <div class="well">
            <h4>Current Value</h4>
            <template v-if="hasMultipleValues" v-for="value, key in formattedValue">
                <h5><code>{{ key }}</code></h5>
                <pre class="m-0 p-0 border-0 galleryContent-valueBox">{{ value }}</pre>
            </template>
            <pre v-else class="m-0 p-0 border-0 galleryContent-valueBox">{{ formattedValue }}</pre>
        </div>
    </div>
</div>
<div v-if="$slots.settings" class="mb-5">
    <h4 class="mt-0">Settings</h4>
    <slot name="settings" />
</div>
<div v-if="importCode || exampleCode || $slots.usage" class="mb-5">
    <h4 class="mt-0 mb-3">Usage Notes</h4>
    <slot name="usage">
        <h5 v-if="importCode" class="mt-3 mb-2">Import</h5>
        <div v-if="importCode" class="galleryContent-codeSampleWrapper">
            <pre class="galleryContent-codeSample"><code v-html="styledImportCode"></code></pre>
            <CopyButton :value="importCode" class="galleryContent-codeCopyButton" btnSize="sm" btnType="link" />
        </div>
        <h5 v-if="exampleCode" class="mt-3 mb-2">Template Syntax</h5>
        <div v-if="exampleCode" class="galleryContent-codeSampleWrapper">
            <pre class="galleryContent-codeSample"><code v-html="styledExampleCode"></code></pre>
            <CopyButton :value="exampleCode" class="galleryContent-codeCopyButton" btnSize="sm" btnType="link" />
        </div>
    </slot>
</div>

<div v-if="$slots.header">
    <p class="text-semibold font-italic">The <code>header</code> slot is no longer supported.</p>
</div>

<div v-if="$attrs.splitWidth !== void 0">
    <p class="text-semibold font-italic">The <code>splitWidth</code> prop is no longer supported.</p>
</div>

<div v-if="$slots.gallery">
    <p class="text-semibold font-italic">The <code>gallery</code> slot is deprecated. Please update to the newest Control Gallery template.</p>
    <slot name="gallery" />
</div>
<div v-if="$slots.result">
    <p class="text-semibold font-italic">The <code>result</code> slot is deprecated. Please update to the newest Control Gallery template.</p>
    <slot name="result" />
</div>
`
});

/**
 * Generate a string of an import statement that imports the control will the given file name.
 * The control's name will be based off the filename
 *
 * @param fileName Name of the control's file
 * @returns A string of code that can be used to import the given control file
 */
export function getControlImportPath(fileName: string): string {
    return `import ${upperCaseFirstCharacter(fileName)} from "@Obsidian/Controls/${fileName}";`;
}

/**
 * Generate a string of an import statement that imports the template will the given file name.
 * The template's name will be based off the filename
 *
 * @param fileName Name of the control's file
 * @returns A string of code that can be used to import the given control file
 */
export function getTemplateImportPath(fileName: string): string {
    return `import ${upperCaseFirstCharacter(fileName)} from "@Obsidian/Templates/${fileName}";`;
}

// #endregion

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
                configurationValues: {}
            },
            color: {
                attributeGuid: newGuid(),
                categories: [categories[0], categories[2]],
                description: "Favorite color? Or just a good one?",
                fieldTypeGuid: FieldType.Color,
                isRequired: false,
                key: "color",
                name: "Random Color",
                order: 4,
                configurationValues: {}
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
                configurationValues: {}
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
            <CheckBox formGroupClasses="col-sm-6" v-model="isEditMode" label="Edit Mode" text="Enable" help="Default: false" />
            <CheckBox formGroupClasses="col-sm-6" v-model="showAbbreviatedName" label="Abbreviated Name" text="Show" help="Default: false" />
        </div>
        <div class="row">
            <CheckBox formGroupClasses="col-sm-6" v-model="showEmptyValues" label="Empty Values" text="Show" help="Default: true; Only applies if not in edit mode" />
            <CheckBox formGroupClasses="col-sm-6" v-model="displayAsTabs" label="Category Tabs" text="Show" help="Default: false; If any attributes are in a category, display each category as a tab. Not applicable while editing." />
        </div>
        <CheckBox v-model="showCategoryLabel" label="Category Labels" text="Show" help="Default: false; Only applies when not displaying tabs." />
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
        PhoneNumberBox
    },
    setup() {
        return {
            phoneNumber: ref("8005551234"),
            importCode: getControlImportPath("phoneNumberBox"),
            exampleCode: `<PhoneNumberBox label="Phone 2" v-model="phoneNumber" />`
        };
    },
    template: `
<GalleryAndResult
    :value="phoneNumber"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <PhoneNumberBox label="Phone 1" v-model="phoneNumber" />

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
            { text: "D Text", value: "d", category: "Second" }
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
            options: options,
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
            importCode: getControlImportPath("datePicker"),
            exampleCode: `<DatePicker label="Date" v-model="date" :displayCurrentOption="false" :isCurrentDateOffset="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="date"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <DatePicker label="Date" v-model="date" :displayCurrentOption="displayCurrentOption" :isCurrentDateOffset="isCurrentDateOffset" />

    <template #settings>
        <div class="row">
            <div class="col-sm-4">
                <InlineCheckBox v-model="displayCurrentOption" label="Display Current Option" />
            </div>
            <div class="col-sm-4">
                <InlineCheckBox v-model="isCurrentDateOffset" label="Is Current Date Offset" />
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
            importCode: getControlImportPath("dateRangePicker"),
            exampleCode: `<DateRangePicker label="Date Range" v-model="date" />`
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
            importCode: getControlImportPath("dateTimePicker"),
            exampleCode: `<DateTimePicker label="Date and Time" v-model="date" :displayCurrentOption="false" :isCurrentDateOffset="false" />`
        };
    },
    template: `
<GalleryAndResult
    :value="date"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <DateTimePicker label="Date and Time" v-model="date" :displayCurrentOption="displayCurrentOption" :isCurrentDateOffset="isCurrentDateOffset" />

    <template #settings>
        <div class="row">
            <div class="col-sm-4">
                <InlineCheckBox v-model="displayCurrentOption" label="Display Current Option" />
            </div>
            <div class="col-sm-4">
                <InlineCheckBox v-model="isCurrentDateOffset" label="Is Current Date Offset" />
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code> and <code>Drop Down List</code>.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a time picker */
const timePickerGallery = defineComponent({
    name: "TimePickerGallery",
    components: {
        GalleryAndResult,
        TimePicker
    },
    setup() {
        return {
            value: ref({ hour: 14, minute: 15 }),
            importCode: getControlImportPath("timePicker"),
            exampleCode: `<TimePicker label="Time" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <TimePicker label="Time" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a currency box */
const currencyBoxGallery = defineComponent({
    name: "CurrencyBoxGallery",
    components: {
        GalleryAndResult,
        CurrencyBox
    },
    setup() {
        return {
            value: ref(1.23),
            importCode: getControlImportPath("currencyBox"),
            exampleCode: `<CurrencyBox label="Currency" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <CurrencyBox label="Currency" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code> and <code>Number Box</code>.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        AddressControl
    },
    setup() {
        return {
            value: ref(getDefaultAddressControlModel()),
            importCode: getControlImportPath("addressControl"),
            exampleCode: `<AddressControl label="Address" v-model="value" />`
        };
    },
    template: `
<GalleryAndResult :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <AddressControl label="Address" v-model="value" />

    <template #settings>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
            importCode: getControlImportPath("panel"),
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
    <Panel v-model="value" v-model:isDrawerOpen="drawerValue" :hasCollapse="collapsibleValue" :hasFullscreen="hasFullscreen" :isFullscreenPageOnly="isFullscreenPageOnly" title="Panel Title" :headerSecondaryActions="headerSecondaryActions">
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
        </div>
        <CheckBoxList v-model="simulateValues" label="Simulate" :items="simulateOptions" />

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a person picker */
const personPickerGallery = defineComponent({
    name: "PersonPickerGallery",
    components: {
        GalleryAndResult,
        PersonPicker
    },
    setup() {
        return {
            value: ref(null),
            importCode: getControlImportPath("personPicker"),
            exampleCode: `<PersonPicker v-model="value" label="Person" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value ?? null"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <PersonPicker v-model="value" label="Person" />
    <template #settings>
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
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
        SlidingDateRangePicker
    },
    setup() {
        const value = ref<SlidingDateRange | null>(null);
        const valueText = computed((): string => value.value ? slidingDateRangeToString(value.value) : "");

        return {
            value,
            valueText,
            importCode: getControlImportPath("slidingDateRangePicker"),
            exampleCode: `<SlidingDateRangePicker v-model="value" label="Sliding Date Range" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >
    <SlidingDateRangePicker v-model="value" label="Sliding Date Range" />

    <template #settings>
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
        return {
            definedTypeGuid: ref(DefinedType.PersonConnectionStatus),
            enhanceForLongLists: ref(false),
            multiple: ref(false),
            value: ref({ "value": "b91ba046-bc1e-400c-b85d-638c1f4e0ce2", "text": "Visitor" }),
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
    <DefinedValuePicker label="Defined Value" v-model="value" :definedTypeGuid="definedTypeGuid" :multiple="multiple" :enhanceForLongLists="enhanceForLongLists" />

    <template #settings>
        <TextBox label="Defined Type" v-model="definedTypeGuid" />
        <CheckBox label="Multiple" v-model="multiple" />
        <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
            <div>
                <a class="btn btn-default btn-xs btn-square"><i class="fa fa-lock"></i></a>
                <a class="btn btn-default btn-xs btn-square"><i class="fa fa-pencil"></i></a>
                <a class="btn btn-danger btn-xs btn-square"><i class="fa fa-trash-alt"></i></a>
            </div>
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
            <div>
                <a class="btn btn-default btn-xs btn-square"><i class="fa fa-lock"></i></a>
                <a class="btn btn-default btn-xs btn-square"><i class="fa fa-pencil"></i></a>
                <a class="btn btn-danger btn-xs btn-square"><i class="fa fa-trash-alt"></i></a>
            </div>
        </template>
        Here's some content to put in here.
    </SectionContainer>

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-xs-4" v-model="showDescription" label="Show Description" />
            <CheckBox formGroupClasses="col-xs-4" v-model="showActionBar" label="Show Action Bar" />
            <CheckBox formGroupClasses="col-xs-4" v-model="showContentToggle" label="Show Content Toggle" />
        </div>
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
            multiple: ref(false),
            value: ref(null),
            importCode: getControlImportPath("locationPicker"),
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
        <CheckBox label="Multiple" v-model="multiple" />

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
const entityTagListGallery = defineComponent({
    name: "EntityTagListGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        EntityTagList
    },
    setup() {
        const store = useStore();

        return {
            disabled: ref(false),
            entityTypeGuid: EntityType.Person,
            entityKey: store.state.currentPerson?.idKey ?? "",
            importCode: getControlImportPath("entityTagList"),
            exampleCode: `<EntityTagList :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode">
    <EntityTagList :entityTypeGuid="entityTypeGuid" :entityKey="entityKey" :disabled="disabled" />

    <template #settings>
        <CheckBox label="Disabled" v-model="disabled" />
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
            importCode: getControlImportPath("dataViewPicker"),
            exampleCode: `<DataViewPicker label="Data View" v-model="value" />`
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
            importCode: getControlImportPath("workflowTypePicker"),
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
            value: "",
            importCode: getControlImportPath("modal"),
            exampleCode: `<Modal title="Modal Dialog Title" saveText="Save" />`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode">
    <CheckBox label="Is Open" v-model="isOpen" />

    <Modal v-model="isOpen" title="Modal Dialog Title" saveText="Save" @save="isOpen = false">
        <TextBox label="Required Value" v-model="value" rules="required" />
    </Modal>

    <template #settings>
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
            importCode: getControlImportPath("interactionChannelPicker"),
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
            importCode: getControlImportPath("interactionComponentPicker"),
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
            importCode: getControlImportPath("stepProgramPicker"),
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
            importCode: getControlImportPath("stepStatusPicker"),
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
            importCode: getControlImportPath("stepTypePicker"),
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

/** Demonstrates an alert list */
const alertGallery = defineComponent({
    name: "AlertGallery",
    components: {
        GalleryAndResult,
        Alert,
        DropDownList,
        CheckBox
    },
    setup() {
        const options: ListItemBag[] = Object.keys(AlertType).map(key => ({ text: key, value: AlertType[key] }));
        return {
            isDismissible: ref(false),
            onDismiss: () => alert('"dismiss" event fired.'),
            options,
            alertType: ref(AlertType.Default),
            importCode: getControlImportPath("alert"),
            exampleCode: `<Alert :dismissable="false" alertType="default" @dismiss="onDismiss">This is an alert!</Alert>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <Alert :dismissible="isDismissible" :alertType="alertType" @dismiss="onDismiss">This is an alert!</Alert>

    <template #settings>
        <div class="row">
            <DropDownList formGroupClasses="col-md-4" label="Alert Type" v-model="alertType" :items="options" :showBlankItem="false" />
            <CheckBox formGroupClasses="col-md-4" label="Dismissable" v-model="isDismissible" />
        </div>
        <p>Additional props extend and are passed to the underlying <code>Rock Form Field</code>.</p>
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
        BasicTimePicker
    },
    setup() {
        return {
            value: ref({}),
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
    <BasicTimePicker label="Time" v-model="value" />

    <template #settings>
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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
            value: ref({}),
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

/** Demonstrates inline slider */
const inlineSliderGallery = defineComponent({
    name: "InlineSliderGallery",
    components: {
        GalleryAndResult,
        InlineSlider,
        CheckBox,
        NumberBox
    },
    setup() {
        return {
            value: ref(10),
            intOnly: ref(false),
            min: ref(0),
            max: ref(100),
            showValue: ref(false),
            importCode: getControlImportPath("inlineSlider"),
            exampleCode: `<InlineSlider v-model="value" :isIntegerOnly="intOnly" :min="min" :max="max" :showValueBar="showValue" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <InlineSlider v-model="value" :isIntegerOnly="intOnly" :min="min" :max="max" :showValueBar="showValue" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Integer Only" v-model="intOnly" />
            <CheckBox formGroupClasses="col-md-3" label="Show Value" v-model="showValue" />
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

    <KeyValueList label="Keys and Values" v-model="value" :valueOptions="valueOptions" :displayValueFirst="displayValueFirst" :keyPlaceholder="keyPlaceholder" :valuePlaceholder="valuePlaceholder" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Limit Possible Values" v-model="limitValues" />
            <CheckBox formGroupClasses="col-md-3" label="Show Value First" v-model="displayValueFirst" />
            <TextBox formGroupClasses="col-md-3" label="Placeholder for Key Field" v-model="keyPlaceholder" />
            <TextBox formGroupClasses="col-md-3" label="Placeholder for Value Field" v-model="valuePlaceholder" />
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

/** Demonstrates panel widget */
const panelWidgetGallery = defineComponent({
    name: "PanelWidgetGallery",
    components: {
        GalleryAndResult,
        PanelWidget
    },
    setup() {
        return {
            importCode: getControlImportPath("panelWidget"),
            exampleCode: `<PanelWidget :isDefaultOpen="false">
    <template #header>Header</template>
    Main Content...
</PanelWidget>`
        };
    },
    template: `
<GalleryAndResult
    :importCode="importCode"
    :exampleCode="exampleCode" >

    <PanelWidget :isDefaultOpen="false">
        <template #header>Panel Widget Header</template>
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
    </PanelWidget>
</GalleryAndResult>`
});

/** Demonstrates progress bar */
const progressBarGallery = defineComponent({
    name: "ProgressBarGallery",
    components: {
        GalleryAndResult,
        ProgressBar,
        InlineSlider
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
            <label>Percent Done</label>
            <InlineSlider formGroupClasses="col-md-6" label="Integer Only" v-model="value" showValueBar isIntegerOnly />
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
        const sizeOptions: ListItemBag[] = Object.keys(BtnSize).map(key => ({ text: key, value: BtnSize[key] }));
        const typeOptions: ListItemBag[] = Object.keys(BtnType).map(key => ({ text: key, value: BtnType[key] }));

        return {
            sizeOptions,
            typeOptions,
            btnSize: ref(BtnSize.Default),
            btnType: ref(BtnType.Default),
            value: ref(10),
            onClick: () => new Promise((res) => setTimeout(() => {
                res(true); alert("done");
            }, 3000)),
            autoLoading: ref(false),
            autoDisable: ref(false),
            isLoading: ref(false),
            loadingText: ref("Loading..."),
            importCode: `import RockButton, { BtnType, BtnSize } from "@Obsidian/Controls/rockButton";`,
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
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <RockButton :btnSize="btnSize" :btnType="btnType" @click="onClick" :isLoading="isLoading" :autoLoading="autoLoading" :autoDisable="autoDisable" :loadingText="loadingText">Click Here to Fire Async Operation</RockButton>

    <template #settings>
        <div class="row">
            <DropDownList formGroupClasses="col-md-3" label="Button Size" v-model="btnSize" :items="sizeOptions" :showBlankItem="false" />
            <DropDownList formGroupClasses="col-md-3" label="Button Type" v-model="btnType" :items="typeOptions" :showBlankItem="false" />
            <CheckBox formGroupClasses="col-md-3" label="Auto Loading Indicator" v-model="autoLoading" />
            <CheckBox formGroupClasses="col-md-3" label="Auto Disable" v-model="autoDisable" />
        </div>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Force Loading" v-model="isLoading" />
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

/** Demonstrates slider */
const sliderGallery = defineComponent({
    name: "SliderGallery",
    components: {
        GalleryAndResult,
        Slider,
        CheckBox,
        NumberBox
    },
    setup() {
        return {
            value: ref(10),
            intOnly: ref(false),
            min: ref(0),
            max: ref(100),
            showValue: ref(false),
            importCode: getControlImportPath("slider"),
            exampleCode: `<Slider v-model="value" :isIntegerOnly="intOnly" :min="min" :max="max" :showValueBar="showValue" />`
        };
    },
    template: `
<GalleryAndResult
    :value="value"
    :importCode="importCode"
    :exampleCode="exampleCode"
    enableReflection >

    <Slider v-model="value" :isIntegerOnly="intOnly" :min="min" :max="max" :showValueBar="showValue" />

    <template #settings>
        <div class="row">
            <CheckBox formGroupClasses="col-md-3" label="Integer Only" v-model="intOnly" />
            <CheckBox formGroupClasses="col-md-3" label="Show Value" v-model="showValue" />
            <NumberBox formGroupClasses="col-md-3" label="Minimum Value" v-model="min" />
            <NumberBox formGroupClasses="col-md-3" label="Maximum Value" v-model="max" />
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
            importCode: getControlImportPath("tabbedContent"),
            exampleCode: `<TabbedContent :tabList="arrayOfItems">
    <template #tab="{item}">
        {{ item }}
    </template>
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


    <TabbedContent :tabList="list">
        <template #tab="{item}">
            {{ item }}
        </template>
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
            importCode: getControlImportPath("pagePicker"),
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
        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
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

        <p class="text-semibold font-italic">Not all options have been implemented yet.</p>
    </template>
</GalleryAndResult>`
});


const controlGalleryComponents: Record<string, Component> = [
    alertGallery,
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
    currencyBoxGallery,
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
    personPickerGallery,
    fileUploaderGallery,
    imageUploaderGallery,
    slidingDateRangePickerGallery,
    definedValuePickerGallery,
    entityTypePickerGallery,
    sectionHeaderGallery,
    sectionContainerGallery,
    categoryPickerGallery,
    locationPickerGallery,
    copyButtonGallery,
    entityTagListGallery,
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
    stepStatusPickerGallery,
    stepTypePickerGallery,
    streakTypePickerGallery,
    badgePickerGallery,
    basicTimePickerGallery,
    birthdayPickerGallery,
    countdownTimerGallery,
    electronicSignatureGallery,
    fieldTypeEditorGallery,
    inlineSliderGallery,
    javaScriptAnchorGallery,
    keyValueListGallery,
    loadingGallery,
    loadingIndicatorGallery,
    numberUpDownGroupGallery,
    panelWidgetGallery,
    progressBarGallery,
    rockButtonGallery,
    rockLabelGallery,
    rockValidationGallery,
    sliderGallery,
    tabbedContentGallery,
    transitionVerticalCollapseGallery,
    valueDetailListGallery,
    pagePickerGallery,
    connectionRequestPickerGallery,
    groupPickerGallery,
    mergeTemplatePickerGallery,
    metricCategoryPickerGallery,
    metricItemPickerGallery
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
.galleryContainer {
    margin: -18px;
}

.galleryContainer > * {
    padding: 24px;
}

.gallerySidebar {
    border-radius: 0;
    margin: -1px 0 -1px -1px;
}

.gallerySidebar li.current {
    font-weight: 700;
}
</v-style>
<Panel type="block">
    <template #title>
        Obsidian Control Gallery
    </template>
    <template #default>
        <div class="galleryContainer row">

            <div class="gallerySidebar well col-sm-3">
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

            <div class="galleryContent col-sm-9">
                <component :is="currentComponent" />
            </div>

        </div>
    </template>
</Panel>`
});
