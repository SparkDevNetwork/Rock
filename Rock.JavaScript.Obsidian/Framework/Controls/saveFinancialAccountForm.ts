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

import { defineComponent, inject, PropType } from "vue";
import Alert from "../Elements/alert";
import InlineCheckBox from "../Elements/inlineCheckBox";
import RockButton from "../Elements/rockButton";
import TextBox from "../Elements/textBox";
import { Guid } from "../Util/guid";
import { Person } from "../ViewModels";
import { BlockHttp } from "../Util/block";
import RockForm from "./rockForm";
import { useStore } from "../Store/index";

const store = useStore();

type SaveFinancialAccountFormResult = {
    title: string;
    detail: string;
    isSuccess: boolean;
};

/** A form to save a payment token for later use as a Financial Person Saved Account */
const SaveFinancialAccountForm = defineComponent({
    name: "SaveFinancialAccountForm",
    components: {
        InlineCheckBox,
        TextBox,
        Alert,
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
    setup () {
        return {
            http: inject("http") as BlockHttp
        };
    },
    data () {
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
        currentPerson (): Person | null {
            return store.state.currentPerson;
        },

        /** Is a new login account needed to attach the new saved financial account to? */
        isLoginCreationNeeded () : boolean {
            return !this.currentPerson;
        },
    },
    methods: {
        async onSubmit () {
            this.errorTitle = "";
            this.errorMessage = "";

            if (this.password !== this.confirmPassword) {
                this.errorTitle = "Password";
                this.errorMessage = "The password fields do not match.";
                return;
            }

            this.isLoading = true;

            const result = await this.http.post<SaveFinancialAccountFormResult>(`/api/v2/controls/savefinancialaccountforms/${this.gatewayGuid}`, null, {
                Password: this.password,
                SavedAccountName: this.savedAccountName,
                TransactionCode: this.transactionCode,
                Username: this.username,
                GatewayPersonIdentifier: this.gatewayPersonIdentifier
            });

            if (result?.data?.isSuccess) {
                this.successTitle = result.data.title;
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
        <InlineCheckBox label="Save account information for future gifts" v-model="doSave" />
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

export default SaveFinancialAccountForm;
