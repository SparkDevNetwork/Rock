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

import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate';
import { defineComponent, PropType } from 'vue';
import PanelWidget from '../../Elements/PanelWidget';
import AttributeValuesContainer from '../../Controls/AttributeValuesContainer';
import { Guid } from '../../Util/Guid';
import { ConfigurationValues } from '../../Fields/Index';
import TextBox from '../../Elements/TextBox';

/** A subset interface of the AttributeValue view model to simplify this block */
interface AttributeValueData
{
    Attribute: {
        Name: string,
        Description: string,
        FieldTypeGuid: Guid,
        QualifierValues: ConfigurationValues
    },
    Value: string
}

/**
 * Convert a simpler set of parameters into AttributeValueData
 * @param name
 * @param fieldTypeGuid
 * @param configValues
 */
const GetAttributeValueData = ( name: string, initialValue: string, fieldTypeGuid: Guid, configValues: Record<string, string> ) =>
{
    const configurationValues: ConfigurationValues = {};

    for ( const key in configValues )
    {
        configurationValues[ key ] = {
            Name: '',
            Description: '',
            Value: configValues[ key ]
        };
    }

    return [
        {
            Attribute: {
                Name: `${name} 1`,
                Description: `This is the description of the ${name} without an initial value`,
                FieldTypeGuid: fieldTypeGuid,
                QualifierValues: configurationValues
            },
            Value: ''
        },
        {
            Attribute: {
                Name: `${name} 2`,
                Description: `This is the description of the ${name} with an initial value`,
                FieldTypeGuid: fieldTypeGuid,
                QualifierValues: configurationValues
            },
            Value: initialValue
        }
    ];
};

/** An inner component that describes the template used for each of the controls
 *  within this field type gallery */
const GalleryAndResult = defineComponent( {
    name: 'GalleryAndResult',
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
            type: Array as PropType<AttributeValueData[]>,
            required: true
        }
    },
    computed: {
        value1Json (): string
        {
            return JSON.stringify( this.attributeValues[ 0 ].Value, null, 4 );
        },
        value2Json (): string
        {
            return JSON.stringify( this.attributeValues[ 1 ].Value, null, 4 );
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
} );

/**
 * Generate a gallery component for a specific field type
 * @param name
 * @param fieldTypeGuid
 * @param configValues
 */
const GetFieldTypeGalleryComponent = ( name: string, initialValue: string, fieldTypeGuid: Guid, initialConfigValues: Record<string, string> ) =>
{
    return defineComponent( {
        name: `${name}Gallery`,
        components: {
            GalleryAndResult,
            TextBox
        },
        data ()
        {
            return {
                name,
                configValues: { ...initialConfigValues } as Record<string, string>,
                attributeValues: GetAttributeValueData( name, initialValue, fieldTypeGuid, initialConfigValues )
            };
        },
        computed: {
            configKeys (): string[]
            {
                const keys: string[] = [];

                for ( const attributeValue of this.attributeValues )
                {
                    for ( const key in attributeValue.Attribute.QualifierValues )
                    {
                        if ( keys.indexOf( key ) === -1 )
                        {
                            keys.push( key );
                        }
                    }
                }

                return keys;
            }
        },
        watch: {
            configValues: {
                deep: true,
                handler ()
                {
                    for ( const attributeValue of this.attributeValues )
                    {
                        for ( const key in attributeValue.Attribute.QualifierValues )
                        {
                            const value = this.configValues[ key ] || '';
                            attributeValue.Attribute.QualifierValues[ key ].Value = value;
                        }
                    }
                }
            }
        },
        template: `
<GalleryAndResult :title="name" :attributeValues="attributeValues">
    <TextBox v-for="configKey in configKeys" :key="configKey" :label="configKey" v-model="configValues[configKey]" />
</GalleryAndResult>`
    } );
};

export default defineComponent( {
    name: 'Example.FieldTypeGallery',
    components: {
        PaneledBlockTemplate,
        TextGallery: GetFieldTypeGalleryComponent( 'Text', 'Hello', '9C204CD0-1233-41C5-818A-C5DA439445AA', {
            ispassword: 'false',
            maxcharacters: '10',
            showcountdown: 'true'
        } ),
        DateGallery: GetFieldTypeGalleryComponent( 'Date', '2009-02-11', '6B6AA175-4758-453F-8D83-FCD8044B5F36', {
            format: 'MMM yyyy',
            displayDiff: 'true',
            displayCurrentOption: 'true',
            datePickerControlType: 'Date Parts Picker',
            futureYearCount: '2'
        } ),
        SingleSelectGallery: GetFieldTypeGalleryComponent( 'SingleSelect', 'pizza', '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0', {
            repeatColumns: '4',
            fieldtype: 'rb',
            values: 'pizza^Pizza,sub^Sub'
        } ),
        MemoGallery: GetFieldTypeGalleryComponent( 'Memo', 'This is a memo', 'C28C7BF3-A552-4D77-9408-DEDCF760CED0', {
            numberofrows: '10',
            maxcharacters: '100',
            showcountdown: 'true',
            allowhtml: 'true'
        } ),
        BooleanGallery: GetFieldTypeGalleryComponent( 'Boolean', 't', '1EDAFDED-DFE6-4334-B019-6EECBA89E05A', {
            truetext: 'This is true',
            falsetext: 'This is false',
            BooleanControlType: '2'
        } ),
        PhoneNumberGallery: GetFieldTypeGalleryComponent( 'PhoneNumber', '(321) 456-7890', '6B1908EC-12A2-463A-A7BD-970CE0FAF097', {
        } ),
        GenderGallery: GetFieldTypeGalleryComponent('Gender', '2', '2E28779B-4C76-4142-AE8D-49EA31DDB503', {
        }),
        ColorGallery: GetFieldTypeGalleryComponent('Color', '#ee7725', 'D747E6AE-C383-4E22-8846-71518E3DD06F', {
            selectiontype: 'Color Picker'
        }),
        DefinedValueGallery: GetFieldTypeGalleryComponent( 'DefinedValue (WIP)', 'af28af43-8461-41ac-a2c5-85122712be96', '59D5A94C-94A0-4630-B80A-BB25697D74C7', {
            definedtype: '92',
            allowmultiple: '',
            displaydescription: 'true',
            enhancedselection: '',
            includeInactive: '',
            AllowAddingNewValues: '',
            RepeatColumns: ''
        } )
    },
    template: `
<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-flask"></i>
        Obsidian Field Type Gallery
    </template>
    <template v-slot:default>
        <TextGallery />
        <DateGallery />
        <SingleSelectGallery />
        <MemoGallery />
        <BooleanGallery />
        <PhoneNumberGallery />
        <GenderGallery />
        <ColorGallery />
        <DefinedValueGallery />
    </template>
</PaneledBlockTemplate>`
} );
