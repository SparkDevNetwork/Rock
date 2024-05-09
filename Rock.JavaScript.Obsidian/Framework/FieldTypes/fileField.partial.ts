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
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { FieldTypeBase } from "./fieldType";

/**
 * The key names for the configuration properties available when editing the
 * configuration of a File field type.
 */
export const enum ConfigurationPropertyKey {
    /** The binary file types available to pick from. */
    BinaryFileTypes = "binaryFileTypes"
}

/**
 * The configuration value keys used by the configuraiton and edit controls.
 */
export const enum ConfigurationValueKey {
    /** The unique identifier of the BinaryFileType to use for uploads. */
    BinaryFileType = "binaryFileType",
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./fileFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./fileFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the File field.
 */
export class FileFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        try {
            const realValue = JSON.parse(value) as ListItemBag;

            return realValue.text ?? "";
        }
        catch {
            return value;
        }
    }

    public override getHtmlValue(value: string, _configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        try {
            const realValue = JSON.parse(value ?? "") as ListItemBag;

            const html = `<a href="/GetFile.ashx?guid=${realValue.value}" title="${escapeHtml(realValue.text ?? "")}" class="btn btn-xs btn-default">View</a>`;

            if (isEscaped) {
                escapeHtml(html);
            }

            return html;
        }
        catch {
            if (isEscaped) {
                escapeHtml(value ?? "");
            }

            return value ?? "";
        }
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return ComparisonType.IsBlank | ComparisonType.IsNotBlank;
    }
}
