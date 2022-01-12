// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
import TextBox from "../../Elements/textBox";
import InlineCheckBox from "../../Elements/inlineCheckBox";
import RockButton from "../../Elements/rockButton";
import { defineComponent, inject } from "vue";
import { InvokeBlockActionFunc } from "../../Util/block";
import Alert from "../../Elements/alert";
import { RockDateTime } from "../../Util/rockDateTime";

type AuthCookie = {
    expires: string;
    name: string;
    value: string;
};

type LoginResponse = {
    authCookie: AuthCookie | null;
};

export default defineComponent({
    name: "Security.Login",
    components: {
        TextBox,
        InlineCheckBox,
        RockButton,
        Alert
    },
    setup () {
        return {
            invokeBlockAction: inject("invokeBlockAction") as InvokeBlockActionFunc
        };
    },
    data () {
        return {
            username: "",
            password: "",
            rememberMe: false,
            isLoading: false,
            errorMessage: ""
        };
    },
    methods: {
        setCookie (cookie: AuthCookie): void {
            let expires = "";

            if (cookie.expires) {
                const date = RockDateTime.parseHTTP(cookie.expires);

                if (date === null || date < RockDateTime.now()) {
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
        redirectAfterLogin (): void {
            const urlParams = new URLSearchParams(window.location.search);
            const returnUrl = urlParams.get("returnurl");

            if (returnUrl) {
                // TODO make this force relative URLs (no absolute URLs)
                window.location.href = decodeURIComponent(returnUrl);
            }
        },
        async onHelpClick (): Promise<void> {
            this.isLoading = true;
            this.errorMessage = "";

            try {
                const result = await this.invokeBlockAction<string>("help", undefined);

                if (result.isError) {
                    this.errorMessage = result.errorMessage || "An unknown error occurred communicating with the server";
                }
                else if (result.data) {
                    // TODO make this force relative URLs (no absolute URLs)
                    window.location.href = result.data;
                }
            }
            catch (e) {
                this.errorMessage = `An exception occurred: ${e}`;
            }
            finally {
                this.isLoading = false;
            }
        },
        async submitLogin (): Promise<void> {
            if (this.isLoading) {
                return;
            }

            this.isLoading = true;

            try {
                const result = await this.invokeBlockAction<LoginResponse>("DoLogin", {
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
                // ts-ignore-line
                console.log(JSON.stringify(e.response, null, 2));

                if (typeof e === "string") {
                    this.errorMessage = e;
                }
                else {
                    this.errorMessage = `An exception occurred: ${e}`;
                }

                this.isLoading = false;
            }
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
            <InlineCheckBox label="Keep me logged in" v-model="rememberMe" />
            <RockButton btnType="primary" :is-loading="isLoading" loading-text="Logging In..." type="submit">
                Log In
            </RockButton>
        </form>

        <RockButton btnType="link" :is-loading="isLoading" @click="onHelpClick">
            Forgot Account
        </RockButton>

    </fieldset>
</div>`
});
