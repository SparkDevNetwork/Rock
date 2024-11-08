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
import { defineAsyncComponent } from "@Obsidian/Utility/component";
import { FieldTypeBase } from "./fieldType";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { getStandardFilterComponent } from "./utils";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./universalItemTreePickerFieldComponents")).EditComponent;
});

// The filter component can be quite large, so load it only as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./universalItemTreePickerFieldComponents")).FilterComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./universalItemFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Universal Item Tree Picker field types.
 */
export class UniversalItemTreePickerFieldType extends FieldTypeBase {
    public override getTextValue(value: string): string {
        try {
            const values = JSON.parse(value) as ListItemBag | ListItemBag[];

            if (Array.isArray(values)) {
                return values.map(v => v.text ?? "").join(", ");
            }
            else {
                return values.text ?? "";
            }
        }
        catch {
            return "";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getFilterComponent(configurationValues: Record<string, string>): Component | null {
        return getStandardFilterComponent(this.getSupportedComparisonTypes(configurationValues), filterComponent);
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(configurationValues: Record<string, string>): ComparisonType {
        if (asBoolean(configurationValues["isMultiple"]) === true) {
            return ComparisonType.Contains | ComparisonType.DoesNotContain | ComparisonType.IsBlank;
        }
        else {
            return ComparisonType.EqualTo | ComparisonType.NotEqualTo;
        }
    }

    public override getFilterValueText(value: ComparisonValue): string {
        try {
            const values = JSON.parse(value.value) as ListItemBag | ListItemBag[];

            if (Array.isArray(values)) {
                return values.map(v => `'${v.text ?? ""}'`).join(" OR ");
            }
            else {
                return `'${values.text ?? ""}'`;
            }
        }
        catch {
            return "";
        }
    }
}
