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
    var vue_1, RockButton_1, Intro_1, Registrant_1, Registration_1, Summary_1;
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
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry',
                components: {
                    RockButton: RockButton_1.default,
                    RegistrationEntryIntro: Intro_1.default,
                    RegistrationEntryRegistrant: Registrant_1.default,
                    RegistrationEntryRegistration: Registration_1.default,
                    RegistrationEntrySummary: Summary_1.default
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
                        registrants: []
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
                template: "\n<div>\n    <RegistrationEntryIntro v-if=\"currentStep === steps.intro\" @next=\"onIntroNext\" :initialRegistrantCount=\"registrants.length\" />\n    <RegistrationEntryRegistrant v-else-if=\"currentStep === steps.perRegistrantForms\" :registrants=\"registrants\" @next=\"onRegistrantNext\" @previous=\"onRegistrantPrevious\" />\n    <RegistrationEntryRegistration v-else-if=\"currentStep === steps.registrationForm\" :registrants=\"registrants\" @next=\"onRegistrationNext\" @previous=\"onRegistrationPrevious\" />\n    <RegistrationEntrySummary v-else-if=\"currentStep === steps.reviewAndPayment\" :registrants=\"registrants\" @previous=\"onSummaryPrevious\" />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map