System.register(["../Fields/Index", "../Vendor/Vue/vue", "../Fields/TextField", "../Fields/BooleanField", "../Fields/DateField", "../Fields/DefinedValueField"], function (exports_1, context_1) {
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
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'RockField',
                props: {
                    fieldTypeGuid: {
                        type: String,
                        required: true
                    }
                },
                computed: {
                    fieldComponent: function () {
                        var field = Index_1.getFieldTypeComponent(this.fieldTypeGuid);
                        if (!field) {
                            // Fallback to text field
                            return TextField_1.default.component;
                        }
                        return field;
                    }
                },
                template: "\n<component :is=\"fieldComponent\" />"
            }));
        }
    };
});
//# sourceMappingURL=RockField.js.map