System.register(["vue", "../Elements/loadingIndicator", "../Util/guid", "./gatewayControl"], function (exports_1, context_1) {
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
    var vue_1, loadingIndicator_1, guid_1, gatewayControl_1, standardStyling;
    var __moduleName = context_1 && context_1.id;
    function loadCollectJSAsync(tokenizationKey) {
        return __awaiter(this, void 0, void 0, function* () {
            if (window.CollectJS === undefined) {
                const script = document.createElement("script");
                script.type = "text/javascript";
                script.src = "https://secure.tnbcigateway.com/token/Collect.js";
                script.setAttribute("data-tokenization-key", tokenizationKey);
                script.setAttribute("data-variant", "inline");
                document.getElementsByTagName("head")[0].appendChild(script);
                try {
                    yield new Promise((resolve, reject) => {
                        script.addEventListener("load", () => resolve());
                        script.addEventListener("error", () => reject());
                    });
                }
                catch (_a) {
                    return false;
                }
            }
            return window.CollectJS !== undefined;
        });
    }
    function loadStandardStyleTagAsync() {
        return __awaiter(this, void 0, void 0, function* () {
            const style = document.createElement("style");
            style.type = "text/css";
            style.innerText = standardStyling;
            yield new Promise((resolve, reject) => {
                style.addEventListener("load", () => resolve());
                style.addEventListener("error", () => reject());
                document.getElementsByTagName("head")[0].appendChild(style);
            });
        });
    }
    function getCollectJSOptions(controlId, inputStyleHook, inputInvalidStyleHook) {
        const customCss = {
            "margin-bottom": "5px",
            "margin-top": "0"
        };
        if (inputStyleHook) {
            const inputStyles = getComputedStyle(inputStyleHook);
            customCss["color"] = inputStyles.color;
            customCss["border-bottom-color"] = inputStyles.borderBottomColor;
            customCss["border-bottom-left-radius"] = inputStyles.borderBottomLeftRadius;
            customCss["border-bottom-right-radius"] = inputStyles.borderBottomRightRadius;
            customCss["border-bottom-style"] = inputStyles.borderBottomStyle;
            customCss["border-bottom-width"] = inputStyles.borderBottomWidth;
            customCss["border-left-color"] = inputStyles.borderLeftColor;
            customCss["border-left-style"] = inputStyles.borderLeftStyle;
            customCss["border-left-width"] = inputStyles.borderLeftWidth;
            customCss["border-right-color"] = inputStyles.borderRightColor;
            customCss["border-right-style"] = inputStyles.borderRightStyle;
            customCss["border-right-width"] = inputStyles.borderRightWidth;
            customCss["border-top-color"] = inputStyles.borderTopColor;
            customCss["border-top-left-radius"] = inputStyles.borderTopLeftRadius;
            customCss["border-top-right-radius"] = inputStyles.borderTopRightRadius;
            customCss["border-top-style"] = inputStyles.borderTopStyle;
            customCss["border-top-width"] = inputStyles.borderTopWidth;
            customCss["border-width"] = inputStyles.borderWidth;
            customCss["border-style"] = inputStyles.borderStyle;
            customCss["border-radius"] = inputStyles.borderRadius;
            customCss["border-color"] = inputStyles.borderColor;
            customCss["background-color"] = inputStyles.backgroundColor;
            customCss["box-shadow"] = inputStyles.boxShadow;
            customCss["padding"] = inputStyles.padding;
            customCss["font-size"] = inputStyles.fontSize;
            customCss["height"] = inputStyles.height;
            customCss["font-family"] = inputStyles.fontFamily;
        }
        const focusCss = {
            "border-color": getComputedStyle(document.documentElement).getPropertyValue("--focus-state-border-color"),
            "outline-style": "none"
        };
        const invalidCss = {};
        if (inputInvalidStyleHook) {
            invalidCss["border-color"] = getComputedStyle(inputInvalidStyleHook).borderColor;
        }
        const placeholderCss = {
            "color": getComputedStyle(document.documentElement).getPropertyValue("--input-placeholder")
        };
        const options = {
            paymentSelector: `${controlId} .js-payment-button`,
            variant: "inline",
            fields: {
                ccnumber: {
                    selector: `#${controlId} .js-credit-card-input`,
                    title: "Card Number",
                    placeholder: "0000 0000 0000 0000"
                },
                ccexp: {
                    selector: `#${controlId} .js-credit-card-exp-input`,
                    title: "Card Expiration",
                    placeholder: "MM / YY"
                },
                cvv: {
                    display: "show",
                    selector: `#${controlId} .js-credit-card-cvv-input`,
                    title: "CVV Code",
                    placeholder: "CVV"
                },
                checkaccount: {
                    selector: `#${controlId} .js-check-account-number-input`,
                    title: "Account Number",
                    placeholder: "Account Number"
                },
                checkaba: {
                    selector: `#${controlId} .js-check-routing-number-input`,
                    title: "Routing Number",
                    placeholder: "Routing Number"
                },
                checkname: {
                    selector: `#${controlId} .js-check-fullname-input`,
                    title: "Name on Checking Account",
                    placeholder: "Name on Account"
                }
            },
            styleSniffer: false,
            customCss,
            focusCss,
            invalidCss,
            placeholderCss,
            timeoutDuration: 10000,
            callback: () => { }
        };
        return options;
    }
    function getFieldFriendlyName(field) {
        if (field === "ccnumber") {
            return "Card Number";
        }
        else if (field === "ccexp") {
            return "Expiration Date";
        }
        else if (field === "cvv") {
            return "CVV";
        }
        else if (field === "checkaccount") {
            return "Account Number";
        }
        else if (field === "checkaba") {
            return "Routing Number";
        }
        else if (field === "checkname") {
            return "Account Owner's Name";
        }
        else {
            return "Payment Information";
        }
    }
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (loadingIndicator_1_1) {
                loadingIndicator_1 = loadingIndicator_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (gatewayControl_1_1) {
                gatewayControl_1 = gatewayControl_1_1;
            }
        ],
        execute: function () {
            standardStyling = `
.nmi-payment-inputs .iframe-input {
  position: relative;
  -ms-flex: 0 0 100%;
  flex: 0 0 100%;
  max-width: 100%;
  height: 42px;
  height: calc(var(--input-height-base) + 5px);
  margin-bottom: 10px;
  padding: 0 3px;
  overflow: hidden;
}
.nmi-payment-inputs .iframe-input::before {
  position: absolute;
  top: 0;
  z-index: -1;
  width: calc(100% - 6px);
  height: 38px;
  height: var(--input-height-base);
  padding: 6px 12px;
  padding: var(--input-padding);
  margin: 0;
  content: " ";
  background: #f3f3f3;
  background: var(--input-bg-disabled);
  border: 1px solid #d8d8d8;
  border-color: var(--input-border);
  border-radius: var(--input-border-radius);
}
.nmi-payment-inputs .iframe-input .CollectJSInlineIframe {
  height: calc(var(--input-height-base) + 5px) !important;
  height: 42px !important;
}
.nmi-payment-inputs .break {
  -webkit-box-flex: 1;
  -ms-flex: 1 1 100%;
  flex: 1 1 100%;
}
.nmi-payment-inputs .gateway-payment-container {
  display: -ms-flexbox;
  display: flex;
  -ms-flex-wrap: wrap;
  flex-wrap: wrap;
  margin: 0 -3px;
  overflow-x: hidden;
}
.nmi-payment-inputs .credit-card-input {
  position: relative;
  -webkit-box-flex: 1;
  -ms-flex: 1 1 0;
  flex: 1 1 0;
  min-width: 200px;
}
.nmi-payment-inputs .check-account-number-input,
.nmi-payment-inputs .check-routing-number-input {
  -ms-flex: 0 0 100%;
  flex: 0 0 100%;
  max-width: 100%;
}
.nmi-payment-inputs .credit-card-exp-input,
.nmi-payment-inputs .credit-card-cvv-input {
  -ms-flex: 1 1 50%;
  flex: 1 1 50%;
  min-width: 50px;
}
@media (min-width: 500px) {
  .nmi-payment-inputs .break {
    -webkit-box-flex: 0;
    -ms-flex: 0 0 0%;
    flex: 0 0 0%;
  }
  .nmi-payment-inputs .check-account-number-input,
  .nmi-payment-inputs .check-routing-number-input {
    -ms-flex: 0 0 50%;
    flex: 0 0 50%;
    max-width: 50%;
  }
  .nmi-payment-inputs .credit-card-exp-input,
  .nmi-payment-inputs .credit-card-cvv-input {
    -webkit-box-flex: 0;
    -ms-flex: 0 0 auto;
    flex: 0 0 auto;
    max-width: 100px;
  }
}
`;
            exports_1("default", vue_1.defineComponent({
                name: "NMIGatewayControl",
                components: {
                    LoadingIndicator: loadingIndicator_1.default
                },
                props: {
                    settings: {
                        type: Object,
                        required: true
                    }
                },
                setup(props, { emit }) {
                    let hasAttemptedSubmit = false;
                    let hasReceivedToken = false;
                    const hasCreditCardPaymentType = vue_1.computed(() => {
                        var _a, _b;
                        return (_b = (_a = props.settings.enabledPaymentTypes) === null || _a === void 0 ? void 0 : _a.includes(0)) !== null && _b !== void 0 ? _b : false;
                    });
                    const hasBankAccountPaymentType = vue_1.computed(() => {
                        var _a, _b;
                        return (_b = (_a = props.settings.enabledPaymentTypes) === null || _a === void 0 ? void 0 : _a.includes(1)) !== null && _b !== void 0 ? _b : false;
                    });
                    const hasMultiplePaymentTypes = vue_1.computed(() => {
                        return hasCreditCardPaymentType.value && hasBankAccountPaymentType.value;
                    });
                    const activePaymentType = vue_1.ref(props.settings.enabledPaymentTypes != null && props.settings.enabledPaymentTypes.length > 0 ? props.settings.enabledPaymentTypes[0] : null);
                    const isCreditCardPaymentTypeActive = vue_1.computed(() => {
                        return activePaymentType.value === 0;
                    });
                    const isBankAccountPaymentTypeActive = vue_1.computed(() => {
                        return activePaymentType.value === 1;
                    });
                    const creditCardButtonClasses = vue_1.computed(() => {
                        return isCreditCardPaymentTypeActive.value
                            ? ["btn", "btn-primary", "active", "payment-creditcard"]
                            : ["btn", "btn-default", "payment-creditcard"];
                    });
                    const bankAccountButtonClasses = vue_1.computed(() => {
                        return isBankAccountPaymentTypeActive.value
                            ? ["btn", "btn-primary", "active", "payment-ach"]
                            : ["btn", "btn-default", "payment-ach"];
                    });
                    const loading = vue_1.ref(true);
                    const failedToLoad = vue_1.ref(false);
                    const validationMessage = vue_1.ref("");
                    const activateCreditCard = () => {
                        CollectJS.clearInputs();
                        activePaymentType.value = 0;
                    };
                    const activateBankAccount = () => {
                        CollectJS.clearInputs();
                        activePaymentType.value = 1;
                    };
                    const tokenResponseSent = vue_1.ref(false);
                    const controlId = `nmi_${guid_1.newGuid()}`;
                    const inputStyleHook = vue_1.ref(null);
                    const inputInvalidStyleHook = vue_1.ref(null);
                    const validationFieldStatus = {
                        ccnumber: { field: getFieldFriendlyName("ccnumber"), status: false, message: "is required" },
                        ccexp: { field: getFieldFriendlyName("ccexp"), status: false, message: "is required" },
                        cvv: { field: getFieldFriendlyName("cvv"), status: false, message: "is required" },
                        checkaccount: { field: getFieldFriendlyName("checkaccount"), status: false, message: "is required" },
                        checkaba: { field: getFieldFriendlyName("checkaba"), status: false, message: "is required" },
                        checkname: { field: getFieldFriendlyName("checkname"), status: false, message: "is required" }
                    };
                    const validateInputs = function () {
                        var _a, _b, _c, _d;
                        let hasValidationError = false;
                        const errors = {};
                        for (const validationFieldKey in validationFieldStatus) {
                            const validationField = validationFieldStatus[validationFieldKey];
                            const inputField = document.querySelector((_b = (_a = CollectJS.config.fields[validationFieldKey]) === null || _a === void 0 ? void 0 : _a.selector) !== null && _b !== void 0 ? _b : "");
                            const fieldVisible = ((_c = inputField === null || inputField === void 0 ? void 0 : inputField.offsetWidth) !== null && _c !== void 0 ? _c : 0) !== 0 || ((_d = inputField === null || inputField === void 0 ? void 0 : inputField.offsetHeight) !== null && _d !== void 0 ? _d : 0) !== 0;
                            if (fieldVisible && !validationField.status) {
                                hasValidationError = true;
                                const validationFieldTitle = getFieldFriendlyName(validationFieldKey);
                                errors[validationFieldTitle] = validationField.message || "unknown validation error";
                            }
                        }
                        return {
                            isValid: !hasValidationError,
                            errors
                        };
                    };
                    const timeoutCallback = () => {
                        if (tokenResponseSent.value) {
                            return;
                        }
                        console.log("The tokenization didn't respond in the expected timeframe. This could be due to an invalid or incomplete field or poor connectivity - " + Date());
                        const validationResult = validateInputs();
                        if (!validationResult.isValid) {
                            emit("validation", validationResult.errors);
                        }
                        else {
                            console.log("Timeout happened for unknown reason, probably poor connectivity since we already validated inputs.");
                            emit("validation", {
                                "Payment Timeout": "Response from gateway timed out. This could be do to poor connectivity or invalid payment values."
                            });
                        }
                    };
                    const validationCallback = (field, validated, message) => {
                        if (message === "Field is empty") {
                            message = "is required";
                        }
                        validationFieldStatus[field] = {
                            field: field,
                            status: validated,
                            message: message
                        };
                        const validationResult = validateInputs();
                        if (hasAttemptedSubmit && !CollectJS.inSubmission && !hasReceivedToken) {
                            emit("validation", validationResult.errors);
                        }
                    };
                    gatewayControl_1.onSubmitPayment(() => {
                        if (loading.value || failedToLoad.value) {
                            return;
                        }
                        tokenResponseSent.value = false;
                        setTimeout(() => {
                            const validationResult = validateInputs();
                            hasAttemptedSubmit = true;
                            if (validationResult.isValid) {
                                CollectJS.startPaymentRequest();
                            }
                            else {
                                emit("validation", validationResult.errors);
                            }
                        }, 0);
                    });
                    const handleTokenResponse = (tokenResponse) => {
                        hasReceivedToken = true;
                        emit("success", tokenResponse.token);
                    };
                    vue_1.onMounted(() => __awaiter(this, void 0, void 0, function* () {
                        var _a;
                        yield loadStandardStyleTagAsync();
                        if (!(yield loadCollectJSAsync((_a = props.settings.tokenizationKey) !== null && _a !== void 0 ? _a : ""))) {
                            emit("error", "Error configuring hosted gateway. This could be due to an invalid or missing Tokenization Key. Please verify that Tokenization Key is configured correctly in gateway settings.");
                            return;
                        }
                        try {
                            const options = getCollectJSOptions(controlId, inputStyleHook.value, inputInvalidStyleHook.value);
                            options.timeoutCallback = timeoutCallback;
                            options.validationCallback = validationCallback;
                            options.callback = handleTokenResponse;
                            options.fieldsAvailableCallback = () => {
                                loading.value = false;
                            };
                            CollectJS.configure(options);
                        }
                        catch (_b) {
                            failedToLoad.value = true;
                            emit("error", "Error configuring hosted gateway. This could be due to an invalid or missing Tokenization Key. Please verify that Tokenization Key is configured correctly in gateway settings.");
                            return;
                        }
                    }));
                    return {
                        controlId,
                        loading,
                        failedToLoad,
                        hasMultiplePaymentTypes,
                        hasCreditCardPaymentType,
                        hasBankAccountPaymentType,
                        isCreditCardPaymentTypeActive,
                        isBankAccountPaymentTypeActive,
                        creditCardButtonClasses,
                        bankAccountButtonClasses,
                        validationMessage,
                        activateCreditCard,
                        activateBankAccount,
                        inputStyleHook,
                        inputInvalidStyleHook
                    };
                },
                template: `
<div>
    <div v-if="loading" class="text-center">
        <LoadingIndicator />
    </div>

    <div v-show="!loading && !failedToLoad" style="max-width: 600px; margin-left: auto; margin-right: auto;">
        <div v-if="hasMultiplePaymentTypes" class="gateway-type-selector btn-group btn-group-justified" role="group">
            <a :class="creditCardButtonClasses" @click.prevent="activateCreditCard">Card</a>
            <a :class="bankAccountButtonClasses" @click.prevent="activateBankAccount">Bank Account</a>
        </div>

        <div :id="controlId" class="nmi-payment-inputs">
            <div v-if="hasCreditCardPaymentType" v-show="isCreditCardPaymentTypeActive" class="gateway-creditcard-container gateway-payment-container">
                <div class="iframe-input credit-card-input js-credit-card-input"></div>
                <div class="break"></div>
                <div class="iframe-input credit-card-exp-input js-credit-card-exp-input"></div>
                <div class="iframe-input credit-card-cvv-input js-credit-card-cvv-input"></div>
            </div>

            <div v-if="hasBankAccountPaymentType" v-show="isBankAccountPaymentTypeActive" class="gateway-ach-container gateway-payment-container">
                <div class="iframe-input check-account-number-input js-check-account-number-input"></div>
                <div class="iframe-input check-routing-number-input js-check-routing-number-input"></div>
                <div class="iframe-input check-fullname-input js-check-fullname-input"></div>
            </div>

            <button type="button" style="display: none;" class="payment-button js-payment-button"></button>
        </div>

        <div v-show="validationMessage" v-text="validationMessage" class="alert alert-validation">
        </div>
    </div>

    <input ref="inputStyleHook" class="form-control nmi-input-style-hook form-group" style="display: none;">

    <div class="form-group has-error" style="display: none;">
        <input ref="inputInvalidStyleHook" type="text" class="form-control">
    </div>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=nmiGatewayControl.js.map