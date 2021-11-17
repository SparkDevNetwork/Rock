System.register(["vue", "../Elements/javaScriptAnchor", "./componentFromUrl"], function (exports_1, context_1) {
    "use strict";
    var vue_1, javaScriptAnchor_1, componentFromUrl_1, submitPaymentCallbackSymbol, prepareSubmitPayment, onSubmitPayment, ValidationField;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (javaScriptAnchor_1_1) {
                javaScriptAnchor_1 = javaScriptAnchor_1_1;
            },
            function (componentFromUrl_1_1) {
                componentFromUrl_1 = componentFromUrl_1_1;
            }
        ],
        execute: function () {
            submitPaymentCallbackSymbol = Symbol("gateway-submit-payment-callback");
            exports_1("prepareSubmitPayment", prepareSubmitPayment = () => {
                const container = {};
                vue_1.provide(submitPaymentCallbackSymbol, container);
                return () => {
                    if (container.callback) {
                        container.callback();
                    }
                    else {
                        throw "Submit payment callback has not been defined.";
                    }
                };
            });
            exports_1("onSubmitPayment", onSubmitPayment = (callback) => {
                const container = vue_1.inject(submitPaymentCallbackSymbol);
                if (!container || container.callback) {
                    throw "Submit payment callback already defined.";
                }
                container.callback = callback;
            });
            (function (ValidationField) {
                ValidationField[ValidationField["CardNumber"] = 0] = "CardNumber";
                ValidationField[ValidationField["Expiry"] = 1] = "Expiry";
                ValidationField[ValidationField["SecurityCode"] = 2] = "SecurityCode";
            })(ValidationField || (ValidationField = {}));
            exports_1("ValidationField", ValidationField);
            exports_1("default", vue_1.defineComponent({
                name: "GatewayControl",
                components: {
                    ComponentFromUrl: componentFromUrl_1.default,
                    JavaScriptAnchor: javaScriptAnchor_1.default
                },
                props: {
                    gatewayControlModel: {
                        type: Object,
                        required: true
                    }
                },
                setup(props, { emit }) {
                    const url = vue_1.computed(() => props.gatewayControlModel.fileUrl);
                    const settings = vue_1.computed(() => props.gatewayControlModel.settings);
                    const onSuccess = (token) => {
                        emit("success", token);
                    };
                    const onValidation = (validationErrors) => {
                        emit("validation", validationErrors);
                    };
                    const onError = (message) => {
                        emit("error", message);
                    };
                    return {
                        url,
                        settings,
                        onSuccess,
                        onValidation,
                        onError
                    };
                },
                methods: {},
                template: `
<div>
    <ComponentFromUrl :url="url" :settings="settings" @validation="onValidation" @success="onSuccess" @error="onError" />
</div>
`
            }));
        }
    };
});
//# sourceMappingURL=gatewayControl.js.map