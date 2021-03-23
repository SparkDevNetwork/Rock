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
System.register(["vue", "../../../Controls/AddressControl", "../../../Controls/ComponentFromUrl", "../../../Elements/Alert", "../../../Elements/BirthdayPicker", "./RegistrationEntryBlockViewModel"], function (exports_1, context_1) {
    "use strict";
    var vue_1, AddressControl_1, ComponentFromUrl_1, Alert_1, BirthdayPicker_1, RegistrationEntryBlockViewModel_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (AddressControl_1_1) {
                AddressControl_1 = AddressControl_1_1;
            },
            function (ComponentFromUrl_1_1) {
                ComponentFromUrl_1 = ComponentFromUrl_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (BirthdayPicker_1_1) {
                BirthdayPicker_1 = BirthdayPicker_1_1;
            },
            function (RegistrationEntryBlockViewModel_1_1) {
                RegistrationEntryBlockViewModel_1 = RegistrationEntryBlockViewModel_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.RegistrantPersonField',
                components: {
                    Alert: Alert_1.default,
                    ComponentFromUrl: ComponentFromUrl_1.default
                },
                props: {
                    field: {
                        type: Object,
                        required: true
                    },
                    fieldValues: {
                        type: Object,
                        required: true
                    },
                    isKnownFamilyMember: {
                        type: Boolean,
                        required: true
                    }
                },
                computed: {
                    componentUrl: function () {
                        var componentPath = '';
                        switch (this.field.PersonFieldType) {
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.FirstName:
                                componentPath = 'Elements/TextBox';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.LastName:
                                componentPath = 'Elements/TextBox';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.MiddleName:
                                componentPath = 'Elements/TextBox';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Campus:
                                componentPath = 'Controls/CampusPicker';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Email:
                                componentPath = 'Elements/EmailBox';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Gender:
                                componentPath = 'Elements/GenderDropDownList';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Birthdate:
                                componentPath = 'Elements/BirthdayPicker';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Address:
                                componentPath = 'Controls/AddressControl';
                                break;
                        }
                        return componentPath ? "../" + componentPath : '';
                    },
                    fieldControlComponentProps: function () {
                        var props = {
                            rules: this.field.IsRequired ? 'required' : ''
                        };
                        switch (this.field.PersonFieldType) {
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.FirstName:
                                props.label = 'First Name';
                                props.disabled = this.isKnownFamilyMember;
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.LastName:
                                props.label = 'Last Name';
                                props.disabled = this.isKnownFamilyMember;
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.MiddleName:
                                props.label = 'Middle Name';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Campus:
                                props.label = 'Campus';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Email:
                                props.label = 'Email';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Gender:
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Birthdate:
                                props.label = 'Birthday';
                                break;
                            case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Address:
                                break;
                        }
                        return props;
                    }
                },
                watch: {
                    fieldValues: {
                        immediate: true,
                        deep: true,
                        handler: function () {
                            // Set the default value if needed
                            if (this.field.Guid in this.fieldValues) {
                                return;
                            }
                            var defaultValue = '';
                            switch (this.field.PersonFieldType) {
                                case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Birthdate:
                                    defaultValue = BirthdayPicker_1.getDefaultBirthdayPickerModel();
                                    break;
                                case RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Address:
                                    defaultValue = AddressControl_1.getDefaultAddressControlModel();
                                    break;
                            }
                            this.fieldValues[this.field.Guid] = defaultValue;
                        }
                    },
                },
                template: "\n<ComponentFromUrl v-if=\"componentUrl\" :url=\"componentUrl\" v-bind=\"fieldControlComponentProps\" v-model=\"fieldValues[field.Guid]\" />"
            }));
        }
    };
});
//# sourceMappingURL=RegistrantPersonField.js.map