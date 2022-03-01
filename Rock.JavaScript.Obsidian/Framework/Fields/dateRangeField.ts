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
import { DateTimeFormat, RockDateTime } from "../Util/rockDateTime";
import { toNumber } from "../Services/number";


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./dateRangeFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./dateRangeFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Date Range field.
 */
export class DateRangeFieldType extends FieldTypeBase {
    public override updateTextValue(value: ClientEditableAttributeValue): void {
        const dateParts = (value.value ?? "").split(",");

        if (dateParts.length !== 2) {
            value.textValue = "";
            return;
        }

        const lowerDateParts = /^(\d+)-(\d+)-(\d+)/.exec(dateParts[0]);
        const upperDateParts = /^(\d+)-(\d+)-(\d+)/.exec(dateParts[1]);

        const lowerDate = lowerDateParts !== null ? RockDateTime.fromParts(toNumber(lowerDateParts[1]), toNumber(lowerDateParts[2]), toNumber(lowerDateParts[3])) : null;
        const upperDate = upperDateParts !== null ? RockDateTime.fromParts(toNumber(upperDateParts[1]), toNumber(upperDateParts[2]), toNumber(upperDateParts[3])) : null;

        if (lowerDate !== null && upperDate !== null) {
            value.textValue = `${lowerDate.toLocaleString(DateTimeFormat.DateShort)} to ${upperDate.toLocaleString(DateTimeFormat.DateShort)}`;
        }
        else if (lowerDate !== null) {
            value.textValue = `from ${lowerDate.toLocaleString(DateTimeFormat.DateShort)}`;
        }
        else if (upperDate !== null) {
            value.textValue = `through ${upperDate.toLocaleString(DateTimeFormat.DateShort)}`;
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
}
