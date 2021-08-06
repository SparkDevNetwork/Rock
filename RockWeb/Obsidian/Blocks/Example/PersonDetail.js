System.register(["../../Util/Bus", "../../Templates/PaneledBlockTemplate", "../../Elements/RockButton", "../../Elements/TextBox", "vue", "../../Store/Index", "../../Elements/EmailBox", "../../Controls/RockValidation", "../../Controls/RockForm", "../../Controls/CampusPicker", "../../Controls/Loading", "../../Controls/PrimaryBlock", "../../Services/Date", "../../Util/RockDate", "../../Elements/DatePicker", "../../Controls/AddressControl"], function (exports_1, context_1) {
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
    var Bus_1, PaneledBlockTemplate_1, RockButton_1, TextBox_1, vue_1, Index_1, EmailBox_1, RockValidation_1, RockForm_1, CampusPicker_1, Loading_1, PrimaryBlock_1, Date_1, RockDate_1, DatePicker_1, AddressControl_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Bus_1_1) {
                Bus_1 = Bus_1_1;
            },
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (EmailBox_1_1) {
                EmailBox_1 = EmailBox_1_1;
            },
            function (RockValidation_1_1) {
                RockValidation_1 = RockValidation_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (CampusPicker_1_1) {
                CampusPicker_1 = CampusPicker_1_1;
            },
            function (Loading_1_1) {
                Loading_1 = Loading_1_1;
            },
            function (PrimaryBlock_1_1) {
                PrimaryBlock_1 = PrimaryBlock_1_1;
            },
            function (Date_1_1) {
                Date_1 = Date_1_1;
            },
            function (RockDate_1_1) {
                RockDate_1 = RockDate_1_1;
            },
            function (DatePicker_1_1) {
                DatePicker_1 = DatePicker_1_1;
            },
            function (AddressControl_1_1) {
                AddressControl_1 = AddressControl_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Example.PersonDetail',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default,
                    RockButton: RockButton_1.default,
                    TextBox: TextBox_1.default,
                    EmailBox: EmailBox_1.default,
                    RockValidation: RockValidation_1.default,
                    RockForm: RockForm_1.default,
                    CampusPicker: CampusPicker_1.default,
                    Loading: Loading_1.default,
                    PrimaryBlock: PrimaryBlock_1.default,
                    DatePicker: DatePicker_1.default,
                    AddressControl: AddressControl_1.default
                },
                setup: function () {
                    return {
                        invokeBlockAction: vue_1.inject('invokeBlockAction')
                    };
                },
                data: function () {
                    return {
                        person: null,
                        personForEditing: null,
                        isEditMode: false,
                        messageToPublish: '',
                        receivedMessage: '',
                        isLoading: false,
                        campusGuid: '',
                        birthdate: null,
                        address: AddressControl_1.getDefaultAddressControlModel()
                    };
                },
                methods: {
                    setIsEditMode: function (isEditMode) {
                        this.isEditMode = isEditMode;
                    },
                    doEdit: function () {
                        var _a;
                        this.personForEditing = this.person ? __assign({}, this.person) : null;
                        this.campusGuid = ((_a = this.campus) === null || _a === void 0 ? void 0 : _a.guid) || '';
                        this.birthdate = this.birthdateOrNull ? RockDate_1.toRockDate(this.birthdateOrNull) : null;
                        this.setIsEditMode(true);
                    },
                    doCancel: function () {
                        this.setIsEditMode(false);
                    },
                    doSave: function () {
                        var _a;
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_b) {
                                switch (_b.label) {
                                    case 0:
                                        if (!this.personForEditing) return [3, 2];
                                        this.person = __assign(__assign({}, this.personForEditing), { BirthDay: RockDate_1.default.getDay(this.birthdate), BirthMonth: RockDate_1.default.getMonth(this.birthdate), BirthYear: RockDate_1.default.getYear(this.birthdate), PrimaryCampusId: ((_a = Index_1.default.getters['campuses/getByGuid'](this.campusGuid)) === null || _a === void 0 ? void 0 : _a.Id) || null });
                                        this.isLoading = true;
                                        return [4, this.invokeBlockAction('EditPerson', {
                                                personArgs: this.person
                                            })];
                                    case 1:
                                        _b.sent();
                                        this.isLoading = false;
                                        _b.label = 2;
                                    case 2:
                                        this.setIsEditMode(false);
                                        return [2];
                                }
                            });
                        });
                    },
                    doPublish: function () {
                        Bus_1.default.publish('PersonDetail:Message', this.messageToPublish);
                        this.messageToPublish = '';
                    },
                    receiveMessage: function (message) {
                        this.receivedMessage = message;
                    }
                },
                computed: {
                    birthdateOrNull: function () {
                        var _a;
                        if (!((_a = this.person) === null || _a === void 0 ? void 0 : _a.BirthDay) || !this.person.BirthMonth || !this.person.BirthYear) {
                            return null;
                        }
                        return new Date(this.person.BirthYear + "-" + this.person.BirthMonth + "-" + this.person.BirthDay);
                    },
                    birthdateFormatted: function () {
                        if (!this.birthdateOrNull) {
                            return 'Not Completed';
                        }
                        return Date_1.asDateString(this.birthdateOrNull);
                    },
                    campus: function () {
                        if (this.person) {
                            return Index_1.default.getters['campuses/getById'](this.person.PrimaryCampusId) || null;
                        }
                        return null;
                    },
                    campusName: function () {
                        var _a;
                        return ((_a = this.campus) === null || _a === void 0 ? void 0 : _a.name) || '';
                    },
                    blockTitle: function () {
                        return this.person ?
                            ": " + (this.person.NickName || this.person.FirstName) + " " + this.person.LastName :
                            '';
                    },
                    currentPerson: function () {
                        return Index_1.default.state.currentPerson;
                    },
                    currentPersonGuid: function () {
                        return this.currentPerson ? this.currentPerson.guid : null;
                    }
                },
                watch: {
                    currentPersonGuid: {
                        immediate: true,
                        handler: function () {
                            return __awaiter(this, void 0, void 0, function () {
                                var _a;
                                return __generator(this, function (_b) {
                                    switch (_b.label) {
                                        case 0:
                                            if (!this.currentPersonGuid) {
                                                this.person = null;
                                                return [2];
                                            }
                                            if (this.person && this.person.Guid === this.currentPersonGuid) {
                                                return [2];
                                            }
                                            this.isLoading = true;
                                            _a = this;
                                            return [4, this.invokeBlockAction('GetPersonViewModel')];
                                        case 1:
                                            _a.person = (_b.sent()).data;
                                            this.isLoading = false;
                                            return [2];
                                    }
                                });
                            });
                        }
                    }
                },
                created: function () {
                    Bus_1.default.subscribe('PersonSecondary:Message', this.receiveMessage);
                },
                template: "\n<PrimaryBlock :hideSecondaryBlocks=\"isEditMode\">\n    <PaneledBlockTemplate>\n        <template v-slot:title>\n            <i class=\"fa fa-flask\"></i>\n            Edit Yourself{{blockTitle}}\n        </template>\n        <template v-slot:default>\n            <Loading :isLoading=\"isLoading\">\n                <p v-if=\"!person\">\n                    There is no person loaded.\n                </p>\n                <RockForm v-else-if=\"isEditMode\" @submit=\"doSave\">\n                    <div class=\"row\">\n                        <div class=\"col-sm-6\">\n                            <TextBox label=\"First Name\" v-model=\"personForEditing.FirstName\" rules=\"required\" />\n                            <TextBox label=\"Nick Name\" v-model=\"personForEditing.NickName\" />\n                            <TextBox label=\"Last Name\" v-model=\"personForEditing.LastName\" rules=\"required\" />\n                        </div>\n                        <div class=\"col-sm-6\">\n                            <EmailBox v-model=\"personForEditing.Email\" />\n                            <CampusPicker v-model=\"campusGuid\" />\n                            <DatePicker label=\"Birthdate\" v-model=\"birthdate\" rules=\"required\" />\n                        </div>\n                        <div class=\"col-sm-12\">\n                            <AddressControl v-model=\"address\" />\n                        </div>\n                    </div>\n                    <div class=\"actions\">\n                        <RockButton btnType=\"primary\" type=\"submit\">Save</RockButton>\n                        <RockButton btnType=\"link\" @click=\"doCancel\">Cancel</RockButton>\n                    </div>\n                </RockForm>\n                <template v-else>\n                    <div class=\"row\">\n                        <div class=\"col-sm-6\">\n                            <dl>\n                                <dt>First Name</dt>\n                                <dd>{{person.FirstName}}</dd>\n                                <dt>Last Name</dt>\n                                <dd>{{person.LastName}}</dd>\n                                <dt>Email</dt>\n                                <dd>{{person.Email}}</dd>\n                                <dt>Campus</dt>\n                                <dd>{{campusName || 'None'}}</dd>\n                                <dt>Birthdate</dt>\n                                <dd>{{birthdateFormatted}}</dd>\n                            </dl>\n                        </div>\n                        <div class=\"col-sm-6\">\n                            <div class=\"well\">\n                                <TextBox label=\"Message\" v-model=\"messageToPublish\" />\n                                <RockButton btnType=\"primary\" btnSize=\"sm\" @click=\"doPublish\">Publish</RockButton>\n                            </div>\n                            <p>\n                                <strong>Secondary block says:</strong>\n                                {{receivedMessage}}\n                            </p>\n                        </div>\n                    </div>\n                    <div class=\"actions\">\n                        <RockButton btnType=\"primary\" @click=\"doEdit\">Edit</RockButton>\n                    </div>\n                </template>\n            </Loading>\n        </template>\n    </PaneledBlockTemplate>\n</PrimaryBlock>"
            }));
        }
    };
});
//# sourceMappingURL=PersonDetail.js.map