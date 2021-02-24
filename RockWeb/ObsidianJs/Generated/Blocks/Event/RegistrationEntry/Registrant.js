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
System.register(["vue", "../../../Elements/RadioButtonList", "../RegistrationEntry", "../../../Filters/Number", "../../../Filters/String", "../../../Elements/RockButton", "../../../Elements/ProgressBar", "./RegistrantPersonField", "./RegistrantAttributeField", "../../../Elements/Alert"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RadioButtonList_1, RegistrationEntry_1, Number_1, String_1, RockButton_1, ProgressBar_1, RegistrantPersonField_1, RegistrantAttributeField_1, Alert_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RadioButtonList_1_1) {
                RadioButtonList_1 = RadioButtonList_1_1;
            },
            function (RegistrationEntry_1_1) {
                RegistrationEntry_1 = RegistrationEntry_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (ProgressBar_1_1) {
                ProgressBar_1 = ProgressBar_1_1;
            },
            function (RegistrantPersonField_1_1) {
                RegistrantPersonField_1 = RegistrantPersonField_1_1;
            },
            function (RegistrantAttributeField_1_1) {
                RegistrantAttributeField_1 = RegistrantAttributeField_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Registrant',
                components: {
                    RadioButtonList: RadioButtonList_1.default,
                    RockButton: RockButton_1.default,
                    ProgressBar: ProgressBar_1.default,
                    RegistrantPersonField: RegistrantPersonField_1.default,
                    RegistrantAttributeField: RegistrantAttributeField_1.default,
                    Alert: Alert_1.default
                },
                setup: function () {
                    return {
                        configurationValues: vue_1.inject('configurationValues')
                    };
                },
                props: {
                    registrants: {
                        type: Array,
                        required: true
                    }
                },
                data: function () {
                    return {
                        noFamilyValue: 'none',
                        selectedFamily: '',
                        currentRegistrantIndex: 0,
                        currentFormIndex: 0,
                        registrationInstance: this.configurationValues['registrationInstance'],
                        registrationTemplate: this.configurationValues['registrationTemplate'],
                        registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || []),
                        registrationTemplateFormFields: (this.configurationValues['registrationTemplateFormFields'] || []),
                        fieldSources: {
                            PersonField: RegistrationEntry_1.RegistrationFieldSource.PersonField,
                            PersonAttribute: RegistrationEntry_1.RegistrationFieldSource.PersonAttribute,
                            GroupMemberAttribute: RegistrationEntry_1.RegistrationFieldSource.GroupMemberAttribute,
                            RegistrantAttribute: RegistrationEntry_1.RegistrationFieldSource.RegistrantAttribute
                        }
                    };
                },
                computed: {
                    currentFormId: function () {
                        var _a;
                        return ((_a = this.registrationTemplateForms[this.currentFormIndex]) === null || _a === void 0 ? void 0 : _a.Id) || 0;
                    },
                    currentFormFields: function () {
                        var _this = this;
                        return this.registrationTemplateFormFields.filter(function (f) { return f.RegistrationTemplateFormId === _this.currentFormId; });
                    },
                    formCountPerRegistrant: function () {
                        return this.registrationTemplateForms.length;
                    },
                    numberOfPages: function () {
                        // All of the steps are 1 page except the "per-registrant"
                        return 3 + (this.registrants.length * this.formCountPerRegistrant);
                    },
                    completionPercentDecimal: function () {
                        return (1 + this.currentFormIndex + this.currentRegistrantIndex * this.formCountPerRegistrant) / this.numberOfPages;
                    },
                    completionPercentInt: function () {
                        return this.completionPercentDecimal * 100;
                    },
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    registrantTerm: function () {
                        var _a, _b;
                        return ((_b = (_a = this.registrationTemplate) === null || _a === void 0 ? void 0 : _a.RegistrantTerm) === null || _b === void 0 ? void 0 : _b.toLowerCase()) || 'registrant';
                    },
                    possibleFamilyMembers: function () {
                        var _a;
                        var options = [];
                        if (((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.PrimaryFamilyGuid) && this.currentPerson.FullName) {
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
                    currentRegistrant: function () {
                        return this.registrants[this.currentRegistrantIndex];
                    },
                    currentRegistrantTitle: function () {
                        var ordinal = Number_1.default.toOrdinal(this.currentRegistrantIndex + 1);
                        var title = String_1.default.toTitleCase(this.registrants.length <= 1 ?
                            this.registrantTerm :
                            ordinal + ' ' + this.registrantTerm);
                        if (this.currentFormIndex > 0) {
                            title += ' (cont)';
                        }
                        return title;
                    }
                },
                methods: {
                    onPrevious: function () {
                        var lastFormIndex = this.formCountPerRegistrant - 1;
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
                    onNext: function () {
                        var lastFormIndex = this.formCountPerRegistrant - 1;
                        var lastRegistrantIndex = this.registrants.length - 1;
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
                    selectedFamily: function () {
                        if (this.selectedFamily === this.noFamilyValue) {
                            this.currentRegistrant.FamilyGuid = null;
                        }
                        else {
                            this.currentRegistrant.FamilyGuid = this.selectedFamily;
                        }
                    }
                },
                template: "\n<div class=\"registrationentry-registrant\">\n    <h1>{{currentRegistrantTitle}}</h1>\n    <ProgressBar :percent=\"completionPercentInt\" />\n    <div v-if=\"possibleFamilyMembers && possibleFamilyMembers.length > 1\" class=\"well js-registration-same-family\">\n        <RadioButtonList label=\"Individual is in the same immediate family as\" rules=\"required\" v-model=\"selectedFamily\" :options=\"possibleFamilyMembers\" />\n    </div>\n\n    <template v-for=\"field in currentFormFields\" :key=\"field.Guid\">\n        <RegistrantPersonField v-if=\"field.FieldSource === fieldSources.PersonField\" :field=\"field\" />\n        <RegistrantAttributeField v-else-if=\"field.FieldSource === fieldSources.RegistrantAttribute || field.FieldSource === fieldSources.PersonAttribute\" :field=\"field\" />\n        <Alert alertType=\"danger\" v-else>Could not resolve field source {{field.FieldSource}}</Alert>\n    </template>\n\n    <div class=\"actions\">\n        <RockButton btnType=\"default\" @click=\"onPrevious\">\n            Previous\n        </RockButton>\n        <RockButton btnType=\"primary\" class=\"pull-right\" @click=\"onNext\">\n            Next\n        </RockButton>\n    </div>\n</div>\n"
            }));
        }
    };
});
//# sourceMappingURL=Registrant.js.map