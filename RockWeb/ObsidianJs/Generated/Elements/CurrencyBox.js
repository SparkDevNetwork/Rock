System.register(["../Filters/Currency.js", "../Vendor/Vue/vue.js", "./TextBox.js"], function (exports_1, context_1) {
    "use strict";
    var Currency_js_1, vue_js_1, TextBox_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Currency_js_1_1) {
                Currency_js_1 = Currency_js_1_1;
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
                name: 'CurrencyBox',
                components: {
                    TextBox: TextBox_js_1.default
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
                        internalValue: Currency_js_1.asFormattedString(this.modelValue)
                    };
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', Currency_js_1.toNumberOrNull(this.internalValue));
                    },
                    value: function () {
                        this.internalValue = Currency_js_1.asFormattedString(this.modelValue);
                    }
                },
                template: "\n<TextBox v-model.lazy=\"internalValue\">\n    <template #preFormControl>\n        <span class=\"input-group-addon\">$</span>\n    </template>\n</TextBox>"
            }));
        }
    };
});
//# sourceMappingURL=CurrencyBox.js.map