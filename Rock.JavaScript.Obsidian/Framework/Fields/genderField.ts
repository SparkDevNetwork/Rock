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
import { toNumberOrNull } from "../Services/number";
import { PublicAttributeValue } from "../ViewModels";
import { FieldTypeBase } from "./fieldType";

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
    public override getTextValue(value: PublicAttributeValue): string {
        return value.textValue || "Unknown";
    }

    public override getTextValueFromConfiguration(value: string, _configurationValues: Record<string, string>): string | null {
        const numberValue = toNumberOrNull(value);

        if (numberValue === 0) {
            return "Unknown";
        }
        else if (numberValue === 1) {
            return "Male";
        }
        else if (numberValue === 2) {
            return "Female";
        }
        else {
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
