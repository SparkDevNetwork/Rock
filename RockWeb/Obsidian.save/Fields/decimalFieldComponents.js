System.register(["vue", "./utils", "../Services/number", "../Elements/numberBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, number_1, numberBox_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (numberBox_1_1) {
                numberBox_1 = numberBox_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "DecimalField.Edit",
                components: {
                    NumberBox: numberBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: null
                    };
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue !== null ? this.internalValue.toString() : "");
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = number_1.toNumberOrNull(this.modelValue || "");
                        }
                    }
                },
                template: `
<NumberBox v-model="internalValue" rules="decimal" />
`
            }));
        }
    };
});
//# sourceMappingURL=decimalFieldComponents.js.map