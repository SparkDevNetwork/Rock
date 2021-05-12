// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
                        /** The number of registrants that this registrar is going to input */
                        numberOfRegistrants: registrationEntryState.Registrants.length || 1,
                        /** The shared state among all the components that make up this block */
                        registrationEntryState: registrationEntryState,
                        /** Should the remaining capacity warning be shown? */
                        showRemainingCapacity: false
                    };
                },
                computed: {
                    /** The currently authenticated person */
                    currentPerson: function () {
                        return this.$store.state.currentPerson;
                    },
                    /** The view model sent by the C# code behind. This is just a convenient shortcut to the shared object. */
                    viewModel: function () {
                        return this.registrationEntryState.ViewModel;
                    },
                    /** The number of these registrants that will be placed on a waitlist because of capacity rules */
                    numberToAddToWaitlist: function () {
                        if (this.viewModel.SpotsRemaining === null || !this.viewModel.WaitListEnabled) {
                            // There is no waitlist or no cap on number of attendees
                            return 0;
                        }
                        if (this.viewModel.SpotsRemaining >= this.numberOfRegistrants) {
                            // There is enough capacity left for all of these registrants
                            return 0;
                        }
                        // Some or all need to go on the waitlist
                        return this.numberOfRegistrants - this.viewModel.SpotsRemaining;
                    },
                    /** The capacity left phrase: Ex: 1 more camper */
                    remainingCapacityPhrase: function () {
                        var spots = this.viewModel.SpotsRemaining;
                        if (spots === null) {
                            return '';
                        }
                        return String_1.pluralConditional(spots, "1 more " + this.registrantTerm, spots + " more " + this.registrantTermPlural);
                    },
                    /** Is this instance full and no one else can register? */
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
                        // If the person is authenticated and the setting is to put registrants in the same family, then we force that family guid
                        var forcedFamilyGuid = RegistrationEntry_1.getForcedFamilyGuid(this.currentPerson, this.viewModel);
                        var usedFamilyMemberGuids = this.registrationEntryState.Registrants
                            .filter(function (r) { return r.PersonGuid; })
                            .map(function (r) { return r.PersonGuid; });
                        var availableFamilyMembers = this.viewModel.FamilyMembers
                            .filter(function (fm) {
                            return Guid_1.areEqual(fm.FamilyGuid, forcedFamilyGuid) &&
                                !usedFamilyMemberGuids.includes(fm.Guid);
                        });
                        // Resize the registrant array to match the selected number
                        while (this.numberOfRegistrants > this.registrationEntryState.Registrants.length) {
                            var registrant = RegistrationEntry_1.getDefaultRegistrantInfo(this.currentPerson, this.viewModel, forcedFamilyGuid);
                            this.registrationEntryState.Registrants.push(registrant);
                        }
                        this.registrationEntryState.Registrants.length = this.numberOfRegistrants;
                        // Set people beyond the capacity tro be on the waitlist
                        var firstWaitListIndex = this.numberOfRegistrants - this.numberToAddToWaitlist;
                        for (var i = firstWaitListIndex; i < this.numberOfRegistrants; i++) {
                            this.registrationEntryState.Registrants[i].IsOnWaitList = true;
                        }
                        // If there are family members, set the first registrant to be the first (feature parity with the original block)
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
                            // Do this on the next tick to allow the events to finish. Otherwise the component tree doesn't have time
                            // to respond to this, since the watch was triggered by the numberOfRegistrants change
                            this.$nextTick(function () { return _this.numberOfRegistrants = spotsRemaining_1; });
                        }
                    }
                },
                template: "\n<div class=\"registrationentry-intro\">\n    <Alert v-if=\"isFull\" class=\"text-left\" alertType=\"warning\">\n        <strong>{{registrationTermTitleCase}} Full</strong>\n        <p>\n            There are not any more {{registrationTermPlural}} available for {{viewModel.InstanceName}}. \n        </p>\n    </Alert>\n    <Alert v-if=\"showRemainingCapacity\" class=\"text-left\" alertType=\"warning\">\n        <strong>{{registrationTermTitleCase}} Full</strong>\n        <p>\n            This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.\n        </p>\n    </Alert>\n    <div class=\"text-left\" v-html=\"viewModel.InstructionsHtml\">\n    </div>\n    <div v-if=\"viewModel.MaxRegistrants > 1\" class=\"registrationentry-intro\">\n        <h1>How many {{viewModel.PluralRegistrantTerm}} will you be registering?</h1>\n        <NumberUpDown v-model=\"numberOfRegistrants\" class=\"margin-t-sm\" numberIncrementClasses=\"input-lg\" :max=\"viewModel.MaxRegistrants\" />\n    </div>\n    <Alert v-if=\"viewModel.TimeoutMinutes\" alertType=\"info\" class=\"text-left\">\n        Due to a high-volume of expected interest your {{registrationTerm}} session will expire after\n        {{pluralConditional(viewModel.TimeoutMinutes, 'a minute', viewModel.TimeoutMinutes + ' minutes')}}\n        of inactivity.\n    </Alert>\n    <Alert v-if=\"numberToAddToWaitlist === numberOfRegistrants\" class=\"text-left\" alertType=\"warning\">\n        This {{registrationTerm}} has reached its capacity. Complete the registration below to be added to the waitlist.\n    </Alert>\n    <Alert v-else-if=\"numberToAddToWaitlist\" class=\"text-left\" alertType=\"warning\">\n        This {{registrationTerm}} only has capacity for {{remainingCapacityPhrase}}.\n        The first {{pluralConditional(viewModel.SpotsRemaining, registrantTerm, viewModel.SpotsRemaining + ' ' + registrantTermPlural)}} you add will be registered for {{viewModel.InstanceName}}.\n        The remaining {{pluralConditional(numberToAddToWaitlist, registrantTerm, numberToAddToWaitlist + ' ' + registrantTermPlural)}} will be added to the waitlist. \n    </Alert>\n    <div class=\"actions text-right\">\n        <RockButton btnType=\"primary\" @click=\"onNext\">\n            Next\n        </RockButton>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Intro.js.map