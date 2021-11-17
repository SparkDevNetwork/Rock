System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1, AlertType, Alert;
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
            Alert = vue_1.defineComponent({
                name: "Alert",
                props: {
                    dismissible: {
                        type: Boolean,
                        default: false
                    },
                    alertType: {
                        type: String,
                        default: AlertType.Default
                    }
                },
                emits: [
                    "dismiss"
                ],
                methods: {
                    onDismiss: function () {
                        this.$emit("dismiss");
                    }
                },
                computed: {
                    typeClass() {
                        return `alert-${this.alertType}`;
                    },
                },
                template: `
<div class="alert" :class="typeClass">
    <button v-if="dismissible" type="button" class="close" @click="onDismiss">
        <span>&times;</span>
    </button>
    <slot />
</div>`
            });
            exports_1("default", Alert);
        }
    };
});
//# sourceMappingURL=alert.js.map