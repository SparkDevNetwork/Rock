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
import { toNumberOrNull } from "../Services/number";

export const enum DayOfWeek {
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./dayOfWeekFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./dayOfWeekFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the DayOfWeek field.
 */
export class DayOfWeekFieldType extends FieldTypeBase {
    public override updateTextValue(value: ClientEditableAttributeValue): void {
        const dayValue = toNumberOrNull(value.value);

        if (dayValue === null) {
            value.textValue = "";
        }
        else {
            switch (dayValue) {
                case DayOfWeek.Sunday:
                    value.textValue = "Sunday";
                    break;

                case DayOfWeek.Monday:
                    value.textValue = "Monday";
                    break;

                case DayOfWeek.Tuesday:
                    value.textValue = "Tuesday";
                    break;

                case DayOfWeek.Wednesday:
                    value.textValue = "Wednesday";
                    break;

                case DayOfWeek.Thursday:
                    value.textValue = "Thursday";
                    break;

                case DayOfWeek.Friday:
                    value.textValue = "Friday";
                    break;

                case DayOfWeek.Saturday:
                    value.textValue = "Saturday";
                    break;

                default:
                    value.textValue = "";
                    break;
            }
        }
    }

    public override getEditComponent(_value: ClientAttributeValue): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
