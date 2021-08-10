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
                name: 'ColorPicker',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    placeholder: {
                        type: String,
                        default: ''
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        internalValue: this.modelValue
                    };
                },
                mounted: function () {
                    var _this = this;
                    var $colorPicker = window['$'](this.$refs.colorPicker);
                    $colorPicker.colorpicker();
                    $colorPicker.find('> input').on('change', function () {
                        _this.internalValue = $colorPicker.find('> input').val();
                    });
                },
                computed: {},
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    modelValue: function () {
                        this.internalValue = this.modelValue;
                    }
                },
                template: "\n<RockFormField\n    v-model=\"internalValue\"\n    formGroupClasses=\"rock-color-picker\"\n    name=\"colorpicker\">\n    <template #default=\"{uniqueId, field, errors, disabled, tabIndex}\">\n        <div class=\"control-wrapper\">\n            <div ref=\"colorPicker\" class=\"input-group input-width-lg\">\n                <input :id=\"uniqueId\" type=\"text\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" :placeholder=\"placeholder\" :tabindex=\"tabIndex\" />\n                <span class=\"input-group-addon\">\n                    <i></i>\n                </span>\n            </div>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=ColorPicker.js.map