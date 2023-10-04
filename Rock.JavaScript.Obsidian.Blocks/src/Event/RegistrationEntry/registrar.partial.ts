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
import InlineCheckBox from "@Obsidian/Controls/inlineCheckBox";
import EmailBox from "@Obsidian/Controls/emailBox";
import RadioButtonList from "@Obsidian/Controls/radioButtonList";
import StaticFormControl from "@Obsidian/Controls/staticFormControl";
import TextBox from "@Obsidian/Controls/textBox";
import { getRegistrantBasicInfo,  } from "./utils.partial";
import { RegistrantInfo, RegistrantsSameFamily, RegistrarInfo, RegistrarOption, RegistrationEntryBlockViewModel, RegistrantBasicInfo, RegistrationEntryState, RegistrationEntryBlockArgs } from "./types.partial";
import { useStore } from "@Obsidian/PageState";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
// LPC CODE
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import DropDownList from "@Obsidian/Controls/dropDownList";
import PhoneNumberBox from "@Obsidian/Controls/phoneNumberBox.obs";
// END LPC CODE

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

export default defineComponent({
    name: "Event.RegistrationEntry.Registrar",
    components: {
        // LPC CODE
        PhoneNumberBox,
        DropDownList,
        // END LPC CODE
        TextBox,
        InlineCheckBox,
        EmailBox,
        StaticFormControl,
        RadioButtonList
    },
    setup () {
        // LPC CODE
        const invokeBlockAction = useInvokeBlockAction();

        const languageOptions: ListItemBag[] = [
            { text: "English", value: "English" },
            { text: "Español", value: "Spanish" }
        ];
        // END LPC CODE

        return {
            // LPC CODE
            languageOptions,
            invokeBlockAction,
            // END LPC CODE
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
        currentPerson (): PersonBag | null {
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
        familyOptions (): ListItemBag[] {
            const options: ListItemBag[] = [];
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
                    // MODIFIED LPC CODE
                    text: getLang() == 'es' ? 'Ninguno' : 'None',
                    // END MODIFIED LPC CODE
                    value: this.registrationEntryState.ownFamilyGuid
                });
            }

            return options;
        },
    },
    methods: {
        // LPC CODE
        getLang,
        // END LPC CODE
        /** Prefill in the registrar form fields based on the admin's settings */
        // LPC MODIFIED CODE
        async prefillRegistrar() {
        // END LPC MODIFIED CODE
            this.isRegistrarPanelShown = true;

            // If the option is to prompt or use the current person, prefill the current person if available
            if (this.currentPerson &&
                (this.viewModel.registrarOption === RegistrarOption.UseLoggedInPerson || this.viewModel.registrarOption === RegistrarOption.PromptForRegistrar)) {
                this.registrar.nickName = this.currentPerson.nickName || this.currentPerson.firstName || "";
                this.registrar.lastName = this.currentPerson.lastName || "";
                this.registrar.email = this.currentPerson.email || "";
                this.registrar.familyGuid = this.currentPerson.primaryFamilyGuid || null;

                // LPC CODE
                const phoneResult = await this.invokeBlockAction<string>("GetMobilePhone", { args: this.getRegistrationEntryBlockArgs() });
                this.registrar.mobilePhone = phoneResult.data || "";

                const langResult = await this.invokeBlockAction<string>("GetPreferredLanguage", { args: this.getRegistrationEntryBlockArgs() });
                this.registrar.preferredLanguage = langResult.data || "";

                if (this.registrar.preferredLanguage == "") {
                    if (getLang() == 'es') {
                        this.registrar.preferredLanguage = "Spanish";
                    }
                    else {
                        this.registrar.preferredLanguage = "English";
                    }
                }
                // END LPC CODE

                return;
            }

            // MODIFIED LPC CODE
            if (this.viewModel.registrarOption === RegistrarOption.UseLoggedInPerson || this.viewModel.registrarOption === RegistrarOption.PromptForRegistrar) {
                if (getLang() == 'es') {
                    this.registrar.preferredLanguage = "Spanish";
                }
                else {
                    this.registrar.preferredLanguage = "English";
                }
            // END MODIFIED LPC CODE

                return;
            }

            // If prefill or first-registrant, then the first registrants info is used (as least as a starting point)
            if (this.viewModel.registrarOption === RegistrarOption.PrefillFirstRegistrant || this.viewModel.registrarOption === RegistrarOption.UseFirstRegistrant) {
                const firstRegistrantInfo = getRegistrantBasicInfo(this.firstRegistrant, this.viewModel.registrantForms);
                this.registrar.nickName = firstRegistrantInfo.firstName;
                this.registrar.lastName = firstRegistrantInfo.lastName;
                this.registrar.email = firstRegistrantInfo.email;
                this.registrar.familyGuid = this.firstRegistrant.familyGuid;
                // LPC CODE
                this.registrar.mobilePhone = firstRegistrantInfo.mobilePhone;

                if (getLang() == 'es') {
                    this.registrar.preferredLanguage = "Spanish";
                }
                else {
                    this.registrar.preferredLanguage = "English";
                }
                // END LPC CODE

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
    <!-- MODIFIED LPC CODE -->
    <h4 v-if="getLang() == 'es'">Este Registro Fue Completado Por </h4>
    <h4 v-else>This Registration Was Completed By</h4>
    <!-- END MODIFIED LPC CODE -->
    <template v-if="useLoggedInPersonForRegistrar">
        <!-- MODIFIED LPC CODE -->
        <div class="row">
            <div class="col-md-6 col-sm-6">
                <StaticFormControl :label="getLang() == 'es' ? 'Nombre' : 'First Name'" v-model="registrar.nickName" />
          </div>
            <div class="col-md-6 col-sm-6">
                <StaticFormControl :label="getLang() == 'es' ? 'Apellido' : 'Last Name'" v-model="registrar.lastName" />
          </div>
        </div>
        <div class="row">
            <div class="col-md-6 col-sm-6">
            <StaticFormControl label="Email" v-model="registrar.email" />
          </div>
            <div class="col-md-6 col-sm-6">
                <StaticFormControl :label="getLang() == 'es' ? 'Teléfono' : 'Mobile Phone'" v-model="registrar.mobilePhone" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <StaticFormControl :label="getLang() == 'es' ? 'Idioma Preferido' : 'Preferred Language'" v-model="registrar.preferredLanguage" />
            </div>
        </div>
        <!-- END MODIFIED LPC CODE -->
    </template>
    <template v-else>
        <!-- MODIFIED LPC CODE -->
        <div class="row">
            <div class="col-md-6 col-sm-6">
                <TextBox :label="getLang() == 'es' ? 'Nombre' : 'First Name'" rules="required" v-model="registrar.nickName" tabIndex="1" />
            </div>
            <div class="col-md-6 col-sm-6">
                <TextBox :label="getLang() == 'es' ? 'Apellido' : 'Last Name'" v-model="registrar.lastName" tabIndex="2" />
                <RadioButtonList
                    v-if="familyOptions.length"
                    :label="(registrar.nickName || (getLang() == 'es'? 'La persona' : 'Person')) + ' ' + (getLang() == 'es'? 'está en la misma familia inmediata que' : 'is in the same immediate family as')"
                    rules='required:{"allowEmptyString": true}'
                    v-model="registrar.familyGuid"
                    :items="familyOptions"
                    validationTitle="Family" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <EmailBox :label="getLang() == 'es' ? 'Mandar Email de Confirmación a' : 'Send Confirmation Emails To'"  rules="required" v-model="registrar.email" tabIndex="3" />
                <InlineCheckBox v-if="doShowUpdateEmailOption" :label="getLang() == 'es' ? '¿Deseas actualizar tu cuenta para usar este email?' : 'Should Your Account Be Updated To Use This Email Address?'" v-model="registrar.updateEmail" />
            </div>
            <div class="col-md-6">
                <PhoneNumberBox
                    :label="getLang() == 'es' ? 'Teléfono' : 'Mobile Phone'"
                    v-model="registrar.mobilePhone"
                    rules="required"
                    tabIndex="4" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <DropDownList
                    :label="getLang() == 'es' ? 'Idioma Preferido' : 'Preferred Language'"
                    v-model="registrar.preferredLanguage"
                    :items="languageOptions"
                    rules="required"
                    tabIndex="5" />
            </div>
        </div>
        <!-- END MODIFIED LPC CODE -->
    </template>
</div>`
});
