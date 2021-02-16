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
System.register(["vue", "../../Elements/NumberUpDown", "../../Elements/ProgressBar", "../../Elements/RockButton", "../../Filters/Number", "../../Filters/String"], function (exports_1, context_1) {
    "use strict";
    var vue_1, NumberUpDown_1, ProgressBar_1, RockButton_1, Number_1, String_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (NumberUpDown_1_1) {
                NumberUpDown_1 = NumberUpDown_1_1;
            },
            function (ProgressBar_1_1) {
                ProgressBar_1 = ProgressBar_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry',
                components: {
                    NumberUpDown: NumberUpDown_1.default,
                    RockButton: RockButton_1.default,
                    ProgressBar: ProgressBar_1.default
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
                        currentRegistrantIndex: 0,
                        currentFormIndex: 0,
                        numberOfRegistrants: 1,
                        registrationInstance: this.configurationValues['registrationInstance'],
                        registrationTemplate: this.configurationValues['registrationTemplate'],
                        registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || [])
                    };
                },
                computed: {
                    formCountPerRegistrant: function () {
                        return this.registrationTemplateForms.length;
                    },
                    currentRegistrantTitle: function () {
                        var ordinal = Number_1.default.toOrdinal(this.currentRegistrantIndex + 1);
                        var title = String_1.default.toTitleCase(this.numberOfRegistrants <= 1 ?
                            this.registrantTerm :
                            ordinal + ' ' + this.registrantTerm);
                        if (this.currentFormIndex > 0) {
                            title += ' (cont)';
                        }
                        return title;
                    },
                    registrantTerm: function () {
                        var _a, _b;
                        return ((_b = (_a = this.registrationTemplate) === null || _a === void 0 ? void 0 : _a.RegistrantTerm) === null || _b === void 0 ? void 0 : _b.toLowerCase()) || 'registrant';
                    },
                    pluralRegistrantTerm: function () {
                        var _a, _b;
                        return ((_b = (_a = this.registrationTemplate) === null || _a === void 0 ? void 0 : _a.PluralRegistrantTerm) === null || _b === void 0 ? void 0 : _b.toLowerCase()) || 'registrants';
                    },
                    registrationInstructions: function () {
                        var _a, _b;
                        return ((_a = this.registrationInstance) === null || _a === void 0 ? void 0 : _a.RegistrationInstructions) || ((_b = this.registrationTemplate) === null || _b === void 0 ? void 0 : _b.RegistrationInstructions) || '';
                    },
                    numberOfPages: function () {
                        // All of the steps are 1 page except the "per-registrant"
                        return 3 + (this.numberOfRegistrants * this.formCountPerRegistrant);
                    },
                    completionPercentDecimal: function () {
                        switch (this.currentStep) {
                            case this.steps.intro:
                                return 0;
                            case this.steps.perRegistrantForms:
                                return (1 + this.currentFormIndex + this.currentRegistrantIndex * this.formCountPerRegistrant) / this.numberOfPages;
                            case this.steps.registrationForm:
                                return (this.numberOfPages - 2) / this.numberOfPages;
                            case this.steps.reviewAndPayment:
                                return (this.numberOfPages - 1) / this.numberOfPages;
                            default:
                                return 0;
                        }
                    },
                    completionPercentInt: function () {
                        return this.completionPercentDecimal * 100;
                    }
                },
                methods: {
                    onIntroNext: function () {
                        this.currentStep = this.steps.perRegistrantForms;
                        this.currentRegistrantIndex = 0;
                        this.currentFormIndex = 0;
                    },
                    onRegistrantPrevious: function () {
                        var lastFormIndex = this.formCountPerRegistrant - 1;
                        if (this.currentFormIndex <= 0 && this.currentRegistrantIndex <= 0) {
                            this.currentStep = this.steps.intro;
                            return;
                        }
                        if (this.currentFormIndex <= 0) {
                            this.currentRegistrantIndex--;
                            this.currentFormIndex = lastFormIndex;
                            return;
                        }
                        this.currentFormIndex--;
                    },
                    onRegistrantNext: function () {
                        var lastFormIndex = this.formCountPerRegistrant - 1;
                        var lastRegistrantIndex = this.numberOfRegistrants - 1;
                        if (this.currentFormIndex >= lastFormIndex && this.currentRegistrantIndex >= lastRegistrantIndex) {
                            this.currentStep = this.steps.registrationForm;
                            return;
                        }
                        if (this.currentFormIndex >= lastFormIndex) {
                            this.currentRegistrantIndex++;
                            this.currentFormIndex = 0;
                            return;
                        }
                        this.currentFormIndex++;
                    },
                    onRegistrationPrevious: function () {
                        var lastFormIndex = this.formCountPerRegistrant - 1;
                        var lastRegistrantIndex = this.numberOfRegistrants - 1;
                        this.currentStep = this.steps.perRegistrantForms;
                        this.currentRegistrantIndex = lastRegistrantIndex;
                        this.currentFormIndex = lastFormIndex;
                    },
                    onRegistrationNext: function () {
                        this.currentStep = this.steps.reviewAndPayment;
                    },
                    onSummaryPrevious: function () {
                        this.currentStep = this.steps.registrationForm;
                    }
                },
                template: "\n<div>\n\n    <div v-if=\"currentStep === steps.intro\" class=\"registrationentry-intro\">\n        <div class=\"text-left\" v-html=\"registrationInstructions\">\n        </div>\n        <div class=\"registrationentry-intro\">\n            <h1>How many {{pluralRegistrantTerm}} will you be registering?</h1>\n            <NumberUpDown v-model=\"numberOfRegistrants\" class=\"margin-t-sm input-lg\" />\n        </div>\n        <div class=\"actions\">\n            <RockButton btnType=\"primary\" class=\"pull-right\" @click=\"onIntroNext\">\n                Next\n            </RockButton>\n        </div>\n    </div>\n\n    <div v-if=\"currentStep === steps.perRegistrantForms\" class=\"registrationentry-registrant\">\n        <h1>{{currentRegistrantTitle}}</h1>\n        <ProgressBar :percent=\"completionPercentInt\" />\n\n        <div class=\"actions\">\n            <RockButton btnType=\"default\" @click=\"onRegistrantPrevious\">\n                Previous\n            </RockButton>\n            <RockButton btnType=\"primary\" class=\"pull-right\" @click=\"onRegistrantNext\">\n                Next\n            </RockButton>\n        </div>\n    </div>\n\n    <div v-if=\"currentStep === steps.registrationForm\" class=\"registrationentry-registration-attributes\">\n        <h1>Registration Attributes</h1>\n        <ProgressBar :percent=\"completionPercentInt\" />\n\n        <div class=\"actions\">\n            <RockButton btnType=\"default\" @click=\"onRegistrationPrevious\">\n                Previous\n            </RockButton>\n            <RockButton btnType=\"primary\" class=\"pull-right\" @click=\"onRegistrationNext\">\n                Next\n            </RockButton>\n        </div>\n    </div>\n\n    <div v-if=\"currentStep === steps.reviewAndPayment\" class=\"registrationentry-summary\">\n        <h1>Summary</h1>\n        <ProgressBar :percent=\"completionPercentInt\" />\n\n        <div class=\"actions\">\n            <RockButton btnType=\"default\" @click=\"onSummaryPrevious\">\n                Previous\n            </RockButton>\n        </div>\n    </div>\n\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map