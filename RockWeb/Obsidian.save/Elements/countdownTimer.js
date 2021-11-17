System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1, AlertType, CountdownTimer;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            (function (AlertType) {
                AlertType["Default"] = "default";
                AlertType["Success"] = "success";
                AlertType["Info"] = "info";
                AlertType["Danger"] = "danger";
                AlertType["Warning"] = "warning";
                AlertType["Primary"] = "primary";
                AlertType["Validation"] = "validation";
            })(AlertType || (AlertType = {}));
            exports_1("AlertType", AlertType);
            CountdownTimer = vue_1.defineComponent({
                name: "CountdownTimer",
                props: {
                    modelValue: {
                        type: Number,
                        required: true
                    }
                },
                data() {
                    return {
                        handle: null
                    };
                },
                computed: {
                    timeString() {
                        const minutes = Math.floor(this.modelValue / 60);
                        const seconds = Math.floor(this.modelValue % 60);
                        return `${minutes}:${seconds < 10 ? "0" + seconds : seconds}`;
                    },
                },
                methods: {
                    onInterval() {
                        if (this.modelValue <= 0) {
                            this.$emit("update:modelValue", 0);
                            return;
                        }
                        this.$emit("update:modelValue", Math.floor(this.modelValue - 1));
                    }
                },
                mounted() {
                    if (this.handle) {
                        clearInterval(this.handle);
                    }
                    this.handle = setInterval(() => this.onInterval(), 1000);
                },
                unmounted() {
                    if (this.handle) {
                        clearInterval(this.handle);
                        this.handle = null;
                    }
                },
                template: `
<span>{{timeString}}</span>`
            });
            exports_1("default", CountdownTimer);
        }
    };
});
//# sourceMappingURL=countdownTimer.js.map