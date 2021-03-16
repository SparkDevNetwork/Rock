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

import { defineComponent, inject, provide, reactive } from 'vue';
import RockButton from '../../Elements/RockButton';
import { Guid } from '../../Util/Guid';
import RegistrationEntryIntro from './RegistrationEntry/Intro';
import RegistrationEntryRegistrants from './RegistrationEntry/Registrants';
import { RegistrationEntryBlockViewModel } from './RegistrationEntry/RegistrationEntryBlockViewModel';
import RegistrationEntryRegistrationStart from './RegistrationEntry/RegistrationStart';
import RegistrationEntryRegistrationEnd from './RegistrationEntry/RegistrationEnd';
import RegistrationEntrySummary from './RegistrationEntry/Summary';
import Registrants from './RegistrationEntry/Registrants';
import ProgressBar from '../../Elements/ProgressBar';
import NumberFilter from '../../Services/Number';
import StringFilter from '../../Services/String';
import Alert from '../../Elements/Alert';

export type RegistrantInfo = {
    FamilyGuid: Guid | null;
    PersonGuid: Guid;
    FieldValues: Record<Guid, unknown>;
    FeeQuantities: Record<Guid, number>;
    Guid: Guid;
};

export type RegistrationEntryState = {
    Steps: Record<string, string>;
    ViewModel: RegistrationEntryBlockViewModel;
    CurrentStep: string;
    CurrentRegistrantIndex: number;
    CurrentRegistrantFormIndex: number;
    Registrants: RegistrantInfo[];
    RegistrationFieldValues: Record<Guid, unknown>;
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
        RegistrationEntrySummary,
        ProgressBar,
        Alert
    },
    setup() {
        const steps = {
            intro: 'intro',
            registrationStartForm: 'registrationStartForm',
            perRegistrantForms: 'perRegistrantForms',
            registrationEndForm: 'registrationEndForm',
            reviewAndPayment: 'reviewAndPayment'
        };

        const viewModel = inject('configurationValues') as RegistrationEntryBlockViewModel;

        const registrationEntryState = reactive({
            Steps: steps,
            ViewModel: viewModel,
            CurrentStep: steps.intro,
            CurrentRegistrantFormIndex: 0,
            CurrentRegistrantIndex: 0,
            Registrants: [] as RegistrantInfo[],
            RegistrationFieldValues: {} as Record<Guid, unknown>
        }) as RegistrationEntryState;

        provide('registrationEntryState', registrationEntryState);

        return {
            viewModel,
            steps,
            registrationEntryState
        };
    },
    computed: {
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.ViewModel;
        },
        currentStep(): string {
            return this.registrationEntryState.CurrentStep;
        },
        registrants(): RegistrantInfo[] {
            return this.registrationEntryState.Registrants;
        },
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
        },
        completionPercentDecimal(): number {
            if (this.currentStep === this.steps.intro) {
                return 0;
            }

            if (this.currentStep === this.steps.registrationStartForm) {
                return 1 / this.numberOfPages;
            }

            if (this.currentStep === this.steps.perRegistrantForms) {
                const firstRegistrantPage = this.viewModel.RegistrationAttributesStart.length === 0 ? 1 : 2;
                const finishedRegistrantForms = this.registrationEntryState.CurrentRegistrantIndex * this.viewModel.RegistrantForms.length;
                return (firstRegistrantPage + this.registrationEntryState.CurrentRegistrantFormIndex + finishedRegistrantForms) / this.numberOfPages;
            }

            if (this.currentStep === this.steps.registrationEndForm) {
                return (this.numberOfPages - 2) / this.numberOfPages;
            }

            if (this.currentStep === this.steps.reviewAndPayment) {
                return (this.numberOfPages - 1) / this.numberOfPages;
            }

            return 0;
        },
        completionPercentInt(): number {
            return this.completionPercentDecimal * 100;
        },
        uppercaseRegistrantTerm(): string {
            return StringFilter.toTitleCase(this.viewModel.RegistrantTerm);
        },
        currentRegistrantTitle(): string {
            const ordinal = NumberFilter.toOrdinal(this.registrationEntryState.CurrentRegistrantIndex + 1);
            let title = StringFilter.toTitleCase(
                this.registrants.length <= 1 ?
                    this.uppercaseRegistrantTerm :
                    ordinal + ' ' + this.uppercaseRegistrantTerm);

            if (this.registrationEntryState.CurrentRegistrantFormIndex > 0) {
                title += ' (cont)';
            }

            return title;
        },
        stepTitle(): string {
            if (this.currentStep === this.steps.registrationStartForm) {
                return this.viewModel.RegistrationAttributeTitleStart;
            }

            if (this.currentStep === this.steps.perRegistrantForms) {
                return this.currentRegistrantTitle;
            }

            if (this.currentStep === this.steps.registrationStartForm) {
                return this.viewModel.RegistrationAttributeTitleEnd;
            }

            return '';
        }
    },
    methods: {
        onIntroNext() {
            this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
        },
        onRegistrationStartPrevious() {
            this.registrationEntryState.CurrentStep = this.steps.intro;
        },
        onRegistrationStartNext() {
            this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
        },
        onRegistrantPrevious() {
            this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
        },
        onRegistrantNext() {
            this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
        },
        onRegistrationEndPrevious() {
            this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
        },
        onRegistrationEndNext() {
            this.registrationEntryState.CurrentStep = this.steps.reviewAndPayment;
        },
        onSummaryPrevious() {
            this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
        }
    },
    template: `
<div>
    <template v-if="currentStep !== steps.intro">
        <h1>{{stepTitle}}</h1>
        <ProgressBar :percent="completionPercentInt" />
    </template>

    <RegistrationEntryIntro v-if="currentStep === steps.intro" @next="onIntroNext" />
    <RegistrationEntryRegistrationStart v-else-if="currentStep === steps.registrationStartForm" @next="onRegistrationStartNext" @previous="onRegistrationStartPrevious" />
    <RegistrationEntryRegistrants v-else-if="currentStep === steps.perRegistrantForms" @next="onRegistrantNext" @previous="onRegistrantPrevious" />
    <RegistrationEntryRegistrationEnd v-else-if="currentStep === steps.registrationEndForm" @next="onRegistrationEndNext" @previous="onRegistrationEndPrevious" />
    <RegistrationEntrySummary v-else-if="currentStep === steps.reviewAndPayment" @previous="onSummaryPrevious" />
    <Alert v-else alertType="danger">Invalid State: '{{currentStep}}'</Alert>
</div>`
});
