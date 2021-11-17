System.register(["vue", "../../../Controls/gatewayControl", "../../../Util/block", "../../../Controls/rockForm", "../../../Controls/rockValidation", "../../../Elements/alert", "../../../Elements/checkBox", "../../../Elements/emailBox", "../../../Elements/rockButton", "../../../Elements/dropDownList", "../registrationEntry", "./costSummary", "./discountCodeForm", "./registrar", "../../../Util/guid"], function (exports_1, context_1) {
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
    var vue_1, gatewayControl_1, block_1, rockForm_1, rockValidation_1, alert_1, checkBox_1, emailBox_1, rockButton_1, dropDownList_1, registrationEntry_1, costSummary_1, discountCodeForm_1, registrar_1, guid_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (gatewayControl_1_1) {
                gatewayControl_1 = gatewayControl_1_1;
            },
            function (block_1_1) {
                block_1 = block_1_1;
            },
            function (rockForm_1_1) {
                rockForm_1 = rockForm_1_1;
            },
            function (rockValidation_1_1) {
                rockValidation_1 = rockValidation_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (checkBox_1_1) {
                checkBox_1 = checkBox_1_1;
            },
            function (emailBox_1_1) {
                emailBox_1 = emailBox_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (registrationEntry_1_1) {
                registrationEntry_1 = registrationEntry_1_1;
            },
            function (costSummary_1_1) {
                costSummary_1 = costSummary_1_1;
            },
            function (discountCodeForm_1_1) {
                discountCodeForm_1 = discountCodeForm_1_1;
            },
            function (registrar_1_1) {
                registrar_1 = registrar_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.Summary",
                components: {
                    RockButton: rockButton_1.default,
                    CheckBox: checkBox_1.default,
                    EmailBox: emailBox_1.default,
                    RockForm: rockForm_1.default,
                    Alert: alert_1.default,
                    DropDownList: dropDownList_1.default,
                    GatewayControl: gatewayControl_1.default,
                    RockValidation: rockValidation_1.default,
                    CostSummary: costSummary_1.default,
                    Registrar: registrar_1.default,
                    DiscountCodeForm: discountCodeForm_1.default
                },
                setup() {
                    const submitPayment = gatewayControl_1.prepareSubmitPayment();
                    const getRegistrationEntryBlockArgs = vue_1.inject("getRegistrationEntryBlockArgs");
                    const invokeBlockAction = block_1.useInvokeBlockAction();
                    const registrationEntryState = vue_1.inject("registrationEntryState");
                    const loading = vue_1.ref(false);
                    const gatewayErrorMessage = vue_1.ref("");
                    const gatewayValidationFields = vue_1.ref({});
                    const submitErrorMessage = vue_1.ref("");
                    const selectedSavedAccount = vue_1.ref("");
                    if (registrationEntryState.viewModel.savedAccounts !== null && registrationEntryState.viewModel.savedAccounts.length > 0) {
                        selectedSavedAccount.value = registrationEntryState.viewModel.savedAccounts[0].value;
                    }
                    return {
                        loading,
                        gatewayErrorMessage,
                        gatewayValidationFields,
                        submitErrorMessage,
                        selectedSavedAccount,
                        submitPayment,
                        getRegistrationEntryBlockArgs,
                        invokeBlockAction,
                        registrationEntryState: registrationEntryState
                    };
                },
                computed: {
                    gatewayControlModel() {
                        return this.viewModel.gatewayControl;
                    },
                    viewModel() {
                        return this.registrationEntryState.viewModel;
                    },
                    registrantInfos() {
                        return this.registrationEntryState.registrants.map(r => registrationEntry_1.getRegistrantBasicInfo(r, this.viewModel.registrantForms));
                    },
                    registrantTerm() {
                        return this.registrantInfos.length === 1 ? this.viewModel.registrantTerm : this.viewModel.pluralRegistrantTerm;
                    },
                    instanceName() {
                        return this.viewModel.instanceName;
                    },
                    finishButtonText() {
                        return (this.viewModel.isRedirectGateway && this.registrationEntryState.amountToPayToday) ? "Pay" : "Finish";
                    },
                    hasSavedAccounts() {
                        return this.registrationEntryState.viewModel.savedAccounts !== null
                            && this.registrationEntryState.viewModel.savedAccounts.length > 0;
                    },
                    savedAccountOptions() {
                        if (this.registrationEntryState.viewModel.savedAccounts === null) {
                            return [];
                        }
                        const options = this.registrationEntryState.viewModel.savedAccounts.map(a => {
                            const option = {
                                value: a.value,
                                text: a.text
                            };
                            return option;
                        });
                        options.push({
                            value: "",
                            text: "Use a different payment method"
                        });
                        return options;
                    },
                    showGateway() {
                        return !this.hasSavedAccounts || this.selectedSavedAccount === "";
                    }
                },
                methods: {
                    onPrevious() {
                        this.$emit("previous");
                    },
                    onNext() {
                        return __awaiter(this, void 0, void 0, function* () {
                            this.loading = true;
                            if (this.registrationEntryState.amountToPayToday) {
                                if (this.viewModel.isRedirectGateway) {
                                    const redirectUrl = yield this.getPaymentRedirect();
                                    if (redirectUrl) {
                                        location.href = redirectUrl;
                                    }
                                    else {
                                        this.loading = false;
                                    }
                                }
                                else if (this.showGateway) {
                                    this.gatewayErrorMessage = "";
                                    this.gatewayValidationFields = {};
                                    this.submitPayment();
                                }
                                else if (this.selectedSavedAccount !== "") {
                                    this.registrationEntryState.savedAccountGuid = guid_1.toGuidOrNull(this.selectedSavedAccount);
                                    const success = yield this.submit();
                                    this.loading = false;
                                    if (success) {
                                        this.$emit("next");
                                    }
                                }
                                else {
                                    this.submitErrorMessage = "Please select a valid payment option.";
                                    this.loading = false;
                                    return;
                                }
                            }
                            else {
                                const success = yield this.submit();
                                this.loading = false;
                                if (success) {
                                    this.$emit("next");
                                }
                            }
                        });
                    },
                    onGatewayControlSuccess(token) {
                        return __awaiter(this, void 0, void 0, function* () {
                            this.registrationEntryState.gatewayToken = token;
                            const success = yield this.submit();
                            this.loading = false;
                            if (success) {
                                this.$emit("next");
                            }
                        });
                    },
                    onGatewayControlError(message) {
                        this.loading = false;
                        this.gatewayErrorMessage = message;
                    },
                    onGatewayControlValidation(invalidFields) {
                        this.loading = false;
                        this.gatewayValidationFields = invalidFields;
                    },
                    submit() {
                        return __awaiter(this, void 0, void 0, function* () {
                            this.submitErrorMessage = "";
                            const result = yield this.invokeBlockAction("SubmitRegistration", {
                                args: this.getRegistrationEntryBlockArgs()
                            });
                            if (result.isError || !result.data) {
                                this.submitErrorMessage = result.errorMessage || "Unknown error";
                            }
                            else {
                                this.registrationEntryState.successViewModel = result.data;
                            }
                            return result.isSuccess;
                        });
                    },
                    getPaymentRedirect() {
                        return __awaiter(this, void 0, void 0, function* () {
                            const result = yield this.invokeBlockAction("GetPaymentRedirect", {
                                args: this.getRegistrationEntryBlockArgs()
                            });
                            if (result.isError || !result.data) {
                                this.submitErrorMessage = result.errorMessage || "Unknown error";
                            }
                            return result.data || "";
                        });
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

        <div v-if="gatewayControlModel" v-show="registrationEntryState.amountToPayToday" class="well">
            <h4>Payment Method</h4>
            <DropDownList v-if="hasSavedAccounts" v-model="selectedSavedAccount" :options="savedAccountOptions" :showBlankItem="false" />
            <div v-show="showGateway">
                <Alert v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</Alert>
                <RockValidation :errors="gatewayValidationFields" />
                <div class="hosted-payment-control">
                    <GatewayControl
                        :gatewayControlModel="gatewayControlModel"
                        @success="onGatewayControlSuccess"
                        @error="onGatewayControlError"
                        @validation="onGatewayControlValidation" />
                </div>
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
            }));
        }
    };
});
//# sourceMappingURL=summary.js.map