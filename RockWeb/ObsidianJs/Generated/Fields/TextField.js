System.register(["../Vendor/Vue/vue.js", "./Index.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Index_js_1, fieldTypeGuid;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '9C204CD0-1233-41C5-818A-C5DA439445AA';
            exports_1("default", Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
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
            })));
        }
    };
});
//# sourceMappingURL=TextField.js.map