System.register(["../Fields/Index", "vue", "../Fields/TextField", "../Fields/BooleanField", "../Fields/ColorField", "../Fields/DateField", "../Fields/DefinedValueField", "../Fields/GenderField", "../Fields/SingleSelect", "../Fields/PhoneNumber", "../Fields/MemoField"], function (exports_1, context_1) {
    "use strict";
    var Index_1, vue_1, TextField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (TextField_1_1) {
                TextField_1 = TextField_1_1;
            },
            function (_1) {
            },
            function (_2) {
            },
            function (_3) {
            },
            function (_4) {
            },
            function (_5) {
            },
            function (_6) {
            },
            function (_7) {
            },
            function (_8) {
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'RockField',
                props: {
                    fieldTypeGuid: {
                        type: String,
                        required: true
                    },
                    rules: {
                        type: String,
                        default: ''
                    }
                },
                setup: function (props) {
                    var isRequired = vue_1.computed(function () { return props.rules.includes('required'); });
                    vue_1.provide('isRequired', isRequired);
                },
                computed: {
                    fieldComponent: function () {
                        var field = Index_1.getFieldTypeComponent(this.fieldTypeGuid);
                        if (!field) {
                            return TextField_1.default.component;
                        }
                        return field;
                    }
                },
                template: "\n<component :is=\"fieldComponent\" :rules=\"rules\" />"
            }));
        }
    };
});
//# sourceMappingURL=RockField.js.map