// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
import { computed, defineComponent, onMounted, PropType, ref } from "vue";
import LoadingIndicator from "../Elements/loadingIndicator";
import { newGuid } from "../Util/guid";
import { GatewayEmitStrings, onSubmitPayment } from "./gatewayControl";
import { CollectJSOptions, InputField, ResponseCallback, TimeoutCallback, TokenResponse, ValidationCallback } from "./nmiGatewayControlTypes";

const enum NMIPaymentType {
    Card = 0,

    BankAccount = 1
}

/**
 * The settings we expect to receive from the web server.
 */
type Settings = {
    /** The payment types that are enabled for use. */
    enabledPaymentTypes?: NMIPaymentType[];

    /** The configured key used to initialize the tokenzier. */
    tokenizationKey?: string;
};

/**
 * The field validation state for the various input fields.
 */
type ValidationField = {
    /** The name of the field. */
    field: string;

    /** true if the field is valid; otherwise false. */
    status: boolean;

    /** If status if false this will contain the error message; otherwise an empty string. */
    message: string;
};

const standardStyling = `
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

/**
 * Ensures the CollectJS script is loaded into the browser.
 * 
 * @param tokenizationKey The tokenization key that will be used to initialize the script.
 */
async function loadCollectJSAsync(tokenizationKey: string): Promise<boolean> {
    if (window.CollectJS === undefined) {
        const script = document.createElement("script");
        script.type = "text/javascript";
        script.src = "https://secure.tnbcigateway.com/token/Collect.js";
        script.setAttribute("data-tokenization-key", tokenizationKey);
        script.setAttribute("data-variant", "inline");
        document.getElementsByTagName("head")[0].appendChild(script);

        try {
            await new Promise<void>((resolve, reject) => {
                script.addEventListener("load", () => resolve());
                script.addEventListener("error", () => reject());
            });
        }
        catch {
            return false;
        }
    }

    return window.CollectJS !== undefined;
}

/**
 * Ensures the CollectJS script is loaded into the browser.
 * 
 * @param tokenizationKey The tokenization key that will be used to initialize the script.
 */
async function loadStandardStyleTagAsync(): Promise<void> {
    const style = document.createElement("style");
    style.type = "text/css";
    style.innerText = standardStyling;

    await new Promise<void>((resolve, reject) => {
        style.addEventListener("load", () => resolve());
        style.addEventListener("error", () => reject());

        document.getElementsByTagName("head")[0].appendChild(style);
    });
}

/**
 * Get the standard CollectJS options. This is primarily all the custom CSS
 * and control references.
 * 
 * @param controlId The identifier of the parent control that contains all the input fields.
 * @param inputStyleHook The element that will be used for standard styling information.
 * @param inputInvalidStyleHook The element that will be used for invalid styling information.
 *
 * @returns An object that contains the standard CollectJS options.
 */
function getCollectJSOptions(controlId: string, inputStyleHook: HTMLElement | null, inputInvalidStyleHook: HTMLElement | null): CollectJSOptions {
    // Populate our custom CSS to make the fields mostly match standard Rock
    // input fields.
    const customCss: Record<string, string> = {
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

    // Custom focus CSS to make the input fields match Rock style.
    const focusCss: Record<string, string> = {
        "border-color": getComputedStyle(document.documentElement).getPropertyValue("--focus-state-border-color"),
        "outline-style": "none"
    };

    // Custom invalid CSS to apply to the field to make it look like a Rock
    // input field.
    const invalidCss: Record<string, string> = {};
    if (inputInvalidStyleHook) {
        invalidCss["border-color"] = getComputedStyle(inputInvalidStyleHook).borderColor;
    }

    // Custom CSS to apply to the placeholder text.
    const placeholderCss: Record<string, string> = {
        "color": getComputedStyle(document.documentElement).getPropertyValue("--input-placeholder")
    };

    // Build the standard CollectJS options.
    const options: CollectJSOptions = {
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
            } as InputField,
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
        callback: () => { /* Intentionally empty, this will be replaced by the caller. */ }
    };

    return options;
}

/**
 * Translates the NMI field name into a user friendly one.
 * 
 * @param field The field name as provided by NMI.
 *
 * @returns A user friendly name for the field.
 */
function getFieldFriendlyName(field: string): string {
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

export default defineComponent({
    name: "NMIGatewayControl",

    components: {
        LoadingIndicator
    },

    props: {
        settings: {
            type: Object as PropType<Settings>,
            required: true
        }
    },

    setup(props, { emit }) {
        /**
         * true if we have attempted to submit the payment information to NMI.
         * This is used to determine if validation messages should be emitted
         * since NMI is a little verbose in its validating.
         */
        let hasAttemptedSubmit = false;

        /** true if we have received a token back from NMI. */
        let hasReceivedToken = false;

        /** true if there is a payment type of Credit Card; otherwise false. */
        const hasCreditCardPaymentType = computed((): boolean => {
            return props.settings.enabledPaymentTypes?.includes(NMIPaymentType.Card) ?? false;
        });

        /** true if there is a payment type of Bank Account (ACH); otherwise false. */
        const hasBankAccountPaymentType = computed((): boolean => {
            return props.settings.enabledPaymentTypes?.includes(NMIPaymentType.BankAccount) ?? false;
        });

        /** true if there are multiple payment types enabled; otherwise false. */
        const hasMultiplePaymentTypes = computed((): boolean => {
            return hasCreditCardPaymentType.value && hasBankAccountPaymentType.value;
        });

        /** The currently active payment type. */
        const activePaymentType = ref(props.settings.enabledPaymentTypes != null && props.settings.enabledPaymentTypes.length > 0 ? props.settings.enabledPaymentTypes[0] : null);

        /** true if the currently active payment type is Credit Card; otherwise false. */
        const isCreditCardPaymentTypeActive = computed((): boolean => {
            return activePaymentType.value === NMIPaymentType.Card;
        });

        /** true if the currently active payment type is Bank Account (ACH); otherwise false. */
        const isBankAccountPaymentTypeActive = computed((): boolean => {
            return activePaymentType.value === NMIPaymentType.BankAccount;
        });

        /** The CSS classes to apply to the credit card payment type button. */
        const creditCardButtonClasses = computed((): string[] => {
            return isCreditCardPaymentTypeActive.value
                ? ["btn", "btn-default", "active", "payment-creditcard"]
                : ["btn", "btn-default", "payment-creditcard"];
        });

        /** The CSS classes to apply to the bank account (ACH) payment type button. */
        const bankAccountButtonClasses = computed((): string[] => {
            return isBankAccountPaymentTypeActive.value
                ? ["btn", "btn-default", "active", "payment-ach"]
                : ["btn", "btn-default", "payment-ach"];
        });

        /** true while we are still loading data; otherwise false. */
        const loading = ref(true);

        /** true if we failed to load the CollectJS content. */
        const failedToLoad = ref(false);

        /** Contains the current validation message to be displayed. */
        const validationMessage = ref("");

        /** Activates the credit card payment type. */
        const activateCreditCard = (): void => {
            CollectJS?.clearInputs();
            activePaymentType.value = NMIPaymentType.Card;
        };

        /** Activates the bank account payment type. */
        const activateBankAccount = (): void => {
            CollectJS?.clearInputs();
            activePaymentType.value = NMIPaymentType.BankAccount;
        };

        /** true if we have already sent a token response to the server; otherwise false. */
        const tokenResponseSent = ref(false);

        /**
         * Contains a unique identifier that we can use to allow CollectJS
         * to find our input fields.
         */
        const controlId = `nmi_${newGuid()}`;

        /** Reference to helper element that allows us to get CSS styles. */
        const inputStyleHook = ref<HTMLElement | null>(null);

        /** Reference to helper element that allows us to get invalid input CSS styles. */
        const inputInvalidStyleHook = ref<HTMLElement | null>(null);

        const paymentInputs = ref<HTMLElement | null>(null);

        /** Contains all the field validation states. */
        const validationFieldStatus: Record<string, ValidationField> = {
            ccnumber: { field: getFieldFriendlyName("ccnumber"), status: false, message: "is required" },
            ccexp: { field: getFieldFriendlyName("ccexp"), status: false, message: "is required" },
            cvv: { field: getFieldFriendlyName("cvv"), status: false, message: "is required" },
            checkaccount: { field: getFieldFriendlyName("checkaccount"), status: false, message: "is required" },
            checkaba: { field: getFieldFriendlyName("checkaba"), status: false, message: "is required" },
            checkname: { field: getFieldFriendlyName("checkname"), status: false, message: "is required" }
        };

        /**
         * Validates all the inputs from CollectJS to see if any visible input
         * fields are invalid.
         *
         * @returns An object that describes if all the inputs are valid.
         */
        const validateInputs = function (): { isValid: boolean, errors: Record<string, string> } {
            let hasValidationError = false;
            const errors: Record<string, string> = {};

            for (const validationFieldKey in validationFieldStatus) {
                const validationField = validationFieldStatus[validationFieldKey];

                // first check visibility. If this is an ACH field, but we are in CC mode (and vice versa), don't validate
                const inputField = document.querySelector(CollectJS?.config.fields[validationFieldKey]?.selector ?? "") as HTMLElement;
                const fieldVisible = (inputField?.offsetWidth ?? 0) !== 0 || (inputField?.offsetHeight ?? 0) !== 0;

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

        /**
         * Callback function that handles the timeout scenario of CollectJS.
         */
        const timeoutCallback: TimeoutCallback = () => {
            // If we got a timeout after sending the response then ignore the error.
            if (tokenResponseSent.value) {
                return;
            }

            // A timeout callback will fire due to a timeout or incomplete
            // input fields (CollectJS doesn't tell us why).
            console.log("The tokenization didn't respond in the expected timeframe. This could be due to an invalid or incomplete field or poor connectivity - " + Date());

            // Since we don't know exactly what happened, lets see if it might
            // be invalid inputs by checking them all manually.
            const validationResult = validateInputs();

            if (!validationResult.isValid) {
                emit(GatewayEmitStrings.Validation, validationResult.errors);
            }
            else {
                // Inputs seem to be valid, so show a message to let them
                // know what seems to be happening.
                console.log("Timeout happened for unknown reason, probably poor connectivity since we already validated inputs.");

                emit(GatewayEmitStrings.Validation, {
                    "Payment Timeout": "Response from gateway timed out. This could be do to poor connectivity or invalid payment values."
                });
            }
        };

        /**
         * Callback function that handles field validation results from the
         * CollectJS back-end.
         * 
         * @param field The name of the field being validated.
         * @param validated true if the field is valid; otherwise false.
         * @param message A message that describes the reason for the validation failure.
         */
        const validationCallback: ValidationCallback = (field: string, validated: boolean, message: string): void => {
            // if there is a validation error, keep the message and field that
            // has the error. Then we'll check it before starting the payment
            // submission.

            if (message === "Field is empty") {
                message = "is required";
            }

            validationFieldStatus[field] = {
                field: field,
                status: validated,
                message: message
            };

            const validationResult = validateInputs();

            if (hasAttemptedSubmit && !(CollectJS?.inSubmission ?? false) && !hasReceivedToken) {
                emit(GatewayEmitStrings.Validation, validationResult.errors);
            }
        };

        // Add a callback when the submit payment button is pressed.
        onSubmitPayment(() => {
            if (loading.value || failedToLoad.value) {
                return;
            }

            tokenResponseSent.value = false;

            // The delay allows field validation when losing field focus.
            setTimeout(() => {
                const validationResult = validateInputs();

                hasAttemptedSubmit = true;
                if (validationResult.isValid) {
                    CollectJS?.startPaymentRequest();
                }
                else {
                    emit(GatewayEmitStrings.Validation, validationResult.errors);
                }
            }, 0);
        });

        /**
         * Callback method when we receive a validated token from NMI.
         * 
         * @param tokenResponse The response data that contains the token.
         */
        const handleTokenResponse: ResponseCallback = (tokenResponse: TokenResponse): void => {
            hasReceivedToken = true;
            emit(GatewayEmitStrings.Success, tokenResponse.token);
        };

        // Additional processing once our template has been processed and mounted
        // into the DOM. Initialize the CollectJS fields.
        onMounted(async () => {
            await loadStandardStyleTagAsync();

            if (!(await loadCollectJSAsync(props.settings.tokenizationKey ?? ""))) {
                emit(GatewayEmitStrings.Error, "Error configuring hosted gateway. This could be due to an invalid or missing Tokenization Key. Please verify that Tokenization Key is configured correctly in gateway settings.");
                return;
            }

            if (paymentInputs.value) {
                paymentInputs.value.querySelectorAll(".iframe-input").forEach(el => {
                    el.innerHTML = "";
                });
            }

            try {
                const options = getCollectJSOptions(controlId, inputStyleHook.value, inputInvalidStyleHook.value);

                options.timeoutCallback = timeoutCallback;
                options.validationCallback = validationCallback;
                options.callback = handleTokenResponse;
                options.fieldsAvailableCallback = () => {
                    loading.value = false;
                };

                CollectJS?.configure(options);
            }
            catch {
                failedToLoad.value = true;
                emit(GatewayEmitStrings.Error, "Error configuring hosted gateway. This could be due to an invalid or missing Tokenization Key. Please verify that Tokenization Key is configured correctly in gateway settings.");
                return;
            }
        });

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
            inputInvalidStyleHook,
            paymentInputs
        };
    },

    template: `
<div>
    <div v-if="loading" class="text-center">
        <LoadingIndicator />
    </div>

    <div v-show="!loading && !failedToLoad" style="max-width: 600px;">
        <div v-if="hasMultiplePaymentTypes" class="gateway-type-selector btn-group btn-group-xs" role="group">
            <a :class="creditCardButtonClasses" @click.prevent="activateCreditCard">Card</a>
            <a :class="bankAccountButtonClasses" @click.prevent="activateBankAccount">Bank Account</a>
        </div>

        <div :id="controlId" class="nmi-payment-inputs" ref="paymentInputs">
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
});
