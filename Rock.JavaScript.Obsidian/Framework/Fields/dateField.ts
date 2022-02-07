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
import { FieldTypeBase } from "./fieldType";
import { ClientAttributeValue, ClientEditableAttributeValue } from "../ViewModels";
import { asBoolean } from "../Services/boolean";
import { toNumber } from "../Services/number";
import { RockDateTime } from "../Util/rockDateTime";

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
    public override updateTextValue(value: ClientEditableAttributeValue): void {
        if (this.isCurrentDateValue(value)) {
            const parts = (value.value ?? "").split(":");
            const diff = parts.length === 2 ? toNumber(parts[1]) : 0;

            if (diff === 1) {
                value.textValue = "Current Date plus 1 day";
            }
            else if (diff > 0) {
                value.textValue = `Current Date plus ${diff} days`;
            }
            else if (diff === -1) {
                value.textValue = "Current Date minus 1 day";
            }
            else if (diff < 0) {
                value.textValue = `Current Date minus ${Math.abs(diff)} days`;
            }
            else {
                value.textValue = "Current Date";
            }
        }
        else if (value.value) {
            const dateValue = RockDateTime.parseISO(value.value);
            const dateFormatTemplate = value.configurationValues?.[ConfigurationValueKey.Format] || "MM/dd/yyy";

            if (dateValue !== null) {
                let textValue = dateValue.toASPString(dateFormatTemplate);

                const displayDiff = asBoolean(value.configurationValues?.[ConfigurationValueKey.DisplayDiff]);

                if (displayDiff === true) {
                    textValue = `${textValue} ${dateValue.toElapsedString()}`;
                }

                value.textValue = textValue;
            }
            else {
                value.textValue = "";
            }
        }
        else {
            value.textValue = "";
        }
    }

    public override getEditComponent(_value: ClientAttributeValue): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    private isCurrentDateValue(value: ClientAttributeValue): boolean {
        return value.value?.indexOf("CURRENT") === 0;
    }
}
