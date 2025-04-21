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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { FieldTypeBase } from "./fieldType";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { resolveMergeFields } from "@Obsidian/Utility/lava";

/**
 * The key names for the configuration properties available when editing the
 * configuration of an Image field type.
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

    /** Determines if the rendered HTML should be clickable. */
    FormatAsLink = "formatAsLink",

    /** The Lava template to use when rendering as an html img tag. */
    ImageTagTemplate = "img_tag_template",

    /** The width property to use when rendering as an html img tag. */
    Width = "width",

    /** The height property to use when rendering as an html img tag. */
    Height = "height",

    /** The image url to use when rendering as an html img tag. */
    ImageUrl = "imageUrl"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./imageFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./imageFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Image field.
 */
export class ImageFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        try {
            const realValue = JSON.parse(value ?? "") as ListItemBag;

            if (!realValue.value) {
                return "";
            }

            return realValue.text ?? "";
        }
        catch {
            return value;
        }
    }

    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        try {
            const realValue = JSON.parse(value ?? "") as ListItemBag;

            if (!realValue.value) {
                return "";
            }

            const width = configurationValues[ConfigurationValueKey.Width];
            const height = configurationValues[ConfigurationValueKey.Height];
            let imageUrl = configurationValues[ConfigurationValueKey.ImageUrl] ?? `/GetImage.ashx?guid=${realValue.value}`;
            let queryParms = "";

            if (width) {
                queryParms += `&width=${width}`;
            }

            if (height) {
                queryParms += `&height=${height}`;
            }

            if (!this.hasQueryParams(imageUrl)) {
                imageUrl += `?guid=${realValue.value}`;
            }

            const imageTagTemplate = configurationValues[ConfigurationValueKey.ImageTagTemplate];
            const formatAsLink = asBoolean(configurationValues[ConfigurationValueKey.FormatAsLink]);
            const mergeFields: Record<string, unknown> = {
                "ImageUrl": imageUrl + queryParms,
                "ImageGuid": realValue.value
            };

            const imageTag = resolveMergeFields(imageTagTemplate, mergeFields);

            const html = formatAsLink ? `<a href='${imageUrl}'>${imageTag}</a>` : imageTag;

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

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const width = configurationValues[ConfigurationValueKey.Width];

        if (!width) {
            configurationValues[ConfigurationValueKey.Width] = "120";
        }

        return this.getHtmlValue(value, configurationValues, isEscaped);
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

    private hasQueryParams(url: string): boolean {
        const { protocol, host } = window.location;
        const baseUrl = `${protocol}//${host}`;
        const urlObj = new URL(url, baseUrl);
        return urlObj.searchParams.toString() !== "";
    }
}

