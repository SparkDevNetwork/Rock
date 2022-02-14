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
import { asBooleanOrNull } from "../Services/boolean";
import { PublicAttributeValue } from "../ViewModels";
import { FieldTypeBase } from "./fieldType";

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
    public override getCondensedTextValue(value: PublicAttributeValue): string {
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

    public override getTextValueFromConfiguration(value: string, configurationValues: Record<string, string>): string | null {
        const boolValue = asBooleanOrNull(value);

        if (boolValue === null) {
            return "";
        }
        else if (boolValue === true) {
            return configurationValues[ConfigurationValueKey.TrueText] || "Yes";
        }
        else {
            return configurationValues[ConfigurationValueKey.FalseText] || "No";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
