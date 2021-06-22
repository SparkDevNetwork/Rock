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

import { defineComponent, PropType } from 'vue';
import { getDefaultAddressControlModel } from '../../../Controls/AddressControl';
import ComponentFromUrl from '../../../Controls/ComponentFromUrl';
import Alert from '../../../Elements/Alert';
import { getDefaultDatePartsPickerModel } from '../../../Elements/DatePartsPicker';
import { Guid } from '../../../Util/Guid';
import { RegistrationEntryBlockFormFieldViewModel, RegistrationPersonFieldType } from './RegistrationEntryBlockViewModel';

export default defineComponent( {
    name: 'Event.RegistrationEntry.RegistrantPersonField',
    components: {
        Alert,
        ComponentFromUrl
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
    computed: {
        componentUrl(): string
        {
            let componentPath = '';

            switch ( this.field.PersonFieldType )
            {
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

            return componentPath ? `../${componentPath}` : '';
        },
        fieldControlComponentProps()
        {
            const props: Record<string, unknown> = {
                rules: this.field.IsRequired ? 'required' : ''
            };

            switch ( this.field.PersonFieldType )
            {
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
            handler()
            {
                // Set the default value if needed
                if ( this.field.Guid in this.fieldValues )
                {
                    return;
                }

                let defaultValue: unknown = '';

                switch ( this.field.PersonFieldType )
                {
                    case RegistrationPersonFieldType.Birthdate:
                        defaultValue = getDefaultDatePartsPickerModel();
                        break;
                    case RegistrationPersonFieldType.Address:
                        defaultValue = getDefaultAddressControlModel();
                        break;
                }

                this.fieldValues[ this.field.Guid ] = defaultValue;
            }
        },
    },
    template: `
<ComponentFromUrl v-if="componentUrl" :url="componentUrl" v-bind="fieldControlComponentProps" v-model="fieldValues[field.Guid]" />`
} );