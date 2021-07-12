System.register(["vue", "../../../Controls/Loading", "../../../Elements/CurrencyBox", "../../../Elements/HelpBlock", "../../../Rules/Index", "../../../Services/Number"], function (exports_1, context_1) {
    "use strict";
    var __assign = (this && this.__assign) || function () {
        __assign = Object.assign || function(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                    t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var __generator = (this && this.__generator) || function (thisArg, body) {
        var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
        return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
        function verb(n) { return function (v) { return step([n, v]); }; }
        function step(op) {
            if (f) throw new TypeError("Generator is already executing.");
            while (_) try {
                if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                if (y = 0, t) op = [op[0] & 2, t.value];
                switch (op[0]) {
                    case 0: case 1: t = op; break;
                    case 4: _.label++; return { value: op[1], done: false };
                    case 5: _.label++; y = op[1]; op = [0]; continue;
                    case 7: op = _.ops.pop(); _.trys.pop(); continue;
                    default:
                        if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                        if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                        if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                        if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                        if (t[2]) _.ops.pop();
                        _.trys.pop(); continue;
                }
                op = body.call(thisArg, _);
            } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
            if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
        }
    };
    var vue_1, Loading_1, CurrencyBox_1, HelpBlock_1, Index_1, Number_1, RegistrationCostSummaryType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Loading_1_1) {
                Loading_1 = Loading_1_1;
            },
            function (CurrencyBox_1_1) {
                CurrencyBox_1 = CurrencyBox_1_1;
            },
            function (HelpBlock_1_1) {
                HelpBlock_1 = HelpBlock_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            }
        ],
        execute: function () {
            (function (RegistrationCostSummaryType) {
                RegistrationCostSummaryType[RegistrationCostSummaryType["Cost"] = 0] = "Cost";
                RegistrationCostSummaryType[RegistrationCostSummaryType["Fee"] = 1] = "Fee";
                RegistrationCostSummaryType[RegistrationCostSummaryType["Discount"] = 2] = "Discount";
                RegistrationCostSummaryType[RegistrationCostSummaryType["Total"] = 3] = "Total";
            })(RegistrationCostSummaryType || (RegistrationCostSummaryType = {}));
            ;
            ;
            ;
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.CostSummary',
                components: {
                    Loading: Loading_1.default,
                    CurrencyBox: CurrencyBox_1.default,
                    HelpBlock: HelpBlock_1.default
                },
                setup: function () {
                    return {
                        getRegistrationEntryBlockArgs: vue_1.inject('getRegistrationEntryBlockArgs'),
                        invokeBlockAction: vue_1.inject('invokeBlockAction'),
                        registrationEntryState: vue_1.inject('registrationEntryState')
                    };
                },
                data: function () {
                    return {
                        isLoading: false,
                        lineItems: []
                    };
                },
                computed: {
                    augmentedLineItems: function () {
                        var _this = this;
                        return this.lineItems.map(function (li) { return (__assign(__assign({}, li), { IsFee: li.Type === RegistrationCostSummaryType.Fee, DiscountHelp: (_this.hasDiscount && li.Cost === li.DiscountedCost) ? 'This item is not eligible for the discount.' : '', AmountFormatted: Number_1.asFormattedString(li.Cost), DiscountedAmountFormatted: Number_1.asFormattedString(li.DiscountedCost) })); });
                    },
                    hasDiscount: function () {
                        return this.lineItems.some(function (li) { return li.DiscountedCost !== li.Cost; });
                    },
                    total: function () {
                        var total = 0;
                        this.lineItems.forEach(function (li) { return total += li.Cost; });
                        return total;
                    },
                    totalFormatted: function () {
                        return "$" + Number_1.asFormattedString(this.total);
                    },
                    defaultPaymentAmount: function () {
                        var total = 0;
                        var hasDefault = false;
                        this.lineItems.forEach(function (li) {
                            if (li.DefaultPayment) {
                                hasDefault = true;
                                total += li.DefaultPayment;
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
                    discountedTotal: function () {
                        var total = 0;
                        this.lineItems.forEach(function (li) { return total += li.DiscountedCost; });
                        return total;
                    },
                    discountedTotalFormatted: function () {
                        return "$" + Number_1.asFormattedString(this.discountedTotal);
                    },
                    amountDueToday: function () {
                        if (this.amountPreviouslyPaid) {
                            return 0;
                        }
                        var total = 0;
                        this.lineItems.forEach(function (li) { return total += li.MinPayment; });
                        return total;
                    },
                    amountDueTodayFormatted: function () {
                        return "$" + Number_1.asFormattedString(this.amountDueToday);
                    },
                    showAmountDueToday: function () {
                        return this.amountDueToday !== this.discountedTotal;
                    },
                    amountPreviouslyPaid: function () {
                        var _a;
                        return ((_a = this.registrationEntryState.ViewModel.session) === null || _a === void 0 ? void 0 : _a.previouslyPaid) || 0;
                    },
                    amountPreviouslyPaidFormatted: function () {
                        return "$" + Number_1.asFormattedString(this.amountPreviouslyPaid);
                    },
                    maxAmountCanBePaid: function () {
                        var balance = this.discountedTotal - this.amountPreviouslyPaid;
                        if (balance > 0) {
                            return balance;
                        }
                        return 0;
                    },
                    maxAmountCanBePaidFormatted: function () {
                        return "$" + Number_1.asFormattedString(this.maxAmountCanBePaid);
                    },
                    amountRemaining: function () {
                        var actual = this.maxAmountCanBePaid - this.registrationEntryState.AmountToPayToday;
                        var bounded = actual < 0 ? 0 : actual > this.maxAmountCanBePaid ? this.maxAmountCanBePaid : actual;
                        return bounded;
                    },
                    amountRemainingFormatted: function () {
                        return "$" + Number_1.asFormattedString(this.amountRemaining);
                    },
                    amountToPayTodayRules: function () {
                        var rules = ['required'];
                        var min = this.amountDueToday;
                        var max = this.maxAmountCanBePaid;
                        if (min > max) {
                            min = max;
                        }
                        rules.push("gte:" + min);
                        rules.push("lte:" + max);
                        return Index_1.ruleArrayToString(rules);
                    },
                },
                methods: {
                    fetchData: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var response;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        this.isLoading = true;
                                        this.lineItems = [];
                                        _a.label = 1;
                                    case 1:
                                        _a.trys.push([1, , 3, 4]);
                                        return [4, this.invokeBlockAction('CalculateCost', {
                                                args: this.getRegistrationEntryBlockArgs()
                                            })];
                                    case 2:
                                        response = _a.sent();
                                        if (response.data) {
                                            this.lineItems = response.data;
                                        }
                                        return [3, 4];
                                    case 3:
                                        this.isLoading = false;
                                        return [7];
                                    case 4: return [2];
                                }
                            });
                        });
                    }
                },
                created: function () {
                    return __awaiter(this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0: return [4, this.fetchData()];
                                case 1:
                                    _a.sent();
                                    return [2];
                            }
                        });
                    });
                },
                watch: {
                    defaultPaymentAmount: {
                        immediate: true,
                        handler: function () {
                            this.registrationEntryState.AmountToPayToday = this.defaultPaymentAmount;
                        }
                    },
                    'registrationEntryState.DiscountCode': function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.fetchData()];
                                    case 1:
                                        _a.sent();
                                        return [2];
                                }
                            });
                        });
                    }
                },
                template: "\n<Loading :isLoading=\"isLoading\">\n    <div class=\"fee-table\">\n        <div class=\"row hidden-xs fee-header\">\n            <div class=\"col-sm-6\">\n                <strong>Description</strong>\n            </div>\n            <div v-if=\"hasDiscount\" class=\"col-sm-3 fee-value\">\n                <strong>Discounted Amount</strong>\n            </div>\n            <div class=\"col-sm-3 fee-value\">\n                <strong>Amount</strong>\n            </div>\n        </div>\n        <div v-for=\"lineItem in augmentedLineItems\" class=\"row\" :class=\"lineItem.IsFee ? 'fee-row-fee' : 'fee-row-cost'\">\n            <div class=\"col-sm-6 fee-caption\">\n                {{lineItem.Description}}\n            </div>\n            <div v-if=\"hasDiscount\" class=\"col-sm-3 fee-value\">\n                <HelpBlock v-if=\"lineItem.DiscountHelp\" :text=\"lineItem.DiscountHelp\" />\n                <span class=\"visible-xs-inline\">Discounted Amount:</span>\n                $ {{lineItem.DiscountedAmountFormatted}}\n            </div>\n            <div class=\"col-sm-3 fee-value\">\n                <span class=\"visible-xs-inline\">Amount:</span>\n                $ {{lineItem.AmountFormatted}}\n            </div>\n        </div>\n        <div class=\"row fee-row-total\">\n            <div class=\"col-sm-6 fee-caption\">\n                Total\n            </div>\n            <div v-if=\"hasDiscount\" class=\"col-sm-3 fee-value\">\n                <span class=\"visible-xs-inline\">Discounted Amount:</span>\n                {{discountedTotalFormatted}}\n            </div>\n            <div class=\"col-sm-3 fee-value\">\n                <span class=\"visible-xs-inline\">Amount:</span>\n                {{totalFormatted}}\n            </div>\n        </div>\n    </div>\n    <div class=\"row fee-totals\">\n        <div class=\"col-sm-offset-8 col-sm-4 fee-totals-options\">\n            <div class=\"form-group static-control\">\n                <label class=\"control-label\">Total Cost</label>\n                <div class=\"control-wrapper\">\n                    <div class=\"form-control-static\">\n                        {{discountedTotalFormatted}}\n                    </div>\n                </div>\n            </div>\n            <div v-if=\"amountPreviouslyPaid\" class=\"form-group static-control\">\n                <label class=\"control-label\">Previously Paid</label>\n                <div class=\"control-wrapper\">\n                    <div class=\"form-control-static\">\n                        {{amountPreviouslyPaidFormatted}}\n                    </div>\n                </div>\n            </div>\n            <template v-if=\"showAmountDueToday && maxAmountCanBePaid\">\n                <div class=\"form-group static-control\">\n                    <label class=\"control-label\">Minimum Due Today</label>\n                    <div class=\"control-wrapper\">\n                        <div class=\"form-control-static\">\n                            {{amountDueTodayFormatted}}\n                        </div>\n                    </div>\n                </div>\n                <CurrencyBox label=\"Amount To Pay Today\" :rules=\"amountToPayTodayRules\" v-model=\"registrationEntryState.AmountToPayToday\" class=\"form-right\" inputGroupClasses=\"input-width-md amount-to-pay\" />\n                <div class=\"form-group static-control\">\n                    <label class=\"control-label\">Amount Remaining</label>\n                    <div class=\"control-wrapper\">\n                        <div class=\"form-control-static\">\n                            {{amountRemainingFormatted}}\n                        </div>\n                    </div>\n                </div>\n            </template>\n            <div v-else class=\"form-group static-control\">\n                <label class=\"control-label\">Amount Due</label>\n                <div class=\"control-wrapper\">\n                    <div class=\"form-control-static\">\n                        {{maxAmountCanBePaidFormatted}}\n                    </div>\n                </div>\n            </div>\n        </div>\n    </div>\n</Loading>"
            }));
        }
    };
});
//# sourceMappingURL=CostSummary.js.map