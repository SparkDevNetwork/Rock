System.register(["vue", "./Registrant", "../../../Elements/Alert"], function (exports_1, context_1) {
    "use strict";
    var __assign = (this && this.__assign) || function () {
        __assign = Object.assign || function(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                    t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };
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
    var vue_1, Registrant_1, Alert_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Registrant_1_1) {
                Registrant_1 = Registrant_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Registrants',
                components: {
                    Registrant: Registrant_1.default,
                    Alert: Alert_1.default
                },
                setup: function () {
                    return {
                        registrationEntryState: vue_1.inject('registrationEntryState'),
                        persistSession: vue_1.inject('persistSession')
                    };
                },
                data: function () {
                    return {
                        hasCopiedCommonValues: false
                    };
                },
                methods: {
                    onPrevious: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var lastFormIndex;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        if (this.registrationEntryState.CurrentRegistrantIndex <= 0) {
                                            this.$emit('previous');
                                            return [2];
                                        }
                                        lastFormIndex = this.registrationEntryState.ViewModel.RegistrantForms.length - 1;
                                        this.registrationEntryState.CurrentRegistrantIndex--;
                                        this.registrationEntryState.CurrentRegistrantFormIndex = lastFormIndex;
                                        return [4, this.persistSession()];
                                    case 1:
                                        _a.sent();
                                        return [2];
                                }
                            });
                        });
                    },
                    onNext: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var lastIndex;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        lastIndex = this.registrationEntryState.Registrants.length - 1;
                                        if (this.registrationEntryState.CurrentRegistrantIndex >= lastIndex) {
                                            this.$emit('next');
                                            return [2];
                                        }
                                        if (this.registrationEntryState.CurrentRegistrantIndex === 0) {
                                            this.copyCommonValuesFromFirstRegistrant();
                                        }
                                        this.registrationEntryState.CurrentRegistrantIndex++;
                                        this.registrationEntryState.CurrentRegistrantFormIndex = 0;
                                        return [4, this.persistSession()];
                                    case 1:
                                        _a.sent();
                                        return [2];
                                }
                            });
                        });
                    },
                    copyCommonValuesFromFirstRegistrant: function () {
                        if (this.hasCopiedCommonValues) {
                            return;
                        }
                        this.hasCopiedCommonValues = true;
                        var firstRegistrant = this.registrants[0];
                        for (var i = 1; i < this.registrants.length; i++) {
                            var currentRegistrant = this.registrants[i];
                            for (var _i = 0, _a = this.registrationEntryState.ViewModel.RegistrantForms; _i < _a.length; _i++) {
                                var form = _a[_i];
                                for (var _b = 0, _c = form.Fields; _b < _c.length; _b++) {
                                    var field = _c[_b];
                                    if (!field.IsSharedValue) {
                                        continue;
                                    }
                                    var valueToShare = firstRegistrant.FieldValues[field.Guid];
                                    if (valueToShare && typeof valueToShare === 'object') {
                                        currentRegistrant.FieldValues[field.Guid] = __assign({}, valueToShare);
                                    }
                                    else {
                                        currentRegistrant.FieldValues[field.Guid] = valueToShare;
                                    }
                                }
                            }
                        }
                    }
                },
                computed: {
                    hasWaitlist: function () {
                        return this.registrationEntryState.Registrants.some(function (r) { return r.IsOnWaitList; });
                    },
                    isOnWaitlist: function () {
                        var currentRegistrant = this.registrationEntryState.Registrants[this.registrationEntryState.CurrentRegistrantIndex];
                        return currentRegistrant.IsOnWaitList;
                    },
                    registrantTerm: function () {
                        return (this.registrationEntryState.ViewModel.RegistrantTerm || 'registrant').toLowerCase();
                    },
                    registrants: function () {
                        return this.registrationEntryState.Registrants;
                    },
                    currentRegistrantIndex: function () {
                        return this.registrationEntryState.CurrentRegistrantIndex;
                    }
                },
                template: "\n<div class=\"registrationentry-registrant\">\n    <Alert v-if=\"hasWaitlist && !isOnWaitlist\" alertType=\"success\">\n        This {{registrantTerm}} will be fully registered.\n    </Alert>\n    <Alert v-else-if=\"isOnWaitlist\" alertType=\"warning\">\n        This {{registrantTerm}} will be on the waiting list.\n    </Alert>\n    <template v-for=\"(r, i) in registrants\" :key=\"r.Guid\">\n        <Registrant v-show=\"currentRegistrantIndex === i\" :currentRegistrant=\"r\" :isWaitList=\"isOnWaitlist\" @next=\"onNext\" @previous=\"onPrevious\" />\n    </template>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Registrants.js.map