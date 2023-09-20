System.register(["vue", "/Obsidian/Elements/loadingIndicator", "/Obsidian/Services/number", "/Obsidian/Controls/gatewayControl", "/Obsidian/Store/index"], function (exports_1, context_1) {
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
    var vue_1, loadingIndicator_1, number_1, gatewayControl_1, index_1, store, standardStyling;
    var __moduleName = context_1 && context_1.id;
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
    function loadStripeJSAsync() {
        return __awaiter(this, void 0, void 0, function* () {
            const filePath = "https://js.stripe.com/v3/";
            var documentScripts = document.getElementsByTagName("script");
            var doesScriptExist = false;
            for (var i = 0; i < documentScripts.length; i++) {
                if (documentScripts[i].src == filePath) {
                    doesScriptExist = true;
                }
            }
            if (!doesScriptExist) {
                const script = document.createElement("script");
                script.type = "text/javascript";
                script.src = filePath;
                document.getElementsByTagName("head")[0].appendChild(script);
                try {
                    yield new Promise((resolve, reject) => {
                        script.addEventListener("load", () => resolve());
                        script.addEventListener("error", () => reject());
                    });
                    doesScriptExist = true;
                }
                catch (_a) {
                    doesScriptExist = false;
                }
            }
            return doesScriptExist;
        });
    }
    function loadPaymentButtonAsync() {
        return __awaiter(this, void 0, void 0, function* () {
            const filePath = "/Plugins/com_simpledonation/js/payment-request-button-obsidian.js";
            var documentScripts = document.getElementsByTagName("script");
            var doesScriptExist = false;
            for (var i = 0; i < documentScripts.length; i++) {
                if (documentScripts[i].src == filePath) {
                    doesScriptExist = true;
                }
            }
            if (!doesScriptExist) {
                const script = document.createElement("script");
                script.type = "text/javascript";
                script.src = filePath;
                document.getElementsByTagName("head")[0].appendChild(script);
                try {
                    yield new Promise((resolve, reject) => {
                        script.addEventListener("load", () => resolve());
                        script.addEventListener("error", () => reject());
                    });
                    doesScriptExist = true;
                }
                catch (_a) {
                    doesScriptExist = false;
                }
            }
            return doesScriptExist;
        });
    }
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (loadingIndicator_1_1) {
                loadingIndicator_1 = loadingIndicator_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (gatewayControl_1_1) {
                gatewayControl_1 = gatewayControl_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            standardStyling = `
    .simpledonation-payment-inputs .simpledonation-input {
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
    .simpledonation-payment-inputs .simpledonation-input::before {
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

    .simpledonation-payment-inputs .break {
        -webkit-box-flex: 1;
        -ms-flex: 1 1 100%;
        flex: 1 1 100%;
    }
    .simpledonation-payment-inputs .gateway-payment-container {
        display: -ms-flexbox;
        display: flex;
        -ms-flex-wrap: wrap;
        flex-wrap: wrap;
        margin: 0 -3px;
        overflow-x: hidden;
    }
    .simpledonation-payment-inputs .gateway-payment-container .rock-text-box {
        padding-right: 15px;
    }
    .simpledonation-payment-inputs .credit-card-input {
        position: relative;
        -webkit-box-flex: 1;
        -ms-flex: 1 1 0;
        flex: 1 1 0;
        min-width: 200px;
    }
    .simpledonation-payment-inputs .check-account-number-input,
    .simpledonation-payment-inputs .check-routing-number-input {
        -ms-flex: 0 0 100%;
        flex: 0 0 100%;
        max-width: 100%;
    }
    .simpledonation-payment-inputs .credit-card-exp-input,
    .simpledonation-payment-inputs .credit-card-cvv-input {
        -ms-flex: 1 1 50%;
        flex: 1 1 50%;
        min-width: 50px;
    }

    .btn-payment-request {
        background-color: black;
        background-size: 100% 100%;
        background-origin: content-box;
        background-repeat: no-repeat;
        width: 100%;
        height: 44px;
        padding: 10px 0;
        border-radius: 5px;
        margin: 20px 0;
    }

    .btn-apple-pay {
        background-image: -webkit-named-image(apple-pay-logo-white);
    }

    .btn-google-pay {
        background-image: url("/Plugins/com_simpledonation/Assets/googlepay.png");
        background-position: center center;
        background-size: auto 51px;
    }

    .strike {
        display: block;
        text-align: center;
        overflow: hidden;
        white-space: nowrap;
        margin-bottom:10px;
    }

    .strike > span {
        position: relative;
        display: inline-block;
    }

    .strike > span:before,
    .strike > span:after {
        content: "";
        position: absolute;
        top: 50%;
        width: 9999px;
        height: 1px;
        background: grey;
    }

    .strike > span:before {
        right: 100%;
        margin-right: 15px;
    }

    .strike > span:after {
        left: 100%;
        margin-left: 15px;
    }
`;
            exports_1("default", vue_1.defineComponent({
                name: "SimpleDonationGatewayControl",
                components: {
                    LoadingIndicator: loadingIndicator_1.default
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
                    vue_1.watch(cardNumber, () => {
                        var formattedString = formatNumber(cardNumber.value, 19, '');
                        if (cardNumber.value != formattedString) {
                            cardNumber.value = formattedString;
                        }
                    });
                    const expirationDate = vue_1.ref("");
                    vue_1.watch(expirationDate, () => {
                        var formattedString = formatNumber(expirationDate.value, 5, '/');
                        if (expirationDate.value != formattedString) {
                            expirationDate.value = formattedString;
                        }
                    });
                    const cvc = vue_1.ref("");
                    vue_1.watch(cvc, () => {
                        var formattedString = formatNumber(cvc.value, 4, '');
                        if (cvc.value != formattedString) {
                            cvc.value = formattedString;
                        }
                    });
                    const accountHolderName = vue_1.ref("");
                    const routingNumber = vue_1.ref("");
                    vue_1.watch(routingNumber, () => {
                        var formattedString = formatNumber(routingNumber.value, 9, '');
                        if (routingNumber.value != formattedString) {
                            routingNumber.value = formattedString;
                        }
                    });
                    const accountNumber = vue_1.ref("");
                    vue_1.watch(accountNumber, () => {
                        var formattedString = formatNumber(accountNumber.value, -1, '');
                        if (accountNumber.value != formattedString) {
                            accountNumber.value = formattedString;
                        }
                    });
                    const activePaymentType = vue_1.ref(0);
                    const clearInputs = () => {
                        clearCreditCardInputs();
                        clearAchInputs();
                    };
                    const clearCreditCardInputs = () => {
                        cardNumber.value = "";
                        expirationDate.value = "";
                        cvc.value = "";
                    };
                    const clearAchInputs = () => {
                        accountHolderName.value = "";
                        routingNumber.value = "";
                        accountNumber.value = "";
                    };
                    const publicStripeKey = vue_1.computed(() => {
                        return props.settings.stripeKey ? props.settings.stripeKey : 'pk_test_TYooMQauvdEDq54NiTphI7jx';
                    });
                    const organizationName = vue_1.computed(() => {
                        return props.settings.organizationName ? props.settings.organizationName : 'pk_test_TYooMQauvdEDq54NiTphI7jx';
                    });
                    const stripeToken = vue_1.computed(() => {
                        return getHiddenInputValue("hfStripeToken");
                    });
                    const walletName = vue_1.computed(() => {
                        return getHiddenInputValue("hfWalletName");
                    });
                    const postbackFromModal = vue_1.computed(() => {
                        return getHiddenInputValue("hfPostbackFromModal");
                    });
                    const getHiddenInputValue = (elementId) => {
                        var value = '';
                        var element = document.getElementById(elementId);
                        if (element != null) {
                            value = element.value;
                        }
                        return value;
                    };
                    const isCreditCardPaymentTypeActive = vue_1.computed(() => {
                        return activePaymentType.value === 0;
                    });
                    const registrationEntryState = vue_1.inject("registrationEntryState");
                    const amountToPay = vue_1.computed(() => {
                        return registrationEntryState.amountToPayToday;
                    });
                    const currentPerson = vue_1.computed(() => {
                        return store.state.currentPerson;
                    });
                    const expirationArray = vue_1.computed(() => {
                        return expirationDate.value.split("/");
                    });
                    const expirationMonth = vue_1.computed(() => {
                        let month = null;
                        if (expirationArray.value.length === 2) {
                            month = number_1.toNumberOrNull(expirationArray.value[0]);
                        }
                        return month;
                    });
                    const expirationYear = vue_1.computed(() => {
                        let year = null;
                        if (expirationArray.value.length === 2) {
                            year = number_1.toNumberOrNull(expirationArray.value[1]);
                        }
                        return year;
                    });
                    const isBankAccountPaymentTypeActive = vue_1.computed(() => {
                        return activePaymentType.value === 1;
                    });
                    const currencyTypeGuid = vue_1.computed(() => {
                        if (walletName.value === "applePay") {
                            return "D42C4DF7-1AE9-4DDE-ADA2-774B866B798C";
                        }
                        else if (walletName.value === "googlePay") {
                            return "6151F6E0-3223-46BA-A59E-E091BE4AF75C";
                        }
                        else if (isCreditCardPaymentTypeActive.value) {
                            return "928A2E04-C77B-4282-888F-EC549CEE026A";
                        }
                        else if (isBankAccountPaymentTypeActive.value) {
                            return "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6";
                        }
                        else {
                            return "56C9AE9C-B5EB-46D5-9650-2EF86B14F856";
                        }
                    });
                    const creditCardButtonClasses = vue_1.computed(() => {
                        return isCreditCardPaymentTypeActive.value
                            ? ["btn", "btn-default", "active", "payment-creditcard"]
                            : ["btn", "btn-default", "payment-creditcard"];
                    });
                    const bankAccountButtonClasses = vue_1.computed(() => {
                        return isBankAccountPaymentTypeActive.value
                            ? ["btn", "btn-default", "active", "payment-ach"]
                            : ["btn", "btn-default", "payment-ach"];
                    });
                    const activateCreditCard = () => {
                        clearInputs();
                        activePaymentType.value = 0;
                    };
                    const activateBankAccount = () => {
                        clearInputs();
                        activePaymentType.value = 1;
                        if (currentPerson.value != null) {
                            accountHolderName.value = currentPerson.value.fullName || "";
                        }
                    };
                    const formatNumber = (numberString, maxLength, additionalCharacters) => {
                        var allowedString = "[^0-9";
                        var additionalCharacterArray = additionalCharacters.split(',').filter(ac => ac != '');
                        if (additionalCharacterArray.length > 0) {
                            additionalCharacterArray.forEach(ac => allowedString = allowedString + '\\' + ac);
                        }
                        allowedString = allowedString + "]";
                        var allowedRegex = new RegExp(allowedString, 'g');
                        var formattedNumber = numberString.replace(allowedRegex, '');
                        if (formattedNumber.length > maxLength && maxLength > 0) {
                            formattedNumber = formattedNumber.substring(0, maxLength);
                        }
                        return formattedNumber;
                    };
                    const validateInputs = function () {
                        if (isCreditCardPaymentTypeActive.value) {
                            return validateCreditCardInputs();
                        }
                        else if (isBankAccountPaymentTypeActive.value) {
                            return validateACHInputs();
                        }
                        else {
                            let hasValidationError = false;
                            const errors = {};
                            return {
                                isValid: !hasValidationError,
                                errors
                            };
                        }
                    };
                    const validateCreditCardInputs = function () {
                        let hasValidationError = false;
                        const errors = {};
                        let isValidCardNumber = cardNumber.value.length >= 13 && cardNumber.value.length <= 16 && number_1.toNumberOrNull(cardNumber.value) != null;
                        let isValidExpirationDate = expirationMonth.value != null && expirationMonth.value <= 12 && expirationYear.value != null;
                        let isValidCvc = cvc.value.length > 0 && number_1.toNumberOrNull(cvc.value) != null;
                        if (!isValidCardNumber) {
                            hasValidationError = true;
                            errors["Card Number"] = "Card Number is invalid.";
                        }
                        if (!isValidExpirationDate) {
                            hasValidationError = true;
                            errors["Expiration Date"] = "Please enter a valid expiration date.";
                        }
                        if (!isValidCvc) {
                            hasValidationError = true;
                            errors["CVC"] = "CVC is invalid.";
                        }
                        return {
                            isValid: !hasValidationError,
                            errors
                        };
                    };
                    const validateACHInputs = function () {
                        let hasValidationError = false;
                        const errors = {};
                        let isValidHolderName = accountNumber.value.length > 0;
                        let isValidAccountNumber = accountNumber.value.length > 0 && number_1.toNumberOrNull(accountNumber.value) != null;
                        let isValidRoutingNumber = routingNumber.value.length === 9 && number_1.toNumberOrNull(routingNumber.value) != null;
                        if (!isValidHolderName) {
                            hasValidationError = true;
                            errors["Account Holder Name"] = "Account number is invalid.";
                        }
                        if (!isValidAccountNumber) {
                            hasValidationError = true;
                            errors["Account Number"] = "Account number is invalid.";
                        }
                        if (!isValidRoutingNumber) {
                            hasValidationError = true;
                            errors["Routing Number"] = "Routing number is invalid.";
                        }
                        return {
                            isValid: !hasValidationError,
                            errors
                        };
                    };
                    const getCreditCardParams = function () {
                        var _a, _b, _c, _d;
                        let urlencoded = new URLSearchParams();
                        urlencoded.append("card[number]", cardNumber.value);
                        urlencoded.append("card[exp_month]", (_b = (_a = expirationMonth.value) === null || _a === void 0 ? void 0 : _a.toString()) !== null && _b !== void 0 ? _b : "");
                        urlencoded.append("card[exp_year]", (_d = (_c = expirationYear.value) === null || _c === void 0 ? void 0 : _c.toString()) !== null && _d !== void 0 ? _d : "");
                        urlencoded.append("card[cvc]", cvc.value);
                        return urlencoded;
                    };
                    const getAchParams = function () {
                        let urlencoded = new URLSearchParams();
                        urlencoded.append("bank_account[account_number]", accountNumber.value);
                        urlencoded.append("bank_account[country]", props.settings.defaultCountryCode ? props.settings.defaultCountryCode : 'US');
                        urlencoded.append("bank_account[account_holder_name]", accountHolderName.value);
                        urlencoded.append("bank_account[account_holder_type]", "individual");
                        urlencoded.append("bank_account[currency]", props.settings.currency ? props.settings.currency : 'usd');
                        urlencoded.append("bank_account[routing_number]", routingNumber.value);
                        return urlencoded;
                    };
                    const submit = () => __awaiter(this, void 0, void 0, function* () {
                        if (postbackFromModal.value === 'PaymentButtonClicked') {
                            if (stripeToken.value != '') {
                                let obsidianToken = currencyTypeGuid.value + '|' + stripeToken.value;
                                emit("success", obsidianToken);
                            }
                            else {
                                emit("error", "There was an error saving your payment token");
                            }
                        }
                        else {
                            const validationResult = validateInputs();
                            if (validationResult.isValid) {
                                let myHeaders = new Headers();
                                myHeaders.append("Content-Type", "application/x-www-form-urlencoded");
                                myHeaders.append("Authorization", "Bearer " + publicStripeKey.value);
                                let urlencoded = new URLSearchParams();
                                if (isCreditCardPaymentTypeActive.value) {
                                    urlencoded = getCreditCardParams();
                                }
                                else if (isBankAccountPaymentTypeActive.value) {
                                    urlencoded = getAchParams();
                                }
                                let response = yield fetch("https://api.stripe.com/v1/tokens", {
                                    method: 'POST',
                                    headers: myHeaders,
                                    body: urlencoded
                                })
                                    .then(response => response.json())
                                    .then(data => {
                                        let obsidianToken = currencyTypeGuid.value + '|' + data.id;
                                        emit("success", obsidianToken);
                                    })
                                    .catch(error => emit("error", error.message));
                            }
                            else {
                                emit("validation", validationResult.errors);
                            }
                        }
                    });
                    gatewayControl_1.onSubmitPayment(submit);
                    vue_1.onMounted(() => __awaiter(this, void 0, void 0, function* () {
                        yield loadStandardStyleTagAsync();
                        yield loadStripeJSAsync();
                        yield loadPaymentButtonAsync();
                    }));
                    return {
                        publicStripeKey,
                        organizationName,
                        amountToPay,
                        stripeToken,
                        walletName,
                        postbackFromModal,
                        cardNumber,
                        expirationDate,
                        cvc,
                        accountHolderName,
                        routingNumber,
                        accountNumber,
                        isCreditCardPaymentTypeActive,
                        isBankAccountPaymentTypeActive,
                        creditCardButtonClasses,
                        bankAccountButtonClasses,
                        activateCreditCard,
                        activateBankAccount,
                    };
                },
                template: `
<div style="max-width: 600px; margin-left: auto; margin-right: auto;">
    <input type='hidden' id='hfPublicKey' :value="publicStripeKey">
    <input type='hidden' id='hfOrganizationName' :value="organizationName">
    <input type='hidden' id='hfTotalCost' :value="amountToPay">
    <input type='hidden' id='hfStripeToken'>
    <input type='hidden' id='hfWalletName'>
    <input type='hidden' id='hfPostbackFromModal' value='PaymentButtonReady'>

    <div id="payment-request-button">
        <button type="button" class="btn btn-payment-request" CausesValidation="true"></button>
        <div class="strike" id="divStrike" runat="server">
            <span>or</span>
        </div>
    </div>

    <div class="gateway-type-selector btn-group btn-group-xs" role="group">
        <a :class="creditCardButtonClasses" @click.prevent="activateCreditCard">Card</a>
        <a :class="bankAccountButtonClasses" @click.prevent="activateBankAccount">Bank Account</a>
    </div>
    <div class="simpledonation-payment-inputs">
        <div v-show="isCreditCardPaymentTypeActive" class="gateway-creditcard-container">
            <div class="form-group rock-text-box">
                <label class="control-label">Card Number</label>
                <div class="control-wrapper">
                    <input class="form-control simpledonation-input credit-card-input js-credit-card-input" v-model="cardNumber" />
                </div>
            </div>
            <div class="break"></div>
            <div class="gateway-payment-container">
                <div class="form-group rock-text-box">
                    <label class="control-label">Expiration Date</label>
                    <div class="control-wrapper">
                        <input class="form-control simpledonation-input credit-card-exp-input js-credit-card-exp-input" placeholder="mm/yy" v-model="expirationDate" />
                    </div>
                </div>
                <div class="form-group rock-text-box">
                    <label class="control-label">CVC</label>
                    <div class="control-wrapper">
                        <input class="form-control simpledonation-input credit-card-cvv-input js-credit-card-cvv-input" v-model="cvc" />
                    </div>
                </div>
            </div>
        </div>

        <div v-show="isBankAccountPaymentTypeActive" class="gateway-ach-container">
            <div class="form-group rock-text-box">
                <label class="control-label">Account Holder Name</label>
                <div class="control-wrapper">
                    <input class="form-control simpledonation-input check-fullname-input js-check-fullname-input" v-model="accountHolderName" />
                </div>
            </div>
            <div class="form-group rock-text-box">
                <label class="control-label">Routing Number</label>
                <div class="control-wrapper"><input
                        class="form-control simpledonation-input check-routing-number-input js-check-routing-number-input" v-model="routingNumber" /> </div>
            </div>
            <div class="form-group rock-text-box">
                <label class="control-label">Account Number</label>
                <div class="control-wrapper"><input
                        class="form-control simpledonation-input check-account-number-input js-check-account-number-input" v-model="accountNumber" /> </div>
            </div>
        </div>

    </div>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=simpleDonationGatewayControl.js.map
