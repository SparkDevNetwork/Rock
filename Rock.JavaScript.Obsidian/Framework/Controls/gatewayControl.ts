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
import { computed, defineComponent, PropType } from "vue";
import JavaScriptAnchor from "./javaScriptAnchor";
import ComponentFromUrl from "./componentFromUrl";
import { FormError } from "@Obsidian/Utility/form";
import { GatewayControlBag } from "@Obsidian/ViewModels/Controls/gatewayControlBag";
import { GatewayEmitStrings } from "@Obsidian/Enums/Controls/gatewayEmitStrings";

export default defineComponent({
    name: "GatewayControl",

    components: {
        ComponentFromUrl,
        JavaScriptAnchor
    },

    props: {
        gatewayControlModel: {
            type: Object as PropType<GatewayControlBag>,
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
        const url = computed((): string => props.gatewayControlModel.fileUrl ?? "");

        /** The settings that will be supplied to the gateway component. */
        const settings = computed((): Record<string, unknown> => props.gatewayControlModel.settings as Record<string, unknown>);

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
        const onValidation = (validationErrors: FormError[]): void => {
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
