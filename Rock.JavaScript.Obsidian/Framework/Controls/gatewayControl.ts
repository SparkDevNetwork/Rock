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
import { computed, defineComponent, inject, InjectionKey, PropType, provide } from "vue";
import JavaScriptAnchor from "../Elements/javaScriptAnchor";
import ComponentFromUrl from "./componentFromUrl";

/**
 * The strings that can be emitted by gateway components.
 */
export const enum GatewayEmitStrings {
    /** Indicates a successful submission, value is a string. */
    Success = "success",

    /** Indicates one or more validation errors, value is Record<string, string> */
    Validation = "validation",

    /** A serious error occurred that prevents the gateway from functioning. */
    Error = "error"
}

/**
 * The gateway control model that contains its settings.
 */
export type GatewayControlModel = {
    fileUrl: string;
    settings: Record<string, unknown>;
};

/**
 * The object to be provided by a parent so that it can interact with the
 * financial gateway.
 */
type SubmitPaymentObject = {
    /** The callback to use when submitting the payment. */
    callback?: SubmitPaymentFunction;
};

/** The unique symbol that holds our custom data. */
const submitPaymentCallbackSymbol: InjectionKey<SubmitPaymentObject> = Symbol("gateway-submit-payment-callback");

/** The function signature that will be called when the payment can be submitted. */
export type SubmitPaymentFunction = () => void;

/**
 * Prepares the gateway control for use. This provides a custom object into the
 * calling controls namespace and then returns the function that can be used to
 * attempt to submit the payment.
 */
export const prepareSubmitPayment = (): SubmitPaymentFunction => {
    const container: SubmitPaymentObject = {};
    provide(submitPaymentCallbackSymbol, container);

    return () => {
        if (container.callback) {
            container.callback();
        }
        else {
            throw "Submit payment callback has not been defined.";
        }
    };
};

/**
 * Provides the callback from the gateway component that should be called when
 * the user presses a button to initiate payment.
 * 
 * @param callback The function to be called when the payment should be attempted.
 */
export const onSubmitPayment = (callback: SubmitPaymentFunction): void => {
    const container = inject(submitPaymentCallbackSymbol);

    if (!container) {
        throw "Gateway control has not been properly initialized.";
    }

    container.callback = callback;
};

/**
 * The field that failed validation.
 *
 * @obsolete This is no longer used.
 */
export enum ValidationField {
    CardNumber,
    Expiry,
    SecurityCode
}

export default defineComponent({
    name: "GatewayControl",

    components: {
        ComponentFromUrl,
        JavaScriptAnchor
    },

    props: {
        gatewayControlModel: {
            type: Object as PropType<GatewayControlModel>,
            required: true
        },

        amountToPay: {
            type: Number as PropType<number>,
            required: true
        },

        returnUrl: {
            type: String as PropType<string>,
            required: false
        }
    },

    setup(props, { emit }) {
        /** The URL that will be used to load the gateway component. */
        const url = computed((): string => props.gatewayControlModel.fileUrl);

        /** The settings that will be supplied to the gateway component. */
        const settings = computed((): Record<string, unknown> => props.gatewayControlModel.settings);

        /** The amount to be charged to the payment method by the gateway. */
        const amountToPay = computed((): number => props.amountToPay);

        /**
         * Intercept the success event, so that local state can reflect it.
         * @param token
         */
        const onSuccess = (token: string): void => {
            emit(GatewayEmitStrings.Success, token);
        };

        /**
         * This method handles validation updates.
         * 
         * @param validationErrors The fields and error messages.
         */
        const onValidation = (validationErrors: Record<string, string>): void => {
            emit(GatewayEmitStrings.Validation, validationErrors);
        };

        /**
         * This method handles errors in the gateway component.
         * 
         * @param message The error message to display.
         */
        const onError = (message: string): void => {
            emit(GatewayEmitStrings.Error, message);
        };

        return {
            url,
            settings,
            amountToPay,
            returnUrl: props.returnUrl,
            onSuccess,
            onValidation,
            onError
        };
    },
    methods: {
    },
    template: `
<div>
    <ComponentFromUrl :url="url"
        :settings="settings"
        :amount="amountToPay"
        :returnUrl="returnUrl"
        @validation="onValidation"
        @success="onSuccess"
        @error="onError" />
</div>
`
});
