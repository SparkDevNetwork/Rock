System.register(["vue", "./utils", "../Elements/socialSecurityNumberBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, socialSecurityNumberBox_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (socialSecurityNumberBox_1_1) {
                socialSecurityNumberBox_1 = socialSecurityNumberBox_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "SSNField.Edit",
                components: {
                    SocialSecurityNumberBox: socialSecurityNumberBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup(props, { emit }) {
                    const internalValue = vue_1.ref(props.modelValue);
                    vue_1.watch(() => props.modelValue, () => { var _a; return internalValue.value = (_a = props.modelValue) !== null && _a !== void 0 ? _a : ""; });
                    vue_1.watchEffect(() => emit("update:modelValue", internalValue.value));
                    return {
                        internalValue
                    };
                },
                template: `
<SocialSecurityNumberBox v-model="internalValue" />
`
            }));
        }
    };
});
//# sourceMappingURL=ssnFieldComponents.js.map