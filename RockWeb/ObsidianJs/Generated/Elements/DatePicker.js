System.register(["../Vendor/Vue/vue.js", "../Util/Guid.js", "../Vendor/VeeValidate/vee-validate.js", "./RockLabel.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Guid_js_1, vee_validate_js_1, RockLabel_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            },
            function (vee_validate_js_1_1) {
                vee_validate_js_1 = vee_validate_js_1_1;
            },
            function (RockLabel_js_1_1) {
                RockLabel_js_1 = RockLabel_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'DatePicker',
                components: {
                    Field: vee_validate_js_1.Field,
                    RockLabel: RockLabel_js_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
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
                        uniqueId: "rock-textbox-" + Guid_js_1.newGuid(),
                        internalValue: this.modelValue
                    };
                },
                computed: {
                    isRequired: function () {
                        return this.rules.includes('required');
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
                template: "\n<Field\n    v-model=\"internalValue\"\n    @input=\"handleInput\"\n    :name=\"label\"\n    :rules=\"rules\"\n    #default=\"{field, errors}\">\n    <div class=\"form-group date-picker\" :class=\"{required: isRequired, 'has-error': Object.keys(errors).length}\">\n        <RockLabel :for=\"uniqueId\" :help=\"help\">\n            {{label}}\n        </RockLabel>\n        <div class=\"control-wrapper\">\n            <div class=\"input-group input-width-md date\">\n                <input :id=\"uniqueId\" type=\"text\" class=\"form-control\" :disabled=\"disabled\" v-bind=\"field\" onfocus=\"(this.type='date')\" onblur=\"(this.type='text')\" />\n                <label :for=\"uniqueId\" class=\"input-group-addon\" :disabled=\"disabled\">\n                    <i class=\"fa fa-calendar\"></i>\n                </span>\n            </div>\n        </div>\n    </div>\n</Field>"
            }));
        }
    };
});
//# sourceMappingURL=DatePicker.js.map