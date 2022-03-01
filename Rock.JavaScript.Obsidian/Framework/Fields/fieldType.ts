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

import { escapeHtml, truncate } from "../Services/string";
import { compile, Component, defineComponent, PropType } from "vue";
import { ClientAttributeValue, ClientEditableAttributeValue } from "../ViewModels";
import { EditComponent as TextEditComponent } from "./textFieldComponents";

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
    getTextValue(value: ClientAttributeValue): string;

    /**
     * Get the HTML representation of the attribute value. This will be used
     * as literal HTML so if you are returning plain text it must be encoded
     * for HTML entities first.
     * 
     * @param value The attribute value.
     * 
     * @returns A string that contains a user-friendly HTML representation of the value.
     */
    getHtmlValue(value: ClientAttributeValue): string;

    /**
     * Updates the textValue property to reflect the value property. If no conversion
     * is possible then implementations should make this an empty function.
     * 
     * @param value The attribute value to be updated.
     */
    updateTextValue(value: ClientEditableAttributeValue): void;

    /**
     * Get the condensed plain text representation of the attribute value.
     * The text should probably be no more than 100 characters, though you
     * can also alter the format entirely.
     * 
     * @param value The attribute value.
     * 
     * @returns A string that contains a condensed version of {@link FieldType.getTextValue getTextValue()}.
     */
    getCondensedTextValue(value: ClientAttributeValue): string;

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
    getCondensedHtmlValue(value: ClientAttributeValue): string;

    /**
     * Get the component that will be used to display the formatted value.
     * 
     * @param value The attribute value.
     * 
     * @returns A component that is already configured to show the value.
     */
    getFormattedComponent(value: ClientAttributeValue): Component;

    /**
     * Get the component that will be used to display the condensed formatted value.
     * 
     * @param value The attribute value.
     * 
     * @returns A component that is already configured to show the condensed value.
     */
    getCondensedFormattedComponent(value: ClientAttributeValue): Component;

    /**
     * Get the component that will be used to edit the value. It will receive
     * the modelValue property which contains the {@link ClientAttributeValueViewModel.value}.
     * 
     * @param value The attribute value.
     * 
     * @returns A component that is already configured to edit the value.
     */
    getEditComponent(value: ClientAttributeValue): Component;

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
    public getTextValue(value: ClientAttributeValue): string {
        return value.textValue ?? "";
    }

    public getHtmlValue(value: ClientAttributeValue): string {
        // The HTML parser in use treats any string that begins with a # as a
        // DOM reference, so we need to wrap our content inside a span tag.
        return `<span>${escapeHtml(this.getTextValue(value))}</span>`;
    }

    public updateTextValue(value: ClientEditableAttributeValue): void {
        value.textValue = value.value;
    }

    public getCondensedTextValue(value: ClientAttributeValue): string {
        return truncate(value.textValue ?? "", 100);
    }

    public getCondensedHtmlValue(value: ClientAttributeValue): string {
        return this.getHtmlValue(value);
    }

    public getFormattedComponent(value: ClientAttributeValue): Component {
        return defineComponent(() => {
            return compile(this.getHtmlValue(value));
        });
    }

    public getCondensedFormattedComponent(value: ClientAttributeValue): Component {
        return defineComponent(() => {
            return compile(this.getCondensedHtmlValue(value));
        });
    }

    public getEditComponent(_value: ClientAttributeValue): Component {
        return TextEditComponent;
    }

    public getConfigurationComponent(): Component {
        return unsupportedFieldTypeConfigurationComponent;
    }

    public hasDefaultComponent(): boolean {
        return true;
    }
}
