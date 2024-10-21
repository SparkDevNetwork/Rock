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

import { inject, InjectionKey, provide } from "vue";

// #region Submit Payment

/** The function signature that will be called when the payment can be submitted. */
type SubmitPaymentFunction = () => void;

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

/**
 * Prepares the gateway control for use. This provides a custom object into the
 * calling controls namespace and then returns the function that can be used to
 * attempt to submit the payment.
 */
export function provideSubmitPayment(): SubmitPaymentFunction {
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
}

/**
 * Provides the callback from the gateway component that should be called when
 * the user presses a button to initiate payment.
 *
 * @param callback The function to be called when the payment should be attempted.
 */
export function onSubmitPayment(callback: SubmitPaymentFunction): void {
    const container = inject(submitPaymentCallbackSymbol);

    if (!container) {
        throw "Gateway control has not been properly initialized.";
    }

    container.callback = callback;
}

// #endregion

// #region Payment Events

/** Event for when payment is about to be submitted .*/
export type PaymentSubmittingEvent = { readonly type: "submitting"; };

/** Event for when payment succeeds. */
export type PaymentSuccessEvent = { readonly type: "success"; };

/** Event for when payment fails. */
export type PaymentFailureEvent = { readonly type: "failure"; errorMessage: string; };

/** Union of all payment events */
type PaymentEvent =
    | PaymentSubmittingEvent
    | PaymentSuccessEvent
    | PaymentFailureEvent;

type PaymentEventHandler<T extends PaymentEvent> = (event: T) => void;

/**
 * Keeps track of registered event handlers (callbacks) that will be
 * invoked when the associated event is dispatched.
 */
type PaymentEventHandlers = {
    submittingCallbacks: PaymentEventHandler<PaymentSubmittingEvent>[];
    successCallbacks: PaymentEventHandler<PaymentSuccessEvent>[];
    failureCallbacks: PaymentEventHandler<PaymentFailureEvent>[];
};

/**
 * Dispatches payment events.
 */
type PaymentEventDispatcher = (event: PaymentEvent) => void;

/**
 * The unique symbol that holds our custom event data.
 */
const paymentEventsSymbol: InjectionKey<PaymentEventHandlers> = Symbol("gateway-payment-events");

/**
 * Provides payment event functionality to this component as an event dispatcher,
 * and allows subcomponents to register to payment events either by
 * calling the `onPaymentSubmitting(callback)` functions or by using
 * event bindings with the Obsidian component `<GatewayControlEvents ＠paymentSubmitting="onPaymentSubmitting" />`.
 */
export function usePaymentEvents(): PaymentEventDispatcher {
    const container: PaymentEventHandlers = {
        submittingCallbacks: [],
        successCallbacks: [],
        failureCallbacks: []
    };

    provide(paymentEventsSymbol, container);

    return (event: PaymentEvent) => {
        switch (event.type) {
            case "submitting":
                if (container.submittingCallbacks) {
                    container.submittingCallbacks.forEach(callback => {
                        callback(event);
                    });
                }
                break;

            case "success":
                if (container.successCallbacks) {
                    container.successCallbacks.forEach(callback => {
                        callback(event);
                    });
                }
                break;

            case "failure":
                if (container.failureCallbacks) {
                    container.failureCallbacks.forEach(callback => {
                        callback(event);
                    });
                }
                break;

            default:
                console.warn(`Unknown payment event: ${event["type"]}`);
        }
    };
}

/**
 * Provides a callback that is invoked when a payment is about to be submitted.
 *
 * @returns A function to deregister this callback.
 */
export function onPaymentSubmitting(callback: PaymentEventHandler<PaymentSubmittingEvent>): () => void {
    const container = inject(paymentEventsSymbol);

    if (!container) {
        throw "Gateway control has not been properly initialized.";
    }

    container.submittingCallbacks.push(callback);

    return () => {
        container.submittingCallbacks = container.submittingCallbacks.filter(cb => cb !== callback);
    };
}

/**
 * Provides a callback that is invoked when a payment succeeds.
 *
 * @returns A function to deregister this callback.
 */
export function onPaymentSuccess(callback: PaymentEventHandler<PaymentSuccessEvent>): () => void {
    const container = inject(paymentEventsSymbol);

    if (!container) {
        throw "Gateway control has not been properly initialized.";
    }

    container.successCallbacks.push(callback);

    return () => {
        container.successCallbacks = container.successCallbacks.filter(cb => cb !== callback);
    };
}

/**
 * Provides a callback that is invoked when a payment fails.
 *
 * @returns A function to deregister this callback.
 */
export function onPaymentFailure(callback: PaymentEventHandler<PaymentFailureEvent>): () => void {
    const container = inject(paymentEventsSymbol);

    if (!container) {
        throw "Gateway control has not been properly initialized.";
    }

    container.failureCallbacks.push(callback);

    return () => {
        container.failureCallbacks = container.failureCallbacks.filter(cb => cb !== callback);
    };
}

// #endregion Payment Events