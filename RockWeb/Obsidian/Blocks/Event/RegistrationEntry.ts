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
import RockButton from '../../Elements/RockButton';
import { ConfigurationValues } from '../../Index.js';
import { Guid } from '../../Util/Guid';
import RegistrationInstance from '../../ViewModels/CodeGenerated/RegistrationInstanceViewModel';
import RegistrationTemplateForm from '../../ViewModels/CodeGenerated/RegistrationTemplateFormViewModel';
import RegistrationTemplate from '../../ViewModels/CodeGenerated/RegistrationTemplateViewModel';
import RegistrationEntryIntro from './RegistrationEntry/Intro';
import RegistrationEntryRegistrant from './RegistrationEntry/Registrant';
import RegistrationEntryRegistration from './RegistrationEntry/Registration';
import RegistrationEntrySummary from './RegistrationEntry/Summary';

export type RegistrantInfo = {
    FamilyGuid: Guid | null
};

export enum RegistrationPersonFieldType {
    FirstName = 0,
    LastName = 1,
    Campus = 2,
    Address = 3,
    Email = 4,
    Birthdate = 5,
    Gender = 6,
    MaritalStatus = 7,
    MobilePhone = 8,
    HomePhone = 9,
    WorkPhone = 10,
    Grade = 11,
    ConnectionStatus = 12,
    MiddleName = 13,
    AnniversaryDate = 14
}

export enum RegistrationFieldSource {
    PersonField = 0,
    PersonAttribute = 1,
    GroupMemberAttribute = 2,
    RegistrantAttribute = 4
}

export default defineComponent({
    name: 'Event.RegistrationEntry',
    components: {
        RockButton,
        RegistrationEntryIntro,
        RegistrationEntryRegistrant,
        RegistrationEntryRegistration,
        RegistrationEntrySummary
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
            registrants: [] as RegistrantInfo[],
            registrationInstance: this.configurationValues['registrationInstance'] as RegistrationInstance | null,
            registrationTemplate: this.configurationValues['registrationTemplate'] as RegistrationTemplate | null,
            registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || []) as RegistrationTemplateForm[]
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
    <RegistrationEntryIntro v-show="currentStep === steps.intro" @next="onIntroNext" :initialRegistrantCount="registrants.length" />
    <RegistrationEntryRegistrant v-show="currentStep === steps.perRegistrantForms" :registrants="registrants" @next="onRegistrantNext" @previous="onRegistrantPrevious" />
    <RegistrationEntryRegistration v-show="currentStep === steps.registrationForm" :registrants="registrants" @next="onRegistrationNext" @previous="onRegistrationPrevious" />
    <RegistrationEntrySummary v-show="currentStep === steps.reviewAndPayment" :registrants="registrants" @previous="onSummaryPrevious" />
</div>`
});
