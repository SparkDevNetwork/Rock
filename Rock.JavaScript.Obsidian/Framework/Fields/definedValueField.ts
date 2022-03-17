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
import { Component, defineAsyncComponent } from "vue";
import { asBoolean } from "../Services/boolean";
import { FieldTypeBase } from "./fieldType";

/**
 * The key names for the configuration properties available when editing the
 * configuration of a DefinedValue field type.
 */
export const enum ConfigurationPropertyKey {
    /** The defined types available to be picked. */
    DefinedTypes = "definedTypes",

    /** The defined values available to be picked. */
    DefinedValues = "definedValues"
}

/**
 * The configuration value keys used by the configuraiton and edit controls.
 */
export const enum ConfigurationValueKey {
    /** The unique identifier of the defined type currently selected. */
    DefinedType = "definedtype",

    /**
     * The unique identifiers of the defined values that can be selected
     * during editing.
     */
    SelectableValues = "selectableValues",

    /**
     * Contains "True" if the edit control should be rendered to allow
     * selecting multiple values.
     */
    AllowMultiple = "allowmultiple",

    /**
     * Contains "True" if the edit control should display descriptions instead
     * of values.
     */
    DisplayDescription = "displaydescription",

    /**
     * Contains "True" if the edit control should use enhanced selection.
     */
    EnhancedSelection = "enhancedselection",

    /** Contains "True" if in-active values should be included. */
    IncludeInactive = "includeInactive",

    /** Contains "True" if adding new values is permitted. */
    AllowAddingNewValues = "AllowAddingNewValues",

    /** The number of columns to use when multiple selection is allowed. */
    RepeatColumns = "RepeatColumns"
}

export type ValueItem = {
    value: string,
    text: string,
    description: string
};

export type ClientValue = {
    value: string,
    text: string,
    description: string
};


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./definedValueFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./definedValueFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Defined Value field.
 */
export class DefinedValueFieldType extends FieldTypeBase {
    public override getTextValueFromConfiguration(value: string, configurationValues: Record<string, string>): string | null {
        try {
            const clientValue = JSON.parse(value ?? "") as ClientValue;

            try {
                const values = JSON.parse(configurationValues[ConfigurationValueKey.SelectableValues] ?? "[]") as ValueItem[];
                const displayDescription = asBoolean(configurationValues[ConfigurationValueKey.DisplayDescription]);
                const rawValues = clientValue.value.split(",");

                return values.filter(v => rawValues.includes(v.value))
                    .map(v => displayDescription ? v.description : v.text)
                    .join(", ");
            }
            catch {
                return clientValue.value;
            }
        }
        catch {
            return "";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
