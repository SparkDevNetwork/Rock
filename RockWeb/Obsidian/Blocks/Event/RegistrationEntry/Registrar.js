System.register(["vue", "../../../Elements/CheckBox", "../../../Elements/EmailBox", "../../../Elements/RadioButtonList", "../../../Elements/StaticFormControl", "../../../Elements/TextBox", "../RegistrationEntry", "./RegistrationEntryBlockViewModel"], function (exports_1, context_1) {
    "use strict";
    var vue_1, CheckBox_1, EmailBox_1, RadioButtonList_1, StaticFormControl_1, TextBox_1, RegistrationEntry_1, RegistrationEntryBlockViewModel_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (CheckBox_1_1) {
                CheckBox_1 = CheckBox_1_1;
            },
            function (EmailBox_1_1) {
                EmailBox_1 = EmailBox_1_1;
            },
            function (RadioButtonList_1_1) {
                RadioButtonList_1 = RadioButtonList_1_1;
            },
            function (StaticFormControl_1_1) {
                StaticFormControl_1 = StaticFormControl_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (RegistrationEntry_1_1) {
                RegistrationEntry_1 = RegistrationEntry_1_1;
            },
            function (RegistrationEntryBlockViewModel_1_1) {
                RegistrationEntryBlockViewModel_1 = RegistrationEntryBlockViewModel_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Registrar',
                components: {
                    TextBox: TextBox_1.default,
                    CheckBox: CheckBox_1.default,
                    EmailBox: EmailBox_1.default,
                    StaticFormControl: StaticFormControl_1.default,
                    RadioButtonList: RadioButtonList_1.default
                },
                setup: function () {
                    return {
                        getRegistrationEntryBlockArgs: vue_1.inject('getRegistrationEntryBlockArgs'),
                        registrationEntryState: vue_1.inject('registrationEntryState')
                    };
                },
                data: function () {
                    return {
                        isRegistrarPanelShown: true
                    };
                },
                computed: {
                    useLoggedInPersonForRegistrar: function () {
                        return (!!this.currentPerson) && this.viewModel.RegistrarOption === RegistrationEntryBlockViewModel_1.RegistrarOption.UseLoggedInPerson;
                    },
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    registrar: function () {
                        return this.registrationEntryState.Registrar;
                    },
                    firstRegistrant: function () {
                        return this.registrationEntryState.Registrants[0];
                    },
                    viewModel: function () {
                        return this.registrationEntryState.ViewModel;
                    },
                    doShowUpdateEmailOption: function () {
                        var _a;
                        return !this.viewModel.ForceEmailUpdate && !!((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.Email);
                    },
                    registrantInfos: function () {
                        var _this = this;
                        return this.registrationEntryState.Registrants.map(function (r) { return RegistrationEntry_1.getRegistrantBasicInfo(r, _this.viewModel.RegistrantForms); });
                    },
                    registrantTerm: function () {
                        return this.registrantInfos.length === 1 ? this.viewModel.RegistrantTerm : this.viewModel.PluralRegistrantTerm;
                    },
                    instanceName: function () {
                        return this.viewModel.InstanceName;
                    },
                    familyOptions: function () {
                        var _a;
                        var options = [];
                        var usedFamilyGuids = {};
                        if (this.viewModel.RegistrantsSameFamily !== RegistrationEntryBlockViewModel_1.RegistrantsSameFamily.Ask) {
                            return options;
                        }
                        for (var i = 0; i < this.registrationEntryState.Registrants.length; i++) {
                            var registrant = this.registrationEntryState.Registrants[i];
                            var info = RegistrationEntry_1.getRegistrantBasicInfo(registrant, this.viewModel.RegistrantForms);
                            if (!usedFamilyGuids[registrant.FamilyGuid] && (info === null || info === void 0 ? void 0 : info.FirstName) && (info === null || info === void 0 ? void 0 : info.LastName)) {
                                options.push({
                                    key: registrant.FamilyGuid,
                                    text: info.FirstName + " " + info.LastName,
                                    value: registrant.FamilyGuid
                                });
                                usedFamilyGuids[registrant.FamilyGuid] = true;
                            }
                        }
                        if (((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.PrimaryFamilyGuid) && this.currentPerson.FullName && !usedFamilyGuids[this.currentPerson.PrimaryFamilyGuid]) {
                            options.push({
                                key: this.currentPerson.PrimaryFamilyGuid,
                                text: this.currentPerson.FullName,
                                value: this.currentPerson.PrimaryFamilyGuid
                            });
                        }
                        options.push({
                            key: this.registrar.OwnFamilyGuid,
                            text: 'None of the above',
                            value: this.registrar.OwnFamilyGuid
                        });
                        return options;
                    },
                },
                methods: {
                    prefillRegistrar: function () {
                        this.isRegistrarPanelShown = true;
                        if (this.currentPerson &&
                            (this.viewModel.RegistrarOption === RegistrationEntryBlockViewModel_1.RegistrarOption.UseLoggedInPerson || this.viewModel.RegistrarOption === RegistrationEntryBlockViewModel_1.RegistrarOption.PromptForRegistrar)) {
                            this.registrar.NickName = this.currentPerson.NickName || this.currentPerson.FirstName || '';
                            this.registrar.LastName = this.currentPerson.LastName || '';
                            this.registrar.Email = this.currentPerson.Email || '';
                            this.registrar.FamilyGuid = this.currentPerson.PrimaryFamilyGuid;
                            return;
                        }
                        if (this.viewModel.RegistrarOption === RegistrationEntryBlockViewModel_1.RegistrarOption.PromptForRegistrar) {
                            return;
                        }
                        if (this.viewModel.RegistrarOption === RegistrationEntryBlockViewModel_1.RegistrarOption.PrefillFirstRegistrant || this.viewModel.RegistrarOption === RegistrationEntryBlockViewModel_1.RegistrarOption.UseFirstRegistrant) {
                            var firstRegistrantInfo = RegistrationEntry_1.getRegistrantBasicInfo(this.firstRegistrant, this.viewModel.RegistrantForms);
                            this.registrar.NickName = firstRegistrantInfo.FirstName;
                            this.registrar.LastName = firstRegistrantInfo.LastName;
                            this.registrar.Email = firstRegistrantInfo.Email;
                            this.registrar.FamilyGuid = this.firstRegistrant.FamilyGuid;
                            var hasAllInfo = (!!this.registrar.NickName) && (!!this.registrar.LastName) && (!!this.registrar.Email);
                            if (hasAllInfo && this.viewModel.RegistrarOption === RegistrationEntryBlockViewModel_1.RegistrarOption.UseFirstRegistrant) {
                                this.isRegistrarPanelShown = false;
                            }
                            return;
                        }
                    }
                },
                watch: {
                    currentPerson: {
                        immediate: true,
                        handler: function () {
                            this.prefillRegistrar();
                        }
                    }
                },
                template: "\n<div v-if=\"isRegistrarPanelShown\" class=\"well\">\n    <h4>This Registration Was Completed By</h4>\n    <template v-if=\"useLoggedInPersonForRegistrar\">\n        <div class=\"row\">\n            <div class=\"col-md-6\">\n                <StaticFormControl label=\"First Name\" v-model=\"registrar.NickName\" />\n                <StaticFormControl label=\"Email\" v-model=\"registrar.Email\" />\n            </div>\n            <div class=\"col-md-6\">\n                <StaticFormControl label=\"Last Name\" v-model=\"registrar.LastName\" />\n            </div>\n        </div>\n    </template>\n    <template v-else>\n        <div class=\"row\">\n            <div class=\"col-md-6\">\n                <TextBox label=\"First Name\" rules=\"required\" v-model=\"registrar.NickName\" tabIndex=\"1\" />\n                <EmailBox label=\"Send Confirmation Emails To\" rules=\"required\" v-model=\"registrar.Email\" tabIndex=\"3\" />\n                <CheckBox v-if=\"doShowUpdateEmailOption\" label=\"Should Your Account Be Updated To Use This Email Address?\" v-model=\"registrar.UpdateEmail\" />\n            </div>\n            <div class=\"col-md-6\">\n                <TextBox label=\"Last Name\" rules=\"required\" v-model=\"registrar.LastName\" tabIndex=\"2\" />\n                <RadioButtonList\n                    v-if=\"familyOptions.length\"\n                    :label=\"(registrar.NickName || 'Person') + ' is in the same immediate family as'\"\n                    rules='required:{\"allowEmptyString\": true}'\n                    v-model=\"registrar.FamilyGuid\"\n                    :options=\"familyOptions\"\n                    validationTitle=\"Family\" />\n            </div>\n        </div>\n    </template>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Registrar.js.map