System.register(["vue", "../../../Elements/Alert", "../../../Elements/NumberUpDown", "../../../Elements/RockButton", "../../../Services/String", "../../../Util/Guid", "../RegistrationEntry"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Alert_1, NumberUpDown_1, RockButton_1, String_1, Guid_1, RegistrationEntry_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (NumberUpDown_1_1) {
                NumberUpDown_1 = NumberUpDown_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (RegistrationEntry_1_1) {
                RegistrationEntry_1 = RegistrationEntry_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Intro',
                components: {
                    NumberUpDown: NumberUpDown_1.default,
                    RockButton: RockButton_1.default,
                    Alert: Alert_1.default
                },
                data: function () {
                    var registrationEntryState = vue_1.inject('registrationEntryState');
                    return {
                        numberOfRegistrants: registrationEntryState.Registrants.length,
                        registrationEntryState: registrationEntryState,
                        showRemainingCapacity: false
                    };
                },
                computed: {
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    viewModel: function () {
                        return this.registrationEntryState.ViewModel;
                    },
                    numberToAddToWaitlist: function () {
                        if (this.viewModel.SpotsRemaining === null || !this.viewModel.WaitListEnabled) {
                            return 0;
                        }
                        if (this.viewModel.SpotsRemaining >= this.numberOfRegistrants) {
                            return 0;
                        }
                        return this.numberOfRegistrants - this.viewModel.SpotsRemaining;
                    },
                    remainingCapacityPhrase: function () {
                        var spots = this.viewModel.SpotsRemaining;
                        if (spots === null) {
                            return '';
                        }
                        return String_1.pluralConditional(spots, "1 more " + this.registrantTerm, spots + " more " + this.registrantTermPlural);
                    },
                    isFull: function () {
                        if (this.viewModel.SpotsRemaining === null) {
                            return false;
                        }
                        return this.viewModel.SpotsRemaining < 1;
                    },
                    registrantTerm: function () {
                        this.viewModel.InstanceName;
                        return (this.viewModel.RegistrantTerm || 'registrant').toLowerCase();
                    },
                    registrantTermPlural: function () {
                        return (this.viewModel.PluralRegistrantTerm || 'registrants').toLowerCase();
                    },
                    registrationTerm: function () {
                        return (this.viewModel.RegistrationTerm || 'registration').toLowerCase();
                    },
                    registrationTermPlural: function () {
                        return (this.viewModel.PluralRegistrationTerm || 'registrations').toLowerCase();
                    },
                    registrationTermTitleCase: function () {
                        return String_1.toTitleCase(this.registrationTerm);
                    }
                },
                methods: {
                    pluralConditional: String_1.pluralConditional,
                    onNext: function () {
                        var forcedFamilyGuid = RegistrationEntry_1.getForcedFamilyGuid(this.currentPerson, this.viewModel);
                        var usedFamilyMemberGuids = this.registrationEntryState.Registrants
                            .filter(function (r) { return r.PersonGuid; })
                            .map(function (r) { return r.PersonGuid; });
                        var availableFamilyMembers = this.viewModel.FamilyMembers
                            .filter(function (fm) {
                            return Guid_1.areEqual(fm.FamilyGuid, forcedFamilyGuid) &&
                                !usedFamilyMemberGuids.includes(fm.Guid);
                        });
                        while (this.numberOfRegistrants > this.registrationEntryState.Registrants.length) {
                            var registrant = RegistrationEntry_1.getDefaultRegistrantInfo(this.currentPerson, this.viewModel, forcedFamilyGuid);
                            this.registrationEntryState.Registrants.push(registrant);
                        }
                        this.registrationEntryState.Registrants.length = this.numberOfRegistrants;
                        var firstWaitListIndex = this.numberOfRegistrants - this.numberToAddToWaitlist;
                        for (var i = firstWaitListIndex; i < this.numberOfRegistrants; i++) {
                            this.registrationEntryState.Registrants[i].IsOnWaitList = true;
                        }
                        if (availableFamilyMembers.length && this.registrationEntryState.Registrants.length) {
                            var familyMember = availableFamilyMembers[0];
                            var registrant = this.registrationEntryState.Registrants[0];
                            registrant.PersonGuid = familyMember.Guid;
                        }
                        this.$emit('next');
                    },
                },
                watch: {
                    numberOfRegistrants: function () {
                        var _this = this;
                        if (!this.viewModel.WaitListEnabled && this.viewModel.SpotsRemaining !== null && this.viewModel.SpotsRemaining < this.numberOfRegistrants) {
                            this.showRemainingCapacity = true;
                            var spotsRemaining_1 = this.viewModel.SpotsRemaining;
                            this.$nextTick(function () { return _this.numberOfRegistrants = spotsRemaining_1; });
                        }
                    }
                },
                template: "\n<div class=\"registrationentry-intro\">\n    <Alert v-if=\"isFull && numberToAddToWaitlist !== numberOfRegistrants\" class=\"text-left\" alertType=\"warning\">\n        <strong>{{registrationTermTitleCase}} Full</strong>\n        <p>\n            There are not any more {{registrationTermPlural}} available for {{viewModel.InstanceName}}. \n        </p>\n    </Alert>\n    <Alert v-if=\"showRemainingCapacity\" class=\"text-left\" alertType=\"warning\">\n        <strong>{{registrationTermTitleCase}} Full</strong>\n        <p>\n            This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.\n        </p>\n    </Alert>\n    <div class=\"text-left\" v-html=\"viewModel.InstructionsHtml\">\n    </div>\n    <div v-if=\"viewModel.MaxRegistrants > 1\" class=\"registrationentry-intro\">\n        <h1>How many {{viewModel.PluralRegistrantTerm}} will you be registering?</h1>\n        <NumberUpDown v-model=\"numberOfRegistrants\" class=\"margin-t-sm\" numberIncrementClasses=\"input-lg\" :max=\"viewModel.MaxRegistrants\" />\n    </div>\n    <Alert v-if=\"viewModel.TimeoutMinutes\" alertType=\"info\" class=\"text-left\">\n        Due to a high-volume of expected interest, your {{registrationTerm}} session will expire after\n        {{pluralConditional(viewModel.TimeoutMinutes, 'a minute', viewModel.TimeoutMinutes + ' minutes')}}\n        of inactivity.\n    </Alert>\n    <Alert v-if=\"numberToAddToWaitlist === numberOfRegistrants\" class=\"text-left\" alertType=\"warning\">\n        This {{registrationTerm}} has reached its capacity. Complete the registration to be added to the waitlist.\n    </Alert>\n    <Alert v-else-if=\"numberToAddToWaitlist\" class=\"text-left\" alertType=\"warning\">\n        This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.\n        The first {{pluralConditional(viewModel.SpotsRemaining, registrantTerm, viewModel.SpotsRemaining + ' ' + registrantTermPlural)}} you add will be registered for {{viewModel.InstanceName}}.\n        The remaining {{pluralConditional(numberToAddToWaitlist, registrantTerm, numberToAddToWaitlist + ' ' + registrantTermPlural)}} will be added to the waitlist. \n    </Alert>\n    <div class=\"actions text-right\">\n        <RockButton btnType=\"primary\" @click=\"onNext\">\n            Next\n        </RockButton>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Intro.js.map