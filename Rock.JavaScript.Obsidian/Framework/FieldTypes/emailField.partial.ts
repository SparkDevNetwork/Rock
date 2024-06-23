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
import { FieldTypeBase } from "./fieldType";
import { getStandardFilterComponent } from "./utils";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./emailFieldComponents")).EditComponent;
});

// The filter component can be quite large, so load it only as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./emailFieldComponents")).FilterComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./emailFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Email field.
 */
export class EmailFieldType extends FieldTypeBase {
    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const textValue = this.getTextValue(value, configurationValues);

        const html = textValue ? `<a href="mailto:${textValue}">${textValue}</a>` : "";

        if (isEscaped) {
            escapeHtml(html);
        }

        return html;
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getFilterComponent(): Component {
        return getStandardFilterComponent(this.getSupportedComparisonTypes(), filterComponent);
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return stringComparisonTypes;
    }
}
