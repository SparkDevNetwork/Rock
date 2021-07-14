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

import { defineComponent, inject, PropType } from 'vue';
import Dialog from '../../../Controls/Dialog';
import { InvokeBlockActionFunc } from '../../../Controls/RockBlock';
import LoadingIndicator from '../../../Elements/LoadingIndicator';
import RockButton from '../../../Elements/RockButton';
import { toWord } from '../../../Services/Number';
import { pluralConditional } from '../../../Services/String';
import { RegistrationEntryState } from '../RegistrationEntry';
import { SessionRenewalResult } from './RegistrationEntryBlockViewModel';

export default defineComponent( {
    name: 'Event.RegistrationEntry.SessionRenewal',
    components: {
        Dialog,
        LoadingIndicator,
        RockButton
    },
    props: {
        isSessionExpired: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    setup ()
    {
        return {
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState,
            invokeBlockAction: inject( 'invokeBlockAction' ) as InvokeBlockActionFunc
        };
    },
    data ()
    {
        return {
            spotsSecured: null as number | null,
            isLoading: false,
            isModalVisible: false
        };
    },
    computed: {
        /** Does this registration instance have a waitlist? */
        hasWaitlist (): boolean
        {
            return this.registrationEntryState.ViewModel.waitListEnabled;
        },

        /** The number of registrants being registered */
        allRegistrantCount (): number
        {
            return this.registrationEntryState.Registrants.length;
        },

        /** The number of registrants pushed to the waitlist */
        waitlistRegistrantCount (): number
        {
            return this.registrationEntryState.Registrants.filter( r => r.IsOnWaitList ).length;
        },

        /** The number of registrants pushed to the waitlist as a word (eg "one") */
        waitlistRegistrantCountWord (): string
        {
            return toWord( this.waitlistRegistrantCount );
        },

        /** The number of registrants not on a waitlist */
        nonWaitlistRegistrantCount (): number
        {
            return this.registrationEntryState.Registrants.filter( r => !r.IsOnWaitList ).length;
        },

        /** The number of registrants not on a waitlist as a word (eg "one") */
        nonWaitlistRegistrantCountWord (): string
        {
            return toWord( this.nonWaitlistRegistrantCount );
        }
    },
    methods: {
        pluralConditional,

        /** Restart the registration by reloading the page */
        restart ()
        {
            this.isLoading = true;
            location.reload();
        },

        /** Close the modal and continue on */
        close ()
        {
            this.isModalVisible = false;

            this.$nextTick( () =>
            {
                this.spotsSecured = null;
                this.isLoading = false;
            } );
        },

        /** Attempt to renew the session and get more time */
        async requestRenewal ()
        {
            this.spotsSecured = 0;
            this.isLoading = true;

            try
            {
                const response = await this.invokeBlockAction<SessionRenewalResult>( 'TryToRenewSession', {
                    registrationSessionGuid: this.registrationEntryState.RegistrationSessionGuid
                } );

                if ( response.data )
                {
                    const asDate = new Date( response.data.expirationDateTime );
                    this.registrationEntryState.SessionExpirationDate = asDate;
                    this.spotsSecured = response.data.spotsSecured;

                    // If there is a deficiency, then update the state to reflect the reduced spots available
                    let deficiency = this.nonWaitlistRegistrantCount - this.spotsSecured;

                    if ( !deficiency )
                    {
                        this.$emit( 'success' );
                        this.close();
                        return;
                    }

                    this.registrationEntryState.ViewModel.spotsRemaining = this.spotsSecured;

                    if ( !this.hasWaitlist )
                    {
                        // Reduce the registrants down to fit the spots available
                        this.registrationEntryState.Registrants.length = this.spotsSecured;
                        return;
                    }

                    // Work backward through the registrants until the deficiency is removed
                    for ( let i = this.allRegistrantCount - 1; i >= 0; i-- )
                    {
                        if ( !deficiency )
                        {
                            break;
                        }

                        const registrant = this.registrationEntryState.Registrants[ i ];

                        if ( registrant.IsOnWaitList )
                        {
                            continue;
                        }

                        registrant.IsOnWaitList = true;
                        deficiency--;
                    }
                }
            }
            finally
            {
                this.isLoading = false;
            }
        }
    },
    watch: {
        isSessionExpired ()
        {
            if ( this.isSessionExpired )
            {
                this.spotsSecured = null;
                this.isModalVisible = true;
            }
        }
    },
    template: `
<Dialog :modelValue="isModalVisible" :dismissible="false">
    <template #header>
        <h4 v-if="isLoading || spotsSecured === null">Registration Timed Out</h4>
        <h4 v-else>Request Extension</h4>
    </template>
    <template #default>
        <LoadingIndicator v-if="isLoading" />
        <template v-else-if="hasWaitlist && spotsSecured === 0">
            Due to high demand there is no longer space available.
            You can continue, but your registrants will be placed on the waitlist.
            Do you wish to continue with the registration?
        </template>
        <template v-else-if="spotsSecured === 0">
            Due to high demand there is no longer space available for this registration.
        </template>
        <template v-else-if="hasWaitlist && spotsSecured !== null">
            Due to high demand there is no longer space available for all your registrants.
            Your last {{waitlistRegistrantCountWord}}
            {{pluralConditional(waitlistRegistrantCount, 'registrant', ' registrants')}}
            will be placed on the waitlist.
            Do you wish to continue with the registration?
        </template>
        <template v-else-if="spotsSecured !== null">
            Due to high demand there is no longer space available for all your registrants.
            Only {{nonWaitlistRegistrantCountWord}} {{pluralConditional(nonWaitlistRegistrantCount, 'spot is', 'spots are')}} available.
            Your registration has been updated to only allow
            {{nonWaitlistRegistrantCountWord}} {{pluralConditional(nonWaitlistRegistrantCount, 'registrant', 'registrants')}}. 
            Do you wish to continue with the registration?
        </template>
        <template v-else>
            Your registration has timed out. Do you wish to request an extension in time?
        </template>
    </template>
    <template v-if="!isLoading" #footer>
        <template v-if="!hasWaitlist && spotsSecured === 0">
            <RockButton btnType="link" @click="restart">Close</RockButton>
        </template>
        <template v-else-if="spotsSecured !== null">
            <RockButton btnType="primary" @click="close">Yes</RockButton>
            <RockButton btnType="link" @click="restart">No, cancel registration</RockButton>
        </template>
        <template v-else>
            <RockButton btnType="primary" @click="requestRenewal">Yes</RockButton>
            <RockButton btnType="link" @click="restart">No, cancel registration</RockButton>
        </template>
    </template>
</Dialog>`
} );