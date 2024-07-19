﻿// <copyright>
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
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { defineAsyncComponent } from "@Obsidian/Utility/component";
import { FieldTypeBase } from "./fieldType";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";

export const enum ConfigurationValueKey {
    AttributeMatrixTemplate = "attributematrixtemplate"
}

export const enum ConfigurationPropertyKey {
    Templates = "templates"
}


// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./matrixFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./matrixFieldComponents")).ConfigurationComponent;
});


/**
 * The field type handler for the Matrix field.
 */
export class MatrixFieldType extends FieldTypeBase {
    public override getTextValue(value: string, _configurationValues: Record<string, string>): string {
        if (value === undefined || value === null || value === "") {
            return "";
        }

        try {
            const val = JSON.parse(value) as ListItemBag;
            return val.value ?? "";
        }
        catch {
            return value;
        }
    }

    public override getHtmlValue(value: string, _configurationValues: Record<string, string>, isEscaped:boolean = false): string {
        if (value === undefined || value === null || value === "") {
            return "";
        }

        try {
            const val = JSON.parse(value) as ListItemBag;

            if (isEscaped) {
                return escapeHtml(val.text ?? "");
            }

            return val.text ?? "";
        }
        catch {
            if (isEscaped) {
                return escapeHtml(value);
            }

            return value;
        }
    }

    public override getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped:boolean = false): string {
        return this.getHtmlValue(value, configurationValues, isEscaped);
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return 0;
    }

    public override isFilterable(): boolean {
        return false;
    }

    public override hasDefaultComponent(): boolean {
        return false;
    }
}
