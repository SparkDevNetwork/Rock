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
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { computed, defineComponent, PropType, provide } from "vue";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { FieldType } from "@Obsidian/SystemGuids/fieldType";

const textField = getFieldType(FieldType.Text);

export default defineComponent({
    name: "RockField",
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: false
        },
        attribute: {
            type: Object as PropType<PublicAttributeBag>,
            required: true
        },
        showEmptyValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        isEditMode: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        showLabel: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        isCondensed: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    setup(props, { emit }) {
        const field = computed(() => {
            const fieldType = getFieldType(props.attribute.fieldTypeGuid ?? "");

            return fieldType ?? textField;
        });

        /** True if the read-only value should be displayed. */
        const showValue = computed(() => props.showEmptyValue || field.value?.getTextValue(props.modelValue ?? "", props.attribute.configurationValues ?? {}) !== "");

        /** True if this field is required and must be filled in. */
        const isRequired = computed(() => props.attribute.isRequired);

        /** Indicates to the editor component if this field is required or not. */
        const rules = computed(() => isRequired.value ? "required" : "");

        /** True if we are currently in edit mode. */
        const isEditMode = computed(() => props.isEditMode);

        /** The label to display above the value or editor. */
        const label = computed(() => props.attribute.name);

        /** The help text to display in the help bubble when in edit mode. */
        const helpText = computed(() => props.attribute.description);

        /** The read-only component to use to display the value. */
        const valueComponent = computed(() => {
            // This is here to cause the component to update reactively if
            // the value changes.
            // Commented out on 3/14/2022 by DSH. If no issues show up it can be removed
            // at a later point in time (maybe 6 months later). I do not believe this
            // is needed anymore after the refactoring that was performed.
            //const _ignored = props.attributeValue.value;

            return props.isCondensed
                ? field.value?.getCondensedFormattedComponent()
                : field.value?.getFormattedComponent();
        });

        /** The edit component to use to display the value. */
        const editComponent = computed(() => field.value?.getEditComponent());

        /** The value to display or edit. */
        const value = computed({
            get: () => props.modelValue || "",
            set(newValue) {
                emit("update:modelValue", newValue);
            }
        });

        /** The configuration values for the editor component. */
        const configurationValues = computed(() => {
            return props.attribute.configurationValues ?? {};
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
<div v-if="!isEditMode">
    <template v-if="showLabel">
        <div v-if="showValue" class="form-group static-control">
            <label class="control-label">
                {{ label }}
            </label>
            <div class="control-wrapper">
                <div class="form-control-static">
                    <component :is="valueComponent" :modelValue="value" :configurationValues="configurationValues" />
                </div>
            </div>
        </div>
    </template>
    <component v-else :is="valueComponent" :modelValue="value" :configurationValues="configurationValues" />
</div>
<div v-else>
    <component :is="editComponent"
        v-model="value"
        :label="label"
        :help="helpText"
        :configurationValues="configurationValues"
        :rules="rules" />
</div>`
});
