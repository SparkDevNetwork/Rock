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

export type PublicValueItem = {
    isLegacyProtectMyMinistry?: boolean;
    isFileBased?: boolean;
    providerName?: string;
    binaryFileGuid?: string;
    fileName?: string;
    recordKey?: string;
    providerEntityTypeGuid?: string;
    providerEntityTypeId?: number;
};

export const enum ConfigurationValueKey {
    BinaryFileType = "binaryFileType",
    FilePath = "filePath",
    OriginalProviderEntityTypeGuid = "originalProviderEntityTypeGuid",
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./backgroundCheckFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./backgroundCheckFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the TimeZone field.
 */
export class BackgroundCheckFieldType extends FieldTypeBase {
    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {

        if (!value) {
            return "";
        }

        let htmlValue = "Report";
        try {
            const clientValue = JSON.parse(value || "{}") as PublicValueItem;

            const filePath = configurationValues[ConfigurationValueKey.FilePath];
            const entityTypeGuid = configurationValues[ConfigurationValueKey.OriginalProviderEntityTypeGuid];

            if (clientValue.isFileBased) {
                htmlValue = `<a href='${filePath}?EntityTypeGuid=${entityTypeGuid}&RecordKey=${clientValue.binaryFileGuid}' title='${escapeHtml(clientValue.fileName ?? "")}' class='btn btn-xs btn-default'>View</a>`;
            }
            else if (clientValue.recordKey) {
                htmlValue = `<a href='${filePath}?EntityTypeGuid=${entityTypeGuid}&RecordKey=${clientValue.recordKey}' title='${escapeHtml(clientValue.fileName ?? "")}' class='btn btn-xs btn-default'>View</a>`;
            }
        }
        catch {
            return htmlValue;
        }

        if (isEscaped) {
            return escapeHtml(htmlValue);
        }

        return htmlValue;
    }

    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        return value ?? "";
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