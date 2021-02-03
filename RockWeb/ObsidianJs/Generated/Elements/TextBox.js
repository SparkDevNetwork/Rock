System.register(["vue", "../Util/Guid", "vee-validate", "./RockLabel"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Guid_1, vee_validate_1, RockLabel_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (vee_validate_1_1) {
                vee_validate_1 = vee_validate_1_1;
            },
            function (RockLabel_1_1) {
                RockLabel_1 = RockLabel_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'TextBox1',
                components: {
                    Field: vee_validate_1.Field,
                    RockLabel: RockLabel_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    label: {
                        type: String,
                        default: ''
                    },
                    help: {
                        type: String,
                        default: ''
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
                    rules: {
                        type: String,
                        default: ''
                    },
                    disabled: {
                        type: Boolean,
                        default: false
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
                        uniqueId: "rock-textbox-" + Guid_1.newGuid(),
                        internalValue: this.modelValue
                    };
                },
                computed: {
                    isRequired: function () {
                        return this.rules.includes('required');
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
                methods: {
                    handleInput: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                watch: {
                    modelValue: function () {
                        this.internalValue = this.modelValue;
                    }
                },
                template: "\n<Field\n    v-model=\"internalValue\"\n    @input=\"handleInput\"\n    :name=\"label\"\n    :rules=\"rules\"\n    #default=\"{field, errors}\">\n    <em v-if=\"showCountDown\" class=\"pull-right badge\" :class=\"countdownClass\">\n        {{charsRemaining}}\n    </em>\n    <div class=\"form-group rock-text-box\" :class=\"{required: isRequired, 'has-error': Object.keys(errors).length}\">\n        <RockLabel v-if=\"label\" :for=\"uniqueId\" :help=\"help\">\n            {{label}}\n        </RockLabel>\n        <div class=\"control-wrapper\">\n            <input :id=\"uniqueId\" :type=\"type\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" :maxlength=\"maxLength\" :placeholder=\"placeholder\" />\n        </div>\n    </div>\n</Field>"
            }));
        }
    };
});
//# sourceMappingURL=TextBox.js.map