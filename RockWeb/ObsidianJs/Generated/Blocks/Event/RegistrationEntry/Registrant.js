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
System.register(["vue", "../../../Elements/DropDownList", "../../../Elements/RadioButtonList", "../RegistrationEntry", "../../../Services/String", "../../../Elements/RockButton", "./RegistrantPersonField", "./RegistrantAttributeField", "../../../Elements/Alert", "./RegistrationEntryBlockViewModel", "../../../Util/Guid", "../../../Controls/RockForm", "./FeeField"], function (exports_1, context_1) {
    "use strict";
    var __assign = (this && this.__assign) || function () {
        __assign = Object.assign || function(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                    t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };
    var vue_1, DropDownList_1, RadioButtonList_1, RegistrationEntry_1, String_1, RockButton_1, RegistrantPersonField_1, RegistrantAttributeField_1, Alert_1, RegistrationEntryBlockViewModel_1, Guid_1, RockForm_1, FeeField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (RadioButtonList_1_1) {
                RadioButtonList_1 = RadioButtonList_1_1;
            },
            function (RegistrationEntry_1_1) {
                RegistrationEntry_1 = RegistrationEntry_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (RegistrantPersonField_1_1) {
                RegistrantPersonField_1 = RegistrantPersonField_1_1;
            },
            function (RegistrantAttributeField_1_1) {
                RegistrantAttributeField_1 = RegistrantAttributeField_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (RegistrationEntryBlockViewModel_1_1) {
                RegistrationEntryBlockViewModel_1 = RegistrationEntryBlockViewModel_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (FeeField_1_1) {
                FeeField_1 = FeeField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Registrant',
                components: {
                    RadioButtonList: RadioButtonList_1.default,
                    RockButton: RockButton_1.default,
                    RegistrantPersonField: RegistrantPersonField_1.default,
                    RegistrantAttributeField: RegistrantAttributeField_1.default,
                    Alert: Alert_1.default,
                    RockForm: RockForm_1.default,
                    FeeField: FeeField_1.default,
                    DropDownList: DropDownList_1.default
                },
                props: {
                    currentRegistrant: {
                        type: Object,
                        required: true
                    }
                },
                setup: function () {
                    var registrationEntryState = vue_1.inject('registrationEntryState');
                    return {
                        registrationEntryState: registrationEntryState
                    };
                },
                data: function () {
                    return {
                        fieldSources: {
                            PersonField: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.PersonField,
                            PersonAttribute: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.PersonAttribute,
                            GroupMemberAttribute: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.GroupMemberAttribute,
                            RegistrantAttribute: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.RegistrantAttribute
                        }
                    };
                },
                computed: {
                    showPrevious: function () {
                        return this.registrationEntryState.FirstStep !== this.registrationEntryState.Steps.perRegistrantForms;
                    },
                    viewModel: function () {
                        return this.registrationEntryState.ViewModel;
                    },
                    currentFormIndex: function () {
                        return this.registrationEntryState.CurrentRegistrantFormIndex;
                    },
                    currentForm: function () {
                        return this.viewModel.RegistrantForms[this.currentFormIndex] || null;
                    },
                    isLastForm: function () {
                        return (this.currentFormIndex + 1) === this.viewModel.RegistrantForms.length;
                    },
                    currentFormFields: function () {
                        var _a;
                        return ((_a = this.currentForm) === null || _a === void 0 ? void 0 : _a.Fields) || [];
                    },
                    formCountPerRegistrant: function () {
                        return this.viewModel.RegistrantForms.length;
                    },
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    pluralFeeTerm: function () {
                        return String_1.default.toTitleCase(this.viewModel.PluralFeeTerm || 'fees');
                    },
                    familyOptions: function () {
                        var _a;
                        var options = [];
                        if (!this.viewModel.DoAskForFamily) {
                            return options;
                        }
                        if (((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.PrimaryFamilyGuid) && this.currentPerson.FullName) {
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
                    familyMemberOptions: function () {
                        var selectedFamily = this.currentRegistrant.FamilyGuid;
                        if (!selectedFamily) {
                            return [];
                        }
                        return this.viewModel.FamilyMembers
                            .filter(function (fm) { return Guid_1.areEqual(fm.FamilyGuid, selectedFamily); })
                            .map(function (fm) { return ({
                            key: fm.Guid,
                            text: fm.FullName,
                            value: fm.Guid
                        }); });
                    },
                    uppercaseRegistrantTerm: function () {
                        return String_1.default.toTitleCase(this.viewModel.RegistrantTerm);
                    },
                    firstName: function () {
                        return RegistrationEntry_1.getRegistrantBasicInfo(this.currentRegistrant, this.viewModel.RegistrantForms).FirstName;
                    },
                    familyMember: function () {
                        var personGuid = this.currentRegistrant.PersonGuid;
                        if (!personGuid) {
                            return null;
                        }
                        return this.viewModel.FamilyMembers.find(function (fm) { return Guid_1.areEqual(fm.Guid, personGuid); }) || null;
                    }
                },
                methods: {
                    onPrevious: function () {
                        if (this.currentFormIndex <= 0) {
                            this.$emit('previous');
                            return;
                        }
                        this.registrationEntryState.CurrentRegistrantFormIndex--;
                    },
                    onNext: function () {
                        var lastFormIndex = this.formCountPerRegistrant - 1;
                        if (this.currentFormIndex >= lastFormIndex) {
                            this.$emit('next');
                            return;
                        }
                        this.registrationEntryState.CurrentRegistrantFormIndex++;
                    }
                },
                watch: {
                    'currentRegistrant.FamilyGuid': function () {
                        // Clear the person guid if the family changes
                        this.currentRegistrant.PersonGuid = '';
                    },
                    familyMember: function () {
                        if (!this.familyMember) {
                            // If the family member selection is cleared then clear all form fields
                            for (var _i = 0, _a = this.viewModel.RegistrantForms; _i < _a.length; _i++) {
                                var form = _a[_i];
                                for (var _b = 0, _c = form.Fields; _b < _c.length; _b++) {
                                    var field = _c[_b];
                                    delete this.currentRegistrant.FieldValues[field.Guid];
                                }
                            }
                        }
                        else {
                            // If the family member selection is made then set all form fields where use existing value is enabled
                            for (var _d = 0, _e = this.viewModel.RegistrantForms; _d < _e.length; _d++) {
                                var form = _e[_d];
                                for (var _f = 0, _g = form.Fields; _f < _g.length; _f++) {
                                    var field = _g[_f];
                                    if (field.Guid in this.familyMember.FieldValues) {
                                        var familyMemberValue = this.familyMember.FieldValues[field.Guid];
                                        if (!familyMemberValue) {
                                            delete this.currentRegistrant.FieldValues[field.Guid];
                                        }
                                        else if (typeof familyMemberValue === 'object') {
                                            this.currentRegistrant.FieldValues[field.Guid] = __assign({}, familyMemberValue);
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
                template: "\n<div>\n    <RockForm @submit=\"onNext\">\n        <template v-if=\"currentFormIndex === 0\">\n            <div v-if=\"familyOptions.length > 1\" class=\"well js-registration-same-family\">\n                <RadioButtonList :label=\"(firstName || uppercaseRegistrantTerm) + ' is in the same immediate family as'\" rules='required:{\"allowEmptyString\": true}' v-model=\"currentRegistrant.FamilyGuid\" :options=\"familyOptions\" validationTitle=\"Family\" />\n            </div>\n            <DropDownList v-if=\"familyMemberOptions.length\" v-model=\"currentRegistrant.PersonGuid\" :options=\"familyMemberOptions\" label=\"Family Member to Register\" />\n        </template>\n\n        <template v-for=\"field in currentFormFields\" :key=\"field.Guid\">\n            <RegistrantPersonField v-if=\"field.FieldSource === fieldSources.PersonField\" :field=\"field\" :fieldValues=\"currentRegistrant.FieldValues\" :isKnownFamilyMember=\"!!currentRegistrant.PersonGuid\" />\n            <RegistrantAttributeField v-else-if=\"field.FieldSource === fieldSources.RegistrantAttribute || field.FieldSource === fieldSources.PersonAttribute\" :field=\"field\" :fieldValues=\"currentRegistrant.FieldValues\" />\n            <Alert alertType=\"danger\" v-else>Could not resolve field source {{field.FieldSource}}</Alert>\n        </template>\n\n        <div v-if=\"isLastForm && viewModel.Fees.length\" class=\"well registration-additional-options\">\n            <h4>{{pluralFeeTerm}}</h4>\n            <template v-for=\"fee in viewModel.Fees\" :key=\"fee.Guid\">\n                <FeeField :fee=\"fee\" v-model=\"currentRegistrant.FeeQuantities\" />\n            </template>\n        </div>\n\n        <div class=\"actions row\">\n            <div class=\"col-xs-6\">\n                <RockButton v-if=\"showPrevious\" btnType=\"default\" @click=\"onPrevious\">\n                    Previous\n                </RockButton>\n            </div>\n            <div class=\"col-xs-6 text-right\">\n                <RockButton btnType=\"primary\" type=\"submit\">\n                    Next\n                </RockButton>\n            </div>\n        </div>\n    </RockForm>\n</div>\n"
            }));
        }
    };
});
//# sourceMappingURL=Registrant.js.map