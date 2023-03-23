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

import { Guid } from "@Obsidian/Types";
import { computed, defineComponent, inject, PropType } from "vue";
import AddressControl from "@Obsidian/Controls/addressControl.obs";
import { getDefaultAddressControlModel } from "@Obsidian/Utility/address";
import TextBox from "@Obsidian/Controls/textBox";
import EmailBox from "@Obsidian/Controls/emailBox";
import DropDownList from "@Obsidian/Controls/dropDownList";
import GenderDropDownList from "@Obsidian/Controls/genderDropDownList";
import BirthdayPicker from "@Obsidian/Controls/birthdayPicker";
import PhoneNumberBox from "@Obsidian/Controls/phoneNumberBox.obs";
import ComponentFromUrl from "@Obsidian/Controls/componentFromUrl";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { getDefaultDatePartsPickerModel } from "@Obsidian/Controls/datePartsPicker";
import { RegistrationEntryBlockFormFieldViewModel, RegistrationPersonFieldType, RegistrationEntryState } from "./types.partial";

export default defineComponent({
    name: "Event.RegistrationEntry.RegistrantPersonField",
    components: {
        NotificationBox,
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

                case RegistrationPersonFieldType.Race:
                    return DropDownList;

                case RegistrationPersonFieldType.Ethnicity:
                    return DropDownList;
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
                    componentProps.items = [...registrationEntryState.viewModel.campuses];
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
                    componentProps.label = "Address";
                    break;

                case RegistrationPersonFieldType.MaritalStatus:
                    componentProps.label = "Marital Status";
                    componentProps.items = [...registrationEntryState.viewModel.maritalStatuses];
                    break;

                case RegistrationPersonFieldType.ConnectionStatus:
                    componentProps.label = "Connection Status";
                    componentProps.items = [...registrationEntryState.viewModel.connectionStatuses];
                    break;

                case RegistrationPersonFieldType.Grade:
                    componentProps.label = "Grade";
                    componentProps.items = [...registrationEntryState.viewModel.grades];
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

                case RegistrationPersonFieldType.Race:
                    componentProps.label = "Race";
                    componentProps.items = [...registrationEntryState.viewModel.races];
                    break;

                case RegistrationPersonFieldType.Ethnicity:
                    componentProps.label = "Ethnicity";
                    componentProps.items = [...registrationEntryState.viewModel.ethnicities];
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
<NotificationBox v-else alertType="danger">Could not load the control for person field {{ fieldType }}.</NotificationBox>
`
});
