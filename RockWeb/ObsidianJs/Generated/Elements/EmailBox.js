System.register(["../Rules/Index", "vue", "./TextBox"], function (exports_1, context_1) {
    "use strict";
    var Index_1, vue_1, TextBox_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'EmailBox',
                components: {
                    TextBox: TextBox_1.default
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
                        var rules = Index_1.ruleStringToArray(this.rules);
                        if (rules.indexOf('email') === -1) {
                            rules.push('email');
                        }
                        return Index_1.ruleArrayToString(rules);
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
                template: "\n<TextBox v-model.trim=\"internalValue\" :label=\"label\" :rules=\"computedRules\" />"
            }));
        }
    };
});
//# sourceMappingURL=EmailBox.js.map