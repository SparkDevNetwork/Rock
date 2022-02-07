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
import { ClientAttributeValue, ClientEditableAttributeValue } from "../ViewModels";
import { toNumberOrNull } from "../Services/number";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./genderFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./genderFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Gender field.
 */
export class GenderFieldType extends FieldTypeBase {
    public override getTextValue(value: ClientAttributeValue): string {
        return value.textValue || "Unknown";
    }

    public override updateTextValue(value: ClientEditableAttributeValue): void {
        const numberValue = toNumberOrNull(value.value);

        if (numberValue === 0) {
            value.textValue = "Unknown";
        }
        else if (numberValue === 1) {
            value.textValue = "Male";
        }
        else if (numberValue === 2) {
            value.textValue = "Female";
        }
        else {
            value.textValue = "";
        }
    }

    public override getEditComponent(_value: ClientAttributeValue): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
