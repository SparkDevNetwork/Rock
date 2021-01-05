define(["require", "exports", "../Vendor/Vue/vue.js", "../Vendor/VeeValidate/vee-validate.js", "./RockValidation.js"], function (require, exports, vue_js_1, vee_validate_js_1, RockValidation_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'RockForm',
        components: {
            Form: vee_validate_js_1.Form,
            RockValidation: RockValidation_js_1.default
        },
        methods: {
            onSubmit: function (payload) {
                this.$emit('submit', payload);
            }
        },
        template: "\n<Form @submit=\"onSubmit\" v-slot=\"{ errors }\">\n    {{JSON.stringify(errors)}}\n    <RockValidation :errors=\"errors\">\n        <slot />\n    </RockValidation>\n</Form>"
    });
});
//# sourceMappingURL=RockForm.js.map