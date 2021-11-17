System.register(["../../Util/bus", "../../Templates/paneledBlockTemplate", "../../Elements/rockButton", "../../Elements/textBox", "vue", "../../Store/index", "../../Elements/emailBox", "../../Controls/rockValidation", "../../Controls/rockForm", "../../Controls/loading", "../../Controls/primaryBlock", "../../Elements/datePicker", "../../Controls/addressControl", "../../Services/number", "../../Util/rockDateTime"], function (exports_1, context_1) {
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
    var bus_1, paneledBlockTemplate_1, rockButton_1, textBox_1, vue_1, index_1, emailBox_1, rockValidation_1, rockForm_1, loading_1, primaryBlock_1, datePicker_1, addressControl_1, number_1, rockDateTime_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (bus_1_1) {
                bus_1 = bus_1_1;
            },
            function (paneledBlockTemplate_1_1) {
                paneledBlockTemplate_1 = paneledBlockTemplate_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            },
            function (emailBox_1_1) {
                emailBox_1 = emailBox_1_1;
            },
            function (rockValidation_1_1) {
                rockValidation_1 = rockValidation_1_1;
            },
            function (rockForm_1_1) {
                rockForm_1 = rockForm_1_1;
            },
            function (loading_1_1) {
                loading_1 = loading_1_1;
            },
            function (primaryBlock_1_1) {
                primaryBlock_1 = primaryBlock_1_1;
            },
            function (datePicker_1_1) {
                datePicker_1 = datePicker_1_1;
            },
            function (addressControl_1_1) {
                addressControl_1 = addressControl_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "Example.PersonDetail",
                components: {
                    PaneledBlockTemplate: paneledBlockTemplate_1.default,
                    RockButton: rockButton_1.default,
                    TextBox: textBox_1.default,
                    EmailBox: emailBox_1.default,
                    RockValidation: rockValidation_1.default,
                    RockForm: rockForm_1.default,
                    Loading: loading_1.default,
                    PrimaryBlock: primaryBlock_1.default,
                    DatePicker: datePicker_1.default,
                    AddressControl: addressControl_1.default
                },
                setup() {
                    return {
                        invokeBlockAction: vue_1.inject("invokeBlockAction")
                    };
                },
                data() {
                    return {
                        person: null,
                        personForEditing: null,
                        isEditMode: false,
                        messageToPublish: "",
                        receivedMessage: "",
                        isLoading: false,
                        birthdate: null,
                        address: addressControl_1.getDefaultAddressControlModel()
                    };
                },
                methods: {
                    setIsEditMode(isEditMode) {
                        this.isEditMode = isEditMode;
                    },
                    doEdit() {
                        var _a, _b;
                        this.personForEditing = this.person ? Object.assign({}, this.person) : null;
                        this.birthdate = (_b = (_a = this.birthdateOrNull) === null || _a === void 0 ? void 0 : _a.toASPString("yyyy-MM-dd")) !== null && _b !== void 0 ? _b : null;
                        this.setIsEditMode(true);
                    },
                    doCancel() {
                        this.setIsEditMode(false);
                    },
                    doSave() {
                        var _a;
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.personForEditing) {
                                const match = /^(\d+)-(\d+)-(\d+)/.exec((_a = this.birthdate) !== null && _a !== void 0 ? _a : "");
                                let birthDay = null;
                                let birthMonth = null;
                                let birthYear = null;
                                if (match !== null) {
                                    birthYear = number_1.toNumber(match[1]);
                                    birthMonth = number_1.toNumber(match[2]);
                                    birthDay = number_1.toNumber(match[3]);
                                }
                                this.person = Object.assign(Object.assign({}, this.personForEditing), { birthDay: birthDay, birthMonth: birthMonth, birthYear: birthYear });
                                this.isLoading = true;
                                yield this.invokeBlockAction("EditPerson", {
                                    personArgs: this.person
                                });
                                this.isLoading = false;
                            }
                            this.setIsEditMode(false);
                        });
                    },
                    doPublish() {
                        bus_1.default.publish("PersonDetail:Message", this.messageToPublish);
                        this.messageToPublish = "";
                    },
                    receiveMessage(message) {
                        this.receivedMessage = message;
                    }
                },
                computed: {
                    birthdateOrNull() {
                        var _a;
                        if (!((_a = this.person) === null || _a === void 0 ? void 0 : _a.birthDay) || !this.person.birthMonth || !this.person.birthYear) {
                            return null;
                        }
                        return rockDateTime_1.RockDateTime.fromParts(this.person.birthYear, this.person.birthMonth, this.person.birthDay);
                    },
                    birthdateFormatted() {
                        if (!this.birthdateOrNull) {
                            return "Not Completed";
                        }
                        return this.birthdateOrNull.toLocaleString(rockDateTime_1.DateTimeFormat.DateTimeShort);
                    },
                    blockTitle() {
                        return this.person ?
                            `: ${this.person.nickName || this.person.firstName} ${this.person.lastName}` :
                            "";
                    },
                    currentPerson() {
                        return store.state.currentPerson;
                    },
                    currentPersonGuid() {
                        return this.currentPerson ? this.currentPerson.guid : null;
                    }
                },
                watch: {
                    currentPersonGuid: {
                        immediate: true,
                        handler() {
                            return __awaiter(this, void 0, void 0, function* () {
                                if (!this.currentPersonGuid) {
                                    this.person = null;
                                    return;
                                }
                                if (this.person && this.person.guid === this.currentPersonGuid) {
                                    return;
                                }
                                this.isLoading = true;
                                this.person = (yield this.invokeBlockAction("GetPersonViewModel")).data;
                                this.isLoading = false;
                            });
                        }
                    }
                },
                created() {
                    bus_1.default.subscribe("PersonSecondary:Message", this.receiveMessage);
                },
                template: `
<PrimaryBlock :hideSecondaryBlocks="isEditMode">
    <PaneledBlockTemplate>
        <template v-slot:title>
            <i class="fa fa-flask"></i>
            Edit Yourself{{blockTitle}}
        </template>
        <template v-slot:default>
            <Loading :isLoading="isLoading">
                <p v-if="!person">
                    There is no person loaded.
                </p>
                <RockForm v-else-if="isEditMode" @submit="doSave">
                    <div class="row">
                        <div class="col-sm-6">
                            <TextBox label="First Name" v-model="personForEditing.firstName" rules="required" />
                            <TextBox label="Nick Name" v-model="personForEditing.nickName" />
                            <TextBox label="Last Name" v-model="personForEditing.lastName" rules="required" />
                        </div>
                        <div class="col-sm-6">
                            <EmailBox label="Email" v-model="personForEditing.email" />
                            <DatePicker label="Birthdate" v-model="birthdate" rules="required" />
                        </div>
                        <div class="col-sm-12">
                            <AddressControl v-model="address" />
                        </div>
                    </div>
                    <div class="actions">
                        <RockButton btnType="primary" type="submit">Save</RockButton>
                        <RockButton btnType="link" @click="doCancel">Cancel</RockButton>
                    </div>
                </RockForm>
                <template v-else>
                    <div class="row">
                        <div class="col-sm-6">
                            <dl>
                                <dt>First Name</dt>
                                <dd>{{person.firstName}}</dd>
                                <dt>Last Name</dt>
                                <dd>{{person.lastName}}</dd>
                                <dt>Email</dt>
                                <dd>{{person.email}}</dd>
                                <dt>Birthdate</dt>
                                <dd>{{birthdateFormatted}}</dd>
                            </dl>
                        </div>
                        <div class="col-sm-6">
                            <div class="well">
                                <TextBox label="Message" v-model="messageToPublish" />
                                <RockButton btnType="primary" btnSize="sm" @click="doPublish">Publish</RockButton>
                            </div>
                            <p>
                                <strong>Secondary block says:</strong>
                                {{receivedMessage}}
                            </p>
                        </div>
                    </div>
                    <div class="actions">
                        <RockButton btnType="primary" @click="doEdit">Edit</RockButton>
                    </div>
                </template>
            </Loading>
        </template>
    </PaneledBlockTemplate>
</PrimaryBlock>`
            }));
        }
    };
});
//# sourceMappingURL=personDetail.js.map