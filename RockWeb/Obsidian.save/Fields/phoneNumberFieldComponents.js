System.register(["vue", "./utils", "../Elements/phoneNumberBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, phoneNumberBox_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (phoneNumberBox_1_1) {
                phoneNumberBox_1 = phoneNumberBox_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "PhoneNumber.Edit",
                components: {
                    PhoneNumberBox: phoneNumberBox_1.default
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
<PhoneNumberBox v-model="internalValue" v-bind="configAttributes" />
`
            }));
        }
    };
});
//# sourceMappingURL=phoneNumberFieldComponents.js.map