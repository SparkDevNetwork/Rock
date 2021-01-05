define(["require", "exports", "../Rules/Index.js", "../Vendor/Vue/vue.js", "./TextBox.js"], function (require, exports, Index_js_1, vue_js_1, TextBox_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'EmailInput',
        components: {
            TextBox: TextBox_js_1.default
        },
        props: {
            modelValue: {
                type: String,
                required: true
            },
            label: {
                type: String,
                default: 'Email'
            },
            rules: {
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
            computedRules: function () {
                var rules = Index_js_1.ruleStringToArray(this.rules);
                if (rules.indexOf('email') === -1) {
                    rules.push('email');
                }
                return Index_js_1.ruleArrayToString(rules);
            }
        },
        watch: {
            internalValue: function () {
                this.$emit('update:modelValue', this.internalValue);
            },
            value: function () {
                this.internalValue = this.modelValue;
            }
        },
        template: "\n<TextBox v-model.trim=\"internalValue\" :label=\"label\" :rules=\"computedRules\" />"
    });
});
//# sourceMappingURL=EmailInput.js.map