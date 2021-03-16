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

import { defineComponent, markRaw, PropType } from 'vue';
import { getDefaultAddressControlModel } from '../../../Controls/AddressControl';
import Alert from '../../../Elements/Alert';
import { getDefaultBirthdayPickerModel } from '../../../Elements/BirthdayPicker';
import { Guid } from '../../../Util/Guid';
import { RegistrationEntryBlockFormFieldViewModel, RegistrationPersonFieldType } from './RegistrationEntryBlockViewModel';

export default defineComponent({
    name: 'Event.RegistrationEntry.RegistrantPersonField',
    components: {
        Alert
    },
    props: {
        field: {
            type: Object as PropType<RegistrationEntryBlockFormFieldViewModel>,
            required: true
        },
        fieldValues: {
            type: Object as PropType<Record<Guid, unknown>>,
            required: true
        },
        isKnownFamilyMember: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    data() {
        return {
            fieldControlComponent: null as unknown,
            loading: true
        };
    },
    computed: {
        fieldControlComponentProps() {
            const props: Record<string, unknown> = {
                rules: this.field.IsRequired ? 'required' : ''
            };

            switch (this.field.PersonFieldType) {
                case RegistrationPersonFieldType.FirstName:
                    props.label = 'First Name';
                    props.disabled = this.isKnownFamilyMember;
                    break;
                case RegistrationPersonFieldType.LastName:
                    props.label = 'Last Name';
                    props.disabled = this.isKnownFamilyMember;
                    break;
                case RegistrationPersonFieldType.MiddleName:
                    props.label = 'Middle Name';
                    break;
                case RegistrationPersonFieldType.Campus:
                    props.label = 'Campus';
                    break;
                case RegistrationPersonFieldType.Email:
                    props.label = 'Email';
                    break;
                case RegistrationPersonFieldType.Gender:
                    break;
                case RegistrationPersonFieldType.Birthdate:
                    props.label = 'Birthday';
                    break;
                case RegistrationPersonFieldType.Address:
                    break;
            }

            return props;
        }
    },
    watch: {
        fieldValues: {
            immediate: true,
            deep: true,
            handler() {
                // Set the default value if needed
                if (this.field.Guid in this.fieldValues) {
                    return;
                }

                let defaultValue: unknown = '';

                switch (this.field.PersonFieldType) {
                    case RegistrationPersonFieldType.Birthdate:
                        defaultValue = getDefaultBirthdayPickerModel();
                        break;
                    case RegistrationPersonFieldType.Address:
                        defaultValue = getDefaultAddressControlModel();
                        break;
                }

                this.fieldValues[this.field.Guid] = defaultValue;
            }
        },
        field: {
            immediate: true,
            async handler() {
                this.loading = true;
                let componentPath = '';

                switch (this.field.PersonFieldType) {
                    case RegistrationPersonFieldType.FirstName:
                        componentPath = 'Elements/TextBox';
                        break;
                    case RegistrationPersonFieldType.LastName:
                        componentPath = 'Elements/TextBox';
                        break;
                    case RegistrationPersonFieldType.MiddleName:
                        componentPath = 'Elements/TextBox';
                        break;
                    case RegistrationPersonFieldType.Campus:
                        componentPath = 'Controls/CampusPicker';
                        break;
                    case RegistrationPersonFieldType.Email:
                        componentPath = 'Elements/EmailBox';
                        break;
                    case RegistrationPersonFieldType.Gender:
                        componentPath = 'Elements/GenderDropDownList';
                        break;
                    case RegistrationPersonFieldType.Birthdate:
                        componentPath = 'Elements/BirthdayPicker';
                        break;
                    case RegistrationPersonFieldType.Address:
                        componentPath = 'Controls/AddressControl';
                        break;
                }

                const componentModule = componentPath ? (await import(`../../../${componentPath}`)) : null;
                const component = componentModule ? (componentModule.default || componentModule) : null;

                if (component) {
                    this.fieldControlComponent = markRaw(component);
                }
                else {
                    this.fieldControlComponent = null;
                }

                this.loading = false;
            }
        }
    },
    template: `
<component v-if="fieldControlComponent" :is="fieldControlComponent" v-bind="fieldControlComponentProps" v-model="fieldValues[field.Guid]" />
<Alert v-else-if="!loading" alertType="danger">Could not resolve person field</Alert>`
});