define(["require", "exports", "../Fields/Index.js", "../Vendor/Vue/vue.js", "../Fields/TextField.js", "../Fields/BooleanField.js", "../Fields/DateField.js"], function (require, exports, Index_js_1, vue_js_1, TextField_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
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
    });
});
//# sourceMappingURL=RockField.js.map