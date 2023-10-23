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
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    EnabledSlidingDateRangeTypes = "enabledSlidingDateRangeTypes",
    EnabledSlidingDateRangeUnits = "enabledSlidingDateRangeUnits",
    TimeUnitTypes = "timeUnitTypes",
    SlidingDateRangeTypes = "slidingDateRangeTypes"

}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./slidingDateRangeFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./slidingDateRangeFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Integer field.
 */
export class SlidingDateRangeFieldType extends FieldTypeBase {
    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}