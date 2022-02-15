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
import { ComparisonType, numericComparisonTypes } from "../Reporting/comparisonType";
import { toCurrencyOrNull } from "../Services/number";
import { FieldTypeBase } from "./fieldType";

// The components can be quite large, so load only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./currencyFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./currencyFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Currency field.
 */
export class CurrencyFieldType extends FieldTypeBase {
    public override getTextValueFromConfiguration(value: string, _configurationValues: Record<string, string>): string | null {
        return toCurrencyOrNull(value) ?? "";
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    override getSupportedComparisonTypes(): ComparisonType {
        return numericComparisonTypes;
    }
}
