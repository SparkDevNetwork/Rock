System.register(["../Vendor/Vue/vue.js", "./Index.js", "../Filters/Boolean.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Index_js_1, Boolean_js_1, fieldTypeGuid;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (Boolean_js_1_1) {
                Boolean_js_1 = Boolean_js_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';
            exports_1("default", Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
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
            })));
        }
    };
});
//# sourceMappingURL=BooleanField.js.map