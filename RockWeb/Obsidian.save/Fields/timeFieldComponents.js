System.register(["vue", "./utils", "../Elements/timePicker", "../Services/number", "../Services/string"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, timePicker_1, number_1, string_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (timePicker_1_1) {
                timePicker_1 = timePicker_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "TimeField.Edit",
                components: {
                    TimePicker: timePicker_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalTimeValue: {},
                        internalValue: ""
                    };
                },
                computed: {
                    displayValue() {
                        if (this.internalTimeValue.hour === undefined || this.internalTimeValue.minute === undefined) {
                            return "";
                        }
                        let hour = this.internalTimeValue.hour;
                        const minute = this.internalTimeValue.minute;
                        const meridiem = hour >= 12 ? "PM" : "AM";
                        if (hour > 12) {
                            hour -= 12;
                        }
                        return `${hour}:${string_1.padLeft(minute.toString(), 2, "0")} ${meridiem}`;
                    },
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    internalTimeValue() {
                        if (this.internalTimeValue.hour === undefined || this.internalTimeValue.minute === undefined) {
                            this.internalValue = "";
                        }
                        else {
                            this.internalValue = `${this.internalTimeValue.hour}:${string_1.padLeft(this.internalTimeValue.minute.toString(), 2, "0")}:00`;
                        }
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            var _a;
                            const values = /^(\d+):(\d+)/.exec((_a = this.modelValue) !== null && _a !== void 0 ? _a : "");
                            if (values !== null) {
                                this.internalTimeValue = {
                                    hour: number_1.toNumber(values[1]),
                                    minute: number_1.toNumber(values[2])
                                };
                            }
                            else {
                                this.internalTimeValue = {};
                            }
                        }
                    }
                },
                template: `
<TimePicker v-model="internalTimeValue" />
`
            }));
        }
    };
});
//# sourceMappingURL=timeFieldComponents.js.map