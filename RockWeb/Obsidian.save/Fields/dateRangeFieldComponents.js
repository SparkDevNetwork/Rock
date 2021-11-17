System.register(["vue", "./utils", "../Elements/dateRangePicker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, dateRangePicker_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (dateRangePicker_1_1) {
                dateRangePicker_1 = dateRangePicker_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "DateRangeField.Edit",
                components: {
                    DateRangePicker: dateRangePicker_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: {}
                    };
                },
                setup() {
                    return {};
                },
                watch: {
                    internalValue() {
                        var _a, _b;
                        if (!this.internalValue.lowerValue && !this.internalValue.upperValue) {
                            this.$emit("update:modelValue", "");
                        }
                        else {
                            this.$emit("update:modelValue", `${(_a = this.internalValue.lowerValue) !== null && _a !== void 0 ? _a : ""},${(_b = this.internalValue.upperValue) !== null && _b !== void 0 ? _b : ""}`);
                        }
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            var _a;
                            const components = ((_a = this.modelValue) !== null && _a !== void 0 ? _a : "").split(",");
                            if (components.length === 2) {
                                this.internalValue = {
                                    lowerValue: components[0],
                                    upperValue: components[1]
                                };
                            }
                            else {
                                this.internalValue = {};
                            }
                        }
                    }
                },
                template: `
<DateRangePicker v-model="internalValue" />
`
            }));
        }
    };
});
//# sourceMappingURL=dateRangeFieldComponents.js.map