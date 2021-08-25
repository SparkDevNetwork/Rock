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
                AlertType["default"] = "default";
                AlertType["success"] = "success";
                AlertType["info"] = "info";
                AlertType["danger"] = "danger";
                AlertType["warning"] = "warning";
                AlertType["primary"] = "primary";
                AlertType["validation"] = "validation";
            })(AlertType || (AlertType = {}));
            exports_1("AlertType", AlertType);
            CountdownTimer = vue_1.defineComponent({
                name: 'CountdownTimer',
                props: {
                    modelValue: {
                        type: Number,
                        required: true
                    }
                },
                data: function () {
                    return {
                        handle: null
                    };
                },
                computed: {
                    timeString: function () {
                        var minutes = Math.floor(this.modelValue / 60);
                        var seconds = Math.floor(this.modelValue % 60);
                        return minutes + ":" + (seconds < 10 ? '0' + seconds : seconds);
                    },
                },
                methods: {
                    onInterval: function () {
                        if (this.modelValue <= 0) {
                            this.$emit('update:modelValue', 0);
                            return;
                        }
                        this.$emit('update:modelValue', Math.floor(this.modelValue - 1));
                    }
                },
                mounted: function () {
                    var _this = this;
                    if (this.handle) {
                        clearInterval(this.handle);
                    }
                    this.handle = setInterval(function () { return _this.onInterval(); }, 1000);
                },
                unmounted: function () {
                    if (this.handle) {
                        clearInterval(this.handle);
                        this.handle = null;
                    }
                },
                template: "\n<span>{{timeString}}</span>"
            });
            exports_1("default", CountdownTimer);
        }
    };
});
//# sourceMappingURL=CountdownTimer.js.map