System.register(["../../Elements/textBox", "../../Elements/checkBox", "../../Elements/rockButton", "vue", "../../Elements/alert", "../../Util/rockDateTime"], function (exports_1, context_1) {
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
    var textBox_1, checkBox_1, rockButton_1, vue_1, alert_1, rockDateTime_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (checkBox_1_1) {
                checkBox_1 = checkBox_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Security.Login",
                components: {
                    TextBox: textBox_1.default,
                    CheckBox: checkBox_1.default,
                    RockButton: rockButton_1.default,
                    Alert: alert_1.default
                },
                setup() {
                    return {
                        invokeBlockAction: vue_1.inject("invokeBlockAction")
                    };
                },
                data() {
                    return {
                        username: "",
                        password: "",
                        rememberMe: false,
                        isLoading: false,
                        errorMessage: ""
                    };
                },
                methods: {
                    setCookie(cookie) {
                        let expires = "";
                        if (cookie.expires) {
                            const date = rockDateTime_1.RockDateTime.parseHTTP(cookie.expires);
                            if (date === null || date < rockDateTime_1.RockDateTime.now()) {
                                expires = "";
                            }
                            else {
                                expires = `; expires=${date.toHTTPString()}`;
                            }
                        }
                        else {
                            expires = "";
                        }
                        document.cookie = `${cookie.name}=${cookie.value}${expires}; path=/`;
                    },
                    redirectAfterLogin() {
                        const urlParams = new URLSearchParams(window.location.search);
                        const returnUrl = urlParams.get("returnurl");
                        if (returnUrl) {
                            window.location.href = decodeURIComponent(returnUrl);
                        }
                    },
                    onHelpClick() {
                        return __awaiter(this, void 0, void 0, function* () {
                            this.isLoading = true;
                            this.errorMessage = "";
                            try {
                                const result = yield this.invokeBlockAction("help", undefined);
                                if (result.isError) {
                                    this.errorMessage = result.errorMessage || "An unknown error occurred communicating with the server";
                                }
                                else if (result.data) {
                                    window.location.href = result.data;
                                }
                            }
                            catch (e) {
                                this.errorMessage = `An exception occurred: ${e}`;
                            }
                            finally {
                                this.isLoading = false;
                            }
                        });
                    },
                    submitLogin() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.isLoading) {
                                return;
                            }
                            this.isLoading = true;
                            try {
                                const result = yield this.invokeBlockAction("DoLogin", {
                                    username: this.username,
                                    password: this.password,
                                    rememberMe: this.rememberMe
                                });
                                if (result && !result.isError && result.data && result.data.authCookie) {
                                    this.setCookie(result.data.authCookie);
                                    this.redirectAfterLogin();
                                    return;
                                }
                                this.isLoading = false;
                                this.errorMessage = result.errorMessage || "An unknown error occurred communicating with the server";
                            }
                            catch (e) {
                                console.log(JSON.stringify(e.response, null, 2));
                                if (typeof e === "string") {
                                    this.errorMessage = e;
                                }
                                else {
                                    this.errorMessage = `An exception occurred: ${e}`;
                                }
                                this.isLoading = false;
                            }
                        });
                    }
                },
                template: `
<div class="login-block">
    <fieldset>
        <legend>Login</legend>

        <Alert v-if="errorMessage" alertType="danger">
            <div v-html="errorMessage"></div>
        </Alert>

        <form @submit.prevent="submitLogin">
            <TextBox label="Username" v-model="username" />
            <TextBox label="Password" v-model="password" type="password" />
            <CheckBox label="Keep me logged in" v-model="rememberMe" />
            <RockButton btnType="primary" :is-loading="isLoading" loading-text="Logging In..." type="submit">
                Log In
            </RockButton>
        </form>

        <RockButton btnType="link" :is-loading="isLoading" @click="onHelpClick">
            Forgot Account
        </RockButton>

    </fieldset>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=login.js.map