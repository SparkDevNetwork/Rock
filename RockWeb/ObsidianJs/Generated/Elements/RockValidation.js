define(["require", "exports", "../Vendor/Vue/vue.js", "../Vendor/VeeValidate/vee-validate.js"], function (require, exports, vue_js_1, vee_validate_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'RockValidation',
        components: {
            Form: vee_validate_js_1.Form
        },
        props: {
            errors: {
                type: Object,
                required: true
            }
        },
        provide: function () {
            return {
                errors: this.errors
            };
        },
        computed: {
            hasErrors: function () {
                return Object.keys(this.errors).length > 0;
            }
        },
        template: "\n<div v-if=\"hasErrors\" class=\"alert alert-validation\">\n    Please correct the following:\n    <ul>\n        <li v-for=\"error of errors\">{{error}}</li>\n    </ul>\n</div>\n<slot />"
    });
});
//# sourceMappingURL=RockValidation.js.map