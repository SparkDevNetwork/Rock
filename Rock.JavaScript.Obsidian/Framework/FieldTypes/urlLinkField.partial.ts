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

export const enum ConfigurationValueKey {
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
        const shouldAlwaysShowCondensed = asBoolean(configurationValues[ConfigurationValueKey.ShouldAlwaysShowCondensed]);
        const textValue = this.getTextValue(value, configurationValues);

        let htmlValue = "";
        if (textValue) {
            if (!shouldAlwaysShowCondensed) {
                htmlValue = `<a href="${textValue}">${textValue}</a>`;
            }
            else {
                htmlValue = textValue;
            }
        }

        if (isEscaped) {
            return escapeHtml(htmlValue);
        }

        return htmlValue;
    }

    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        const shouldAlwaysShowCondensed = asBoolean(configurationValues[ConfigurationValueKey.ShouldAlwaysShowCondensed]);
        if (shouldAlwaysShowCondensed) {
            return `<a href="${value}">${value}</a>`;
        }

        return value;
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
