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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { getStandardFilterComponent } from "./utils";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./universalItemSearchPickerFieldComponents")).EditComponent;
});

// The filter component can be quite large, so load it only as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./universalItemSearchPickerFieldComponents")).FilterComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./universalItemFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Universal Tree Item Picker field types.
 */
export class UniversalItemSearchPickerFieldType extends FieldTypeBase {
    public override getTextValue(value: string): string {
        try {
            const bag = JSON.parse(value) as ListItemBag;

            return bag.text ?? "";
        }
        catch {
            return "";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getFilterComponent(): Component | null {
        return getStandardFilterComponent(this.getSupportedComparisonTypes(), filterComponent);
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return ComparisonType.EqualTo | ComparisonType.NotEqualTo;
    }
}
