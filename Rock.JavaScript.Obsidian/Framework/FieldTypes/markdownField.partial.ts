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
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { stringComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { escapeHtml, truncate } from "@Obsidian/Utility/stringUtils";

const enum ConfigurationValueKey  {
    HtmlValue = "htmlvalue"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./markdownFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./markdownFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Markdown field.
 */
export class MarkdownFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        return value;
    }

    public override getHtmlValue(_value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string {
        return isEscaped ? escapeHtml(configurationValues[ConfigurationValueKey.HtmlValue]) : configurationValues[ConfigurationValueKey.HtmlValue];
    }

    public override getCondensedTextValue(value: string, configurationValues: Record<string, string>): string {
        return truncate(this.getTextValue(value, configurationValues), 100);
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string {
        let htmlValue = truncate(this.getHtmlValue(value, configurationValues, false), 100).trim();

        if (htmlValue.startsWith("<p>") && htmlValue.endsWith("</p>")) {
            htmlValue = htmlValue.substring(3, htmlValue.length - 4);
        }
        return isEscaped ? escapeHtml(htmlValue) : htmlValue;
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(_configurationValues: Record<string, string>): ComparisonType {
        return stringComparisonTypes;
    }
}
