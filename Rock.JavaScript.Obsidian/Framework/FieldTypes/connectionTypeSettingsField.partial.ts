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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/** Wire shape of the composite edit value. Each slot is independently nullable. */
export type ConnectionTypeSettings = {
    connectionType: ListItemBag | null;
    connectionOpportunity: ListItemBag | null;
    connectionStatus: ListItemBag | null;
    connectionTypeSource: ListItemBag | null;
};

/** Configuration value keys sent by the field type's `GetPublicEditConfigurationValues`. */
export const ConfigurationValueKey = {
    ConnectionTypes: "connectionTypes",
    ConnectionOpportunitiesByType: "connectionOpportunitiesByType",
    ConnectionStatusesByType: "connectionStatusesByType",
    ConnectionTypeSourcesByType: "connectionTypeSourcesByType"
} as const;

const editComponent = defineAsyncComponent(async () => {
    return (await import("./connectionTypeSettingsFieldComponents")).EditComponent;
});

const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./connectionTypeSettingsFieldComponents")).ConfigurationComponent;
});

export class ConnectionTypeSettingsFieldType extends FieldTypeBase {
    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}
