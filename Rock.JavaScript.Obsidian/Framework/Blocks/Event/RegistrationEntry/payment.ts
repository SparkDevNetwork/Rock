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

import { defineComponent, inject, ref } from "vue";
import GatewayControl, { GatewayControlModel, prepareSubmitPayment } from "../../../Controls/gatewayControl";
import RockForm from "../../../Controls/rockForm";
import RockValidation from "../../../Controls/rockValidation";
import Alert from "../../../Elements/alert";
import DropDownList, { DropDownListOption } from "../../../Elements/dropDownList";
import RockButton from "../../../Elements/rockButton";
import { useInvokeBlockAction } from "../../../Util/block";
import { toGuidOrNull } from "../../../Util/guid";
import { RegistrationEntryState } from "../registrationEntry";
import DiscountCodeForm from "./discountCodeForm";
import { RegistrationEntryBlockArgs } from "./registrationEntryBlockArgs";
import { RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel } from "./registrationEntryBlockViewModel";

export default defineComponent({
    name: "Event.RegistrationEntry.Payment",
    components: {
        RockButton,
        RockForm,
        Alert,
        DropDownList,
        GatewayControl,
        RockValidation,
        DiscountCodeForm
    },
    setup() {
        const submitPayment = prepareSubmitPayment();

        const getRegistrationEntryBlockArgs = inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs;
        const invokeBlockAction = useInvokeBlockAction();
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;

        /** Is there an AJAX call in-flight? */
        const loading = ref(false);

        /** Gateway indicated error */
        const gatewayErrorMessage = ref("");

        /** Gateway indicated validation issues */
        const gatewayValidationFields = ref<Record<string, string>>({});

        /** An error message received from a bad submission */
        const submitErrorMessage = ref("");

        /** The currently selected saved account. */
        const selectedSavedAccount = ref("");

        if (registrationEntryState.viewModel.savedAccounts !== null && registrationEntryState.viewModel.savedAccounts.length > 0) {
            selectedSavedAccount.value = registrationEntryState.viewModel.savedAccounts[0].value;
        }

        return {
            loading,
            gatewayErrorMessage,
            gatewayValidationFields,
            submitErrorMessage,
            selectedSavedAccount,
            submitPayment,
            getRegistrationEntryBlockArgs,
            invokeBlockAction,
            registrationEntryState: registrationEntryState
        };
    },

    computed: {
        /** The settings for the gateway (MyWell, etc) control */
        gatewayControlModel(): GatewayControlModel {
            return this.viewModel.gatewayControl;
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },

        /** The text to be displayed on the "Finish" button */
        finishButtonText(): string {
            return "Pay";
        },

        /** true if there are any saved accounts to be selected. */
        hasSavedAccounts(): boolean {
            return this.registrationEntryState.viewModel.savedAccounts !== null
                && this.registrationEntryState.viewModel.savedAccounts.length > 0;
        },

        /** Contains the options to display in the saved account drop down list. */
        savedAccountOptions(): DropDownListOption[] {
            if (this.registrationEntryState.viewModel.savedAccounts === null) {
                return [];
            }

            const options = this.registrationEntryState.viewModel.savedAccounts.map(a => {
                const option: DropDownListOption = {
                    value: a.value,
                    text: a.text
                };

                return option;
            });

            options.push({
                value: "",
                text: "Use a different payment method"
            });

            return options;
        },

        /** true if the gateway control should be visible. */
        showGateway(): boolean {
            return !this.hasSavedAccounts || this.selectedSavedAccount === "";
        },

        /** The amount to pay in dollars and cents. */
        amountToPay(): number {
            return this.registrationEntryState.amountToPayToday;
        },

        /** The amount to pay as a friendly text string. */
        amountToPayText(): string {
            return `$${this.registrationEntryState.amountToPayToday.toFixed(2)}`;
        },

        /** The URL to return to if the gateway control needs to perform a redirect. */
        redirectReturnUrl(): string {
            if (window.location.href.includes("?")) {
                return `${window.location.href}&sessionGuid=${this.registrationEntryState.registrationSessionGuid}`;
            }
            else {
                return `${window.location.href}?sessionGuid=${this.registrationEntryState.registrationSessionGuid}`;
            }
        }
    },

    methods: {
        /** User clicked the "previous" button */
        onPrevious() {
            this.$emit("previous");
        },

        /** User clicked the "finish" button */
        async onNext() {
            this.loading = true;

            // If there is a cost, then the gateway will need to be used to pay
            if (this.registrationEntryState.amountToPayToday) {
                if (this.showGateway) {
                    // Otherwise, this is a traditional gateway
                    this.gatewayErrorMessage = "";
                    this.gatewayValidationFields = {};
                    this.submitPayment();
                }
                else if (this.selectedSavedAccount !== "") {
                    this.registrationEntryState.savedAccountGuid = toGuidOrNull(this.selectedSavedAccount);
                    const success = await this.submit();
                    this.loading = false;

                    if (success) {
                        this.$emit("next");
                    }
                }
                else {
                    this.submitErrorMessage = "Please select a valid payment option.";
                    this.loading = false;

                    return;
                }
            }
            else {
                const success = await this.submit();
                this.loading = false;

                if (success) {
                    this.$emit("next");
                }
            }
        },

        /**
         * The gateway indicated success and returned a token
         * @param token
         */
        async onGatewayControlSuccess(token: string) {
            this.registrationEntryState.gatewayToken = token;
            const success = await this.submit();

            this.loading = false;

            if (success) {
                this.$emit("next");
            }
        },

        /**
         * The gateway indicated an error
         * @param message
         */
        onGatewayControlError(message: string) {
            this.loading = false;
            this.gatewayErrorMessage = message;
        },

        /**
         * The gateway wants the user to fix some fields
         * @param invalidFields
         */
        onGatewayControlValidation(invalidFields: Record<string, string>) {
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },

        /** Submit the registration to the server */
        async submit(): Promise<boolean> {
            this.submitErrorMessage = "";

            const result = await this.invokeBlockAction<RegistrationEntryBlockSuccessViewModel>("SubmitRegistration", {
                args: this.getRegistrationEntryBlockArgs()
            });

            if (result.isError || !result.data) {
                this.submitErrorMessage = result.errorMessage || "Unknown error";
            }
            else {
                this.registrationEntryState.successViewModel = result.data;
            }

            return result.isSuccess;
        }
    },

    template: `
<div class="registrationentry-payment">
    <RockForm @submit="onNext">
        <div class="amount-summary">
            <div class="amount-display">
                <span>{{ amountToPayText }}</span>
            </div>
        </div>

        <div v-if="gatewayControlModel" class="well">
            <h4>Payment Method</h4>
            <DropDownList v-if="hasSavedAccounts" v-model="selectedSavedAccount" :options="savedAccountOptions" :showBlankItem="false" />
            <div v-show="showGateway">
                <Alert v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</Alert>
                <RockValidation :errors="gatewayValidationFields" />
                <div class="hosted-payment-control">
                    <GatewayControl
                        :gatewayControlModel="gatewayControlModel"
                        :amountToPay="amountToPay"
                        :returnUrl="redirectReturnUrl"
                        @success="onGatewayControlSuccess"
                        @error="onGatewayControlError"
                        @validation="onGatewayControlValidation" />
                </div>
            </div>
        </div>

        <Alert v-if="submitErrorMessage" alertType="danger">{{submitErrorMessage}}</Alert>

        <div class="actions text-right">
            <RockButton class="pull-left" btnType="default" @click="onPrevious" :isLoading="loading">
                Previous
            </RockButton>
            <RockButton btnType="primary" type="submit" :isLoading="loading">
                {{finishButtonText}}
            </RockButton>
        </div>
    </RockForm>
</div>`
});
