System.register(["vue", "vee-validate", "./RockValidation.js"], function (exports_1, context_1) {
    "use strict";
    var vue_1, vee_validate_1, RockValidation_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (vee_validate_1_1) {
                vee_validate_1 = vee_validate_1_1;
            },
            function (RockValidation_js_1_1) {
                RockValidation_js_1 = RockValidation_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'RockForm',
                components: {
                    Form: vee_validate_1.Form,
                    RockValidation: RockValidation_js_1.default
                },
                data: function () {
                    return {
                        errorsToDisplay: [],
                        submitCount: 0
                    };
                },
                methods: {
                    onInternalSubmit: function (handleSubmit, $event) {
                        this.submitCount++;
                        return handleSubmit($event, this.emitSubmit);
                    },
                    emitSubmit: function (payload) {
                        this.$emit('submit', payload);
                    }
                },
                template: "\n<Form as=\"\" #default=\"{errors, handleSubmit}\">\n    <RockValidation :submitCount=\"submitCount\" :errors=\"errors\" />\n    <form @submit=\"onInternalSubmit(handleSubmit, $event)\">\n        <slot />\n    </form>\n</Form>"
            }));
        }
    };
});
//# sourceMappingURL=RockForm.js.map