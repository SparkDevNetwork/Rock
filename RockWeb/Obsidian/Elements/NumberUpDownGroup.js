System.register(["vue", "./NumberUpDown", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, NumberUpDown_1, RockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (NumberUpDown_1_1) {
                NumberUpDown_1 = NumberUpDown_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'NumberUpDownGroup',
                components: {
                    RockFormField: RockFormField_1.default,
                    NumberUpDownInternal: NumberUpDown_1.NumberUpDownInternal
                },
                props: {
                    modelValue: {
                        type: Object,
                        required: true
                    },
                    options: {
                        type: Array,
                        required: true
                    }
                },
                computed: {
                    total: function () {
                        var total = 0;
                        for (var _i = 0, _a = this.options; _i < _a.length; _i++) {
                            var option = _a[_i];
                            total += (this.modelValue[option.key] || 0);
                        }
                        return total;
                    }
                },
                template: "\n<RockFormField\n    :modelValue=\"total\"\n    formGroupClasses=\"margin-b-md number-up-down-group\"\n    name=\"numberupdowngroup\">\n    <template #default=\"{uniqueId, field, errors, disabled}\">\n        <div class=\"control-wrapper\">\n            <div v-for=\"option in options\" :key=\"option.key\" class=\"margin-l-sm margin-b-sm\">\n                <div v-if=\"option.label\" class=\"margin-b-sm\">\n                    {{option.label}}\n                </div>\n                <NumberUpDownInternal v-model=\"modelValue[option.key]\" :min=\"option.min\" :max=\"option.max\" class=\"margin-t-sm\" />\n            </div>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=NumberUpDownGroup.js.map