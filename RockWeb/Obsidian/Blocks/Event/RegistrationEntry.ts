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
import { InvokeBlockActionFunc } from '../../Controls/RockBlock';
import NumberUpDown from '../../Elements/NumberUpDown';
import ProgressBar from '../../Elements/ProgressBar';
import RockButton from '../../Elements/RockButton';
import NumberFilter from '../../Filters/Number';
import StringFilter from '../../Filters/String';
import { ConfigurationValues } from '../../Index.js';
import RegistrationInstance from '../../ViewModels/CodeGenerated/RegistrationInstanceViewModel';
import RegistrationTemplateForm from '../../ViewModels/CodeGenerated/RegistrationTemplateFormViewModel';
import RegistrationTemplate from '../../ViewModels/CodeGenerated/RegistrationTemplateViewModel';

export default defineComponent({
    name: 'Event.RegistrationEntry',
    components: {
        NumberUpDown,
        RockButton,
        ProgressBar
    },
    setup() {
        return {
            invokeBlockAction: inject('invokeBlockAction') as InvokeBlockActionFunc,
            configurationValues: inject('configurationValues') as ConfigurationValues
        };
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
            currentRegistrantIndex: 0,
            currentFormIndex: 0,
            numberOfRegistrants: 1,
            registrationInstance: this.configurationValues['registrationInstance'] as RegistrationInstance | null,
            registrationTemplate: this.configurationValues['registrationTemplate'] as RegistrationTemplate | null,
            registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || []) as RegistrationTemplateForm[]
        };
    },
    computed: {
        formCountPerRegistrant(): number {
            return this.registrationTemplateForms.length;
        },
        currentRegistrantTitle(): string {
            const ordinal = NumberFilter.toOrdinal(this.currentRegistrantIndex + 1);
            let title = StringFilter.toTitleCase(
                this.numberOfRegistrants <= 1 ?
                    this.registrantTerm :
                    ordinal + ' ' + this.registrantTerm);

            if (this.currentFormIndex > 0) {
                title += ' (cont)';
            }

            return title;
        },
        registrantTerm(): string {
            return this.registrationTemplate?.RegistrantTerm?.toLowerCase() || 'registrant';
        },
        pluralRegistrantTerm(): string {
            return this.registrationTemplate?.PluralRegistrantTerm?.toLowerCase() || 'registrants';
        },
        registrationInstructions(): string {
            return this.registrationInstance?.RegistrationInstructions || this.registrationTemplate?.RegistrationInstructions || '';
        },
        numberOfPages(): number {
            // All of the steps are 1 page except the "per-registrant"
            return 3 + (this.numberOfRegistrants * this.formCountPerRegistrant);
        },
        completionPercentDecimal(): number {
            switch (this.currentStep) {
                case this.steps.intro:
                    return 0;
                case this.steps.perRegistrantForms:
                    return (1 + this.currentFormIndex + this.currentRegistrantIndex * this.formCountPerRegistrant) / this.numberOfPages;
                case this.steps.registrationForm:
                    return (this.numberOfPages - 2) / this.numberOfPages;
                case this.steps.reviewAndPayment:
                    return (this.numberOfPages - 1) / this.numberOfPages;
                default:
                    return 0;
            }
        },
        completionPercentInt(): number {
            return this.completionPercentDecimal * 100;
        }
    },
    methods: {
        onIntroNext() {
            this.currentStep = this.steps.perRegistrantForms;
            this.currentRegistrantIndex = 0;
            this.currentFormIndex = 0;
        },
        onRegistrantPrevious() {
            const lastFormIndex = this.formCountPerRegistrant - 1;

            if (this.currentFormIndex <= 0 && this.currentRegistrantIndex <= 0) {
                this.currentStep = this.steps.intro;
                return;
            }

            if (this.currentFormIndex <= 0) {
                this.currentRegistrantIndex--;
                this.currentFormIndex = lastFormIndex;
                return;
            }

            this.currentFormIndex--;
        },
        onRegistrantNext() {
            const lastFormIndex = this.formCountPerRegistrant - 1;
            const lastRegistrantIndex = this.numberOfRegistrants - 1;

            if (this.currentFormIndex >= lastFormIndex && this.currentRegistrantIndex >= lastRegistrantIndex) {
                this.currentStep = this.steps.registrationForm;
                return;
            }

            if (this.currentFormIndex >= lastFormIndex) {
                this.currentRegistrantIndex++;
                this.currentFormIndex = 0;
                return;
            }

            this.currentFormIndex++;
        },
        onRegistrationPrevious() {
            const lastFormIndex = this.formCountPerRegistrant - 1;
            const lastRegistrantIndex = this.numberOfRegistrants - 1;

            this.currentStep = this.steps.perRegistrantForms;
            this.currentRegistrantIndex = lastRegistrantIndex;
            this.currentFormIndex = lastFormIndex;
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

    <div v-if="currentStep === steps.intro" class="registrationentry-intro">
        <div class="text-left" v-html="registrationInstructions">
        </div>
        <div class="registrationentry-intro">
            <h1>How many {{pluralRegistrantTerm}} will you be registering?</h1>
            <NumberUpDown v-model="numberOfRegistrants" class="margin-t-sm input-lg" />
        </div>
        <div class="actions">
            <RockButton btnType="primary" class="pull-right" @click="onIntroNext">
                Next
            </RockButton>
        </div>
    </div>

    <div v-if="currentStep === steps.perRegistrantForms" class="registrationentry-registrant">
        <h1>{{currentRegistrantTitle}}</h1>
        <ProgressBar :percent="completionPercentInt" />

        <div class="actions">
            <RockButton btnType="default" @click="onRegistrantPrevious">
                Previous
            </RockButton>
            <RockButton btnType="primary" class="pull-right" @click="onRegistrantNext">
                Next
            </RockButton>
        </div>
    </div>

    <div v-if="currentStep === steps.registrationForm" class="registrationentry-registration-attributes">
        <h1>Registration Attributes</h1>
        <ProgressBar :percent="completionPercentInt" />

        <div class="actions">
            <RockButton btnType="default" @click="onRegistrationPrevious">
                Previous
            </RockButton>
            <RockButton btnType="primary" class="pull-right" @click="onRegistrationNext">
                Next
            </RockButton>
        </div>
    </div>

    <div v-if="currentStep === steps.reviewAndPayment" class="registrationentry-summary">
        <h1>Summary</h1>
        <ProgressBar :percent="completionPercentInt" />

        <div class="actions">
            <RockButton btnType="default" @click="onSummaryPrevious">
                Previous
            </RockButton>
        </div>
    </div>

</div>`
});
