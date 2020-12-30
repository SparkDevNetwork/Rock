define(["require", "exports", "../Vendor/Vue/vue.js", "./Index.js"], function (require, exports, vue_js_1, Index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var fieldTypeGuid = '9C204CD0-1233-41C5-818A-C5DA439445AA';
    exports.default = Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
        name: 'TextField',
        props: {
            modelValue: {
                type: String,
                required: true
            }
        },
        computed: {
            safeValue: function () {
                return (this.modelValue || '').trim();
            },
            valueIsNull: function () {
                return !this.safeValue;
            }
        },
        template: "\n<span>{{ safeValue }}</span>"
    }));
});
//# sourceMappingURL=TextField.js.map