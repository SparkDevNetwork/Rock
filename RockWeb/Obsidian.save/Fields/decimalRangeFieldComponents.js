System.register(["vue", "./utils", "../Services/number", "../Elements/numberRangeBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, number_1, numberRangeBox_1, EditComponent;
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
            function (numberRangeBox_1_1) {
                numberRangeBox_1 = numberRangeBox_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "IntegerRangeField.Edit",
                components: {
                    NumberRangeBox: numberRangeBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: {}
                    };
                },
                watch: {
                    internalValue() {
                        var _a, _b;
                        const value = `${(_a = this.internalValue.lower) !== null && _a !== void 0 ? _a : ""},${(_b = this.internalValue.upper) !== null && _b !== void 0 ? _b : ""}`;
                        this.$emit("update:modelValue", value !== "," ? value : "");
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            var _a;
                            const values = ((_a = this.modelValue) !== null && _a !== void 0 ? _a : "").split(",");
                            const lower = number_1.toNumberOrNull(values[0]);
                            const upper = values.length >= 2 ? number_1.toNumberOrNull(values[1]) : null;
                            if (lower !== this.internalValue.lower || upper !== this.internalValue.upper) {
                                this.internalValue = {
                                    lower: lower,
                                    upper: upper
                                };
                            }
                        }
                    }
                },
                template: `
<NumberRangeBox v-model="internalValue" />
`
            }));
        }
    };
});
//# sourceMappingURL=decimalRangeFieldComponents.js.map