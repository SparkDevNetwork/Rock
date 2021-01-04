define(["require", "exports", "../Vendor/Vue/vue.js", "./Index.js", "../Filters/Date.js"], function (require, exports, vue_js_1, Index_js_1, Date_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var fieldTypeGuid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';
    exports.default = Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
        name: 'DateField',
        props: {
            modelValue: {
                type: String,
                required: true
            }
        },
        computed: {
            valueAsDateString: function () {
                return Date_js_1.asDateString(this.modelValue);
            }
        },
        template: "\n<span>{{modelValue}} => {{ valueAsDateString }}</span>"
    }));
});
//# sourceMappingURL=DateField.js.map