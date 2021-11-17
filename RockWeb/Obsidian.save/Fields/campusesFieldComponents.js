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
                name: "CampusesField.Edit",
                components: {
                    CheckBoxList: checkBoxList_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup(props, context) {
                    const internalValue = vue_1.ref(props.modelValue ? props.modelValue.split(",") : []);
                    const options = vue_1.computed(() => {
                        var _a;
                        try {
                            return JSON.parse((_a = props.configurationValues["values"]) !== null && _a !== void 0 ? _a : "[]");
                        }
                        catch (_b) {
                            return [];
                        }
                    });
                    const repeatColumns = vue_1.computed(() => {
                        var _a;
                        const repeatColumnsConfig = props.configurationValues["repeatColumns"];
                        return (_a = number_1.toNumberOrNull(repeatColumnsConfig)) !== null && _a !== void 0 ? _a : 4;
                    });
                    vue_1.watch(() => props.modelValue, () => internalValue.value = props.modelValue ? props.modelValue.split(",") : []);
                    vue_1.watchEffect(() => context.emit("update:modelValue", internalValue.value.join(",")));
                    return {
                        internalValue,
                        options,
                        repeatColumns
                    };
                },
                template: `
<CheckBoxList v-model="internalValue" horizontal :options="options" :repeatColumns="repeatColumns" />
`
            }));
        }
    };
});
//# sourceMappingURL=campusesFieldComponents.js.map