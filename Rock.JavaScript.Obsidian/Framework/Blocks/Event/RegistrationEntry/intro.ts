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
import Alert from "../../../Elements/alert";
import NumberUpDown from "../../../Elements/numberUpDown";
import RockButton from "../../../Elements/rockButton";
import { toTitleCase, pluralConditional } from "../../../Services/string";
import { areEqual } from "../../../Util/guid";
import { Person } from "../../../ViewModels";
import { getDefaultRegistrantInfo, getForcedFamilyGuid, RegistrationEntryState } from "../registrationEntry";
import { RegistrationEntryBlockViewModel } from "./registrationEntryBlockViewModel";
import { useStore } from "../../../Store/index";

const store = useStore();

export default defineComponent({
    name: "Event.RegistrationEntry.Intro",
    components: {
        NumberUpDown,
        RockButton,
        Alert
    },
    data () {
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
        currentPerson (): Person | null {
            return store.state.currentPerson;
        },

        /** The view model sent by the C# code behind. This is just a convenient shortcut to the shared object. */
        viewModel (): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },

        /** The number of these registrants that will be placed on a waitlist because of capacity rules */
        numberToAddToWaitlist (): number {
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
        remainingCapacityPhrase (): string {
            const spots = this.viewModel.spotsRemaining;

            if (spots === null) {
                return "";
            }

            return pluralConditional(spots, `1 more ${this.registrantTerm}`, `${spots} more ${this.registrantTermPlural}`);
        },

        /** Is this instance full and no one else can register? */
        isFull (): boolean {
            if (this.viewModel.spotsRemaining === null) {
                return false;
            }

            return this.viewModel.spotsRemaining < 1;
        },

        /** True if the user is allowed to move on to the next screen. */
        canContinue(): boolean {
            return !(this.isFull && this.numberToAddToWaitlist !== this.numberOfRegistrants);
        },

        registrantTerm (): string {
            this.viewModel.instanceName;
            return (this.viewModel.registrantTerm || "registrant").toLowerCase();
        },
        registrantTermPlural (): string {
            return (this.viewModel.pluralRegistrantTerm || "registrants").toLowerCase();
        },
        registrationTerm (): string {
            return (this.viewModel.registrationTerm || "registration").toLowerCase();
        },
        registrationTermPlural (): string {
            return (this.viewModel.pluralRegistrationTerm || "registrations").toLowerCase();
        },
        registrationTermTitleCase (): string {
            return toTitleCase(this.registrationTerm);
        }
    },
    methods: {
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
    <Alert v-if="isFull && numberToAddToWaitlist !== numberOfRegistrants" class="text-left" alertType="warning">
        <strong>{{registrationTermTitleCase}} Full</strong>
        <p>
            There are not any more {{registrationTermPlural}} available for {{viewModel.instanceName}}. 
        </p>
    </Alert>
    <Alert v-if="showRemainingCapacity" class="text-left" alertType="warning">
        <strong>{{registrationTermTitleCase}} Full</strong>
        <p>
            This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.
        </p>
    </Alert>
    <div class="text-left" v-html="viewModel.instructionsHtml">
    </div>
    <div v-if="viewModel.maxRegistrants > 1" class="registrationentry-intro">
        <h1>How many {{viewModel.pluralRegistrantTerm}} will you be registering?</h1>
        <NumberUpDown v-model="numberOfRegistrants" class="margin-t-sm" numberIncrementClasses="input-lg" :max="viewModel.maxRegistrants" />
    </div>
    <Alert v-if="viewModel.timeoutMinutes" alertType="info" class="text-left">
        Due to a high-volume of expected interest, your {{registrationTerm}} session will expire after
        {{pluralConditional(viewModel.timeoutMinutes, 'a minute', viewModel.timeoutMinutes + ' minutes')}}
        of inactivity.
    </Alert>
    <Alert v-if="numberToAddToWaitlist === numberOfRegistrants" class="text-left" alertType="warning">
        This {{registrationTerm}} has reached its capacity. Complete the registration to be added to the waitlist.
    </Alert>
    <Alert v-else-if="numberToAddToWaitlist" class="text-left" alertType="warning">
        This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.
        The first {{pluralConditional(viewModel.spotsRemaining, registrantTerm, viewModel.spotsRemaining + ' ' + registrantTermPlural)}} you add will be registered for {{viewModel.instanceName}}.
        The remaining {{pluralConditional(numberToAddToWaitlist, registrantTerm, numberToAddToWaitlist + ' ' + registrantTermPlural)}} will be added to the waitlist. 
    </Alert>

    <div v-if="canContinue" class="actions text-right">
        <RockButton btnType="primary" @click="onNext">
            Next
        </RockButton>
    </div>
</div>`
});
