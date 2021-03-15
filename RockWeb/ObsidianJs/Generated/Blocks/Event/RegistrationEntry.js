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
System.register(["vue", "../../Elements/RockButton", "../../Util/Guid", "./RegistrationEntry/Intro", "./RegistrationEntry/Registrants", "./RegistrationEntry/RegistrationStart", "./RegistrationEntry/RegistrationEnd", "./RegistrationEntry/Summary"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockButton_1, Guid_1, Intro_1, Registrants_1, RegistrationStart_1, RegistrationEnd_1, Summary_1, Registrants_2;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (Intro_1_1) {
                Intro_1 = Intro_1_1;
            },
            function (Registrants_1_1) {
                Registrants_1 = Registrants_1_1;
                Registrants_2 = Registrants_1_1;
            },
            function (RegistrationStart_1_1) {
                RegistrationStart_1 = RegistrationStart_1_1;
            },
            function (RegistrationEnd_1_1) {
                RegistrationEnd_1 = RegistrationEnd_1_1;
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
                    Registrants: Registrants_2.default,
                    RegistrationEntryIntro: Intro_1.default,
                    RegistrationEntryRegistrants: Registrants_1.default,
                    RegistrationEntryRegistrationStart: RegistrationStart_1.default,
                    RegistrationEntryRegistrationEnd: RegistrationEnd_1.default,
                    RegistrationEntrySummary: Summary_1.default
                },
                setup: function () {
                    return {
                        viewModel: vue_1.inject('configurationValues')
                    };
                },
                data: function () {
                    var steps = {
                        intro: 'intro',
                        registrationStartForm: 'registrationStartForm',
                        perRegistrantForms: 'perRegistrantForms',
                        registrationEndForm: 'registrationEndForm',
                        reviewAndPayment: 'reviewAndPayment'
                    };
                    return {
                        steps: steps,
                        currentStep: steps.intro,
                        registrants: [],
                        registrationFieldValues: {}
                    };
                },
                methods: {
                    onIntroNext: function (_a) {
                        var numberOfRegistrants = _a.numberOfRegistrants;
                        // Resize the registrant array to match the selected number
                        while (numberOfRegistrants > this.registrants.length) {
                            this.registrants.push({
                                FamilyGuid: null,
                                FieldValues: {},
                                FeeQuantities: {},
                                Guid: Guid_1.newGuid()
                            });
                        }
                        this.registrants.length = numberOfRegistrants;
                        // Advance to the next step
                        this.currentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
                    },
                    onRegistrationStartPrevious: function () {
                        this.currentStep = this.steps.intro;
                    },
                    onRegistrationStartNext: function () {
                        this.currentStep = this.steps.perRegistrantForms;
                    },
                    onRegistrantPrevious: function () {
                        this.currentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
                    },
                    onRegistrantNext: function () {
                        this.currentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
                    },
                    onRegistrationEndPrevious: function () {
                        this.currentStep = this.steps.perRegistrantForms;
                    },
                    onRegistrationEndNext: function () {
                        this.currentStep = this.steps.reviewAndPayment;
                    },
                    onSummaryPrevious: function () {
                        this.currentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
                    }
                },
                computed: {
                    hasPreAttributes: function () {
                        return this.viewModel.RegistrationAttributesStart.length > 0;
                    },
                    hasPostAttributes: function () {
                        return this.viewModel.RegistrationAttributesEnd.length > 0;
                    },
                    numberOfPages: function () {
                        return 2 + // Intro and summary
                            (this.hasPostAttributes ? 1 : 0) +
                            (this.hasPreAttributes ? 1 : 0) +
                            (this.viewModel.RegistrantForms.length * this.registrants.length);
                    }
                },
                template: "\n<div>\n    <RegistrationEntryIntro v-if=\"currentStep === steps.intro\" @next=\"onIntroNext\" :initialRegistrantCount=\"registrants.length\" />\n    <RegistrationEntryRegistrationStart v-else-if=\"currentStep === steps.registrationStartForm\" :registrationFieldValues=\"registrationFieldValues\" :registrantCount=\"registrants.length\" @next=\"onRegistrationStartNext\" @previous=\"onRegistrationStartPrevious\"  :numberOfPages=\"numberOfPages\" />\n    <RegistrationEntryRegistrants v-else-if=\"currentStep === steps.perRegistrantForms\" :registrants=\"registrants\" @next=\"onRegistrantNext\" @previous=\"onRegistrantPrevious\" :numberOfPages=\"numberOfPages\" />\n    <RegistrationEntryRegistrationEnd v-else-if=\"currentStep === steps.registrationEndForm\" :registrationFieldValues=\"registrationFieldValues\" @next=\"onRegistrationEndNext\" @previous=\"onRegistrationEndPrevious\" :numberOfPages=\"numberOfPages\" />\n    <RegistrationEntrySummary v-else-if=\"currentStep === steps.reviewAndPayment\" :registrants=\"registrants\" @previous=\"onSummaryPrevious\" />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map