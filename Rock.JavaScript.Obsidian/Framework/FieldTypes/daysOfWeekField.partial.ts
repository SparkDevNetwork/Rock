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
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { containsComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { DayOfWeek } from "@Obsidian/Enums/Controls/dayOfWeek";
import { FieldTypeBase } from "./fieldType";


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./daysOfWeekFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./daysOfWeekFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the DaysOfWeek field.
 */
export class DaysOfWeekFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        if (value === null || value === undefined || value === "") {
            return "";
        }

        return value.split(",")
            .map(v => {
                const dayValue = toNumberOrNull(v);

                if (dayValue === null) {
                    return "";
                }
                else {
                    switch (dayValue) {
                        case DayOfWeek.Sunday:
                            return "Sunday";

                        case DayOfWeek.Monday:
                            return "Monday";

                        case DayOfWeek.Tuesday:
                            return "Tuesday";

                        case DayOfWeek.Wednesday:
                            return "Wednesday";

                        case DayOfWeek.Thursday:
                            return "Thursday";

                        case DayOfWeek.Friday:
                            return "Friday";

                        case DayOfWeek.Saturday:
                            return "Saturday";

                        default:
                            return "";
                    }
                }
            })
            .filter(v => v != "")
            .join(", ");
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
}
