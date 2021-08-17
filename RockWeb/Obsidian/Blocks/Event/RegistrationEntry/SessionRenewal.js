System.register(["vue", "../../../Controls/Dialog", "../../../Elements/LoadingIndicator", "../../../Elements/RockButton", "../../../Services/Number", "../../../Services/String"], function (exports_1, context_1) {
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
    var vue_1, Dialog_1, LoadingIndicator_1, RockButton_1, Number_1, String_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Dialog_1_1) {
                Dialog_1 = Dialog_1_1;
            },
            function (LoadingIndicator_1_1) {
                LoadingIndicator_1 = LoadingIndicator_1_1;
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
                name: 'Event.RegistrationEntry.SessionRenewal',
                components: {
                    Dialog: Dialog_1.default,
                    LoadingIndicator: LoadingIndicator_1.default,
                    RockButton: RockButton_1.default
                },
                props: {
                    isSessionExpired: {
                        type: Boolean,
                        required: true
                    }
                },
                setup: function () {
                    return {
                        registrationEntryState: vue_1.inject('registrationEntryState'),
                        invokeBlockAction: vue_1.inject('invokeBlockAction')
                    };
                },
                data: function () {
                    return {
                        spotsSecured: null,
                        isLoading: false,
                        isModalVisible: false
                    };
                },
                computed: {
                    hasWaitlist: function () {
                        return this.registrationEntryState.ViewModel.waitListEnabled;
                    },
                    allRegistrantCount: function () {
                        return this.registrationEntryState.Registrants.length;
                    },
                    waitlistRegistrantCount: function () {
                        return this.registrationEntryState.Registrants.filter(function (r) { return r.IsOnWaitList; }).length;
                    },
                    waitlistRegistrantCountWord: function () {
                        return Number_1.toWord(this.waitlistRegistrantCount);
                    },
                    nonWaitlistRegistrantCount: function () {
                        return this.registrationEntryState.Registrants.filter(function (r) { return !r.IsOnWaitList; }).length;
                    },
                    nonWaitlistRegistrantCountWord: function () {
                        return Number_1.toWord(this.nonWaitlistRegistrantCount);
                    }
                },
                methods: {
                    pluralConditional: String_1.pluralConditional,
                    restart: function () {
                        this.isLoading = true;
                        location.reload();
                    },
                    close: function () {
                        var _this = this;
                        this.isModalVisible = false;
                        this.$nextTick(function () {
                            _this.spotsSecured = null;
                            _this.isLoading = false;
                        });
                    },
                    requestRenewal: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var response, asDate, deficiency, i, registrant;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        this.spotsSecured = 0;
                                        this.isLoading = true;
                                        _a.label = 1;
                                    case 1:
                                        _a.trys.push([1, , 3, 4]);
                                        return [4, this.invokeBlockAction('TryToRenewSession', {
                                                registrationSessionGuid: this.registrationEntryState.RegistrationSessionGuid
                                            })];
                                    case 2:
                                        response = _a.sent();
                                        if (response.data) {
                                            asDate = new Date(response.data.expirationDateTime);
                                            this.registrationEntryState.SessionExpirationDate = asDate;
                                            this.spotsSecured = response.data.spotsSecured;
                                            deficiency = this.nonWaitlistRegistrantCount - this.spotsSecured;
                                            if (!deficiency) {
                                                this.$emit('success');
                                                this.close();
                                                return [2];
                                            }
                                            this.registrationEntryState.ViewModel.spotsRemaining = this.spotsSecured;
                                            if (!this.hasWaitlist) {
                                                this.registrationEntryState.Registrants.length = this.spotsSecured;
                                                return [2];
                                            }
                                            for (i = this.allRegistrantCount - 1; i >= 0; i--) {
                                                if (!deficiency) {
                                                    break;
                                                }
                                                registrant = this.registrationEntryState.Registrants[i];
                                                if (registrant.IsOnWaitList) {
                                                    continue;
                                                }
                                                registrant.IsOnWaitList = true;
                                                deficiency--;
                                            }
                                        }
                                        return [3, 4];
                                    case 3:
                                        this.isLoading = false;
                                        return [7];
                                    case 4: return [2];
                                }
                            });
                        });
                    }
                },
                watch: {
                    isSessionExpired: function () {
                        if (this.isSessionExpired) {
                            this.spotsSecured = null;
                            this.isModalVisible = true;
                        }
                    }
                },
                template: "\n<Dialog :modelValue=\"isModalVisible\" :dismissible=\"false\">\n    <template #header>\n        <h4 v-if=\"isLoading || spotsSecured === null\">Registration Timed Out</h4>\n        <h4 v-else>Request Extension</h4>\n    </template>\n    <template #default>\n        <LoadingIndicator v-if=\"isLoading\" />\n        <template v-else-if=\"hasWaitlist && spotsSecured === 0\">\n            Due to high demand there is no longer space available.\n            You can continue, but your registrants will be placed on the waitlist.\n            Do you wish to continue with the registration?\n        </template>\n        <template v-else-if=\"spotsSecured === 0\">\n            Due to high demand there is no longer space available for this registration.\n        </template>\n        <template v-else-if=\"hasWaitlist && spotsSecured !== null\">\n            Due to high demand there is no longer space available for all your registrants.\n            Your last {{waitlistRegistrantCountWord}}\n            {{pluralConditional(waitlistRegistrantCount, 'registrant', ' registrants')}}\n            will be placed on the waitlist.\n            Do you wish to continue with the registration?\n        </template>\n        <template v-else-if=\"spotsSecured !== null\">\n            Due to high demand there is no longer space available for all your registrants.\n            Only {{nonWaitlistRegistrantCountWord}} {{pluralConditional(nonWaitlistRegistrantCount, 'spot is', 'spots are')}} available.\n            Your registration has been updated to only allow\n            {{nonWaitlistRegistrantCountWord}} {{pluralConditional(nonWaitlistRegistrantCount, 'registrant', 'registrants')}}. \n            Do you wish to continue with the registration?\n        </template>\n        <template v-else>\n            Your registration has timed out. Do you wish to request an extension in time?\n        </template>\n    </template>\n    <template v-if=\"!isLoading\" #footer>\n        <template v-if=\"!hasWaitlist && spotsSecured === 0\">\n            <RockButton btnType=\"link\" @click=\"restart\">Close</RockButton>\n        </template>\n        <template v-else-if=\"spotsSecured !== null\">\n            <RockButton btnType=\"primary\" @click=\"close\">Yes</RockButton>\n            <RockButton btnType=\"link\" @click=\"restart\">No, cancel registration</RockButton>\n        </template>\n        <template v-else>\n            <RockButton btnType=\"primary\" @click=\"requestRenewal\">Yes</RockButton>\n            <RockButton btnType=\"link\" @click=\"restart\">No, cancel registration</RockButton>\n        </template>\n    </template>\n</Dialog>"
            }));
        }
    };
});
//# sourceMappingURL=SessionRenewal.js.map