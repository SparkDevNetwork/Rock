System.register(["../Vendor/Vue/vue.js", "./Index.js", "../Filters/Date.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Index_js_1, Date_js_1, fieldTypeGuid;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (Date_js_1_1) {
                Date_js_1 = Date_js_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';
            exports_1("default", Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
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
                template: "\n<span>{{ valueAsDateString }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=DateField.js.map