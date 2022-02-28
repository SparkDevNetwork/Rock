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

import { compile, Component, defineComponent, PropType } from "vue";
import { ComparisonType, getComparisonName } from "../Reporting/comparisonType";
import { ComparisonValue } from "../Reporting/comparisonValue";
import { escapeHtml, truncate } from "../Services/string";
import { PublicAttributeValue } from "../ViewModels";
import { PublicFilterableAttribute } from "../ViewModels/publicFilterableAttribute";
import { EditComponent as TextEditComponent } from "./textFieldComponents";
import { getStandardFilterComponent } from "./utils";

/**
 * Handles the conversion of an attribute value into one that can be displayed
 * in various ways.
 *
 * Note to plugins: Do not implement this interface directly or your implementation
 * may break if we add new required methods.
 */
export interface IFieldType {
    /**
     * Get the plain text representation of the attribute value.
     * 
     * @param value The attribute value.
     * 
     * @returns A string that contains a user-friendly text representation of the value.
     */
    getTextValue(value: PublicAttributeValue): string;

    /**
     * Get the HTML representation of the attribute value. This will be used
     * as literal HTML so if you are returning plain text it must be encoded
     * for HTML entities first.
     * 
     * @param value The attribute value.
     * 
     * @returns A string that contains a user-friendly HTML representation of the value.
     */
    getHtmlValue(value: PublicAttributeValue): string;

    /**
     * Attempts to convert the value into a text value that can be used to
     * represent the attribute value. If no conversion is possible then null
     * should be returned.
     *
     * @param value The raw value from an edit component that needs to be formatted.
     * @param configurationValues The configuration values that will provide the necessary information to format the value.
     */
    getTextValueFromConfiguration(value: string, configurationValues: Record<string, string>): string | null;

    /**
     * Get the condensed plain text representation of the attribute value.
     * The text should probably be no more than 100 characters, though you
     * can also alter the format entirely.
     * 
     * @param value The attribute value.
     * 
     * @returns A string that contains a condensed version of {@link FieldType.getTextValue getTextValue()}.
     */
    getCondensedTextValue(value: PublicAttributeValue): string;

    /**
     * Get the condensed HTML representation of the attribute value. This will
     * be used as literal HTML so if you are returning plain text it must be
     * encoded for HTML entities first. This should be a more concise
     * representation of the {@link FieldType.getHtmlValue getHtmlValue()}.
     * 
     * @param value The attribute value.
     * 
     * @returns A string that contains a condensed version of {@link FieldType.getHtmlValue getHtmlValue()}.
     */
    getCondensedHtmlValue(value: PublicAttributeValue): string;

    /**
     * Get the component that will be used to display the formatted value.
     * 
     * @param value The attribute value.
     * 
     * @returns A component that is already configured to show the value.
     */
    getFormattedComponent(value: PublicAttributeValue): Component;

    /**
     * Get the component that will be used to display the condensed formatted value.
     * 
     * @param value The attribute value.
     * 
     * @returns A component that is already configured to show the condensed value.
     */
    getCondensedFormattedComponent(value: PublicAttributeValue): Component;

    /**
     * Get the component that will be used to edit the value. It will receive
     * the modelValue property which contains the {@link PublicAttributeValueViewModel.value}.
     * 
     * @returns A component that is already configured to edit the value.
     */
    getEditComponent(): Component;

    /**
     * Get the component that will be used to configure the field. It will receive
     * the modelValue property which contains a Record<string, string> object
     * with the configuration values. It will also receive a configurationProperties
     * value of type Record<string, string>.
     * 
     * @returns A component that is already configured to edit the value.
     */
    getConfigurationComponent(): Component;

    /**
     * Determines if this field type supports a default value component when
     * editing the configuration. It is rare for a field type to not support
     * this but there are some edge cases.
     *
     * @returns true if a default value component should be rendered when editing
     * the field configuration; otherwise false.
     */
    hasDefaultComponent(): boolean;

    /**
     * Determines if this field type supports filtering. By returning true it is
     * assumed that a filter component is available.
     *
     * @returns true if this field type supports filtering and has a filter
     * component; otherwise false.
     */
    isFilterable(): boolean;

    /**
     * Get the component that will be used to configure a filter rule for the
     * field.
     *
     * @returns A component that can be used to edit a filter value or null if not supported.
     */
    getFilterComponent(): Component | null;

    /**
     * Get the comparison types that are supported by this field type. All
     * supported types should be OR'd together into a single value.
     *
     * @returns The bit flags that make up the supported comparison types.
     */
    getSupportedComparisonTypes(): ComparisonType;

    /**
     * Get a human friendly description of the configured filter value. Such as
     * "equal to 3". It should not include the name of the attribute.
     * 
     * @param value The comparison value to be formatted.
     * @param attribute The attribute containing the configuration to use during formatting.
     */
    getFilterValueDescription(value: ComparisonValue, attribute: PublicFilterableAttribute): string;

    /**
     * Gets a human friendly string of text that represents the comparison
     * value. This should not include the comparison type.
     * 
     * @param value The comparison value to be formatted.
     * @param attribute The attribute containing the configuration to use during formatting.
     */
    getFilterValueText(value: ComparisonValue, attribute: PublicFilterableAttribute): string;
}

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
    public getTextValue(value: PublicAttributeValue): string {
        return value.textValue ?? "";
    }

    public getHtmlValue(value: PublicAttributeValue): string {
        // The HTML parser in use treats any string that begins with a # as a
        // DOM reference, so we need to wrap our content inside a span tag.
        return `<span>${escapeHtml(this.getTextValue(value))}</span>`;
    }

    public getTextValueFromConfiguration(value: string, _configurationValues: Record<string, string>): string | null {
        return value;
    }

    public getCondensedTextValue(value: PublicAttributeValue): string {
        return truncate(value.textValue ?? "", 100);
    }

    public getCondensedHtmlValue(value: PublicAttributeValue): string {
        return this.getHtmlValue(value);
    }

    public getFormattedComponent(value: PublicAttributeValue): Component {
        return defineComponent(() => {
            return compile(this.getHtmlValue(value));
        });
    }

    public getCondensedFormattedComponent(value: PublicAttributeValue): Component {
        return defineComponent(() => {
            return compile(this.getCondensedHtmlValue(value));
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

    public getFilterValueDescription(value: ComparisonValue, attribute: PublicFilterableAttribute): string {
        const valueText = this.getFilterValueText(value, attribute);

        if (value.comparisonType === null || value.comparisonType === undefined) {
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

    public getFilterValueText(value: ComparisonValue, attribute: PublicFilterableAttribute): string {
        return this.getTextValueFromConfiguration(value.value, attribute.configurationValues ?? {}) ?? "";
    }
}
