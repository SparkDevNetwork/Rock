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
                name: 'TextBox',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    type: {
                        type: String,
                        default: 'text'
                    },
                    maxLength: {
                        type: Number,
                        default: 524288
                    },
                    showCountDown: {
                        type: Boolean,
                        default: false
                    },
                    placeholder: {
                        type: String,
                        default: ''
                    },
                    inputClasses: {
                        type: String,
                        default: ''
                    },
                    rows: {
                        type: Number,
                        default: 3
                    },
                    textMode: {
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
                computed: {
                    isTextarea: function () {
                        var _a;
                        return ((_a = this.textMode) === null || _a === void 0 ? void 0 : _a.toLowerCase()) === 'multiline';
                    },
                    charsRemaining: function () {
                        return this.maxLength - this.modelValue.length;
                    },
                    countdownClass: function () {
                        if (this.charsRemaining >= 10) {
                            return 'badge-default';
                        }
                        if (this.charsRemaining >= 0) {
                            return 'badge-warning';
                        }
                        return 'badge-danger';
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    modelValue: function () {
                        this.internalValue = this.modelValue;
                    }
                },
                template: "\n<RockFormField\n    v-model=\"internalValue\"\n    formGroupClasses=\"rock-text-box\"\n    name=\"textbox\">\n    <template #pre>\n        <em v-if=\"showCountDown\" class=\"pull-right badge\" :class=\"countdownClass\">\n            {{charsRemaining}}\n        </em>\n    </template>\n    <template #default=\"{uniqueId, field, errors, disabled, tabIndex}\">\n        <div class=\"control-wrapper\">\n            <textarea v-if=\"isTextarea\" :rows=\"rows\" cols=\"20\" :maxlength=\"maxLength\" :id=\"uniqueId\" class=\"form-control\" v-bind=\"field\"></textarea>\n            <input v-else :id=\"uniqueId\" :type=\"type\" class=\"form-control\" :class=\"inputClasses\" v-bind=\"field\" :disabled=\"disabled\" :maxlength=\"maxLength\" :placeholder=\"placeholder\" :tabindex=\"tabIndex\" />\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=TextBox.js.map