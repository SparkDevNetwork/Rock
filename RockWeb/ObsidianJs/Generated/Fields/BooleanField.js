define(["require", "exports", "../Vendor/Vue/vue.js", "./Index.js", "../Filters/Boolean.js"], function (require, exports, vue_js_1, Index_js_1, Boolean_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var fieldTypeGuid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';
    exports.default = Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
        name: 'BooleanField',
        props: {
            modelValue: {
                type: String,
                required: true
            }
        },
        computed: {
            valueAsYesNoOrNull: function () {
                return Boolean_js_1.asYesNoOrNull(this.modelValue);
            }
        },
        template: "\n<span>{{ valueAsYesNoOrNull }}</span>"
    }));
});
//# sourceMappingURL=BooleanField.js.map