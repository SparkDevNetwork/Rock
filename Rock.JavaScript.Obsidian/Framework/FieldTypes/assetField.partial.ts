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
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { FileAsset } from "@Obsidian/ViewModels/Controls/fileAsset";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./assetFieldComponents")).EditComponent;
});

// Load the configuration component only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./assetFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Asset Storage Provider Type field.
 */
export class AssetFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        if (value) {
            try {
                // eslint-disable-next-line @typescript-eslint/naming-convention
                const asset = JSON.parse(value) as Partial<{Url: string}>;

                return asset?.Url ?? "";
            }
            catch {
                return value ?? "";
            }
        }
        else {
            return "";
        }
    }

    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string {
        const url = escapeHtml(this.getTextValue(value, configurationValues));
        return isEscaped ? url : `<a href="${url}">${url}</a>`;
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string {
        return this.getHtmlValue(value, configurationValues, isEscaped);
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}