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
            emitSubmit: function (payload) {
                this.$emit('submit', payload);
            }
        },
        template: "\n<Form as=\"div\" #default=\"{errors, handleSubmit, submitCount}\">\n    <RockValidation v-if=\"submitCount\" :errors=\"errors\" />\n    <form @submit=\"handleSubmit($event, emitSubmit)\">\n        <slot />\n    </form>\n</Form>"
    });
});
//# sourceMappingURL=RockForm.js.map