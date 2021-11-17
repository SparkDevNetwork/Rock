System.register(["vue", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "StaticFormControl",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        required: true
                    }
                },
                template: `
<RockFormField
    :modelValue="modelValue"
    formGroupClasses="static-control"
    name="static-form-control">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <div class="form-control-static">
                {{ modelValue }}
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=staticFormControl.js.map