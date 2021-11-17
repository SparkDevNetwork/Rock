System.register(["vue", "../Elements/rockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockButton_1, ValidationField;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            }
        ],
        execute: function () {
            (function (ValidationField) {
                ValidationField[ValidationField["CardNumber"] = 0] = "CardNumber";
                ValidationField[ValidationField["Expiry"] = 1] = "Expiry";
                ValidationField[ValidationField["SecurityCode"] = 2] = "SecurityCode";
            })(ValidationField || (ValidationField = {}));
            exports_1("ValidationField", ValidationField);
            exports_1("default", vue_1.defineComponent({
                name: "Dialog",
                components: {
                    RockButton: rockButton_1.default
                },
                props: {
                    modelValue: {
                        type: Boolean,
                        required: true
                    },
                    dismissible: {
                        type: Boolean,
                        default: true
                    }
                },
                data() {
                    return {
                        doShake: false
                    };
                },
                computed: {
                    hasHeader() {
                        return !!this.$slots["header"];
                    }
                },
                methods: {
                    close() {
                        this.$emit("update:modelValue", false);
                    },
                    shake() {
                        if (!this.doShake) {
                            this.doShake = true;
                            setTimeout(() => this.doShake = false, 1000);
                        }
                    },
                    centerOnScreen() {
                        this.$nextTick(() => {
                            const div = this.$refs["modalDiv"];
                            if (!div) {
                                return;
                            }
                            const height = div.offsetHeight;
                            const margin = height / 2;
                            div.style.marginTop = `-${margin}px`;
                        });
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler() {
                            const body = document.body;
                            const cssClasses = ["modal-open", "page-overflow"];
                            if (this.modelValue) {
                                for (const cssClass of cssClasses) {
                                    body.classList.add(cssClass);
                                }
                                this.centerOnScreen();
                            }
                            else {
                                for (const cssClass of cssClasses) {
                                    body.classList.remove(cssClass);
                                }
                            }
                        }
                    }
                },
                template: `
<div v-if="modelValue">
    <div @click="shake" class="modal-scrollable" style="z-index: 1060;">
        <div @click.stop ref="modalDiv" class="modal fade in" :class="{'animated shake': doShake}" tabindex="-1" role="dialog" style="display: block;">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div v-if="hasHeader" class="modal-header">
                        <button v-if="dismissible" @click="close" type="button" class="close" style="margin-top: -10px;">×</button>
                        <slot name="header" />
                    </div>
                    <div class="modal-body">
                        <button v-if="!hasHeader && dismissible" @click="close" type="button" class="close" style="margin-top: -10px;">×</button>
                        <slot />
                    </div>
                    <div v-if="$slots.footer" class="modal-footer">
                        <slot name="footer" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal-backdrop fade in" style="z-index: 1050;"></div>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=dialog.js.map