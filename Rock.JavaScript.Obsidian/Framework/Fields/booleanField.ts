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
import { asBooleanOrNull } from "../Services/boolean";

export const enum ConfigurationValueKey {
    BooleanControlType = "BooleanControlType",
    FalseText = "falsetext",
    TrueText = "truetext"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./booleanFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./booleanFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Boolean field.
 */
export class BooleanFieldType extends FieldTypeBase {
    public override getCondensedTextValue(value: ClientAttributeValue): string {
        const boolValue = asBooleanOrNull(value.value);

        if (boolValue === null) {
            return "";
        }
        else if (boolValue === true) {
            return "Y";
        }
        else {
            return "N";
        }
    }

    public override updateTextValue(value: ClientEditableAttributeValue): void {
        const boolValue = asBooleanOrNull(value.value);

        if (boolValue === null) {
            value.textValue = "";
        }
        else if (boolValue === true) {
            value.textValue = value.configurationValues?.[ConfigurationValueKey.TrueText] || "Yes";
        }
        else {
            value.textValue = value.configurationValues?.[ConfigurationValueKey.FalseText] || "No";
        }
    }

    public override getEditComponent(_value: ClientAttributeValue): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
