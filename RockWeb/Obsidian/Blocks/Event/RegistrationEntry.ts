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
import RockButton from '../../Elements/RockButton';
import { Guid, newGuid } from '../../Util/Guid';
import RegistrationEntryIntro from './RegistrationEntry/Intro';
import RegistrationEntryRegistrants from './RegistrationEntry/Registrants';
import { RegistrationEntryBlockViewModel } from './RegistrationEntry/RegistrationEntryBlockViewModel';
import RegistrationEntryRegistrationStart from './RegistrationEntry/RegistrationStart';
import RegistrationEntryRegistrationEnd from './RegistrationEntry/RegistrationEnd';
import RegistrationEntrySummary from './RegistrationEntry/Summary';
import Registrants from './RegistrationEntry/Registrants';

export type RegistrantInfo = {
    FamilyGuid: Guid | null;
    FieldValues: Record<Guid, unknown>;
    FeeQuantities: Record<Guid, number>;
    Guid: Guid;
};

export default defineComponent({
    name: 'Event.RegistrationEntry',
    components: {
        RockButton,
        Registrants,
        RegistrationEntryIntro,
        RegistrationEntryRegistrants,
        RegistrationEntryRegistrationStart,
        RegistrationEntryRegistrationEnd,
        RegistrationEntrySummary
    },
    setup() {
        return {
            viewModel: inject('configurationValues') as RegistrationEntryBlockViewModel
        };
    },
    data() {
        const steps = {
            intro: 'intro',
            registrationStartForm: 'registrationStartForm',
            perRegistrantForms: 'perRegistrantForms',
            registrationEndForm: 'registrationEndForm',
            reviewAndPayment: 'reviewAndPayment'
        };

        return {
            steps,
            currentStep: steps.intro,
            registrants: [] as RegistrantInfo[],
            registrationFieldValues: {} as Record<Guid, unknown>
        };
    },
    methods: {
        onIntroNext({ numberOfRegistrants }) {
            // Resize the registrant array to match the selected number
            while (numberOfRegistrants > this.registrants.length) {
                this.registrants.push({
                    FamilyGuid: null,
                    FieldValues: {},
                    FeeQuantities: {},
                    Guid: newGuid()
                });
            }

            this.registrants.length = numberOfRegistrants;

            // Advance to the next step
            this.currentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
        },
        onRegistrationStartPrevious() {
            this.currentStep = this.steps.intro;
        },
        onRegistrationStartNext() {
            this.currentStep = this.steps.perRegistrantForms;
        },
        onRegistrantPrevious() {
            this.currentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
        },
        onRegistrantNext() {
            this.currentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
        },
        onRegistrationEndPrevious() {
            this.currentStep = this.steps.perRegistrantForms;
        },
        onRegistrationEndNext() {
            this.currentStep = this.steps.reviewAndPayment;
        },
        onSummaryPrevious() {
            this.currentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
        }
    },
    computed: {
        hasPreAttributes(): boolean {
            return this.viewModel.RegistrationAttributesStart.length > 0;
        },
        hasPostAttributes(): boolean {
            return this.viewModel.RegistrationAttributesEnd.length > 0;
        },
        numberOfPages(): number {
            return 2 + // Intro and summary
                (this.hasPostAttributes ? 1 : 0) +
                (this.hasPreAttributes ? 1 : 0) +
                (this.viewModel.RegistrantForms.length * this.registrants.length);
        }
    },
    template: `
<div>
    <RegistrationEntryIntro v-if="currentStep === steps.intro" @next="onIntroNext" :initialRegistrantCount="registrants.length" />
    <RegistrationEntryRegistrationStart v-else-if="currentStep === steps.registrationStartForm" :registrationFieldValues="registrationFieldValues" :registrantCount="registrants.length" @next="onRegistrationStartNext" @previous="onRegistrationStartPrevious"  :numberOfPages="numberOfPages" />
    <RegistrationEntryRegistrants v-else-if="currentStep === steps.perRegistrantForms" :registrants="registrants" @next="onRegistrantNext" @previous="onRegistrantPrevious" :numberOfPages="numberOfPages" />
    <RegistrationEntryRegistrationEnd v-else-if="currentStep === steps.registrationEndForm" :registrationFieldValues="registrationFieldValues" @next="onRegistrationEndNext" @previous="onRegistrationEndPrevious" :numberOfPages="numberOfPages" />
    <RegistrationEntrySummary v-else-if="currentStep === steps.reviewAndPayment" :registrants="registrants" @previous="onSummaryPrevious" />
</div>`
});
