System.register(["vue", "../Elements/Alert", "../Elements/CheckBox", "../Elements/RockButton", "../Elements/TextBox", "./RockForm"], function (exports_1, context_1) {
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
    var vue_1, Alert_1, CheckBox_1, RockButton_1, TextBox_1, RockForm_1, SaveFinancialAccountForm;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (CheckBox_1_1) {
                CheckBox_1 = CheckBox_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            }
        ],
        execute: function () {
            ;
            SaveFinancialAccountForm = vue_1.defineComponent({
                name: 'SaveFinancialAccountForm',
                components: {
                    CheckBox: CheckBox_1.default,
                    TextBox: TextBox_1.default,
                    Alert: Alert_1.default,
                    RockButton: RockButton_1.default,
                    RockForm: RockForm_1.default
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
                setup: function () {
                    return {
                        http: vue_1.inject('http')
                    };
                },
                data: function () {
                    return {
                        doSave: false,
                        username: '',
                        password: '',
                        confirmPassword: '',
                        savedAccountName: '',
                        isLoading: false,
                        successTitle: '',
                        successMessage: '',
                        errorTitle: '',
                        errorMessage: ''
                    };
                },
                computed: {
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    isLoginCreationNeeded: function () {
                        return !this.currentPerson;
                    },
                },
                methods: {
                    onSubmit: function () {
                        var _a, _b, _c;
                        return __awaiter(this, void 0, void 0, function () {
                            var result;
                            return __generator(this, function (_d) {
                                switch (_d.label) {
                                    case 0:
                                        this.errorTitle = '';
                                        this.errorMessage = '';
                                        if (this.password !== this.confirmPassword) {
                                            this.errorTitle = 'Password';
                                            this.errorMessage = 'The password fields do not match.';
                                            return [2];
                                        }
                                        this.isLoading = true;
                                        return [4, this.http.post("api/v2/controls/savefinancialaccountforms/" + this.gatewayGuid, null, {
                                                Password: this.password,
                                                SavedAccountName: this.savedAccountName,
                                                TransactionCode: this.transactionCode,
                                                Username: this.username,
                                                GatewayPersonIdentifier: this.gatewayPersonIdentifier
                                            })];
                                    case 1:
                                        result = _d.sent();
                                        if ((_a = result === null || result === void 0 ? void 0 : result.data) === null || _a === void 0 ? void 0 : _a.IsSuccess) {
                                            this.successTitle = result.data.Title;
                                            this.successMessage = result.data.Detail || 'Success';
                                        }
                                        else {
                                            this.errorTitle = ((_b = result.data) === null || _b === void 0 ? void 0 : _b.Title) || '';
                                            this.errorMessage = ((_c = result.data) === null || _c === void 0 ? void 0 : _c.Detail) || 'Error';
                                        }
                                        this.isLoading = false;
                                        return [2];
                                }
                            });
                        });
                    }
                },
                template: "\n<div>\n    <Alert v-if=\"successMessage\" alertType=\"success\" class=\"m-0\">\n        <strong v-if=\"successTitle\">{{successTitle}}:</strong>\n        {{successMessage}}\n    </Alert>\n    <template v-else>\n        <slot name=\"header\">\n            <h3>Make Giving Even Easier</h3>\n        </slot>\n        <Alert v-if=\"errorMessage\" alertType=\"danger\">\n            <strong v-if=\"errorTitle\">{{errorTitle}}:</strong>\n            {{errorMessage}}\n        </Alert>\n        <CheckBox label=\"Save account information for future gifts\" v-model=\"doSave\" />\n        <RockForm v-if=\"doSave\" @submit=\"onSubmit\">\n            <TextBox label=\"Name for the account\" rules=\"required\" v-model=\"savedAccountName\" />\n            <template v-if=\"isLoginCreationNeeded\">\n                <Alert alertType=\"info\">\n                    <strong>Note:</strong>\n                    For security purposes you will need to login to use your saved account information. To create\n                    a login account please provide a user name and password below. You will be sent an email with\n                    the account information above as a reminder.\n                </Alert>\n                <TextBox label=\"Username\" v-model=\"username\" rules=\"required\" />\n                <TextBox label=\"Password\" v-model=\"password\" type=\"password\" rules=\"required\" />\n                <TextBox label=\"Confirm Password\" v-model=\"confirmPassword\" type=\"password\" rules=\"required\" />\n            </template>\n            <RockButton :isLoading=\"isLoading\" btnType=\"primary\" type=\"submit\">Save Account</RockButton>\n        </RockForm>\n    </template>\n</div>"
            });
            exports_1("default", SaveFinancialAccountForm);
        }
    };
});
//# sourceMappingURL=SaveFinancialAccountForm.js.map