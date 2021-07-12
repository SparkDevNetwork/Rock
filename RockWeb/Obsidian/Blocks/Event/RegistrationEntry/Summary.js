System.register(["vue", "../../../Controls/GatewayControl", "../../../Controls/RockForm", "../../../Controls/RockValidation", "../../../Elements/Alert", "../../../Elements/CheckBox", "../../../Elements/EmailBox", "../../../Elements/RockButton", "../RegistrationEntry", "./CostSummary", "./DiscountCodeForm", "./Registrar"], function (exports_1, context_1) {
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
    var vue_1, GatewayControl_1, RockForm_1, RockValidation_1, Alert_1, CheckBox_1, EmailBox_1, RockButton_1, RegistrationEntry_1, CostSummary_1, DiscountCodeForm_1, Registrar_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (GatewayControl_1_1) {
                GatewayControl_1 = GatewayControl_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (RockValidation_1_1) {
                RockValidation_1 = RockValidation_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (CheckBox_1_1) {
                CheckBox_1 = CheckBox_1_1;
            },
            function (EmailBox_1_1) {
                EmailBox_1 = EmailBox_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (RegistrationEntry_1_1) {
                RegistrationEntry_1 = RegistrationEntry_1_1;
            },
            function (CostSummary_1_1) {
                CostSummary_1 = CostSummary_1_1;
            },
            function (DiscountCodeForm_1_1) {
                DiscountCodeForm_1 = DiscountCodeForm_1_1;
            },
            function (Registrar_1_1) {
                Registrar_1 = Registrar_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Summary',
                components: {
                    RockButton: RockButton_1.default,
                    CheckBox: CheckBox_1.default,
                    EmailBox: EmailBox_1.default,
                    RockForm: RockForm_1.default,
                    Alert: Alert_1.default,
                    GatewayControl: GatewayControl_1.default,
                    RockValidation: RockValidation_1.default,
                    CostSummary: CostSummary_1.default,
                    Registrar: Registrar_1.default,
                    DiscountCodeForm: DiscountCodeForm_1.default
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
                        loading: false,
                        doGatewayControlSubmit: false,
                        gatewayErrorMessage: '',
                        gatewayValidationFields: {},
                        submitErrorMessage: ''
                    };
                },
                computed: {
                    gatewayControlModel: function () {
                        return this.viewModel.gatewayControl;
                    },
                    viewModel: function () {
                        return this.registrationEntryState.ViewModel;
                    },
                    registrantInfos: function () {
                        var _this = this;
                        return this.registrationEntryState.Registrants.map(function (r) { return RegistrationEntry_1.getRegistrantBasicInfo(r, _this.viewModel.registrantForms); });
                    },
                    registrantTerm: function () {
                        return this.registrantInfos.length === 1 ? this.viewModel.registrantTerm : this.viewModel.pluralRegistrantTerm;
                    },
                    instanceName: function () {
                        return this.viewModel.instanceName;
                    },
                    finishButtonText: function () {
                        return (this.viewModel.isRedirectGateway && this.registrationEntryState.AmountToPayToday) ? 'Pay' : 'Finish';
                    }
                },
                methods: {
                    onPrevious: function () {
                        this.$emit('previous');
                    },
                    onNext: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var redirectUrl, success;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        this.loading = true;
                                        if (!this.registrationEntryState.AmountToPayToday) return [3, 4];
                                        if (!this.viewModel.isRedirectGateway) return [3, 2];
                                        return [4, this.getPaymentRedirect()];
                                    case 1:
                                        redirectUrl = _a.sent();
                                        if (redirectUrl) {
                                            location.href = redirectUrl;
                                        }
                                        else {
                                            this.loading = false;
                                        }
                                        return [3, 3];
                                    case 2:
                                        this.gatewayErrorMessage = '';
                                        this.gatewayValidationFields = {};
                                        this.doGatewayControlSubmit = true;
                                        _a.label = 3;
                                    case 3: return [3, 6];
                                    case 4: return [4, this.submit()];
                                    case 5:
                                        success = _a.sent();
                                        this.loading = false;
                                        if (success) {
                                            this.$emit('next');
                                        }
                                        _a.label = 6;
                                    case 6: return [2];
                                }
                            });
                        });
                    },
                    onGatewayControlSuccess: function (token) {
                        return __awaiter(this, void 0, void 0, function () {
                            var success;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        this.registrationEntryState.GatewayToken = token;
                                        return [4, this.submit()];
                                    case 1:
                                        success = _a.sent();
                                        this.loading = false;
                                        if (success) {
                                            this.$emit('next');
                                        }
                                        return [2];
                                }
                            });
                        });
                    },
                    onGatewayControlReset: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                this.registrationEntryState.GatewayToken = '';
                                this.doGatewayControlSubmit = false;
                                return [2];
                            });
                        });
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
                    submit: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var result;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.invokeBlockAction('SubmitRegistration', {
                                            args: this.getRegistrationEntryBlockArgs()
                                        })];
                                    case 1:
                                        result = _a.sent();
                                        if (result.isError || !result.data) {
                                            this.submitErrorMessage = result.errorMessage || 'Unknown error';
                                        }
                                        else {
                                            this.registrationEntryState.SuccessViewModel = result.data;
                                        }
                                        return [2, result.isSuccess];
                                }
                            });
                        });
                    },
                    getPaymentRedirect: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var result;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.invokeBlockAction('GetPaymentRedirect', {
                                            args: this.getRegistrationEntryBlockArgs()
                                        })];
                                    case 1:
                                        result = _a.sent();
                                        if (result.isError || !result.data) {
                                            this.submitErrorMessage = result.errorMessage || 'Unknown error';
                                        }
                                        return [2, result.data || ''];
                                }
                            });
                        });
                    }
                },
                template: "\n<div class=\"registrationentry-summary\">\n    <RockForm @submit=\"onNext\">\n\n        <Registrar />\n\n        <div v-if=\"viewModel.cost\">\n            <h4>Payment Summary</h4>\n            <DiscountCodeForm />\n            <CostSummary />\n        </div>\n\n        <div v-if=\"gatewayControlModel && registrationEntryState.AmountToPayToday\" class=\"well\">\n            <h4>Payment Method</h4>\n            <Alert v-if=\"gatewayErrorMessage\" alertType=\"danger\">{{gatewayErrorMessage}}</Alert>\n            <RockValidation :errors=\"gatewayValidationFields\" />\n            <div class=\"hosted-payment-control\">\n                <GatewayControl\n                    :gatewayControlModel=\"gatewayControlModel\"\n                    :submit=\"doGatewayControlSubmit\"\n                    @success=\"onGatewayControlSuccess\"\n                    @reset=\"onGatewayControlReset\"\n                    @error=\"onGatewayControlError\"\n                    @validation=\"onGatewayControlValidation\" />\n            </div>\n        </div>\n\n        <div v-if=\"!viewModel.cost\" class=\"margin-b-md\">\n            <p>The following {{registrantTerm}} will be registered for {{instanceName}}:</p>\n            <ul>\n                <li v-for=\"r in registrantInfos\" :key=\"r.Guid\">\n                    <strong>{{r.FirstName}} {{r.LastName}}</strong>\n                </li>\n            </ul>\n        </div>\n\n        <Alert v-if=\"submitErrorMessage\" alertType=\"danger\">{{submitErrorMessage}}</Alert>\n\n        <div class=\"actions text-right\">\n            <RockButton v-if=\"viewModel.allowRegistrationUpdates\" class=\"pull-left\" btnType=\"default\" @click=\"onPrevious\" :isLoading=\"loading\">\n                Previous\n            </RockButton>\n            <RockButton btnType=\"primary\" type=\"submit\" :isLoading=\"loading\">\n                {{finishButtonText}}\n            </RockButton>\n        </div>\n    </RockForm>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Summary.js.map