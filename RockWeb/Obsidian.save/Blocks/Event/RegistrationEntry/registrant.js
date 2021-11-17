System.register(["vue", "../../../Elements/dropDownList", "../../../Elements/radioButtonList", "../registrationEntry", "../../../Services/string", "../../../Elements/rockButton", "./registrantPersonField", "./registrantAttributeField", "../../../Elements/alert", "../../../Util/guid", "../../../Controls/rockForm", "./feeField", "../../../Elements/itemsWithPreAndPostHtml", "../../../Store/index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, dropDownList_1, radioButtonList_1, registrationEntry_1, string_1, rockButton_1, registrantPersonField_1, registrantAttributeField_1, alert_1, guid_1, rockForm_1, feeField_1, itemsWithPreAndPostHtml_1, index_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (radioButtonList_1_1) {
                radioButtonList_1 = radioButtonList_1_1;
            },
            function (registrationEntry_1_1) {
                registrationEntry_1 = registrationEntry_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (registrantPersonField_1_1) {
                registrantPersonField_1 = registrantPersonField_1_1;
            },
            function (registrantAttributeField_1_1) {
                registrantAttributeField_1 = registrantAttributeField_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (rockForm_1_1) {
                rockForm_1 = rockForm_1_1;
            },
            function (feeField_1_1) {
                feeField_1 = feeField_1_1;
            },
            function (itemsWithPreAndPostHtml_1_1) {
                itemsWithPreAndPostHtml_1 = itemsWithPreAndPostHtml_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.Registrant",
                components: {
                    RadioButtonList: radioButtonList_1.default,
                    RockButton: rockButton_1.default,
                    RegistrantPersonField: registrantPersonField_1.default,
                    RegistrantAttributeField: registrantAttributeField_1.default,
                    Alert: alert_1.default,
                    RockForm: rockForm_1.default,
                    FeeField: feeField_1.default,
                    DropDownList: dropDownList_1.default,
                    ItemsWithPreAndPostHtml: itemsWithPreAndPostHtml_1.default
                },
                props: {
                    currentRegistrant: {
                        type: Object,
                        required: true
                    },
                    isWaitList: {
                        type: Boolean,
                        required: true
                    }
                },
                setup() {
                    const registrationEntryState = vue_1.inject("registrationEntryState");
                    return {
                        registrationEntryState
                    };
                },
                data() {
                    return {
                        fieldSources: {
                            personField: 0,
                            personAttribute: 1,
                            groupMemberAttribute: 2,
                            registrantAttribute: 4
                        }
                    };
                },
                computed: {
                    showPrevious() {
                        return this.registrationEntryState.firstStep !== this.registrationEntryState.steps.perRegistrantForms;
                    },
                    viewModel() {
                        return this.registrationEntryState.viewModel;
                    },
                    currentFormIndex() {
                        return this.registrationEntryState.currentRegistrantFormIndex;
                    },
                    currentForm() {
                        return this.formsToShow[this.currentFormIndex] || null;
                    },
                    isLastForm() {
                        return (this.currentFormIndex + 1) === this.formsToShow.length;
                    },
                    formsToShow() {
                        if (!this.isWaitList) {
                            return this.viewModel.registrantForms;
                        }
                        return this.viewModel.registrantForms.filter(form => form.fields.some(field => field.showOnWaitList));
                    },
                    currentFormFields() {
                        var _a;
                        return (((_a = this.currentForm) === null || _a === void 0 ? void 0 : _a.fields) || [])
                            .filter(f => !this.isWaitList || f.showOnWaitList);
                    },
                    prePostHtmlItems() {
                        return this.currentFormFields
                            .map(f => ({
                            preHtml: f.preHtml,
                            postHtml: f.postHtml,
                            slotName: f.guid
                        }));
                    },
                    currentPerson() {
                        return store.state.currentPerson;
                    },
                    pluralFeeTerm() {
                        return string_1.default.toTitleCase(this.viewModel.pluralFeeTerm || "fees");
                    },
                    familyOptions() {
                        var _a;
                        const options = [];
                        const usedFamilyGuids = {};
                        if (this.viewModel.registrantsSameFamily !== 2) {
                            return options;
                        }
                        for (let i = 0; i < this.registrationEntryState.currentRegistrantIndex; i++) {
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
                        if (((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.primaryFamilyGuid) && this.currentPerson.fullName && !usedFamilyGuids[this.currentPerson.primaryFamilyGuid]) {
                            options.push({
                                text: this.currentPerson.fullName,
                                value: this.currentPerson.primaryFamilyGuid
                            });
                        }
                        const familyGuid = usedFamilyGuids[this.currentRegistrant.familyGuid] == true
                            ? guid_1.newGuid()
                            : this.currentRegistrant.familyGuid;
                        options.push({
                            text: "None of the above",
                            value: familyGuid
                        });
                        return options;
                    },
                    familyMemberOptions() {
                        const selectedFamily = this.currentRegistrant.familyGuid;
                        if (!selectedFamily) {
                            return [];
                        }
                        const usedFamilyMemberGuids = this.registrationEntryState.registrants
                            .filter(r => r.personGuid && r.personGuid !== this.currentRegistrant.personGuid)
                            .map(r => r.personGuid);
                        return this.viewModel.familyMembers
                            .filter(fm => guid_1.areEqual(fm.familyGuid, selectedFamily) &&
                            !usedFamilyMemberGuids.includes(fm.guid))
                            .map(fm => ({
                            text: fm.fullName,
                            value: fm.guid
                        }));
                    },
                    uppercaseRegistrantTerm() {
                        return string_1.default.toTitleCase(this.viewModel.registrantTerm);
                    },
                    firstName() {
                        return registrationEntry_1.getRegistrantBasicInfo(this.currentRegistrant, this.viewModel.registrantForms).firstName;
                    },
                    familyMember() {
                        const personGuid = this.currentRegistrant.personGuid;
                        if (!personGuid) {
                            return null;
                        }
                        return this.viewModel.familyMembers.find(fm => guid_1.areEqual(fm.guid, personGuid)) || null;
                    }
                },
                methods: {
                    onPrevious() {
                        if (this.currentFormIndex <= 0) {
                            this.$emit("previous");
                            return;
                        }
                        this.registrationEntryState.currentRegistrantFormIndex--;
                    },
                    onNext() {
                        const lastFormIndex = this.formsToShow.length - 1;
                        if (this.currentFormIndex >= lastFormIndex) {
                            this.$emit("next");
                            return;
                        }
                        this.registrationEntryState.currentRegistrantFormIndex++;
                    },
                    copyValuesFromFamilyMember() {
                        if (!this.familyMember) {
                            return;
                        }
                        for (const form of this.viewModel.registrantForms) {
                            for (const field of form.fields) {
                                if (field.guid in this.familyMember.fieldValues) {
                                    const familyMemberValue = this.familyMember.fieldValues[field.guid];
                                    if (!familyMemberValue) {
                                        delete this.currentRegistrant.fieldValues[field.guid];
                                    }
                                    else if (typeof familyMemberValue === "object") {
                                        this.currentRegistrant.fieldValues[field.guid] = Object.assign({}, familyMemberValue);
                                    }
                                    else {
                                        this.currentRegistrant.fieldValues[field.guid] = familyMemberValue;
                                    }
                                }
                                else {
                                    delete this.currentRegistrant.fieldValues[field.guid];
                                }
                            }
                        }
                    }
                },
                watch: {
                    "currentRegistrant.familyGuid"() {
                        this.currentRegistrant.personGuid = "";
                    },
                    familyMember: {
                        handler() {
                            if (!this.familyMember) {
                                for (const form of this.viewModel.registrantForms) {
                                    for (const field of form.fields) {
                                        delete this.currentRegistrant.fieldValues[field.guid];
                                    }
                                }
                            }
                            else {
                                this.copyValuesFromFamilyMember();
                            }
                        }
                    }
                },
                created() {
                    this.copyValuesFromFamilyMember();
                },
                template: `
<div>
    <RockForm @submit="onNext">
        <template v-if="currentFormIndex === 0">
            <div v-if="familyOptions.length > 1" class="well js-registration-same-family">
                <RadioButtonList :label="(firstName || uppercaseRegistrantTerm) + ' is in the same immediate family as'" rules='required:{"allowEmptyString": true}' v-model="currentRegistrant.familyGuid" :options="familyOptions" validationTitle="Family" />
            </div>
            <div v-if="familyMemberOptions.length" class="row">
                <div class="col-md-6">
                    <DropDownList v-model="currentRegistrant.personGuid" :options="familyMemberOptions" label="Family Member to Register" />
                </div>
            </div>
        </template>

        <ItemsWithPreAndPostHtml :items="prePostHtmlItems">
            <template v-for="field in currentFormFields" :key="field.guid" v-slot:[field.guid]>
                <RegistrantPersonField v-if="field.fieldSource === fieldSources.personField" :field="field" :fieldValues="currentRegistrant.fieldValues" :isKnownFamilyMember="!!currentRegistrant.personGuid" />
                <RegistrantAttributeField v-else-if="field.fieldSource === fieldSources.registrantAttribute || field.fieldSource === fieldSources.personAttribute" :field="field" :fieldValues="currentRegistrant.fieldValues" />
                <Alert alertType="danger" v-else>Could not resolve field source {{field.fieldSource}}</Alert>
            </template>
        </ItemsWithPreAndPostHtml>

        <div v-if="!isWaitList && isLastForm && viewModel.fees.length" class="well registration-additional-options">
            <h4>{{pluralFeeTerm}}</h4>
            <template v-for="fee in viewModel.fees" :key="fee.guid">
                <FeeField :fee="fee" v-model="currentRegistrant.feeItemQuantities" />
            </template>
        </div>

        <div class="actions row">
            <div class="col-xs-6">
                <RockButton v-if="showPrevious" btnType="default" @click="onPrevious">
                    Previous
                </RockButton>
            </div>
            <div class="col-xs-6 text-right">
                <RockButton btnType="primary" type="submit">
                    Next
                </RockButton>
            </div>
        </div>
    </RockForm>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=registrant.js.map