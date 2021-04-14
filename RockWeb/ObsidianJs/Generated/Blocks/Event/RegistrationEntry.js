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
System.register(["vue", "../../Elements/RockButton", "../../Util/Guid", "./RegistrationEntry/Intro", "./RegistrationEntry/Registrants", "./RegistrationEntry/RegistrationEntryBlockViewModel", "./RegistrationEntry/RegistrationStart", "./RegistrationEntry/RegistrationEnd", "./RegistrationEntry/Summary", "../../Elements/ProgressBar", "../../Services/Number", "../../Services/String", "../../Elements/Alert", "./RegistrationEntry/Success", "../../Util/Page"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockButton_1, Guid_1, Intro_1, Registrants_1, RegistrationEntryBlockViewModel_1, RegistrationStart_1, RegistrationEnd_1, Summary_1, Registrants_2, ProgressBar_1, Number_1, String_1, Alert_1, Success_1, Page_1, Step;
    var __moduleName = context_1 && context_1.id;
    function getDefaultRegistrantInfo() {
        var ownFamilyGuid = Guid_1.newGuid();
        return {
            IsOnWaitList: false,
            FamilyGuid: ownFamilyGuid,
            FieldValues: {},
            FeeItemQuantities: {},
            Guid: Guid_1.newGuid(),
            PersonGuid: '',
            OwnFamilyGuid: ownFamilyGuid
        };
    }
    exports_1("getDefaultRegistrantInfo", getDefaultRegistrantInfo);
    function getRegistrantBasicInfo(registrant, registrantForms) {
        var _a, _b, _c;
        var fields = (registrantForms === null || registrantForms === void 0 ? void 0 : registrantForms.flatMap(function (f) { return f.Fields; })) || [];
        var firstNameGuid = ((_a = fields.find(function (f) { return f.PersonFieldType === RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.FirstName; })) === null || _a === void 0 ? void 0 : _a.Guid) || '';
        var lastNameGuid = ((_b = fields.find(function (f) { return f.PersonFieldType === RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.LastName; })) === null || _b === void 0 ? void 0 : _b.Guid) || '';
        var emailGuid = ((_c = fields.find(function (f) { return f.PersonFieldType === RegistrationEntryBlockViewModel_1.RegistrationPersonFieldType.Email; })) === null || _c === void 0 ? void 0 : _c.Guid) || '';
        return {
            FirstName: ((registrant === null || registrant === void 0 ? void 0 : registrant.FieldValues[firstNameGuid]) || ''),
            LastName: ((registrant === null || registrant === void 0 ? void 0 : registrant.FieldValues[lastNameGuid]) || ''),
            Email: ((registrant === null || registrant === void 0 ? void 0 : registrant.FieldValues[emailGuid]) || ''),
            Guid: registrant === null || registrant === void 0 ? void 0 : registrant.Guid
        };
    }
    exports_1("getRegistrantBasicInfo", getRegistrantBasicInfo);
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
            function (RegistrationEntryBlockViewModel_1_1) {
                RegistrationEntryBlockViewModel_1 = RegistrationEntryBlockViewModel_1_1;
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
            },
            function (Success_1_1) {
                Success_1 = Success_1_1;
            },
            function (Page_1_1) {
                Page_1 = Page_1_1;
            }
        ],
        execute: function () {
            (function (Step) {
                Step["intro"] = "intro";
                Step["registrationStartForm"] = "registrationStartForm";
                Step["perRegistrantForms"] = "perRegistrantForms";
                Step["registrationEndForm"] = "registrationEndForm";
                Step["reviewAndPayment"] = "reviewAndPayment";
                Step["success"] = "success";
            })(Step || (Step = {}));
            exports_1("Step", Step);
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
                    RegistrationEntrySuccess: Success_1.default,
                    ProgressBar: ProgressBar_1.default,
                    Alert: Alert_1.default
                },
                setup: function () {
                    var _a;
                    var _b;
                    var steps = (_a = {},
                        _a[Step.intro] = Step.intro,
                        _a[Step.registrationStartForm] = Step.registrationStartForm,
                        _a[Step.perRegistrantForms] = Step.perRegistrantForms,
                        _a[Step.registrationEndForm] = Step.registrationEndForm,
                        _a[Step.reviewAndPayment] = Step.reviewAndPayment,
                        _a[Step.success] = Step.success,
                        _a);
                    var notFound = vue_1.ref(false);
                    var viewModel = vue_1.inject('configurationValues');
                    if (!(viewModel === null || viewModel === void 0 ? void 0 : viewModel.RegistrationAttributesStart)) {
                        notFound.value = true;
                    }
                    var hasPreAttributes = ((_b = viewModel.RegistrationAttributesStart) === null || _b === void 0 ? void 0 : _b.length) > 0;
                    var currentStep = steps.intro;
                    if (viewModel.MaxRegistrants === 1 && String_1.isNullOrWhitespace(viewModel.InstructionsHtml)) {
                        // There is no need to show the numer of registrants selector or instructions. Start at the second page.
                        currentStep = hasPreAttributes ? steps.registrationStartForm : steps.perRegistrantForms;
                    }
                    var registrationEntryState = vue_1.reactive({
                        Steps: steps,
                        ViewModel: viewModel,
                        FirstStep: currentStep,
                        CurrentStep: currentStep,
                        CurrentRegistrantFormIndex: 0,
                        CurrentRegistrantIndex: 0,
                        Registrants: [getDefaultRegistrantInfo()],
                        RegistrationFieldValues: {},
                        Registrar: {
                            NickName: '',
                            LastName: '',
                            Email: '',
                            UpdateEmail: true
                        },
                        GatewayToken: '',
                        DiscountCode: '',
                        SuccessViewModel: null
                    });
                    vue_1.provide('registrationEntryState', registrationEntryState);
                    return {
                        viewModel: viewModel,
                        steps: steps,
                        registrationEntryState: registrationEntryState,
                        notFound: notFound
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
                        if (this.currentStep === this.steps.success) {
                            return 1;
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
                    stepTitleHtml: function () {
                        var _a;
                        if (this.currentStep === this.steps.registrationStartForm) {
                            return this.viewModel.RegistrationAttributeTitleStart;
                        }
                        if (this.currentStep === this.steps.perRegistrantForms) {
                            return this.currentRegistrantTitle;
                        }
                        if (this.currentStep === this.steps.registrationEndForm) {
                            return this.viewModel.RegistrationAttributeTitleEnd;
                        }
                        if (this.currentStep === this.steps.reviewAndPayment) {
                            return 'Review Registration';
                        }
                        if (this.currentStep === this.steps.success) {
                            return ((_a = this.registrationEntryState.SuccessViewModel) === null || _a === void 0 ? void 0 : _a.TitleHtml) || 'Congratulations';
                        }
                        return '';
                    }
                },
                methods: {
                    onIntroNext: function () {
                        this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
                        Page_1.default.smoothScrollToTop();
                    },
                    onRegistrationStartPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.steps.intro;
                        Page_1.default.smoothScrollToTop();
                    },
                    onRegistrationStartNext: function () {
                        this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
                        Page_1.default.smoothScrollToTop();
                    },
                    onRegistrantPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
                        Page_1.default.smoothScrollToTop();
                    },
                    onRegistrantNext: function () {
                        this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
                        Page_1.default.smoothScrollToTop();
                    },
                    onRegistrationEndPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
                        Page_1.default.smoothScrollToTop();
                    },
                    onRegistrationEndNext: function () {
                        this.registrationEntryState.CurrentStep = this.steps.reviewAndPayment;
                        Page_1.default.smoothScrollToTop();
                    },
                    onSummaryPrevious: function () {
                        this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
                        Page_1.default.smoothScrollToTop();
                    },
                    onSummaryNext: function () {
                        this.registrationEntryState.CurrentStep = this.steps.success;
                        Page_1.default.smoothScrollToTop();
                    }
                },
                template: "\n<div>\n    <Alert v-if=\"notFound\" alertType=\"warning\">\n        <strong>Sorry</strong>\n        <p>The selected registration could not be found or is no longer active.</p>\n    </Alert>\n    <template v-else>\n        <template v-if=\"currentStep !== steps.intro\">\n            <h1 v-html=\"stepTitleHtml\"></h1>\n            <ProgressBar :percent=\"completionPercentInt\" />\n        </template>\n\n        <RegistrationEntryIntro v-if=\"currentStep === steps.intro\" @next=\"onIntroNext\" />\n        <RegistrationEntryRegistrationStart v-else-if=\"currentStep === steps.registrationStartForm\" @next=\"onRegistrationStartNext\" @previous=\"onRegistrationStartPrevious\" />\n        <RegistrationEntryRegistrants v-else-if=\"currentStep === steps.perRegistrantForms\" @next=\"onRegistrantNext\" @previous=\"onRegistrantPrevious\" />\n        <RegistrationEntryRegistrationEnd v-else-if=\"currentStep === steps.registrationEndForm\" @next=\"onRegistrationEndNext\" @previous=\"onRegistrationEndPrevious\" />\n        <RegistrationEntrySummary v-else-if=\"currentStep === steps.reviewAndPayment\" @next=\"onSummaryNext\" @previous=\"onSummaryPrevious\" />\n        <RegistrationEntrySuccess v-else-if=\"currentStep === steps.success\" />\n        <Alert v-else alertType=\"danger\">Invalid State: '{{currentStep}}'</Alert>\n    </template>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map