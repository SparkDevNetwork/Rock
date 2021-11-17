System.register(["vue", "../../../Controls/addressControl", "../../../Elements/textBox", "../../../Elements/emailBox", "../../../Elements/dropDownList", "../../../Elements/genderDropDownList", "../../../Elements/birthdayPicker", "../../../Controls/componentFromUrl", "../../../Elements/alert", "../../../Elements/datePartsPicker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, addressControl_1, textBox_1, emailBox_1, dropDownList_1, genderDropDownList_1, birthdayPicker_1, componentFromUrl_1, alert_1, datePartsPicker_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (addressControl_1_1) {
                addressControl_1 = addressControl_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (emailBox_1_1) {
                emailBox_1 = emailBox_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (genderDropDownList_1_1) {
                genderDropDownList_1 = genderDropDownList_1_1;
            },
            function (birthdayPicker_1_1) {
                birthdayPicker_1 = birthdayPicker_1_1;
            },
            function (componentFromUrl_1_1) {
                componentFromUrl_1 = componentFromUrl_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (datePartsPicker_1_1) {
                datePartsPicker_1 = datePartsPicker_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.RegistrantPersonField",
                components: {
                    Alert: alert_1.default,
                    ComponentFromUrl: componentFromUrl_1.default
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
                setup(props) {
                    const registrationEntryState = vue_1.inject("registrationEntryState");
                    const component = vue_1.computed(() => {
                        switch (props.field.personFieldType) {
                            case 0:
                                return textBox_1.default;
                            case 1:
                                return textBox_1.default;
                            case 13:
                                return textBox_1.default;
                            case 2:
                                return dropDownList_1.default;
                            case 4:
                                return emailBox_1.default;
                            case 6:
                                return genderDropDownList_1.default;
                            case 5:
                                return birthdayPicker_1.default;
                            case 3:
                                return addressControl_1.default;
                        }
                        return null;
                    });
                    const fieldControlComponentProps = vue_1.computed(() => {
                        const componentProps = {
                            rules: props.field.isRequired ? "required" : ""
                        };
                        switch (props.field.personFieldType) {
                            case 0:
                                componentProps.label = "First Name";
                                componentProps.disabled = props.isKnownFamilyMember;
                                break;
                            case 1:
                                componentProps.label = "Last Name";
                                componentProps.disabled = props.isKnownFamilyMember;
                                break;
                            case 13:
                                componentProps.label = "Middle Name";
                                break;
                            case 2:
                                componentProps.label = "Campus";
                                componentProps.options = [...registrationEntryState.viewModel.campuses];
                                break;
                            case 4:
                                componentProps.label = "Email";
                                break;
                            case 6:
                                break;
                            case 5:
                                componentProps.label = "Birthday";
                                break;
                            case 3:
                                break;
                        }
                        return componentProps;
                    });
                    if (!(props.field.guid in props.fieldValues)) {
                        let defaultValue = "";
                        switch (props.field.personFieldType) {
                            case 5:
                                defaultValue = datePartsPicker_1.getDefaultDatePartsPickerModel();
                                break;
                            case 3:
                                defaultValue = addressControl_1.getDefaultAddressControlModel();
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
            }));
        }
    };
});
//# sourceMappingURL=registrantPersonField.js.map