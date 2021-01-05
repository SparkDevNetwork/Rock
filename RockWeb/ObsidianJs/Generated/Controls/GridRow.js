define(["require", "exports", "../Vendor/Vue/vue.js"], function (require, exports, vue_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
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
    exports.default = OfType;
});
//# sourceMappingURL=GridRow.js.map