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
import { FieldTypeBase } from "./fieldType";

/**
 * The configuration value keys used by the configuraiton and edit controls.
 */
export const enum ConfigurationValueKey {
    /** Allowed Picker Modes */
    AllowedPickerModes = "allowedPickerModes",
    CurrentPickerMode = "currentPickerMode"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./locationFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./locationFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the LocationList field.
 */
export class LocationFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        try {
            const locationItemBag = JSON.parse(value);
            return locationItemBag.text ?? "";
        }
        catch {
            return value ?? "";
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
