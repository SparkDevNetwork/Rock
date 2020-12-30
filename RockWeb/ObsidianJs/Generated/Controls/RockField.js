define(["require", "exports", "../Fields/Index.js", "../Vendor/Vue/vue.js", "./BooleanField.js", "./TextField.js"], function (require, exports, Index_js_1, vue_js_1) {
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
                return Index_js_1.getFieldTypeComponent(this.fieldTypeGuid);
            }
        },
        template: "\n<component :is=\"fieldComponent\" v-model=\"modelValue\" />"
    });
});
//# sourceMappingURL=RockField.js.map