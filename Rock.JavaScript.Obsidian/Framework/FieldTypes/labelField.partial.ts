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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const enum ConfigurationValueKey {
    FilePath = "filePath",
    EncodedFileName = "encodedFileName",
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./labelFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./labelFieldComponents")).ConfigurationComponent;
});

export class LabelFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        if (value === undefined || value === null || value === "") {
            return "";
        }

        const binaryFile = JSON.parse(value) as ListItemBag;

        return binaryFile.text ?? "";
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

    public override getHtmlValue(value: string, configurationValues: Record<string, string>): string {
        if (!value) {
            return "";
        }

        const binaryFile = JSON.parse(value) as ListItemBag;
        const filePath = configurationValues[ConfigurationValueKey.FilePath];
        const fileName = configurationValues[ConfigurationValueKey.EncodedFileName];

        const html = `
<a
    href='${filePath}?guid=${binaryFile.value}'
    title='${fileName}'
    class='btn btn-xs btn-default'>
    View
</a>`;

        return html;
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>): string {
        return this.getHtmlValue(value, configurationValues);
    }
}