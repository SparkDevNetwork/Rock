System.register(["vue", "./utils", "../Elements/listBox", "../Elements/checkBoxList", "../Services/number", "../Services/boolean"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, listBox_1, checkBoxList_1, number_1, boolean_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (listBox_1_1) {
                listBox_1 = listBox_1_1;
            },
            function (checkBoxList_1_1) {
                checkBoxList_1 = checkBoxList_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "MultiSelectField.Edit",
                components: {
                    ListBox: listBox_1.default,
                    CheckBoxList: checkBoxList_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup() {
                    return {
                        isRequired: vue_1.inject("isRequired")
                    };
                },
                data() {
                    return {
                        internalValue: []
                    };
                },
                computed: {
                    options() {
                        var _a;
                        try {
                            const valuesConfig = JSON.parse((_a = this.configurationValues["values"]) !== null && _a !== void 0 ? _a : "[]");
                            return valuesConfig.map(v => {
                                return {
                                    text: v.text,
                                    value: v.value
                                };
                            });
                        }
                        catch (_b) {
                            return [];
                        }
                    },
                    listBoxConfigAttributes() {
                        const attributes = {};
                        const enhancedSelection = this.configurationValues["enhancedselection"];
                        if (boolean_1.asBoolean(enhancedSelection)) {
                            attributes.enhanceForLongLists = true;
                        }
                        return attributes;
                    },
                    checkBoxListConfigAttributes() {
                        const attributes = {};
                        const repeatColumnsConfig = this.configurationValues["repeatColumns"];
                        const repeatDirection = this.configurationValues["repeatDirection"];
                        if (repeatColumnsConfig) {
                            attributes["repeatColumns"] = number_1.toNumberOrNull(repeatColumnsConfig) || 0;
                        }
                        if (repeatDirection !== "Vertical") {
                            attributes["horizontal"] = true;
                        }
                        return attributes;
                    },
                    isListBox() {
                        const enhancedSelection = this.configurationValues["enhancedselection"];
                        return boolean_1.asBoolean(enhancedSelection);
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue.join(","));
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            const value = this.modelValue || "";
                            this.internalValue = value !== "" ? value.split(",") : [];
                        }
                    }
                },
                template: `
<ListBox v-if="isListBox" v-model="internalValue" v-bind="listBoxConfigAttributes" :options="options" />
<CheckBoxList v-else v-model="internalValue" v-bind="checkBoxListConfigAttributes" :options="options" />
`
            }));
        }
    };
});
//# sourceMappingURL=multiSelectFieldComponents.js.map