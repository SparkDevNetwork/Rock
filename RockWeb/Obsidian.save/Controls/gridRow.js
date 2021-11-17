System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "GridRow",
                props: {
                    rowContext: {
                        type: Object,
                        required: true
                    }
                },
                provide() {
                    return {
                        rowContext: this.rowContext
                    };
                },
                methods: {
                    onRowClick() {
                        if (!this.rowContext.isHeader) {
                            this.$emit("click:body", this.rowContext);
                        }
                        else {
                            this.$emit("click:header", this.rowContext);
                        }
                    }
                },
                template: `
<tr @click="onRowClick">
    <slot />
</tr>`
            }));
        }
    };
});
//# sourceMappingURL=gridRow.js.map