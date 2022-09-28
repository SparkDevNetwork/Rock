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

import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";

/**
 * Class that provides some convenience methods to building an array of
 * ValueDetailListItem objects for use in the valueDetailList control.
 */
export class ValueDetailListItemBuilder {
    private values: ValueDetailListItem[] = [];

    /**
     * Add a new field value with a plain text value. The text value will be
     * escaped so it is safe to use HTML characters.
     * 
     * @param title The title of the field to be displayed.
     * @param text The text value to be displayed.
     */
    public addTextValue(title: string, text: string): void {
        this.values.push({
            title: title,
            textValue: text
        });
    }

    /**
     * Add a new field value with a formatted HTML value. The HTML content
     * will not be escaped so you must ensure it is valid HTML.
     * 
     * @param title The title of the field to be displayed.
     * @param html The text value to be displayed.
     */
    public addHtmlValue(title: string, html: string): void {
        this.values.push({
            title: title,
            htmlValue: html
        });
    }

    /**
     * Builds the values to be displayed.
     *
     * @returns An array of ValueDetailListItem objects containing the items to display.
     */
    public build(): ValueDetailListItem[] {
        return [...this.values.map(v => ({ ...v }))];
    }
}
