System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'RockValidation',
                props: {
                    errors: {
                        type: Object,
                        required: true
                    }
                },
                computed: {
                    hasErrors: function () {
                        return Object.keys(this.errors).length > 0;
                    }
                },
                template: "\n<div v-if=\"hasErrors\" class=\"alert alert-validation\">\n    Please correct the following:\n    <ul>\n        <li v-for=\"(error, fieldLabel) of errors\">\n            <strong>{{fieldLabel}}</strong>\n            {{error}}\n        </li>\n    </ul>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RockValidation.js.map