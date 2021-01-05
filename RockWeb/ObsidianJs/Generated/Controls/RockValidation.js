define(["require", "exports", "../Vendor/Vue/vue.js"], function (require, exports, vue_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
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
    });
});
//# sourceMappingURL=RockValidation.js.map