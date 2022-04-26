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

import { Component, computed, defineComponent, PropType, ref, watch } from "vue";
import FieldVisibilityRulesEditor from "@Obsidian/Controls/fieldFilterEditor";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import TextBox from "@Obsidian/Controls/textBox";
import EmailBox from "@Obsidian/Controls/emailBox";
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
import RockButton from "@Obsidian/Controls/rockButton";
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
import DetailBlock from "@Obsidian/Templates/detailBlock";
import PersonPicker from "@Obsidian/Controls/personPicker";
import FileUploader from "@Obsidian/Controls/fileUploader";
import ImageUploader from "@Obsidian/Controls/imageUploader";
import SlidingDateRangePicker from "@Obsidian/Controls/slidingDateRangePicker";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { newGuid } from "@Obsidian/Utility/guid";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { BinaryFiletype, EntityType, FieldType } from "@Obsidian/SystemGuids";
import { SlidingDateRange, slidingDateRangeToString } from "@Obsidian/Utility/slidingDateRange";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";
import { sleep } from "@Obsidian/Utility/promiseUtils";

/** An inner component that describes the template used for each of the controls
 *  within this control gallery */
// eslint-disable-next-line @typescript-eslint/naming-convention
const GalleryAndResult = defineComponent({
    name: "GalleryAndResult",
    components: {
        Panel
    },
    props: {
        splitWidth: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    },
    template: `
<Panel :hasCollapse="true">
    <template #title><slot name="header" /></template>
    <div v-if="splitWidth" class="row">
        <div class="col-md-6">
            <slot name="gallery" />
        </div>
        <div class="col-md-6">
            <slot name="result" />
        </div>
    </div>
    <template v-else>
        <div>
            <slot name="gallery" />
        </div>
        <div>
            <slot name="result" />
        </div>
    </template>
</Panel>`
});


/** Demonstrates an attribute values container. */
const attributeValuesContainerGallery = defineComponent({
    name: "AttributeValuesContainerGallery",
    components: {
        GalleryAndResult,
        AttributeValuesContainer,
        CheckBox
    },
    setup() {
        const isEditMode = ref(true);

        const showEmptyValues = ref(false);

        const showAbbreviatedName = ref(false);

        const attributes = ref<Record<string, PublicAttributeBag>>({
            text: {
                attributeGuid: newGuid(),
                categories: [],
                description: "A text attribute.",
                fieldTypeGuid: FieldType.Text,
                isRequired: false,
                key: "text",
                name: "Text Attribute",
                order: 2,
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
            single: ""
        });

        return {
            attributes,
            attributeValues,
            isEditMode,
            showAbbreviatedName,
            showEmptyValues
        };
    },
    template: `
<GalleryAndResult :splitWidth="false">
    <template #header>
        AttributeValuesContainer
    </template>
    <template #gallery>
        <CheckBox v-model="isEditMode" label="Is Editable" />

        <CheckBox v-model="showAbbreviatedName" label="Show Abbreviated Name" />

        <CheckBox v-model="showEmptyValues" label="Show Empty Values" />

        <AttributeValuesContainer v-model="attributeValues"
            :attributes="attributes"
            :isEditMode="isEditMode"
            :showAbbreviatedName="showAbbreviatedName"
            :showEmptyValues="showEmptyValues" />
    </template>
</GalleryAndResult>`
});

/** Demonstrates a field visibility rules editor. */
const filterRules = defineComponent({
    name: "FilterRules",
    components: {
        Panel,
        FieldVisibilityRulesEditor,
        CheckBox,
        TextBox
    },
    setup() {
        
        const sourcesText = ref(`[
            {
                "guid": "2a50d342-3a0b-4da3-83c1-25839c75615c",
                "type": 0,
                "attribute": {
                    "attributeGuid": "4eb1eb34-988b-4212-8c93-844fae61b43c",
                    "fieldTypeGuid": "9C204CD0-1233-41C5-818A-C5DA439445AA",
                    "name": "Text Field",
                    "description": "",
                    "configurationValues": {
                        "maxcharacters": "10"
                    }
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c0",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b0",
                    "fieldTypeGuid": "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                    "name": "Integer Field",
                    "description": "",
                    "configurationValues": {}
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c1",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b1",
                    "fieldTypeGuid": "D747E6AE-C383-4E22-8846-71518E3DD06F",
                    "name": "Color",
                    "description": "",
                    "configurationValues": {
                        "selectiontype": "Color Picker"
                }
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c2",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b2",
                    "fieldTypeGuid": "3EE69CBC-35CE-4496-88CC-8327A447603F",
                    "name": "Currency",
                    "description": "",
                    "configurationValues": {}
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c3",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b3",
                    "fieldTypeGuid": "9C7D431C-875C-4792-9E76-93F3A32BB850",
                    "name": "Date Range",
                    "description": "",
                    "configurationValues": {}
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c4",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b4",
                    "fieldTypeGuid": "7EDFA2DE-FDD3-4AC1-B356-1F5BFC231DAE",
                    "name": "Day of Week",
                    "description": "",
                    "configurationValues": {}
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c5",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b5",
                    "fieldTypeGuid": "3D045CAE-EA72-4A04-B7BE-7FD1D6214217",
                    "name": "Email",
                    "description": "",
                    "configurationValues": {}
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c6",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b6",
                    "fieldTypeGuid": "2E28779B-4C76-4142-AE8D-49EA31DDB503",
                    "name": "Gender",
                    "description": "",
                    "configurationValues": {
                        "hideUnknownGender": "True"
                    }
                }
            },
            {
                "guid": "6dbb47c4-5816-4110-8a52-92880d4d05c7",
                "type": 0,
                "attribute": {
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b7",
                    "fieldTypeGuid": "C28C7BF3-A552-4D77-9408-DEDCF760CED0",
                    "name": "Memo",
                    "description": "",
                    "configurationValues": {
                        "numberofrows": "4",
                        "allowhtml": "True",
                        "maxcharacters": "5",
                        "showcountdown": "True"
                    }
                }
            }
        ]`);

        const sources = computed(() => {
            return JSON.parse(sourcesText.value);
        });

        const prefilled = (): FieldFilterGroupBag => ({
            guid: newGuid(),
            expressionType: 4,
            "rules": [
                {
                    "guid": "a81c3ef9-72a9-476b-8b88-b52f513d92e6",
                    "comparisonType": 128,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b0",
                    "value": "50"
                },
                {
                    "guid": "74d34117-4cc6-4cea-92c5-8297aa693ba5",
                    "comparisonType": 2,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b1",
                    "value": "BlanchedAlmond"
                },
                {
                    "guid": "0fa2b6ea-bc86-4fae-b0da-02e48fed8d96",
                    "comparisonType": 8,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b5",
                    "value": "@gmail.com"
                },
                {
                    "guid": "434107e6-6c0c-4698-90ef-d615b1c2de4b",
                    "comparisonType": 2,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b6",
                    "value": "2"
                },
                {
                    "guid": "706179b9-7518-4a74-8e0f-8a48016aec04",
                    "comparisonType": 16,
                    sourceType: 0,
                    "attributeGuid": "4eb1eb34-988b-4212-8c93-844fae61b43c",
                    "value": "text"
                },
                {
                    "guid": "4564eac2-15d9-48d9-b618-563523285af0",
                    "comparisonType": 512,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b2",
                    "value": "999"
                },
                {
                    "guid": "e6c56d4c-7f63-44f9-8f07-1ea0860b605d",
                    "comparisonType": 1,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b3",
                    "value": "2022-02-01,2022-02-28"
                },
                {
                    "guid": "0c27507f-9fb7-4f37-8026-70933bbf1398",
                    "comparisonType": 0,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b4",
                    "value": "3"
                },
                {
                    "guid": "4f68fa2c-0942-4084-bb4d-3c045cef4551",
                    "comparisonType": 8,
                    sourceType: 0,
                    "attributeGuid": "c41817d8-be26-460c-9f89-a7059ae6a9b7",
                    "value": "more text than I want to deal with...."
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

        const json = computed(() => {
            return JSON.stringify(value.value, null, 4);
        });

        return { json, sourcesText, sources, value, title, usePrefilled };
    },
    template: `
<Panel :has-collapse="true" title="Form Field Filter">
    <template #drawer>
        <TextBox v-model="title" label="Attribute Name" />
        <TextBox v-model="sourcesText" label="Sources JSON" text-mode="multiline" :rows="15" />
        <CheckBox v-model="usePrefilled" text="Use prefilled data" />
    </template>
    <div>
        <FieldVisibilityRulesEditor :sources="sources" v-model="value" :title="title" />
    </div>
    <pre class="mt-3">{{json}}</pre>
</Panel>`
});

/** Demonstrates a phone number box */
const phoneNumberBoxGallery = defineComponent({
    name: "PhoneNumberBoxGallery",
    components: {
        GalleryAndResult,
        PhoneNumberBox
    },
    data () {
        return {
            phoneNumber: "8005551234"
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        PhoneNumberBox
    </template>
    <template #gallery>
        <PhoneNumberBox label="Phone 1" v-model="phoneNumber" />
        <PhoneNumberBox label="Phone 2" v-model="phoneNumber" />
    </template>
    <template #result>
        {{phoneNumber}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a help block */
const helpBlockGallery = defineComponent({
    name: "HelpBlockGallery",
    components: {
        GalleryAndResult,
        HelpBlock
    },
    template: `
<GalleryAndResult>
    <template #header>
        HelpBlock
    </template>
    <template #gallery>
        <HelpBlock text="This is some helpful text that explains something." />
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
            //loadOptionsAsync,
            showBlankItem: ref(true),
            grouped: ref(false),
            multiple: ref(false),
            value: ref("a"),
            options: options
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        DropDownList
    </template>
    <template #gallery>
        <CheckBox label="Show Blank Item" v-model="showBlankItem" />
        <CheckBox label="Enhance For Long Lists" v-model="enhanceForLongLists" />
        <CheckBox label="Grouped" v-model="grouped" />
        <CheckBox label="Multiple" v-model="multiple" />

        <DropDownList label="Select 1" v-model="value" :options="options" :optionsSource="loadOptionsAsync" :showBlankItem="showBlankItem" :enhanceForLongLists="enhanceForLongLists" :grouped="grouped" :multiple="multiple" />
        <DropDownList label="Select 2" v-model="value" :options="options" :optionsSource="loadOptionsAsync" :showBlankItem="showBlankItem" :enhanceForLongLists="enhanceForLongLists" :grouped="grouped" :multiple="multiple" />
    </template>
    <template #result>
        {{value}}
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
    data () {
        return {
            value: "a",
            isHorizontal: true,
            repeatColumns: 0,
            options: [
                { text: "A Text", value: "a" },
                { text: "B Text", value: "b" },
                { text: "C Text", value: "c" },
                { text: "D Text", value: "d" },
                { text: "E Text", value: "e" },
                { text: "F Text", value: "f" },
                { text: "G Text", value: "g" }
            ] as ListItemBag[]
        };
    },
    template: `
<GalleryAndResult :splitWidth="false">
    <template #header>
        RadioButtonList
    </template>
    <template #gallery>
        <NumberUpDown label="Horizontal Columns" v-model="repeatColumns" :min="0" />
        <Toggle label="Horizontal" v-model="isHorizontal" />
        <RadioButtonList label="Radio List 1" v-model="value" :options="options" :horizontal="isHorizontal" :repeatColumns="repeatColumns" />
        <RadioButtonList label="Radio List 2" v-model="value" :options="options" />
    </template>
    <template #result>
        Value: {{value}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a checkbox */
const checkBoxGallery = defineComponent({
    name: "CheckBoxGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        Toggle
    },
    data () {
        return {
            isChecked: false,
            inline: true
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        CheckBox
    </template>
    <template #gallery>
        <CheckBox label="Check 1" v-model="isChecked" />
        <CheckBox label="Check 2" v-model="isChecked" />
    </template>
    <template #result>
        {{isChecked}}
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
            inline: true
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        InlineCheckBox
    </template>
    <template #gallery>
        <InlineCheckBox label="Check 1" v-model="isChecked" />
        <InlineCheckBox label="Check 2" v-model="isChecked" />
    </template>
    <template #result>
        {{isChecked}}
    </template>
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
    data () {
        return {
            isDialogVisible: false,
            isDismissible: false
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        Dialog
    </template>
    <template #gallery>
        <RockButton @click="isDialogVisible = true">Show</RockButton>
        <CheckBox label="Dismissible" v-model="isDismissible" />
    </template>
    <template #result>
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
    </template>
</GalleryAndResult>`
});

/** Demonstrates how rock forms work with rules to produce validation messages */
const formRulesGallery = defineComponent({
    name: "FormRulesGallery",
    components: {
        GalleryAndResult,
        RockForm,
        TextBox,
        CurrencyBox,
        RockButton
    },
    data () {
        return {
            ruleTestCurrency: 1,
            ruleTestText: "",
            rules: "required"
        };
    },
    template: `
<GalleryAndResult :splitWidth="false">
    <template #header>
        Form Rules
    </template>
    <template #gallery>
        <TextBox label="Rules" v-model="rules" help="Try 'required', 'gte:1', 'lt:2', and others. Combine rules like this: 'required|lt:7|gt:6'" />
        <hr />
        <RockForm>
            <TextBox label="Text" v-model="ruleTestText" :rules="rules" />
            <CurrencyBox label="Currency" v-model="ruleTestCurrency" :rules="rules" />
            <RockButton btnType="primary" type="submit">Test</RockButton>
        </RockForm>
    </template>
</GalleryAndResult>`
});

/** Demonstrates check box list */
const checkBoxListGallery = defineComponent({
    name: "CheckBoxListGallery",
    components: {
        GalleryAndResult,
        CheckBoxList
    },
    data() {
        return {
            options: [
                { value: "red", text: "Red" },
                { value: "green", text: "Green" },
                { value: "blue", text: "Blue" }
            ] as ListItemBag[],
            items: ["green"]
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        CheckBoxList
    </template>
    <template #gallery>
        <CheckBoxList label="CheckBoxList 1" v-model="items" :options="options" />
        <CheckBoxList label="CheckBoxList 2" v-model="items" :options="options" />
    </template>
    <template #result>
        Items: {{JSON.stringify(items, null, 2)}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a list box */
const listBoxGallery = defineComponent({
    name: "ListBoxGallery",
    components: {
        GalleryAndResult,
        ListBox
    },
    data() {
        return {
            value: ["a"],
            options: [
                { text: "A Text", value: "a" },
                { text: "B Text", value: "b" },
                { text: "C Text", value: "c" },
                { text: "D Text", value: "d" }
            ] as ListItemBag[]
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        ListBox
    </template>
    <template #gallery>
        <ListBox label="Select 1" v-model="value" :options="options" />
        <ListBox label="Select 2" v-model="value" :options="options" />
        <ListBox label="Enhanced Select 1" v-model="value" :options="options" enhanceForLongLists />
        <ListBox label="Enhanced Select 2" v-model="value" :options="options" enhanceForLongLists />
    </template>
    <template #result>
        {{value}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates date pickers */
const datePickerGallery = defineComponent({
    name: "DatePickerGallery",
    components: {
        GalleryAndResult,
        DatePicker
    },
    data () {
        return {
            date: null as string | null,
            currentDate: "CURRENT:1"
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        DatePicker
    </template>
    <template #gallery>
        <DatePicker label="Date 1" v-model="date" />
        <DatePicker label="Date 2" v-model="date" />
        <DatePicker label="Current Date 1" v-model="currentDate" displayCurrentOption isCurrentDateOffset />
        <DatePicker label="Current Date 2" v-model="currentDate" displayCurrentOption isCurrentDateOffset />
    </template>
    <template #result>
        Date: {{JSON.stringify(date, null, 2)}}
        <br />
        Current Date: {{JSON.stringify(currentDate, null, 2)}}
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
    data() {
        return {
            date: {}
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        DateRangePicker
    </template>
    <template #gallery>
        <DateRangePicker label="Date Range 1" v-model="date" />
        <DateRangePicker label="Date Range 2" v-model="date" />
    </template>
    <template #result>
        Date: {{JSON.stringify(date, null, 2)}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates date time pickers */
const dateTimePickerGallery = defineComponent({
    name: "DatePickerGallery",
    components: {
        GalleryAndResult,
        DateTimePicker
    },
    data() {
        return {
            date: null as string | null
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        DateTimePicker
    </template>
    <template #gallery>
        <DateTimePicker label="Date 1" v-model="date" />
        <DateTimePicker label="Date 2" v-model="date" />
    </template>
    <template #result>
        Date: {{JSON.stringify(date, null, 2)}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates date part pickers */
const datePartsPickerGallery = defineComponent({
    name: "DatePartsPickerGallery",
    components: {
        GalleryAndResult,
        Toggle,
        BirthdayPicker,
        DatePartsPicker
    },
    data () {
        return {
            showYear: true,
            datePartsModel: {
                month: 1,
                day: 1,
                year: 2020
            } as DatePartsPickerValue
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        DatePartsPicker
    </template>
    <template #gallery>
        <Toggle label="Show Year" v-model="showYear" />
        <DatePartsPicker label="DatePartsPicker 1" v-model="datePartsModel" :showYear="showYear" />
        <DatePartsPicker label="DatePartsPicker 2" v-model="datePartsModel" :showYear="showYear" />
    </template>
    <template #result>
        <span>{{datePartsModel.month}} / {{datePartsModel.day}}</span><span v-if="showYear"> / {{datePartsModel.year}}</span>
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
    data () {
        return {
            text: "Some two-way bound text",
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        TextBox
    </template>
    <template #gallery>
        <TextBox label="Text 1" v-model="text" :maxLength="10" showCountDown />
        <TextBox label="Text 2" v-model="text" />
        <TextBox label="Memo" v-model="text" textMode="MultiLine" :rows="10" :maxLength="100" showCountDown />
    </template>
    <template #result>
        {{text}}
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
    data() {
        return {
            value: "#ee7725",
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        ColorPicker
    </template>
    <template #gallery>
        <ColorPicker label="Color" v-model="value" />
    </template>
    <template #result>
        {{value}}
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
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        NumberBox
    </template>
    <template #gallery>
        <TextBox label="Minimum Value" v-model="minimumValue" />
        <TextBox label="Maximum Value" v-model="maximumValue" />
        <RockForm>
            <NumberBox label="Number" v-model="value" :minimumValue="numericMinimumValue" :maximumValue="numericMaximumValue" />
            <RockButton btnType="primary" type="submit">Test</RockButton>
        </RockForm>
    </template>
    <template #result>
        {{value}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a number box */
const numberRangeBoxGallery = defineComponent({
    name: "NumberRangeBoxGallery",
    components: {
        GalleryAndResult,
        RockForm,
        RockButton,
        TextBox,
        NumberRangeBox
    },
    data() {
        return {
            value: { lower: 0, upper: 100 },
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        NumberRangeBox
    </template>
    <template #gallery>
        <RockForm>
            <NumberRangeBox label="Number Range" v-model="value" />
            <RockButton btnType="primary" type="submit">Test</RockButton>
        </RockForm>
    </template>
    <template #result>
        {{value.lower}} to {{value.upper}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a gender picker */
const genderDropDownListGallery = defineComponent({
    name: "GenderDropDownListGallery",
    components: {
        GalleryAndResult,
        RockForm,
        RockButton,
        TextBox,
        GenderDropDownList
    },
    data() {
        return {
            value: "1",
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        GenderDropDownList
    </template>
    <template #gallery>
        <RockForm>
            <GenderDropDownList label="Your Gender" v-model="value" />
            <RockButton btnType="primary" type="submit">Test</RockButton>
        </RockForm>
    </template>
    <template #result>
        {{value}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a social security number box */
const socialSecurityNumberBoxGallery = defineComponent({
    name: "SocialSecurityNumberBoxGallery",
    components: {
        GalleryAndResult,
        RockForm,
        RockButton,
        TextBox,
        SocialSecurityNumberBox
    },
    data() {
        return {
            value: "123-45-6789",
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        SocialSecurityNumberBox
    </template>
    <template #gallery>
        <RockForm>
            <SocialSecurityNumberBox label="SSN" v-model="value" />
            <RockButton btnType="primary" type="submit">Test</RockButton>
        </RockForm>
    </template>
    <template #result>
        {{value}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a time picker */
const timePickerGallery = defineComponent({
    name: "TimePickerGallery",
    components: {
        GalleryAndResult,
        RockForm,
        RockButton,
        TextBox,
        TimePicker
    },
    data() {
        return {
            value: { hour: 14, minute: 15 },
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        TimePicker
    </template>
    <template #gallery>
        <RockForm>
            <TimePicker label="Time" v-model="value" />
            <RockButton btnType="primary" type="submit">Test</RockButton>
        </RockForm>
    </template>
    <template #result>
        {{value.hour}}:{{value.minute}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates a rating picker */
const ratingGallery = defineComponent({
    name: "RatingGallery",
    components: {
        GalleryAndResult,
        RockForm,
        NumberBox,
        Rating
    },
    data() {
        return {
            value: 3,
            maximumValue: 5
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        Rating
    </template>
    <template #gallery>
        <NumberBox label="Maximum Rating" v-model="maximumValue" />
        <RockForm>
            <Rating label="Time" v-model="value" :maxRating="maximumValue || 5" />
        </RockForm>
    </template>
    <template #result>
        {{value}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates an switch */
const switchGallery = defineComponent({
    name: "SwitchGallery",
    components: {
        GalleryAndResult,
        TextBox,
        Switch
    },
    data() {
        return {
            text: "",
            isChecked: false
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        Switch
    </template>
    <template #gallery>
        <TextBox label="Text" v-model="text" />

        <Switch label="Switch 1" v-model="isChecked" :text="text" />
        <Switch label="Switch 2" v-model="isChecked" :text="text" />
    </template>
    <template #result>
        {{isChecked}}
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
    data() {
        return {
            isBold: false,
            isChecked: false
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        InlineSwitch
    </template>
    <template #gallery>
        <CheckBox label="Is Bold" v-model="isBold" />

        <InlineSwitch label="Inline Switch 1" v-model="isChecked" :isBold="isBold" />
        <InlineSwitch label="Inline Switch 2" v-model="isChecked" :isBold="isBold" />
    </template>
    <template #result>
        {{isChecked}}
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
    data() {
        return {
            value: 1.23
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        CurrencyBox
    </template>
    <template #gallery>
        <CurrencyBox label="Currency 1" v-model="value" />
        <CurrencyBox label="Currency 2" v-model="value" />
    </template>
    <template #result>
        {{value}}
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
    data() {
        return {
            value: "ted@rocksolidchurchdemo.com"
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        EmailBox
    </template>
    <template #gallery>
        <EmailBox label="EmailBox 1" v-model="value" />
        <EmailBox label="EmailBox 2" v-model="value" />
    </template>
    <template #result>
        {{value}}
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
    data() {
        return {
            value: 1
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        NumberUpDown
    </template>
    <template #gallery>
        <NumberUpDown label="NumberUpDown 1" v-model="value" />
        <NumberUpDown label="NumberUpDown 2" v-model="value" />
    </template>
    <template #result>
        {{value}}
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
    data() {
        return {
            value: "This is some text"
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        StaticFormControl
    </template>
    <template #gallery>
        <StaticFormControl label="StaticFormControl 1" v-model="value" />
        <StaticFormControl label="StaticFormControl 2" v-model="value" />
    </template>
    <template #result>
        {{value}}
    </template>
</GalleryAndResult>`
});

/** Demonstrates an address control */
const addressControlGallery = defineComponent({
    name: "AddressControlGallery",
    components: {
        GalleryAndResult,
        AddressControl
    },
    data() {
        return {
            value: getDefaultAddressControlModel()
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        AddressControl
    </template>
    <template #gallery>
        <AddressControl label="Address 1" v-model="value" />
        <AddressControl label="Address 2" v-model="value" />
    </template>
    <template #result>
        <pre>{{JSON.stringify(value, null, 2)}}</pre>
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
    data() {
        return {
            trueText: "On",
            falseText: "Off",
            btnSize: "",
            sizeOptions: [
                { value: "lg", text: "Large" },
                { value: "md", text: "Medium" },
                { value: "sm", text: "Small" },
                { value: "xs", text: "Extra Small" },
            ],
            value: false
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        Toggle
    </template>
    <template #gallery>
        <TextBox label="True Text" v-model="trueText" />
        <TextBox label="False Text" v-model="falseText" />
        <DropDownList label="Button Size" v-model="btnSize" :options="sizeOptions" />

       <Toggle label="Toggle 1" v-model="value" :trueText="trueText" :falseText="falseText" :btnSize="btnSize" />
       <Toggle label="Toggle 2" v-model="value" :trueText="trueText" :falseText="falseText" :btnSize="btnSize" />
    </template>
    <template #result>
        {{value}}
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
    data() {
        return {
            value: 0,
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
            ] as ProgressTrackerItem[]
        };
    },
    template: `
<GalleryAndResult :splitWidth="false">
    <template #header>
        ProgressTracker
    </template>
    <template #gallery>
        <NumberUpDown label="Index" v-model="value" :min="0" :max="100" />
    </template>
    <template #result>
        <ProgressTracker :items="items" :currentIndex="value" />
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
    data() {
        return {
            value: [
                { preHtml: '<div class="row"><div class="col-sm-6">', postHtml: "</div>", slotName: "item1" },
                { preHtml: '<div class="col-sm-6">', postHtml: "</div></div>", slotName: "item2" }
            ] as ItemWithPreAndPostHtml[],
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        ItemsWithPreAndPostHtml
    </template>
    <template #gallery>
        <TextBox label="Item 1 - Pre Html" v-model="value[0].preHtml" />
        <TextBox label="Item 1 - Post Html" v-model="value[0].postHtml" />
        <TextBox label="Item 2 - Pre Html" v-model="value[1].preHtml" />
        <TextBox label="Item 2 - Post Html" v-model="value[1].postHtml" />
    </template>
    <template #result>
        <ItemsWithPreAndPostHtml :items="value">
            <template #item1>
                <div style="background-color: #fcc; padding: 5px;">This is item 1</div>
            </template>
            <template #item2>
                <div style="background-color: #ccf; padding: 5px;">This is item 2</div>
            </template>
        </ItemsWithPreAndPostHtml>
    </template>
</GalleryAndResult>`
});

/** Demonstrates a URL link box */
const urlLinkBoxGallery = defineComponent({
    name: "UrlLinkBoxGallery",
    components: {
        UrlLinkBox,
        RockForm,
        RockButton,
        GalleryAndResult
    },
    data() {
        return {
            value: "/home/",
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        UrlLinkBox
    </template>
    <template #gallery>
        <RockForm>
            <UrlLinkBox label="URL" v-model="value" />
            <RockButton btnType="primary" type="submit">Test</RockButton>
        </RockForm>
    </template>
    <template #result>
        {{value}}
    </template>
</GalleryAndResult>`
});


/** Demonstrates the fullscreen component. */
const fullscreenGallery = defineComponent({
    name: "FullscreenGallery",
    components: {
        GalleryAndResult,
        CheckBox,
        Fullscreen
    },
    data() {
        return {
            colors: Array.apply(0, Array(256)).map((_: unknown, index: number) => `rgb(${index}, ${index}, ${index})`),
            pageOnly: true,
            value: false
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        Fullscreen
    </template>
    <template #gallery>
        <CheckBox v-model="pageOnly" label="Is Page Only" />

        <Fullscreen v-model="value" :isPageOnly="pageOnly">
            <CheckBox v-model="value" label="Fullscreen" />
            <div v-for="c in colors" :style="{ background: c, height: '5px' }"></div>
        </Fullscreen>
    </template>
    <template #result>
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
            collapsableValue: ref(true),
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
            value: ref(true)
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        Panel
    </template>
    <template #gallery>
        <CheckBox v-model="collapsableValue" label="Collapsable" />
        <CheckBox v-model="value" label="Panel Open" />
        <CheckBox v-model="hasFullscreen" label="Has Fullscreen" />
        <CheckBox v-model="isFullscreenPageOnly" label="Page Only Fullscreen" />
        <CheckBoxList v-model="simulateValues" label="Simulate" :options="simulateOptions" />

        <Panel v-model="value" v-model:isDrawerOpen="drawerValue" :hasCollapse="collapsableValue" :hasFullscreen="hasFullscreen" :isFullscreenPageOnly="isFullscreenPageOnly" title="Panel Title" :headerSecondaryActions="headerSecondaryActions">
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

            <div v-for="c in colors" :style="{ background: c, height: simulateLargeBody ? '5px' : '1px' }"></div>
        </Panel>
    </template>
    <template #result>
    </template>
</GalleryAndResult>`
});

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
            simulateHelp: computed((): boolean => simulateValues.value.includes("helpContent"))
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        DetailBlock
    </template>
    <template #gallery>
        <CheckBox v-model="isAuditHidden" label="Is Audit Hidden" />
        <CheckBox v-model="isBadgesVisible" label="Is Badges Visible" />
        <CheckBox v-model="isDeleteVisible" label="Is Delete Visible" />
        <CheckBox v-model="isEditVisible" label="Is Edit Visible" />
        <CheckBox v-model="isFollowVisible" label="Is Follow Visible" />
        <CheckBox v-model="isSecurityHidden" label="Is Security Hidden" />
        <CheckBoxList v-model="simulateValues" label="Simulate" :options="simulateOptions" />

        <DetailBlock name="Sample Group"
            :entityTypeGuid="entityTypeGuid"
            entityTypeName="Group"
            entityKey="1"
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
            :isSecurityHidden="isSecurityHidden">
            <template v-if="simulateHelp" #helpContent>
                This is some help text.
            </template>

            <div v-for="c in colors" :style="{ background: c, height: '1px' }"></div>
        </DetailBlock>
    </template>
    <template #result>
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
    data() {
        return {
            value: undefined
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        PersonPicker
    </template>
    <template #gallery>
        <PersonPicker v-model="value" label="Person" />
    </template>
    <template #result>
        <span v-if="value">{{ JSON.stringify(value) }}</span>
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
    data() {
        return {
            binaryFileTypeGuid: BinaryFiletype.Default,
            showDeleteButton: true,
            uploadAsTemporary: true,
            uploadButtonText: "Upload",
            value: null
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        File Uploader
    </template>
    <template #gallery>
        <CheckBox v-model="uploadAsTemporary" label="Upload As Temporary" />
        <CheckBox v-model="showDeleteButton" label="Show Delete Button" />
        <TextBox v-model="binaryFileTypeGuid" label="Binary File Type Guid" />
        <TextBox v-model="uploadButtonText" label="Upload Button Text" />

        <FileUploader v-model="value"
            label="File Uploader"
            :uploadAsTemporary="uploadAsTemporary"
            :binaryFileTypeGuid="binaryFileTypeGuid"
            :uploadButtonText="uploadButtonText"
            :showDeleteButton="showDeleteButton" />
    </template>
    <template #result>
        {{ JSON.stringify(value) }}
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
    data() {
        return {
            binaryFileTypeGuid: BinaryFiletype.Default,
            showDeleteButton: true,
            uploadAsTemporary: true,
            uploadButtonText: "Upload",
            value: null
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        Image Uploader
    </template>
    <template #gallery>
        <CheckBox v-model="uploadAsTemporary" label="Upload As Temporary" />
        <CheckBox v-model="showDeleteButton" label="Show Delete Button" />
        <TextBox v-model="binaryFileTypeGuid" label="Binary File Type Guid" />
        <TextBox v-model="uploadButtonText" label="Upload Button Text" />

        <ImageUploader v-model="value"
            label="Image Uploader"
            :uploadAsTemporary="uploadAsTemporary"
            :binaryFileTypeGuid="binaryFileTypeGuid"
            :uploadButtonText="uploadButtonText"
            :showDeleteButton="showDeleteButton" />
    </template>
    <template #result>
        {{ JSON.stringify(value) }}
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
            valueText
        };
    },
    template: `
<GalleryAndResult>
    <template #header>
        SlidingDateRangePicker
    </template>
    <template #gallery>
        <SlidingDateRangePicker v-model="value" label="Sliding Date Range" />
    </template>
    <template #result>
        <div v-if="value">
            <div>{{ value }}</div>
            <div>{{ valueText }}</div>
        </div>
    </template>
</GalleryAndResult>`
});


const galleryComponents: Record<string, Component> = {
    attributeValuesContainerGallery,
    filterRules,
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
    formRulesGallery,
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
    detailBlockGallery,
    personPickerGallery,
    fileUploaderGallery,
    imageUploaderGallery,
    slidingDateRangePickerGallery
};

const galleryTemplate = Object.keys(galleryComponents).sort().map(g => `<${g} />`).join("");

export default defineComponent({
    name: "Example.ControlGallery",
    components: {
        Panel,
        ...galleryComponents
    },

    template: `
<Panel type="block" hasFullscreen>
    <template v-slot:title>
        <i class="fa fa-flask"></i>
        Obsidian Control Gallery
    </template>
    <template v-slot:default>
        ${galleryTemplate}
    </template>
</Panel>`
});
