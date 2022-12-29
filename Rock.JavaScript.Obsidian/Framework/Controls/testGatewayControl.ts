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
import { defineComponent, PropType, ref } from "vue";
import LoadingIndicator from "./loadingIndicator";
import TextBox from "./textBox";
import { newGuid } from "@Obsidian/Utility/guid";
import { onSubmitPayment } from "@Obsidian/Core/Controls/financialGateway";
import { GatewayEmitStrings } from "@Obsidian/Enums/Controls/gatewayEmitStrings";

type Settings = {
};

export default defineComponent({
    name: "TestGatewayControl",
    components: {
        LoadingIndicator,
        TextBox
    },
    props: {
        settings: {
            type: Object as PropType<Settings>,
            required: true
        },
        submit: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },

    setup(props, { emit }) {
        const cardNumber = ref("");

        const submit = async (): Promise<void> => {
            // Simulate an AJAX call delay
            await new Promise(resolve => setTimeout(resolve, 500));

            // Throw an error for a '0000'
            if (cardNumber.value === "0000") {
                emit(GatewayEmitStrings.Error, "This is a serious problem with the gateway.");
                return;
            }

            // Validate the card number is greater than 10 digits
            if (cardNumber.value.length <= 10) {
                emit(GatewayEmitStrings.Validation, {
                    "Card Number": "Card number is invalid."
                });
                return;
            }

            const token = newGuid().replace(/-/g, "");
            emit(GatewayEmitStrings.Success, token);
        };

        onSubmitPayment(submit);

        return {
            cardNumber
        };
    },

    template: `
<div style="max-width: 600px; margin-left: auto; margin-right: auto;">
    <TextBox label="Credit Card" v-model="cardNumber" />
</div>`
});
