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
import Registrant from './Registrant';
import { RegistrantInfo, RegistrationEntryState } from '../RegistrationEntry';
import Alert from '../../../Elements/Alert';

export default defineComponent( {
    name: 'Event.RegistrationEntry.Registrants',
    components: {
        Registrant,
        Alert
    },
    setup()
    {
        return {
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    methods: {
        onPrevious()
        {
            if ( this.registrationEntryState.CurrentRegistrantIndex <= 0 )
            {
                this.$emit( 'previous' );
                return;
            }

            const lastFormIndex = this.registrationEntryState.ViewModel.RegistrantForms.length - 1;
            this.registrationEntryState.CurrentRegistrantIndex--;
            this.registrationEntryState.CurrentRegistrantFormIndex = lastFormIndex;
        },
        onNext()
        {
            const lastIndex = this.registrationEntryState.Registrants.length - 1;

            if ( this.registrationEntryState.CurrentRegistrantIndex >= lastIndex )
            {
                this.$emit( 'next' );
                return;
            }

            this.registrationEntryState.CurrentRegistrantIndex++;
            this.registrationEntryState.CurrentRegistrantFormIndex = 0;
        }
    },
    computed: {
        /** Will some of the registrants have to be added to a waitlist */
        hasWaitlist(): boolean
        {
            return this.registrationEntryState.Registrants.some( r => r.IsOnWaitList );
        },

        /** Will this registrant be added to the waitlist? */
        isOnWaitlist(): boolean
        {
            const currentRegistrant = this.registrationEntryState.Registrants[ this.registrationEntryState.CurrentRegistrantIndex ];
            return currentRegistrant.IsOnWaitList;
        },

        /** What are the registrants called? */
        registrantTerm(): string
        {
            return ( this.registrationEntryState.ViewModel.RegistrantTerm || 'registrant' ).toLowerCase();
        },

        registrants(): RegistrantInfo[]
        {
            return this.registrationEntryState.Registrants;
        },
        currentRegistrantIndex(): number
        {
            return this.registrationEntryState.CurrentRegistrantIndex;
        }
    },
    template: `
<div class="registrationentry-registrant">
    <Alert v-if="hasWaitlist && !isOnWaitlist" alertType="success">
        This {{registrantTerm}} will be fully registered.
    </Alert>
    <Alert v-else-if="isOnWaitlist" alertType="warning">
        This {{registrantTerm}} will be on the waiting list.
    </Alert>
    <template v-for="(r, i) in registrants" :key="r.Guid">
        <Registrant v-show="currentRegistrantIndex === i" :currentRegistrant="r" :isWaitList="isOnWaitlist" @next="onNext" @previous="onPrevious" />
    </template>
</div>`
} );