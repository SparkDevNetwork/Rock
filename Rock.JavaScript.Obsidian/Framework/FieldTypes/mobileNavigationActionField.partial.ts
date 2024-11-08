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
import { MobileNavigationActionBag } from "@Obsidian/ViewModels/Controls/mobileNavigationActionBag";
import { FieldTypeBase } from "./fieldType";
import { MobileNavigationActionType } from "@Obsidian/Enums/Mobile/mobileNavigationActionType";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./mobileNavigationActionFieldComponents")).EditComponent;
});

// Load the configuration component only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./mobileNavigationActionFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Mobile Navigation Action field.
 */
export class MobileNavigationActionFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        const actionValue = JSON.parse(value || "{}") as MobileNavigationActionBag;

        if (!actionValue) {
            return "";
        }

        switch (actionValue.type) {
            case MobileNavigationActionType.None:
                return "None";

            case MobileNavigationActionType.PopPage:
                return `Pop ${actionValue.popCount ?? 1} page${actionValue.popCount == 1 ? "" : "s"}`;

            case MobileNavigationActionType.ResetToPage:
                return `Reset to '${actionValue.page?.text ?? "page"}'`;

            case MobileNavigationActionType.ReplacePage:
                return `Replace page with '${actionValue.page?.text ?? "page"}'`;

            case MobileNavigationActionType.PushPage:
                return `Push '${actionValue.page?.text ?? "page"}'`;

            case MobileNavigationActionType.DismissCoverSheet:
                return "Dismiss Cover Sheet";

            default:
                return "";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
