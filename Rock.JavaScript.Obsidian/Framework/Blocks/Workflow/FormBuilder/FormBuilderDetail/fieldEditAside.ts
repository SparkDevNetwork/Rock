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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import Panel from "../../../../Controls/panel";
import FieldTypeEditor from "../../../../Controls/fieldTypeEditor";
import InlineSwitch from "../../../../Elements/switch";
import NumberBox from "../../../../Elements/numberBox";
import TextBox from "../../../../Elements/textBox";
import Slider from "../../../../Elements/slider";
import RockForm from "../../../../Controls/rockForm";
import { List } from "../../../../Util/linq";
import { FormField, FormFieldType } from "./types";
import { FieldTypeConfigurationViewModel } from "../../../../ViewModels/Controls/fieldTypeEditor";
import { useFormSources } from "./utils";
import { areEqual } from "../../../../Util/guid";
import { confirmDelete } from "../../../../Util/dialogs";

/**
 * Check if the two records are equal. This makes sure all the key names match
 * and the associated values also match. Strict checking is performed.
 * 
 * @param a The first record value to be compared.
 * @param b The second record value to be compared.
 *
 * @returns True if the two record values are equal, otherwise false.
 */
function shallowStrictEqual(a: Record<string, string>, b: Record<string, string>): boolean {
    const aKeys = Object.keys(a);
    const bKeys = Object.keys(b);

    // Check we have the same number of keys.
    if (aKeys.length !== bKeys.length) {
        return false;
    }

    for (const key of aKeys) {
        // Check that all keys from 'a' exist in 'b'.
        if (!bKeys.includes(key)) {
            return false;
        }

        // Check that all the values from 'a' match those in 'b'.
        if (a[key] !== b[key]) {
            return false;
        }
    }

    return true;
}

export default defineComponent({
    name: "Workflow.FormBuilderDetail.FieldEditAside",
    components: {
        Panel,
        FieldTypeEditor,
        InlineSwitch,
        NumberBox,
        RockForm,
        Slider,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<FormField>,
            required: true
        }
    },

    emits: [
        "update:modelValue",
        "close"
    ],

    methods: {
        /**
         * Checks if this aside is safe to close or if there are errors that
         * must be corrected first.
         */
        isSafeToClose(): boolean {
            this.formSubmit = true;

            return Object.keys(this.validationErrors).length === 0;
        }
    },

    setup(props, { emit }) {
        /** The value used by the FieldTypeEditor for editing the field configuration. */
        const fieldTypeValue = ref<FieldTypeConfigurationViewModel>({
            fieldTypeGuid: props.modelValue.fieldTypeGuid,
            configurationOptions: props.modelValue.configurationValues ?? {},
            defaultValue: props.modelValue.defaultValue ?? ""
        });

        const fieldTypes = useFormSources().fieldTypes ?? [];

        /**
         * The key which forces the field type editor to reload itself whenever the
         * attribute we are editing changes.
         */
        const fieldTypeEditorKey = computed((): string => `fieldTypeEditor_${props.modelValue.guid}`);

        /** The FormFieldType of the attribute we are editing. */
        const fieldType = computed((): FormFieldType | null => {
            return new List(fieldTypes).firstOrUndefined(f => areEqual(f.guid, props.modelValue.fieldTypeGuid)) ?? null;
        });

        /** The icon to display in the title area. */
        const asideIconClass = computed((): string => fieldType.value?.icon ?? "");

        const fieldName = ref(props.modelValue.name);
        const fieldDescription = ref(props.modelValue.description ?? "");
        const fieldKey = ref(props.modelValue.key);
        const fieldSize = ref(props.modelValue.size);
        const isFieldRequired = ref(props.modelValue.isRequired ?? false);
        const isFieldLabelHidden = ref(props.modelValue.isHideLabel ?? false);
        const isShowOnGrid = ref(props.modelValue.isShowOnGrid ?? false);

        /** The validation errors for the form. */
        const validationErrors = ref<Record<string, string>>({});

        /** True if the form should start to submit. */
        const formSubmit = ref(false);

        /**
         * Event handler for when the back button is clicked.
         */
        const onBackClick = (): void => emit("close");

        /**
         * Event handler for when the field type editor has updated any configuration
         * values.
         * 
         * @param value The value that contains the changed information.
         */
        const onFieldTypeModelValueUpdate = (value: FieldTypeConfigurationViewModel): void => {
            emit("update:modelValue", {
                ...props.modelValue,
                configurationValues: value.configurationOptions,
                defaultValue: value.defaultValue
            });
        };

        /**
         * Event handler for when the validation state of the form has changed.
         * 
         * @param errors Any errors that were detected on the form.
         */
        const onValidationChanged = (errors: Record<string, string>): void => {
            validationErrors.value = errors;
        };

        // Watch for any changes in our simple field values and update the
        // modelValue.
        watch([fieldName, fieldDescription, fieldKey, fieldSize, isFieldRequired, isFieldLabelHidden, isShowOnGrid], () => {
            const newValue: FormField = {
                ...props.modelValue,
                name: fieldName.value,
                description: fieldDescription.value,
                key: fieldKey.value,
                size: fieldSize.value,
                isRequired: isFieldRequired.value,
                isHideLabel: isFieldLabelHidden.value,
                isShowOnGrid: isShowOnGrid.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any incoming changes from the parent component and update
        // all our individual field values.
        watch(() => props.modelValue, () => {
            fieldName.value = props.modelValue.name;
            fieldDescription.value = props.modelValue.description ?? "";
            fieldKey.value = props.modelValue.key;
            fieldSize.value = props.modelValue.size;
            isFieldRequired.value = props.modelValue.isRequired ?? false;
            isFieldLabelHidden.value = props.modelValue.isHideLabel ?? false;
            isShowOnGrid.value = props.modelValue.isShowOnGrid ?? false;

            const isConfigChanged = fieldTypeValue.value.fieldTypeGuid !== props.modelValue.fieldTypeGuid
                || !shallowStrictEqual(fieldTypeValue.value.configurationOptions, props.modelValue.configurationValues ?? {})
                || fieldTypeValue.value.defaultValue !== props.modelValue.defaultValue;

            // Only update the field type if anything actually changed.
            if (isConfigChanged) {
                fieldTypeValue.value = {
                    fieldTypeGuid: props.modelValue.fieldTypeGuid,
                    configurationOptions: props.modelValue.configurationValues ?? {},
                    defaultValue: props.modelValue.defaultValue ?? ""
                };
            }
        });

        return {
            asideIconClass,
            fieldDescription,
            fieldKey,
            fieldName,
            fieldSize,
            fieldTypeEditorKey,
            fieldTypeValue,
            formSubmit,
            isFieldLabelHidden,
            isFieldRequired,
            isShowOnGrid,
            onBackClick,
            onFieldTypeModelValueUpdate,
            onValidationChanged,
            validationErrors
        };
    },

    template: `
<div class="d-flex flex-column" style="overflow-y: hidden; flex-grow: 1;">
    <div class="d-flex">
        <div class="d-flex clickable" style="background-color: #484848; color: #fff; align-items: center; justify-content: center; width: 40px;" @click="onBackClick">
            <i class="fa fa-chevron-left"></i>
        </div>

        <div class="p-2 aside-header" style="flex-grow: 1;">
            <i v-if="asideIconClass" :class="asideIconClass"></i>
            <span class="title">{{ fieldName }}</span>
        </div>
    </div>

    <div class="aside-body d-flex flex-column" style="flex-grow: 1; overflow-y: auto;">
        <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged" class="d-flex flex-column" style="flex-grow: 1;">
            <Panel :modelValue="true" title="Field Type" :hasCollapse="true">
                <TextBox v-model="fieldName"
                    rules="required"
                    label="Name" />
                <TextBox v-model="fieldDescription"
                    label="Description"
                    textMode="multiline" />
                <FieldTypeEditor :modelValue="fieldTypeValue" :key="fieldTypeEditorKey" @update:modelValue="onFieldTypeModelValueUpdate" isFieldTypeReadOnly />
            </Panel>

            <Panel title="Conditionals" :hasCollapse="true">
                TODO: Need to build this.
            </Panel>

            <Panel title="Format" :hasCollapse="true">
                <Slider v-model="fieldSize" label="Column Span" :min="1" :max="12" isIntegerOnly showValueBar/>
                <InlineSwitch v-model="isFieldRequired" text="Required" />
                <InlineSwitch v-model="isFieldLabelHidden" text="Hide Label" />
            </Panel>

            <Panel title="Advanced" :hasCollapse="true">
                <TextBox v-model="fieldKey"
                    rules="required"
                    label="Field Key" />
                <InlineSwitch v-model="isShowOnGrid" text="Show on Results Grid" />
            </Panel>
        </RockForm>
    </div>
</div>
`
});
