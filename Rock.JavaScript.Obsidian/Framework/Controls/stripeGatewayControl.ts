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
import { Guid, newGuid } from "../Util/guid";
import { GatewayEmitStrings, onSubmitPayment } from "./gatewayControl";

/**
 * The settings we expect to receive from the web server.
 */
type Settings = {
    /** The gateway unique identifier being used. */
    gatewayGuid?: Guid;

    publicKey?: string;
};

declare global {
    /* eslint-disable-next-line */
    var Stripe: any;
}

/**
 * Ensures the Stripe script is loaded into the browser.
 */
async function loadStripeJSAsync(): Promise<boolean> {
    if (window.Stripe === undefined) {
        const script = document.createElement("script");
        script.type = "text/javascript";
        script.src = "https://js.stripe.com/v3/";
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

    return window.Stripe !== undefined;
}

export default defineComponent({
    name: "StripeGatewayControl",

    components: {
        LoadingIndicator
    },

    props: {
        settings: {
            type: Object as PropType<Settings>,
            required: true
        },

        amount: {
            type: Number as PropType<number>,
            required: true
        },

        returnUrl: {
            type: String as PropType<string>,
            required: false
        }
    },

    setup(props, { emit }) {
        let stripeClient: any = null;
        let stripeElements: any = null;

        /** true while we are still loading data; otherwise false. */
        const loading = ref(true);

        /** true if we failed to load the CollectJS content. */
        const failedToLoad = ref(false);

        /** true if we have already sent a token response to the server; otherwise false. */
        const tokenResponseSent = ref(false);

        /** Reference to the Stripe payment element. */
        const paymentElement = ref<HTMLElement | null>(null);

        // Add a callback when the submit payment button is pressed.
        onSubmitPayment(async () => {
            if (loading.value || failedToLoad.value) {
                return;
            }

            tokenResponseSent.value = false;

            if (stripeClient && stripeElements) {
                const result = await stripeClient.confirmPayment({
                    elements: stripeElements,
                    confirmParams: {
                        return_url: props.returnUrl ?? ""
                    }
                });

                if (result.error) {
                    emit(GatewayEmitStrings.Validation, {
                        "Payment Information": result.error.message
                    });
                }
            }
        });

        // Additional processing once our template has been processed and mounted
        // into the DOM. Initialize the CollectJS fields.
        onMounted(async () => {
            if (!(await loadStripeJSAsync())) {
                emit(GatewayEmitStrings.Error, "Error configuring hosted gateway.");
                return;
            }

            try {
                stripeClient = Stripe(props.settings.publicKey ?? "");

                const response = await fetch("/api/v2/StripeGateway/createPaymentIntent", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        gatewayGuid: props.settings.gatewayGuid,
                        amount: props.amount
                    })
                });

                const { clientSecret } = await response.json();

                const appearance = {
                    theme: "stripe",
                };

                stripeElements = stripeClient.elements({ appearance, clientSecret });
                const stripePayment = stripeElements.create("payment");
                stripePayment.mount(paymentElement.value);

                loading.value = false;
            }
            catch (ex) {
                console.log(ex);
                failedToLoad.value = true;
                emit(GatewayEmitStrings.Error, "Error configuring hosted gateway.");
                return;
            }
        });

        return {
            loading,
            failedToLoad,
            paymentElement
        };
    },

    template: `
<div>
    <div v-if="loading" class="text-center">
        <LoadingIndicator />
    </div>

    <div v-show="!loading && !failedToLoad" style="max-width: 600px; margin-left: auto; margin-right: auto;">
        <div ref="paymentElement" id="payment-element"></div>
    </div>
</div>`
});
