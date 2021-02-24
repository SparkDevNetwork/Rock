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
System.register(["vue", "../../Elements/RockButton", "./RegistrationEntry/Intro", "./RegistrationEntry/Registrant", "./RegistrationEntry/Registration", "./RegistrationEntry/Summary"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockButton_1, Intro_1, Registrant_1, Registration_1, Summary_1, RegistrationPersonFieldType, RegistrationFieldSource;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (Intro_1_1) {
                Intro_1 = Intro_1_1;
            },
            function (Registrant_1_1) {
                Registrant_1 = Registrant_1_1;
            },
            function (Registration_1_1) {
                Registration_1 = Registration_1_1;
            },
            function (Summary_1_1) {
                Summary_1 = Summary_1_1;
            }
        ],
        execute: function () {
            (function (RegistrationPersonFieldType) {
                RegistrationPersonFieldType[RegistrationPersonFieldType["FirstName"] = 0] = "FirstName";
                RegistrationPersonFieldType[RegistrationPersonFieldType["LastName"] = 1] = "LastName";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Campus"] = 2] = "Campus";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Address"] = 3] = "Address";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Email"] = 4] = "Email";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Birthdate"] = 5] = "Birthdate";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Gender"] = 6] = "Gender";
                RegistrationPersonFieldType[RegistrationPersonFieldType["MaritalStatus"] = 7] = "MaritalStatus";
                RegistrationPersonFieldType[RegistrationPersonFieldType["MobilePhone"] = 8] = "MobilePhone";
                RegistrationPersonFieldType[RegistrationPersonFieldType["HomePhone"] = 9] = "HomePhone";
                RegistrationPersonFieldType[RegistrationPersonFieldType["WorkPhone"] = 10] = "WorkPhone";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Grade"] = 11] = "Grade";
                RegistrationPersonFieldType[RegistrationPersonFieldType["ConnectionStatus"] = 12] = "ConnectionStatus";
                RegistrationPersonFieldType[RegistrationPersonFieldType["MiddleName"] = 13] = "MiddleName";
                RegistrationPersonFieldType[RegistrationPersonFieldType["AnniversaryDate"] = 14] = "AnniversaryDate";
            })(RegistrationPersonFieldType || (RegistrationPersonFieldType = {}));
            exports_1("RegistrationPersonFieldType", RegistrationPersonFieldType);
            (function (RegistrationFieldSource) {
                RegistrationFieldSource[RegistrationFieldSource["PersonField"] = 0] = "PersonField";
                RegistrationFieldSource[RegistrationFieldSource["PersonAttribute"] = 1] = "PersonAttribute";
                RegistrationFieldSource[RegistrationFieldSource["GroupMemberAttribute"] = 2] = "GroupMemberAttribute";
                RegistrationFieldSource[RegistrationFieldSource["RegistrantAttribute"] = 4] = "RegistrantAttribute";
            })(RegistrationFieldSource || (RegistrationFieldSource = {}));
            exports_1("RegistrationFieldSource", RegistrationFieldSource);
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry',
                components: {
                    RockButton: RockButton_1.default,
                    RegistrationEntryIntro: Intro_1.default,
                    RegistrationEntryRegistrant: Registrant_1.default,
                    RegistrationEntryRegistration: Registration_1.default,
                    RegistrationEntrySummary: Summary_1.default
                },
                setup: function () {
                    return {
                        invokeBlockAction: vue_1.inject('invokeBlockAction'),
                        configurationValues: vue_1.inject('configurationValues')
                    };
                },
                data: function () {
                    var steps = {
                        intro: 'intro',
                        perRegistrantForms: 'perRegistrantForms',
                        registrationForm: 'registrationForm',
                        reviewAndPayment: 'reviewAndPayment'
                    };
                    return {
                        steps: steps,
                        currentStep: steps.intro,
                        registrants: [],
                        registrationInstance: this.configurationValues['registrationInstance'],
                        registrationTemplate: this.configurationValues['registrationTemplate'],
                        registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || [])
                    };
                },
                methods: {
                    onIntroNext: function (_a) {
                        var numberOfRegistrants = _a.numberOfRegistrants;
                        // Resize the registrant array to match the selected number
                        while (numberOfRegistrants > this.registrants.length) {
                            this.registrants.push({ FamilyGuid: null });
                        }
                        this.registrants.length = numberOfRegistrants;
                        // Advance to the next step
                        this.currentStep = this.steps.perRegistrantForms;
                    },
                    onRegistrantPrevious: function () {
                        this.currentStep = this.steps.intro;
                    },
                    onRegistrantNext: function () {
                        this.currentStep = this.steps.registrationForm;
                    },
                    onRegistrationPrevious: function () {
                        this.currentStep = this.steps.perRegistrantForms;
                    },
                    onRegistrationNext: function () {
                        this.currentStep = this.steps.reviewAndPayment;
                    },
                    onSummaryPrevious: function () {
                        this.currentStep = this.steps.registrationForm;
                    }
                },
                template: "\n<div>\n    <RegistrationEntryIntro v-show=\"currentStep === steps.intro\" @next=\"onIntroNext\" :initialRegistrantCount=\"registrants.length\" />\n    <RegistrationEntryRegistrant v-show=\"currentStep === steps.perRegistrantForms\" :registrants=\"registrants\" @next=\"onRegistrantNext\" @previous=\"onRegistrantPrevious\" />\n    <RegistrationEntryRegistration v-show=\"currentStep === steps.registrationForm\" :registrants=\"registrants\" @next=\"onRegistrationNext\" @previous=\"onRegistrationPrevious\" />\n    <RegistrationEntrySummary v-show=\"currentStep === steps.reviewAndPayment\" :registrants=\"registrants\" @previous=\"onSummaryPrevious\" />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map