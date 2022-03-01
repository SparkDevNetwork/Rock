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

import { defineComponent, inject } from "vue";
import InlineCheckBox from "../../../Elements/inlineCheckBox";
import EmailBox from "../../../Elements/emailBox";
import RadioButtonList from "../../../Elements/radioButtonList";
import StaticFormControl from "../../../Elements/staticFormControl";
import TextBox from "../../../Elements/textBox";
import { Guid } from "../../../Util/guid";
import { ListItem, Person } from "../../../ViewModels";
import { getRegistrantBasicInfo, RegistrantBasicInfo, RegistrationEntryState } from "../registrationEntry";
import { RegistrationEntryBlockArgs } from "./registrationEntryBlockArgs";
import { RegistrantInfo, RegistrantsSameFamily, RegistrarInfo, RegistrarOption, RegistrationEntryBlockViewModel } from "./registrationEntryBlockViewModel";
import { useStore } from "../../../Store/index";

const store = useStore();

export default defineComponent({
    name: "Event.RegistrationEntry.Registrar",
    components: {
        TextBox,
        InlineCheckBox,
        EmailBox,
        StaticFormControl,
        RadioButtonList
    },
    setup () {
        return {
            getRegistrationEntryBlockArgs: inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs,
            registrationEntryState: inject("registrationEntryState") as RegistrationEntryState
        };
    },
    data () {
        return {
            /** Should the registrar panel be shown */
            isRegistrarPanelShown: true
        };
    },
    computed: {
        /** Is the registrar option set to UseLoggedInPerson */
        useLoggedInPersonForRegistrar (): boolean {
            return (!!this.currentPerson) && this.viewModel.registrarOption === RegistrarOption.UseLoggedInPerson;
        },

        /** The person that is currently authenticated */
        currentPerson (): Person | null {
            return store.state.currentPerson;
        },

        /** The person entering the registration information. This object is part of the registration state. */
        registrar (): RegistrarInfo {
            return this.registrationEntryState.registrar;
        },

        /** The first registrant entered into the registration. */
        firstRegistrant (): RegistrantInfo {
            return this.registrationEntryState.registrants[ 0 ];
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel (): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },

        /** Should the checkbox allowing the registrar to choose to update their email address be shown? */
        doShowUpdateEmailOption (): boolean {
            return !this.viewModel.forceEmailUpdate && !!this.currentPerson?.email;
        },

        /** Info about the registrants made available by .FirstName instead of by field guid */
        registrantInfos (): RegistrantBasicInfo[] {
            return this.registrationEntryState.registrants.map(r => getRegistrantBasicInfo(r, this.viewModel.registrantForms));
        },

        /** The registrant term - plural if there are more than 1 */
        registrantTerm (): string {
            return this.registrantInfos.length === 1 ? this.viewModel.registrantTerm : this.viewModel.pluralRegistrantTerm;
        },

        /** The name of this registration instance */
        instanceName (): string {
            return this.viewModel.instanceName;
        },

        /** The radio options that are displayed to allow the user to pick another person that this
         *  registrar is part of a family. */
        familyOptions (): ListItem[] {
            const options: ListItem[] = [];
            const usedFamilyGuids: Record<Guid, boolean> = {};

            if (this.viewModel.registrantsSameFamily !== RegistrantsSameFamily.Ask) {
                return options;
            }

            // Add previous registrants as options
            for (let i = 0; i < this.registrationEntryState.registrants.length; i++) {
                const registrant = this.registrationEntryState.registrants[ i ];
                const info = getRegistrantBasicInfo(registrant, this.viewModel.registrantForms);

                if (!usedFamilyGuids[ registrant.familyGuid ] && info?.firstName && info?.lastName) {
                    options.push({
                        text: `${info.firstName} ${info.lastName}`,
                        value: registrant.familyGuid
                    });

                    usedFamilyGuids[ registrant.familyGuid ] = true;
                }
            }

            // Add the current person (registrant) if not already added
            if (!usedFamilyGuids[this.registrationEntryState.ownFamilyGuid]) {
                options.push({
                    text: "None",
                    value: this.registrationEntryState.ownFamilyGuid
                });
            }

            return options;
        },
    },
    methods: {
        /** Prefill in the registrar form fields based on the admin's settings */
        prefillRegistrar () {
            this.isRegistrarPanelShown = true;

            // If the option is to prompt or use the current person, prefill the current person if available
            if (this.currentPerson &&
                (this.viewModel.registrarOption === RegistrarOption.UseLoggedInPerson || this.viewModel.registrarOption === RegistrarOption.PromptForRegistrar)) {
                this.registrar.nickName = this.currentPerson.nickName || this.currentPerson.firstName || "";
                this.registrar.lastName = this.currentPerson.lastName || "";
                this.registrar.email = this.currentPerson.email || "";
                this.registrar.familyGuid = this.currentPerson.primaryFamilyGuid || null;
                return;
            }

            if (this.viewModel.registrarOption === RegistrarOption.PromptForRegistrar) {
                return;
            }

            // If prefill or first-registrant, then the first registrants info is used (as least as a starting point)
            if (this.viewModel.registrarOption === RegistrarOption.PrefillFirstRegistrant || this.viewModel.registrarOption === RegistrarOption.UseFirstRegistrant) {
                const firstRegistrantInfo = getRegistrantBasicInfo(this.firstRegistrant, this.viewModel.registrantForms);
                this.registrar.nickName = firstRegistrantInfo.firstName;
                this.registrar.lastName = firstRegistrantInfo.lastName;
                this.registrar.email = firstRegistrantInfo.email;
                this.registrar.familyGuid = this.firstRegistrant.familyGuid;

                const hasAllInfo = (!!this.registrar.nickName) && (!!this.registrar.lastName) && (!!this.registrar.email);

                if (hasAllInfo && this.viewModel.registrarOption === RegistrarOption.UseFirstRegistrant) {
                    this.isRegistrarPanelShown = false;
                }

                return;
            }
        }
    },
    watch: {
        currentPerson: {
            immediate: true,
            handler () {
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
                <StaticFormControl label="First Name" v-model="registrar.nickName" />
                <StaticFormControl label="Email" v-model="registrar.email" />
            </div>
            <div class="col-md-6">
                <StaticFormControl label="Last Name" v-model="registrar.lastName" />
            </div>
        </div>
    </template>
    <template v-else>
        <div class="row">
            <div class="col-md-6">
                <TextBox label="First Name" rules="required" v-model="registrar.nickName" tabIndex="1" />
                <EmailBox label="Send Confirmation Emails To" rules="required" v-model="registrar.email" tabIndex="3" />
                <InlineCheckBox v-if="doShowUpdateEmailOption" label="Should Your Account Be Updated To Use This Email Address?" v-model="registrar.updateEmail" />
            </div>
            <div class="col-md-6">
                <TextBox label="Last Name" rules="required" v-model="registrar.lastName" tabIndex="2" />
                <RadioButtonList
                    v-if="familyOptions.length"
                    :label="(registrar.nickName || 'Person') + ' is in the same immediate family as'"
                    rules='required:{"allowEmptyString": true}'
                    v-model="registrar.familyGuid"
                    :options="familyOptions"
                    validationTitle="Family" />
            </div>
        </div>
    </template>
</div>`
});
