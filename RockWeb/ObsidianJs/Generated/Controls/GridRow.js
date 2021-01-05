System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1;
    var __moduleName = context_1 && context_1.id;
    function OfType() {
        return vue_js_1.defineComponent({
            name: 'GridRow',
            props: {
                rowContext: {
                    type: Object,
                    required: true
                }
            },
            provide: function () {
                return {
                    rowContext: this.rowContext
                };
            },
            methods: {
                onRowClick: function () {
                    if (!this.rowContext.isHeader) {
                        this.$emit('click:body', this.rowContext);
                    }
                    else {
                        this.$emit('click:header', this.rowContext);
                    }
                }
            },
            template: "\n<tr @click=\"onRowClick\">\n    <slot />\n</tr>"
        });
    }
    exports_1("default", OfType);
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
        }
    };
});
//# sourceMappingURL=GridRow.js.map