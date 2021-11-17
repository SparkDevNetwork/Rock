System.register(["../Fields/index", "vue", "../Fields/textField"], function (exports_1, context_1) {
    "use strict";
    var index_1, vue_1, textField_1, textField;
    var __moduleName = context_1 && context_1.id;
    function instanceOfEditable(value) {
        return value.key !== undefined;
    }
    return {
        setters: [
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (textField_1_1) {
                textField_1 = textField_1_1;
            }
        ],
        execute: function () {
            textField = new textField_1.TextFieldType();
            exports_1("default", vue_1.defineComponent({
                name: "RockField",
                props: {
                    attributeValue: {
                        type: Object,
                        required: true
                    },
                    showEmptyValue: {
                        type: Boolean,
                        default: false
                    },
                    isEditMode: {
                        type: Boolean,
                        default: false
                    }
                },
                setup(props) {
                    const field = vue_1.computed(() => {
                        const fieldType = index_1.getFieldType(props.attributeValue.fieldTypeGuid);
                        return fieldType !== null && fieldType !== void 0 ? fieldType : textField;
                    });
                    const showValue = vue_1.computed(() => props.showEmptyValue || field.value.getTextValue(props.attributeValue) !== "");
                    const isRequired = vue_1.computed(() => instanceOfEditable(props.attributeValue) && props.attributeValue.isRequired);
                    const rules = vue_1.computed(() => isRequired.value ? "required" : "");
                    const isEditMode = vue_1.computed(() => props.isEditMode && instanceOfEditable(props.attributeValue));
                    const label = vue_1.computed(() => props.attributeValue.name);
                    const helpText = vue_1.computed(() => instanceOfEditable(props.attributeValue) ? props.attributeValue.description : "");
                    const valueComponent = vue_1.computed(() => {
                        const _ignored = props.attributeValue.value;
                        const _ignored2 = props.attributeValue.textValue;
                        return field.value.getFormattedComponent(props.attributeValue);
                    });
                    const editComponent = vue_1.computed(() => field.value.getEditComponent(props.attributeValue));
                    const value = vue_1.computed({
                        get: () => props.attributeValue.value || "",
                        set(newValue) {
                            props.attributeValue.value = newValue;
                            if (instanceOfEditable(props.attributeValue)) {
                                field.value.updateTextValue(props.attributeValue);
                            }
                        }
                    });
                    const configurationValues = vue_1.computed(() => {
                        if (instanceOfEditable(props.attributeValue)) {
                            return props.attributeValue.configurationValues;
                        }
                        else {
                            return {};
                        }
                    });
                    vue_1.provide("isRequired", isRequired);
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
            }));
        }
    };
});
//# sourceMappingURL=rockField.js.map