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
import CheckBox from '../../../Elements/CheckBox';
import EmailBox from '../../../Elements/EmailBox';
import StaticFormControl from '../../../Elements/StaticFormControl';
import TextBox from '../../../Elements/TextBox';
import Person from '../../../ViewModels/CodeGenerated/PersonViewModel';
import { getRegistrantBasicInfo, RegistrantBasicInfo, RegistrationEntryState } from '../RegistrationEntry';
import { RegistrationEntryBlockArgs } from './RegistrationEntryBlockArgs';
import { RegistrantInfo, RegistrarInfo, RegistrarOption, RegistrationEntryBlockViewModel } from './RegistrationEntryBlockViewModel';

export default defineComponent( {
    name: 'Event.RegistrationEntry.Registrar',
    components: {
        TextBox,
        CheckBox,
        EmailBox,
        StaticFormControl
    },
    setup ()
    {
        return {
            getRegistrationEntryBlockArgs: inject( 'getRegistrationEntryBlockArgs' ) as () => RegistrationEntryBlockArgs,
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    data ()
    {
        return {
            /** Should the registrar panel be shown */
            isRegistrarPanelShown: true
        };
    },
    computed: {
        /** Is the registrar option set to UseLoggedInPerson */
        useLoggedInPersonForRegistrar (): boolean
        {
            return ( !!this.currentPerson ) && this.viewModel.RegistrarOption === RegistrarOption.UseLoggedInPerson;
        },

        /** The person that is currently authenticated */
        currentPerson (): Person | null
        {
            return this.$store.state.currentPerson;
        },

        /** The person entering the registration information. This object is part of the registration state. */
        registrar (): RegistrarInfo
        {
            return this.registrationEntryState.Registrar;
        },

        /** The first registrant entered into the registration. */
        firstRegistrant (): RegistrantInfo
        {
            return this.registrationEntryState.Registrants[ 0 ];
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel (): RegistrationEntryBlockViewModel
        {
            return this.registrationEntryState.ViewModel;
        },

        /** Should the checkbox allowing the registrar to choose to update their email address be shown? */
        doShowUpdateEmailOption (): boolean
        {
            return !this.viewModel.ForceEmailUpdate && !!this.currentPerson?.Email;
        },

        /** Info about the registrants made available by .FirstName instead of by field guid */
        registrantInfos (): RegistrantBasicInfo[]
        {
            return this.registrationEntryState.Registrants.map( r => getRegistrantBasicInfo( r, this.viewModel.RegistrantForms ) );
        },

        /** The registrant term - plural if there are more than 1 */
        registrantTerm (): string
        {
            return this.registrantInfos.length === 1 ? this.viewModel.RegistrantTerm : this.viewModel.PluralRegistrantTerm;
        },

        /** The name of this registration instance */
        instanceName (): string
        {
            return this.viewModel.InstanceName;
        }
    },
    methods: {
        /** Prefill in the registrar form fields based on the admin's settings */
        prefillRegistrar ()
        {
            this.isRegistrarPanelShown = true;

            // If the option is to prompt or use the current person, prefill the current person if available
            if ( this.currentPerson &&
                ( this.viewModel.RegistrarOption === RegistrarOption.UseLoggedInPerson || this.viewModel.RegistrarOption === RegistrarOption.PromptForRegistrar ) )
            {
                this.registrar.NickName = this.currentPerson.NickName || this.currentPerson.FirstName || '';
                this.registrar.LastName = this.currentPerson.LastName || '';
                this.registrar.Email = this.currentPerson.Email || '';
                return;
            }

            if ( this.viewModel.RegistrarOption === RegistrarOption.PromptForRegistrar )
            {
                return;
            }

            // If prefill or first-registrant, then the first registrants info is used (as least as a starting point)
            if ( this.viewModel.RegistrarOption === RegistrarOption.PrefillFirstRegistrant || this.viewModel.RegistrarOption === RegistrarOption.UseFirstRegistrant )
            {
                const firstRegistrantInfo = getRegistrantBasicInfo( this.firstRegistrant, this.viewModel.RegistrantForms );
                this.registrar.NickName = firstRegistrantInfo.FirstName;
                this.registrar.LastName = firstRegistrantInfo.LastName;
                this.registrar.Email = firstRegistrantInfo.Email;

                const hasAllInfo = ( !!this.registrar.NickName ) && ( !!this.registrar.LastName ) && ( !!this.registrar.Email );

                if ( hasAllInfo && this.viewModel.RegistrarOption === RegistrarOption.UseFirstRegistrant )
                {
                    this.isRegistrarPanelShown = false;
                }

                return;
            }
        }
    },
    watch: {
        currentPerson: {
            immediate: true,
            handler ()
            {
                this.prefillRegistrar();
            }
        }
    },
    template: `
<div v-if="isRegistrarPanelShown" class="well">
    <h4>This Registration Was Completed By</h4>
    <template v-if="useLoggedInPersonForRegistrar">
        <div class="row">
            <div class="col-md-6">
                <StaticFormControl label="First Name" v-model="registrar.NickName" />
            </div>
            <div class="col-md-6">
                <StaticFormControl label="Last Name" v-model="registrar.LastName" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <StaticFormControl label="Email" v-model="registrar.Email" />
            </div>
        </div>
    </template>
    <template v-else>
        <div class="row">
            <div class="col-md-6">
                <TextBox label="First Name" rules="required" v-model="registrar.NickName" />
            </div>
            <div class="col-md-6">
                <TextBox label="Last Name" rules="required" v-model="registrar.LastName" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <EmailBox label="Send Confirmation Emails To" rules="required" v-model="registrar.Email" />
                <CheckBox v-if="doShowUpdateEmailOption" label="Should Your Account Be Updated To Use This Email Address?" v-model="registrar.UpdateEmail" />
            </div>
        </div>
    </template>
</div>`
} );