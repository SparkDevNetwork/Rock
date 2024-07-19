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
import { FieldTypeBase } from "./fieldType";
import { stringComparisonTypes } from "@Obsidian/Core/Reporting/comparisonType";
import { escapeHtml, truncate } from "@Obsidian/Utility/stringUtils";

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./audioUrlFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./audioUrlFieldComponents")).ConfigurationComponent;
});

export class AudioUrlFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        return value ?? "";
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

    public override getHtmlValue(value: string, _configurationValues: Record<string, string>, isEscaped: boolean = false): string {
        if (!value) {
            return "";
        }

        const html = `
<audio
    src='${value}'
    class='js-media-audio'
    controls>
</audio>

<script>
    Rock.controls.mediaPlayer.initialize();
</script>
        `;

        if (isEscaped) {
            return escapeHtml(html);
        }

        return html;
    }

    public override getCondensedHtmlValue(value: string, _configurationValues: Record<string, string>): string {
        if (!value) {
            return "";
        }

        const condensedValue = `<a href="${this.encodeXml(value)}">${truncate(value, 100)}</a>`;
        return `${escapeHtml(condensedValue)}`;
    }

    // Turns a string into a properly XML Encoded string.
    private encodeXml(str: string): string {
        let encoded = "";
        for (let i = 0; i < str.length; i++) {
            const chr = str.charAt(i);
            switch (chr) {
                case "<":
                    encoded += "&lt;";
                    break;
                case ">":
                    encoded += "&gt;";
                    break;
                case "&":
                    encoded += "&amp;";
                    break;
                case '"':
                    encoded += "&quot;";
                    break;
                case "'":
                    encoded += "&apos;";
                    break;
                case "\n":
                    encoded += "&#xA;";
                    break;
                case "\r":
                    encoded += "&#xD;";
                    break;
                case "\t":
                    encoded += "&#x9;";
                    break;
                default:
                    encoded += chr;
                    break;
            }
        }

        return encoded;
    }
}
