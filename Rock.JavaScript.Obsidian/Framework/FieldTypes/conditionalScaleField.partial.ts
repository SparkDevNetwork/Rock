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
import { Guid } from "@Obsidian/Types";
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { numericComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";

export const enum ConfigurationKey {
    ConfigurationJSON = "ConfigurationJSON"
}

export type ClientValue = {
    label: string,
    highValue: number | null,
    lowValue: number | null,
    color: string,
    rangeIndex: number,
    guid: Guid
};



// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./conditionalScaleFieldComponents")).EditComponent;
});

// Load the configuration component as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./conditionalScaleFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Conditional Scale field.
 */
export class ConditionalScaleFieldType extends FieldTypeBase {
    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return numericComparisonTypes;
    }

    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const scaleLabelHtml = this.getScaleLabelHtml(value, configurationValues);

        if (scaleLabelHtml !== null) {
            return isEscaped ? escapeHtml(scaleLabelHtml) : scaleLabelHtml;
        }

        return super.getHtmlValue(value, configurationValues, isEscaped);
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const scaleLabelHtml = this.getScaleLabelHtml(value, configurationValues);

        if (scaleLabelHtml !== null) {
            return isEscaped ? escapeHtml(scaleLabelHtml) : scaleLabelHtml;
        }

        return super.getCondensedHtmlValue(value, configurationValues, isEscaped);
    }

    /**
     * Builds the colored scale label markup for a value, matching the value's
     * label against the configured ranges to find its color. This mirrors the
     * server's GetHtmlValue so the value renders as the same badge it does in
     * WebForms. Returns null when there is no value or no matching colored
     * range, so the caller can fall back to the default (plain text) rendering.
     */
    private getScaleLabelHtml(value: string, configurationValues: Record<string, string>): string | null {
        if (!value) {
            return null;
        }

        try {
            const ranges = JSON.parse(configurationValues[ConfigurationKey.ConfigurationJSON] ?? "[]") as ClientValue[];
            const matchingRange = ranges.find(range => range.label === value);

            if (matchingRange?.color) {
                return `<span class="label scale-label" style="background-color:${matchingRange.color}">${escapeHtml(value)}</span>`;
            }
        }
        catch {
            // Malformed configuration - fall back to the default rendering.
        }

        return null;
    }
}
