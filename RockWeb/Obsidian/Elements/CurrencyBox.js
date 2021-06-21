System.register(["../Services/Number", "vue", "../Util/Guid", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var Number_1, vue_1, Guid_1, RockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'CurrencyBox',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: Number,
                        default: null
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        uniqueId: "rock-currencybox-" + Guid_1.newGuid(),
                        internalValue: ''
                    };
                },
                methods: {
                    onChange: function () {
                        this.internalValue = Number_1.asFormattedString(this.modelValue);
                    }
                },
                computed: {
                    internalNumberValue: function () {
                        return Number_1.toNumberOrNull(this.internalValue);
                    }
                },
                watch: {
                    internalNumberValue: function () {
                        this.$emit('update:modelValue', this.internalNumberValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            if (this.modelValue !== this.internalNumberValue) {
                                this.internalValue = Number_1.asFormattedString(this.modelValue);
                            }
                        }
                    }
                },
                template: "\n<RockFormField\n    v-model=\"internalValue\"\n    @change=\"onChange\"\n    formGroupClasses=\"rock-currency-box\"\n    name=\"currencybox\">\n    <template #default=\"{uniqueId, field, errors, disabled, inputGroupClasses}\">\n        <div class=\"control-wrapper\">\n            <div class=\"input-group\" :class=\"inputGroupClasses\">\n                <span class=\"input-group-addon\">$</span>\n                <input :id=\"uniqueId\" type=\"text\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" />\n            </div>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=CurrencyBox.js.map