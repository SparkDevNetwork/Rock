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
import { ComparisonType, stringComparisonTypes } from "../Reporting/comparisonType";
import { FieldTypeBase } from "./fieldType";

export const enum ConfigurationValueKey {
    /** Contains "True" if the text field is designed for password entry. */
    IsPassword = "ispassword",

    /** The maximum number of characters allowed in the text entry field. */
    MaxCharacters = "maxcharacters",

    /** Contains "True" if the text field should show the character countdown. */
    ShowCountdown = "showcountdown"
}

// The configuration component can be quite large, so load it only as needed.
const configurationComponent = defineAsyncComponent(async () => {
    return (await import("./textFieldComponents")).ConfigurationComponent;
});

export class TextFieldType extends FieldTypeBase {
    public override getConfigurationComponent(): Component {
        return configurationComponent;
    }

    public override getSupportedComparisonTypes(): ComparisonType {
        return stringComparisonTypes;
    }
}
