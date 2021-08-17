System.register(["../../Elements/TextBox", "../../Elements/CheckBox", "../../Elements/RockButton", "vue", "../../Elements/Alert"], function (exports_1, context_1) {
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
    var TextBox_1, CheckBox_1, RockButton_1, vue_1, Alert_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (CheckBox_1_1) {
                CheckBox_1 = CheckBox_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Security.Login',
                components: {
                    TextBox: TextBox_1.default,
                    CheckBox: CheckBox_1.default,
                    RockButton: RockButton_1.default,
                    Alert: Alert_1.default
                },
                setup: function () {
                    return {
                        invokeBlockAction: vue_1.inject('invokeBlockAction')
                    };
                },
                data: function () {
                    return {
                        username: '',
                        password: '',
                        rememberMe: false,
                        isLoading: false,
                        errorMessage: ''
                    };
                },
                methods: {
                    setCookie: function (cookie) {
                        var expires = '';
                        if (cookie.Expires) {
                            var date = new Date(cookie.Expires);
                            if (date < new Date()) {
                                expires = '';
                            }
                            else {
                                expires = "; expires=" + date.toUTCString();
                            }
                        }
                        else {
                            expires = '';
                        }
                        document.cookie = cookie.Name + "=" + cookie.Value + expires + "; path=/";
                    },
                    redirectAfterLogin: function () {
                        var urlParams = new URLSearchParams(window.location.search);
                        var returnUrl = urlParams.get('returnurl');
                        if (returnUrl) {
                            window.location.href = decodeURIComponent(returnUrl);
                        }
                    },
                    onHelpClick: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var result, e_1;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        this.isLoading = true;
                                        this.errorMessage = '';
                                        _a.label = 1;
                                    case 1:
                                        _a.trys.push([1, 3, 4, 5]);
                                        return [4, this.invokeBlockAction('help', undefined)];
                                    case 2:
                                        result = _a.sent();
                                        if (result.isError) {
                                            this.errorMessage = result.errorMessage || 'An unknown error occurred communicating with the server';
                                        }
                                        else if (result.data) {
                                            window.location.href = result.data;
                                        }
                                        return [3, 5];
                                    case 3:
                                        e_1 = _a.sent();
                                        this.errorMessage = "An exception occurred: " + e_1;
                                        return [3, 5];
                                    case 4:
                                        this.isLoading = false;
                                        return [7];
                                    case 5: return [2];
                                }
                            });
                        });
                    },
                    submitLogin: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var result, e_2;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (this.isLoading) {
                                            return [2];
                                        }
                                        this.isLoading = true;
                                        _a.label = 1;
                                    case 1:
                                        _a.trys.push([1, 3, , 4]);
                                        return [4, this.invokeBlockAction('DoLogin', {
                                                username: this.username,
                                                password: this.password,
                                                rememberMe: this.rememberMe
                                            })];
                                    case 2:
                                        result = _a.sent();
                                        if (result && !result.isError && result.data && result.data.AuthCookie) {
                                            this.setCookie(result.data.AuthCookie);
                                            this.redirectAfterLogin();
                                            return [2];
                                        }
                                        this.isLoading = false;
                                        this.errorMessage = result.errorMessage || 'An unknown error occurred communicating with the server';
                                        return [3, 4];
                                    case 3:
                                        e_2 = _a.sent();
                                        console.log(JSON.stringify(e_2.response, null, 2));
                                        if (typeof e_2 === 'string') {
                                            this.errorMessage = e_2;
                                        }
                                        else {
                                            this.errorMessage = "An exception occurred: " + e_2;
                                        }
                                        this.isLoading = false;
                                        return [3, 4];
                                    case 4: return [2];
                                }
                            });
                        });
                    }
                },
                template: "\n<div class=\"login-block\">\n    <fieldset>\n        <legend>Login</legend>\n\n        <Alert v-if=\"errorMessage\" alertType=\"danger\">\n            <div v-html=\"errorMessage\"></div>\n        </Alert>\n\n        <form @submit.prevent=\"submitLogin\">\n            <TextBox label=\"Username\" v-model=\"username\" />\n            <TextBox label=\"Password\" v-model=\"password\" type=\"password\" />\n            <CheckBox label=\"Keep me logged in\" v-model=\"rememberMe\" />\n            <RockButton btnType=\"primary\" :is-loading=\"isLoading\" loading-text=\"Logging In...\" type=\"submit\">\n                Log In\n            </RockButton>\n        </form>\n\n        <RockButton btnType=\"link\" :is-loading=\"isLoading\" @click=\"onHelpClick\">\n            Forgot Account\n        </RockButton>\n\n    </fieldset>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Login.js.map