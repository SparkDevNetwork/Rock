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
                name: 'RockButton',
                props: {
                    isLoading: {
                        type: Boolean,
                        default: false
                    },
                    loadingText: {
                        type: String,
                        default: 'Loading...'
                    },
                    type: {
                        type: String,
                        default: 'button'
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    }
                },
                emits: [
                    'click'
                ],
                methods: {
                    handleClick: function (event) {
                        this.$emit('click', event);
                    }
                },
                template: "<button class=\"btn\" :disabled=\"isLoading || disabled\" @click=\"handleClick\" :type=\"type\">\n    <template v-if=\"isLoading\">\n        {{loadingText}}\n    </template>\n    <slot v-else />\n</button>"
            }));
        }
    };
});
//# sourceMappingURL=RockButton.js.map