System.register(["../Rules/Index.js", "../Vendor/Vue/vue.js", "./TextBox.js"], function (exports_1, context_1) {
    "use strict";
    var Index_js_1, vue_js_1, TextBox_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (TextBox_js_1_1) {
                TextBox_js_1 = TextBox_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'YesNoSelect',
                components: {
                    TextBox: TextBox_js_1.default
                },
                props: {
                    modelValue: {
                        type: Boolean,
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
            }));
        }
    };
});
//# sourceMappingURL=YesNoSelect.js.map