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
import { dateComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { padLeft } from "@Obsidian/Utility/stringUtils";
import { FieldTypeBase } from "./fieldType";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./timeFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./timeFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Time field.
 */
export class TimeFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        const values = /^(\d+):(\d+)/.exec(value ?? "");

        if (values === null || values.length < 3) {
            return "";
        }

        let hour = toNumber(values[1]);
        const minute = toNumber(values[2]);
        const meridiem = hour >= 12 ? "PM" : "AM";

        if (hour > 12) {
            hour -= 12;
        }

        return `${hour}:${padLeft(minute.toString(), 2, "0")} ${meridiem}`;
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return dateComparisonTypes;
    }
}
