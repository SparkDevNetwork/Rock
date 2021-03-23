System.register(["vee-validate", "../Services/Number", "vue", "./RockLabel", "../Util/Guid"], function (exports_1, context_1) {
    "use strict";
    var vee_validate_1, Number_1, vue_1, RockLabel_1, Guid_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vee_validate_1_1) {
                vee_validate_1 = vee_validate_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockLabel_1_1) {
                RockLabel_1 = RockLabel_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'CurrencyBox',
                components: {
                    RockLabel: RockLabel_1.default,
                    Field: vee_validate_1.Field
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
                    isRequired: function () {
                        return this.rules.includes('required');
                    },
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
                template: "\n<Field\n    v-model=\"internalValue\"\n    @change=\"onChange\"\n    :name=\"label\"\n    :rules=\"rules\"\n    #default=\"{field, errors}\">\n    <div class=\"form-group rock-currency-box\" :class=\"{required: isRequired, 'has-error': Object.keys(errors).length}\">\n        <RockLabel :for=\"uniqueId\" :help=\"help\">\n            {{label}}\n        </RockLabel>\n        <div class=\"input-group\">\n            <span class=\"input-group-addon\">$</span>\n            <input :id=\"uniqueId\" type=\"text\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" />\n        </div>\n    </div>\n</Field>"
            }));
        }
    };
});
//# sourceMappingURL=CurrencyBox.js.map