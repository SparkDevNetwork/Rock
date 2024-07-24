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

import { Guid } from "@Obsidian/Types";
import { defineComponent, inject, PropType } from "vue";
import NotificationBox from "./notificationBox.obs";
import InlineCheckBox from "./inlineCheckBox";
import RockButton from "./rockButton";
import TextBox from "./textBox";
import RockForm from "./rockForm";
import { useStore } from "@Obsidian/PageState";
import { SaveFinancialAccountFormSaveAccountOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/saveFinancialAccountFormSaveAccountOptionsBag";
import { SaveFinancialAccountFormSaveAccountResultBag } from "@Obsidian/ViewModels/Rest/Controls/saveFinancialAccountFormSaveAccountResultBag";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import { useHttp } from "@Obsidian/Utility/http";

const store = useStore();

// LPC CODE
/** Gets the lang parameter from the query string.
 * Returns "en" or "es". Defaults to "en" if invalid. */
function getLang(): string {
    var lang = typeof store.state.pageParameters["lang"] === 'string' ? store.state.pageParameters["lang"] : "";

    if (lang != "es") {
        lang = "en";
    }

    return lang;
}
// END LPC CODE

/** A form to save a payment token for later use as a Financial Person Saved Account */
const SaveFinancialAccountForm = defineComponent({
    name: "SaveFinancialAccountForm",
    components: {
        InlineCheckBox,
        TextBox,
        NotificationBox,
        RockButton,
        RockForm
    },
    props: {
        gatewayGuid: {
            type: String as PropType<Guid>,
            required: true
        },
        transactionCode: {
            type: String as PropType<string>,
            required: true
        },
        gatewayPersonIdentifier: {
            type: String as PropType<string>,
            required: true
        }
    },
    setup() {
        const http = useHttp();

        return {
            http
        };
    },
    data() {
        return {
            /** Will the payment token be saved for future use? */
            doSave: false,

            /** The username to create a login with */
            username: "",

            /** The password to create a login with */
            password: "",

            /** The confirmed password to create a login with */
            confirmPassword: "",

            /** What the account will be named once created */
            savedAccountName: "",

            /** Is an AJAX call currently in-flight? */
            isLoading: false,

            successTitle: "",
            successMessage: "",
            errorTitle: "",
            errorMessage: ""
        };
    },
    computed: {
        /** The person currently authenticated */
        currentPerson(): PersonBag | null {
            return store.state.currentPerson;
        },

        /** Is a new login account needed to attach the new saved financial account to? */
        isLoginCreationNeeded(): boolean {
            return !this.currentPerson;
        },
    },
    methods: {
        // LPC CODE
        getLang,
        // END LPC CODE
        async onSubmit() {
            this.errorTitle = "";
            this.errorMessage = "";

            if (this.password !== this.confirmPassword) {
                this.errorTitle = "Password";
                this.errorMessage = "The password fields do not match.";
                return;
            }

            this.isLoading = true;

            const options: Partial<SaveFinancialAccountFormSaveAccountOptionsBag> = {
                gatewayGuid: this.gatewayGuid,
                password: this.password,
                savedAccountName: this.savedAccountName,
                transactionCode: this.transactionCode,
                username: this.username,
                gatewayPersonIdentifier: this.gatewayPersonIdentifier
            };
            const result = await this.http.post<SaveFinancialAccountFormSaveAccountResultBag>("/api/v2/Controls/SaveFinancialAccountFormSaveAccount", null, options);

            if (result.isSuccess && result.data?.isSuccess) {
                this.successTitle = result.data.title || "";
                this.successMessage = result.data.detail || "Success";
            }
            else {
                this.errorTitle = result.data?.title || "";
                this.errorMessage = result.data?.detail || "Error";
            }

            this.isLoading = false;
        }
    },
    template: `
<div>
    <NotificationBox v-if="successMessage" alertType="success" class="m-0">
        <strong v-if="successTitle">{{successTitle}}:</strong>
        {{successMessage}}
    </NotificationBox>
    <template v-else>
        <slot name="header">
            <h3>Make Giving Even Easier</h3>
        </slot>
        <NotificationBox v-if="errorMessage" alertType="danger">
            <strong v-if="errorTitle">{{errorTitle}}:</strong>
            {{errorMessage}}
        </NotificationBox>
        <InlineCheckBox :label="getLang() == 'es' ? 'Guardar información para futuros pagos' : 'Save account information for future gifts'" v-model="doSave" />
        <RockForm v-if="doSave" @submit="onSubmit">
            <TextBox :label="getLang() == 'es' ? 'Nombre de la cuenta' : 'Name for the account'" rules="required" v-model="savedAccountName" />
            <template v-if="isLoginCreationNeeded">
                <NotificationBox alertType="info">
                    <strong>
                        {{ getLang() == 'es' ? 'Nota:' : 'Note:' }}
                    </strong>
                    <span v-if="getLang() == 'es'">
                        Por seguridad, necesitarás iniciar sesión para usar tu información guardada. Para crear una cuenta,
                        por favor provee un usuario y contraseña a continuación. Te enviaremos un email con la información
                        de la cuenta como recordatorio.
                    </span>
                    <span v-else>
                        For security purposes you will need to login to use your saved account information. To create
                        a login account please provide a user name and password below. You will be sent an email with
                        the account information above as a reminder.
                    </span>
                </NotificationBox>
                <TextBox :label="getLang() == 'es' ? 'Usuario' : 'Username'" v-model="username" rules="required" />
                <TextBox :label="getLang() == 'es' ? 'Contraseña' : 'Password'" v-model="password" type="password" rules="required" />
                <TextBox :label="getLang() == 'es' ? 'Confirmar Contraseña' : 'Confirm Password'" v-model="confirmPassword" type="password" rules="required" />
            </template>
            <RockButton :isLoading="isLoading" btnType="primary" type="submit">{{ getLang() == 'es' ? 'Guardar Cuenta' : 'Save Account' }}</RockButton>
        </RockForm>
    </template>
</div>`
});

export default SaveFinancialAccountForm;
