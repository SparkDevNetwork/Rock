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

import { Component, computed, defineComponent, PropType } from "vue";
import { getComparisonName } from "@Obsidian/Core/Reporting/comparisonType";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { escapeHtml, truncate } from "@Obsidian/Utility/stringUtils";
import { EditComponent as TextEditComponent } from "./textFieldComponents";
import { getFieldEditorProps, getStandardFilterComponent } from "./utils";
import { IFieldType } from "@Obsidian/Types/fieldType";
import { ComparisonType } from "@Obsidian/Types/Reporting/comparisonType";

/**
 * Define a simple component that can be used when editing the configuration of
 * a field type that isn't yet supported.
 */
const unsupportedFieldTypeConfigurationComponent = defineComponent({
    props: {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },

        configurationProperties: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },

    setup() {
        return {};
    },

    template: `
<div class="alert alert-warning">
    Configuration of this field type is not supported.
</div>
`
});

/**
 * Basic field type implementation that is suitable for implementations to
 * extend.
 */
export abstract class FieldTypeBase implements IFieldType {
    public getTextValue(value: string, _configurationValues: Record<string, string>): string {
        return value ?? "";
    }

    public getHtmlValue(value: string, configurationValues: Record<string, string>): string {
        return `${escapeHtml(this.getTextValue(value, configurationValues))}`;
    }

    public getCondensedTextValue(value: string, configurationValues: Record<string, string>): string {
        return truncate(this.getTextValue(value, configurationValues), 100);
    }

    public getCondensedHtmlValue(value: string, configurationValues: Record<string, string>): string {
        return `${escapeHtml(this.getCondensedTextValue(value, configurationValues))}`;
    }

    public getFormattedComponent(): Component {
        return defineComponent({
            name: "FieldType.Formatted",
            props: getFieldEditorProps(),
            setup: (props) => {
                return {
                    content: computed(() => this.getHtmlValue(props.modelValue ?? "", props.configurationValues))
                };
            },

            template: `<div v-html="content"></div>`
        });
    }

    public getCondensedFormattedComponent(): Component {
        return defineComponent({
            name: "FieldType.CondensedFormatted",
            props: getFieldEditorProps(),
            setup: (props) => {
                return {
                    content: computed(() => this.getCondensedHtmlValue(props.modelValue ?? "", props.configurationValues))
                };
            },

            template: `<span v-html="content"></span>`
        });
    }

    public getEditComponent(): Component {
        return TextEditComponent;
    }

    public getConfigurationComponent(): Component {
        return unsupportedFieldTypeConfigurationComponent;
    }

    public hasDefaultComponent(): boolean {
        return true;
    }

    public isFilterable(): boolean {
        return true;
    }

    public getSupportedComparisonTypes(): ComparisonType {
        return ComparisonType.EqualTo | ComparisonType.NotEqualTo;
    }

    public getFilterComponent(): Component | null {
        return getStandardFilterComponent(this.getSupportedComparisonTypes(), this.getEditComponent());
    }

    public getFilterValueDescription(value: ComparisonValue, configurationValues: Record<string, string>): string {
        const valueText = this.getFilterValueText(value, configurationValues);

        if (!value.comparisonType) {
            return valueText ? `Is ${valueText}` : "";
        }

        if (value.comparisonType === ComparisonType.IsBlank || value.comparisonType === ComparisonType.IsNotBlank) {
            return getComparisonName(value.comparisonType);
        }

        if (valueText === "") {
            // If the field type supports IsBlank and we have a blank value and
            // the selected comparison type is EqualTo or NotEqualTo then perform
            // special wrapping around the blank value.
            if (this.getSupportedComparisonTypes() & ComparisonType.IsBlank && (value.comparisonType === ComparisonType.EqualTo || value.comparisonType === ComparisonType.NotEqualTo)) {
                return `${getComparisonName(value.comparisonType)} ''`;
            }

            // No value specified basically means no filter.
            return "";
        }

        return `${getComparisonName(value.comparisonType)} ${valueText}`;
    }

    public getFilterValueText(value: ComparisonValue, configurationValues: Record<string, string>): string {
        const text = this.getTextValue(value.value, configurationValues ?? {}) ?? "";

        return text ? `'${text}'` : text;
    }
}
