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
import GatewayControl from "@Obsidian/Controls/gatewayControl";
import { provideSubmitPayment } from "@Obsidian/Core/Controls/financialGateway";
import { GatewayControlBag } from "@Obsidian/ViewModels/Controls/gatewayControlBag";
import RockForm from "@Obsidian/Controls/rockForm";
import RockValidation from "@Obsidian/Controls/rockValidation";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import RockButton from "@Obsidian/Controls/rockButton";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { newGuid, toGuidOrNull } from "@Obsidian/Utility/guid";
import { SavedFinancialAccountListItemBag } from "@Obsidian/ViewModels/Finance/savedFinancialAccountListItemBag";
import { RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel, RegistrationEntryBlockArgs, RegistrationEntryState } from "./types.partial";
import { FormError } from "@Obsidian/Utility/form";

export default defineComponent({
    name: "Event.RegistrationEntry.Payment",
    components: {
        RockButton,
        RockForm,
        NotificationBox,
        GatewayControl,
        RockValidation
    },
    setup() {
        const submitPayment = provideSubmitPayment();

        const getRegistrationEntryBlockArgs = inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs;
        const invokeBlockAction = useInvokeBlockAction();
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;

        /** Is there an AJAX call in-flight? */
        const loading = ref(false);

        /** Gateway indicated error */
        const gatewayErrorMessage = ref("");

        /** Gateway indicated validation issues */
        const gatewayValidationFields = ref<FormError[]>([]);

        /** An error message received from a bad submission */
        const submitErrorMessage = ref("");

        /** The currently selected saved account. */
        const selectedSavedAccount = ref("");

        return {
            uniqueId: newGuid(),
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
        gatewayControlModel(): GatewayControlBag {
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
        savedAccountOptions(): SavedFinancialAccountListItemBag[] {
            if (this.registrationEntryState.viewModel.savedAccounts === null) {
                return [];
            }

            const options = [...this.registrationEntryState.viewModel.savedAccounts];

            options.push({
                value: "",
                text: "New Payment Method"
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
                    this.gatewayValidationFields = [];
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
        onGatewayControlValidation(invalidFields: FormError[]) {
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },

        /**
         * Get the unique identifier of the option to use on the input control.
         *
         * @param option The option that represents the saved account.
         *
         * @returns A string that contains the unique control identifier.
         */
        getOptionUniqueId(option: SavedFinancialAccountListItemBag): string {
            const key = option.value?.replace(" ", "-") ?? "";

            return `${this.uniqueId}-${key}`;
        },

        /**
         * Gets the image to display for the saved account input control.
         *
         * @param option The option that represents the saved account.
         *
         * @returns A string with the URL of the image to display.
         */
        getAccountImage(option: SavedFinancialAccountListItemBag): string {
            return option.image ?? "";
        },

        /**
         * Gets the name to display for the saved account input control.
         *
         * @param option The option that represents the saved account.
         *
         * @returns A string with the user friendly name of the saved account.
         */
        getAccountName(option: SavedFinancialAccountListItemBag): string {
            return option.text ?? "";
        },

        /**
         * Gets the descriptive text to display for the saved account input control.
         *
         * @param option The option that represents the saved account.
         *
         * @returns A string with the user friendly description of the saved account.
         */
        getAccountDescription(option: SavedFinancialAccountListItemBag): string {
            return option.description ?? "";
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
        <h4>Payment Information</h4>
        <div>
            Payment Amount: {{ amountToPayText }}
        </div>

        <hr/>

        <div v-if="gatewayControlModel" class="payment-method-options">
            <div v-if="hasSavedAccounts" v-for="savedAccount in savedAccountOptions" class="radio payment-method">
                <label :for="getOptionUniqueId(savedAccount)">
                    <input :id="getOptionUniqueId(savedAccount)"
                        :name="uniqueId"
                        type="radio"
                        :value="savedAccount.value"
                        v-model="selectedSavedAccount" />
                    <span class="label-text payment-method-account">
                        <img v-if="getAccountImage(savedAccount)" class="payment-method-image" :src="getAccountImage(savedAccount)">
                        <span class="payment-method-name">{{ getAccountName(savedAccount) }}</span>
                        <span class="payment-method-description text-muted">{{ getAccountDescription(savedAccount) }}</span>
                    </span>
                </label>
            </div>

            <div class="position-relative overflow-hidden">
                <transition name="rockslide">
                    <div v-if="showGateway" class="hosted-gateway-container payment-method-entry">
                        <NotificationBox v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</NotificationBox>
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
                </transition>
            </div>
        </div>

        <NotificationBox v-if="submitErrorMessage" alertType="danger">{{submitErrorMessage}}</NotificationBox>

        <div class="actions text-right">
            <RockButton class="pull-left" btnType="default" @click="onPrevious" :isLoading="loading" autoDisable>
                Previous
            </RockButton>

            <RockButton v-if="gatewayControlModel" btnType="primary" type="submit" :isLoading="loading" autoDisable>
                {{finishButtonText}}
            </RockButton>
        </div>
    </RockForm>
</div>`
});
