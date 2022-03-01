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
import { List } from "../Util/linq";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    Values = "values",
    KeyPrompt = "keyprompt",
    ValuePrompt = "valueprompt",
    DisplayValueFirst = "displayvaluefirst",
    AllowHtml = "allowhtml"
}

export type ValueItem = {
    value: string,
    text: string
};

export type ClientValue = {
    key: string,
    value: string
};


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./keyValueListFieldComponents")).EditComponent;
});

/**
 * The field type handler for the Key Value List field.
 */
export class KeyValueListFieldType extends FieldTypeBase {
    public override getTextValueFromConfiguration(value: string, configurationValues: Record<string, string>): string | null {
        try {
            const clientValues = JSON.parse(value ?? "[]") as ClientValue[];
            const configuredValues = new List(JSON.parse(configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[]);
            const values: string[] = [];

            for (const clientValue of clientValues) {
                const configuredValue = configuredValues.firstOrUndefined(v => v.value === clientValue.value);

                if (configuredValue !== undefined) {
                    values.push(`${clientValue.key}: ${configuredValue.text}`);
                }
                else {
                    values.push(`${clientValue.key}: ${clientValue.value}`);
                }
            }

            return values.join(", ");
        }
        catch {
            return "";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }
}
