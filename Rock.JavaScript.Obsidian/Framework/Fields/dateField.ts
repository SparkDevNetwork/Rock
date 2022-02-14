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
import { asBoolean } from "../Services/boolean";
import { toNumber } from "../Services/number";
import { RockDateTime } from "../Util/rockDateTime";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    Format = "format",
    DisplayDiff = "displayDiff",
    DisplayCurrentOption = "displayCurrentOption",
    DatePickerControlType = "datePickerControlType",
    FutureYearCount = "futureYearCount"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./dateFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./dateFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Date field.
 */
export class DateFieldType extends FieldTypeBase {
    public override getTextValueFromConfiguration(value: string, configurationValues: Record<string, string>): string | null {
        if (this.isCurrentDateValue(value)) {
            const parts = (value ?? "").split(":");
            const diff = parts.length === 2 ? toNumber(parts[1]) : 0;

            if (diff === 1) {
                return "Current Date plus 1 day";
            }
            else if (diff > 0) {
                return `Current Date plus ${diff} days`;
            }
            else if (diff === -1) {
                return "Current Date minus 1 day";
            }
            else if (diff < 0) {
                return `Current Date minus ${Math.abs(diff)} days`;
            }
            else {
                return "Current Date";
            }
        }
        else if (value) {
            const dateValue = RockDateTime.parseISO(value);
            const dateFormatTemplate = configurationValues[ConfigurationValueKey.Format] || "MM/dd/yyy";

            if (dateValue !== null) {
                let textValue = dateValue.toASPString(dateFormatTemplate);

                const displayDiff = asBoolean(configurationValues[ConfigurationValueKey.DisplayDiff]);

                if (displayDiff === true) {
                    textValue = `${textValue} ${dateValue.toElapsedString()}`;
                }

                return textValue;
            }
            else {
                return "";
            }
        }
        else {
            return "";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    private isCurrentDateValue(value: string): boolean {
        return value.indexOf("CURRENT") === 0;
    }
}
