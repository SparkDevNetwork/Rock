import TextBox from '../../Elements/TextBox.js';
import CheckBox from '../../Elements/CheckBox.js';
import RockButton from '../../Elements/RockButton.js';
import { defineComponent, inject } from '../../Vendor/Vue/vue.js';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock.js';

type AuthCookie = {
    Expires: string;
    Name: string;
    Value: string;
};

type LoginResponse = {
    AuthCookie: AuthCookie | null;
};

export default defineComponent({
    name: 'Security.Login',
    components: {
        TextBox,
        CheckBox,
        RockButton
    },
    setup() {
        return {
            invokeBlockAction: inject('invokeBlockAction') as InvokeBlockActionFunc
        };
    },
    data() {
        return {
            username: '',
            password: '',
            rememberMe: false,
            isLoading: false,
            errorMessage: ''
        };
    },
    methods: {
        setCookie(cookie: AuthCookie): void {
            let expires = '';

            if (cookie.Expires) {
                const date = new Date(cookie.Expires);

                if (date < new Date()) {
                    expires = '';
                }
                else {
                    expires = `; expires=${date.toUTCString()}`;
                }
            }
            else {
                expires = '';
            }

            document.cookie = `${cookie.Name}=${cookie.Value}${expires}; path=/`;
        },
        redirectAfterLogin(): void {
            const urlParams = new URLSearchParams(window.location.search);
            const returnUrl = urlParams.get('returnurl');

            if (returnUrl) {
                // TODO make this force relative URLs (no absolute URLs)
                window.location.href = decodeURIComponent(returnUrl);
            }
        },
        async onHelpClick(): Promise<void> {
            this.isLoading = true;
            this.errorMessage = '';

            try {
                const result = await this.invokeBlockAction<string>('help', undefined);

                if (result.isError) {
                    this.errorMessage = result.errorMessage || 'An unknown error occurred communicating with the server';
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
        async submitLogin(): Promise<void> {
            if (this.isLoading) {
                return;
            }

            this.isLoading = true;

            try {
                const result = await this.invokeBlockAction<LoginResponse>('DoLogin', {
                    username: this.username,
                    password: this.password,
                    rememberMe: this.rememberMe
                });

                if (result && !result.isError && result.data && result.data.AuthCookie) {
                    this.setCookie(result.data.AuthCookie);
                    this.redirectAfterLogin();
                    return;
                }

                this.isLoading = false;
                this.errorMessage = result.errorMessage || 'An unknown error occurred communicating with the server';
            }
            catch (e) {
                // ts-ignore-line
                console.log(JSON.stringify(e.response, null, 2));

                if (typeof e === 'string') {
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

        <div class="alert alert-danger" v-if="errorMessage" v-html="errorMessage"></div>

        <form @submit.prevent="submitLogin">
            <TextBox label="Username" v-model="username" />
            <TextBox label="Password" v-model="password" type="password" />
            <CheckBox label="Keep me logged in" v-model="rememberMe" />
            <RockButton primary :is-loading="isLoading" loading-text="Logging In..." type="submit">
                Log In
            </RockButton>
        </form>

        <RockButton link :is-loading="isLoading" @click="onHelpClick">
            Forgot Account
        </RockButton>

    </fieldset>
</div>`
});
