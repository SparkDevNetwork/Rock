System.register(["vue", "./utils", "../Services/boolean", "../Elements/dropDownList", "../Elements/toggle", "../Elements/checkBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, boolean_1, dropDownList_1, toggle_1, checkBox_1, BooleanControlType, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (toggle_1_1) {
                toggle_1 = toggle_1_1;
            },
            function (checkBox_1_1) {
                checkBox_1 = checkBox_1_1;
            }
        ],
        execute: function () {
            (function (BooleanControlType) {
                BooleanControlType[BooleanControlType["DropDown"] = 0] = "DropDown";
                BooleanControlType[BooleanControlType["Checkbox"] = 1] = "Checkbox";
                BooleanControlType[BooleanControlType["Toggle"] = 2] = "Toggle";
            })(BooleanControlType || (BooleanControlType = {}));
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "BooleanField.Edit",
                components: {
                    DropDownList: dropDownList_1.default,
                    Toggle: toggle_1.default,
                    CheckBox: checkBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalBooleanValue: false,
                        internalValue: ""
                    };
                },
                computed: {
                    booleanControlType() {
                        const controlType = this.configurationValues["BooleanControlType"];
                        switch (controlType) {
                            case "1":
                                return BooleanControlType.Checkbox;
                            case "2":
                                return BooleanControlType.Toggle;
                            default:
                                return BooleanControlType.DropDown;
                        }
                    },
                    trueText() {
                        let trueText = "Yes";
                        const trueConfig = this.configurationValues["truetext"];
                        if (trueConfig) {
                            trueText = trueConfig;
                        }
                        return trueText || "Yes";
                    },
                    falseText() {
                        let falseText = "No";
                        const falseConfig = this.configurationValues["falsetext"];
                        if (falseConfig) {
                            falseText = falseConfig;
                        }
                        return falseText || "No";
                    },
                    isToggle() {
                        return this.booleanControlType === BooleanControlType.Toggle;
                    },
                    isCheckBox() {
                        return this.booleanControlType === BooleanControlType.Checkbox;
                    },
                    toggleOptions() {
                        return {
                            trueText: this.trueText,
                            falseText: this.falseText
                        };
                    },
                    dropDownListOptions() {
                        const trueVal = boolean_1.asTrueFalseOrNull(true);
                        const falseVal = boolean_1.asTrueFalseOrNull(false);
                        return [
                            { text: this.falseText, value: falseVal },
                            { text: this.trueText, value: trueVal }
                        ];
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    internalBooleanValue() {
                        const valueToEmit = boolean_1.asTrueFalseOrNull(this.internalBooleanValue) || "";
                        this.$emit("update:modelValue", valueToEmit);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = boolean_1.asTrueFalseOrNull(this.modelValue) || "";
                            this.internalBooleanValue = boolean_1.asBoolean(this.modelValue);
                        }
                    }
                },
                template: `
<Toggle v-if="isToggle" v-model="internalBooleanValue" v-bind="toggleOptions" />
<CheckBox v-else-if="isCheckBox" v-model="internalBooleanValue" :inline="false" />
<DropDownList v-else v-model="internalValue" :options="dropDownListOptions" />
`
            }));
        }
    };
});
//# sourceMappingURL=booleanFieldComponents.js.map