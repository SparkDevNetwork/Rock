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
import { FieldTypeBase } from "./fieldType";
import { getStandardFilterComponent } from "./utils";

export const enum ConfigurationValueKey {
    NumberOfRows = "numberofrows",
    AllowHtml = "allowhtml",
    MaxCharacters = "maxcharacters",
    ShowCountDown = "showcountdown"
}

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./memoFieldComponents")).EditComponent;
});

// The filter component can be quite large, so load it only as needed.
const filterComponent = defineAsyncComponent(async () => {
    return (await import("./memoFieldComponents")).FilterComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./memoFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Memo field.
 */
export class MemoFieldType extends FieldTypeBase {
    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return stringComparisonTypes;
    }

    public override getFilterComponent(): Component {
        return getStandardFilterComponent(this.getSupportedComparisonTypes(), filterComponent);
    }
}
