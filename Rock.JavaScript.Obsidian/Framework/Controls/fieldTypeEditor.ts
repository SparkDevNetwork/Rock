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

import { Component, computed, defineComponent, PropType, ref, watch } from "vue";
import RockField from "../Controls/rockField";
import Alert from "../Elements/alert";
import DropDownList from "../Elements/dropDownList";
import StaticFormControl from "../Elements/staticFormControl";
import { getFieldType } from "../Fields/index";
import { get, post } from "../Util/http";
import { ClientEditableAttributeValue, ListItem } from "../ViewModels";
import { FieldTypeConfigurationPropertiesViewModel, FieldTypeConfigurationViewModel } from "../ViewModels/Controls/fieldTypeEditor";

export default defineComponent({
    name: "FieldTypeEditor",

    components: {
        Alert,
        DropDownList,
        RockField,
        StaticFormControl
    },

    props: {
        modelValue: {
            type: Object as PropType<FieldTypeConfigurationViewModel | null>,
            default: null
        },

        isFieldTypeReadOnly: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props, { emit }) {
        /** The selected field type in the drop down list. */
        const fieldTypeValue = ref(props.modelValue?.fieldTypeGuid ?? "");

        // Tracks a pending resetToDefaults() call.
        let resetToDefaultsTimer: number | null = null;

        /** The details about the default value used for the field. */
        const defaultValue = ref<ClientEditableAttributeValue | null>(null);

        /** The current configuration properties that describe the field type options. */
        const configurationProperties = ref<Record<string, string>>({});

        /** The current options selected in the configuration properties. */
        const configurationOptions = ref<Record<string, string>>(props.modelValue?.configurationOptions ?? {});

        /** True if the default value component should be shown. */
        const hasDefaultValue = computed((): boolean => {
            // Verify the configuration component is visible and we have a
            // default value.
            if (!showConfigurationComponent.value || defaultValue.value === null) {
                return false;
            }

            const fieldType = getFieldType(fieldTypeValue.value);

            // Field type has final say if a default value should be shown.
            return fieldType?.hasDefaultComponent() ?? false;
        });

        /** True if the field types options are ready for display. */
        const isFieldTypesReady = ref(false);

        /** True if the configuration data has been retrieved and is ready for display. */
        const isConfigurationReady = ref(false);

        /** True if everything is loaded and ready for display. */
        const isReady = computed(() => isFieldTypesReady.value && isConfigurationReady.value);

        /** Contains any error message to be displayed to the user about the operation. */
        const fieldErrorMessage = ref("");

        /** The options to be shown in the field type drop down control. */
        const fieldTypeOptions = ref<ListItem[]>([]);

        /** The UI component that will handle the configuration of the field type. */
        const configurationComponent = computed((): Component | null => {
            const fieldType = getFieldType(fieldTypeValue.value);
            if (fieldType) {
                return fieldType.getConfigurationComponent();
            }
            return null;
        });

        /** True if the configuration component is ready to be displayed. */
        const showConfigurationComponent = computed((): boolean => {
            return configurationComponent.value !== null && isReady.value;
        });

        /** The name of the currently selected field type. */
        const fieldTypeName = computed((): string => {
            const matches = fieldTypeOptions.value.filter(v => v.value === fieldTypeValue.value);

            return matches.length >= 1 ? matches[0].text : "";
        });

        /**
         * True if an update request has been caused by an internal value change.
         * In this case, the update is not emitted to the parent.
         */
        let isInternalUpdate = false;

        /**
         * Called when the modelValue needs to be updated from any change
         * that was made.
         */
        const updateModelValue = (): void => {
            if (isInternalUpdate) {
                return;
            }

            const newValue: FieldTypeConfigurationViewModel = {
                fieldTypeGuid: fieldTypeValue.value,
                configurationOptions: configurationOptions.value,
                defaultValue: defaultValue.value?.value ?? ""
            };

            emit("update:modelValue", newValue);
        };

        /**
         * Resets all configuration details as if the user selected a blank
         * field type.
         */
        const resetToDefaults = (): void => {
            if (resetToDefaultsTimer !== null) {
                clearTimeout(resetToDefaultsTimer);
                resetToDefaultsTimer = null;
            }

            isConfigurationReady.value = false;
            isInternalUpdate = true;
            configurationProperties.value = {};
            configurationOptions.value = {};
            defaultValue.value = null;
            isInternalUpdate = false;

            updateModelValue();
        };

        /** Updates the field configuration from new data on the server. */
        const updateFieldConfiguration = (currentDefaultValue: string): void => {
            if (fieldTypeValue.value === "") {
                resetToDefaults();

                return;
            }

            const update: FieldTypeConfigurationViewModel = {
                fieldTypeGuid: fieldTypeValue.value,
                configurationOptions: configurationOptions.value,
                defaultValue: currentDefaultValue
            };

            post<FieldTypeConfigurationPropertiesViewModel>("/api/v2/Controls/FieldTypeEditor/fieldTypeConfiguration", null, update)
                .then(result => {
                    resetToDefaults();
                    console.debug("got configuration", result.data);

                    if (result.isSuccess && result.data && result.data.configurationProperties && result.data.configurationOptions && result.data.defaultValue) {
                        fieldErrorMessage.value = "";
                        isConfigurationReady.value = true;

                        isInternalUpdate = true;
                        configurationProperties.value = result.data.configurationProperties;
                        configurationOptions.value = result.data.configurationOptions;
                        defaultValue.value = result.data.defaultValue;
                        isInternalUpdate = false;

                        updateModelValue();
                    }
                    else {
                        fieldErrorMessage.value = result.errorMessage ?? "Encountered unknown error communicating with server.";
                    }
                });
        };

        // Called when the field type drop down value is changed.
        watch(fieldTypeValue, () => {
            if (resetToDefaultsTimer === null) {
                resetToDefaultsTimer = window.setTimeout(resetToDefaults, 250);
            }

            updateFieldConfiguration("");
        });

        /** Called when the default value has been changed by the screen control. */
        const onDefaultValueUpdate = (): void => {
            console.debug("default value updated");
            updateModelValue();
        };

        /**
         * Called when the field type configuration control requests that the
         * configuration properties be updated from the server.
         */
        const onUpdateConfiguration = (): void => {
            console.debug("onUpdateConfiguration");
            updateFieldConfiguration(defaultValue.value?.value ?? "");
        };

        /**
         * Called when the field type configuration control has updated one of
         * the configuration values that does not require a full reload.
         * 
         * @param key The key of the configuration value that was changed.
         * @param value The new value of the configuration value.
         */
        const onUpdateConfigurationValue = (key: string, value: string): void => {
            if (defaultValue.value?.configurationValues) {
                console.debug("update configuration value", key, value);
                defaultValue.value.configurationValues[key] = value;
                updateModelValue();
            }
        };

        // Get all the available field types that the user is allowed to edit.
        get<ListItem[]>("/api/v2/Controls/FieldTypeEditor/availableFieldTypes")
            .then(result => {
                if (result.isSuccess && result.data) {
                    fieldTypeOptions.value = result.data;
                    isFieldTypesReady.value = true;

                    // If the field type is already selected then begin to load
                    // all the field configuration.
                    if (fieldTypeValue.value !== "") {
                        updateFieldConfiguration(props.modelValue?.defaultValue ?? "");
                    }
                }
            });

        return {
            configurationComponent,
            configurationOptions,
            configurationProperties,
            defaultValue,
            hasDefaultValue,
            fieldErrorMessage,
            fieldTypeName,
            fieldTypeOptions,
            fieldTypeValue,
            isFieldTypesReady,
            onDefaultValueUpdate,
            onUpdateConfiguration,
            onUpdateConfigurationValue,
            showConfigurationComponent
        };
    },

    template: `
<div>
    <template v-if="isFieldTypesReady">
        <StaticFormControl v-if="isFieldTypeReadOnly" label="Field Type" v-model="fieldTypeName" />
        <DropDownList v-else label="Field Type" v-model="fieldTypeValue" :options="fieldTypeOptions" rules="required" />
    </template>
    <Alert v-if="fieldErrorMessage" alertType="warning">
        {{ fieldErrorMessage }}
    </Alert>
    <component v-if="showConfigurationComponent" :is="configurationComponent" v-model="configurationOptions" :configurationProperties="configurationProperties" @updateConfiguration="onUpdateConfiguration" @updateConfigurationValue="onUpdateConfigurationValue" />
    <RockField v-if="hasDefaultValue" :attributeValue="defaultValue" @update:attributeValue="onDefaultValueUpdate" isEditMode />
</div>
`
});
