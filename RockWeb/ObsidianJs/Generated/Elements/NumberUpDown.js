System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'NumberUpDown',
                props: {
                    modelValue: {
                        type: Number,
                        required: true
                    },
                    min: {
                        type: Number,
                        default: 1
                    },
                    max: {
                        type: Number,
                        default: 9
                    }
                },
                methods: {
                    goUp: function () {
                        if (!this.isUpDisabled) {
                            this.$emit('update:modelValue', this.modelValue + 1);
                        }
                    },
                    goDown: function () {
                        if (!this.isDownDisabled) {
                            this.$emit('update:modelValue', this.modelValue - 1);
                        }
                    }
                },
                computed: {
                    isUpDisabled: function () {
                        return this.modelValue >= this.max;
                    },
                    isDownDisabled: function () {
                        return this.modelValue <= this.min;
                    }
                },
                template: "\n<div class=\"numberincrement\">\n    <a @click=\"goDown\" class=\"numberincrement-down\" :class=\"{disabled: isDownDisabled}\" :disabled=\"isDownDisabled\">\n        <i class=\"fa fa-minus \"></i>\n    </a>\n    <span class=\"numberincrement-value\">{{modelValue}}</span>\n    <a @click=\"goUp\" class=\"numberincrement-up\" :class=\"{disabled: isUpDisabled}\" :disabled=\"isUpDisabled\">\n        <i class=\"fa fa-plus \"></i>\n    </a>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=NumberUpDown.js.map