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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { binaryComparisonTypes, containsComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    Values = "values",
    EnhancedSelection = "enhancedselection",
    RepeatColumns = "repeatColumns",
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./reminderTypesFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./reminderTypesFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Reminder Types field.
 */
export class ReminderTypesFieldType extends FieldTypeBase {
    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        if (value === undefined || value === null || value === "") {
            return "";
        }

        try {
            const values = JSON.parse(configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            const userValues = value.split(",");
            const selectedValues = values.filter(o => userValues.includes(o.value ?? ""));

            return selectedValues.map(o => o.text).join(", ");
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

    public override getSupportedComparisonTypes(): ComparisonType {
        return binaryComparisonTypes | containsComparisonTypes;
    }
}
