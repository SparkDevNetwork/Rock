System.register(["vue", "./utils", "../Elements/textBox", "../Services/boolean", "../Services/number"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, textBox_1, boolean_1, number_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "MemoField.Edit",
                components: {
                    TextBox: textBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: ""
                    };
                },
                computed: {
                    configAttributes() {
                        const attributes = {};
                        const maxCharsConfig = this.configurationValues["maxcharacters"];
                        const maxCharsValue = number_1.toNumber(maxCharsConfig);
                        if (maxCharsValue) {
                            attributes.maxLength = maxCharsValue;
                        }
                        const showCountDownConfig = this.configurationValues["showcountdown"];
                        const showCountDownValue = boolean_1.asBooleanOrNull(showCountDownConfig) || false;
                        if (showCountDownValue) {
                            attributes.showCountDown = showCountDownValue;
                        }
                        const rowsConfig = this.configurationValues["numberofrows"];
                        const rows = number_1.toNumber(rowsConfig || null) || 3;
                        if (rows > 0) {
                            attributes.rows = rows;
                        }
                        return attributes;
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
<TextBox v-model="internalValue" v-bind="configAttributes" textMode="MultiLine" />
`
            }));
        }
    };
});
//# sourceMappingURL=memoFieldComponents.js.map