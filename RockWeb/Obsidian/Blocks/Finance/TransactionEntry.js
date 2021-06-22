System.register(["../../Controls/CampusPicker", "../../Controls/DefinedValuePicker", "../../Elements/CurrencyBox", "vue", "../../SystemGuid/DefinedType", "../../Elements/DatePicker", "../../Elements/RockButton", "../../Util/Guid", "../../Elements/Alert", "../../Services/Number", "../../Elements/Toggle", "../../Store/Index", "../../Elements/TextBox", "../../Services/String", "../../Util/RockDate", "../../Controls/GatewayControl", "../../Controls/RockValidation"], function (exports_1, context_1) {
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
    var CampusPicker_1, DefinedValuePicker_1, CurrencyBox_1, vue_1, DefinedType_1, DatePicker_1, RockButton_1, Guid_1, Alert_1, Number_1, Toggle_1, Index_1, TextBox_1, String_1, RockDate_1, GatewayControl_1, RockValidation_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (CampusPicker_1_1) {
                CampusPicker_1 = CampusPicker_1_1;
            },
            function (DefinedValuePicker_1_1) {
                DefinedValuePicker_1 = DefinedValuePicker_1_1;
            },
            function (CurrencyBox_1_1) {
                CurrencyBox_1 = CurrencyBox_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DefinedType_1_1) {
                DefinedType_1 = DefinedType_1_1;
            },
            function (DatePicker_1_1) {
                DatePicker_1 = DatePicker_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (Toggle_1_1) {
                Toggle_1 = Toggle_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (RockDate_1_1) {
                RockDate_1 = RockDate_1_1;
            },
            function (GatewayControl_1_1) {
                GatewayControl_1 = GatewayControl_1_1;
            },
            function (RockValidation_1_1) {
                RockValidation_1 = RockValidation_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Finance.TransactionEntry',
                components: {
                    CurrencyBox: CurrencyBox_1.default,
                    CampusPicker: CampusPicker_1.default,
                    DefinedValuePicker: DefinedValuePicker_1.default,
                    DatePicker: DatePicker_1.default,
                    RockButton: RockButton_1.default,
                    Alert: Alert_1.default,
                    Toggle: Toggle_1.default,
                    TextBox: TextBox_1.default,
                    GatewayControl: GatewayControl_1.default,
                    RockValidation: RockValidation_1.default
                },
                setup: function () {
                    return {
                        invokeBlockAction: vue_1.inject('invokeBlockAction'),
                        configurationValues: vue_1.inject('configurationValues')
                    };
                },
                data: function () {
                    return {
                        loading: false,
                        gatewayErrorMessage: '',
                        gatewayValidationFields: {},
                        transactionGuid: Guid_1.newGuid(),
                        criticalError: '',
                        doGatewayControlSubmit: false,
                        pageIndex: 1,
                        page1Error: '',
                        frequencyDefinedTypeGuid: DefinedType_1.FINANCIAL_FREQUENCY,
                        args: {
                            IsGivingAsPerson: true,
                            Email: '',
                            PhoneNumber: '',
                            PhoneCountryCode: '',
                            AccountAmounts: {},
                            Street1: '',
                            Street2: '',
                            City: '',
                            State: '',
                            PostalCode: '',
                            Country: '',
                            FirstName: '',
                            LastName: '',
                            BusinessName: '',
                            FinancialPersonSavedAccountGuid: null,
                            Comment: '',
                            TransactionEntityId: null,
                            ReferenceNumber: '',
                            CampusGuid: '',
                            BusinessGuid: null,
                            FrequencyValueGuid: '',
                            GiftDate: RockDate_1.default.newDate(),
                            IsGiveAnonymously: false
                        }
                    };
                },
                computed: {
                    totalAmount: function () {
                        var total = 0;
                        for (var accountGuid in this.args.AccountAmounts) {
                            total += this.args.AccountAmounts[accountGuid];
                        }
                        return total;
                    },
                    totalAmountFormatted: function () {
                        return "$" + Number_1.asFormattedString(this.totalAmount);
                    },
                    gatewayControlModel: function () {
                        return this.configurationValues['GatewayControl'];
                    },
                    currentPerson: function () {
                        return Index_1.default.state.currentPerson;
                    },
                    accounts: function () {
                        return this.configurationValues['FinancialAccounts'] || [];
                    },
                    campus: function () {
                        return Index_1.default.getters['campuses/getByGuid'](this.args.CampusGuid) || null;
                    },
                    accountAndCampusString: function () {
                        var accountNames = [];
                        var _loop_1 = function (accountGuid) {
                            var account = this_1.accounts.find(function (a) { return Guid_1.areEqual(accountGuid, a.Guid); });
                            if (!account || !account.PublicName) {
                                return "continue";
                            }
                            accountNames.push(account.PublicName);
                        };
                        var this_1 = this;
                        for (var accountGuid in this.args.AccountAmounts) {
                            _loop_1(accountGuid);
                        }
                        if (this.campus) {
                            return String_1.asCommaAnd(accountNames) + " - " + this.campus.Name;
                        }
                        return String_1.asCommaAnd(accountNames);
                    }
                },
                methods: {
                    goBack: function () {
                        this.pageIndex--;
                        this.doGatewayControlSubmit = false;
                    },
                    onPageOneSubmit: function () {
                        if (this.totalAmount <= 0) {
                            this.page1Error = 'Please specify an amount';
                            return;
                        }
                        this.page1Error = '';
                        this.pageIndex = 2;
                    },
                    onPageTwoSubmit: function () {
                        this.loading = true;
                        this.gatewayErrorMessage = '';
                        this.gatewayValidationFields = {};
                        this.doGatewayControlSubmit = true;
                    },
                    onGatewayControlSuccess: function (token) {
                        this.loading = false;
                        this.args.ReferenceNumber = token;
                        this.pageIndex = 3;
                    },
                    onGatewayControlError: function (message) {
                        this.doGatewayControlSubmit = false;
                        this.loading = false;
                        this.gatewayErrorMessage = message;
                    },
                    onGatewayControlValidation: function (invalidFields) {
                        this.doGatewayControlSubmit = false;
                        this.loading = false;
                        this.gatewayValidationFields = invalidFields;
                    },
                    onPageThreeSubmit: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var e_1;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        this.loading = true;
                                        _a.label = 1;
                                    case 1:
                                        _a.trys.push([1, 3, 4, 5]);
                                        return [4, this.invokeBlockAction('ProcessTransaction', {
                                                args: this.args,
                                                transactionGuid: this.transactionGuid
                                            })];
                                    case 2:
                                        _a.sent();
                                        this.pageIndex = 4;
                                        return [3, 5];
                                    case 3:
                                        e_1 = _a.sent();
                                        console.log(e_1);
                                        return [3, 5];
                                    case 4:
                                        this.loading = false;
                                        return [7];
                                    case 5: return [2];
                                }
                            });
                        });
                    }
                },
                watch: {
                    currentPerson: {
                        immediate: true,
                        handler: function () {
                            if (!this.currentPerson) {
                                return;
                            }
                            this.args.FirstName = this.args.FirstName || this.currentPerson.FirstName || '';
                            this.args.LastName = this.args.LastName || this.currentPerson.LastName || '';
                            this.args.Email = this.args.Email || this.currentPerson.Email || '';
                        }
                    }
                },
                template: "\n<div class=\"transaction-entry-v2\">\n    <Alert v-if=\"criticalError\" danger>\n        {{criticalError}}\n    </Alert>\n    <template v-else-if=\"!gatewayControlModel || !gatewayControlModel.FileUrl\">\n        <h4>Welcome to Rock's On-line Giving Experience</h4>\n        <p>\n            There is currently no gateway configured.\n        </p>\n    </template>\n    <template v-else-if=\"pageIndex === 1\">\n        <h2>Your Generosity Changes Lives (Vue)</h2>\n        <template v-for=\"account in accounts\">\n            <CurrencyBox :label=\"account.PublicName\" v-model=\"args.AccountAmounts[account.Guid]\" />\n        </template>\n        <CampusPicker v-model=\"args.CampusGuid\" :showBlankItem=\"false\" />\n        <DefinedValuePicker :definedTypeGuid=\"frequencyDefinedTypeGuid\" v-model=\"args.FrequencyValueGuid\" label=\"Frequency\" :showBlankItem=\"false\" />\n        <DatePicker label=\"Process Gift On\" v-model=\"args.GiftDate\" />\n        <Alert alertType=\"validation\" v-if=\"page1Error\">{{page1Error}}</Alert>\n        <RockButton btnType=\"primary\" @click=\"onPageOneSubmit\">Give Now</RockButton>\n    </template>\n    <template v-else-if=\"pageIndex === 2\">\n        <div class=\"amount-summary\">\n            <div class=\"amount-summary-text\">\n                {{accountAndCampusString}}\n            </div>\n            <div class=\"amount-display\">\n                {{totalAmountFormatted}}\n            </div>\n        </div>\n        <div>\n            <Alert v-if=\"gatewayErrorMessage\" alertType=\"danger\">{{gatewayErrorMessage}}</Alert>\n            <RockValidation :errors=\"gatewayValidationFields\" />\n            <div class=\"hosted-payment-control\">\n                <GatewayControl\n                    :gatewayControlModel=\"gatewayControlModel\"\n                    :submit=\"doGatewayControlSubmit\"\n                    @success=\"onGatewayControlSuccess\"\n                    @error=\"onGatewayControlError\"\n                    @validation=\"onGatewayControlValidation\" />\n            </div>\n            <div class=\"navigation actions\">\n                <RockButton btnType=\"default\" @click=\"goBack\" :isLoading=\"loading\">Back</RockButton>\n                <RockButton btnType=\"primary\" class=\"pull-right\" @click=\"onPageTwoSubmit\" :isLoading=\"loading\">Next</RockButton>\n            </div>\n        </div>\n    </template>\n    <template v-else-if=\"pageIndex === 3\">\n        <Toggle v-model=\"args.IsGivingAsPerson\">\n            <template #on>Individual</template>\n            <template #off>Business</template>\n        </Toggle>\n        <template v-if=\"args.IsGivingAsPerson && currentPerson\">\n            <div class=\"form-control-static\">\n                {{currentPerson.FullName}}\n            </div>\n        </template>\n        <template v-else-if=\"args.IsGivingAsPerson\">\n            <TextBox v-model=\"args.FirstName\" placeholder=\"First Name\" class=\"margin-b-sm\" />\n            <TextBox v-model=\"args.LastName\" placeholder=\"Last Name\" class=\"margin-b-sm\" />\n        </template>\n        <div class=\"navigation actions margin-t-md\">\n            <RockButton :isLoading=\"loading\" @click=\"goBack\">Back</RockButton>\n            <RockButton :isLoading=\"loading\" btnType=\"primary\" class=\"pull-right\" @click=\"onPageThreeSubmit\">Finish</RockButton>\n        </div>\n    </template>\n    <template v-else-if=\"pageIndex === 4\">\n        Last Page\n    </template>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=TransactionEntry.js.map