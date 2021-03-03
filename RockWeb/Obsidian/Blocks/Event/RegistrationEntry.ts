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

import { defineComponent } from 'vue';
import RockButton from '../../Elements/RockButton';
import { Guid } from '../../Util/Guid';
import RegistrationEntryIntro from './RegistrationEntry/Intro';
import RegistrationEntryRegistrant from './RegistrationEntry/Registrant';
import RegistrationEntryRegistration from './RegistrationEntry/Registration';
import RegistrationEntrySummary from './RegistrationEntry/Summary';

export type RegistrantInfo = {
    FamilyGuid: Guid | null
};

export default defineComponent({
    name: 'Event.RegistrationEntry',
    components: {
        RockButton,
        RegistrationEntryIntro,
        RegistrationEntryRegistrant,
        RegistrationEntryRegistration,
        RegistrationEntrySummary
    },
    data() {
        const steps = {
            intro: 'intro',
            perRegistrantForms: 'perRegistrantForms',
            registrationForm: 'registrationForm',
            reviewAndPayment: 'reviewAndPayment'
        };

        return {
            steps,
            currentStep: steps.intro,
            registrants: [] as RegistrantInfo[]
        };
    },
    methods: {
        onIntroNext({ numberOfRegistrants }) {
            // Resize the registrant array to match the selected number
            while (numberOfRegistrants > this.registrants.length) {
                this.registrants.push({ FamilyGuid: null });
            }

            this.registrants.length = numberOfRegistrants;

            // Advance to the next step
            this.currentStep = this.steps.perRegistrantForms;
        },
        onRegistrantPrevious() {
            this.currentStep = this.steps.intro;
        },
        onRegistrantNext() {
            this.currentStep = this.steps.registrationForm;
        },
        onRegistrationPrevious() {
            this.currentStep = this.steps.perRegistrantForms;
        },
        onRegistrationNext() {
            this.currentStep = this.steps.reviewAndPayment;
        },
        onSummaryPrevious() {
            this.currentStep = this.steps.registrationForm;
        }
    },
    template: `
<div>
    <RegistrationEntryIntro v-if="currentStep === steps.intro" @next="onIntroNext" :initialRegistrantCount="registrants.length" />
    <RegistrationEntryRegistrant v-else-if="currentStep === steps.perRegistrantForms" :registrants="registrants" @next="onRegistrantNext" @previous="onRegistrantPrevious" />
    <RegistrationEntryRegistration v-else-if="currentStep === steps.registrationForm" :registrants="registrants" @next="onRegistrationNext" @previous="onRegistrationPrevious" />
    <RegistrationEntrySummary v-else-if="currentStep === steps.reviewAndPayment" :registrants="registrants" @previous="onSummaryPrevious" />
</div>`
});
