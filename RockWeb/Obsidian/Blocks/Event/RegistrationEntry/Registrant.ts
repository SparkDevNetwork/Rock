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
import DropDownList, { DropDownListOption } from '../../../Elements/DropDownList';
import RadioButtonList from '../../../Elements/RadioButtonList';
import Person from '../../../ViewModels/CodeGenerated/PersonViewModel';
import { RegistrantInfo } from '../RegistrationEntry';
import NumberFilter from '../../../Services/Number';
import StringFilter from '../../../Services/String';
import RockButton from '../../../Elements/RockButton';
import ProgressBar from '../../../Elements/ProgressBar';
import RegistrantPersonField from './RegistrantPersonField';
import RegistrantAttributeField from './RegistrantAttributeField';
import Alert from '../../../Elements/Alert';
import { RegistrationEntryBlockFamilyMemberViewModel, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockViewModel, RegistrationFieldSource, RegistrationPersonFieldType } from './RegistrationEntryBlockViewModel';
import { areEqual, Guid } from '../../../Util/Guid';
import RockForm from '../../../Controls/RockForm';
import FeeField from './FeeField';

export default defineComponent({
    name: 'Event.RegistrationEntry.Registrant',
    components: {
        RadioButtonList,
        RockButton,
        ProgressBar,
        RegistrantPersonField,
        RegistrantAttributeField,
        Alert,
        RockForm,
        FeeField,
        DropDownList
    },
    setup() {
        return {
            viewModel: inject('configurationValues') as RegistrationEntryBlockViewModel
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
            selectedFamily: '' as Guid,
            familyMemberGuid: '' as Guid,
            currentRegistrantIndex: 0,
            currentFormIndex: 0,
            fieldValues: {} as Record<Guid, unknown>,
            feeQuantities: {} as Record<Guid, number>,
            fieldSources: {
                PersonField: RegistrationFieldSource.PersonField,
                PersonAttribute: RegistrationFieldSource.PersonAttribute,
                GroupMemberAttribute: RegistrationFieldSource.GroupMemberAttribute,
                RegistrantAttribute: RegistrationFieldSource.RegistrantAttribute
            }
        };
    },
    computed: {
        currentForm(): RegistrationEntryBlockFormViewModel | null {
            return this.viewModel.RegistrantForms[this.currentFormIndex] || null;
        },
        isLastForm(): boolean {
            return (this.currentFormIndex + 1) === this.viewModel.RegistrantForms.length;
        },
        currentFormFields(): RegistrationEntryBlockFormFieldViewModel[] {
            return this.currentForm?.Fields || [];
        },
        formCountPerRegistrant(): number {
            return this.viewModel.RegistrantForms.length;
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
        uppercaseRegistrantTerm(): string {
            return StringFilter.toTitleCase(this.viewModel.RegistrantTerm);
        },
        pluralFeeTerm(): string {
            return StringFilter.toTitleCase(this.viewModel.PluralFeeTerm || 'fees');
        },
        familyOptions(): DropDownListOption[] {
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
        familyMemberOptions(): DropDownListOption[] {
            if (!this.selectedFamily || this.selectedFamily === this.noFamilyValue) {
                return [];
            }

            return this.viewModel.FamilyMembers
                .filter(fm => areEqual(fm.FamilyGuid, this.selectedFamily))
                .map(fm => ({
                    key: fm.Guid,
                    text: fm.FullName,
                    value: fm.Guid
                }));
        },
        currentRegistrant(): RegistrantInfo {
            return this.registrants[this.currentRegistrantIndex];
        },
        currentRegistrantTitle(): string {
            const ordinal = NumberFilter.toOrdinal(this.currentRegistrantIndex + 1);
            let title = StringFilter.toTitleCase(
                this.registrants.length <= 1 ?
                    this.uppercaseRegistrantTerm :
                    ordinal + ' ' + this.uppercaseRegistrantTerm);

            if (this.currentFormIndex > 0) {
                title += ' (cont)';
            }

            return title;
        },
        firstName(): string {
            // This is always on the first form
            const form = this.viewModel.RegistrantForms[0];
            const field = form?.Fields.find(f => f.PersonFieldType === RegistrationPersonFieldType.FirstName);
            const fieldValue = this.fieldValues[field?.Guid || ''];
            return typeof fieldValue === 'string' ? fieldValue : '';
        },
        familyMember(): RegistrationEntryBlockFamilyMemberViewModel | null {
            return this.viewModel.FamilyMembers.find(fm => areEqual(fm.Guid, this.familyMemberGuid)) || null;
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

            this.familyMemberGuid = '';
        },
        viewModel: {
            deep: true,
            immediate: true,
            handler() {
                for (const form of this.viewModel.RegistrantForms) {
                    for (const field of form.Fields) {
                        if (!(field.Guid in this.fieldValues)) {
                            this.fieldValues[field.Guid] = '';
                        }
                    }
                }
            }
        },
        familyMember() {
            if (!this.familyMember) {
                for (const form of this.viewModel.RegistrantForms) {
                    for (const field of form.Fields) {
                        this.fieldValues[field.Guid] = '';
                    }
                }
            }
            else {
                for (const form of this.viewModel.RegistrantForms) {
                    for (const field of form.Fields) {
                        if (field.Guid in this.familyMember.FieldValues) {
                            this.fieldValues[field.Guid] = this.familyMember.FieldValues[field.Guid];
                        }
                    }
                }
            }
        }
    },
    template: `
<div class="registrationentry-registrant">
    <h1>{{currentRegistrantTitle}}</h1>
    <ProgressBar :percent="completionPercentInt" />

    <RockForm @submit="onNext">
        <div v-if="familyOptions.length > 1 && currentFormIndex === 0" class="well js-registration-same-family">
            <RadioButtonList :label="(firstName || uppercaseRegistrantTerm) + ' is in the same immediate family as'" rules="required" v-model="selectedFamily" :options="familyOptions" validationTitle="Family" />
        </div>

        <DropDownList v-if="familyMemberOptions.length" v-model="familyMemberGuid" :options="familyMemberOptions" label="Family Member to Register" />

        <template v-for="field in currentFormFields" :key="field.Guid">
            <RegistrantPersonField v-if="field.FieldSource === fieldSources.PersonField" :field="field" :fieldValues="fieldValues" :isKnownFamilyMember="!!familyMemberGuid" />
            <RegistrantAttributeField v-else-if="field.FieldSource === fieldSources.RegistrantAttribute || field.FieldSource === fieldSources.PersonAttribute" :field="field" :fieldValues="fieldValues" />
            <Alert alertType="danger" v-else>Could not resolve field source {{field.FieldSource}}</Alert>
        </template>

        <div v-if="isLastForm" class="well registration-additional-options">
            <h4>{{pluralFeeTerm}}</h4>
            <template v-for="fee in viewModel.Fees" :key="fee.Guid">
                <FeeField :fee="fee" v-model="feeQuantities" />
            </template>
        </div>

        <div class="actions">
            <RockButton btnType="default" @click="onPrevious">
                Previous
            </RockButton>
            <RockButton btnType="primary" class="pull-right" type="submit">
                Next
            </RockButton>
        </div>
    </RockForm>
</div>
`
});