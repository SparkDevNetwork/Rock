System.register(["../../Controls/CampusPicker.js", "../../Controls/DefinedValuePicker.js", "../../Elements/CurrencyBox.js", "../../Vendor/Vue/vue.js", "../../SystemGuid/DefinedType.js", "../../Elements/DatePicker.js", "../../Elements/RockButton.js", "../../Util/Guid.js", "../../Elements/Alert.js", "../../Filters/Number.js", "../../Elements/Toggle.js", "../../Store/Index.js", "../../Elements/TextBox.js", "../../Filters/Date.js", "../../Filters/String.js"], function (exports_1, context_1) {
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
    var CampusPicker_js_1, DefinedValuePicker_js_1, CurrencyBox_js_1, vue_js_1, DefinedType_js_1, DatePicker_js_1, RockButton_js_1, Guid_js_1, Alert_js_1, Number_js_1, Toggle_js_1, Index_js_1, TextBox_js_1, Date_js_1, String_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (CampusPicker_js_1_1) {
                CampusPicker_js_1 = CampusPicker_js_1_1;
            },
            function (DefinedValuePicker_js_1_1) {
                DefinedValuePicker_js_1 = DefinedValuePicker_js_1_1;
            },
            function (CurrencyBox_js_1_1) {
                CurrencyBox_js_1 = CurrencyBox_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (DefinedType_js_1_1) {
                DefinedType_js_1 = DefinedType_js_1_1;
            },
            function (DatePicker_js_1_1) {
                DatePicker_js_1 = DatePicker_js_1_1;
            },
            function (RockButton_js_1_1) {
                RockButton_js_1 = RockButton_js_1_1;
            },
            function (Guid_js_1_1) {
                Guid_js_1 = Guid_js_1_1;
            },
            function (Alert_js_1_1) {
                Alert_js_1 = Alert_js_1_1;
            },
            function (Number_js_1_1) {
                Number_js_1 = Number_js_1_1;
            },
            function (Toggle_js_1_1) {
                Toggle_js_1 = Toggle_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (TextBox_js_1_1) {
                TextBox_js_1 = TextBox_js_1_1;
            },
            function (Date_js_1_1) {
                Date_js_1 = Date_js_1_1;
            },
            function (String_js_1_1) {
                String_js_1 = String_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'Finance.TransactionEntry',
                components: {
                    CurrencyBox: CurrencyBox_js_1.default,
                    CampusPicker: CampusPicker_js_1.default,
                    DefinedValuePicker: DefinedValuePicker_js_1.default,
                    DatePicker: DatePicker_js_1.default,
                    RockButton: RockButton_js_1.default,
                    Alert: Alert_js_1.default,
                    Toggle: Toggle_js_1.default,
                    TextBox: TextBox_js_1.default
                },
                setup: function () {
                    return {
                        invokeBlockAction: vue_js_1.inject('invokeBlockAction'),
                        configurationValues: vue_js_1.inject('configurationValues')
                    };
                },
                data: function () {
                    return {
                        transactionGuid: Guid_js_1.newGuid(),
                        criticalError: '',
                        doGatewayControlSubmit: false,
                        pageIndex: 1,
                        page1Error: '',
                        frequencyDefinedTypeGuid: DefinedType_js_1.FINANCIAL_FREQUENCY,
                        gatewayControl: null,
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
                            GiftDate: Date_js_1.toDatePickerValue(new Date()),
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
                        return "$" + Number_js_1.asFormattedString(this.totalAmount);
                    },
                    gatewayControlSettings: function () {
                        var blockSettings = this.configurationValues || {};
                        return blockSettings['GatewayControlSettings'] || {};
                    },
                    currentPerson: function () {
                        return Index_js_1.default.state.currentPerson;
                    },
                    accounts: function () {
                        return this.configurationValues['FinancialAccounts'] || [];
                    },
                    campus: function () {
                        return Index_js_1.default.getters['campuses/getByGuid'](this.args.CampusGuid) || null;
                    },
                    accountAndCampusString: function () {
                        var accountNames = [];
                        var _loop_1 = function (accountGuid) {
                            var account = this_1.accounts.find(function (a) { return Guid_js_1.areEqual(accountGuid, a.Guid); });
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
                            return String_js_1.asCommaAnd(accountNames) + " - " + this.campus.Name;
                        }
                        return String_js_1.asCommaAnd(accountNames);
                    }
                },
                methods: {
                    goBack: function () {
                        this.pageIndex--;
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
                        this.doGatewayControlSubmit = true;
                    },
                    onGatewayControlDone: function () {
                        this.pageIndex = 3;
                    },
                    onPageThreeSubmit: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var e_1;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        _a.trys.push([0, 2, , 3]);
                                        return [4 /*yield*/, this.invokeBlockAction('ProcessTransaction', {
                                                args: this.args,
                                                transactionGuid: this.transactionGuid
                                            })];
                                    case 1:
                                        _a.sent();
                                        this.pageIndex = 4;
                                        return [3 /*break*/, 3];
                                    case 2:
                                        e_1 = _a.sent();
                                        console.log(e_1);
                                        return [3 /*break*/, 3];
                                    case 3: return [2 /*return*/];
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
                created: function () {
                    return __awaiter(this, void 0, void 0, function () {
                        var controlPath, controlComponentModule, gatewayControl;
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    controlPath = this.configurationValues['GatewayControlFileUrl'];
                                    if (!controlPath) return [3 /*break*/, 2];
                                    return [4 /*yield*/, context_1.import(controlPath)];
                                case 1:
                                    controlComponentModule = _a.sent();
                                    gatewayControl = controlComponentModule ?
                                        (controlComponentModule.default || controlComponentModule) :
                                        null;
                                    if (gatewayControl) {
                                        this.gatewayControl = vue_js_1.markRaw(gatewayControl);
                                    }
                                    _a.label = 2;
                                case 2:
                                    if (!this.gatewayControl) {
                                        this.criticalError = 'Could not find the correct gateway control';
                                    }
                                    return [2 /*return*/];
                            }
                        });
                    });
                },
                template: "\n<div class=\"transaction-entry-v2\">\n    <Alert v-if=\"criticalError\" danger>\n        {{criticalError}}\n    </Alert>\n    <template v-else-if=\"!gatewayControl\">\n        <h4>Welcome to Rock's On-line Giving Experience</h4>\n        <p>\n            There is currently no gateway configured.\n        </p>\n    </template>\n    <template v-else-if=\"pageIndex === 1\">\n        <h2>Your Generosity Changes Lives (Vue)</h2>\n        <template v-for=\"account in accounts\">\n            <CurrencyBox :label=\"account.PublicName\" v-model=\"args.AccountAmounts[account.Guid]\" />\n        </template>\n        <CampusPicker v-model=\"args.CampusGuid\" :showBlankItem=\"false\" />\n        <DefinedValuePicker :definedTypeGuid=\"frequencyDefinedTypeGuid\" v-model=\"args.FrequencyValueGuid\" label=\"Frequency\" :showBlankItem=\"false\" />\n        <DatePicker label=\"Process Gift On\" v-model=\"args.GiftDate\" />\n        <Alert validation v-if=\"page1Error\">{{page1Error}}</Alert>\n        <RockButton primary @click=\"onPageOneSubmit\">Give Now</RockButton>\n    </template>\n    <template v-else-if=\"pageIndex === 2\">\n        <div class=\"amount-summary\">\n            <div class=\"amount-summary-text\">\n                {{accountAndCampusString}}\n            </div>\n            <div class=\"amount-display\">\n                {{totalAmountFormatted}}\n            </div>\n        </div>\n        <div>\n            <div class=\"hosted-payment-control\">\n                <component :is=\"gatewayControl\" :settings=\"gatewayControlSettings\" :submit=\"doGatewayControlSubmit\" :args=\"args\" @done=\"onGatewayControlDone\" />\n            </div>\n            <div class=\"navigation actions\">\n                <RockButton default @click=\"goBack\" :disabled=\"doGatewayControlSubmit\">Back</RockButton>\n                <RockButton primary class=\"pull-right\" @click=\"onPageTwoSubmit\" :disabled=\"doGatewayControlSubmit\">Next</RockButton>\n            </div>\n        </div>\n    </template>\n    <template v-else-if=\"pageIndex === 3\">\n        <Toggle v-model=\"args.IsGivingAsPerson\">\n            <template #on>Individual</template>\n            <template #off>Business</template>\n        </Toggle>\n        <template v-if=\"args.IsGivingAsPerson && currentPerson\">\n            <div class=\"form-control-static\">\n                {{currentPerson.FullName}}\n            </div>\n        </template>\n        <template v-else-if=\"args.IsGivingAsPerson\">\n            <TextBox v-model=\"args.FirstName\" placeholder=\"First Name\" class=\"margin-b-sm\" />\n            <TextBox v-model=\"args.LastName\" placeholder=\"Last Name\" class=\"margin-b-sm\" />\n        </template>\n        <div class=\"navigation actions margin-t-md\">\n            <RockButton default @click=\"goBack\">Back</RockButton>\n            <RockButton primary class=\"pull-right\" @click=\"onPageThreeSubmit\">Finish</RockButton>\n        </div>\n    </template>\n    <template v-else-if=\"pageIndex === 4\">\n        Last Page\n    </template>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=TransactionEntry.js.map