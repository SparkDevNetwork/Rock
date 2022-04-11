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
import { ComparisonValue } from "../Reporting/comparisonValue";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { FieldTypeBase } from "./fieldType";
import { getStandardFilterComponent } from "./utils";

export const enum ConfigurationValueKey {
    Values = "values",
    FieldType = "fieldtype",
    RepeatColumns = "repeatColumns",

    /** Only used during editing of the field type configuration. */
    CustomValues = "customValues"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./singleSelectFieldComponents")).EditComponent;
});

// Load the filter component only as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./singleSelectFieldComponents")).FilterComponent;
});

// Load the configuration component only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./singleSelectFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the SingleSelect field.
 */
export class SingleSelectFieldType extends FieldTypeBase {
    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        if (value === "") {
            return "";
        }

        try {
            const values = JSON.parse(configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];
            const selectedValues = values.filter(v => v.value === value);

            if (selectedValues.length >= 1) {
                return selectedValues[0].text ?? "";
            }
            else {
                return "";
            }
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

    public override getFilterComponent(): Component {
        return getStandardFilterComponent("Is", filterComponent);
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
}
