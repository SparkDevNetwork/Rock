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
System.register(["vue", "../../Elements/RockButton", "./RegistrationEntry/Intro", "./RegistrationEntry/Registrants", "./RegistrationEntry/RegistrationStart", "./RegistrationEntry/RegistrationEnd", "./RegistrationEntry/Summary", "../../Elements/ProgressBar", "../../Services/Number", "../../Services/String", "../../Elements/Alert"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockButton_1, Intro_1, Registrants_1, RegistrationStart_1, RegistrationEnd_1, Summary_1, Registrants_2, ProgressBar_1, Number_1, String_1, Alert_1;
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
            },
            function (ProgressBar_1_1) {
                ProgressBar_1 = ProgressBar_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
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
                    RegistrationEntrySummary: Summary_1.default,
                    ProgressBar: ProgressBar_1.default,
                    Alert: Alert_1.default
                },
                setup: function () {
                    var steps = {
                        intro: 'intro',
                        registrationStartForm: 'registrationStartForm',
                        perRegistrantForms: 'perRegistrantForms',
                        registrationEndForm: 'registrationEndForm',
                        reviewAndPayment: 'reviewAndPayment'
                    };
                    var viewModel = vue_1.inject('configurationValues');
                    var registrationEntryState = vue_1.reactive({
                        Steps: steps,
                        ViewModel: viewModel,
                        CurrentStep: steps.intro,
                        CurrentRegistrantFormIndex: 0,
                        CurrentRegistrantIndex: 0,
                        Registrants: [],
                        RegistrationFieldValues: {}
                    });
                    vue_1.provide('registrationEntryState', registrationEntryState);
                    return {
                        viewModel: viewModel,
                        steps: steps,
                        registrationEntryState: registrationEntryState
                    };
                },
                computed: {
                    viewModel: function () {
                        return this.registrationEntryState.ViewModel;
                    },
                    currentStep: function () {
                        return this.registrationEntryState.CurrentStep;
                    },
                    registrants: function () {
                        return this.registrationEntryState.Registrants;
                    },
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
                    },
                    completionPercentDecimal: function () {
                        if (this.currentStep === this.steps.intro) {
                            return 0;
                        }
                        if (this.currentStep === this.steps.registrationStartForm) {
                            return 1 / this.numberOfPages;
                        }
                        if (this.currentStep === this.steps.perRegistrantForms) {
                            var firstRegistrantPage = this.viewModel.RegistrationAttributesStart.length === 0 ? 1 : 2;
                            var finishedRegistrantForms = this.registrationEntryState.CurrentRegistrantIndex * this.viewModel.RegistrantForms.length;
                            return (firstRegistrantPage + this.registrationEntryState.CurrentRegistrantFormIndex + finishedRegistrantForms) / this.numberOfPages;
                        }
                        if (this.currentStep === this.steps.registrationEndForm) {
                            return (this.numberOfPages - 2) / this.numberOfPages;
                        }
                        if (this.currentStep === this.steps.reviewAndPayment) {
                            return (this.numberOfPages - 1) / this.numberOfPages;
                        }
                        return 0;
                    },
                    completionPercentInt: function () {
                        return this.completionPercentDecimal * 100;
                    },
                    uppercaseRegistrantTerm: function () {
                        return String_1.default.toTitleCase(this.viewModel.RegistrantTerm);
                    },
                    currentRegistrantTitle: function () {
                        var ordinal = Number_1.default.toOrdinal(this.registrationEntryState.CurrentRegistrantIndex + 1);
                        var title = String_1.default.toTitleCase(this.registrants.length <= 1 ?
                            this.uppercaseRegistrantTerm :
                            ordinal + ' ' + this.uppercaseRegistrantTerm);
                        if (this.registrationEntryState.CurrentRegistrantFormIndex > 0) {
                            title += ' (cont)';
                        }
                        return title;
                    },
                    stepTitle: function () {
                        if (this.currentStep === this.steps.registrationStartForm) {
                            return this.viewModel.RegistrationAttributeTitleStart;
                        }
                        if (this.currentStep === this.steps.perRegistrantForms) {
                            return this.currentRegistrantTitle;
                        }
                        if (this.currentStep === this.steps.registrationStartForm) {
                            return this.viewModel.RegistrationAttributeTitleEnd;
                        }
                        return '';
                    }
                },
                methods: {
                    onIntroNext: function () {
                        this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
                    },
                    onRegistrationStartPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.steps.intro;
                    },
                    onRegistrationStartNext: function () {
                        this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
                    },
                    onRegistrantPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
                    },
                    onRegistrantNext: function () {
                        this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
                    },
                    onRegistrationEndPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
                    },
                    onRegistrationEndNext: function () {
                        this.registrationEntryState.CurrentStep = this.steps.reviewAndPayment;
                    },
                    onSummaryPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
                    }
                },
                template: "\n<div>\n    <template v-if=\"currentStep !== steps.intro\">\n        <h1>{{stepTitle}}</h1>\n        <ProgressBar :percent=\"completionPercentInt\" />\n    </template>\n\n    <RegistrationEntryIntro v-if=\"currentStep === steps.intro\" @next=\"onIntroNext\" />\n    <RegistrationEntryRegistrationStart v-else-if=\"currentStep === steps.registrationStartForm\" @next=\"onRegistrationStartNext\" @previous=\"onRegistrationStartPrevious\" />\n    <RegistrationEntryRegistrants v-else-if=\"currentStep === steps.perRegistrantForms\" @next=\"onRegistrantNext\" @previous=\"onRegistrantPrevious\" />\n    <RegistrationEntryRegistrationEnd v-else-if=\"currentStep === steps.registrationEndForm\" @next=\"onRegistrationEndNext\" @previous=\"onRegistrationEndPrevious\" />\n    <RegistrationEntrySummary v-else-if=\"currentStep === steps.reviewAndPayment\" @previous=\"onSummaryPrevious\" />\n    <Alert v-else alertType=\"danger\">Invalid State: '{{currentStep}}'</Alert>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map