System.register(["vue", "./Index", "../Services/String", "../Elements/PhoneNumberBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, String_1, PhoneNumberBox_1, fieldTypeGuid;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (PhoneNumberBox_1_1) {
                PhoneNumberBox_1 = PhoneNumberBox_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '6B1908EC-12A2-463A-A7BD-970CE0FAF097';
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'PhoneNumber',
                components: {
                    PhoneNumberBox: PhoneNumberBox_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: ''
                    };
                },
                computed: {
                    safeValue: function () {
                        return String_1.formatPhoneNumber(this.modelValue || '');
                    },
                    configAttributes: function () {
                        var attributes = {};
                        return attributes;
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue || '';
                        }
                    }
                },
                template: "\n<PhoneNumberBox v-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"configAttributes\" />\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=PhoneNumber.js.map