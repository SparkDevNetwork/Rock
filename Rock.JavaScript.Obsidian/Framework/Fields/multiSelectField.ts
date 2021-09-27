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
import { FieldTypeBase } from "./fieldType";
import { ClientAttributeValue, ClientEditableAttributeValue, ListItem } from "../ViewModels";

export const enum ConfigurationValueKey {
    Values = "values",
    RepeatColumns = "repeatColumns",
    RepeatDirection = "repeatDirection",
    EnhancedSelection = "enhancedselection"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./multiSelectFieldComponents")).EditComponent;
});

/**
 * The field type handler for the MultiSelect field.
 */
export class MultiSelectFieldType extends FieldTypeBase {
    public override updateTextValue(value: ClientEditableAttributeValue): void {
        if (value.value === undefined || value.value === null || value.value === "") {
            value.textValue = "";
            return;
        }

        try {
            const values = JSON.parse(value.configurationValues?.[ConfigurationValueKey.Values] ?? "[]") as ListItem[];
            const userValues = value.value.split(",");
            const selectedValues = values.filter(v => userValues.includes(v.value));

            value.textValue = selectedValues.map(v => v.text).join(", ");
        }
        catch {
            value.textValue = value.value;
        }
    }

    public override getEditComponent(_value: ClientAttributeValue): Component {
        return editComponent;
    }
}
