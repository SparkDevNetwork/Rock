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
import { Component, computed, defineComponent, PropType, reactive, ref } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import PanelWidget from "@Obsidian/Controls/panelWidget";
import TextBox from "@Obsidian/Controls/textBox";
import { FieldType as FieldTypeGuids } from "@Obsidian/SystemGuids/fieldType";
import Block from "@Obsidian/Templates/block";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import FieldTypeEditor from "@Obsidian/Controls/fieldTypeEditor";

/**
 * Convert a simpler set of parameters into PublicAttribute
 * @param name
 * @param fieldTypeGuid
 * @param configValues
 */
const getAttributeData = (name: string, fieldTypeGuid: Guid, configValues: Record<string, string>): Record<string, PublicAttributeBag> => {
    const configurationValues = configValues;

    return {
        "value1": reactive({
            fieldTypeGuid: fieldTypeGuid,
            name: `${name} 1`,
            key: "value1",
            description: `This is the description of the ${name} without an initial value`,
            configurationValues,
            isRequired: false,
            attributeGuid: "",
            order: 0,
            categories: []
        }),
        "value2": reactive({
            fieldTypeGuid: fieldTypeGuid,
            name: `${name} 2`,
            key: "value2",
            description: `This is the description of the ${name} with an initial value`,
            configurationValues,
            isRequired: false,
            attributeGuid: "",
            order: 0,
            categories: []
        })
    };
};

/** An inner component that describes the template used for each of the controls
 *  within this field type gallery */
const galleryAndResult = defineComponent({
    name: "GalleryAndResult",
    components: {
        PanelWidget,
        AttributeValuesContainer
    },
    props: {
        values: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },
        title: {
            type: String as PropType<string>,
            required: true
        },
        attributes: {
            type: Object as PropType<Record<string, PublicAttributeBag>>,
            required: true
        }
    },

    setup(props) {
        const values = ref(props.values);
        const value1Json = computed((): string => values.value["value1"]);
        const value2Json = computed((): string => values.value["value2"]);

        return {
            value1Json,
            value2Json,
            values
        };
    },

    template: `
<PanelWidget>
    <template #header>{{title}}</template>
    <div class="row">
        <div class="col-md-6">
            <h4>Qualifier Values</h4>
            <slot />
            <hr />
            <h4>Attribute Values Container (edit)</h4>
            <AttributeValuesContainer v-model="values" :attributes="attributes" :isEditMode="true" :showCategoryLabel="false" />
        </div>
        <div class="col-md-6">
            <h4>Attribute Values Container (view)</h4>
            <AttributeValuesContainer v-model="values" :attributes="attributes" :isEditMode="false" :showCategoryLabel="false" />
            <hr />
            <h4>Values</h4>
            <p>
                <strong>Value 1</strong>
                <pre>{{value1Json}}</pre>
            </p>
            <p>
                <strong>Value 2</strong>
                <pre>{{value2Json}}</pre>
            </p>
        </div>
    </div>
</PanelWidget>`
});

/**
 * Generate a gallery component for a specific field type
 * @param name
 * @param fieldTypeGuid
 * @param configValues
 */
const getFieldTypeGalleryComponent = (name: string, initialValue: string, fieldTypeGuid: Guid, initialConfigValues: Record<string, string>): Component => {
    return defineComponent({
        name: `${name}Gallery`,
        components: {
            GalleryAndResult: galleryAndResult,
            TextBox,
            FieldTypeEditor
        },
        data() {
            return {
                name,
                values: { "value1": "", "value2": initialValue },
                configValues: { ...initialConfigValues } as Record<string, string>,
                editorValue: { configurationValues: { ...initialConfigValues }, fieldTypeGuid }
            };
        },
        computed: {
            attributes(): Record<string, PublicAttributeBag> {
                return getAttributeData(name, fieldTypeGuid, this.editorValue.configurationValues);
            }
        },
        template: `
<GalleryAndResult :title="name" :values="values" :attributes="attributes" fieldTypeEditor>
    <FieldTypeEditor v-model="editorValue" showConfigOnly />
</GalleryAndResult>`
    });
};

const galleryComponents: Record<string, Component> = {
    AddressGallery: getFieldTypeGalleryComponent("Address", '{"street1": "3120 W Cholla St", "city": "Phoenix", "state": "AZ", "postalCode": "85029-4113", "country": "US"}', FieldTypeGuids.Address, {
    }),

    BooleanGallery: getFieldTypeGalleryComponent("Boolean", "True", FieldTypeGuids.Boolean, {
        truetext: "This is true",
        falsetext: "This is false",
        BooleanControlType: "2"
    }),

    CampusGallery: getFieldTypeGalleryComponent("Campus", "", FieldTypeGuids.Campus, {
        values: JSON.stringify([
            { value: "069D4509-398A-4E08-8225-A0658E8A51E8", text: "Main Campus" },
            { value: "0D8B2F85-5DC2-406E-8A7D-D435F3153C58", text: "Secondary Campus" },
            { value: "8C99160C-D0FC-49E4-AA9D-87EAE7297AF1", text: "Tertiary Campus" }
        ] as ListItemBag[])
    }),

    CampusesGallery: getFieldTypeGalleryComponent("Campuses", "", FieldTypeGuids.Campuses, {
        repeatColumns: "4",
        values: JSON.stringify([
            { value: "069D4509-398A-4E08-8225-A0658E8A51E8", text: "Main Campus" },
            { value: "0D8B2F85-5DC2-406E-8A7D-D435F3153C58", text: "Secondary Campus" },
            { value: "8C99160C-D0FC-49E4-AA9D-87EAE7297AF1", text: "Tertiary Campus" }
        ] as ListItemBag[])
    }),

    CategoryGallery: getFieldTypeGalleryComponent("Category", "", FieldTypeGuids.Category, {
        qualifierColumn: "GroupId",
        qualifierValue: "5",
        entityTypeName: JSON.stringify({ value: "9bbfda11-0d22-40d5-902f-60adfbc88987", text: "Group" })
    }),

    CategoriesGallery: getFieldTypeGalleryComponent("Categories", "", FieldTypeGuids.Categories, {
        qualifierColumn: "GroupId",
        qualifierValue: "5",
        entityTypeName: JSON.stringify({ value: "9bbfda11-0d22-40d5-902f-60adfbc88987", text: "Group" })
    }),


    ColorGallery: getFieldTypeGalleryComponent("Color", "#ee7725", FieldTypeGuids.Color, {
        selectiontype: "Color Picker"
    }),

    CurrencyGallery: getFieldTypeGalleryComponent("Currency", "4.70", FieldTypeGuids.Currency, {
    }),

    DateGallery: getFieldTypeGalleryComponent("Date", "2009-02-11", FieldTypeGuids.Date, {
        format: "MMM yyyy",
        displayDiff: "true",
        displayCurrentOption: "true",
        datePickerControlType: "Date Parts Picker",
        futureYearCount: "2"
    }),

    DateRangeGallery: getFieldTypeGalleryComponent("DateRange", "2021-07-25T00:00:00.0000000,2021-07-29T00:00:00.0000000", FieldTypeGuids.DateRange, {
    }),

    DateTimeGallery: getFieldTypeGalleryComponent("DateTime", "2009-02-11T14:23:00", FieldTypeGuids.DateTime, {
        format: "MMM dd, yyyy h:mm tt",
        displayDiff: "false",
        displayCurrentOption: "true",
    }),

    DayOfWeekGallery: getFieldTypeGalleryComponent("DayOfWeek", "2", FieldTypeGuids.DayOfWeek, {
    }),

    DaysOfWeekGallery: getFieldTypeGalleryComponent("DaysOfWeek", "2,5", FieldTypeGuids.DaysOfWeek, {
    }),

    DecimalGallery: getFieldTypeGalleryComponent("Decimal", "18.283", FieldTypeGuids.Decimal, {
    }),

    DecimalRangeGallery: getFieldTypeGalleryComponent("DecimalRange", "18.283,100", FieldTypeGuids.DecimalRange, {
    }),

    DefinedValueGallery: getFieldTypeGalleryComponent("DefinedValue", '{ "value": "F19FC180-FE8F-4B72-A59C-8013E3B0EB0D", "text": "Single", "description": "Used when the individual is single." }', FieldTypeGuids.DefinedValue, {
        selectableValues: JSON.stringify([
            { value: "5FE5A540-7D9F-433E-B47E-4229D1472248", text: "Married", description: "Used when an individual is married." },
            { value: "F19FC180-FE8F-4B72-A59C-8013E3B0EB0D", text: "Single", description: "Used when the individual is single." },
            { value: "3B689240-24C2-434B-A7B9-A4A6CBA7928C", text: "Divorced", description: "Used when the individual is divorced." },
            { value: "AE5A0228-9910-4505-B3C6-E6C98BEE2E7F", text: "Unknown", description: "" }
        ]),
        allowmultiple: "",
        displaydescription: "true",
        enhancedselection: "",
        includeInactive: "",
        AllowAddingNewValues: "",
        RepeatColumns: ""
    }),

    DefinedValueRangeGallery: getFieldTypeGalleryComponent("DefinedValueRange", '{ "value": "F19FC180-FE8F-4B72-A59C-8013E3B0EB0D,3B689240-24C2-434B-A7B9-A4A6CBA7928C", "text": "Single to Divorced", "description": "Used when the individual is single. to Used when the individual is divorced." }', FieldTypeGuids.DefinedValueRange, {
        values: JSON.stringify([
            { value: "5FE5A540-7D9F-433E-B47E-4229D1472248", text: "Married", description: "Used when an individual is married." },
            { value: "F19FC180-FE8F-4B72-A59C-8013E3B0EB0D", text: "Single", description: "Used when the individual is single." },
            { value: "3B689240-24C2-434B-A7B9-A4A6CBA7928C", text: "Divorced", description: "Used when the individual is divorced." },
            { value: "AE5A0228-9910-4505-B3C6-E6C98BEE2E7F", text: "Unknown", description: "" }
        ]),
        displaydescription: "false"
    }),

    EmailGallery: getFieldTypeGalleryComponent("Email", "ted@rocksolidchurchdemo.com", FieldTypeGuids.Email, {
    }),

    GenderGallery: getFieldTypeGalleryComponent("Gender", "2", FieldTypeGuids.Gender, {
    }),

    GroupGallery: getFieldTypeGalleryComponent("Group", "2", FieldTypeGuids.Group, {
    }),

    GroupLocationTypeGallery: getFieldTypeGalleryComponent("GroupLocationType", "2", FieldTypeGuids.GroupLocationType, {
        groupTypeGuid: JSON.stringify({ value: "790E3215-3B10-442B-AF69-616C0DCB998E", text: "Family" }),
        groupTypeLocations: `{"790E3215-3B10-442B-AF69-616C0DCB998E": ${JSON.stringify('[{"value":"8c52e53c-2a66-435a-ae6e-5ee307d9a0dc","text":"Home","category":null},{"value":"e071472a-f805-4fc4-917a-d5e3c095c35c","text":"Work","category":null},{"value":"853d98f1-6e08-4321-861b-520b4106cfe0","text":"Previous","category":null}]')} }`,
    }),

    GroupMemberGallery: getFieldTypeGalleryComponent("GroupMember", "2", FieldTypeGuids.GroupMember, {
        allowmultiple: "false",
        enhancedselection: "false",
        group: JSON.stringify({ value: "0BA93D66-21B1-4229-979D-F76CEB57666D", text: "A/V Team" })
    }),

    GroupRoleGallery: getFieldTypeGalleryComponent("GroupRole", "2", FieldTypeGuids.GroupRole, {
    }),

    IntegerGallery: getFieldTypeGalleryComponent("Integer", "20", FieldTypeGuids.Integer, {
    }),

    IntegerRangeGallery: getFieldTypeGalleryComponent("IntegerRange", "0,100", FieldTypeGuids.IntegerRange, {
    }),

    KeyValueListGallery: getFieldTypeGalleryComponent("KeyValueList", `[{"key":"One","value":"Two"},{"key":"Three","value":"Four"}]`, FieldTypeGuids.KeyValueList, {
        keyprompt: "Enter Key",
        valueprompt: "Enter Value",
        displayvaluefirst: "false",
        allowhtml: "false",
        values: JSON.stringify([])
    }),

    MemoGallery: getFieldTypeGalleryComponent("Memo", "This is a memo", FieldTypeGuids.Memo, {
        numberofrows: "10",
        maxcharacters: "100",
        showcountdown: "true",
        allowhtml: "true"
    }),

    MonthDayGallery: getFieldTypeGalleryComponent("MonthDay", "7/4", FieldTypeGuids.MonthDay, {
    }),

    MultiSelectGallery: getFieldTypeGalleryComponent("MultiSelect", "pizza", FieldTypeGuids.MultiSelect, {
        repeatColumns: "4",
        repeatDirection: "Horizontal",
        enhancedselection: "false",
        values: '[{"value": "pizza", "text": "Pizza"}, {"value": "sub", "text": "Sub"}, {"value": "bagel", "text": "Bagel"}]'
    }),

    PersonGallery: getFieldTypeGalleryComponent("Person", '{ "value": "996c8b72-c255-40e6-bb98-b1d5cf345f3b", "text": "Admin Admin" }', FieldTypeGuids.Person, {
        includeBusinesses: "false",
        EnableSelfSelection: "True"
    }),

    PhoneNumberGallery: getFieldTypeGalleryComponent("PhoneNumber", "(321) 456-7890", FieldTypeGuids.PhoneNumber, {
    }),

    RatingGallery: getFieldTypeGalleryComponent("Rating", '{"value":3,"maxValue":5}', FieldTypeGuids.Rating, {
        max: "5"
    }),

    ScheduleGallery: getFieldTypeGalleryComponent("Schedule", "2", FieldTypeGuids.Schedule, {
    }),

    SchedulesGallery: getFieldTypeGalleryComponent("Schedules", "2", FieldTypeGuids.Schedules, {
    }),

    SingleSelectGallery: getFieldTypeGalleryComponent("SingleSelect", "pizza", FieldTypeGuids.SingleSelect, {
        repeatColumns: "4",
        fieldtype: "rb",
        values: '[{"value": "pizza", "text": "Pizza"}, {"value": "sub", "text": "Sub"}, {"value": "bagel", "text": "Bagel"}]'
    }),

    SSNGallery: getFieldTypeGalleryComponent("SSN", "123456789", FieldTypeGuids.Ssn, {
    }),

    TextGallery: getFieldTypeGalleryComponent("Text", "Hello", FieldTypeGuids.Text, {
        ispassword: "false",
        maxcharacters: "10",
        showcountdown: "true"
    }),

    TimeGallery: getFieldTypeGalleryComponent("Time", "13:15:00", FieldTypeGuids.Time, {
    }),

    UrlLinkGallery: getFieldTypeGalleryComponent("URL Link", "https://rockrms.com", FieldTypeGuids.UrlLink, {
        ShouldRequireTrailingForwardSlash: "false",
        ShouldAlwaysShowCondensed: "false"
    }),
};

const galleryTemplate: string = Object.keys(galleryComponents).sort().map(g => `<${g} />`).join("");

export default defineComponent({
    name: "Example.FieldTypeGallery",

    components: {
        Block,
        ...galleryComponents
    },

    setup() {
        return {
        };
    },

    template: `
<Block title="Obsidian Field Type Gallery">
    <template #default>
        ${galleryTemplate}
    </template>
</Block>
`
});
