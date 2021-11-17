System.register(["vue", "../Elements/loadingIndicator", "../Elements/textBox", "../Util/guid", "./gatewayControl"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var vue_1, loadingIndicator_1, textBox_1, guid_1, gatewayControl_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (loadingIndicator_1_1) {
                loadingIndicator_1 = loadingIndicator_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (gatewayControl_1_1) {
                gatewayControl_1 = gatewayControl_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "TestGatewayControl",
                components: {
                    LoadingIndicator: loadingIndicator_1.default,
                    TextBox: textBox_1.default
                },
                props: {
                    settings: {
                        type: Object,
                        required: true
                    },
                    submit: {
                        type: Boolean,
                        required: true
                    }
                },
                setup(props, { emit }) {
                    const cardNumber = vue_1.ref("");
                    const submit = () => __awaiter(this, void 0, void 0, function* () {
                        yield new Promise(resolve => setTimeout(resolve, 500));
                        if (cardNumber.value === "0000") {
                            emit("error", "This is a serious problem with the gateway.");
                            return;
                        }
                        if (cardNumber.value.length <= 10) {
                            emit("validation", {
                                "Card Number": "Card number is invalid."
                            });
                            return;
                        }
                        const token = guid_1.newGuid().replace(/-/g, "");
                        emit("success", token);
                    });
                    gatewayControl_1.onSubmitPayment(submit);
                    return {
                        cardNumber
                    };
                },
                template: `
<div style="max-width: 600px; margin-left: auto; margin-right: auto;">
    <TextBox label="Credit Card" v-model="cardNumber" />
</div>`
            }));
        }
    };
});
//# sourceMappingURL=testGatewayControl.js.map