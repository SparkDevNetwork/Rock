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
import { stringComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { FieldTypeBase } from "./fieldType";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { isUrlSchemeSafe } from "@Obsidian/Utility/url";

export const enum ConfigurationKey {
    ShouldRequireTrailingForwardSlash = "ShouldRequireTrailingForwardSlash",
    ShouldAlwaysShowCondensed = "ShouldAlwaysShowCondensed"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./urlLinkFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./urlLinkFieldComponents")).ConfigurationComponent;
});
/**
 * The field type handler for the Email field.
 */
export class UrlLinkFieldType extends FieldTypeBase {
    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const shouldAlwaysShowCondensed = asBoolean(configurationValues[ConfigurationKey.ShouldAlwaysShowCondensed]);
        const textValue = this.getTextValue(value, configurationValues);

        let htmlValue = "";
        if (textValue) {
            if (!shouldAlwaysShowCondensed) {
                htmlValue = this.getLinkHtml(textValue);
            }
            else {
                // The text value is the raw link markup, so build the safe
                // link from the value instead.
                htmlValue = this.getLinkHtml(value);
            }
        }

        if (isEscaped) {
            return escapeHtml(htmlValue);
        }

        return htmlValue;
    }

    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        const shouldAlwaysShowCondensed = asBoolean(configurationValues[ConfigurationKey.ShouldAlwaysShowCondensed]);
        if (shouldAlwaysShowCondensed) {
            return `<a href="${value}">${value}</a>`;
        }

        return value;
    }

    /**
     * Gets the HTML link for the URL. A URL that does not use a safe scheme
     * is returned as encoded text with no link.
     *
     * @param value The URL to be displayed.
     *
     * @returns The HTML to display the URL.
     */
    private getLinkHtml(value: string): string {
        /*
            10/16/2026 - MSE

            The stored value can come from public forms (such as Workflow
            Entry) that do not check its format, and it is displayed with
            v-html. It is always encoded, and only an http or https (or
            relative) URL becomes an href.

            Reason: Stored values were rendered as raw HTML.
        */
        if (!value) {
            return "";
        }

        if (isUrlSchemeSafe(value)) {
            return `<a href="${escapeHtml(value)}">${escapeHtml(value)}</a>`;
        }

        return escapeHtml(value);
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return stringComparisonTypes;
    }
}
