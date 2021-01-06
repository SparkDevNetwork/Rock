System.register(["../Fields/Index.js", "../Vendor/Vue/vue.js", "../Fields/TextField.js", "../Fields/BooleanField.js", "../Fields/DateField.js"], function (exports_1, context_1) {
    "use strict";
    var Index_js_1, vue_js_1, TextField_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (TextField_js_1_1) {
                TextField_js_1 = TextField_js_1_1;
            },
            function (_1) {
            },
            function (_2) {
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'RockField',
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    fieldTypeGuid: {
                        type: String,
                        required: true
                    }
                },
                computed: {
                    fieldComponent: function () {
                        var field = Index_js_1.getFieldTypeComponent(this.fieldTypeGuid);
                        if (!field) {
                            // Fallback to text field
                            return TextField_js_1.default.component;
                        }
                        return field;
                    }
                },
                template: "\n<component :is=\"fieldComponent\" v-model=\"modelValue\" />"
            }));
        }
    };
});
//# sourceMappingURL=RockField.js.map