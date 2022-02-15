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
import { getFieldType } from "../Fields/index";
import { computed, defineComponent, PropType, provide } from "vue";
import { TextFieldType } from "../Fields/textField";
import { PublicAttributeValue, PublicEditableAttributeValue } from "../ViewModels";

const textField = new TextFieldType();

function instanceOfEditable(value: PublicAttributeValue): value is PublicEditableAttributeValue {
    return (<PublicEditableAttributeValue>value).key !== undefined;
}

export default defineComponent({
    name: "RockField",
    props: {
        attributeValue: {
            type: Object as PropType<PublicAttributeValue | PublicEditableAttributeValue>,
            required: true
        },
        showEmptyValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        isEditMode: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    setup(props, { emit }) {
        const field = computed(() => {
            const fieldType = getFieldType(props.attributeValue.fieldTypeGuid);

            return fieldType ?? textField;
        });

        /** True if the read-only value should be displayed. */
        const showValue = computed(() => props.showEmptyValue || field.value.getTextValue(props.attributeValue) !== "");

        /** True if this field is required and must be filled in. */
        const isRequired = computed(() => instanceOfEditable(props.attributeValue) && props.attributeValue.isRequired);

        /** Indicates to the editor component if this field is required or not. */
        const rules = computed(() => isRequired.value ? "required" : "");

        /** True if we are currently in edit mode. */
        const isEditMode = computed(() => props.isEditMode && instanceOfEditable(props.attributeValue));

        /** The label to display above the value or editor. */
        const label = computed(() => props.attributeValue.name);

        /** The help text to display in the help bubble when in edit mode. */
        const helpText = computed(() => instanceOfEditable(props.attributeValue) ? props.attributeValue.description : "");

        /** The read-only component to use to display the value. */
        const valueComponent = computed(() => {
            // This is here to cause the component to update reactively if
            // the value changes.
            const _ignored = props.attributeValue.value;

            // TODO: Remove me, this is a temporary hack for DateField needing
            // to make an async call.
            const _ignored2 = props.attributeValue.textValue;

            return field.value.getFormattedComponent(props.attributeValue);
        });

        /** The edit component to use to display the value. */
        const editComponent = computed(() => field.value.getEditComponent());

        /** The value to display or edit. */
        const value = computed({
            get: () => props.attributeValue.value || "",
            set(newValue) {
                props.attributeValue.value = newValue;

                if (instanceOfEditable(props.attributeValue)) {
                    props.attributeValue.textValue = field.value.getTextValueFromConfiguration(props.attributeValue.value, props.attributeValue.configurationValues ?? {});
                }

                emit("update:attributeValue", props.attributeValue);
            }
        });

        /** The configuration values for the editor component. */
        const configurationValues = computed(() => {
            if (instanceOfEditable(props.attributeValue)) {
                return props.attributeValue.configurationValues;
            }
            else {
                return {};
            }
        });

        provide("isRequired", isRequired);

        return {
            label,
            showValue,
            valueComponent,
            rules,
            isEditMode,
            editComponent,
            value,
            helpText,
            configurationValues
        };
    },
    template: `
<template v-if="!isEditMode">
    <div v-if="showValue" class="form-group static-control">
        <label class="control-label">
            {{ label }}
        </label>
        <div class="control-wrapper">
            <div class="form-control-static">
                <component :is="valueComponent" />
            </div>
        </div>
    </div>
</template>
<template v-else>
    <component :is="editComponent"
        v-model="value"
        :label="label"
        :help="helpText"
        :configurationValues="configurationValues"
        :rules="rules" />
</template>`
});
