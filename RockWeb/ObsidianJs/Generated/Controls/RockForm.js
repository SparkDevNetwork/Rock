System.register(["../Vendor/Vue/vue.js", "../Vendor/VeeValidate/vee-validate.js", "./RockValidation.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, vee_validate_js_1, RockValidation_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (vee_validate_js_1_1) {
                vee_validate_js_1 = vee_validate_js_1_1;
            },
            function (RockValidation_js_1_1) {
                RockValidation_js_1 = RockValidation_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
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
                template: "\n<Form as=\"\" #default=\"{errors, handleSubmit, submitCount}\">\n    <RockValidation v-if=\"submitCount\" :errors=\"errors\" />\n    <form @submit=\"handleSubmit($event, emitSubmit)\">\n        <slot />\n    </form>\n</Form>"
            }));
        }
    };
});
//# sourceMappingURL=RockForm.js.map