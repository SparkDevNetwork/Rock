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

import { defineComponent, inject } from "vue";
import GatewayControl, { GatewayControlModel } from "../../../Controls/gatewayControl";
import { InvokeBlockActionFunc } from "../../../Util/block";
import RockForm from "../../../Controls/rockForm";
import RockValidation from "../../../Controls/rockValidation";
import Alert from "../../../Elements/alert";
import CheckBox from "../../../Elements/checkBox";
import EmailBox from "../../../Elements/emailBox";
import RockButton from "../../../Elements/rockButton";
import { getRegistrantBasicInfo, RegistrantBasicInfo, RegistrationEntryState } from "../registrationEntry";
import CostSummary from "./costSummary";
import DiscountCodeForm from "./discountCodeForm";
import Registrar from "./registrar";
import { RegistrationEntryBlockArgs } from "./registrationEntryBlockArgs";
import { RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel } from "./registrationEntryBlockViewModel";

export default defineComponent( {
    name: "Event.RegistrationEntry.Summary",
    components: {
        RockButton,
        CheckBox,
        EmailBox,
        RockForm,
        Alert,
        GatewayControl,
        RockValidation,
        CostSummary,
        Registrar,
        DiscountCodeForm
    },
    setup () {
        return {
            getRegistrationEntryBlockArgs: inject( "getRegistrationEntryBlockArgs" ) as () => RegistrationEntryBlockArgs,
            invokeBlockAction: inject( "invokeBlockAction" ) as InvokeBlockActionFunc,
            registrationEntryState: inject( "registrationEntryState" ) as RegistrationEntryState
        };
    },
    data () {
        return {
            /** Is there an AJAX call in-flight? */
            loading: false,

            /** Should the gateway control submit to the gateway to create a token? */
            doGatewayControlSubmit: false,

            /** Gateway indicated error */
            gatewayErrorMessage: "",

            /** Gateway indicated validation issues */
            gatewayValidationFields: {} as Record<string, string>,

            /** An error message received from a bad submission */
            submitErrorMessage: ""
        };
    },
    computed: {
        /** The settings for the gateway (MyWell, etc) control */
        gatewayControlModel (): GatewayControlModel {
            return this.viewModel.gatewayControl;
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel (): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },

        /** Info about the registrants made available by .FirstName instead of by field guid */
        registrantInfos (): RegistrantBasicInfo[] {
            return this.registrationEntryState.registrants.map( r => getRegistrantBasicInfo( r, this.viewModel.registrantForms ) );
        },

        /** The registrant term - plural if there are more than 1 */
        registrantTerm (): string {
            return this.registrantInfos.length === 1 ? this.viewModel.registrantTerm : this.viewModel.pluralRegistrantTerm;
        },

        /** The name of this registration instance */
        instanceName (): string {
            return this.viewModel.instanceName;
        },

        /** The text to be displayed on the "Finish" button */
        finishButtonText (): string {
            return ( this.viewModel.isRedirectGateway && this.registrationEntryState.amountToPayToday ) ? "Pay" : "Finish";
        }
    },
    methods: {
        /** User clicked the "previous" button */
        onPrevious () {
            this.$emit( "previous" );
        },

        /** User clicked the "finish" button */
        async onNext () {
            this.loading = true;

            // If there is a cost, then the gateway will need to be used to pay
            if ( this.registrationEntryState.amountToPayToday ) {
                // If this is a redirect gateway, then persist and redirect now
                if ( this.viewModel.isRedirectGateway ) {
                    const redirectUrl = await this.getPaymentRedirect();

                    if ( redirectUrl ) {
                        location.href = redirectUrl;
                    }
                    else {
                        // Error is shown by getPaymentRedirect method
                        this.loading = false;
                    }
                }
                else {
                    // Otherwise, this is a traditional gateway
                    this.gatewayErrorMessage = "";
                    this.gatewayValidationFields = {};
                    this.doGatewayControlSubmit = true;
                }
            }
            else {
                const success = await this.submit();
                this.loading = false;

                if ( success ) {
                    this.$emit( "next" );
                }
            }
        },

        /**
         * The gateway indicated success and returned a token
         * @param token
         */
        async onGatewayControlSuccess ( token: string ) {
            this.registrationEntryState.gatewayToken = token;
            const success = await this.submit();
            this.loading = false;

            if ( success ) {
                this.$emit( "next" );
            }
        },

        /** The gateway was requested by the user to reset. The token should be cleared */
        async onGatewayControlReset () {
            this.registrationEntryState.gatewayToken = "";
            this.doGatewayControlSubmit = false;
        },

        /**
         * The gateway indicated an error
         * @param message
         */
        onGatewayControlError ( message: string ) {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayErrorMessage = message;
        },

        /**
         * The gateway wants the user to fix some fields
         * @param invalidFields
         */
        onGatewayControlValidation ( invalidFields: Record<string, string> ) {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },

        /** Submit the registration to the server */
        async submit (): Promise<boolean> {
            const result = await this.invokeBlockAction<RegistrationEntryBlockSuccessViewModel>( "SubmitRegistration", {
                args: this.getRegistrationEntryBlockArgs()
            } );

            if ( result.isError || !result.data ) {
                this.submitErrorMessage = result.errorMessage || "Unknown error";
            }
            else {
                this.registrationEntryState.successViewModel = result.data;
            }

            return result.isSuccess;
        },

        /** Persist the args to the server so the user can be redirected for payment. Returns the redirect URL. */
        async getPaymentRedirect (): Promise<string> {
            const result = await this.invokeBlockAction<string>( "GetPaymentRedirect", {
                args: this.getRegistrationEntryBlockArgs()
            } );

            if ( result.isError || !result.data ) {
                this.submitErrorMessage = result.errorMessage || "Unknown error";
            }

            return result.data || "";
        }
    },
    template: `
<div class="registrationentry-summary">
    <RockForm @submit="onNext">

        <Registrar />

        <div v-if="viewModel.cost">
            <h4>Payment Summary</h4>
            <DiscountCodeForm />
            <CostSummary />
        </div>

        <div v-if="gatewayControlModel && registrationEntryState.amountToPayToday" class="well">
            <h4>Payment Method</h4>
            <Alert v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</Alert>
            <RockValidation :errors="gatewayValidationFields" />
            <div class="hosted-payment-control">
                <GatewayControl
                    :gatewayControlModel="gatewayControlModel"
                    :submit="doGatewayControlSubmit"
                    @success="onGatewayControlSuccess"
                    @reset="onGatewayControlReset"
                    @error="onGatewayControlError"
                    @validation="onGatewayControlValidation" />
            </div>
        </div>

        <div v-if="!viewModel.cost" class="margin-b-md">
            <p>The following {{registrantTerm}} will be registered for {{instanceName}}:</p>
            <ul>
                <li v-for="r in registrantInfos" :key="r.guid">
                    <strong>{{r.firstName}} {{r.lastName}}</strong>
                </li>
            </ul>
        </div>

        <Alert v-if="submitErrorMessage" alertType="danger">{{submitErrorMessage}}</Alert>

        <div class="actions text-right">
            <RockButton v-if="viewModel.allowRegistrationUpdates" class="pull-left" btnType="default" @click="onPrevious" :isLoading="loading">
                Previous
            </RockButton>
            <RockButton btnType="primary" type="submit" :isLoading="loading">
                {{finishButtonText}}
            </RockButton>
        </div>
    </RockForm>
</div>`
} );