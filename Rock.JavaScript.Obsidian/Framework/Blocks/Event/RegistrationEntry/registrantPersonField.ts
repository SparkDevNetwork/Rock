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

import { computed, defineComponent, inject, PropType } from "vue";
import AddressControl, { getDefaultAddressControlModel } from "../../../Controls/addressControl";
import TextBox from "../../../Elements/textBox";
import EmailBox from "../../../Elements/emailBox";
import DropDownList from "../../../Elements/dropDownList";
import GenderDropDownList from "../../../Elements/genderDropDownList";
import BirthdayPicker from "../../../Elements/birthdayPicker";
import PhoneNumberBox from "../../../Elements/phoneNumberBox";
import ComponentFromUrl from "../../../Controls/componentFromUrl";
import Alert from "../../../Elements/alert";
import { getDefaultDatePartsPickerModel } from "../../../Elements/datePartsPicker";
import { Guid } from "../../../Util/guid";
import { RegistrationEntryBlockFormFieldViewModel, RegistrationPersonFieldType } from "./registrationEntryBlockViewModel";
import { RegistrationEntryState } from "../registrationEntry";

export default defineComponent({
    name: "Event.RegistrationEntry.RegistrantPersonField",
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

    setup(props) {
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;

        const component = computed(() => {
            switch (props.field.personFieldType) {
                case RegistrationPersonFieldType.FirstName:
                    return TextBox;

                case RegistrationPersonFieldType.LastName:
                    return TextBox;

                case RegistrationPersonFieldType.MiddleName:
                    return TextBox;

                case RegistrationPersonFieldType.Campus:
                    return DropDownList;

                case RegistrationPersonFieldType.Email:
                    return EmailBox;

                case RegistrationPersonFieldType.Gender:
                    return GenderDropDownList;

                case RegistrationPersonFieldType.Birthdate:
                    return BirthdayPicker;

                case RegistrationPersonFieldType.AnniversaryDate:
                    return BirthdayPicker;

                case RegistrationPersonFieldType.Address:
                    return AddressControl;

                case RegistrationPersonFieldType.MaritalStatus:
                    return DropDownList;

                case RegistrationPersonFieldType.ConnectionStatus:
                    return DropDownList;

                case RegistrationPersonFieldType.Grade:
                    return DropDownList;

                case RegistrationPersonFieldType.HomePhone:
                    return PhoneNumberBox;

                case RegistrationPersonFieldType.WorkPhone:
                    return PhoneNumberBox;

                case RegistrationPersonFieldType.MobilePhone:
                    return PhoneNumberBox;
            }
        });

        const fieldControlComponentProps = computed(() => {
            const componentProps: Record<string, unknown> = {
                rules: props.field.isRequired ? "required" : ""
            };

            switch (props.field.personFieldType) {
                case RegistrationPersonFieldType.FirstName:
                    componentProps.label = "First Name";
                    componentProps.disabled = props.isKnownFamilyMember;
                    break;

                case RegistrationPersonFieldType.LastName:
                    componentProps.label = "Last Name";
                    componentProps.disabled = props.isKnownFamilyMember;
                    break;

                case RegistrationPersonFieldType.MiddleName:
                    componentProps.label = "Middle Name";
                    break;

                case RegistrationPersonFieldType.Campus:
                    componentProps.label = "Campus";
                    componentProps.options = [...registrationEntryState.viewModel.campuses];

                    break;

                case RegistrationPersonFieldType.Email:
                    componentProps.label = "Email";
                    break;

                case RegistrationPersonFieldType.Gender:
                    break;

                case RegistrationPersonFieldType.Birthdate:
                    componentProps.label = "Birthday";
                    break;

                case RegistrationPersonFieldType.AnniversaryDate:
                    componentProps.label = "Anniversary Date";
                    break;

                case RegistrationPersonFieldType.Address:
                    break;

                case RegistrationPersonFieldType.MaritalStatus:
                    componentProps.label = "Marital Status";
                    componentProps.options = [...registrationEntryState.viewModel.maritalStatuses];
                    break;

                case RegistrationPersonFieldType.ConnectionStatus:
                    componentProps.label = "Connection Status";
                    componentProps.options = [...registrationEntryState.viewModel.connectionStatuses];
                    break;

                case RegistrationPersonFieldType.Grade:
                    componentProps.label = "Grade";
                    componentProps.options = [...registrationEntryState.viewModel.grades];
                    break;

                case RegistrationPersonFieldType.HomePhone:
                    componentProps.label = "Home Phone";
                    break;

                case RegistrationPersonFieldType.WorkPhone:
                    componentProps.label = "Work Phone";
                    break;

                case RegistrationPersonFieldType.MobilePhone:
                    componentProps.label = "Mobile Phone";
                    break;
            }

            return componentProps;
        });

        // Set the default value if needed
        if (!(props.field.guid in props.fieldValues)) {
            let defaultValue: unknown = "";

            switch (props.field.personFieldType) {
                case RegistrationPersonFieldType.Birthdate:
                    defaultValue = getDefaultDatePartsPickerModel();
                    break;

                case RegistrationPersonFieldType.AnniversaryDate:
                    defaultValue = getDefaultDatePartsPickerModel();
                    break;

                case RegistrationPersonFieldType.Address:
                    defaultValue = getDefaultAddressControlModel();
                    break;
            }

            props.fieldValues[props.field.guid] = defaultValue;
        }

        return {
            component,
            fieldControlComponentProps,
            fieldValues: props.fieldValues,
            fieldType: props.field.personFieldType
        };
    },

    template: `
<component v-if="component" :is="component" v-bind="fieldControlComponentProps" v-model="fieldValues[field.guid]" />
<Alert v-else alertType="danger">Could not load the control for person field {{ fieldType }}.</Alert>
`
});
