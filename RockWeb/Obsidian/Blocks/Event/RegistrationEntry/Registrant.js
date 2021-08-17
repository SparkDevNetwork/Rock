System.register(["vue", "../../../Elements/DropDownList", "../../../Elements/RadioButtonList", "../RegistrationEntry", "../../../Services/String", "../../../Elements/RockButton", "./RegistrantPersonField", "./RegistrantAttributeField", "../../../Elements/Alert", "./RegistrationEntryBlockViewModel", "../../../Util/Guid", "../../../Controls/RockForm", "./FeeField", "../../../Elements/ItemsWithPreAndPostHtml"], function (exports_1, context_1) {
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
    var vue_1, DropDownList_1, RadioButtonList_1, RegistrationEntry_1, String_1, RockButton_1, RegistrantPersonField_1, RegistrantAttributeField_1, Alert_1, RegistrationEntryBlockViewModel_1, Guid_1, RockForm_1, FeeField_1, ItemsWithPreAndPostHtml_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (RadioButtonList_1_1) {
                RadioButtonList_1 = RadioButtonList_1_1;
            },
            function (RegistrationEntry_1_1) {
                RegistrationEntry_1 = RegistrationEntry_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (RegistrantPersonField_1_1) {
                RegistrantPersonField_1 = RegistrantPersonField_1_1;
            },
            function (RegistrantAttributeField_1_1) {
                RegistrantAttributeField_1 = RegistrantAttributeField_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (RegistrationEntryBlockViewModel_1_1) {
                RegistrationEntryBlockViewModel_1 = RegistrationEntryBlockViewModel_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (FeeField_1_1) {
                FeeField_1 = FeeField_1_1;
            },
            function (ItemsWithPreAndPostHtml_1_1) {
                ItemsWithPreAndPostHtml_1 = ItemsWithPreAndPostHtml_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Registrant',
                components: {
                    RadioButtonList: RadioButtonList_1.default,
                    RockButton: RockButton_1.default,
                    RegistrantPersonField: RegistrantPersonField_1.default,
                    RegistrantAttributeField: RegistrantAttributeField_1.default,
                    Alert: Alert_1.default,
                    RockForm: RockForm_1.default,
                    FeeField: FeeField_1.default,
                    DropDownList: DropDownList_1.default,
                    ItemsWithPreAndPostHtml: ItemsWithPreAndPostHtml_1.default
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
                setup: function () {
                    var registrationEntryState = vue_1.inject('registrationEntryState');
                    return {
                        registrationEntryState: registrationEntryState
                    };
                },
                data: function () {
                    return {
                        fieldSources: {
                            PersonField: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.PersonField,
                            PersonAttribute: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.PersonAttribute,
                            GroupMemberAttribute: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.GroupMemberAttribute,
                            RegistrantAttribute: RegistrationEntryBlockViewModel_1.RegistrationFieldSource.RegistrantAttribute
                        }
                    };
                },
                computed: {
                    showPrevious: function () {
                        return this.registrationEntryState.FirstStep !== this.registrationEntryState.Steps.perRegistrantForms;
                    },
                    viewModel: function () {
                        return this.registrationEntryState.ViewModel;
                    },
                    currentFormIndex: function () {
                        return this.registrationEntryState.CurrentRegistrantFormIndex;
                    },
                    currentForm: function () {
                        return this.formsToShow[this.currentFormIndex] || null;
                    },
                    isLastForm: function () {
                        return (this.currentFormIndex + 1) === this.formsToShow.length;
                    },
                    formsToShow: function () {
                        if (!this.isWaitList) {
                            return this.viewModel.registrantForms;
                        }
                        return this.viewModel.registrantForms.filter(function (form) { return form.fields.some(function (field) { return field.showOnWaitList; }); });
                    },
                    currentFormFields: function () {
                        var _this = this;
                        var _a;
                        return (((_a = this.currentForm) === null || _a === void 0 ? void 0 : _a.fields) || [])
                            .filter(function (f) { return !_this.isWaitList || f.showOnWaitList; });
                    },
                    prePostHtmlItems: function () {
                        return this.currentFormFields
                            .map(function (f) { return ({
                            PreHtml: f.preHtml,
                            PostHtml: f.postHtml,
                            SlotName: f.guid
                        }); });
                    },
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    pluralFeeTerm: function () {
                        return String_1.default.toTitleCase(this.viewModel.pluralFeeTerm || 'fees');
                    },
                    familyOptions: function () {
                        var _a;
                        var options = [];
                        var usedFamilyGuids = {};
                        if (this.viewModel.registrantsSameFamily !== RegistrationEntryBlockViewModel_1.RegistrantsSameFamily.Ask) {
                            return options;
                        }
                        for (var i = 0; i < this.registrationEntryState.CurrentRegistrantIndex; i++) {
                            var registrant = this.registrationEntryState.Registrants[i];
                            var info = RegistrationEntry_1.getRegistrantBasicInfo(registrant, this.viewModel.registrantForms);
                            if (!usedFamilyGuids[registrant.FamilyGuid] && (info === null || info === void 0 ? void 0 : info.FirstName) && (info === null || info === void 0 ? void 0 : info.LastName)) {
                                options.push({
                                    key: registrant.FamilyGuid,
                                    text: info.FirstName + " " + info.LastName,
                                    value: registrant.FamilyGuid
                                });
                                usedFamilyGuids[registrant.FamilyGuid] = true;
                            }
                        }
                        if (((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.primaryFamilyGuid) && this.currentPerson.fullName && !usedFamilyGuids[this.currentPerson.primaryFamilyGuid]) {
                            options.push({
                                key: this.currentPerson.primaryFamilyGuid,
                                text: this.currentPerson.fullName,
                                value: this.currentPerson.primaryFamilyGuid
                            });
                        }
                        options.push({
                            key: this.currentRegistrant.OwnFamilyGuid,
                            text: 'None of the above',
                            value: this.currentRegistrant.OwnFamilyGuid
                        });
                        return options;
                    },
                    familyMemberOptions: function () {
                        var _this = this;
                        var selectedFamily = this.currentRegistrant.FamilyGuid;
                        if (!selectedFamily) {
                            return [];
                        }
                        var usedFamilyMemberGuids = this.registrationEntryState.Registrants
                            .filter(function (r) { return r.PersonGuid && r.PersonGuid !== _this.currentRegistrant.PersonGuid; })
                            .map(function (r) { return r.PersonGuid; });
                        return this.viewModel.familyMembers
                            .filter(function (fm) {
                            return Guid_1.areEqual(fm.familyGuid, selectedFamily) &&
                                !usedFamilyMemberGuids.includes(fm.guid);
                        })
                            .map(function (fm) { return ({
                            key: fm.guid,
                            text: fm.fullName,
                            value: fm.guid
                        }); });
                    },
                    uppercaseRegistrantTerm: function () {
                        return String_1.default.toTitleCase(this.viewModel.registrantTerm);
                    },
                    firstName: function () {
                        return RegistrationEntry_1.getRegistrantBasicInfo(this.currentRegistrant, this.viewModel.registrantForms).FirstName;
                    },
                    familyMember: function () {
                        var personGuid = this.currentRegistrant.PersonGuid;
                        if (!personGuid) {
                            return null;
                        }
                        return this.viewModel.familyMembers.find(function (fm) { return Guid_1.areEqual(fm.guid, personGuid); }) || null;
                    }
                },
                methods: {
                    onPrevious: function () {
                        if (this.currentFormIndex <= 0) {
                            this.$emit('previous');
                            return;
                        }
                        this.registrationEntryState.CurrentRegistrantFormIndex--;
                    },
                    onNext: function () {
                        var lastFormIndex = this.formsToShow.length - 1;
                        if (this.currentFormIndex >= lastFormIndex) {
                            this.$emit('next');
                            return;
                        }
                        this.registrationEntryState.CurrentRegistrantFormIndex++;
                    },
                    copyValuesFromFamilyMember: function () {
                        if (!this.familyMember) {
                            return;
                        }
                        for (var _i = 0, _a = this.viewModel.registrantForms; _i < _a.length; _i++) {
                            var form = _a[_i];
                            for (var _b = 0, _c = form.fields; _b < _c.length; _b++) {
                                var field = _c[_b];
                                if (field.guid in this.familyMember.fieldValues) {
                                    var familyMemberValue = this.familyMember.fieldValues[field.guid];
                                    if (!familyMemberValue) {
                                        delete this.currentRegistrant.FieldValues[field.guid];
                                    }
                                    else if (typeof familyMemberValue === 'object') {
                                        this.currentRegistrant.FieldValues[field.guid] = __assign({}, familyMemberValue);
                                    }
                                    else {
                                        this.currentRegistrant.FieldValues[field.guid] = familyMemberValue;
                                    }
                                }
                            }
                        }
                    }
                },
                watch: {
                    'currentRegistrant.FamilyGuid': function () {
                        this.currentRegistrant.PersonGuid = '';
                    },
                    familyMember: {
                        handler: function () {
                            if (!this.familyMember) {
                                for (var _i = 0, _a = this.viewModel.registrantForms; _i < _a.length; _i++) {
                                    var form = _a[_i];
                                    for (var _b = 0, _c = form.fields; _b < _c.length; _b++) {
                                        var field = _c[_b];
                                        delete this.currentRegistrant.FieldValues[field.guid];
                                    }
                                }
                            }
                            else {
                                this.copyValuesFromFamilyMember();
                            }
                        }
                    }
                },
                created: function () {
                    this.copyValuesFromFamilyMember();
                },
                template: "\n<div>\n    <RockForm @submit=\"onNext\">\n        <template v-if=\"currentFormIndex === 0\">\n            <div v-if=\"familyOptions.length > 1\" class=\"well js-registration-same-family\">\n                <RadioButtonList :label=\"(firstName || uppercaseRegistrantTerm) + ' is in the same immediate family as'\" rules='required:{\"allowEmptyString\": true}' v-model=\"currentRegistrant.FamilyGuid\" :options=\"familyOptions\" validationTitle=\"Family\" />\n            </div>\n            <div v-if=\"familyMemberOptions.length\" class=\"row\">\n                <div class=\"col-md-6\">\n                    <DropDownList v-model=\"currentRegistrant.PersonGuid\" :options=\"familyMemberOptions\" label=\"Family Member to Register\" />\n                </div>\n            </div>\n        </template>\n\n        <ItemsWithPreAndPostHtml :items=\"prePostHtmlItems\">\n            <template v-for=\"field in currentFormFields\" :key=\"field.guid\" v-slot:[field.guid]>\n                <RegistrantPersonField v-if=\"field.fieldSource === fieldSources.PersonField\" :field=\"field\" :fieldValues=\"currentRegistrant.FieldValues\" :isKnownFamilyMember=\"!!currentRegistrant.PersonGuid\" />\n                <RegistrantAttributeField v-else-if=\"field.fieldSource === fieldSources.RegistrantAttribute || field.fieldSource === fieldSources.PersonAttribute\" :field=\"field\" :fieldValues=\"currentRegistrant.FieldValues\" />\n                <Alert alertType=\"danger\" v-else>Could not resolve field source {{field.fieldSource}}</Alert>\n            </template>\n        </ItemsWithPreAndPostHtml>\n\n        <div v-if=\"!isWaitList && isLastForm && viewModel.fees.length\" class=\"well registration-additional-options\">\n            <h4>{{pluralFeeTerm}}</h4>\n            <template v-for=\"fee in viewModel.fees\" :key=\"fee.Guid\">\n                <FeeField :fee=\"fee\" v-model=\"currentRegistrant.FeeItemQuantities\" />\n            </template>\n        </div>\n\n        <div class=\"actions row\">\n            <div class=\"col-xs-6\">\n                <RockButton v-if=\"showPrevious\" btnType=\"default\" @click=\"onPrevious\">\n                    Previous\n                </RockButton>\n            </div>\n            <div class=\"col-xs-6 text-right\">\n                <RockButton btnType=\"primary\" type=\"submit\">\n                    Next\n                </RockButton>\n            </div>\n        </div>\n    </RockForm>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Registrant.js.map