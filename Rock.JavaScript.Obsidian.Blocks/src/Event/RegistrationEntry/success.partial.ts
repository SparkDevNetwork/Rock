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
import { defineComponent, inject } from "vue";
import SaveFinancialAccountForm from "@Obsidian/Controls/saveFinancialAccountForm";
import { RegistrationEntryState } from "./types.partial";
// LPC CODE
import { useStore } from "@Obsidian/PageState";

const store = useStore();

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

export default defineComponent({
    name: "Event.RegistrationEntry.Success",
    components: {
        SaveFinancialAccountForm
    },
    setup () {
        return {
            registrationEntryState: inject("registrationEntryState") as RegistrationEntryState
        };
    },
    computed: {
        /** The term to refer to a registrant */
        registrationTerm (): string {
            // MODIFIED LPC CODE
            return getLang() == 'es' ? 'registro' : this.registrationEntryState.viewModel.registrationTerm.toLowerCase();
            // END MODIFIED LPC CODE
        },

        /** The success lava markup */
        messageHtml (): string {
            return this.registrationEntryState.successViewModel?.messageHtml || `You have successfully completed this ${this.registrationTerm}`;
        },

        /** The financial gateway record's guid */
        gatewayGuid (): Guid | null {
            return this.registrationEntryState.viewModel.gatewayGuid;
        },

        /** The transaction code that can be used to create a saved account */
        transactionCode (): string {
            return this.registrationEntryState.viewModel.isRedirectGateway ?
                "" :
                this.registrationEntryState.successViewModel?.transactionCode || "";
        },

        /** The token returned for the payment method */
        gatewayPersonIdentifier (): string {
            return this.registrationEntryState.successViewModel?.gatewayPersonIdentifier || "";
        },

        enableSaveAccount(): boolean {
            return this.registrationEntryState.viewModel.enableSaveAccount && this.registrationEntryState.savedAccountGuid === null;
        }
    },
    methods: {
        getLang
    },
    template: `
<div>
    <div v-html="messageHtml"></div>
    <SaveFinancialAccountForm v-if="gatewayGuid && transactionCode && gatewayPersonIdentifier && enableSaveAccount"
        :gatewayGuid="gatewayGuid"
        :transactionCode="transactionCode"
        :gatewayPersonIdentifier="gatewayPersonIdentifier"
        class="well">
        <template #header>
            <!-- MODIFIED LPC CODE -->
            <h3>{{ getLang() == 'es' ? 'Haz Los Pagos Más Fáciles' : 'Make Payments Even Easier' }}</h3>
            <!-- END MODIFIED LPC CODE -->
        </template>
    </SaveFinancialAccountForm>
</div>`
});
