System.register(["vue", "../Util/guid", "vee-validate", "./rockLabel"], function (exports_1, context_1) {
    "use strict";
    var vue_1, guid_1, vee_validate_1, rockLabel_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (vee_validate_1_1) {
                vee_validate_1 = vee_validate_1_1;
            },
            function (rockLabel_1_1) {
                rockLabel_1 = rockLabel_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "RockFormField",
                components: {
                    Field: vee_validate_1.Field,
                    RockLabel: rockLabel_1.default
                },
                setup() {
                    const formState = vue_1.inject("formState", null);
                    return {
                        formState
                    };
                },
                props: {
                    modelValue: {
                        required: true
                    },
                    name: {
                        type: String,
                        required: true
                    },
                    label: {
                        type: String,
                        default: ""
                    },
                    help: {
                        type: String,
                        default: ""
                    },
                    rules: {
                        type: String,
                        default: ""
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    },
                    formGroupClasses: {
                        type: String,
                        default: ""
                    },
                    inputGroupClasses: {
                        type: String,
                        default: ""
                    },
                    validationTitle: {
                        type: String,
                        default: ""
                    },
                    "class": {
                        type: String,
                        default: ""
                    },
                    tabIndex: {
                        type: String,
                        default: ""
                    }
                },
                emits: [
                    "update:modelValue"
                ],
                data: function () {
                    return {
                        uniqueId: `rock-${this.name}-${guid_1.newGuid()}`,
                        internalValue: this.modelValue
                    };
                },
                computed: {
                    isRequired() {
                        return this.rules.includes("required");
                    },
                    classAttr() {
                        return this.class;
                    },
                    errorClasses() {
                        return (formState, errors) => {
                            if (!formState || formState.submitCount < 1) {
                                return "";
                            }
                            return Object.keys(errors).length ? "has-error" : "";
                        };
                    },
                    fieldLabel() {
                        return this.validationTitle || this.label;
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue() {
                        this.internalValue = this.modelValue;
                    }
                },
                template: `
<Field v-model="internalValue" :name="fieldLabel" :rules="rules" #default="{field, errors}">
    <slot name="pre" />
    <div class="form-group" :class="[classAttr, formGroupClasses, isRequired ? 'required' : '', errorClasses(formState, errors)]">
        <RockLabel v-if="label || help" :for="uniqueId" :help="help">
            {{label}}
        </RockLabel>
        <slot v-bind="{uniqueId, field, errors, disabled, inputGroupClasses, tabIndex, fieldLabel}" />
    </div>
    <slot name="post" />
</Field>`
            }));
        }
    };
});
//# sourceMappingURL=rockFormField.js.map