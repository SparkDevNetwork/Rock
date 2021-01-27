System.register(["../Vendor/VeeValidate/vee-validate.js", "../Filters/Number.js", "../Vendor/Vue/vue.js", "./RockLabel.js", "../Util/Guid.js"], function (exports_1, context_1) {
    "use strict";
    var vee_validate_js_1, Number_js_1, vue_js_1, RockLabel_js_1, Guid_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vee_validate_js_1_1) {
                vee_validate_js_1 = vee_validate_js_1_1;
            },
            function (Number_js_1_1) {
                Number_js_1 = Number_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (RockLabel_js_1_1) {
                RockLabel_js_1 = RockLabel_js_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'CurrencyBox',
                components: {
                    RockLabel: RockLabel_js_1.default,
                    Field: vee_validate_js_1.Field
                },
                props: {
                    modelValue: {
                        type: Number,
                        default: null
                    },
                    label: {
                        type: String,
                        required: true
                    },
                    help: {
                        type: String,
                        default: ''
                    },
                    rules: {
                        type: String,
                        default: ''
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        uniqueId: "rock-currencybox-" + Guid_js_1.newGuid(),
                        internalValue: ''
                    };
                },
                methods: {
                    onChange: function () {
                        this.internalValue = Number_js_1.asFormattedString(this.modelValue);
                    }
                },
                computed: {
                    isRequired: function () {
                        return this.rules.includes('required');
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', Number_js_1.toNumberOrNull(this.internalValue));
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = Number_js_1.asFormattedString(this.modelValue);
                        }
                    }
                },
                template: "\n<Field\n    v-model.lazy=\"internalValue\"\n    @change=\"onChange\"\n    :name=\"label\"\n    :rules=\"rules\"\n    #default=\"{field, errors}\">\n    <div class=\"form-group rock-currency-box\" :class=\"{required: isRequired, 'has-error': Object.keys(errors).length}\">\n        <RockLabel :for=\"uniqueId\" :help=\"help\">\n            {{label}}\n        </RockLabel>\n        <div class=\"input-group\">\n            <span class=\"input-group-addon\">$</span>\n            <input :id=\"uniqueId\" type=\"text\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" />\n        </div>\n    </div>\n</Field>"
            }));
        }
    };
});
//# sourceMappingURL=CurrencyBox.js.map