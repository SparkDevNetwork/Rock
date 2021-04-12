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
import JavaScriptAnchor from '../../../Elements/JavaScriptAnchor';
import RockButton from '../../../Elements/RockButton';
import TextBox from '../../../Elements/TextBox';
import { ruleArrayToString } from '../../../Rules/Index';
import { asFormattedString } from '../../../Services/Number';
import { Guid } from '../../../Util/Guid';
import Person from '../../../ViewModels/CodeGenerated/PersonViewModel';
import { getRegistrantBasicInfo, RegistrationEntryState } from '../RegistrationEntry';
import { RegistrationEntryBlockArgs } from './RegistrationEntryBlockArgs';
import { RegistrantInfo, RegistrarInfo, RegistrarOption, RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel } from './RegistrationEntryBlockViewModel';

type CheckDiscountCodeResult = {
    DiscountCode: string;
    UsagesRemaining: number | null;
    DiscountAmount: number;
    DiscountPercentage: number;
};

type LineItem = {
    Key: Guid;
    IsFee: boolean;
    Description: string;
    Amount: number;
    AmountFormatted: string;
    DiscountedAmount: number;
    DiscountedAmountFormatted: string;
    DiscountHelp: string;
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
        JavaScriptAnchor
    },
    setup()
    {
        return {
            invokeBlockAction: inject( 'invokeBlockAction' ) as InvokeBlockActionFunc,
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    data()
    {
        return {
            /** Is there an AJAX call in-flight? */
            loading: false,

            /** The bound value to the discount code input */
            discountCodeInput: '',

            /** A warning message about the discount code that is a result of a failed AJAX call */
            discountCodeWarningMessage: '',

            /** A success message about the discount code that is a result of a successful AJAX call */
            discountCodeSuccessMessage: '',

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
            submitErrorMessage: '',

            /** The amount that will be paid today */
            amountToPayToday: 0
        };
    },
    computed: {
        /** The settings for the gateway (MyWell, etc) control */
        gatewayControlModel(): GatewayControlModel
        {
            return this.viewModel.GatewayControl;
        },

        /** The person that is currently authenticated */
        currentPerson(): Person | null
        {
            return this.$store.state.currentPerson;
        },

        /** The person entering the registration information. This object is part of the registration state. */
        registrar(): RegistrarInfo
        {
            return this.registrationEntryState.Registrar;
        },

        /** The first registrant entered into the registration. */
        firstRegistrant(): RegistrantInfo
        {
            return this.registrationEntryState.Registrants[ 0 ];
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel(): RegistrationEntryBlockViewModel
        {
            return this.registrationEntryState.ViewModel;
        },

        /** Should the checkbox allowing the registrar to choose to update their email address be shown? */
        doShowUpdateEmailOption(): boolean
        {
            return !this.viewModel.ForceEmailUpdate && !!this.currentPerson?.Email;
        },

        /** Should the discount column in the fee table be shown? */
        showDiscountCol(): boolean
        {
            return this.discountPercent > 0 || this.discountAmount > 0;
        },

        /** The fee line items that will be displayed in the summary */
        lineItems(): LineItem[]
        {
            const lineItems: LineItem[] = [];

            for ( const registrant of this.registrationEntryState.Registrants )
            {
                let total = this.viewModel.Cost;
                let discountedTotal = total;
                let discountRemaining = 0;

                if ( this.discountAmount && total < this.discountAmount )
                {
                    discountRemaining = this.discountAmount - total;
                    discountedTotal = 0;
                }
                else if ( this.discountAmount )
                {
                    discountedTotal = total - this.discountAmount;
                }
                else if ( this.discountPercent )
                {
                    const discount = this.discountPercent >= 1 ?
                        this.total :
                        this.discountPercent <= 0 ?
                            0 :
                            ( total * this.discountPercent );

                    discountedTotal = total - discount;
                }

                const info = getRegistrantBasicInfo( registrant, this.viewModel.RegistrantForms );
                const name = registrant.IsOnWaitList ?
                    `${info.FirstName} ${info.LastName} (Waiting List)` :
                    `${info.FirstName} ${info.LastName}`;

                if ( registrant.IsOnWaitList )
                {
                    total = 0;
                    discountedTotal = 0;
                }

                lineItems.push( {
                    Key: registrant.Guid,
                    IsFee: false,
                    Description: name,
                    Amount: total,
                    AmountFormatted: asFormattedString( total ),
                    DiscountedAmount: discountedTotal,
                    DiscountedAmountFormatted: asFormattedString( discountedTotal ),
                    DiscountHelp: ''
                } );

                // Don't show fees if on the waitlist
                if ( registrant.IsOnWaitList )
                {
                    continue;
                }

                for ( const fee of this.viewModel.Fees )
                {
                    for ( const feeItem of fee.Items )
                    {
                        const qty = registrant.FeeItemQuantities[ feeItem.Guid ];

                        if ( !qty )
                        {
                            continue;
                        }

                        const itemTotal = qty * feeItem.Cost;
                        let itemDiscountedTotal = itemTotal;

                        if ( fee.DiscountApplies )
                        {
                            if ( itemTotal < discountRemaining )
                            {
                                discountRemaining -= itemTotal;
                                itemDiscountedTotal = 0;
                            }
                            else if ( discountRemaining )
                            {
                                itemDiscountedTotal -= discountRemaining;
                                discountRemaining = 0;
                            }
                            else if ( this.discountPercent )
                            {
                                const discount = this.discountPercent >= 1 ?
                                    itemTotal :
                                    this.discountPercent <= 0 ?
                                        0 :
                                        ( itemTotal * this.discountPercent );

                                itemDiscountedTotal = itemTotal - discount;
                            }
                        }

                        lineItems.push( {
                            Key: `${registrant.Guid}-${feeItem.Guid}`,
                            IsFee: true,
                            Description: `${fee.Name}-${feeItem.Name} (${qty} @ $${asFormattedString( feeItem.Cost )})`,
                            Amount: itemTotal,
                            AmountFormatted: asFormattedString( itemTotal ),
                            DiscountedAmount: itemDiscountedTotal,
                            DiscountedAmountFormatted: asFormattedString( itemDiscountedTotal ),
                            DiscountHelp: fee.DiscountApplies ? '' : 'This item is not eligible for the discount.'
                        } );
                    }
                }
            }

            return lineItems;
        },

        /** The total amount of the registration before discounts */
        total(): number
        {
            let total = 0;

            for ( const lineItem of this.lineItems )
            {
                total += lineItem.Amount;
            }

            return total;
        },

        /** The total amount of the registration after discounts */
        discountedTotal(): number
        {
            let total = 0;

            for ( const lineItem of this.lineItems )
            {
                total += lineItem.DiscountedAmount;
            }

            return total;
        },

        /** The total before discounts as a formatted string */
        totalFormatted(): string
        {
            return `$${asFormattedString( this.total )}`;
        },

        /** The total after discounts as a formatted string */
        discountedTotalFormatted(): string
        {
            return `$${asFormattedString( this.discountedTotal )}`;
        },

        showAmountDueToday (): boolean
        {
            return this.viewModel.AmountDueToday !== null;
        },

        /** The amount due today */
        amountDueToday (): number
        {
            return this.viewModel.AmountDueToday || 0;
        },

        /** The amount due today formatted as currency */
        amountDueTodayFormatted (): string
        {
            return `$${asFormattedString( this.amountDueToday )}`;
        },

        /** The vee-validate rules for the amount to pay today */
        amountToPayTodayRules (): string
        {
            var rules: string[] = ['required'];
            let min = this.viewModel.AmountDueToday || 0;
            const max = this.discountAmount;

            if ( min > max )
            {
                min = max;
            }

            rules.push( `gte:${min}` );
            rules.push( `lte:${max}` );
            return ruleArrayToString( rules );
        }
    },
    methods: {
        /** User clicked the "previous" button */
        onPrevious()
        {
            this.$emit( 'previous' );
        },

        /** User clicked the "finish" button */
        onNext()
        {
            this.loading = true;
            this.gatewayErrorMessage = '';
            this.gatewayValidationFields = {};
            this.doGatewayControlSubmit = true;
        },

        /** Send a user input discount code to the server so the server can check and send back
         *  the discount amount. */
        async tryDiscountCode()
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

                    const discountText = result.data.DiscountPercentage ?
                        `${asFormattedString( this.discountPercent * 100, 0 )}%` :
                        `$${asFormattedString( this.discountAmount, 2 )}`;

                    this.discountCodeSuccessMessage = `Your ${discountText} discount code for all registrants was successfully applied.`;
                    this.registrationEntryState.DiscountCode = result.data.DiscountCode;
                }
            }
            finally
            {
                this.loading = false;
            }
        },

        /** Prefill in the registrar form fields based on the admin's settings */
        prefillRegistrar()
        {
            // If the information is aleady recorded, do not change it
            if ( this.registrar.NickName || this.registrar.LastName || this.registrar.Email )
            {
                return;
            }

            // If the option is to prompt or use the current person, prefill the current person if available
            if ( this.currentPerson &&
                ( this.viewModel.RegistrarOption === RegistrarOption.UseLoggedInPerson || this.viewModel.RegistrarOption === RegistrarOption.PromptForRegistrar ) )
            {
                this.registrar.NickName = this.currentPerson.NickName || this.currentPerson.FirstName || '';
                this.registrar.LastName = this.currentPerson.LastName || '';
                this.registrar.Email = this.currentPerson.Email || '';
                return;
            }

            if ( this.viewModel.RegistrarOption === RegistrarOption.PromptForRegistrar )
            {
                return;
            }

            // If prefill or first-registrant, then the first registrants info is used (as least as a starting point)
            if ( this.viewModel.RegistrarOption === RegistrarOption.PrefillFirstRegistrant || this.viewModel.RegistrarOption === RegistrarOption.UseFirstRegistrant )
            {
                const firstRegistrantInfo = getRegistrantBasicInfo( this.firstRegistrant, this.viewModel.RegistrantForms );
                this.registrar.NickName = firstRegistrantInfo.FirstName;
                this.registrar.LastName = firstRegistrantInfo.LastName;
                this.registrar.Email = firstRegistrantInfo.Email;
                return;
            }
        },

        /**
         * The gateway indicated success and returned a token
         * @param token
         */
        async onGatewayControlSuccess( token: string )
        {
            this.registrationEntryState.GatewayToken = token;
            const success = await this.submit();
            this.loading = false;

            if ( success )
            {
                this.$emit( 'next' );
            }
        },

        /**
         * The gateway indicated an error
         * @param message
         */
        onGatewayControlError( message: string )
        {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayErrorMessage = message;
        },

        /**
         * The gateway wants the user to fix some fields
         * @param invalidFields
         */
        onGatewayControlValidation( invalidFields: Record<string, string> )
        {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },

        /** Submit the registration to the server */
        async submit(): Promise<boolean>
        {
            const result = await this.invokeBlockAction<RegistrationEntryBlockSuccessViewModel>( 'SubmitRegistration', {
                args: {
                    GatewayToken: this.registrationEntryState.GatewayToken,
                    DiscountCode: this.registrationEntryState.DiscountCode,
                    FieldValues: this.registrationEntryState.RegistrationFieldValues,
                    Registrar: this.registrationEntryState.Registrar,
                    Registrants: this.registrationEntryState.Registrants,
                    AmountToPayNow: this.amountToPayToday
                } as RegistrationEntryBlockArgs
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
        }
    },
    created ()
    {
        if ( this.viewModel.InitialAmountToPay !== null )
        {
            this.amountToPayToday = this.viewModel.InitialAmountToPay;
        }
        else
        {
            this.amountToPayToday = this.discountAmount;
        }
    },
    watch: {
        currentPerson: {
            immediate: true,
            handler()
            {
                this.prefillRegistrar();
            }
        }
    },
    template: `
<div class="registrationentry-summary">
    <RockForm @submit="onNext">

        <div class="well">
            <h4>This Registration Was Completed By</h4>
            <div class="row">
                <div class="col-md-6">
                    <TextBox label="First Name" rules="required" v-model="registrar.NickName" />
                </div>
                <div class="col-md-6">
                    <TextBox label="Last Name" rules="required" v-model="registrar.LastName" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <EmailBox label="Send Confirmation Emails To" rules="required" v-model="registrar.Email" />
                    <CheckBox v-if="doShowUpdateEmailOption" label="Should Your Account Be Updated To Use This Email Address?" v-model="registrar.UpdateEmail" />
                </div>
            </div>
        </div>

        <div>
            <h4>Payment Summary</h4>
            <Alert v-if="discountCodeWarningMessage" alertType="warning">{{discountCodeWarningMessage}}</Alert>
            <Alert v-if="discountCodeSuccessMessage" alertType="success">{{discountCodeSuccessMessage}}</Alert>
            <div class="clearfix">
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
            <div class="fee-table">
                <div class="row hidden-xs fee-header">
                    <div class="col-sm-6">
                        <strong>Description</strong>
                    </div>
                    <div v-if="showDiscountCol" class="col-sm-3 fee-value">
                        <strong>Discounted Amount</strong>
                    </div>
                    <div class="col-sm-3 fee-value">
                        <strong>Amount</strong>
                    </div>
                </div>
                <div v-for="lineItem in lineItems" :key="lineItem.Key" class="row" :class="lineItem.IsFee ? 'fee-row-fee' : 'fee-row-cost'">
                    <div class="col-sm-6 fee-caption">
                        {{lineItem.Description}}
                    </div>
                    <div v-if="showDiscountCol" class="col-sm-3 fee-value">
                        <JavaScriptAnchor v-if="lineItem.DiscountHelp" class="help" :title="lineItem.DiscountHelp">
                            <i class="fa fa-info-circle"></i>
                        </JavaScriptAnchor>
                        <span class="visible-xs-inline">Discounted Amount:</span>
                        $ {{lineItem.DiscountedAmountFormatted}}
                    </div>
                    <div class="col-sm-3 fee-value">
                        <span class="visible-xs-inline">Amount:</span>
                        $ {{lineItem.AmountFormatted}}
                    </div>
                </div>
                <div class="row fee-row-total">
                    <div class="col-sm-6 fee-caption">
                        Total
                    </div>
                    <div v-if="showDiscountCol" class="col-sm-3 fee-value">
                        <span class="visible-xs-inline">Discounted Amount:</span>
                        {{discountedTotalFormatted}}
                    </div>
                    <div class="col-sm-3 fee-value">
                        <span class="visible-xs-inline">Amount:</span>
                        {{totalFormatted}}
                    </div>
                </div>
            </div>

            <div class="row fee-totals">
                <div class="col-sm-offset-8 col-sm-4 fee-totals-options">
                    <div class="form-group static-control">
                        <label class="control-label">Total Cost</label>
                        <div class="control-wrapper">
                            <div class="form-control-static">
                                {{discountedTotalFormatted}}
                            </div>
                        </div>
                    </div>
                    <div class="form-group static-control">
                        <label class="control-label">Amount Due</label>
                        <div class="control-wrapper">
                            <div class="form-control-static">
                                {{discountedTotalFormatted}}
                            </div>
                        </div>
                    </div>
                    <template v-if="showAmountDueToday">
                        <div class="form-group static-control">
                            <label class="control-label">Minimum Due Today</label>
                            <div class="control-wrapper">
                                <div class="form-control-static">
                                    {{amountDueTodayFormatted}}
                                </div>
                            </div>
                        </div>
                        <CurrencyBox label="Amount To Pay Today" :rules="amountToPayTodayRules" v-model="amountToPayToday" />
                        <div v-if="amountRemaining" class="form-group static-control">
                            <label class="control-label">Minimum Due Today</label>
                            <div class="control-wrapper">
                                <div class="form-control-static">
                                    {{amountRemainingFormatted}}
                                </div>
                            </div>
                        </div>
                    </template>
                </div>
            </div>
        </div>

        <div class="well">
            <h4>Payment Method</h4>
            <Alert v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</Alert>
            <RockValidation :errors="gatewayValidationFields" />
            <div class="hosted-payment-control">
                <GatewayControl
                    :gatewayControlModel="gatewayControlModel"
                    :submit="doGatewayControlSubmit"
                    @success="onGatewayControlSuccess"
                    @error="onGatewayControlError"
                    @validation="onGatewayControlValidation" />
            </div>
        </div>

        <Alert v-if="submitErrorMessage" alertType="danger">{{submitErrorMessage}}</Alert>

        <div class="actions">
            <RockButton btnType="default" @click="onPrevious" :isLoading="loading">
                Previous
            </RockButton>
            <RockButton btnType="primary" class="pull-right" type="submit" :isLoading="loading">
                Finish
            </RockButton>
        </div>
    </RockForm>
</div>`
} );