define(["require", "exports", "../Vendor/Vue/vue.js", "../Vendor/VeeValidate/vee-validate.js"], function (require, exports, vue_js_1, vee_validate_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'RockForm',
        components: {
            Form: vee_validate_js_1.Form
        },
        template: "\n<Form v-if=\"isEditMode\" @submit=\"doSave\" v-slot=\"{ errors }\">\n    <div class=\"alert alert-validation\">\n        Please correct the following:\n        <ul>\n            <li v-for=\"error of errors\">{{error}}</li>\n        </ul>\n    </div>\n    <slot />\n</Form>"
    });
});
//# sourceMappingURL=Form.js.map