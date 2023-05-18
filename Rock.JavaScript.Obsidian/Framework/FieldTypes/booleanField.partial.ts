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
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { asBooleanOrNull } from "@Obsidian/Utility/booleanUtils";
import { defineAsyncComponent } from "@Obsidian/Utility/component";
import { FieldTypeBase } from "./fieldType";
import { getStandardFilterComponent } from "./utils";

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

// Only load the filter component as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./booleanFieldComponents")).FilterComponent;
});

/**
 * The field type handler for the Boolean field.
 */
export class BooleanFieldType extends FieldTypeBase {
    public override getCondensedTextValue(value: string, _configurationValues: Record<string, string>): string {
        const boolValue = asBooleanOrNull(value);

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

    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
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

    public override getSupportedComparisonTypes(): ComparisonType {
        return ComparisonType.EqualTo | ComparisonType.NotEqualTo;
    }

    public override getFilterComponent(): Component {
        return getStandardFilterComponent(this.getSupportedComparisonTypes(), filterComponent);
    }
}
