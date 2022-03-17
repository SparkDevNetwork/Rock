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
import { Component, defineAsyncComponent } from "vue";
import { ComparisonType, stringComparisonTypes } from "../Reporting/comparisonType";
import { PublicAttributeValue } from "../ViewModels";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    ShouldRequireTrailingForwardSlash = "ShouldRequireTrailingForwardSlash",
    ShouldAlwaysShowCondensed = "ShouldAlwaysShowCondensed"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./urlLinkFieldComponents")).EditComponent;
});

/**
 * The field type handler for the Email field.
 */
export class UrlLinkFieldType extends FieldTypeBase {
    public override getHtmlValue(value: PublicAttributeValue): string {
        const textValue = this.getTextValue(value);

        return textValue ? `<a href="${textValue}">${textValue}</a>` : "";
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return stringComparisonTypes;
    }
}
