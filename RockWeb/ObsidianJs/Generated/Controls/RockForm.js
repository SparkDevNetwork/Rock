System.register(["vue", "vee-validate", "./RockValidation"], function (exports_1, context_1) {
    "use strict";
    var vue_1, vee_validate_1, RockValidation_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (vee_validate_1_1) {
                vee_validate_1 = vee_validate_1_1;
            },
            function (RockValidation_1_1) {
                RockValidation_1 = RockValidation_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'RockForm',
                components: {
                    Form: vee_validate_1.Form,
                    RockValidation: RockValidation_1.default
                },
                setup: function () {
                    var formState = {
                        submitCount: 0
                    };
                    vue_1.provide('formState', formState);
                    return {
                        formState: formState
                    };
                },
                data: function () {
                    return {
                        errorsToDisplay: []
                    };
                },
                methods: {
                    onInternalSubmit: function (handleSubmit, $event) {
                        this.formState.submitCount++;
                        return handleSubmit($event, this.emitSubmit);
                    },
                    emitSubmit: function (payload) {
                        this.$emit('submit', payload);
                    }
                },
                template: "\n<Form as=\"\" #default=\"{errors, handleSubmit}\">\n    <RockValidation :submitCount=\"formState.submitCount\" :errors=\"errors\" />\n    <form @submit=\"onInternalSubmit(handleSubmit, $event)\">\n        <slot />\n    </form>\n</Form>"
            }));
        }
    };
});
//# sourceMappingURL=RockForm.js.map