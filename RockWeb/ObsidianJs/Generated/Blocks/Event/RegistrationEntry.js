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
System.register(["vue", "../../Elements/RockButton", "../../Util/Guid", "./RegistrationEntry/Intro", "./RegistrationEntry/Registrants", "./RegistrationEntry/RegistrationEntryBlockViewModel", "./RegistrationEntry/RegistrationStart", "./RegistrationEntry/RegistrationEnd", "./RegistrationEntry/Summary", "../../Services/Number", "../../Services/String", "../../Elements/Alert", "./RegistrationEntry/Success", "../../Util/Page", "../../Elements/ProgressTracker"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockButton_1, Guid_1, Intro_1, Registrants_1, RegistrationEntryBlockViewModel_1, RegistrationStart_1, RegistrationEnd_1, Summary_1, Registrants_2, Number_1, String_1, Alert_1, Success_1, Page_1, ProgressTracker_1, Step;
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
            },
            function (ProgressTracker_1_1) {
                ProgressTracker_1 = ProgressTracker_1_1;
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
                    Alert: Alert_1.default,
                    ProgressTracker: ProgressTracker_1.default
                },
                setup: function () {
                    var _a;
                    var _b, _c, _d, _e, _f;
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
                    if (viewModel.SuccessViewModel) {
                        // This is after having paid via redirect gateway
                        currentStep = steps.success;
                    }
                    else if (viewModel.Session) {
                        // This is an existing registration, start at the summary
                        currentStep = steps.reviewAndPayment;
                    }
                    else if (viewModel.MaxRegistrants === 1 && String_1.isNullOrWhitespace(viewModel.InstructionsHtml)) {
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
                        Registrants: ((_c = viewModel.Session) === null || _c === void 0 ? void 0 : _c.Registrants) || [getDefaultRegistrantInfo()],
                        RegistrationFieldValues: ((_d = viewModel.Session) === null || _d === void 0 ? void 0 : _d.FieldValues) || {},
                        Registrar: ((_e = viewModel.Session) === null || _e === void 0 ? void 0 : _e.Registrar) || {
                            NickName: '',
                            LastName: '',
                            Email: '',
                            UpdateEmail: true
                        },
                        GatewayToken: '',
                        DiscountCode: ((_f = viewModel.Session) === null || _f === void 0 ? void 0 : _f.DiscountCode) || '',
                        SuccessViewModel: viewModel.SuccessViewModel
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
                    mustLogin: function () {
                        return !this.$store.state.currentPerson && (this.viewModel.IsUnauthorized || this.viewModel.LoginRequiredToRegister);
                    },
                    isUnauthorized: function () {
                        return this.viewModel.IsUnauthorized;
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
                    progressTrackerIndex: function () {
                        if (this.currentStep === this.steps.intro) {
                            return 0;
                        }
                        if (this.currentStep === this.steps.registrationStartForm) {
                            return 1;
                        }
                        var stepsBeforeRegistrants = this.hasPreAttributes ? 2 : 1;
                        if (this.currentStep === this.steps.perRegistrantForms) {
                            return this.registrationEntryState.CurrentRegistrantIndex + stepsBeforeRegistrants;
                        }
                        var stepsToCompleteRegistrants = this.registrationEntryState.Registrants.length + stepsBeforeRegistrants;
                        if (this.currentStep === this.steps.registrationEndForm) {
                            return stepsToCompleteRegistrants;
                        }
                        if (this.currentStep === this.steps.reviewAndPayment) {
                            return stepsToCompleteRegistrants + (this.hasPostAttributes ? 1 : 0);
                        }
                        return 0;
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
                    },
                    /** The items to display in the progress tracker */
                    progressTrackerItems: function () {
                        var items = [{
                                Key: 'Start',
                                Title: 'Start',
                                Subtitle: this.viewModel.RegistrationTerm
                            }];
                        if (this.hasPreAttributes) {
                            items.push({
                                Key: 'Pre',
                                Title: this.viewModel.RegistrationAttributeTitleStart,
                                Subtitle: this.viewModel.RegistrationTerm
                            });
                        }
                        if (!this.registrationEntryState.Registrants.length) {
                            items.push({
                                Key: 'Registrant',
                                Title: String_1.toTitleCase(this.viewModel.RegistrantTerm),
                                Subtitle: this.viewModel.RegistrationTerm
                            });
                        }
                        for (var i = 0; i < this.registrationEntryState.Registrants.length; i++) {
                            var registrant = this.registrationEntryState.Registrants[i];
                            var info = getRegistrantBasicInfo(registrant, this.viewModel.RegistrantForms);
                            if ((info === null || info === void 0 ? void 0 : info.FirstName) && (info === null || info === void 0 ? void 0 : info.LastName)) {
                                items.push({
                                    Key: "Registrant-" + registrant.Guid,
                                    Title: info.FirstName,
                                    Subtitle: info.LastName
                                });
                            }
                            else {
                                items.push({
                                    Key: "Registrant-" + registrant.Guid,
                                    Title: String_1.toTitleCase(this.viewModel.RegistrantTerm),
                                    Subtitle: String_1.toTitleCase(Number_1.toWord(i + 1))
                                });
                            }
                        }
                        if (this.hasPostAttributes) {
                            items.push({
                                Key: 'Post',
                                Title: this.viewModel.RegistrationAttributeTitleEnd,
                                Subtitle: this.viewModel.RegistrationTerm
                            });
                        }
                        items.push({
                            Key: 'Finalize',
                            Title: 'Finalize',
                            Subtitle: this.viewModel.RegistrationTerm
                        });
                        return items;
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
                mounted: function () {
                    if (this.viewModel.LoginRequiredToRegister && !this.$store.state.currentPerson) {
                        this.$store.dispatch('redirectToLogin');
                    }
                },
                template: "\n<div>\n    <Alert v-if=\"notFound\" alertType=\"warning\">\n        <strong>Sorry</strong>\n        <p>The selected registration could not be found or is no longer active.</p>\n    </Alert>\n    <Alert v-else-if=\"mustLogin\" alertType=\"warning\">\n        <strong>Please log in</strong>\n        <p>You must be logged in to access this registration.</p>\n    </Alert>\n    <Alert v-else-if=\"isUnauthorized\" alertType=\"warning\">\n        <strong>Sorry</strong>\n        <p>You are not allowed to view or edit the selected registration since you are not the one who created the registration.</p>\n    </Alert>\n    <template v-else>\n        <h1 v-if=\"currentStep !== steps.intro\" v-html=\"stepTitleHtml\"></h1>\n        <ProgressTracker v-if=\"currentStep !== steps.success\" :items=\"progressTrackerItems\" :currentIndex=\"progressTrackerIndex\">\n            <template #aside>\n                <div class=\"remaining-time flex-grow-1 flex-md-grow-0\">\n                    <span class=\"remaining-time-title\">Time left before timeout</span>\n                    <p class=\"remaining-time-countdown\">10:34</p>\n                </div>\n            </template>\n        </ProgressTracker>\n        <RegistrationEntryIntro v-if=\"currentStep === steps.intro\" @next=\"onIntroNext\" />\n        <RegistrationEntryRegistrationStart v-else-if=\"currentStep === steps.registrationStartForm\" @next=\"onRegistrationStartNext\" @previous=\"onRegistrationStartPrevious\" />\n        <RegistrationEntryRegistrants v-else-if=\"currentStep === steps.perRegistrantForms\" @next=\"onRegistrantNext\" @previous=\"onRegistrantPrevious\" />\n        <RegistrationEntryRegistrationEnd v-else-if=\"currentStep === steps.registrationEndForm\" @next=\"onRegistrationEndNext\" @previous=\"onRegistrationEndPrevious\" />\n        <RegistrationEntrySummary v-else-if=\"currentStep === steps.reviewAndPayment\" @next=\"onSummaryNext\" @previous=\"onSummaryPrevious\" />\n        <RegistrationEntrySuccess v-else-if=\"currentStep === steps.success\" />\n        <Alert v-else alertType=\"danger\">Invalid State: '{{currentStep}}'</Alert>\n    </template>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map