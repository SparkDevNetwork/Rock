System.register(["vue", "./utils", "../Services/number", "../Elements/datePartsPicker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, number_1, datePartsPicker_1, EditComponent;
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
            function (datePartsPicker_1_1) {
                datePartsPicker_1 = datePartsPicker_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "MonthDayField.Edit",
                components: {
                    DatePartsPicker: datePartsPicker_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: {
                            year: 0,
                            month: 0,
                            day: 0
                        }
                    };
                },
                watch: {
                    internalValue() {
                        const value = this.internalValue.month !== 0 && this.internalValue.day !== 0
                            ? `${this.internalValue.month}/${this.internalValue.day}`
                            : "";
                        this.$emit("update:modelValue", value);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            const components = (this.modelValue || "").split("/");
                            if (components.length == 2) {
                                this.internalValue = {
                                    year: 0,
                                    month: number_1.toNumber(components[0]),
                                    day: number_1.toNumber(components[1])
                                };
                            }
                            else {
                                this.internalValue = {
                                    year: 0,
                                    month: 0,
                                    day: 0
                                };
                            }
                        }
                    }
                },
                template: `
<DatePartsPicker v-model="internalValue" :showYear="false" />
`
            }));
        }
    };
});
//# sourceMappingURL=monthDayFieldComponents.js.map