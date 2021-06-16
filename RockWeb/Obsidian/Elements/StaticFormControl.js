System.register(["vue", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'StaticFormControl',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        required: true
                    }
                },
                template: "\n<RockFormField\n    :modelValue=\"modelValue\"\n    formGroupClasses=\"static-control\"\n    name=\"static-form-control\">\n    <template #default=\"{uniqueId, field, errors, disabled}\">\n        <div class=\"control-wrapper\">\n            <div class=\"form-control-static\">\n                {{ modelValue }}\n            </div>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=StaticFormControl.js.map