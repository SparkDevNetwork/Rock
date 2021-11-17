System.register(["vue", "../Elements/alert", "../Elements/checkBox", "../Elements/rockButton", "../Elements/textBox", "./rockForm", "../Store/index"], function (exports_1, context_1) {
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
    var vue_1, alert_1, checkBox_1, rockButton_1, textBox_1, rockForm_1, index_1, store, SaveFinancialAccountForm;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (checkBox_1_1) {
                checkBox_1 = checkBox_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (rockForm_1_1) {
                rockForm_1 = rockForm_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            SaveFinancialAccountForm = vue_1.defineComponent({
                name: "SaveFinancialAccountForm",
                components: {
                    CheckBox: checkBox_1.default,
                    TextBox: textBox_1.default,
                    Alert: alert_1.default,
                    RockButton: rockButton_1.default,
                    RockForm: rockForm_1.default
                },
                props: {
                    gatewayGuid: {
                        type: String,
                        required: true
                    },
                    transactionCode: {
                        type: String,
                        required: true
                    },
                    gatewayPersonIdentifier: {
                        type: String,
                        required: true
                    }
                },
                setup() {
                    return {
                        http: vue_1.inject("http")
                    };
                },
                data() {
                    return {
                        doSave: false,
                        username: "",
                        password: "",
                        confirmPassword: "",
                        savedAccountName: "",
                        isLoading: false,
                        successTitle: "",
                        successMessage: "",
                        errorTitle: "",
                        errorMessage: ""
                    };
                },
                computed: {
                    currentPerson() {
                        return store.state.currentPerson;
                    },
                    isLoginCreationNeeded() {
                        return !this.currentPerson;
                    },
                },
                methods: {
                    onSubmit() {
                        var _a, _b, _c;
                        return __awaiter(this, void 0, void 0, function* () {
                            this.errorTitle = "";
                            this.errorMessage = "";
                            if (this.password !== this.confirmPassword) {
                                this.errorTitle = "Password";
                                this.errorMessage = "The password fields do not match.";
                                return;
                            }
                            this.isLoading = true;
                            const result = yield this.http.post(`api/v2/controls/savefinancialaccountforms/${this.gatewayGuid}`, null, {
                                Password: this.password,
                                SavedAccountName: this.savedAccountName,
                                TransactionCode: this.transactionCode,
                                Username: this.username,
                                GatewayPersonIdentifier: this.gatewayPersonIdentifier
                            });
                            if ((_a = result === null || result === void 0 ? void 0 : result.data) === null || _a === void 0 ? void 0 : _a.isSuccess) {
                                this.successTitle = result.data.title;
                                this.successMessage = result.data.detail || "Success";
                            }
                            else {
                                this.errorTitle = ((_b = result.data) === null || _b === void 0 ? void 0 : _b.title) || "";
                                this.errorMessage = ((_c = result.data) === null || _c === void 0 ? void 0 : _c.detail) || "Error";
                            }
                            this.isLoading = false;
                        });
                    }
                },
                template: `
<div>
    <Alert v-if="successMessage" alertType="success" class="m-0">
        <strong v-if="successTitle">{{successTitle}}:</strong>
        {{successMessage}}
    </Alert>
    <template v-else>
        <slot name="header">
            <h3>Make Giving Even Easier</h3>
        </slot>
        <Alert v-if="errorMessage" alertType="danger">
            <strong v-if="errorTitle">{{errorTitle}}:</strong>
            {{errorMessage}}
        </Alert>
        <CheckBox label="Save account information for future gifts" v-model="doSave" />
        <RockForm v-if="doSave" @submit="onSubmit">
            <TextBox label="Name for the account" rules="required" v-model="savedAccountName" />
            <template v-if="isLoginCreationNeeded">
                <Alert alertType="info">
                    <strong>Note:</strong>
                    For security purposes you will need to login to use your saved account information. To create
                    a login account please provide a user name and password below. You will be sent an email with
                    the account information above as a reminder.
                </Alert>
                <TextBox label="Username" v-model="username" rules="required" />
                <TextBox label="Password" v-model="password" type="password" rules="required" />
                <TextBox label="Confirm Password" v-model="confirmPassword" type="password" rules="required" />
            </template>
            <RockButton :isLoading="isLoading" btnType="primary" type="submit">Save Account</RockButton>
        </RockForm>
    </template>
</div>`
            });
            exports_1("default", SaveFinancialAccountForm);
        }
    };
});
//# sourceMappingURL=saveFinancialAccountForm.js.map