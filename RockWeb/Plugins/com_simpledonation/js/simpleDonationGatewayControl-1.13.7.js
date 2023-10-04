System.register(["vue", "/Obsidian/Elements/loadingIndicator", "/Obsidian/Services/number", "/Obsidian/Controls/gatewayControl"], function (exports_1, context_1) {
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
    var vue_1, loadingIndicator_1, number_1, gatewayControl_1, standardStyling;
    var __moduleName = context_1 && context_1.id;
    function loadStandardStyleTagAsync() {
        return __awaiter(this, void 0, void 0, function* () {
            const style = document.createElement('style');
            style.type = 'text/css';
            style.innerText = standardStyling;
            yield new Promise((resolve, reject) => {
                style.addEventListener('load', () => resolve());
                style.addEventListener('error', () => reject());
                document.getElementsByTagName('head')[0].appendChild(style);
            });
        });
    }
    function loadStripeJSAsync() {
        return __awaiter(this, void 0, void 0, function* () {
            const filePath = 'https://js.stripe.com/v3/';
            const documentScripts = document.getElementsByTagName('script');
            let doesScriptExist = false;
            for (let i = 0; i < documentScripts.length; i++) {
                if (documentScripts[i].src === filePath) {
                    doesScriptExist = true;
                }
            }
            if (!doesScriptExist) {
                const script = document.createElement('script');
                script.type = 'text/javascript';
                script.src = filePath;
                document.getElementsByTagName('head')[0].appendChild(script);
                try {
                    yield new Promise((resolve, reject) => {
                        script.addEventListener('load', () => resolve());
                        script.addEventListener('error', () => reject());
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
            const filePath = '/Plugins/com_simpledonation/js/payment-request-button-obsidian.js';
            const documentScripts = document.getElementsByTagName('script');
            let doesScriptExist = false;
            for (let i = 0; i < documentScripts.length; i++) {
                if (documentScripts[i].src === filePath) {
                    doesScriptExist = true;
                }
            }
            if (!doesScriptExist) {
                const script = document.createElement('script');
                script.type = 'text/javascript';
                script.src = filePath;
                document.getElementsByTagName('head')[0].appendChild(script);
                try {
                    yield new Promise((resolve, reject) => {
                        script.addEventListener('load', () => resolve());
                        script.addEventListener('error', () => reject());
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
            }
        ],
        execute: function () {
            standardStyling = `.simpledonation-payment-inputs .simpledonation-input {
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
                name: 'SimpleDonationGatewayControl',
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
                    const cardNumber = vue_1.ref('');
                    const expirationDate = vue_1.ref('');
                    const cvc = vue_1.ref('');
                    vue_1.watch(cvc, () => {
                        const formattedString = formatNumber(cvc.value, 4, '');
                        if (cvc.value !== formattedString) {
                            cvc.value = formattedString;
                        }
                    });
                    const routingNumber = vue_1.ref('');
                    vue_1.watch(routingNumber, () => {
                        const formattedString = formatNumber(routingNumber.value, 9, '');
                        if (routingNumber.value !== formattedString) {
                            routingNumber.value = formattedString;
                        }
                    });
                    const accountNumber = vue_1.ref('');
                    vue_1.watch(accountNumber, () => {
                        const formattedString = formatNumber(accountNumber.value, -1, '');
                        if (accountNumber.value !== formattedString) {
                            accountNumber.value = formattedString;
                        }
                    });
                    const activePaymentType = vue_1.ref(0);
                    const clearInputs = () => {
                        clearCreditCardInputs();
                        clearAchInputs();
                    };
                    const clearCreditCardInputs = () => {
                        cardNumber.value = '';
                        expirationDate.value = '';
                        cvc.value = '';
                    };
                    const clearAchInputs = () => {
                        routingNumber.value = '';
                        accountNumber.value = '';
                    };
                    const publicStripeKey = vue_1.computed(() => {
                        return props.settings.stripeKey ? props.settings.stripeKey : 'pk_test_TYooMQauvdEDq54NiTphI7jx';
                    });
                    const organizationName = vue_1.computed(() => {
                        return props.settings.organizationName ? props.settings.organizationName : 'pk_test_TYooMQauvdEDq54NiTphI7jx';
                    });
                    const stripeToken = vue_1.computed(() => {
                        return getHiddenInputValue('hfStripeToken');
                    });
                    const walletName = vue_1.computed(() => {
                        return getHiddenInputValue('hfWalletName');
                    });
                    const postbackFromModal = vue_1.computed(() => {
                        return getHiddenInputValue('hfPostbackFromModal');
                    });
                    const getHiddenInputValue = (elementId) => {
                        let value = '';
                        const element = document.getElementById(elementId);
                        if (element != null) {
                            value = element.value;
                        }
                        return value;
                    };
                    const isCreditCardPaymentTypeActive = vue_1.computed(() => {
                        return activePaymentType.value === 0;
                    });
                    const registrationEntryState = vue_1.inject('registrationEntryState');
                    const amountToPay = vue_1.computed(() => {
                        return registrationEntryState.amountToPayToday;
                    });
                    const expirationArray = vue_1.computed(() => {
                        return expirationDate.value.split('/');
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
                        if (walletName.value === 'applePay') {
                            return 'D42C4DF7-1AE9-4DDE-ADA2-774B866B798C';
                        }
                        else if (walletName.value === 'googlePay') {
                            return '6151F6E0-3223-46BA-A59E-E091BE4AF75C';
                        }
                        else if (isCreditCardPaymentTypeActive.value) {
                            return '928A2E04-C77B-4282-888F-EC549CEE026A';
                        }
                        else if (isBankAccountPaymentTypeActive.value) {
                            return 'DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6';
                        }
                        else {
                            return '56C9AE9C-B5EB-46D5-9650-2EF86B14F856';
                        }
                    });
                    const creditCardButtonClasses = vue_1.computed(() => {
                        return isCreditCardPaymentTypeActive.value
                            ? ['btn', 'btn-default', 'active', 'payment-creditcard']
                            : ['btn', 'btn-default', 'payment-creditcard'];
                    });
                    const bankAccountButtonClasses = vue_1.computed(() => {
                        return isBankAccountPaymentTypeActive.value
                            ? ['btn', 'btn-default', 'active', 'payment-ach']
                            : ['btn', 'btn-default', 'payment-ach'];
                    });
                    const activateCreditCard = () => {
                        clearInputs();
                        activePaymentType.value = 0;
                    };
                    const activateBankAccount = () => {
                        clearInputs();
                        activePaymentType.value = 1;
                    };
                    const formatNumber = (numberString, maxLength, additionalCharacters) => {
                        let allowedString = '[^0-9';
                        const additionalCharacterArray = additionalCharacters.split(',').filter(ac => ac !== '');
                        if (additionalCharacterArray.length > 0) {
                            additionalCharacterArray.forEach(ac => allowedString = allowedString + '\\' + ac);
                        }
                        allowedString = allowedString + ']';
                        const allowedRegex = new RegExp(allowedString, 'g');
                        let formattedNumber = numberString.replace(allowedRegex, '');
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
                            const hasValidationError = false;
                            const errors = {};
                            return {
                                isValid: !hasValidationError,
                                errors
                            };
                        }
                    };
                    const cards = [
                        {
                            type: 'maestro',
                            patterns: [
                                5018, 502, 503, 506, 56, 58, 639, 6220, 67, 633
                            ],
                            format: /(\d{1,4})/g,
                            length: [12, 13, 14, 15, 16, 17, 18, 19],
                            cvcLength: [3],
                            luhn: true
                        },
                        {
                            type: 'forbrugsforeningen',
                            patterns: [600],
                            format: /(\d{1,4})/g,
                            length: [16],
                            cvcLength: [3],
                            luhn: true
                        },
                        {
                            type: 'dankort',
                            patterns: [5019],
                            format: /(\d{1,4})/g,
                            length: [16],
                            cvcLength: [3],
                            luhn: true
                        },
                        {
                            type: 'visa',
                            patterns: [4],
                            format: /(\d{1,4})/g,
                            length: [13, 16],
                            cvcLength: [3],
                            luhn: true
                        },
                        {
                            type: 'mastercard',
                            patterns: [
                                51, 52, 53, 54, 55,
                                22, 23, 24, 25, 26, 27
                            ],
                            format: /(\d{1,4})/g,
                            length: [16],
                            cvcLength: [3],
                            luhn: true
                        },
                        {
                            type: 'amex',
                            patterns: [34, 37],
                            format: /(\d{1,4})(\d{1,6})?(\d{1,5})?/,
                            length: [15, 16],
                            cvcLength: [3, 4],
                            luhn: true
                        },
                        {
                            type: 'dinersclub',
                            patterns: [30, 36, 38, 39],
                            format: /(\d{1,4})(\d{1,6})?(\d{1,4})?/,
                            length: [14],
                            cvcLength: [3],
                            luhn: true
                        },
                        {
                            type: 'discover',
                            patterns: [60, 64, 65, 622],
                            format: /(\d{1,4})/g,
                            length: [16],
                            cvcLength: [3],
                            luhn: true
                        },
                        {
                            type: 'unionpay',
                            patterns: [62, 88],
                            format: /(\d{1,4})/g,
                            length: [16, 17, 18, 19],
                            cvcLength: [3],
                            luhn: false
                        },
                        {
                            type: 'jcb',
                            patterns: [35],
                            format: /(\d{1,4})/g,
                            length: [16],
                            cvcLength: [3],
                            luhn: true
                        }
                    ];
                    const validation = {
                        cardExpiryVal: function (value) {
                            let [monthString, yearString] = Array.from(value.split(/[\s\/]+/, 2));
                            if (((yearString != null ? yearString.length : undefined) === 2) && /^\d+$/.test(yearString)) {
                                let prefix = (new Date).getFullYear();
                                let prefixString = prefix.toString().slice(0, 2);
                                yearString = prefixString + yearString;
                            }
                            let month = parseInt(monthString, 10);
                            let year = parseInt(yearString, 10);
                            return { month, year };
                        },
                        validateCardNumber: function (num) {
                            num = (num + '').replace(/\s+|-/g, '');
                            if (!/^\d+$/.test(num)) {
                                return false;
                            }
                            let card = cardFormatUtils.cardFromNumber(num);
                            if (!card) {
                                return false;
                            }
                            return Array.from(card.length).includes(num.length) &&
                                ((card.luhn === false) || cardFormatUtils.luhnCheck(num));
                        },
                        validateCardExpiry: function (month, year) {
                            if (!month)
                                return false;
                            if (!year) {
                                ({ month, year } = validation.cardExpiryVal(month));
                            }
                            if ((typeof month === 'object') && 'month' in month) {
                                ({ month, year } = month);
                            }
                            if (!month || !year) {
                                return false;
                            }
                            month = month.toString().trim();
                            year = year.toString().trim();
                            if (!/^\d+$/.test(month)) {
                                return false;
                            }
                            if (!/^\d+$/.test(year)) {
                                return false;
                            }
                            if (!(1 <= month && month <= 12)) {
                                return false;
                            }
                            if (year.length === 2) {
                                if (year < 70) {
                                    year = `20${year}`;
                                }
                                else {
                                    year = `19${year}`;
                                }
                            }
                            if (year.length !== 4) {
                                return false;
                            }
                            let expiry = new Date(year, month);
                            let currentTime = new Date;
                            expiry.setMonth(expiry.getMonth() - 1);
                            expiry.setMonth(expiry.getMonth() + 1, 1);
                            return expiry > currentTime;
                        },
                        validateCardCVC: function (cvc, type) {
                            if (!cvc) {
                                return false;
                            }
                            cvc = cvc.toString().trim();
                            if (!/^\d+$/.test(cvc)) {
                                return false;
                            }
                            let card = cardFormatUtils.cardFromType(type);
                            if (card != null) {
                                return Array.from(card.cvcLength).includes(cvc.length);
                            }
                            else {
                                return (cvc.length >= 3) && (cvc.length <= 4);
                            }
                        },
                        cardType: function (num) {
                            if (!num) {
                                return null;
                            }
                            const func = (x) => x.type;
                            return cardFormatUtils.__guard__(cardFormatUtils.cardFromNumber(num), func) || null;
                        },
                        formatCardNumber: function (num) {
                            num = num.toString().replace(/\D/g, '');
                            let card = cardFormatUtils.cardFromNumber(num);
                            if (!card) {
                                return num;
                            }
                            let upperLength = card.length[card.length.length - 1];
                            console.log('upperlength', upperLength);
                            num = num.slice(0, upperLength);
                            console.log('num 195', num);
                            if (card.format.global) {
                                const func = (x) => x.join(' ');
                                return cardFormatUtils.__guard__(num.match(card.format), func);
                            }
                            else {
                                let groups = card.format.exec(num);
                                if (groups == null) {
                                    return;
                                }
                                groups.shift();
                                return groups.join(' ');
                            }
                        },
                        formatExpiry: function (expiry) {
                            let parts = expiry.match(/^\D*(\d{1,2})(\D+)?(\d{1,4})?/);
                            if (!parts) {
                                return '';
                            }
                            let mon = parts[1] || '';
                            let sep = parts[2] || '';
                            let year = parts[3] || '';
                            if (year.length > 0) {
                                sep = ' / ';
                            }
                            else if (sep === ' /') {
                                mon = mon.substring(0, 1);
                                sep = '';
                            }
                            else if ((mon.length === 2) || (sep.length > 0)) {
                                sep = ' / ';
                            }
                            else if ((mon.length === 1) && !['0', '1'].includes(mon)) {
                                mon = `0${mon}`;
                                sep = ' / ';
                            }
                            return mon + sep + year;
                        }
                    };
                    const cardFormatUtils = {
                        cardFromNumber: function (num) {
                            num = (num + '').replace(/\D/g, '');
                            for (let i in cards) {
                                for (let j in cards[i].patterns) {
                                    let p = cards[i].patterns[j] + '';
                                    if (num.substr(0, p.length) === p) {
                                        return cards[i];
                                    }
                                }
                            }
                        },
                        cardFromType: function (type) {
                            for (let i in cards) {
                                if (cards[i].type === type) {
                                    return cards[i];
                                }
                            }
                        },
                        luhnCheck: function (num) {
                            let odd = true;
                            let sum = 0;
                            let digits = (num + '').split('').reverse();
                            for (let i in digits) {
                                let digit = parseInt(digits[i], 10);
                                if (odd = !odd) {
                                    digit *= 2;
                                }
                                if (digit > 9) {
                                    digit -= 9;
                                }
                                sum += digit;
                            }
                            return (sum % 10) === 0;
                        },
                        hasTextSelected: function (target) {
                            if ((target.selectionStart != null) &&
                                (target.selectionStart !== target.selectionEnd)) {
                                return true;
                            }
                            return false;
                        },
                        safeVal: function (value, target, e) {
                            if (e.inputType === 'deleteContentBackward') {
                                return;
                            }
                            let cursor;
                            try {
                                cursor = target.selectionStart;
                            }
                            catch (error) {
                                cursor = null;
                            }
                            let last = target.value;
                            target.value = value;
                            value = target.value;
                            if ((cursor !== null) && document.activeElement == target) {
                                if (cursor === last.length) {
                                    cursor = target.value.length;
                                }
                                if (last !== value) {
                                    let prevPair = last.slice(cursor - 1, +cursor + 1 || undefined);
                                    let currPair = target.value.slice(cursor - 1, +cursor + 1 || undefined);
                                    let digit = value[cursor];
                                    if (/\d/.test(digit) &&
                                        (prevPair === `${digit} `) && (currPair === ` ${digit}`)) {
                                        cursor = cursor + 1;
                                    }
                                }
                                target.selectionStart = cursor;
                                return target.selectionEnd = cursor;
                            }
                        },
                        replaceFullWidthChars: function (str) {
                            if (str == null) {
                                str = '';
                            }
                            let fullWidth = '\uff10\uff11\uff12\uff13\uff14\uff15\uff16\uff17\uff18\uff19';
                            let halfWidth = '0123456789';
                            let value = '';
                            let chars = str.split('');
                            for (let i in chars) {
                                let idx = fullWidth.indexOf(chars[i]);
                                if (idx > -1) {
                                    chars[i] = halfWidth[idx];
                                }
                                value += chars[i];
                            }
                            return value;
                        },
                        reFormatNumeric: function (e) {
                            let target = e.currentTarget;
                            return setTimeout(function () {
                                let value = target.value;
                                value = cardFormatUtils.replaceFullWidthChars(value);
                                value = value.replace(/\D/g, '');
                                return cardFormatUtils.safeVal(value, target, e);
                            });
                        },
                        reFormatCardNumber: function (e) {
                            let target = e.currentTarget;
                            return setTimeout(() => {
                                let value = target.value;
                                value = cardFormatUtils.replaceFullWidthChars(value);
                                value = validation.formatCardNumber(value);
                                return cardFormatUtils.safeVal(value, target, e);
                            });
                        },
                        formatCardNumber: function (e) {
                            let re;
                            let digit = String.fromCharCode(e.which);
                            if (!/^\d+$/.test(digit)) {
                                return;
                            }
                            let target = e.currentTarget;
                            let value = target.value;
                            let card = cardFormatUtils.cardFromNumber(value + digit);
                            let length = (value.replace(/\D/g, '') + digit);
                            let upperLength = 16;
                            if (card) {
                                upperLength = card.length[card.length.length - 1];
                            }
                            if (length.length >= upperLength) {
                                return;
                            }
                            if ((target.selectionStart != null) &&
                                (target.selectionStart !== value.length)) {
                                return;
                            }
                            if (card && (card.type === 'amex')) {
                                re = /^(\d{4}|\d{4}\s\d{6})$/;
                            }
                            else {
                                re = /(?:^|\s)(\d{4})$/;
                            }
                            if (re.test(value + digit)) {
                                e.preventDefault();
                                return setTimeout(() => target.value = value + ' ' + digit);
                            }
                            else if (re.test(value + digit)) {
                                e.preventDefault();
                                return setTimeout(() => target.value = value + digit + ' ');
                            }
                        },
                        formatBackCardNumber: function (e) {
                            let target = e.currentTarget;
                            let value = target.value;
                            if (e.which !== 8) {
                                return;
                            }
                            if ((target.selectionStart != null) &&
                                (target.selectionStart !== value.length)) {
                                return;
                            }
                            if (/\d\s$/.test(value)) {
                                e.preventDefault();
                                return setTimeout(() => target.value = value.replace(/\d\s$/, ''));
                            }
                            else if (/\s\d?$/.test(value)) {
                                e.preventDefault();
                                return setTimeout(() => target.value = value.replace(/\d$/, ''));
                            }
                        },
                        reFormatExpiry: function (e) {
                            let target = e.currentTarget;
                            return setTimeout(function () {
                                let value = target.value;
                                value = cardFormatUtils.replaceFullWidthChars(value);
                                value = validation.formatExpiry(value);
                                return cardFormatUtils.safeVal(value, target, e);
                            });
                        },
                        formatExpiry: function (e) {
                            let digit = String.fromCharCode(e.which);
                            if (!/^\d+$/.test(digit)) {
                                return;
                            }
                            let target = e.currentTarget;
                            let val = target.value + digit;
                            if (/^\d$/.test(val) && !['0', '1'].includes(val)) {
                                e.preventDefault();
                                return setTimeout(() => target.value = (`0${val} / `));
                            }
                            else if (/^\d\d$/.test(val)) {
                                e.preventDefault();
                                return setTimeout(function () {
                                    let m1 = parseInt(val[0], 10);
                                    let m2 = parseInt(val[1], 10);
                                    if ((m2 > 2) && (m1 !== 0)) {
                                        return target.value = (`0${m1} / ${m2}`);
                                    }
                                    else {
                                        return target.value = (`${val} / `);
                                    }
                                });
                            }
                        },
                        formatForwardExpiry: function (e) {
                            let digit = String.fromCharCode(e.which);
                            if (!/^\d+$/.test(digit)) {
                                return;
                            }
                            let target = e.currentTarget;
                            let val = target.value;
                            if (/^\d\d$/.test(val)) {
                                return target.value = (`${val} / `);
                            }
                        },
                        formatForwardSlashAndSpace: function (e) {
                            let which = String.fromCharCode(e.which);
                            if ((which !== '/') && (which !== ' ')) {
                                return;
                            }
                            let target = e.currentTarget;
                            let val = target.value;
                            if (/^\d$/.test(val) && (val !== '0')) {
                                return target.value = (`0${val} / `);
                            }
                        },
                        formatBackExpiry: function (e) {
                            let target = e.currentTarget;
                            let value = target.value;
                            if (e.which !== 8) {
                                return;
                            }
                            if ((target.selectionStart != null) &&
                                (target.selectionStart !== value.length)) {
                                return;
                            }
                            if (/\d\s\/\s$/.test(value)) {
                                e.preventDefault();
                                return setTimeout(() => target.value = value.replace(/\d\s\/\s$/, ''));
                            }
                        },
                        handleExpiryAttributes: function (e) {
                            e.setAttribute('maxlength', 9);
                        },
                        reFormatCVC: function (e) {
                            let target = e.currentTarget;
                            return setTimeout(function () {
                                let value = target.value;
                                value = cardFormatUtils.replaceFullWidthChars(value);
                                value = value.replace(/\D/g, '').slice(0, 4);
                                return cardFormatUtils.safeVal(value, target, e);
                            });
                        },
                        restrictNumeric: function (e) {
                            if (e.metaKey || e.ctrlKey) {
                                return true;
                            }
                            if (e.which === 32) {
                                return false;
                            }
                            if (e.which === 0) {
                                return true;
                            }
                            if (e.which < 33) {
                                return true;
                            }
                            let input = String.fromCharCode(e.which);
                            return (!!/[\d\s]/.test(input)) ? true : e.preventDefault();
                        },
                        restrictCardNumber: function (e) {
                            let target = e.currentTarget;
                            let digit = String.fromCharCode(e.which);
                            if (!/^\d+$/.test(digit)) {
                                return;
                            }
                            if (cardFormatUtils.hasTextSelected(target)) {
                                return;
                            }
                            let value = (target.value + digit).replace(/\D/g, '');
                            let card = cardFormatUtils.cardFromNumber(value);
                            if (card) {
                                return value.length <= card.length[card.length.length - 1];
                            }
                            else {
                                return value.length <= 16;
                            }
                        },
                        restrictExpiry: function (e) {
                            let target = e.currentTarget;
                            let digit = String.fromCharCode(e.which);
                            if (!/^\d+$/.test(digit)) {
                                return;
                            }
                            if (cardFormatUtils.hasTextSelected(target)) {
                                return;
                            }
                            let value = target.value + digit;
                            value = value.replace(/\D/g, '');
                            if (value.length > 6) {
                                return false;
                            }
                        },
                        restrictCVC: function (e) {
                            let target = e.currentTarget;
                            let digit = String.fromCharCode(e.which);
                            if (!/^\d+$/.test(digit)) {
                                return;
                            }
                            if (cardFormatUtils.hasTextSelected(target)) {
                                return;
                            }
                            let val = target.value + digit;
                            return val.length <= 4;
                        },
                        setCardType: function (e) {
                            let target = e.currentTarget;
                            let val = target.value;
                            let cardType = validation.cardType(val) || 'unknown';
                            if (target.className.indexOf(cardType) === -1) {
                                let allTypes = [];
                                for (let i in cards) {
                                    allTypes.push(cards[i].type);
                                }
                                target.classList.remove('unknown');
                                target.classList.remove('identified');
                                target.classList.remove(...allTypes);
                                target.classList.add(cardType);
                                target.dataset.cardBrand = cardType;
                                if (cardType !== 'unknown') {
                                    target.classList.add('identified');
                                }
                            }
                        },
                        __guard__: function (value, transform) {
                            return (typeof value !== 'undefined' && value !== null) ? transform(value) : undefined;
                        }
                    };
                    const validateCreditCardInputs = function () {
                        let hasValidationError = false;
                        const errors = {};
                        const isValidCardNumber = cardNumber.value.length >= 13 && cardNumber.value.length <= 16 && number_1.toNumberOrNull(cardNumber.value) != null;
                        const isValidExpirationDate = expirationMonth.value != null && expirationMonth.value <= 12 && expirationYear.value != null;
                        const isValidCvc = cvc.value.length > 0 && number_1.toNumberOrNull(cvc.value) != null;
                        if (!isValidCardNumber) {
                            hasValidationError = true;
                            errors['Card Number'] = 'Card Number is invalid.';
                        }
                        if (!isValidExpirationDate) {
                            hasValidationError = true;
                            errors['Expiration Date'] = 'Please enter a valid expiration date.';
                        }
                        if (!isValidCvc) {
                            hasValidationError = true;
                            errors['CVC'] = 'CVC is invalid.';
                        }
                        return {
                            isValid: !hasValidationError,
                            errors
                        };
                    };
                    const validateACHInputs = function () {
                        let hasValidationError = false;
                        const errors = {};
                        const isValidHolderName = accountNumber.value.length > 0;
                        const isValidAccountNumber = accountNumber.value.length > 0 && number_1.toNumberOrNull(accountNumber.value) != null;
                        const isValidRoutingNumber = routingNumber.value.length === 9 && number_1.toNumberOrNull(routingNumber.value) != null;
                        if (!isValidHolderName) {
                            hasValidationError = true;
                            errors['Account Holder Name'] = 'Account number is invalid.';
                        }
                        if (!isValidAccountNumber) {
                            hasValidationError = true;
                            errors['Account Number'] = 'Account number is invalid.';
                        }
                        if (!isValidRoutingNumber) {
                            hasValidationError = true;
                            errors['Routing Number'] = 'Routing number is invalid.';
                        }
                        return {
                            isValid: !hasValidationError,
                            errors
                        };
                    };
                    const getCreditCardParams = function () {
                        var _a, _b, _c, _d;
                        let urlencoded = new URLSearchParams();
                        urlencoded.append('card[number]', cardNumber.value);
                        urlencoded.append('card[exp_month]', (_b = (_a = expirationMonth.value) === null || _a === void 0 ? void 0 : _a.toString()) !== null && _b !== void 0 ? _b : '');
                        urlencoded.append('card[exp_year]', (_d = (_c = expirationYear.value) === null || _c === void 0 ? void 0 : _c.toString()) !== null && _d !== void 0 ? _d : '');
                        urlencoded.append('card[cvc]', cvc.value);
                        return urlencoded;
                    };
                    const getAchParams = function () {
                        let urlencoded = new URLSearchParams();
                        urlencoded.append('bank_account[account_number]', accountNumber.value);
                        urlencoded.append('bank_account[country]', props.settings.defaultCountryCode ? props.settings.defaultCountryCode : 'US');
                        urlencoded.append('bank_account[account_holder_type]', 'individual');
                        urlencoded.append('bank_account[currency]', props.settings.currency ? props.settings.currency : 'usd');
                        urlencoded.append('bank_account[routing_number]', routingNumber.value);
                        return urlencoded;
                    };
                    const submit = () => __awaiter(this, void 0, void 0, function* () {
                        if (postbackFromModal.value === 'PaymentButtonClicked') {
                            if (stripeToken.value !== '') {
                                const obsidianToken = currencyTypeGuid.value + '|' + stripeToken.value;
                                emit("success", obsidianToken);
                            }
                            else {
                                emit("error", 'There was an error saving your payment token');
                            }
                        }
                        else {
                            const validationResult = validateInputs();
                            if (validationResult.isValid) {
                                let myHeaders = new Headers();
                                myHeaders.append('Content-Type', 'application/x-www-form-urlencoded');
                                myHeaders.append('Authorization', 'Bearer ' + publicStripeKey.value);
                                let urlencoded = new URLSearchParams();
                                if (isCreditCardPaymentTypeActive.value) {
                                    urlencoded = getCreditCardParams();
                                }
                                else if (isBankAccountPaymentTypeActive.value) {
                                    urlencoded = getAchParams();
                                }
                                yield fetch('https://api.stripe.com/v1/tokens', {
                                    method: 'POST',
                                    headers: myHeaders,
                                    body: urlencoded
                                })
                                    .then(response => response.json())
                                    .then(data => {
                                    const obsidianToken = currencyTypeGuid.value + '|' + data.id;
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
                        restrictNumeric: cardFormatUtils.restrictNumeric,
                        restrictCardNumber: cardFormatUtils.restrictCardNumber,
                        formatCardNumber: cardFormatUtils.formatCardNumber,
                        reFormatCardNumber: cardFormatUtils.reFormatCardNumber,
                        setCardType: cardFormatUtils.setCardType,
                        formatBackCardNumber: cardFormatUtils.formatBackCardNumber,
                        reFormatExpiry: cardFormatUtils.reFormatExpiry,
                        formatExpiry: cardFormatUtils.formatExpiry,
                        routingNumber,
                        accountNumber,
                        isCreditCardPaymentTypeActive,
                        isBankAccountPaymentTypeActive,
                        creditCardButtonClasses,
                        bankAccountButtonClasses,
                        activateCreditCard,
                        activateBankAccount
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
              <input
                type="tel"
                maxlength="19"
                class="form-control simpledonation-input credit-card-input js-credit-card-input"
                v-model="cardNumber"
                @keypress="restrictNumeric($event); restrictCardNumber($event); formatCardNumber($event);"
                @keydown="formatBackCardNumber"
                @keyup="setCardType"
                @paste="reFormatCardNumber"
                @change="reFormatCardNumber"
                @input="reFormatCardNumber($event); setCardType($event);"
              />
            </div>
          </div>
          <div class="break"></div>
          <div class="gateway-payment-container">
            <div class="form-group rock-text-box">
              <label class="control-label">Expiration Date</label>
              <div class="control-wrapper">
                <input
                  pattern="[\d\s\/]*"
                  type="tel"
                  class="form-control simpledonation-input credit-card-exp-input js-credit-card-exp-input"
                  placeholder="MM/YY"
                  @keypress="formatExpiry"
                  @input="reFormatExpiry"
                  @change="reFormatExpiry"
                  @blur="reFormatExpiry"
                  v-model="expirationDate"
                  maxlength="9"
                />
              </div>
            </div>
            <div class="form-group rock-text-box">
              <label class="control-label">CVC</label>
              <div class="control-wrapper">
                <input pattern="\d*" type="tel" class="form-control simpledonation-input credit-card-cvv-input js-credit-card-cvv-input" v-model="cvc" />
              </div>
            </div>
          </div>
        </div>
        <div v-show="isBankAccountPaymentTypeActive" class="gateway-ach-container">
          <div class="form-group rock-text-box">
            <label class="control-label">Routing Number</label>
            <div class="control-wrapper">
              <input class="form-control simpledonation-input check-routing-number-input js-check-routing-number-input" v-model="routingNumber" />
            </div>
          </div>
          <div class="form-group rock-text-box">
            <label class="control-label">Account Number</label>
            <div class="control-wrapper">
              <input class="form-control simpledonation-input check-account-number-input js-check-account-number-input" v-model="accountNumber" />
            </div>
          </div>
        </div>
      </div>
    </div>`
            }));
        }
    };
});
//# sourceMappingURL=simpleDonationGatewayControl.js.map