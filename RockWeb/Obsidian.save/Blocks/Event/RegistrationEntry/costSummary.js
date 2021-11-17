System.register(["vue", "../../../Controls/loading", "../../../Elements/currencyBox", "../../../Elements/helpBlock", "../../../Rules/index", "../../../Services/number"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var vue_1, loading_1, currencyBox_1, helpBlock_1, index_1, number_1, RegistrationCostSummaryType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (loading_1_1) {
                loading_1 = loading_1_1;
            },
            function (currencyBox_1_1) {
                currencyBox_1 = currencyBox_1_1;
            },
            function (helpBlock_1_1) {
                helpBlock_1 = helpBlock_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            }
        ],
        execute: function () {
            (function (RegistrationCostSummaryType) {
                RegistrationCostSummaryType[RegistrationCostSummaryType["Cost"] = 0] = "Cost";
                RegistrationCostSummaryType[RegistrationCostSummaryType["Fee"] = 1] = "Fee";
                RegistrationCostSummaryType[RegistrationCostSummaryType["Discount"] = 2] = "Discount";
                RegistrationCostSummaryType[RegistrationCostSummaryType["Total"] = 3] = "Total";
            })(RegistrationCostSummaryType || (RegistrationCostSummaryType = {}));
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.CostSummary",
                components: {
                    Loading: loading_1.default,
                    CurrencyBox: currencyBox_1.default,
                    HelpBlock: helpBlock_1.default
                },
                setup() {
                    return {
                        getRegistrationEntryBlockArgs: vue_1.inject("getRegistrationEntryBlockArgs"),
                        invokeBlockAction: vue_1.inject("invokeBlockAction"),
                        registrationEntryState: vue_1.inject("registrationEntryState")
                    };
                },
                data() {
                    return {
                        isLoading: false,
                        lineItems: []
                    };
                },
                computed: {
                    augmentedLineItems() {
                        return this.lineItems.map(li => (Object.assign(Object.assign({}, li), { isFee: li.type === RegistrationCostSummaryType.Fee, discountHelp: (this.hasDiscount && li.cost === li.discountedCost) ? "This item is not eligible for the discount." : "", amountFormatted: number_1.asFormattedString(li.cost, 2), discountedAmountFormatted: number_1.asFormattedString(li.discountedCost, 2) })));
                    },
                    hasDiscount() {
                        return this.lineItems.some(li => li.discountedCost !== li.cost);
                    },
                    total() {
                        let total = 0;
                        this.lineItems.forEach(li => total += li.cost);
                        return total;
                    },
                    totalFormatted() {
                        return `$${number_1.asFormattedString(this.total, 2)}`;
                    },
                    defaultPaymentAmount() {
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
                    discountedTotal() {
                        let total = 0;
                        this.lineItems.forEach(li => total += li.discountedCost);
                        return total;
                    },
                    discountedTotalFormatted() {
                        return `$${number_1.asFormattedString(this.discountedTotal, 2)}`;
                    },
                    amountDueToday() {
                        if (this.amountPreviouslyPaid) {
                            return 0;
                        }
                        let total = 0;
                        this.lineItems.forEach(li => total += li.minPayment);
                        return total;
                    },
                    amountDueTodayFormatted() {
                        return `$${number_1.asFormattedString(this.amountDueToday, 2)}`;
                    },
                    showAmountDueToday() {
                        return this.amountDueToday !== this.discountedTotal;
                    },
                    amountPreviouslyPaid() {
                        var _a;
                        return ((_a = this.registrationEntryState.viewModel.session) === null || _a === void 0 ? void 0 : _a.previouslyPaid) || 0;
                    },
                    amountPreviouslyPaidFormatted() {
                        return `$${number_1.asFormattedString(this.amountPreviouslyPaid, 2)}`;
                    },
                    maxAmountCanBePaid() {
                        const balance = this.discountedTotal - this.amountPreviouslyPaid;
                        if (balance > 0) {
                            return balance;
                        }
                        return 0;
                    },
                    maxAmountCanBePaidFormatted() {
                        return `$${number_1.asFormattedString(this.maxAmountCanBePaid, 2)}`;
                    },
                    amountRemaining() {
                        const actual = this.maxAmountCanBePaid - this.registrationEntryState.amountToPayToday;
                        const bounded = actual < 0 ? 0 : actual > this.maxAmountCanBePaid ? this.maxAmountCanBePaid : actual;
                        return bounded;
                    },
                    amountRemainingFormatted() {
                        return `$${number_1.asFormattedString(this.amountRemaining, 2)}`;
                    },
                    amountToPayTodayRules() {
                        const rules = ["required"];
                        let min = this.amountDueToday;
                        const max = this.maxAmountCanBePaid;
                        if (min > max) {
                            min = max;
                        }
                        rules.push(`gte:${min}`);
                        rules.push(`lte:${max}`);
                        return index_1.ruleArrayToString(rules);
                    },
                },
                methods: {
                    fetchData() {
                        return __awaiter(this, void 0, void 0, function* () {
                            this.isLoading = true;
                            this.lineItems = [];
                            try {
                                const response = yield this.invokeBlockAction("CalculateCost", {
                                    args: this.getRegistrationEntryBlockArgs()
                                });
                                if (response.data) {
                                    this.lineItems = response.data;
                                }
                            }
                            finally {
                                this.isLoading = false;
                            }
                        });
                    }
                },
                created() {
                    return __awaiter(this, void 0, void 0, function* () {
                        yield this.fetchData();
                    });
                },
                watch: {
                    defaultPaymentAmount: {
                        immediate: true,
                        handler() {
                            this.registrationEntryState.amountToPayToday = this.defaultPaymentAmount;
                        }
                    },
                    "registrationEntryState.discountCode"() {
                        return __awaiter(this, void 0, void 0, function* () {
                            yield this.fetchData();
                        });
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
            }));
        }
    };
});
//# sourceMappingURL=costSummary.js.map