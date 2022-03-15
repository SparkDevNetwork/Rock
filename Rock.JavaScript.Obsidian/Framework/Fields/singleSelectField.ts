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
import { ListItem } from "../ViewModels";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    Values = "values",
    FieldType = "fieldtype",
    RepeatColumns = "repeatColumns"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./singleSelectFieldComponents")).EditComponent;
});

/**
 * The field type handler for the SingleSelect field.
 */
export class SingleSelectFieldType extends FieldTypeBase {
    public override getTextValueFromConfiguration(value: string, configurationValues: Record<string, string>): string | null {
        if (value === "") {
            return "";
        }

        try {
            const values = JSON.parse(configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItem[];
            const selectedValues = values.filter(v => v.value === value);

            if (selectedValues.length >= 1) {
                return selectedValues[0].text;
            }
            else {
                return "";
            }
        }
        catch {
            return value;
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }
}
