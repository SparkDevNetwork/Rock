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
        json (): string
        {
            return JSON.stringify( this.attributeValues.map( av => ( {
                Name: av.Attribute.Name,
                Value: av.Value
            } ) ), null, 4 );
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
            <h4>Raw Data</h4>
            <pre>{{json}}</pre>
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
    </template>
</PaneledBlockTemplate>`
} );
