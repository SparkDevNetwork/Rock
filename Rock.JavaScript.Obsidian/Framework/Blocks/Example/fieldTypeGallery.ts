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

import { Component, defineComponent, PropType, reactive, ref } from "vue";
import AttributeValuesContainer from "../../Controls/attributeValuesContainer";
import PanelWidget from "../../Elements/panelWidget";
import TextBox from "../../Elements/textBox";
import { FieldType as FieldTypeGuids } from "../../SystemGuids";
import PaneledBlockTemplate from "../../Templates/paneledBlockTemplate";
import { useConfigurationValues, useInvokeBlockAction } from "../../Util/block";
import { Guid } from "../../Util/guid";
import { ClientEditableAttributeValue, ListItem } from "../../ViewModels";

/**
 * Convert a simpler set of parameters into AttributeValueData
 * @param name
 * @param fieldTypeGuid
 * @param configValues
 */
const getAttributeValueData = (name: string, initialValue: string, fieldTypeGuid: Guid, configValues: Record<string, string>): Array<ClientEditableAttributeValue> => {
    const configurationValues = configValues;

    return [reactive({
            fieldTypeGuid: fieldTypeGuid,
            name: `${name} 1`,
            key: name,
            description: `This is the description of the ${name} without an initial value`,
            configurationValues,
            isRequired: false,
            textValue: "",
            value: "",
            attributeGuid: "",
            order: 0,
            categories: []
        }),
        reactive({
            fieldTypeGuid: fieldTypeGuid,
            name: `${name} 2`,
            key: name,
            description: `This is the description of the ${name} with an initial value`,
            configurationValues,
            isRequired: false,
            textValue: initialValue,
            value: initialValue,
            attributeGuid: "",
            order: 0,
            categories: []
        })
    ];
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
        title: {
            type: String as PropType<string>,
            required: true
        },
        attributeValues: {
            type: Array as PropType<ClientEditableAttributeValue[]>,
            required: true
        }
    },
    computed: {
        value1Json(): string {
            return this.attributeValues[0].value ?? "";
        },
        value2Json(): string {
            return this.attributeValues[1].value ?? "";
        }
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
            <AttributeValuesContainer :attributeValues="attributeValues" :isEditMode="true" />
        </div>
        <div class="col-md-6">
            <h4>Attribute Values Container (view)</h4>
            <AttributeValuesContainer :attributeValues="attributeValues" :isEditMode="false" />
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
            TextBox
        },
        data () {
            return {
                name,
                configValues: { ...initialConfigValues } as Record<string, string>,
                attributeValues: getAttributeValueData(name, initialValue, fieldTypeGuid, initialConfigValues)
            };
        },
        computed: {
            configKeys(): string[] {
                const keys: string[] = [];

                for (const attributeValue of this.attributeValues) {
                    for (const key in attributeValue.configurationValues) {
                        if (keys.indexOf(key) === -1) {
                            keys.push(key);
                        }
                    }
                }

                return keys;
            }
        },
        watch: {
            configValues: {
                deep: true,
                handler() {
                    for (const attributeValue of this.attributeValues) {
                        for (const key in attributeValue.configurationValues) {
                            const value = this.configValues[key] || "";
                            attributeValue.configurationValues[key] = value;
                        }
                    }
                }
            }
        },
        template: `
<GalleryAndResult :title="name" :attributeValues="attributeValues">
    <TextBox v-for="configKey in configKeys" :key="configKey" :label="configKey" v-model="configValues[configKey]" />
</GalleryAndResult>`
    });
};

const galleryComponents: Record<string, Component> = {
    AddressGallery: getFieldTypeGalleryComponent("Address", '{"street1": "3120 W Cholla St", "city": "Phoenix", "state": "AZ", "postalCode": "85029-4113", "country": "US"}', FieldTypeGuids.Address, {
    }),

    BooleanGallery: getFieldTypeGalleryComponent("Boolean", "t", FieldTypeGuids.Boolean, {
        truetext: "This is true",
        falsetext: "This is false",
        BooleanControlType: "2"
    }),

    CampusGallery: getFieldTypeGalleryComponent("Campus", "", FieldTypeGuids.Campus, {
        values: JSON.stringify([
            { value: "069D4509-398A-4E08-8225-A0658E8A51E8", text: "Main Campus" },
            { value: "0D8B2F85-5DC2-406E-8A7D-D435F3153C58", text: "Secondary Campus" },
            { value: "8C99160C-D0FC-49E4-AA9D-87EAE7297AF1", text: "Tertiary Campus" }
        ] as ListItem[])
    }),

    CampusesGallery: getFieldTypeGalleryComponent("Campuses", "", FieldTypeGuids.Campuses, {
        repeatColumns: "4",
        values: JSON.stringify([
            { value: "069D4509-398A-4E08-8225-A0658E8A51E8", text: "Main Campus" },
            { value: "0D8B2F85-5DC2-406E-8A7D-D435F3153C58", text: "Secondary Campus" },
            { value: "8C99160C-D0FC-49E4-AA9D-87EAE7297AF1", text: "Tertiary Campus" }
        ] as ListItem[])
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
        values: JSON.stringify([
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

    PhoneNumberGallery: getFieldTypeGalleryComponent("PhoneNumber", "(321) 456-7890", FieldTypeGuids.PhoneNumber, {
    }),

    RatingGallery: getFieldTypeGalleryComponent("Rating", '{"value":3,"maxValue":5}', FieldTypeGuids.Rating, {
        max: "5"
    }),

    SingleSelectGallery: getFieldTypeGalleryComponent("SingleSelect", "pizza", FieldTypeGuids.SingleSelect, {
        repeatColumns: "4",
        fieldtype: "rb",
        values: '[{"value": "pizza", "text": "Pizza"}, {"value": "sub", "text": "Sub"}, {"value": "bagel", "text": "Bagel"}]'
    }),

    SSNGallery: getFieldTypeGalleryComponent("SSN", "123-45-6789", FieldTypeGuids.Ssn, {
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
        PaneledBlockTemplate,
        ...galleryComponents
    },

    setup() {
        return {
        };
    },

    template: `
<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-flask"></i>
        Obsidian Field Type Gallery
    </template>
    <template v-slot:default>
        ${galleryTemplate}
    </template>
</PaneledBlockTemplate>
`
});
