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

import { defineComponent, inject, PropType } from 'vue';
import { DropDownListOption } from '../../../Elements/DropDownList';
import RadioButtonList from '../../../Elements/RadioButtonList';
import Person from '../../../ViewModels/CodeGenerated/PersonViewModel';
import { RegistrantInfo } from '../RegistrationEntry';
import NumberFilter from '../../../Filters/Number';
import StringFilter from '../../../Filters/String';
import RockButton from '../../../Elements/RockButton';
import { ConfigurationValues } from '../../../Index.js';
import RegistrationInstance from '../../../ViewModels/CodeGenerated/RegistrationInstanceViewModel';
import RegistrationTemplate from '../../../ViewModels/CodeGenerated/RegistrationTemplateViewModel';
import RegistrationTemplateForm from '../../../ViewModels/CodeGenerated/RegistrationTemplateFormViewModel';
import ProgressBar from '../../../Elements/ProgressBar';

export default defineComponent({
    name: 'Event.RegistrationEntry.Registrant',
    components: {
        RadioButtonList,
        RockButton,
        ProgressBar
    },
    setup() {
        return {
            configurationValues: inject('configurationValues') as ConfigurationValues
        };
    },
    props: {
        registrants: {
            type: Array as PropType<RegistrantInfo[]>,
            required: true
        }
    },
    data() {
        return {
            noFamilyValue: 'none',
            selectedFamily: '',
            currentRegistrantIndex: 0,
            currentFormIndex: 0,
            registrationInstance: this.configurationValues['registrationInstance'] as RegistrationInstance | null,
            registrationTemplate: this.configurationValues['registrationTemplate'] as RegistrationTemplate | null,
            registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || []) as RegistrationTemplateForm[]
        };
    },
    computed: {
        formCountPerRegistrant(): number {
            return this.registrationTemplateForms.length;
        },
        numberOfPages(): number {
            // All of the steps are 1 page except the "per-registrant"
            return 3 + (this.registrants.length * this.formCountPerRegistrant);
        },
        completionPercentDecimal(): number {
            return (1 + this.currentFormIndex + this.currentRegistrantIndex * this.formCountPerRegistrant) / this.numberOfPages;
        },
        completionPercentInt(): number {
            return this.completionPercentDecimal * 100;
        },
        currentPerson(): Person | null {
            return this.$store.state.currentPerson;
        },
        registrantTerm(): string {
            return this.registrationTemplate?.RegistrantTerm?.toLowerCase() || 'registrant';
        },
        possibleFamilyMembers(): DropDownListOption[] {
            const options = [] as DropDownListOption[];

            if (this.currentPerson?.PrimaryFamilyGuid && this.currentPerson.FullName) {
                options.push({
                    key: this.currentPerson.PrimaryFamilyGuid,
                    text: this.currentPerson.FullName,
                    value: this.currentPerson.PrimaryFamilyGuid
                });
            }

            options.push({
                key: this.noFamilyValue,
                text: 'None of the above',
                value: this.noFamilyValue
            });

            return options;
        },
        currentRegistrant(): RegistrantInfo {
            return this.registrants[this.currentRegistrantIndex];
        },
        currentRegistrantTitle(): string {
            const ordinal = NumberFilter.toOrdinal(this.currentRegistrantIndex + 1);
            let title = StringFilter.toTitleCase(
                this.registrants.length <= 1 ?
                    this.registrantTerm :
                    ordinal + ' ' + this.registrantTerm);

            if (this.currentFormIndex > 0) {
                title += ' (cont)';
            }

            return title;
        }
    },
    methods: {
        onPrevious() {
            const lastFormIndex = this.formCountPerRegistrant - 1;

            if (this.currentFormIndex <= 0 && this.currentRegistrantIndex <= 0) {
                this.$emit('previous');
                return;
            }

            if (this.currentFormIndex <= 0) {
                this.currentRegistrantIndex--;
                this.currentFormIndex = lastFormIndex;
                return;
            }

            this.currentFormIndex--;
        },
        onNext() {
            const lastFormIndex = this.formCountPerRegistrant - 1;
            const lastRegistrantIndex = this.registrants.length - 1;

            if (this.currentFormIndex >= lastFormIndex && this.currentRegistrantIndex >= lastRegistrantIndex) {
                this.$emit('next');
                return;
            }

            if (this.currentFormIndex >= lastFormIndex) {
                this.currentRegistrantIndex++;
                this.currentFormIndex = 0;
                return;
            }

            this.currentFormIndex++;
        }
    },
    watch: {
        selectedFamily() {
            if (this.selectedFamily === this.noFamilyValue) {
                this.currentRegistrant.FamilyGuid = null;
            }
            else {
                this.currentRegistrant.FamilyGuid = this.selectedFamily;
            }
        }
    },
    template: `
<div class="registrationentry-registrant">
    <h1>{{currentRegistrantTitle}}</h1>
    <ProgressBar :percent="completionPercentInt" />
    <div class="well js-registration-same-family">
        <RadioButtonList label="Individual is in the same immediate family as" rules="required" v-model="selectedFamily" :options="possibleFamilyMembers" />
    </div>
    <div class="actions">
        <RockButton btnType="default" @click="onPrevious">
            Previous
        </RockButton>
        <RockButton btnType="primary" class="pull-right" @click="onNext">
            Next
        </RockButton>
    </div>
</div>
`
});