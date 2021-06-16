System.register(["../Rules/Index", "vue", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var Index_1, vue_1, RockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'EmailBox',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
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
                template: "\n<RockFormField\n    v-model=\"internalValue\"\n    formGroupClasses=\"rock-text-box\"\n    name=\"textbox\"\n    :rules=\"computedRules\">\n    <template #default=\"{uniqueId, field, errors, tabIndex, disabled}\">\n        <div class=\"control-wrapper\">\n            <div class=\"input-group\">\n                <span class=\"input-group-addon\">\n                    <i class=\"fa fa-envelope\"></i>\n                </span>\n                <input :id=\"uniqueId\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" :tabindex=\"tabIndex\" />\n            </div>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=EmailBox.js.map