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
import { toNumber } from "../Services/number";
import { padLeft } from "../Services/string";

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
    public override updateTextValue(value: ClientEditableAttributeValue): void {
        const values = /^(\d+):(\d+)/.exec(value.value ?? "");

        if (values === null || values.length < 3) {
            value.textValue = "";
            return;
        }

        let hour = toNumber(values[1]);
        const minute = toNumber(values[2]);
        const meridiem = hour >= 12 ? "PM" : "AM";

        if (hour > 12) {
            hour -= 12;
        }

        value.textValue = `${hour}:${padLeft(minute.toString(), 2, "0")} ${meridiem}`;
    }

    public override getEditComponent(_value: ClientAttributeValue): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
