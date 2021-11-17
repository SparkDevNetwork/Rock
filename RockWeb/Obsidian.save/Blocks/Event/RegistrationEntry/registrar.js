System.register(["vue", "../../../Elements/checkBox", "../../../Elements/emailBox", "../../../Elements/radioButtonList", "../../../Elements/staticFormControl", "../../../Elements/textBox", "../registrationEntry", "../../../Store/index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, checkBox_1, emailBox_1, radioButtonList_1, staticFormControl_1, textBox_1, registrationEntry_1, index_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (checkBox_1_1) {
                checkBox_1 = checkBox_1_1;
            },
            function (emailBox_1_1) {
                emailBox_1 = emailBox_1_1;
            },
            function (radioButtonList_1_1) {
                radioButtonList_1 = radioButtonList_1_1;
            },
            function (staticFormControl_1_1) {
                staticFormControl_1 = staticFormControl_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (registrationEntry_1_1) {
                registrationEntry_1 = registrationEntry_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.Registrar",
                components: {
                    TextBox: textBox_1.default,
                    CheckBox: checkBox_1.default,
                    EmailBox: emailBox_1.default,
                    StaticFormControl: staticFormControl_1.default,
                    RadioButtonList: radioButtonList_1.default
                },
                setup() {
                    return {
                        getRegistrationEntryBlockArgs: vue_1.inject("getRegistrationEntryBlockArgs"),
                        registrationEntryState: vue_1.inject("registrationEntryState")
                    };
                },
                data() {
                    return {
                        isRegistrarPanelShown: true
                    };
                },
                computed: {
                    useLoggedInPersonForRegistrar() {
                        return (!!this.currentPerson) && this.viewModel.registrarOption === 3;
                    },
                    currentPerson() {
                        return store.state.currentPerson;
                    },
                    registrar() {
                        return this.registrationEntryState.registrar;
                    },
                    firstRegistrant() {
                        return this.registrationEntryState.registrants[0];
                    },
                    viewModel() {
                        return this.registrationEntryState.viewModel;
                    },
                    doShowUpdateEmailOption() {
                        var _a;
                        return !this.viewModel.forceEmailUpdate && !!((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.email);
                    },
                    registrantInfos() {
                        return this.registrationEntryState.registrants.map(r => registrationEntry_1.getRegistrantBasicInfo(r, this.viewModel.registrantForms));
                    },
                    registrantTerm() {
                        return this.registrantInfos.length === 1 ? this.viewModel.registrantTerm : this.viewModel.pluralRegistrantTerm;
                    },
                    instanceName() {
                        return this.viewModel.instanceName;
                    },
                    familyOptions() {
                        const options = [];
                        const usedFamilyGuids = {};
                        if (this.viewModel.registrantsSameFamily !== 2) {
                            return options;
                        }
                        for (let i = 0; i < this.registrationEntryState.registrants.length; i++) {
                            const registrant = this.registrationEntryState.registrants[i];
                            const info = registrationEntry_1.getRegistrantBasicInfo(registrant, this.viewModel.registrantForms);
                            if (!usedFamilyGuids[registrant.familyGuid] && (info === null || info === void 0 ? void 0 : info.firstName) && (info === null || info === void 0 ? void 0 : info.lastName)) {
                                options.push({
                                    text: `${info.firstName} ${info.lastName}`,
                                    value: registrant.familyGuid
                                });
                                usedFamilyGuids[registrant.familyGuid] = true;
                            }
                        }
                        if (!usedFamilyGuids[this.registrationEntryState.ownFamilyGuid]) {
                            options.push({
                                text: "None",
                                value: this.registrationEntryState.ownFamilyGuid
                            });
                        }
                        return options;
                    },
                },
                methods: {
                    prefillRegistrar() {
                        this.isRegistrarPanelShown = true;
                        if (this.currentPerson &&
                            (this.viewModel.registrarOption === 3 || this.viewModel.registrarOption === 0)) {
                            this.registrar.nickName = this.currentPerson.nickName || this.currentPerson.firstName || "";
                            this.registrar.lastName = this.currentPerson.lastName || "";
                            this.registrar.email = this.currentPerson.email || "";
                            this.registrar.familyGuid = this.currentPerson.primaryFamilyGuid || null;
                            return;
                        }
                        if (this.viewModel.registrarOption === 0) {
                            return;
                        }
                        if (this.viewModel.registrarOption === 1 || this.viewModel.registrarOption === 2) {
                            const firstRegistrantInfo = registrationEntry_1.getRegistrantBasicInfo(this.firstRegistrant, this.viewModel.registrantForms);
                            this.registrar.nickName = firstRegistrantInfo.firstName;
                            this.registrar.lastName = firstRegistrantInfo.lastName;
                            this.registrar.email = firstRegistrantInfo.email;
                            this.registrar.familyGuid = this.firstRegistrant.familyGuid;
                            const hasAllInfo = (!!this.registrar.nickName) && (!!this.registrar.lastName) && (!!this.registrar.email);
                            if (hasAllInfo && this.viewModel.registrarOption === 2) {
                                this.isRegistrarPanelShown = false;
                            }
                            return;
                        }
                    }
                },
                watch: {
                    currentPerson: {
                        immediate: true,
                        handler() {
                            this.prefillRegistrar();
                        }
                    }
                },
                template: `
<div v-if="isRegistrarPanelShown" class="well">
    <h4>This Registration Was Completed By</h4>
    <template v-if="useLoggedInPersonForRegistrar">
        <div class="row">
            <div class="col-md-6">
                <StaticFormControl label="First Name" v-model="registrar.nickName" />
                <StaticFormControl label="Email" v-model="registrar.email" />
            </div>
            <div class="col-md-6">
                <StaticFormControl label="Last Name" v-model="registrar.lastName" />
            </div>
        </div>
    </template>
    <template v-else>
        <div class="row">
            <div class="col-md-6">
                <TextBox label="First Name" rules="required" v-model="registrar.nickName" tabIndex="1" />
                <EmailBox label="Send Confirmation Emails To" rules="required" v-model="registrar.email" tabIndex="3" />
                <CheckBox v-if="doShowUpdateEmailOption" label="Should Your Account Be Updated To Use This Email Address?" v-model="registrar.updateEmail" />
            </div>
            <div class="col-md-6">
                <TextBox label="Last Name" rules="required" v-model="registrar.lastName" tabIndex="2" />
                <RadioButtonList
                    v-if="familyOptions.length"
                    :label="(registrar.nickName || 'Person') + ' is in the same immediate family as'"
                    rules='required:{"allowEmptyString": true}'
                    v-model="registrar.familyGuid"
                    :options="familyOptions"
                    validationTitle="Family" />
            </div>
        </div>
    </template>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=registrar.js.map