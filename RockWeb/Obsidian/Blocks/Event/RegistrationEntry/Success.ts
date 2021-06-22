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

import { defineComponent, inject } from 'vue';
import SaveFinancialAccountForm from '../../../Controls/SaveFinancialAccountForm';
import { Guid } from '../../../Util/Guid';
import { RegistrationEntryState } from '../RegistrationEntry';

export default defineComponent( {
    name: 'Event.RegistrationEntry.Success',
    components: {
        SaveFinancialAccountForm
    },
    setup ()
    {
        return {
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    computed: {
        /** The term to refer to a registrant */
        registrationTerm (): string
        {
            return this.registrationEntryState.ViewModel.RegistrationTerm.toLowerCase();
        },

        /** The success lava markup */
        messageHtml (): string
        {
            return this.registrationEntryState.SuccessViewModel?.MessageHtml || `You have successfully completed this ${this.registrationTerm}`;
        },

        /** The financial gateway record's guid */
        gatewayGuid (): Guid | null
        {
            return this.registrationEntryState.ViewModel.GatewayGuid;
        },

        /** The transaction code that can be used to create a saved account */
        transactionCode (): string
        {
            return this.registrationEntryState.ViewModel.IsRedirectGateway ?
                '' :
                this.registrationEntryState.SuccessViewModel?.TransactionCode || '';
        },

        /** The token returned for the payment method */
        gatewayPersonIdentifier (): string
        {
            return this.registrationEntryState.SuccessViewModel?.GatewayPersonIdentifier || '';
        }
    },
    template: `
<div>
    <div v-html="messageHtml"></div>
    <SaveFinancialAccountForm v-if="gatewayGuid && transactionCode && gatewayPersonIdentifier" :gatewayGuid="gatewayGuid" :transactionCode="transactionCode" :gatewayPersonIdentifier="gatewayPersonIdentifier" class="well">
        <template #header>
            <h3>Make Payments Even Easier</h3>
        </template>
    </SaveFinancialAccountForm>
</div>`
} );