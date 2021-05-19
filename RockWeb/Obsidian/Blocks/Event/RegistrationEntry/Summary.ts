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

import { defineComponent, inject } from 'vue';
import GatewayControl, { GatewayControlModel } from '../../../Controls/GatewayControl';
import { InvokeBlockActionFunc } from '../../../Controls/RockBlock';
import RockForm from '../../../Controls/RockForm';
import RockValidation from '../../../Controls/RockValidation';
import Alert from '../../../Elements/Alert';
import CheckBox from '../../../Elements/CheckBox';
import EmailBox from '../../../Elements/EmailBox';
import RockButton from '../../../Elements/RockButton';
import TextBox from '../../../Elements/TextBox';
import { asFormattedString } from '../../../Services/Number';
import { getRegistrantBasicInfo, RegistrantBasicInfo, RegistrationEntryState } from '../RegistrationEntry';
import CostSummary from './CostSummary';
import Registrar from './Registrar';
import { RegistrationEntryBlockArgs } from './RegistrationEntryBlockArgs';
import { RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel } from './RegistrationEntryBlockViewModel';

type CheckDiscountCodeResult = {
    DiscountCode: string;
    UsagesRemaining: number | null;
    DiscountAmount: number;
    DiscountPercentage: number;
};

export default defineComponent( {
    name: 'Event.RegistrationEntry.Summary',
    components: {
        RockButton,
        TextBox,
        CheckBox,
        EmailBox,
        RockForm,
        Alert,
        GatewayControl,
        RockValidation,
        CostSummary,
        Registrar
    },
    setup ()
    {
        return {
            getRegistrationEntryBlockArgs: inject( 'getRegistrationEntryBlockArgs' ) as () => RegistrationEntryBlockArgs,
            invokeBlockAction: inject( 'invokeBlockAction' ) as InvokeBlockActionFunc,
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    data ()
    {
        return {
            /** Is there an AJAX call in-flight? */
            loading: false,

            /** The bound value to the discount code input */
            discountCodeInput: '',

            /** A warning message about the discount code that is a result of a failed AJAX call */
            discountCodeWarningMessage: '',

            /** The dollar amount to be discounted because of the discount code entered. */
            discountAmount: 0,

            /** The percent of the total to be discounted because of the discount code entered. */
            discountPercent: 0,

            /** Should the gateway control submit to the gateway to create a token? */
            doGatewayControlSubmit: false,

            /** Gateway indicated error */
            gatewayErrorMessage: '',

            /** Gateway indicated validation issues */
            gatewayValidationFields: {} as Record<string, string>,

            /** An error message received from a bad submission */
            submitErrorMessage: ''
        };
    },
    computed: {
        /** The success message displayed once a discount code has been applied */
        discountCodeSuccessMessage (): string
        {
            const discountAmount = this.viewModel.Session?.DiscountAmount || this.discountAmount;
            const discountPercent = this.viewModel.Session?.DiscountPercentage || this.discountPercent;

            if ( !discountPercent && !discountAmount )
            {
                return '';
            }

            const discountText = discountPercent ?
                `${asFormattedString( discountPercent * 100, 0 )}%` :
                `$${asFormattedString( discountAmount, 2 )}`;

            return `Your ${discountText} discount code for all registrants was successfully applied.`;
        },

        /** Should the discount panel be shown? */
        isDiscountPanelVisible (): boolean
        {
            if ( !this.viewModel.HasDiscountsAvailable )
            {
                return false;
            }

            return true;
        },

        /** The settings for the gateway (MyWell, etc) control */
        gatewayControlModel (): GatewayControlModel
        {
            return this.viewModel.GatewayControl;
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel (): RegistrationEntryBlockViewModel
        {
            return this.registrationEntryState.ViewModel;
        },

        /** Info about the registrants made available by .FirstName instead of by field guid */
        registrantInfos (): RegistrantBasicInfo[]
        {
            return this.registrationEntryState.Registrants.map( r => getRegistrantBasicInfo( r, this.viewModel.RegistrantForms ) );
        },

        /** The registrant term - plural if there are more than 1 */
        registrantTerm (): string
        {
            return this.registrantInfos.length === 1 ? this.viewModel.RegistrantTerm : this.viewModel.PluralRegistrantTerm;
        },

        /** The name of this registration instance */
        instanceName (): string
        {
            return this.viewModel.InstanceName;
        },

        /** The text to be displayed on the "Finish" button */
        finishButtonText (): string
        {
            return ( this.viewModel.IsRedirectGateway && this.registrationEntryState.AmountToPayToday ) ? 'Pay' : 'Finish';
        }
    },
    methods: {
        /** User clicked the "previous" button */
        onPrevious ()
        {
            this.$emit( 'previous' );
        },

        /** User clicked the "finish" button */
        async onNext ()
        {
            this.loading = true;

            // If there is a cost, then the gateway will need to be used to pay
            if ( this.registrationEntryState.AmountToPayToday )
            {
                // If this is a redirect gateway, then persist and redirect now
                if ( this.viewModel.IsRedirectGateway )
                {
                    const redirectUrl = await this.getPaymentRedirect();

                    if ( redirectUrl )
                    {
                        location.href = redirectUrl;
                    }
                }
                else
                {
                    // Otherwise, this is a traditional gateway
                    this.gatewayErrorMessage = '';
                    this.gatewayValidationFields = {};
                    this.doGatewayControlSubmit = true;
                }
            }
            else
            {
                const success = await this.submit();
                this.loading = false;

                if ( success )
                {
                    this.$emit( 'next' );
                }
            }
        },

        /** Send a user input discount code to the server so the server can check and send back
         *  the discount amount. */
        async tryDiscountCode ()
        {
            this.loading = true;

            try
            {
                const result = await this.invokeBlockAction<CheckDiscountCodeResult>( 'CheckDiscountCode', {
                    code: this.discountCodeInput
                } );

                if ( result.isError || !result.data )
                {
                    this.discountCodeWarningMessage = `'${this.discountCodeInput}' is not a valid Discount Code.`;
                }
                else
                {
                    this.discountCodeWarningMessage = '';
                    this.discountAmount = result.data.DiscountAmount;
                    this.discountPercent = result.data.DiscountPercentage;
                    this.registrationEntryState.DiscountCode = result.data.DiscountCode;
                }
            }
            finally
            {
                this.loading = false;
            }
        },

        /**
         * The gateway indicated success and returned a token
         * @param token
         */
        async onGatewayControlSuccess ( token: string )
        {
            this.registrationEntryState.GatewayToken = token;
            const success = await this.submit();
            this.loading = false;

            if ( success )
            {
                this.$emit( 'next' );
            }
        },

        /** The gateway was requested by the user to reset. The token should be cleared */
        async onGatewayControlReset ()
        {
            this.registrationEntryState.GatewayToken = '';
            this.doGatewayControlSubmit = false;
        },

        /**
         * The gateway indicated an error
         * @param message
         */
        onGatewayControlError ( message: string )
        {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayErrorMessage = message;
        },

        /**
         * The gateway wants the user to fix some fields
         * @param invalidFields
         */
        onGatewayControlValidation ( invalidFields: Record<string, string> )
        {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },

        /** Submit the registration to the server */
        async submit (): Promise<boolean>
        {
            const result = await this.invokeBlockAction<RegistrationEntryBlockSuccessViewModel>( 'SubmitRegistration', {
                args: this.getRegistrationEntryBlockArgs()
            } );

            if ( result.isError || !result.data )
            {
                this.submitErrorMessage = result.errorMessage || 'Unknown error';
            }
            else
            {
                this.registrationEntryState.SuccessViewModel = result.data;
            }

            return result.isSuccess;
        },

        /** Persist the args to the server so the user can be redirected for payment. Returns the redirect URL. */
        async getPaymentRedirect (): Promise<string>
        {
            const result = await this.invokeBlockAction<string>( 'GetPaymentRedirect', {
                args: this.getRegistrationEntryBlockArgs()
            } );

            if ( result.isError || !result.data )
            {
                this.submitErrorMessage = result.errorMessage || 'Unknown error';
            }

            return result.data || '';
        }
    },
    template: `
<div class="registrationentry-summary">
    <RockForm @submit="onNext">

        <Registrar />

        <div v-if="viewModel.Cost">
            <h4>Payment Summary</h4>
            <Alert v-if="discountCodeWarningMessage" alertType="warning">{{discountCodeWarningMessage}}</Alert>
            <Alert v-if="discountCodeSuccessMessage" alertType="success">{{discountCodeSuccessMessage}}</Alert>
            <div v-if="isDiscountPanelVisible || discountCodeInput" class="clearfix">
                <div class="form-group pull-right">
                    <label class="control-label">Discount Code</label>
                    <div class="input-group">
                        <input type="text" :disabled="loading || !!discountCodeSuccessMessage" class="form-control input-width-md input-sm" v-model="discountCodeInput" />
                        <RockButton v-if="!discountCodeSuccessMessage" btnSize="sm" :isLoading="loading" class="margin-l-sm" @click="tryDiscountCode">
                            Apply
                        </RockButton>
                    </div>
                </div>
            </div>
            <CostSummary />
        </div>

        <div v-if="gatewayControlModel && registrationEntryState.AmountToPayToday" class="well">
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

        <div v-if="!viewModel.Cost" class="margin-b-md">
            <p>The following {{registrantTerm}} will be registered for {{instanceName}}:</p>
            <ul>
                <li v-for="r in registrantInfos" :key="r.Guid">
                    <strong>{{r.FirstName}} {{r.LastName}}</strong>
                </li>
            </ul>
        </div>

        <Alert v-if="submitErrorMessage" alertType="danger">{{submitErrorMessage}}</Alert>

        <div class="actions text-right">
            <RockButton v-if="viewModel.AllowRegistrationUpdates" class="pull-left" btnType="default" @click="onPrevious" :isLoading="loading">
                Previous
            </RockButton>
            <RockButton btnType="primary" type="submit" :isLoading="loading">
                {{finishButtonText}}
            </RockButton>
        </div>
    </RockForm>
</div>`
} );