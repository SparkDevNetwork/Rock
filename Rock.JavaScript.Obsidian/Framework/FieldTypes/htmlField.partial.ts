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

export const enum ConfigurationValueKey {
    Toolbar = "toolbar",
    DocumentFolderRoot = "documentfolderroot",
    ImageFolderRoot = "imagefolderroot",
    UserSpecificRoot = "userspecificroot",
    CondensedHtml = "condensedHtml",
    EncryptedDocumentFolderRoot = "encrypteddocumentfolderroot",
    EncryptedImageFolderRoot = "encryptedimagefolderroot"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./htmlFieldComponents")).EditComponent;
});

// Load the configuration component only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./htmlFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the HTML field.
 */
export class HtmlFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        return escapeHtml(value ?? "");
    }

    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string {
        return isEscaped ? this.getTextValue(value, configurationValues) : value;
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string {
        const html = configurationValues?.[ConfigurationValueKey.CondensedHtml] ?? value ?? "";
        return isEscaped ? escapeHtml(html) : html;
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }
}