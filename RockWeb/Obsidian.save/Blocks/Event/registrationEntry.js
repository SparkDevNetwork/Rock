System.register(["vue", "../../Elements/alert", "../../Elements/countdownTimer", "../../Elements/javaScriptAnchor", "../../Elements/progressTracker", "../../Elements/rockButton", "../../Services/number", "../../Services/string", "../../Store/index", "../../Util/block", "../../Util/guid", "../../Util/linq", "../../Util/page", "../../Util/rockDateTime", "./RegistrationEntry/intro", "./RegistrationEntry/registrants", "./RegistrationEntry/registrationEnd", "./RegistrationEntry/registrationStart", "./RegistrationEntry/sessionRenewal", "./RegistrationEntry/success", "./RegistrationEntry/summary"], function (exports_1, context_1) {
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
    var vue_1, alert_1, countdownTimer_1, javaScriptAnchor_1, progressTracker_1, rockButton_1, number_1, string_1, index_1, block_1, guid_1, linq_1, page_1, rockDateTime_1, intro_1, registrants_1, registrationEnd_1, registrationStart_1, sessionRenewal_1, success_1, summary_1, store, unknownSingleFamilyGuid;
    var __moduleName = context_1 && context_1.id;
    function getForcedFamilyGuid(currentPerson, viewModel) {
        return (currentPerson && viewModel.registrantsSameFamily === 1) ?
            (currentPerson.primaryFamilyGuid || unknownSingleFamilyGuid) :
            null;
    }
    exports_1("getForcedFamilyGuid", getForcedFamilyGuid);
    function getDefaultRegistrantInfo(currentPerson, viewModel, familyGuid) {
        const forcedFamilyGuid = getForcedFamilyGuid(currentPerson, viewModel);
        if (forcedFamilyGuid) {
            familyGuid = forcedFamilyGuid;
        }
        if (!familyGuid) {
            familyGuid = guid_1.newGuid();
        }
        return {
            isOnWaitList: false,
            familyGuid: familyGuid,
            fieldValues: {},
            feeItemQuantities: {},
            guid: guid_1.newGuid(),
            personGuid: ""
        };
    }
    exports_1("getDefaultRegistrantInfo", getDefaultRegistrantInfo);
    function getRegistrantBasicInfo(registrant, registrantForms) {
        var _a, _b, _c;
        const fields = (registrantForms === null || registrantForms === void 0 ? void 0 : registrantForms.reduce((acc, f) => acc.concat(f.fields), [])) || [];
        const firstNameGuid = ((_a = fields.find(f => f.personFieldType === 0)) === null || _a === void 0 ? void 0 : _a.guid) || "";
        const lastNameGuid = ((_b = fields.find(f => f.personFieldType === 1)) === null || _b === void 0 ? void 0 : _b.guid) || "";
        const emailGuid = ((_c = fields.find(f => f.personFieldType === 4)) === null || _c === void 0 ? void 0 : _c.guid) || "";
        return {
            firstName: ((registrant === null || registrant === void 0 ? void 0 : registrant.fieldValues[firstNameGuid]) || ""),
            lastName: ((registrant === null || registrant === void 0 ? void 0 : registrant.fieldValues[lastNameGuid]) || ""),
            email: ((registrant === null || registrant === void 0 ? void 0 : registrant.fieldValues[emailGuid]) || ""),
            guid: registrant === null || registrant === void 0 ? void 0 : registrant.guid
        };
    }
    exports_1("getRegistrantBasicInfo", getRegistrantBasicInfo);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (countdownTimer_1_1) {
                countdownTimer_1 = countdownTimer_1_1;
            },
            function (javaScriptAnchor_1_1) {
                javaScriptAnchor_1 = javaScriptAnchor_1_1;
            },
            function (progressTracker_1_1) {
                progressTracker_1 = progressTracker_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (block_1_1) {
                block_1 = block_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (linq_1_1) {
                linq_1 = linq_1_1;
            },
            function (page_1_1) {
                page_1 = page_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            },
            function (intro_1_1) {
                intro_1 = intro_1_1;
            },
            function (registrants_1_1) {
                registrants_1 = registrants_1_1;
            },
            function (registrationEnd_1_1) {
                registrationEnd_1 = registrationEnd_1_1;
            },
            function (registrationStart_1_1) {
                registrationStart_1 = registrationStart_1_1;
            },
            function (sessionRenewal_1_1) {
                sessionRenewal_1 = sessionRenewal_1_1;
            },
            function (success_1_1) {
                success_1 = success_1_1;
            },
            function (summary_1_1) {
                summary_1 = summary_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            unknownSingleFamilyGuid = guid_1.newGuid();
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry",
                components: {
                    RockButton: rockButton_1.default,
                    Registrants: registrants_1.default,
                    RegistrationEntryIntro: intro_1.default,
                    RegistrationEntryRegistrants: registrants_1.default,
                    RegistrationEntryRegistrationStart: registrationStart_1.default,
                    RegistrationEntryRegistrationEnd: registrationEnd_1.default,
                    RegistrationEntrySummary: summary_1.default,
                    RegistrationEntrySuccess: success_1.default,
                    ProgressTracker: progressTracker_1.default,
                    Alert: alert_1.default,
                    CountdownTimer: countdownTimer_1.default,
                    JavaScriptAnchor: javaScriptAnchor_1.default,
                    SessionRenewal: sessionRenewal_1.default
                },
                setup() {
                    var _a, _b, _c, _d, _e, _f, _g, _h, _j;
                    const steps = {
                        ["intro"]: "intro",
                        ["registrationStartForm"]: "registrationStartForm",
                        ["perRegistrantForms"]: "perRegistrantForms",
                        ["registrationEndForm"]: "registrationEndForm",
                        ["reviewAndPayment"]: "reviewAndPayment",
                        ["success"]: "success"
                    };
                    const notFound = vue_1.ref(false);
                    const viewModel = block_1.useConfigurationValues();
                    const invokeBlockAction = block_1.useInvokeBlockAction();
                    if (viewModel === null) {
                        notFound.value = true;
                        return {
                            viewModel,
                            notFound
                        };
                    }
                    if (!viewModel.registrationAttributesStart) {
                        notFound.value = true;
                    }
                    const hasPreAttributes = ((_a = viewModel.registrationAttributesStart) === null || _a === void 0 ? void 0 : _a.length) > 0;
                    let currentStep = steps.intro;
                    if (viewModel.successViewModel) {
                        currentStep = steps.success;
                    }
                    else if (viewModel.session && !viewModel.startAtBeginning) {
                        currentStep = steps.reviewAndPayment;
                    }
                    else if (viewModel.maxRegistrants === 1 && string_1.isNullOrWhiteSpace(viewModel.instructionsHtml)) {
                        currentStep = hasPreAttributes ? steps.registrationStartForm : steps.perRegistrantForms;
                    }
                    const staticRegistrationEntryState = {
                        steps: steps,
                        viewModel: viewModel,
                        firstStep: currentStep,
                        currentStep: currentStep,
                        currentRegistrantFormIndex: 0,
                        currentRegistrantIndex: 0,
                        registrants: ((_b = viewModel.session) === null || _b === void 0 ? void 0 : _b.registrants) || [getDefaultRegistrantInfo(null, viewModel, null)],
                        registrationFieldValues: ((_c = viewModel.session) === null || _c === void 0 ? void 0 : _c.fieldValues) || {},
                        registrar: ((_d = viewModel.session) === null || _d === void 0 ? void 0 : _d.registrar) || {
                            nickName: "",
                            lastName: "",
                            email: "",
                            updateEmail: true,
                            familyGuid: null
                        },
                        gatewayToken: "",
                        savedAccountGuid: null,
                        discountCode: ((_e = viewModel.session) === null || _e === void 0 ? void 0 : _e.discountCode) || "",
                        discountAmount: ((_f = viewModel.session) === null || _f === void 0 ? void 0 : _f.discountAmount) || 0,
                        discountPercentage: ((_g = viewModel.session) === null || _g === void 0 ? void 0 : _g.discountPercentage) || 0,
                        successViewModel: viewModel.successViewModel,
                        amountToPayToday: 0,
                        sessionExpirationDateMs: null,
                        registrationSessionGuid: ((_h = viewModel.session) === null || _h === void 0 ? void 0 : _h.registrationSessionGuid) || guid_1.newGuid(),
                        ownFamilyGuid: ((_j = store.state.currentPerson) === null || _j === void 0 ? void 0 : _j.primaryFamilyGuid) || guid_1.newGuid()
                    };
                    const registrationEntryState = vue_1.reactive(staticRegistrationEntryState);
                    vue_1.provide("registrationEntryState", registrationEntryState);
                    const getRegistrationEntryBlockArgs = () => {
                        var _a;
                        return {
                            registrationSessionGuid: registrationEntryState.registrationSessionGuid,
                            gatewayToken: registrationEntryState.gatewayToken,
                            savedAccountGuid: registrationEntryState.savedAccountGuid,
                            discountCode: registrationEntryState.discountCode,
                            fieldValues: registrationEntryState.registrationFieldValues,
                            registrar: registrationEntryState.registrar,
                            registrants: registrationEntryState.registrants,
                            amountToPayNow: registrationEntryState.amountToPayToday,
                            registrationGuid: ((_a = viewModel.session) === null || _a === void 0 ? void 0 : _a.registrationGuid) || null
                        };
                    };
                    vue_1.provide("getRegistrationEntryBlockArgs", getRegistrationEntryBlockArgs);
                    const persistSession = (force = false) => __awaiter(this, void 0, void 0, function* () {
                        var _k;
                        if (!force && !viewModel.timeoutMinutes) {
                            return;
                        }
                        const response = yield invokeBlockAction("PersistSession", {
                            args: getRegistrationEntryBlockArgs()
                        });
                        if (response.data) {
                            const expirationDate = rockDateTime_1.RockDateTime.parseISO(response.data.expirationDateTime);
                            registrationEntryState.sessionExpirationDateMs = (_k = expirationDate === null || expirationDate === void 0 ? void 0 : expirationDate.toMilliseconds()) !== null && _k !== void 0 ? _k : null;
                        }
                    });
                    vue_1.provide("persistSession", persistSession);
                    return {
                        viewModel,
                        steps,
                        registrationEntryState,
                        notFound,
                        persistSession
                    };
                },
                data() {
                    return {
                        secondsBeforeExpiration: -1,
                        hasSessionRenewalSuccess: false
                    };
                },
                computed: {
                    currentPerson() {
                        return store.state.currentPerson;
                    },
                    isSessionExpired() {
                        return this.secondsBeforeExpiration === 0 && this.currentStep !== "success";
                    },
                    mustLogin() {
                        return !store.state.currentPerson && this.viewModel != null && (this.viewModel.isUnauthorized || this.viewModel.loginRequiredToRegister);
                    },
                    isUnauthorized() {
                        var _a, _b;
                        return (_b = (_a = this.viewModel) === null || _a === void 0 ? void 0 : _a.isUnauthorized) !== null && _b !== void 0 ? _b : false;
                    },
                    currentStep() {
                        var _a, _b;
                        return (_b = (_a = this.registrationEntryState) === null || _a === void 0 ? void 0 : _a.currentStep) !== null && _b !== void 0 ? _b : "";
                    },
                    registrants() {
                        var _a, _b;
                        return (_b = (_a = this.registrationEntryState) === null || _a === void 0 ? void 0 : _a.registrants) !== null && _b !== void 0 ? _b : [];
                    },
                    hasPreAttributes() {
                        var _a, _b, _c;
                        return ((_c = (_b = (_a = this.viewModel) === null || _a === void 0 ? void 0 : _a.registrationAttributesStart) === null || _b === void 0 ? void 0 : _b.length) !== null && _c !== void 0 ? _c : 0) > 0;
                    },
                    hasPostAttributes() {
                        var _a, _b, _c;
                        return ((_c = (_b = (_a = this.viewModel) === null || _a === void 0 ? void 0 : _a.registrationAttributesEnd) === null || _b === void 0 ? void 0 : _b.length) !== null && _c !== void 0 ? _c : 0) > 0;
                    },
                    progressTrackerIndex() {
                        var _a;
                        if (this.currentStep === "intro" || this.registrationEntryState == null) {
                            return 0;
                        }
                        const stepsBeforePre = ((_a = this.registrationEntryState) === null || _a === void 0 ? void 0 : _a.firstStep) === "intro" ? 1 : 0;
                        if (this.currentStep === "registrationStartForm") {
                            return stepsBeforePre;
                        }
                        const stepsBeforeRegistrants = stepsBeforePre + (this.hasPreAttributes ? 1 : 0);
                        if (this.currentStep === "perRegistrantForms") {
                            return this.registrationEntryState.currentRegistrantIndex + stepsBeforeRegistrants;
                        }
                        const stepsToCompleteRegistrants = this.registrationEntryState.registrants.length + stepsBeforeRegistrants;
                        if (this.currentStep === "registrationEndForm") {
                            return stepsToCompleteRegistrants;
                        }
                        if (this.currentStep === "reviewAndPayment") {
                            return stepsToCompleteRegistrants + (this.hasPostAttributes ? 1 : 0);
                        }
                        return 0;
                    },
                    uppercaseRegistrantTerm() {
                        var _a, _b;
                        return string_1.default.toTitleCase((_b = (_a = this.viewModel) === null || _a === void 0 ? void 0 : _a.registrantTerm) !== null && _b !== void 0 ? _b : "");
                    },
                    currentRegistrantTitle() {
                        if (this.registrationEntryState == null) {
                            return "";
                        }
                        const ordinal = number_1.default.toOrdinal(this.registrationEntryState.currentRegistrantIndex + 1);
                        let title = string_1.default.toTitleCase(this.registrants.length <= 1 ?
                            this.uppercaseRegistrantTerm :
                            ordinal + " " + this.uppercaseRegistrantTerm);
                        if (this.registrationEntryState.currentRegistrantFormIndex > 0) {
                            title += " (cont)";
                        }
                        return title;
                    },
                    stepTitleHtml() {
                        var _a, _b, _c, _d, _e, _f;
                        if (this.currentStep === "registrationStartForm") {
                            return (_b = (_a = this.viewModel) === null || _a === void 0 ? void 0 : _a.registrationAttributeTitleStart) !== null && _b !== void 0 ? _b : "";
                        }
                        if (this.currentStep === "perRegistrantForms") {
                            return this.currentRegistrantTitle;
                        }
                        if (this.currentStep === "registrationEndForm") {
                            return (_d = (_c = this.viewModel) === null || _c === void 0 ? void 0 : _c.registrationAttributeTitleEnd) !== null && _d !== void 0 ? _d : "";
                        }
                        if (this.currentStep === "reviewAndPayment") {
                            return "Review Registration";
                        }
                        if (this.currentStep === "success") {
                            return ((_f = (_e = this.registrationEntryState) === null || _e === void 0 ? void 0 : _e.successViewModel) === null || _f === void 0 ? void 0 : _f.titleHtml) || "Congratulations";
                        }
                        return "";
                    },
                    progressTrackerItems() {
                        const items = [];
                        if (this.registrationEntryState == null) {
                            return items;
                        }
                        if (this.registrationEntryState.firstStep === "intro") {
                            items.push({
                                key: "Start",
                                title: "Start",
                                subtitle: this.viewModel.registrationTerm
                            });
                        }
                        if (this.hasPreAttributes) {
                            items.push({
                                key: "Pre",
                                title: this.viewModel.registrationAttributeTitleStart,
                                subtitle: this.viewModel.registrationTerm
                            });
                        }
                        if (!this.registrationEntryState.registrants.length) {
                            items.push({
                                key: "Registrant",
                                title: string_1.toTitleCase(this.viewModel.registrantTerm),
                                subtitle: this.viewModel.registrationTerm
                            });
                        }
                        for (let i = 0; i < this.registrationEntryState.registrants.length; i++) {
                            const registrant = this.registrationEntryState.registrants[i];
                            const info = getRegistrantBasicInfo(registrant, this.viewModel.registrantForms);
                            if ((info === null || info === void 0 ? void 0 : info.firstName) && (info === null || info === void 0 ? void 0 : info.lastName)) {
                                items.push({
                                    key: `Registrant-${registrant.guid}`,
                                    title: info.firstName,
                                    subtitle: info.lastName
                                });
                            }
                            else {
                                items.push({
                                    key: `Registrant-${registrant.guid}`,
                                    title: string_1.toTitleCase(this.viewModel.registrantTerm),
                                    subtitle: string_1.toTitleCase(number_1.toWord(i + 1))
                                });
                            }
                        }
                        if (this.hasPostAttributes) {
                            items.push({
                                key: "Post",
                                title: this.viewModel.registrationAttributeTitleEnd,
                                subtitle: this.viewModel.registrationTerm
                            });
                        }
                        items.push({
                            key: "Finalize",
                            title: "Finalize",
                            subtitle: this.viewModel.registrationTerm
                        });
                        return items;
                    },
                    isInvalidGateway() {
                        if (!this.viewModel) {
                            return false;
                        }
                        const hasCostFees = new linq_1.List(this.viewModel.fees)
                            .any(f => new linq_1.List(f.items).any(i => i.cost > 0));
                        if (this.viewModel.cost <= 0 && !hasCostFees) {
                            return false;
                        }
                        if (this.viewModel.isRedirectGateway || this.viewModel.gatewayControl.fileUrl) {
                            return false;
                        }
                        return true;
                    }
                },
                methods: {
                    onSessionRenewalSuccess() {
                        this.hasSessionRenewalSuccess = true;
                        setTimeout(() => this.hasSessionRenewalSuccess = false, 5000);
                    },
                    onIntroNext() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = this.hasPreAttributes ? "registrationStartForm" : "perRegistrantForms";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onRegistrationStartPrevious() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = "intro";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onRegistrationStartNext() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = "perRegistrantForms";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onRegistrantPrevious() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = this.hasPreAttributes ? "registrationStartForm" : "intro";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onRegistrantNext() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = this.hasPostAttributes ? "registrationEndForm" : "reviewAndPayment";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onRegistrationEndPrevious() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = "perRegistrantForms";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onRegistrationEndNext() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = "reviewAndPayment";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onSummaryPrevious() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                yield this.persistSession(false);
                                this.registrationEntryState.currentStep = this.hasPostAttributes ? "registrationEndForm" : "perRegistrantForms";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    },
                    onSummaryNext() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.persistSession && this.registrationEntryState) {
                                this.registrationEntryState.currentStep = "success";
                                page_1.default.smoothScrollToTop();
                            }
                        });
                    }
                },
                watch: {
                    currentPerson: {
                        immediate: true,
                        handler() {
                            if (this.viewModel != null && this.registrationEntryState != null) {
                                const forcedFamilyGuid = getForcedFamilyGuid(this.currentPerson, this.viewModel);
                                if (forcedFamilyGuid) {
                                    for (const registrant of this.registrationEntryState.registrants) {
                                        registrant.familyGuid = forcedFamilyGuid;
                                    }
                                }
                            }
                        }
                    },
                    "registrationEntryState.sessionExpirationDateMs": {
                        immediate: true,
                        handler() {
                            var _a;
                            if (!((_a = this.registrationEntryState) === null || _a === void 0 ? void 0 : _a.sessionExpirationDateMs)) {
                                this.secondsBeforeExpiration = -1;
                                return;
                            }
                            const nowMs = rockDateTime_1.RockDateTime.now().toMilliseconds();
                            const thenMs = this.registrationEntryState.sessionExpirationDateMs;
                            const diffMs = thenMs - nowMs;
                            this.secondsBeforeExpiration = diffMs / 1000;
                        }
                    }
                },
                mounted() {
                    var _a;
                    if (((_a = this.viewModel) === null || _a === void 0 ? void 0 : _a.loginRequiredToRegister) && !store.state.currentPerson) {
                        store.redirectToLogin();
                    }
                },
                template: `
<div>
    <Alert v-if="notFound" alertType="warning">
        <strong>Sorry</strong>
        <p>The selected registration could not be found or is no longer active.</p>
    </Alert>
    <Alert v-else-if="mustLogin" alertType="warning">
        <strong>Please log in</strong>
        <p>You must be logged in to access this registration.</p>
    </Alert>
    <Alert v-else-if="isUnauthorized" alertType="warning">
        <strong>Sorry</strong>
        <p>You are not allowed to view or edit the selected registration since you are not the one who created the registration.</p>
    </Alert>
    <Alert v-else-if="isInvalidGateway" alertType="warning">
        <strong>Incorrect Configuration</strong>
        <p>This registration has costs/fees associated with it but the configured payment gateway is not supported.</p>
    </Alert>
    <template v-else>
        <h1 v-if="currentStep !== steps.intro" v-html="stepTitleHtml"></h1>
        <ProgressTracker v-if="currentStep !== steps.success" :items="progressTrackerItems" :currentIndex="progressTrackerIndex">
            <template #aside>
                <div v-if="secondsBeforeExpiration >= 0" v-show="secondsBeforeExpiration <= (30 * 60)" class="remaining-time flex-grow-1 flex-md-grow-0">
                    <Alert v-if="hasSessionRenewalSuccess" alertType="success" class="m-0 pt-3" style="position: absolute; top: 0; left: 0; right: 0; bottom: 0;">
                        <h4>Success</h4>
                    </Alert>
                    <span class="remaining-time-title">Time left before timeout</span>
                    <p class="remaining-time-countdown">
                        <CountdownTimer v-model="secondsBeforeExpiration" />
                    </p>
                </div>
            </template>
        </ProgressTracker>
        <RegistrationEntryIntro v-if="currentStep === steps.intro" @next="onIntroNext" />
        <RegistrationEntryRegistrationStart v-else-if="currentStep === steps.registrationStartForm" @next="onRegistrationStartNext" @previous="onRegistrationStartPrevious" />
        <RegistrationEntryRegistrants v-else-if="currentStep === steps.perRegistrantForms" @next="onRegistrantNext" @previous="onRegistrantPrevious" />
        <RegistrationEntryRegistrationEnd v-else-if="currentStep === steps.registrationEndForm" @next="onRegistrationEndNext" @previous="onRegistrationEndPrevious" />
        <RegistrationEntrySummary v-else-if="currentStep === steps.reviewAndPayment" @next="onSummaryNext" @previous="onSummaryPrevious" />
        <RegistrationEntrySuccess v-else-if="currentStep === steps.success" />
        <Alert v-else alertType="danger">Invalid State: '{{currentStep}}'</Alert>
    </template>
    <SessionRenewal :isSessionExpired="isSessionExpired" @success="onSessionRenewalSuccess" />
</div>`
            }));
        }
    };
});
//# sourceMappingURL=registrationEntry.js.map