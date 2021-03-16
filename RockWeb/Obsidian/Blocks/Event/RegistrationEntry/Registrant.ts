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
import { RegistrantInfo, RegistrationEntryState } from '../RegistrationEntry';
import StringFilter from '../../../Services/String';
import RockButton from '../../../Elements/RockButton';
import RegistrantPersonField from './RegistrantPersonField';
import RegistrantAttributeField from './RegistrantAttributeField';
import Alert from '../../../Elements/Alert';
import { RegistrationEntryBlockFamilyMemberViewModel, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockViewModel, RegistrationFieldSource, RegistrationPersonFieldType } from './RegistrationEntryBlockViewModel';
import { areEqual } from '../../../Util/Guid';
import RockForm from '../../../Controls/RockForm';
import FeeField from './FeeField';

export default defineComponent({
    name: 'Event.RegistrationEntry.Registrant',
    components: {
        RadioButtonList,
        RockButton,
        RegistrantPersonField,
        RegistrantAttributeField,
        Alert,
        RockForm,
        FeeField,
        DropDownList
    },
    props: {
        currentRegistrant: {
            type: Object as PropType<RegistrantInfo>,
            required: true
        }
    },
    setup() {
        const registrationEntryState = inject('registrationEntryState') as RegistrationEntryState;

        return {
            registrationEntryState
        };
    },
    data() {
        return {
            fieldSources: {
                PersonField: RegistrationFieldSource.PersonField,
                PersonAttribute: RegistrationFieldSource.PersonAttribute,
                GroupMemberAttribute: RegistrationFieldSource.GroupMemberAttribute,
                RegistrantAttribute: RegistrationFieldSource.RegistrantAttribute
            }
        };
    },
    computed: {
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.ViewModel;
        },
        currentFormIndex(): number {
            return this.registrationEntryState.CurrentRegistrantFormIndex;
        },
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
        currentPerson(): Person | null {
            return this.$store.state.currentPerson;
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
                key: 'none',
                text: 'None of the above',
                value: ''
            });

            return options;
        },
        familyMemberOptions(): DropDownListOption[] {
            const selectedFamily = this.currentRegistrant.FamilyGuid;

            if (!selectedFamily) {
                return [];
            }

            return this.viewModel.FamilyMembers
                .filter(fm => areEqual(fm.FamilyGuid, selectedFamily))
                .map(fm => ({
                    key: fm.Guid,
                    text: fm.FullName,
                    value: fm.Guid
                }));
        },
        uppercaseRegistrantTerm(): string {
            return StringFilter.toTitleCase(this.viewModel.RegistrantTerm);
        },
        firstName(): string {
            // This is always on the first form
            const form = this.viewModel.RegistrantForms[0];
            const field = form?.Fields.find(f => f.PersonFieldType === RegistrationPersonFieldType.FirstName);
            const fieldValue = this.currentRegistrant.FieldValues[field?.Guid || ''];
            return typeof fieldValue === 'string' ? fieldValue : '';
        },
        familyMember(): RegistrationEntryBlockFamilyMemberViewModel | null {
            const personGuid = this.currentRegistrant.PersonGuid;

            if (!personGuid) {
                return null;
            }

            return this.viewModel.FamilyMembers.find(fm => areEqual(fm.Guid, personGuid)) || null;
        }
    },
    methods: {
        onPrevious() {
            if (this.currentFormIndex <= 0) {
                this.$emit('previous');
                return;
            }

            this.registrationEntryState.CurrentRegistrantFormIndex--;
        },
        onNext() {
            const lastFormIndex = this.formCountPerRegistrant - 1;

            if (this.currentFormIndex >= lastFormIndex) {
                this.$emit('next');
                return;
            }

            this.registrationEntryState.CurrentRegistrantFormIndex++;
        }
    },
    watch: {
        'currentRegistrant.FamilyGuid'() {
            // Clear the person guid if the family changes
            this.currentRegistrant.PersonGuid = '';
        },
        familyMember() {
            if (!this.familyMember) {
                // If the family member selection is cleared then clear all form fields
                for (const form of this.viewModel.RegistrantForms) {
                    for (const field of form.Fields) {
                        delete this.currentRegistrant.FieldValues[field.Guid];
                    }
                }
            }
            else {
                // If the family member selection is made then set all form fields where use existing value is enabled
                for (const form of this.viewModel.RegistrantForms) {
                    for (const field of form.Fields) {
                        if (field.Guid in this.familyMember.FieldValues) {
                            const familyMemberValue = this.familyMember.FieldValues[field.Guid];

                            if (!familyMemberValue) {
                                delete this.currentRegistrant.FieldValues[field.Guid];
                            }
                            else if (typeof familyMemberValue === 'object') {
                                this.currentRegistrant.FieldValues[field.Guid] = { ...familyMemberValue };
                            }
                            else {
                                this.currentRegistrant.FieldValues[field.Guid] = familyMemberValue;
                            }
                        }
                    }
                }
            }
        }
    },
    template: `
<div>
    <RockForm @submit="onNext">
        <template v-if="currentFormIndex === 0">
            <div v-if="familyOptions.length > 1" class="well js-registration-same-family">
                <RadioButtonList :label="(firstName || uppercaseRegistrantTerm) + ' is in the same immediate family as'" rules='required:{"allowEmptyString": true}' v-model="currentRegistrant.FamilyGuid" :options="familyOptions" validationTitle="Family" />
            </div>
            <DropDownList v-if="familyMemberOptions.length" v-model="currentRegistrant.PersonGuid" :options="familyMemberOptions" label="Family Member to Register" />
        </template>

        <template v-for="field in currentFormFields" :key="field.Guid">
            <RegistrantPersonField v-if="field.FieldSource === fieldSources.PersonField" :field="field" :fieldValues="currentRegistrant.FieldValues" :isKnownFamilyMember="!!currentRegistrant.PersonGuid" />
            <RegistrantAttributeField v-else-if="field.FieldSource === fieldSources.RegistrantAttribute || field.FieldSource === fieldSources.PersonAttribute" :field="field" :fieldValues="currentRegistrant.FieldValues" />
            <Alert alertType="danger" v-else>Could not resolve field source {{field.FieldSource}}</Alert>
        </template>

        <div v-if="isLastForm" class="well registration-additional-options">
            <h4>{{pluralFeeTerm}}</h4>
            <template v-for="fee in viewModel.Fees" :key="fee.Guid">
                <FeeField :fee="fee" v-model="currentRegistrant.FeeQuantities" />
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