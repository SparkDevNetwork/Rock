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
export const provideSubmitPayment = (): SubmitPaymentFunction => {
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
