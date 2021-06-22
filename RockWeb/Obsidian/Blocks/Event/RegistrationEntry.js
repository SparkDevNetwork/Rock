System.register(["vue", "../../Elements/RockButton", "../../Util/Guid", "./RegistrationEntry/Intro", "./RegistrationEntry/Registrants", "./RegistrationEntry/RegistrationEntryBlockViewModel", "./RegistrationEntry/RegistrationStart", "./RegistrationEntry/RegistrationEnd", "./RegistrationEntry/Summary", "../../Elements/ProgressTracker", "../../Services/Number", "../../Services/String", "../../Elements/Alert", "../../Elements/CountdownTimer", "./RegistrationEntry/Success", "../../Util/Page", "../../Elements/JavaScriptAnchor", "./RegistrationEntry/SessionRenewal"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var __generator = (this && this.__generator) || function (thisArg, body) {
        var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
        return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
        function verb(n) { return function (v) { return step([n, v]); }; }
        function step(op) {
            if (f) throw new TypeError("Generator is already executing.");
            while (_) try {
                if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                if (y = 0, t) op = [op[0] & 2, t.value];
                switch (op[0]) {
                    case 0: case 1: t = op; break;
                    case 4: _.label++; return { value: op[1], done: false };
                    case 5: _.label++; y = op[1]; op = [0]; continue;
                    case 7: op = _.ops.pop(); _.trys.pop(); continue;
                    default:
                        if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                        if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                        if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                        if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                        if (t[2]) _.ops.pop();
                        _.trys.pop(); continue;
                }
                op = body.call(thisArg, _);
            } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
            if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
        }
    };
    var vue_1, RockButton_1, Guid_1, Intro_1, Registrants_1, RegistrationEntryBlockViewModel_1, RegistrationStart_1, RegistrationEnd_1, Summary_1, Registrants_2, ProgressTracker_1, Number_1, String_1, Alert_1, CountdownTimer_1, Success_1, Page_1, JavaScriptAnchor_1, SessionRenewal_1, Step, unknownSingleFamilyGuid;
    var __moduleName = context_1 && context_1.id;
    function getForcedFamilyGuid(currentPerson, viewModel) {
        return (currentPerson && viewModel.RegistrantsSameFamily === RegistrationEntryBlockViewModel_1.RegistrantsSameFamily.Yes) ?
            (currentPerson.PrimaryFamilyGuid || unknownSingleFamilyGuid) :
            null;
    }
    exports_1("getForcedFamilyGuid", getForcedFamilyGuid);
    function getDefaultRegistrantInfo(currentPerson, viewModel, familyGuid) {
        var forcedFamilyGuid = getForcedFamilyGuid(currentPerson, viewModel);
        var ownFamilyGuid = Guid_1.newGuid();
        if (forcedFamilyGuid) {
            familyGuid = forcedFamilyGuid;
        }
        if (!familyGuid) {
            familyGuid = ownFamilyGuid;
        }
        return {
            IsOnWaitList: false,
            FamilyGuid: familyGuid,
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
            function (ProgressTracker_1_1) {
                ProgressTracker_1 = ProgressTracker_1_1;
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
            function (CountdownTimer_1_1) {
                CountdownTimer_1 = CountdownTimer_1_1;
            },
            function (Success_1_1) {
                Success_1 = Success_1_1;
            },
            function (Page_1_1) {
                Page_1 = Page_1_1;
            },
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            },
            function (SessionRenewal_1_1) {
                SessionRenewal_1 = SessionRenewal_1_1;
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
            unknownSingleFamilyGuid = Guid_1.newGuid();
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
                    ProgressTracker: ProgressTracker_1.default,
                    Alert: Alert_1.default,
                    CountdownTimer: CountdownTimer_1.default,
                    JavaScriptAnchor: JavaScriptAnchor_1.default,
                    SessionRenewal: SessionRenewal_1.default
                },
                setup: function () {
                    var _a;
                    var _this = this;
                    var _b, _c, _d, _e, _f, _g, _h, _j;
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
                    var invokeBlockAction = vue_1.inject('invokeBlockAction');
                    if (!(viewModel === null || viewModel === void 0 ? void 0 : viewModel.RegistrationAttributesStart)) {
                        notFound.value = true;
                    }
                    var hasPreAttributes = ((_b = viewModel.RegistrationAttributesStart) === null || _b === void 0 ? void 0 : _b.length) > 0;
                    var currentStep = steps.intro;
                    if (viewModel.SuccessViewModel) {
                        currentStep = steps.success;
                    }
                    else if (viewModel.Session && !viewModel.StartAtBeginning) {
                        currentStep = steps.reviewAndPayment;
                    }
                    else if (viewModel.MaxRegistrants === 1 && String_1.isNullOrWhitespace(viewModel.InstructionsHtml)) {
                        currentStep = hasPreAttributes ? steps.registrationStartForm : steps.perRegistrantForms;
                    }
                    var registrationEntryState = vue_1.reactive({
                        Steps: steps,
                        ViewModel: viewModel,
                        FirstStep: currentStep,
                        CurrentStep: currentStep,
                        CurrentRegistrantFormIndex: 0,
                        CurrentRegistrantIndex: 0,
                        Registrants: ((_c = viewModel.Session) === null || _c === void 0 ? void 0 : _c.Registrants) || [getDefaultRegistrantInfo(null, viewModel, null)],
                        RegistrationFieldValues: ((_d = viewModel.Session) === null || _d === void 0 ? void 0 : _d.FieldValues) || {},
                        Registrar: ((_e = viewModel.Session) === null || _e === void 0 ? void 0 : _e.Registrar) || {
                            NickName: '',
                            LastName: '',
                            Email: '',
                            UpdateEmail: true,
                            OwnFamilyGuid: Guid_1.newGuid(),
                            FamilyGuid: null
                        },
                        GatewayToken: '',
                        DiscountCode: ((_f = viewModel.Session) === null || _f === void 0 ? void 0 : _f.DiscountCode) || '',
                        DiscountAmount: ((_g = viewModel.Session) === null || _g === void 0 ? void 0 : _g.DiscountAmount) || 0,
                        DiscountPercentage: ((_h = viewModel.Session) === null || _h === void 0 ? void 0 : _h.DiscountPercentage) || 0,
                        SuccessViewModel: viewModel.SuccessViewModel,
                        AmountToPayToday: 0,
                        SessionExpirationDate: null,
                        RegistrationSessionGuid: ((_j = viewModel.Session) === null || _j === void 0 ? void 0 : _j.RegistrationSessionGuid) || Guid_1.newGuid()
                    });
                    vue_1.provide('registrationEntryState', registrationEntryState);
                    var getRegistrationEntryBlockArgs = function () {
                        var _a;
                        return {
                            RegistrationSessionGuid: registrationEntryState.RegistrationSessionGuid,
                            GatewayToken: registrationEntryState.GatewayToken,
                            DiscountCode: registrationEntryState.DiscountCode,
                            FieldValues: registrationEntryState.RegistrationFieldValues,
                            Registrar: registrationEntryState.Registrar,
                            Registrants: registrationEntryState.Registrants,
                            AmountToPayNow: registrationEntryState.AmountToPayToday,
                            RegistrationGuid: ((_a = viewModel.Session) === null || _a === void 0 ? void 0 : _a.RegistrationGuid) || null
                        };
                    };
                    vue_1.provide('getRegistrationEntryBlockArgs', getRegistrationEntryBlockArgs);
                    var persistSession = function (force) {
                        if (force === void 0) { force = false; }
                        return __awaiter(_this, void 0, void 0, function () {
                            var response, asDate;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (!force && !viewModel.TimeoutMinutes) {
                                            return [2];
                                        }
                                        return [4, invokeBlockAction('PersistSession', {
                                                args: getRegistrationEntryBlockArgs()
                                            })];
                                    case 1:
                                        response = _a.sent();
                                        if (response.data) {
                                            asDate = new Date(response.data.ExpirationDateTime);
                                            registrationEntryState.SessionExpirationDate = asDate;
                                        }
                                        return [2];
                                }
                            });
                        });
                    };
                    vue_1.provide('persistSession', persistSession);
                    return {
                        viewModel: viewModel,
                        steps: steps,
                        registrationEntryState: registrationEntryState,
                        notFound: notFound,
                        persistSession: persistSession
                    };
                },
                data: function () {
                    return {
                        secondsBeforeExpiration: -1,
                        hasSessionRenewalSuccess: false
                    };
                },
                computed: {
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    isSessionExpired: function () {
                        return this.secondsBeforeExpiration === 0 && this.currentStep !== this.steps.success;
                    },
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
                        var stepsBeforePre = this.registrationEntryState.FirstStep === this.steps.intro ? 1 : 0;
                        if (this.currentStep === this.steps.registrationStartForm) {
                            return stepsBeforePre;
                        }
                        var stepsBeforeRegistrants = stepsBeforePre + (this.hasPreAttributes ? 1 : 0);
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
                    progressTrackerItems: function () {
                        var items = [];
                        if (this.registrationEntryState.FirstStep === this.steps.intro) {
                            items.push({
                                Key: 'Start',
                                Title: 'Start',
                                Subtitle: this.viewModel.RegistrationTerm
                            });
                        }
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
                    onSessionRenewalSuccess: function () {
                        var _this = this;
                        this.hasSessionRenewalSuccess = true;
                        setTimeout(function () { return _this.hasSessionRenewalSuccess = false; }, 5000);
                    },
                    onIntroNext: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onRegistrationStartPrevious: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.steps.intro;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onRegistrationStartNext: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onRegistrantPrevious: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onRegistrantNext: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onRegistrationEndPrevious: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onRegistrationEndNext: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.steps.reviewAndPayment;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onSummaryPrevious: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.persistSession(false)];
                                    case 1:
                                        _a.sent();
                                        this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
                                        Page_1.default.smoothScrollToTop();
                                        return [2];
                                }
                            });
                        });
                    },
                    onSummaryNext: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                this.registrationEntryState.CurrentStep = this.steps.success;
                                Page_1.default.smoothScrollToTop();
                                return [2];
                            });
                        });
                    }
                },
                watch: {
                    currentPerson: {
                        immediate: true,
                        handler: function () {
                            var forcedFamilyGuid = getForcedFamilyGuid(this.currentPerson, this.viewModel);
                            if (forcedFamilyGuid) {
                                for (var _i = 0, _a = this.registrationEntryState.Registrants; _i < _a.length; _i++) {
                                    var registrant = _a[_i];
                                    registrant.FamilyGuid = forcedFamilyGuid;
                                }
                            }
                        }
                    },
                    'registrationEntryState.SessionExpirationDate': {
                        immediate: true,
                        handler: function () {
                            if (!this.registrationEntryState.SessionExpirationDate) {
                                this.secondsBeforeExpiration = -1;
                                return;
                            }
                            var nowMs = new Date().getTime();
                            var thenMs = this.registrationEntryState.SessionExpirationDate.getTime();
                            var diffMs = thenMs - nowMs;
                            this.secondsBeforeExpiration = diffMs / 1000;
                        }
                    }
                },
                mounted: function () {
                    if (this.viewModel.LoginRequiredToRegister && !this.$store.state.currentPerson) {
                        this.$store.dispatch('redirectToLogin');
                    }
                },
                template: "\n<div>\n    <Alert v-if=\"notFound\" alertType=\"warning\">\n        <strong>Sorry</strong>\n        <p>The selected registration could not be found or is no longer active.</p>\n    </Alert>\n    <Alert v-else-if=\"mustLogin\" alertType=\"warning\">\n        <strong>Please log in</strong>\n        <p>You must be logged in to access this registration.</p>\n    </Alert>\n    <Alert v-else-if=\"isUnauthorized\" alertType=\"warning\">\n        <strong>Sorry</strong>\n        <p>You are not allowed to view or edit the selected registration since you are not the one who created the registration.</p>\n    </Alert>\n    <template v-else>\n        <h1 v-if=\"currentStep !== steps.intro\" v-html=\"stepTitleHtml\"></h1>\n        <ProgressTracker v-if=\"currentStep !== steps.success\" :items=\"progressTrackerItems\" :currentIndex=\"progressTrackerIndex\">\n            <template #aside>\n                <div v-if=\"secondsBeforeExpiration >= 0\" v-show=\"secondsBeforeExpiration <= (30 * 60)\" class=\"remaining-time flex-grow-1 flex-md-grow-0\">\n                    <Alert v-if=\"hasSessionRenewalSuccess\" alertType=\"success\" class=\"m-0 pt-3\" style=\"position: absolute; top: 0; left: 0; right: 0; bottom: 0;\">\n                        <h4>Success</h4>\n                    </Alert>\n                    <span class=\"remaining-time-title\">Time left before timeout</span>\n                    <p class=\"remaining-time-countdown\">\n                        <CountdownTimer v-model=\"secondsBeforeExpiration\" />\n                    </p>\n                </div>\n            </template>\n        </ProgressTracker>\n        <RegistrationEntryIntro v-if=\"currentStep === steps.intro\" @next=\"onIntroNext\" />\n        <RegistrationEntryRegistrationStart v-else-if=\"currentStep === steps.registrationStartForm\" @next=\"onRegistrationStartNext\" @previous=\"onRegistrationStartPrevious\" />\n        <RegistrationEntryRegistrants v-else-if=\"currentStep === steps.perRegistrantForms\" @next=\"onRegistrantNext\" @previous=\"onRegistrantPrevious\" />\n        <RegistrationEntryRegistrationEnd v-else-if=\"currentStep === steps.registrationEndForm\" @next=\"onRegistrationEndNext\" @previous=\"onRegistrationEndPrevious\" />\n        <RegistrationEntrySummary v-else-if=\"currentStep === steps.reviewAndPayment\" @next=\"onSummaryNext\" @previous=\"onSummaryPrevious\" />\n        <RegistrationEntrySuccess v-else-if=\"currentStep === steps.success\" />\n        <Alert v-else alertType=\"danger\">Invalid State: '{{currentStep}}'</Alert>\n    </template>\n    <SessionRenewal :isSessionExpired=\"isSessionExpired\" @success=\"onSessionRenewalSuccess\" />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEntry.js.map