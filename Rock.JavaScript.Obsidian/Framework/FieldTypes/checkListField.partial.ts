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
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { containsComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { FieldTypeBase } from "./fieldType";
import { getStandardFilterComponent } from "./utils";
import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";

export const enum ConfigurationValueKey {
    Values = "values",
    RepeatColumns = "repeatColumns",
    ListItems = "listItems"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./checkListFieldComponents")).EditComponent;
});

// Load the filter component only as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./checkListFieldComponents")).FilterComponent;
});

// Load the configuration component only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./checkListFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Check List field.
 */
export class CheckListFieldType extends FieldTypeBase {
    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        if (value === "") {
            return "";
        }

        try {
            const values = JSON.parse(configurationValues[ConfigurationValueKey.ListItems] ?? "[]") as KeyValueItem[];
            const userValues = value.split(",");
            const selectedValues = values.filter(v => userValues.includes(v.key ?? ""));

            return selectedValues.map(v => v.value).join(", ");
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
        return containsComparisonTypes;
    }

    public override getFilterComponent(): Component {
        return getStandardFilterComponent(this.getSupportedComparisonTypes(), filterComponent);
    }

    public override getFilterValueText(value: ComparisonValue, configurationValues: Record<string, string>): string {
        if (value.value === "") {
            return "";
        }

        try {
            const rawValues = value.value.split(",");
            const values = JSON.parse(configurationValues?.[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            const selectedValues = values.filter(v => rawValues.includes(v.value ?? ""));

            if (selectedValues.length >= 1) {
                return `'${selectedValues.map(v => v.value).join("' OR '")}'`;
            }
            else {
                return "";
            }
        }
        catch {
            return value.value;
        }
    }

    public override doesValueMatchFilter(value: string, filterValue: ComparisonValue, _configurationValues: Record<string, string>): boolean {
        const selectedValues = (filterValue.value ?? "").split(",").filter(v => v !== "").map(v => v.toLowerCase());
        let comparisonType = filterValue.comparisonType;

        if (comparisonType === ComparisonType.EqualTo) {
            // Treat EqualTo as if it were Contains.
            comparisonType = ComparisonType.Contains;
        }
        else if (comparisonType === ComparisonType.NotEqualTo) {
            // Treat NotEqualTo as if it were DoesNotContain.
            comparisonType = ComparisonType.DoesNotContain;
        }

        if (comparisonType === ComparisonType.IsBlank) {
            return value === "";
        }
        else if (comparisonType === ComparisonType.IsNotBlank) {
            return value !== "";
        }

        if (selectedValues.length > 0) {
            const userValues = value?.split(",").filter(v => v !== "").map(v => v.toLowerCase()) ?? [];

            if (comparisonType === ComparisonType.Contains) {
                let matchedCount = 0;

                for (const userValue of userValues) {
                    if (selectedValues.includes(userValue)) {
                        matchedCount += 1;
                    }
                }

                return matchedCount > 0;
            }
            else {
                let matchedCount = 0;

                for (const userValue of userValues) {
                    if (selectedValues.includes(userValue)) {
                        matchedCount += 1;
                    }
                }

                return matchedCount !== selectedValues.length;
            }
        }

        return false;
    }
}
