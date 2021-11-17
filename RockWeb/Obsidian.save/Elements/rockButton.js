System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1, BtnType, BtnSize;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            (function (BtnType) {
                BtnType["Default"] = "default";
                BtnType["Primary"] = "primary";
                BtnType["Danger"] = "danger";
                BtnType["Warning"] = "warning";
                BtnType["Success"] = "success";
                BtnType["Info"] = "info";
                BtnType["Link"] = "link";
            })(BtnType || (BtnType = {}));
            exports_1("BtnType", BtnType);
            (function (BtnSize) {
                BtnSize["Default"] = "";
                BtnSize["ExtraSmall"] = "xs";
                BtnSize["Small"] = "sm";
                BtnSize["Large"] = "lg";
            })(BtnSize || (BtnSize = {}));
            exports_1("BtnSize", BtnSize);
            exports_1("default", vue_1.defineComponent({
                name: "RockButton",
                props: {
                    isLoading: {
                        type: Boolean,
                        default: false
                    },
                    loadingText: {
                        type: String,
                        default: "Loading..."
                    },
                    type: {
                        type: String,
                        default: "button"
                    },
                    disabled: {
                        type: Boolean,
                        default: false
                    },
                    btnType: {
                        type: String,
                        default: BtnType.Default
                    },
                    btnSize: {
                        type: String,
                        default: BtnSize.Default
                    }
                },
                emits: [
                    "click"
                ],
                methods: {
                    handleClick: function (event) {
                        if (!this.isLoading) {
                            this.$emit("click", event);
                        }
                    }
                },
                computed: {
                    typeClass() {
                        return `btn-${this.btnType}`;
                    },
                    sizeClass() {
                        if (!this.btnSize) {
                            return "";
                        }
                        return `btn-${this.btnSize}`;
                    },
                    cssClasses() {
                        return `btn ${this.typeClass} ${this.sizeClass}`;
                    }
                },
                template: `
<button :class="cssClasses" :disabled="isLoading || disabled" @click="handleClick" :type="type">
    <template v-if="isLoading">
        {{loadingText}}
    </template>
    <slot v-else />
</button>`
            }));
        }
    };
});
//# sourceMappingURL=rockButton.js.map