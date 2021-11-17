System.register(["../../Elements/dropDownList", "../../Elements/currencyBox", "vue", "../../Elements/datePicker", "../../Elements/rockButton", "../../Util/guid", "../../Util/rockDateTime", "../../Elements/alert", "../../Services/number", "../../Elements/toggle", "../../Store/index", "../../Elements/textBox", "../../Services/string", "../../Controls/gatewayControl", "../../Controls/rockValidation"], function (exports_1, context_1) {
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
    var dropDownList_1, currencyBox_1, vue_1, datePicker_1, rockButton_1, guid_1, rockDateTime_1, alert_1, number_1, toggle_1, index_1, textBox_1, string_1, gatewayControl_1, rockValidation_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (currencyBox_1_1) {
                currencyBox_1 = currencyBox_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (datePicker_1_1) {
                datePicker_1 = datePicker_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (toggle_1_1) {
                toggle_1 = toggle_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            },
            function (gatewayControl_1_1) {
                gatewayControl_1 = gatewayControl_1_1;
            },
            function (rockValidation_1_1) {
                rockValidation_1 = rockValidation_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "Finance.TransactionEntry",
                components: {
                    CurrencyBox: currencyBox_1.default,
                    DropDownList: dropDownList_1.default,
                    DatePicker: datePicker_1.default,
                    RockButton: rockButton_1.default,
                    Alert: alert_1.default,
                    Toggle: toggle_1.default,
                    TextBox: textBox_1.default,
                    GatewayControl: gatewayControl_1.default,
                    RockValidation: rockValidation_1.default
                },
                setup() {
                    return {
                        invokeBlockAction: vue_1.inject("invokeBlockAction"),
                        configurationValues: vue_1.inject("configurationValues")
                    };
                },
                data() {
                    return {
                        loading: false,
                        gatewayErrorMessage: "",
                        gatewayValidationFields: {},
                        transactionGuid: guid_1.newGuid(),
                        criticalError: "",
                        doGatewayControlSubmit: false,
                        pageIndex: 1,
                        page1Error: "",
                        args: {
                            isGivingAsPerson: true,
                            email: "",
                            phoneNumber: "",
                            phoneCountryCode: "",
                            accountAmounts: {},
                            street1: "",
                            street2: "",
                            city: "",
                            state: "",
                            postalCode: "",
                            country: "",
                            firstName: "",
                            lastName: "",
                            businessName: "",
                            financialPersonSavedAccountGuid: null,
                            comment: "",
                            transactionEntityId: null,
                            referenceNumber: "",
                            campusGuid: "",
                            businessGuid: null,
                            frequencyValueGuid: "",
                            giftDate: rockDateTime_1.RockDateTime.now().toASPString("yyyy-MM-dd"),
                            isGiveAnonymously: false
                        }
                    };
                },
                computed: {
                    totalAmount() {
                        let total = 0;
                        for (const accountGuid in this.args.accountAmounts) {
                            total += this.args.accountAmounts[accountGuid];
                        }
                        return total;
                    },
                    totalAmountFormatted() {
                        return `$${number_1.asFormattedString(this.totalAmount, 2)}`;
                    },
                    gatewayControlModel() {
                        return this.configurationValues["gatewayControl"];
                    },
                    currentPerson() {
                        return store.state.currentPerson;
                    },
                    accounts() {
                        return this.configurationValues["financialAccounts"] || [];
                    },
                    campuses() {
                        return this.configurationValues["campuses"] || [];
                    },
                    frequencies() {
                        return this.configurationValues["frequencies"] || [];
                    },
                    campusName() {
                        if (this.args.campusGuid === null) {
                            return null;
                        }
                        const matchedCampuses = this.campuses.filter(c => c.value === this.args.campusGuid);
                        return matchedCampuses.length >= 1 ? matchedCampuses[0].text : null;
                    },
                    accountAndCampusString() {
                        const accountNames = [];
                        for (const accountGuid in this.args.accountAmounts) {
                            const account = this.accounts.find(a => guid_1.areEqual(accountGuid, a.guid));
                            if (!account || !account.publicName) {
                                continue;
                            }
                            accountNames.push(account.publicName);
                        }
                        if (this.campusName) {
                            return `${string_1.asCommaAnd(accountNames)} - ${this.campusName}`;
                        }
                        return string_1.asCommaAnd(accountNames);
                    }
                },
                methods: {
                    goBack() {
                        this.pageIndex--;
                        this.doGatewayControlSubmit = false;
                    },
                    onPageOneSubmit() {
                        if (this.totalAmount <= 0) {
                            this.page1Error = "Please specify an amount";
                            return;
                        }
                        this.page1Error = "";
                        this.pageIndex = 2;
                    },
                    onPageTwoSubmit() {
                        this.loading = true;
                        this.gatewayErrorMessage = "";
                        this.gatewayValidationFields = {};
                        this.doGatewayControlSubmit = true;
                    },
                    onGatewayControlSuccess(token) {
                        this.loading = false;
                        this.args.referenceNumber = token;
                        this.pageIndex = 3;
                    },
                    onGatewayControlError(message) {
                        this.doGatewayControlSubmit = false;
                        this.loading = false;
                        this.gatewayErrorMessage = message;
                    },
                    onGatewayControlValidation(invalidFields) {
                        this.doGatewayControlSubmit = false;
                        this.loading = false;
                        this.gatewayValidationFields = invalidFields;
                    },
                    onPageThreeSubmit() {
                        return __awaiter(this, void 0, void 0, function* () {
                            this.loading = true;
                            try {
                                yield this.invokeBlockAction("ProcessTransaction", {
                                    args: this.args,
                                    transactionGuid: this.transactionGuid
                                });
                                this.pageIndex = 4;
                            }
                            catch (e) {
                                console.log(e);
                            }
                            finally {
                                this.loading = false;
                            }
                        });
                    }
                },
                watch: {
                    currentPerson: {
                        immediate: true,
                        handler() {
                            if (!this.currentPerson) {
                                return;
                            }
                            this.args.firstName = this.args.firstName || this.currentPerson.firstName || "";
                            this.args.lastName = this.args.lastName || this.currentPerson.lastName || "";
                            this.args.email = this.args.email || this.currentPerson.email || "";
                        }
                    }
                },
                template: `
<div class="transaction-entry-v2">
    <Alert v-if="criticalError" danger>
        {{criticalError}}
    </Alert>
    <template v-else-if="!gatewayControlModel || !gatewayControlModel.fileUrl">
        <h4>Welcome to Rock's On-line Giving Experience</h4>
        <p>
            There is currently no gateway configured.
        </p>
    </template>
    <template v-else-if="pageIndex === 1">
        <h2>Your Generosity Changes Lives (Vue)</h2>
        <template v-for="account in accounts">
            <CurrencyBox :label="account.publicName" v-model="args.accountAmounts[account.guid]" />
        </template>
        <DropDownList label="Campus" v-model="args.campusGuid" :showBlankItem="false" :options="campuses" />
        <DropDownList label="Frequency" v-model="args.frequencyValueGuid" :showBlankItem="false" :options="frequencies" />
        <DatePicker label="Process Gift On" v-model="args.giftDate" />
        <Alert alertType="validation" v-if="page1Error">{{page1Error}}</Alert>
        <RockButton btnType="primary" @click="onPageOneSubmit">Give Now</RockButton>
    </template>
    <template v-else-if="pageIndex === 2">
        <div class="amount-summary">
            <div class="amount-summary-text">
                {{accountAndCampusString}}
            </div>
            <div class="amount-display">
                {{totalAmountFormatted}}
            </div>
        </div>
        <div>
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
            <div class="navigation actions">
                <RockButton btnType="default" @click="goBack" :isLoading="loading">Back</RockButton>
                <RockButton btnType="primary" class="pull-right" @click="onPageTwoSubmit" :isLoading="loading">Next</RockButton>
            </div>
        </div>
    </template>
    <template v-else-if="pageIndex === 3">
        <Toggle v-model="args.isGivingAsPerson">
            <template #on>Individual</template>
            <template #off>Business</template>
        </Toggle>
        <template v-if="args.isGivingAsPerson && currentPerson">
            <div class="form-control-static">
                {{currentPerson.FullName}}
            </div>
        </template>
        <template v-else-if="args.isGivingAsPerson">
            <TextBox v-model="args.firstName" placeholder="First Name" class="margin-b-sm" />
            <TextBox v-model="args.lastName" placeholder="Last Name" class="margin-b-sm" />
        </template>
        <div class="navigation actions margin-t-md">
            <RockButton :isLoading="loading" @click="goBack">Back</RockButton>
            <RockButton :isLoading="loading" btnType="primary" class="pull-right" @click="onPageThreeSubmit">Finish</RockButton>
        </div>
    </template>
    <template v-else-if="pageIndex === 4">
        Last Page
    </template>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=transactionEntry.js.map