System.register(["vue", "./utils", "../Elements/checkBoxList", "../Services/number"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, checkBoxList_1, number_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (checkBoxList_1_1) {
                checkBoxList_1 = checkBoxList_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "DaysOfWeekField.Edit",
                components: {
                    CheckBoxList: checkBoxList_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: [],
                    };
                },
                methods: {
                    options() {
                        return [
                            { text: "Sunday", value: 0..toString() },
                            { text: "Monday", value: 1..toString() },
                            { text: "Tuesday", value: 2..toString() },
                            { text: "Wednesday", value: 3..toString() },
                            { text: "Thursday", value: 4..toString() },
                            { text: "Friday", value: 5..toString() },
                            { text: "Saturday", value: 6..toString() }
                        ];
                    },
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue.sort((a, b) => number_1.toNumber(a) - number_1.toNumber(b)).join(","));
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            var _a;
                            const value = (_a = this.modelValue) !== null && _a !== void 0 ? _a : "";
                            this.internalValue = value !== "" ? value.split(",") : [];
                        }
                    }
                },
                template: `
<CheckBoxList v-model="internalValue" :options="options()" />
`
            }));
        }
    };
});
//# sourceMappingURL=daysOfWeekFieldComponents.js.map