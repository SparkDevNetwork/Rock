System.register(["vue", "./utils", "../Elements/emailBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, emailBox_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (emailBox_1_1) {
                emailBox_1 = emailBox_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "EmailField.Edit",
                components: {
                    EmailBox: emailBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: ""
                    };
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
<EmailBox v-model="internalValue" />
`
            }));
        }
    };
});
//# sourceMappingURL=emailFieldComponents.js.map