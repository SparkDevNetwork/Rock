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
import { ComparisonType, containsComparisonTypes } from "../Reporting/comparisonType";
import { ComparisonValue } from "../Reporting/comparisonValue";
import { ListItem } from "../ViewModels";
import { PublicFilterableAttribute } from "../ViewModels/publicFilterableAttribute";
import { FieldTypeBase } from "./fieldType";
import { getStandardFilterComponent } from "./utils";

export const enum ConfigurationValueKey {
    Values = "values",
    RepeatColumns = "repeatColumns",
    RepeatDirection = "repeatDirection",
    EnhancedSelection = "enhancedselection",

    /** Only used during editing of the field type configuration. */
    CustomValues = "customValues"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./multiSelectFieldComponents")).EditComponent;
});

// Load the filter component only as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./multiSelectFieldComponents")).FilterComponent;
});

// Load the configuration component only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./multiSelectFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the MultiSelect field.
 */
export class MultiSelectFieldType extends FieldTypeBase {
    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        if (value === "") {
            return "";
        }

        try {
            const values = JSON.parse(configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItem[];
            const userValues = value.split(",");
            const selectedValues = values.filter(v => userValues.includes(v.value));

            return selectedValues.map(v => v.text).join(", ");
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
            const values = JSON.parse(configurationValues?.[ConfigurationValueKey.Values] ?? "[]") as ListItem[];
            const selectedValues = values.filter(v => rawValues.includes(v.value));

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
}
