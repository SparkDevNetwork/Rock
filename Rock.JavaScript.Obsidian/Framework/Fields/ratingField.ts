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
import { ComparisonType, numericComparisonTypes } from "../Reporting/comparisonType";
import { toNumberOrNull } from "../Services/number";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    MaxRating = "max"
}

export type RatingValue = {
    value?: number;

    maxValue?: number;
};

// The edit component can be quite large, so load it only as needed.
const editComponent = defineAsyncComponent(async () => {
    return (await import("./ratingFieldComponents")).EditComponent;
});

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./ratingFieldComponents")).ConfigurationComponent;
});

/**
 * The field type handler for the Rating field.
 */
export class RatingFieldType extends FieldTypeBase {
    public override getHtmlValue(value: string, configurationValues: Record<string, string>): string {
        let ratingValue: RatingValue | null;

        try {
            ratingValue = JSON.parse(value ?? "") as RatingValue;
        }
        catch {
            ratingValue = null;
        }

        const rating = ratingValue?.value ?? 0;
        const maxRating = toNumberOrNull(configurationValues[ConfigurationValueKey.MaxRating]) ?? 5;
        let html = "";

        for (let i = 0; i < rating && i < maxRating; i++) {
            html += `<i class="fa fa-rating-selected"></i>`;
        }

        for (let i = rating; i < maxRating; i++) {
            html += `<i class="fa fa-rating-unselected"></i>`;
        }

        return html;
    }

    public override getEditComponent(): Component {
        return editComponent;
    }

    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return numericComparisonTypes;
    }
}
