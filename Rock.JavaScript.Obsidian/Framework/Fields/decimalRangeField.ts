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
import { toNumberOrNull } from "../Services/number";
import { FieldTypeBase } from "./fieldType";


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./decimalRangeFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./decimalRangeFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Decimal Range field.
 */
export class DecimalRangeFieldType extends FieldTypeBase {
    public override getTextValueFromConfiguration(value: string, _configurationValues: Record<string, string>): string | null {
        if (value === null || value === undefined || value === "" || value === ",") {
            return "";
        }

        const numbers = value.split(",").map(v => toNumberOrNull(v));

        // If there are not two components then it's not valid, or if both
        // components are not numbers then it's not valid.
        if (numbers.length !== 2 || (numbers[0] === null && numbers[1] === null)) {
            return "";
        }

        if (numbers[0] === null) {
            return `through ${numbers[1]}`;
        }
        else if (numbers[1] === null) {
            return `from ${numbers[0]}`;
        }
        else {
            return `${numbers[0]} to ${numbers[1]}`;
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override isFilterable(): boolean {
        return false;
    }
}
