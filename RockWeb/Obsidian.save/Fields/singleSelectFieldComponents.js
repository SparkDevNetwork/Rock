System.register(["vue", "./utils", "../Elements/dropDownList", "../Elements/radioButtonList", "../Services/number"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, dropDownList_1, radioButtonList_1, number_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (radioButtonList_1_1) {
                radioButtonList_1 = radioButtonList_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "SingleSelectField.Edit",
                components: {
                    DropDownList: dropDownList_1.default,
                    RadioButtonList: radioButtonList_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup() {
                    return {
                        isRequired: vue_1.inject("isRequired")
                    };
                },
                data() {
                    return {
                        internalValue: ""
                    };
                },
                computed: {
                    options() {
                        var _a;
                        try {
                            const valuesConfig = JSON.parse((_a = this.configurationValues["values"]) !== null && _a !== void 0 ? _a : "[]");
                            const providedOptions = valuesConfig.map(v => {
                                return {
                                    text: v.text,
                                    value: v.value
                                };
                            });
                            if (this.isRadioButtons && !this.isRequired) {
                                providedOptions.unshift({
                                    text: "None",
                                    value: ""
                                });
                            }
                            return providedOptions;
                        }
                        catch (_b) {
                            return [];
                        }
                    },
                    ddlConfigAttributes() {
                        const attributes = {};
                        const fieldTypeConfig = this.configurationValues["fieldtype"];
                        if (fieldTypeConfig === "ddl_enhanced") {
                            attributes.enhanceForLongLists = true;
                        }
                        return attributes;
                    },
                    rbConfigAttributes() {
                        const attributes = {};
                        const repeatColumnsConfig = this.configurationValues["repeatColumns"];
                        if (repeatColumnsConfig) {
                            attributes["repeatColumns"] = number_1.toNumberOrNull(repeatColumnsConfig) || 0;
                        }
                        return attributes;
                    },
                    isRadioButtons() {
                        const fieldTypeConfig = this.configurationValues["fieldtype"];
                        return fieldTypeConfig === "rb";
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = this.modelValue || "";
                        }
                    }
                },
                template: `
<RadioButtonList v-if="isRadioButtons" v-model="internalValue" v-bind="rbConfigAttributes" :options="options" horizontal />
<DropDownList v-else v-model="internalValue" v-bind="ddlConfigAttributes" :options="options" />
`
            }));
        }
    };
});
//# sourceMappingURL=singleSelectFieldComponents.js.map