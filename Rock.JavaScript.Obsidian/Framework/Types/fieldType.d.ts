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

import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { Component } from "vue";
/**
 * Handles the conversion of an attribute value into one that can be displayed
 * in various ways.
 *
 * Note to plugins: Do not implement this interface directly or your implementation
 * may break if we add new required methods.
 */
export interface IFieldType {
    /**
     * Attempts to convert the value into a text value that can be used to
     * represent the attribute value. If no conversion is possible then the
     * original value should be returned.
     *
     * @param value The raw value that needs to be formatted.
     * @param configurationValues The configuration values that will provide the necessary information to format the value.
     *
     * @returns A string that contains a user-friendly text representation of the value.
     */
    getTextValue(value: string, configurationValues: Record<string, string>): string;

    /**
     * Get the HTML representation of the attribute value. This will be used
     * as literal HTML so if you are returning plain text it must be encoded
     * for HTML entities first. If you are returning renderable HTML, make sure
     * to esacpe that HTML if the `isEscaped` parameter is set to `true`.
     *
     * @param value The attribute value.
     * @param configurationValues The configuration values that will provide the necessary information to format the value.
     * @param isEscaped If true, escape the renderable HTML
     *
     * @returns A string that contains a user-friendly HTML representation of the value.
     */
    getHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string;

    /**
     * Get the condensed plain text representation of the attribute value.
     * The text should probably be no more than 100 characters, though you
     * can also alter the format entirely.
     *
     * @param value The attribute value.
     * @param configurationValues The configuration values that will provide the necessary information to format the value.
     *
     * @returns A string that contains a condensed version of {@link FieldType.getTextValue getTextValue()}.
     */
    getCondensedTextValue(value: string, configurationValues: Record<string, string>): string;

    /**
     * Get the condensed HTML representation of the attribute value. This will
     * be used as literal HTML so if you are returning plain text it must be
     * encoded for HTML entities first. If you are returning renderable HTML, make
     * sure to esacpe that HTML if the `isEscaped` parameter is set to `true`.
     *
     * This should be a more concise representation of the {@link FieldType.getHtmlValue getHtmlValue()}.
     *
     * @param value The attribute value.
     * @param configurationValues The configuration values that will provide the necessary information to format the value.
     * @param isEscaped If true, escape the renderable HTML
     *
     * @returns A string that contains a condensed version of {@link FieldType.getHtmlValue getHtmlValue()}.
     */
    getCondensedHtmlValue(value: string, configurationValues: Record<string, string>, isEscaped?: boolean): string;

    /**
     * Get the component that will be used to display the formatted value.
     *
     * @param configurationValues The configuration values that will provide the
     * necessary information to process the value.
     *
     * @returns A component that is already configured to show the value.
     */
    getFormattedComponent(configurationValues: Record<string, string>): Component;

    /**
     * Get the component that will be used to display the condensed formatted value.
     *
     * @param configurationValues The configuration values that will provide the
     * necessary information to process the value.
     *
     * @returns A component that is already configured to show the condensed value.
     */
    getCondensedFormattedComponent(configurationValues: Record<string, string>): Component;

    /**
     * Get the component that will be used to edit the value. It will receive
     * the modelValue property which contains the {@link PublicAttributeValueViewModel.value}.
     *
     * @param configurationValues The configuration values that will provide the
     * necessary information to process the value.
     *
     * @returns A component that is already configured to edit the value.
     */
    getEditComponent(configurationValues: Record<string, string>): Component;

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
     * @param configurationValues The configuration values that will provide the
     * necessary information to process the value.
     *
     * @returns true if a default value component should be rendered when editing
     * the field configuration; otherwise false.
     */
    hasDefaultComponent(configurationValues: Record<string, string>): boolean;

    /**
     * Determines if this field type supports filtering. By returning true it is
     * assumed that a filter component is available.
     *
     * @param configurationValues The configuration values that will provide the
     * necessary information to process the value.
     *
     * @returns true if this field type supports filtering and has a filter
     * component; otherwise false.
     */
    isFilterable(configurationValues: Record<string, string>): boolean;

    /**
     * Get the component that will be used to configure a filter rule for the
     * field.
     *
     * @param configurationValues The configuration values that will provide the
     * necessary information to process the value.
     *
     * @returns A component that can be used to edit a filter value or null if not supported.
     */
    getFilterComponent(configurationValues: Record<string, string>): Component | null;

    /**
     * Get the comparison types that are supported by this field type. All
     * supported types should be OR'd together into a single value.
     *
     * @param configurationValues The configuration values that will provide the
     * necessary information to process the value.
     *
     * @returns The bit flags that make up the supported comparison types.
     */
    getSupportedComparisonTypes(configurationValues: Record<string, string>): ComparisonType;

    /**
     * Get a human friendly description of the configured filter value. Such as
     * "equal to 3". It should not include the name of the attribute.
     *
     * @param value The comparison value to be formatted.
     * @param configurationValues The configuration of the field type to use when formatting the value.
     */
    getFilterValueDescription(value: ComparisonValue, configurationValues: Record<string, string>): string;

    /**
     * Gets a human friendly string of text that represents the comparison
     * value. This should not include the comparison type.
     *
     * @param value The comparison value to be formatted.
     * @param configurationValues The configuration of the field type to use when formatting the value.
     */
    getFilterValueText(value: ComparisonValue, configurationValues: Record<string, string>): string;

    /**
     * Determines if the value matches the filter value for this field type.
     *
     * @param value The value to be compared with this filter value.
     * @param filterValue The comparison value specified by the filter.
     * @param configurationValues The configuration of the field type to use when formatting the value.
     *
     * @returns True if the value matches the comparison value; otherwise false.
     */
    doesValueMatchFilter(value: string, filterValue: ComparisonValue, configurationValues: Record<string, string>): boolean;
}
