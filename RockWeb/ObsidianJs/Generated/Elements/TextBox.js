define(["require", "exports", "../Vendor/Vue/vue.js", "../Util/Guid.js", "../Vendor/VeeValidate/vee-validate.js"], function (require, exports, vue_js_1, Guid_js_1, vee_validate_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'TextBox',
        components: {
            Field: vee_validate_js_1.Field
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
            type: {
                type: String,
                default: 'text'
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
            value: function () {
                this.internalValue = this.modelValue;
            }
        },
        template: "\n<Field\n    v-model=\"internalValue\"\n    @input=\"handleInput\"\n    :name=\"label\"\n    :rules=\"rules\"\n    #default=\"{field, errors}\">\n    <div class=\"form-group rock-text-box\" :class=\"{required: isRequired, 'has-error': Object.keys(errors).length}\">\n        <label class=\"control-label\" :for=\"uniqueId\">\n            {{label}}\n        </label>\n        <div class=\"control-wrapper\">\n            <input :id=\"uniqueId\" :type=\"type\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" />\n        </div>\n    </div>\n</Field>"
    });
});
//# sourceMappingURL=TextBox.js.map