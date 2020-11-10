Obsidian.Blocks.registerBlock({
    name: 'Security.Login',
    components: {
        TextBox: Obsidian.Elements.TextBox,
        CheckBox: Obsidian.Elements.CheckBox,
        RockButton: Obsidian.Elements.RockButton,
    },
    inject: [
        'blockAction'
    ],
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
        setCookie(cookie) {
            let expires = '';

            if (cookie.Expires) {
                const date = new Date(cookie.Expires);

                if (date < new Date()) {
                    expires = '';
                }
                else {
                    expires = `; expires=${date.toGMTString()}`;
                }
            }
            else {
                expires = '';
            }

            document.cookie = `${cookie.Name}=${cookie.Value}${expires}; path=/`;
        },
        redirectAfterLogin() {
            const urlParams = new URLSearchParams(window.location.search);
            const returnUrl = urlParams.get('returnurl');

            // TODO make this force relative URLs (no absolute URLs)
            window.location.href = decodeURIComponent(returnUrl);
        },
        async onHelpClick() {
            this.isLoading = true;
            this.errorMessage = '';

            try {
                const result = await this.blockAction('help');
                const url = result.data;

                if (!url) {
                    this.errorMessage = 'An unknown error occurred communicating with the server';
                }
                else {
                    // TODO make this force relative URLs (no absolute URLs)
                    window.location.href = url;
                }
            }
            catch (e) {
                this.errorMessage = `An exception occurred: ${e}`;
            }
            finally {
                this.isLoading = false;
            }
        },
        async submitLogin() {
            if (this.isLoading) {
                return;
            }

            this.isLoading = true;
            this.errorMessage = '';

            try {
                const result = await this.blockAction('login', {
                    username: this.username,
                    password: this.password,
                    rememberMe: this.rememberMe
                });

                if (result.data.AuthCookie) {
                    this.setCookie(result.data.AuthCookie);
                    this.redirectAfterLogin();
                }
                else {
                    this.errorMessage = 'Authentication seemed to succeed, but the server did not generate a cookie';
                    this.isLoading = false;
                }
            }
            catch (e) {
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
    template:
`<div class="login-block">
    <fieldset>
        <legend>Login</legend>

        <div class="alert alert-danger" v-if="errorMessage" v-html="errorMessage"></div>

        <form @submit.prevent="submitLogin">
            <TextBox label="Username" v-model="username" />
            <TextBox label="Password" v-model="password" type="password" />
            <CheckBox label="Keep me logged in" v-model="rememberMe" />
            <RockButton :is-loading="isLoading" loading-text="Logging In..." class="btn btn-primary" @click="submitLogin" type="submit">
                Log In
            </RockButton>
        </form>

        <RockButton :is-loading="isLoading" class="btn btn-link" @click="onHelpClick">
            Forgot Account
        </RockButton>

    </fieldset>
</div>`
});
