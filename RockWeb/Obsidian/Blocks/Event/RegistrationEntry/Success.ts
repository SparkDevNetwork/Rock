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
import { RegistrationEntryState } from '../RegistrationEntry';

export default defineComponent( {
    name: 'Event.RegistrationEntry.Success',
    setup()
    {
        return {
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    computed: {
        /** The term to refer to a registrant */
        registrationTerm(): string
        {
            return this.registrationEntryState.ViewModel.RegistrationTerm.toLowerCase();
        },

        /** The success lava markup */
        messageHtml (): string
        {
            return this.registrationEntryState.SuccessViewModel?.MessageHtml || `You have successfully completed this ${this.registrationTerm}`;
        }
    },
    template: `
<div>
    <div v-html="messageHtml"></div>
    <pre>{{JSON.stringify(registrationEntryState, null, 2)}}</pre>
</div>`
} );