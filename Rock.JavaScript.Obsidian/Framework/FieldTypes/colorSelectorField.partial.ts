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
import { Component, nextTick } from "vue";
import { FieldTypeBase } from "./fieldType";
import { ConfigurationValueKey } from "./colorSelectorField.types.partial";
import { deserializeColors, deserializeValue, getSelectedColors, setCamouflagedClass } from "./colorSelectorField.utils.partial";
import { defineAsyncComponent } from "@Obsidian/Utility/component";
import guid from "@Obsidian/Utility/guid";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./colorSelectorComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./colorSelectorComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Color field.
 */
export class ColorSelectorFieldType extends FieldTypeBase {
    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        // value is a comma-separated list of guids/keys.
        // configurationValues["Colors"] is an array of Key-Value pairs, where the key is a guid/key and the value is the hex color.
        try {
            const values = deserializeValue(value).map(clientKey => clientKey?.toLowerCase());
            const colors: string[] = deserializeColors(configurationValues[ConfigurationValueKey.Colors]);

            return getSelectedColors(colors, values).join(",");
        }
        catch {
            return "";
        }
    }

    public override getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        const htmlStringBuilder: string[] = [];
        const selectorsToProcess: string[] = [];
        const colors = deserializeColors(configurationValues[ConfigurationValueKey.Colors]);
        const values = deserializeValue(value);

        htmlStringBuilder.push("<div class='color-selector-items'>");

        const selectedColors = getSelectedColors(colors, values);

        for (const color of selectedColors) {
            const itemGuid = guid.newGuid();
            const containerId = `color-selector-item-container-${itemGuid}`;
            const itemId = `color-selector-item-${itemGuid}`;

            htmlStringBuilder.push(`<div id="${containerId}" class="color-selector-item-container">`);
            htmlStringBuilder.push(`<div id="${itemId}" class="color-selector-item readonly" style="background-color: ${color};">`);
            htmlStringBuilder.push("</div>");
            htmlStringBuilder.push("</div>");

            // Keep track of the color elements that may have to be outlined
            // if they are too hard to see.
            selectorsToProcess.push(`#${itemId}`);
        }

        htmlStringBuilder.push("</div>");

        this.processSelectors(selectorsToProcess);

        const html = htmlStringBuilder.join("\n");

        if (isEscaped) {
            return escapeHtml(html);
        }

        return html;
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    private processSelectors(selectorsToProcess: string[]): void {
        const maxAttempts = 5;
        let attemptCount = 0;

        function internalProcessSelectors(): void {
            if (!selectorsToProcess.length || attemptCount >= maxAttempts) {
                return;
            }

            nextTick(() => {
                attemptCount++;
                const failedSelectors: string[] = [];

                while (selectorsToProcess.length) {
                    // Process the element at the front of the array.
                    const selector = selectorsToProcess.shift();

                    if (!selector) {
                        // Skip to the next selector if this one is empty.
                        continue;
                    }

                    const element = document.querySelector(selector);

                    if (!element) {
                        // Add the selector to the end array and skip to the next selector.
                        failedSelectors.push(selector);
                    }
                    else {
                        setCamouflagedClass(element);
                    }
                }

                // Add the failed selectors back to the array.
                selectorsToProcess.push(...failedSelectors);

                if (selectorsToProcess.length) {
                    internalProcessSelectors();
                }
            });
        }

        // Start processing selectors.
        internalProcessSelectors();
    }
}
