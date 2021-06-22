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
import { Component, PropType } from 'vue';
import { Guid, normalize } from '../Util/Guid';

const fieldTypeComponentPaths: Record<Guid, Component> = {};

export interface ConfigurationValue
{
    Name: string;
    Description: string;
    Value: string;
}

export type ConfigurationValues = Record<string, ConfigurationValue>;

/**
 * Gets the configuration value's value using the case insensitive key.
 * @param key
 * @param configurationValues
 */
export function getConfigurationValue ( key: string | null, configurationValues: ConfigurationValues | null ): string
{
    key = ( key || '' ).toLowerCase().trim();

    if ( !configurationValues || !key )
    {
        return '';
    }

    const objectKey = Object.keys( configurationValues ).find( k => k.toLowerCase().trim() === key );

    if ( !objectKey )
    {
        return '';
    }

    const configObject = configurationValues[ objectKey ];
    return configObject?.Value || '';
}

export type FieldTypeModule = {
    fieldTypeGuid: Guid;
    component: Component;
};

export function getFieldTypeProps ()
{
    return {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        isEditMode: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        configurationValues: {
            type: Object as PropType<ConfigurationValues>,
            default: () => ( {} )
        }
    };
}

export function registerFieldType ( fieldTypeGuid: Guid, component: Component )
{
    const normalizedGuid = normalize( fieldTypeGuid )!;

    const dataToExport: FieldTypeModule = {
        fieldTypeGuid: normalizedGuid,
        component: component
    };

    if ( fieldTypeComponentPaths[ normalizedGuid ] )
    {
        console.error( `Field type "${fieldTypeGuid}" is already registered` );
    }
    else
    {
        fieldTypeComponentPaths[ normalizedGuid ] = component;
    }

    return dataToExport;
}

export function getFieldTypeComponent ( fieldTypeGuid: Guid ): Component | null
{
    const field = fieldTypeComponentPaths[ normalize( fieldTypeGuid )! ];

    if ( field )
    {
        return field;
    }

    console.error( `Field type "${fieldTypeGuid}" was not found` );
    return null;
}