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
import { Component, defineAsyncComponent } from "vue";
import { ComparisonType } from "../Reporting/comparisonType";
import { escapeHtml } from "../Services/string";
import { ListItem } from "../ViewModels";
import { FieldTypeBase } from "./fieldType";

/**
 * The key names for the configuration properties available when editing the
 * configuration of a File field type.
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
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./fileFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./fileFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the File field.
 */
export class FileFieldType extends FieldTypeBase {
    public override getTextValue(value: string, configurationValues: Record<string, string>): string {
        try {
            const realValue = JSON.parse(value) as ListItem;

            return realValue.text;
        }
        catch {
            return value;
        }
    }

    public override getHtmlValue(value: string, _configurationValues: Record<string, string>): string {
        try {
            const realValue = JSON.parse(value ?? "") as ListItem;

            return `<a href="/GetFile.ashx?guid=${realValue.value}" title="${escapeHtml(realValue.text)}" class="btn btn-xs btn-default">View</a>`;
        }
        catch {
            return value ?? "";
        }
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
}
