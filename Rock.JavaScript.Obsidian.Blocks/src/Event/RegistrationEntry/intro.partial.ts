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
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import NumberUpDown from "@Obsidian/Controls/numberUpDown";
import RockButton from "@Obsidian/Controls/rockButton";
import { toTitleCase, pluralConditional } from "@Obsidian/Utility/stringUtils";
import { areEqual } from "@Obsidian/Utility/guid";
import { getDefaultRegistrantInfo, getForcedFamilyGuid } from "./utils.partial";
import { RegistrationEntryBlockViewModel, RegistrationEntryState } from "./types.partial";
import { useStore } from "@Obsidian/PageState";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";

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
    name: "Event.RegistrationEntry.Intro",
    components: {
        NumberUpDown,
        RockButton,
        NotificationBox
    },
    data() {
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;

        return {
            /** The number of registrants that this registrar is going to input */
            numberOfRegistrants: registrationEntryState.registrants.length,

            /** The shared state among all the components that make up this block */
            registrationEntryState,

            /** Should the remaining capacity warning be shown? */
            showRemainingCapacity: false
        };
    },
    computed: {
        /** The currently authenticated person */
        currentPerson(): PersonBag | null {
            return store.state.currentPerson;
        },

        /** The view model sent by the C# code behind. This is just a convenient shortcut to the shared object. */
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },

        /** The number of these registrants that will be placed on a waitlist because of capacity rules */
        numberToAddToWaitlist(): number {
            if (this.viewModel.spotsRemaining === null || !this.viewModel.waitListEnabled) {
                // There is no waitlist or no cap on number of attendees
                return 0;
            }

            if (this.viewModel.spotsRemaining >= this.numberOfRegistrants) {
                // There is enough capacity left for all of these registrants
                return 0;
            }

            // Some or all need to go on the waitlist
            return this.numberOfRegistrants - this.viewModel.spotsRemaining;
        },

        /** The capacity left phrase: Ex: 1 more camper */
        remainingCapacityPhrase(): string {
            const spots = this.viewModel.spotsRemaining;

            if (spots === null) {
                return "";
            }

            // MODIFIED LPC CODE
            if (this.getLang() == 'es') {
                return pluralConditional(spots, `1 ${this.registrantTerm} más`, `${spots} ${this.registrantTermPlural} más`);
            } else {
            return pluralConditional(spots, `1 more ${this.registrantTerm}`, `${spots} more ${this.registrantTermPlural}`);
            }
            // END MODIFIED LPC CODE
        },

        /** Is this instance full and no one else can register? */
        isFull(): boolean {
            if (this.viewModel.spotsRemaining === null) {
                return false;
            }

            return this.viewModel.spotsRemaining < 1;
        },

        /** True if the user is allowed to move on to the next screen. */
        canContinue(): boolean {
            return !(this.isFull && this.numberToAddToWaitlist !== this.numberOfRegistrants);
        },

        // MODIFIED LPC CODE
        registrantTerm(): string {
            if (this.getLang() == 'es') {
                return 'persona';
            } else {
            return (this.viewModel.registrantTerm || "registrant").toLowerCase();
            }
        },
        registrantTermPlural(): string {
            if (this.getLang() == 'es') {
                return 'personas';
            } else {
            return (this.viewModel.pluralRegistrantTerm || "registrants").toLowerCase();
            }
        },
        registrationTerm(): string {
            if (this.getLang() == 'es') {
                return 'registro';
            } else {
            return (this.viewModel.registrationTerm || "registration").toLowerCase();
            }
        },
        registrationTermPlural(): string {
            if (this.getLang() == 'es') {
                return 'registros';
            } else {
            return (this.viewModel.pluralRegistrationTerm || "registrations").toLowerCase();
            }
        },
        // END MODIFIED LPC CODE
        registrationTermTitleCase(): string {
            return toTitleCase(this.registrationTerm);
        }
    },
    methods: {
        // LPC CODE
        getLang,
        // END LPC CODE
        pluralConditional,
        onNext(): void {
            // If the person is authenticated and the setting is to put registrants in the same family, then we force that family guid
            const forcedFamilyGuid = getForcedFamilyGuid(this.currentPerson, this.viewModel);

            const usedFamilyMemberGuids = this.registrationEntryState.registrants
                .filter(r => r.personGuid)
                .map(r => r.personGuid);

            const availableFamilyMembers = this.viewModel.familyMembers
                .filter(fm =>
                    areEqual(fm.familyGuid, forcedFamilyGuid) &&
                    !usedFamilyMemberGuids.includes(fm.guid));

            // Resize the registrant array to match the selected number
            while (this.numberOfRegistrants > this.registrationEntryState.registrants.length) {
                const registrant = getDefaultRegistrantInfo(this.currentPerson, this.viewModel, forcedFamilyGuid);
                this.registrationEntryState.registrants.push(registrant);
            }

            this.registrationEntryState.registrants.length = this.numberOfRegistrants;

            // Set people beyond the capacity to be on the waitlist
            const firstWaitListIndex = this.numberOfRegistrants - this.numberToAddToWaitlist;

            for (let i = firstWaitListIndex; i < this.numberOfRegistrants; i++) {
                this.registrationEntryState.registrants[i].isOnWaitList = true;
            }

            this.$emit("next");
        },
    },
    watch: {
        numberOfRegistrants(): void {
            if (!this.viewModel.waitListEnabled && this.viewModel.spotsRemaining !== null && this.viewModel.spotsRemaining < this.numberOfRegistrants) {
                this.showRemainingCapacity = true;
                const spotsRemaining = this.viewModel.spotsRemaining;

                // Do this on the next tick to allow the events to finish. Otherwise the component tree doesn't have time
                // to respond to this, since the watch was triggered by the numberOfRegistrants change
                this.$nextTick(() => this.numberOfRegistrants = spotsRemaining);
            }
        }
    },
    template: `
<div class="registrationentry-intro">
    <NotificationBox v-if="isFull && numberToAddToWaitlist !== numberOfRegistrants" class="text-left" alertType="warning">
        <!-- MODIFIED LPC CODE -->
            <strong v-if="getLang() == 'es'">{{registrationTermTitleCase}} Lleno</strong>
            <strong v-else>{{registrationTermTitleCase}} Full. </strong>
            <p v-if="getLang() == 'es'">No hay más {{registrationTermPlural}} disponibles para {{viewModel.instanceName}}.</p>
            <p v-else>There are not any more {{registrationTermPlural}} available for {{viewModel.instanceName}}.</p>
        <!-- END MODIFIED LPC CODE -->
    </NotificationBox>
    <NotificationBox v-if="showRemainingCapacity" class="text-left" alertType="warning">
        <!-- MODIFIED LPC CODE -->
            <strong v-if="getLang() == 'es'">{{registrationTermTitleCase}} Lleno</strong>
            <strong v-else>{{registrationTermTitleCase}} Full. </strong>
            <p v-if="getLang() == 'es'">Este {{registrationTerm}} solo tiene capacidad para {{remainingCapacityPhrase}}.</p>
            <p v-else>This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.</p>
       <!-- END MODIFIED LPC CODE -->
    </NotificationBox>
    <div class="text-left" v-html="viewModel.instructionsHtml">
    </div>
    <div v-if="viewModel.maxRegistrants > 1" class="registrationentry-intro">
        <!-- MODIFIED LPC CODE -->
            <h1 v-if="getLang() == 'es'"> ¿Cuántas Personas Estarás Registrando?</h1>
            <h1 v-else>How many {{viewModel.pluralRegistrantTerm}} will you be registering?</h1>
        <!-- END MODIFIED LPC CODE -->
        <NumberUpDown v-model="numberOfRegistrants" class="margin-t-sm" numberIncrementClasses="input-lg" :max="viewModel.maxRegistrants" />
    </div>
    <NotificationBox v-if="viewModel.timeoutMinutes" alertType="info" class="text-left">
        Due to a high-volume of expected interest, your {{registrationTerm}} session will expire after
        {{pluralConditional(viewModel.timeoutMinutes, 'a minute', viewModel.timeoutMinutes + ' minutes')}}
        of inactivity.
    </NotificationBox>
    <NotificationBox v-if="numberToAddToWaitlist === numberOfRegistrants" class="text-left" alertType="warning">
        <!-- MODIFIED LPC CODE -->
            <p v-if="getLang() == 'es'">Este {{registrationTerm}} ha llegado a su capacidad. Si lo deseas, puedes completar el registro para agregarte a una lista de espera.</p>
            <p v-else>This {{registrationTerm}} has reached its capacity. Complete the registration to be added to the waitlist.</p>
        <!-- END MODIFIED LPC CODE -->
    </NotificationBox>
    <NotificationBox v-else-if="numberToAddToWaitlist" class="text-left" alertType="warning">
		<!-- MODIFIED LPC CODE -->
        <p v-if="getLang() == 'es'">
            Este {{registrationTerm}} solo tiene capacidad para {{remainingCapacityPhrase}}.
            La primera {{pluralConditional(viewModel.spotsRemaining, registrantTerm, viewModel.spotsRemaining + ' ' + registrantTermPlural)}} que agregue se registrará para {{viewModel.instanceName}}.
            El resto se agregarán a la lista de espera. 
        </p>
        <p v-else>
        This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.
        The first {{pluralConditional(viewModel.spotsRemaining, registrantTerm, viewModel.spotsRemaining + ' ' + registrantTermPlural)}} you add will be registered for {{viewModel.instanceName}}.
        The remaining {{pluralConditional(numberToAddToWaitlist, registrantTerm, numberToAddToWaitlist + ' ' + registrantTermPlural)}} will be added to the waitlist.
    	</p>
        <!-- END MODIFIED LPC CODE -->
    </NotificationBox>

    <div v-if="canContinue" class="actions text-right">
        <!-- MODIFIED LPC CODE -->
        <RockButton btnType="primary" @click="onNext">
            {{ getLang() == 'es' ? 'Siguiente' : 'Next' }}
        </RockButton>
        <!-- END MODIFIED LPC CODE -->
    </div>
</div>`
});
