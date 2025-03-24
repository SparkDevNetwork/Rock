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
import { Component } from "vue";
import { defineAsyncComponent } from "@Obsidian/Utility/component";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { List } from "@Obsidian/Utility/linq";
import { FieldTypeBase } from "./fieldType";

/**
 * The key names for the configuration properties available when editing the
 * configuration of a DefinedValue field type.
 */
export const enum ConfigurationPropertyKey {
    /** The defined types available to be picked. */
    DefinedTypes = "definedTypes"
}

export const enum ConfigurationValueKey {
    Values = "values",
    DisplayDescription = "displaydescription",
    DefinedType = "definedtype",
    DefinedTypes = "definedTypes"
}

export type ValueItem = {
    value: string,
    text: string,
    description: string
};

export type ClientValue = {
    value?: string;
    text?: string;
    description?: string;
};


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./definedValueRangeFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./definedValueRangeFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Defined Value Range field.
 */
export class DefinedValueRangeFieldType extends FieldTypeBase {
    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        try {
            const clientValue = JSON.parse(value) as ClientValue;

            try {
                const values = new List(JSON.parse(configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[]);
                const displayDescription = asBoolean(configurationValues[ConfigurationValueKey.DisplayDescription]);
                const rawValues = (clientValue.value ?? "").split(",");

                if (rawValues.length !== 2) {
                    return value;
                }

                const lowerValue = values.firstOrUndefined(v => v?.value === rawValues[0]);
                const upperValue = values.firstOrUndefined(v => v?.value === rawValues[1]);

                if (lowerValue === undefined && upperValue === undefined) {
                    return "";
                }

                if (displayDescription) {
                    return `${lowerValue?.description ?? ""} to ${upperValue?.description ?? ""}`;
                }
                else {
                    return `${lowerValue?.text ?? ""} to ${upperValue?.text ?? ""}`;
                }
            }
            catch {
                return clientValue.value ?? "";
            }
        }
        catch {
            return value;
        }
    }

    public override getCondensedTextValue(value: string, _configurationValues: Record<string, string>): string {
        try {
            const clientValue = JSON.parse(value ?? "") as ClientValue;

            return clientValue.text ?? "";
        }
        catch {
            return value;
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override isFilterable(): boolean {
        return false;
    }
}
