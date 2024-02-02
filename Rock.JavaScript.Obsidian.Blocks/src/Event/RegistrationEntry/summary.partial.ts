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

import { computed, defineComponent, inject, ref } from "vue";
import Captcha from "@Obsidian/Controls/captcha.obs";
import GatewayControl from "@Obsidian/Controls/gatewayControl.obs";
import RockForm from "@Obsidian/Controls/rockForm.obs";
import RockValidation from "@Obsidian/Controls/rockValidation.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import EmailBox from "@Obsidian/Controls/emailBox.obs";
import RockButton from "@Obsidian/Controls/rockButton.obs";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { getRegistrantBasicInfo } from "./utils.partial";
import CostSummary from "./costSummary.partial";
import DiscountCodeForm from "./discountCodeForm.partial";
import Registrar from "./registrar.partial";
import { RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel, RegistrantBasicInfo, RegistrationEntryState, RegistrationEntryBlockArgs } from "./types.partial";
import { Guid } from "@Obsidian/Types";
import { BlockActionContextBag } from "@Obsidian/ViewModels/Blocks/blockActionContextBag";

export default defineComponent({
    name: "Event.RegistrationEntry.Summary",
    components: {
        Captcha,
        RockButton,
        EmailBox,
        RockForm,
        NotificationBox,
        DropDownList,
        GatewayControl,
        RockValidation,
        CostSummary,
        Registrar,
        DiscountCodeForm
    },
    setup() {
        const getRegistrationEntryBlockArgs = inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs;
        const invokeBlockAction = useInvokeBlockAction();
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;

        const disableCaptchaSupport = ref(registrationEntryState.viewModel.disableCaptchaSupport);
        const captchaElement = ref<InstanceType<typeof Captcha> | undefined>();

        /** Is there an AJAX call in-flight? */
        const loading = ref(false);

        /** An error message received from a bad submission */
        const submitErrorMessage = ref("");
        const hasError = ref(false);

        const persistSession = inject("persistSession") as (force: boolean) => Promise<void>;

        const hasPaymentCost = computed(() => {
            const usedFeeIds: Guid[] = [];

            // Get a list of all fees that are in use.
            for (const registrant of registrationEntryState.registrants) {
                for (const feeId in registrant.feeItemQuantities) {
                    if (registrant.feeItemQuantities[feeId] > 0) {
                        usedFeeIds.push(feeId);
                    }
                }
            }

            // See if any of those fees have a cost.
            const hasCostFees = registrationEntryState.viewModel.fees.some(f => f.items.some(i => i.cost > 0 && usedFeeIds.includes(i.guid)));

            return hasCostFees || registrationEntryState.viewModel.cost > 0;
        });

        if (!hasPaymentCost.value) {
            registrationEntryState.amountToPayToday = 0;
        }

        return {
            captchaElement,
            disableCaptchaSupport,
            loading,
            hasError,
            submitErrorMessage,
            getRegistrationEntryBlockArgs,
            hasPaymentCost,
            invokeBlockAction,
            persistSession,
            registrationEntryState: registrationEntryState
        };
    },

    computed: {
        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },

        /** Info about the registrants made available by .FirstName instead of by field guid */
        registrantInfos(): RegistrantBasicInfo[] {
            return this.registrationEntryState.registrants.map(r => getRegistrantBasicInfo(r, this.viewModel.registrantForms));
        },

        /** The registrant term - plural if there are more than 1 */
        registrantTerm(): string {
            return this.registrantInfos.length === 1 ? this.viewModel.registrantTerm : this.viewModel.pluralRegistrantTerm;
        },

        /** The name of this registration instance */
        instanceName(): string {
            return this.viewModel.instanceName;
        },

        /** The text to be displayed on the "Finish" button */
        finishButtonText(): string {
            if (this.registrationEntryState.amountToPayToday) {
                return this.viewModel.isRedirectGateway ? "Pay" : "Next";
            }
            else {
                return "Finish";
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
                await this.persistSession(true);

                if (this.viewModel.isRedirectGateway) {
                    const redirectUrl = await this.getPaymentRedirect();

                    if (redirectUrl) {
                        location.href = redirectUrl;
                    }
                    else {
                        // Error is shown by getPaymentRedirect method
                        this.loading = false;
                    }
                }
                else {
                    this.loading = false;
                    this.$emit("next");
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

        /** Submit the registration to the server */
        async submit(): Promise<boolean> {
            this.submitErrorMessage = "";

            const actionContext: BlockActionContextBag = {};

            if (this.captchaElement) {
                actionContext.captcha = await this.captchaElement.getToken();
                this.captchaElement.refreshToken();
            }

            const result = await this.invokeBlockAction<RegistrationEntryBlockSuccessViewModel>("SubmitRegistration", {
                args: this.getRegistrationEntryBlockArgs()
            }, actionContext);

            if (result.isError || !result.data) {
                this.submitErrorMessage = result.errorMessage || "Unknown error";
            }
            else {
                this.registrationEntryState.successViewModel = result.data;
            }

            return result.isSuccess;
        },


        /**
         * Persist the args to the server so the user can be redirected for
         * payment. Returns the redirect URL.
         */
        async getPaymentRedirect(): Promise<string> {
            const result = await this.invokeBlockAction<string>("GetPaymentRedirect", {
                args: this.getRegistrationEntryBlockArgs(),
                returnUrl: window.location.href
            });

            if (result.isError || !result.data) {
                this.submitErrorMessage = result.errorMessage || "Unknown error";
            }

            return result.data || "";
        },

        onCostErrorState(error: boolean): void {
            this.hasError = error;
        }
    },

    template: `
<div class="registrationentry-summary">
    <RockForm @submit="onNext">

        <Registrar />

        <div v-if="hasPaymentCost">
            <h4>Payment Summary</h4>
            <DiscountCodeForm />
            <CostSummary @errorState="onCostErrorState" />
        </div>

        <div v-if="!hasPaymentCost" class="margin-b-md">
            <p>The following {{registrantTerm}} will be registered for {{instanceName}}:</p>
            <ul>
                <li v-for="r in registrantInfos" :key="r.guid">
                    <strong>{{r.firstName}} {{r.lastName}}</strong>
                </li>
            </ul>
        </div>

        <Captcha v-if="!disableCaptchaSupport" ref="captchaElement" />

        <NotificationBox v-if="submitErrorMessage" alertType="danger">{{submitErrorMessage}}</NotificationBox>

        <div class="actions text-right clearfix">
            <RockButton v-if="viewModel.allowRegistrationUpdates" class="pull-left" btnType="default" @click="onPrevious" :isLoading="loading" autoDisable>
                Previous
            </RockButton>
            <RockButton v-if="!hasError" btnType="primary" type="submit" :isLoading="loading" autoDisable>
                {{finishButtonText}}
            </RockButton>
        </div>
    </RockForm>
</div>`
});
