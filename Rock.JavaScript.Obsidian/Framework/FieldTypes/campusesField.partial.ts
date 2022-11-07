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
import { FieldTypeBase } from "./fieldType";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { ComparisonType } from "@Obsidian/Types/Reporting/comparisonType";

export const enum ConfigurationValueKey {
    Values = "values",
    EnhancedSelection = "enhancedselection",
    RepeatColumns = "repeatColumns",
    IncludeInactive = "includeInactive",
    FilterCampusTypes = "filterCampusTypes",
    FilterCampusStatus = "filterCampusStatus",
    SelectableCampuses = "selectableCampuses"
}

export const enum ConfigurationPropertyKey {
    Campuses = "campuses",
    CampusTypes = "campusTypes",
    CampusStatuses = "campusStatuses"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./campusesFieldComponents")).EditComponent;
});

// Load the configuration component only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./campusesFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Campuses field.
 */
export class CampusesFieldType extends FieldTypeBase {
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
            let matched = selectedValues.includes(value.toLowerCase());

            if (comparisonType === ComparisonType.DoesNotContain) {
                matched = !matched;
            }

            return matched;
        }

        return false;
    }
}
