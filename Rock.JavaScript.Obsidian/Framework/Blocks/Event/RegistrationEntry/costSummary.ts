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
import Loading from "../../../Controls/loading";
import { InvokeBlockActionFunc } from "../../../Util/block";
import CurrencyBox from "../../../Elements/currencyBox";
import HelpBlock from "../../../Elements/helpBlock";
import { ValidationRule } from "../../../Rules/index";
import { asFormattedString } from "../../../Services/number";
import { RegistrationEntryState } from "../registrationEntry";
import { RegistrationEntryBlockArgs } from "./registrationEntryBlockArgs";

enum RegistrationCostSummaryType {
    Cost = 0,
    Fee = 1,
    Discount = 2,
    Total = 3
}

type LineItem = {
    type: RegistrationCostSummaryType;
    description: string;
    cost: number;
    discountedCost: number;
    minPayment: number;
    defaultPayment: number | null;
};

type AugmentedLineItem = LineItem & {
    isFee: boolean;
    discountHelp: string;
    discountedAmountFormatted: string;
    amountFormatted: string;
};

export default defineComponent({
    name: "Event.RegistrationEntry.CostSummary",
    components: {
        Loading,
        CurrencyBox,
        HelpBlock
    },
    setup() {
        return {
            getRegistrationEntryBlockArgs: inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs,
            invokeBlockAction: inject("invokeBlockAction") as InvokeBlockActionFunc,
            registrationEntryState: inject("registrationEntryState") as RegistrationEntryState
        };
    },
    data() {
        return {
            isLoading: false,
            lineItems: [] as LineItem[]
        };
    },
    computed: {
        /** Line items with some extra info computed for table rendering */
        augmentedLineItems(): AugmentedLineItem[] {
            return this.lineItems.map(li => ({
                ...li,
                isFee: li.type === RegistrationCostSummaryType.Fee,
                discountHelp: (this.hasDiscount && li.cost === li.discountedCost) ? "This item is not eligible for the discount." : "",
                amountFormatted: asFormattedString(li.cost, 2),
                discountedAmountFormatted: asFormattedString(li.discountedCost, 2)
            } as AugmentedLineItem));
        },

        /** Should the discount column in the fee table be shown? */
        hasDiscount(): boolean {
            return this.lineItems.some(li => li.discountedCost !== li.cost);
        },

        /** The total cost before discounts */
        total(): number {
            let total = 0;
            this.lineItems.forEach(li => total += li.cost);
            return total;
        },

        /** The total before discounts as a formatted string */
        totalFormatted(): string {
            return `$${asFormattedString(this.total, 2)}`;
        },

        /** The total cost before discounts */
        defaultPaymentAmount(): number {
            let total = 0;
            let hasDefault = false;

            this.lineItems.forEach(li => {
                if (li.defaultPayment) {
                    hasDefault = true;
                    total += li.defaultPayment;
                }
            });

            total = hasDefault ? total : this.maxAmountCanBePaid;

            if (total > this.maxAmountCanBePaid) {
                total = this.maxAmountCanBePaid;
            }

            if (total < this.amountDueToday) {
                total = this.amountDueToday;
            }

            if (total < 0) {
                total = 0;
            }

            return total;
        },

        /** The total cost after discounts */
        discountedTotal(): number {
            let total = 0;
            this.lineItems.forEach(li => total += li.discountedCost);
            return total;
        },

        /** The total after discounts as a formatted string */
        discountedTotalFormatted(): string {
            return `$${asFormattedString(this.discountedTotal, 2)}`;
        },

        /** The min amount that must be paid today */
        amountDueToday(): number {
            if (this.amountPreviouslyPaid) {
                return 0;
            }

            let total = 0;
            this.lineItems.forEach(li => total += li.minPayment);
            return total;
        },

        /** The min amount that must be paid today as a formatted string */
        amountDueTodayFormatted(): string {
            return `$${asFormattedString(this.amountDueToday, 2)}`;
        },

        /** Should the amount that is due today be shown */
        showAmountDueToday(): boolean {
            return this.amountDueToday !== this.discountedTotal;
        },

        /** The amount previously paid */
        amountPreviouslyPaid(): number {
            return this.registrationEntryState.viewModel.session?.previouslyPaid || 0;
        },

        /** The amount previously paid formatted as a string */
        amountPreviouslyPaidFormatted(): string {
            return `$${asFormattedString(this.amountPreviouslyPaid, 2)}`;
        },

        /** The max amount that can be paid today */
        maxAmountCanBePaid(): number {
            const balance = this.discountedTotal - this.amountPreviouslyPaid;

            if (balance > 0) {
                return balance;
            }

            return 0;
        },

        /** The max amount that can be paid today as a formatted string */
        maxAmountCanBePaidFormatted(): string {
            return `$${asFormattedString(this.maxAmountCanBePaid, 2)}`;
        },

        /** The amount that would remain if the user paid the amount indicated in the currency box */
        amountRemaining(): number {
            const actual = this.maxAmountCanBePaid - this.registrationEntryState.amountToPayToday;
            const bounded = actual < 0 ? 0 : actual > this.maxAmountCanBePaid ? this.maxAmountCanBePaid : actual;
            return bounded;
        },

        /** The amount that would remain if the user paid the amount indicated in the currency box as a formatted string */
        amountRemainingFormatted(): string {
            return `$${asFormattedString(this.amountRemaining, 2)}`;
        },

        /** The vee-validate rules for the amount to pay today */
        amountToPayTodayRules(): ValidationRule[] {
            const rules: ValidationRule[] = ["required"];
            let min = this.amountDueToday;
            const max = this.maxAmountCanBePaid;

            if (min > max) {
                min = max;
            }

            rules.push(`gte:${min}`);
            rules.push(`lte:${max}`);

            return rules;
        },
    },
    methods: {
        /** Retrieve the line item costs from the server */
        async fetchData(): Promise<void> {
            this.isLoading = true;
            this.lineItems = [];

            try {
                const response = await this.invokeBlockAction<LineItem[]>("CalculateCost", {
                    args: this.getRegistrationEntryBlockArgs()
                });

                if (response.data) {
                    this.lineItems = response.data;
                }
            }
            finally {
                this.isLoading = false;
            }
        }
    },
    async created() {
        await this.fetchData();
    },
    watch: {
        defaultPaymentAmount: {
            immediate: true,
            handler(): void {
                this.registrationEntryState.amountToPayToday = this.defaultPaymentAmount;
            }
        },

        async "registrationEntryState.discountCode"(): Promise<void> {
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
        <div v-for="lineItem in augmentedLineItems" class="row" :class="lineItem.isFee ? 'fee-row-fee' : 'fee-row-cost'">
            <div class="col-sm-6 fee-caption">
                {{lineItem.description}}
            </div>
            <div v-if="hasDiscount" class="col-sm-3 fee-value">
                <HelpBlock v-if="lineItem.discountHelp" :text="lineItem.discountHelp" />
                <span class="visible-xs-inline">Discounted Amount:</span>
                $ {{lineItem.discountedAmountFormatted}}
            </div>
            <div class="col-sm-3 fee-value">
                <span class="visible-xs-inline">Amount:</span>
                $ {{lineItem.amountFormatted}}
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
                <CurrencyBox label="Amount To Pay Today" :rules="amountToPayTodayRules" v-model="registrationEntryState.amountToPayToday" class="form-right" inputGroupClasses="input-width-md amount-to-pay" />
                <div class="form-group static-control">
                    <label class="control-label">Amount Remaining After Payment</label>
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
});
