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
import Loading from '../../../Controls/Loading';
import { InvokeBlockActionFunc } from '../../../Controls/RockBlock';
import CurrencyBox from '../../../Elements/CurrencyBox';
import HelpBlock from '../../../Elements/HelpBlock';
import { ruleArrayToString } from '../../../Rules/Index';
import { asFormattedString } from '../../../Services/Number';
import { RegistrationEntryState } from '../RegistrationEntry';
import { RegistrationEntryBlockArgs } from './RegistrationEntryBlockArgs';

enum RegistrationCostSummaryType
{
    Cost = 0,
    Fee = 1,
    Discount = 2,
    Total = 3
};

interface LineItem {
    Type: RegistrationCostSummaryType,
    Description: string,
    Cost: number,
    DiscountedCost: number,
    MinPayment: number,
    DefaultPayment: number | null
};

interface AugmentedLineItem extends LineItem
{
    IsFee: boolean,
    DiscountHelp: string,
    DiscountedAmountFormatted: string,
    AmountFormatted: string
};

export default defineComponent( {
    name: 'Event.RegistrationEntry.CostSummary',
    components: {
        Loading,
        CurrencyBox,
        HelpBlock
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
            isLoading: false,
            lineItems: [] as LineItem[]
        };
    },
    computed: {
        /** Line items with some extra info computed for table rendering */
        augmentedLineItems (): AugmentedLineItem[]
        {
            return this.lineItems.map( li => ( {
                ...li,
                IsFee: li.Type === RegistrationCostSummaryType.Fee,
                DiscountHelp: ( this.hasDiscount && li.Cost === li.DiscountedCost ) ? 'This item is not eligible for the discount.' : '',
                AmountFormatted: asFormattedString( li.Cost ),
                DiscountedAmountFormatted: asFormattedString( li.DiscountedCost )
            } as AugmentedLineItem ) );
        },

        /** Should the discount column in the fee table be shown? */
        hasDiscount (): boolean
        {
            return this.lineItems.some( li => li.DiscountedCost !== li.Cost );
        },

        /** The total cost before discounts */
        total (): number
        {
            let total = 0;
            this.lineItems.forEach( li => total += li.Cost );
            return total;
        },

        /** The total before discounts as a formatted string */
        totalFormatted (): string
        {
            return `$${asFormattedString( this.total )}`;
        },

        /** The total cost before discounts */
        defaultPaymentAmount (): number
        {
            let total = 0;
            let hasDefault = false;

            this.lineItems.forEach( li =>
            {
                if ( li.DefaultPayment )
                {
                    hasDefault = true
                    total += li.DefaultPayment
                }
            } );

            total = hasDefault ? total : this.maxAmountCanBePaid;

            if ( total > this.maxAmountCanBePaid )
            {
                total = this.maxAmountCanBePaid;
            }

            if ( total < this.amountDueToday )
            {
                total = this.amountDueToday;
            }

            if ( total < 0 )
            {
                total = 0;
            }

            return total;
        },

        /** The total cost after discounts */
        discountedTotal (): number
        {
            let total = 0;
            this.lineItems.forEach( li => total += li.DiscountedCost );
            return total;
        },

        /** The total after discounts as a formatted string */
        discountedTotalFormatted (): string
        {
            return `$${asFormattedString( this.discountedTotal )}`;
        },

        /** The min amount that must be paid today */
        amountDueToday (): number
        {
            if ( this.amountPreviouslyPaid )
            {
                return 0;
            }

            let total = 0;
            this.lineItems.forEach( li => total += li.MinPayment );
            return total;
        },

        /** The min amount that must be paid today as a formatted string */
        amountDueTodayFormatted (): string
        {
            return `$${asFormattedString( this.amountDueToday )}`;
        },

        /** Should the amount that is due today be shown */
        showAmountDueToday (): boolean
        {
            return this.amountDueToday !== this.discountedTotal;
        },

        /** The amount previously paid */
        amountPreviouslyPaid (): number
        {
            return this.registrationEntryState.ViewModel.Session?.PreviouslyPaid || 0;
        },

        /** The amount previously paid formatted as a string */
        amountPreviouslyPaidFormatted (): string
        {
            return `$${asFormattedString( this.amountPreviouslyPaid )}`;
        },

        /** The max amount that can be paid today */
        maxAmountCanBePaid (): number
        {
            const balance = this.discountedTotal - this.amountPreviouslyPaid;

            if ( balance > 0 )
            {
                return balance;
            }

            return 0;
        },

        /** The max amount that can be paid today as a formatted string */
        maxAmountCanBePaidFormatted (): string
        {
            return `$${asFormattedString( this.maxAmountCanBePaid )}`;
        },

        /** The amount that would remain if the user paid the amount indicated in the currency box */
        amountRemaining (): number
        {
            const actual = this.maxAmountCanBePaid - this.registrationEntryState.AmountToPayToday;
            const bounded = actual < 0 ? 0 : actual > this.maxAmountCanBePaid ? this.maxAmountCanBePaid : actual;
            return bounded;
        },

        /** The amount that would remain if the user paid the amount indicated in the currency box as a formatted string */
        amountRemainingFormatted (): string
        {
            return `$${asFormattedString( this.amountRemaining )}`;
        },

        /** The vee-validate rules for the amount to pay today */
        amountToPayTodayRules (): string
        {
            var rules: string[] = [ 'required' ];
            let min = this.amountDueToday;
            const max = this.maxAmountCanBePaid;

            if ( min > max )
            {
                min = max;
            }

            rules.push( `gte:${min}` );
            rules.push( `lte:${max}` );
            return ruleArrayToString( rules );
        },
    },
    methods: {
        /** Retrieve the line item costs from the server */
        async fetchData ()
        {
            this.isLoading = true;
            this.lineItems = [];

            try
            {
                const response = await this.invokeBlockAction<LineItem[]>( 'CalculateCost', {
                    args: this.getRegistrationEntryBlockArgs()
                } );

                if ( response.data )
                {
                    this.lineItems = response.data;
                }
            }
            finally
            {
                this.isLoading = false;
            }
        }
    },
    async created ()
    {
        await this.fetchData();
    },
    watch: {
        defaultPaymentAmount: {
            immediate: true,
            handler ()
            {
                this.registrationEntryState.AmountToPayToday = this.defaultPaymentAmount;
            }
        },

        async 'registrationEntryState.DiscountCode' ()
        {
            await this.fetchData();
        }
    },
    template: `
<Loading :isLoading="isLoading">
    <div class="fee-table">
        <div class="row hidden-xs fee-header">
            <div class="col-sm-6">
                <strong>Description</strong>
            </div>
            <div v-if="hasDiscount" class="col-sm-3 fee-value">
                <strong>Discounted Amount</strong>
            </div>
            <div class="col-sm-3 fee-value">
                <strong>Amount</strong>
            </div>
        </div>
        <div v-for="lineItem in augmentedLineItems" class="row" :class="lineItem.IsFee ? 'fee-row-fee' : 'fee-row-cost'">
            <div class="col-sm-6 fee-caption">
                {{lineItem.Description}}
            </div>
            <div v-if="hasDiscount" class="col-sm-3 fee-value">
                <HelpBlock v-if="lineItem.DiscountHelp" :text="lineItem.DiscountHelp" />
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
            <div v-if="hasDiscount" class="col-sm-3 fee-value">
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
            <div v-if="amountPreviouslyPaid" class="form-group static-control">
                <label class="control-label">Previously Paid</label>
                <div class="control-wrapper">
                    <div class="form-control-static">
                        {{amountPreviouslyPaidFormatted}}
                    </div>
                </div>
            </div>
            <template v-if="showAmountDueToday && maxAmountCanBePaid">
                <div class="form-group static-control">
                    <label class="control-label">Minimum Due Today</label>
                    <div class="control-wrapper">
                        <div class="form-control-static">
                            {{amountDueTodayFormatted}}
                        </div>
                    </div>
                </div>
                <CurrencyBox label="Amount To Pay Today" :rules="amountToPayTodayRules" v-model="registrationEntryState.AmountToPayToday" class="form-right" inputGroupClasses="input-width-md amount-to-pay" />
                <div class="form-group static-control">
                    <label class="control-label">Amount Remaining</label>
                    <div class="control-wrapper">
                        <div class="form-control-static">
                            {{amountRemainingFormatted}}
                        </div>
                    </div>
                </div>
            </template>
            <div v-else class="form-group static-control">
                <label class="control-label">Amount Due</label>
                <div class="control-wrapper">
                    <div class="form-control-static">
                        {{maxAmountCanBePaidFormatted}}
                    </div>
                </div>
            </div>
        </div>
    </div>
</Loading>`
} );