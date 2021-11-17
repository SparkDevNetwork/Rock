System.register(["vue", "../../../Controls/dialog", "../../../Elements/loadingIndicator", "../../../Elements/rockButton", "../../../Services/number", "../../../Services/string", "../../../Util/rockDateTime"], function (exports_1, context_1) {
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
    var vue_1, dialog_1, loadingIndicator_1, rockButton_1, number_1, string_1, rockDateTime_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (dialog_1_1) {
                dialog_1 = dialog_1_1;
            },
            function (loadingIndicator_1_1) {
                loadingIndicator_1 = loadingIndicator_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (number_1_1) {
                number_1 = number_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            },
            function (rockDateTime_1_1) {
                rockDateTime_1 = rockDateTime_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.SessionRenewal",
                components: {
                    Dialog: dialog_1.default,
                    LoadingIndicator: loadingIndicator_1.default,
                    RockButton: rockButton_1.default
                },
                props: {
                    isSessionExpired: {
                        type: Boolean,
                        required: true
                    }
                },
                setup() {
                    return {
                        registrationEntryState: vue_1.inject("registrationEntryState"),
                        invokeBlockAction: vue_1.inject("invokeBlockAction")
                    };
                },
                data() {
                    return {
                        spotsSecured: null,
                        isLoading: false,
                        isModalVisible: false
                    };
                },
                computed: {
                    hasWaitlist() {
                        return this.registrationEntryState.viewModel.waitListEnabled;
                    },
                    allRegistrantCount() {
                        return this.registrationEntryState.registrants.length;
                    },
                    waitlistRegistrantCount() {
                        return this.registrationEntryState.registrants.filter(r => r.isOnWaitList).length;
                    },
                    waitlistRegistrantCountWord() {
                        return number_1.toWord(this.waitlistRegistrantCount);
                    },
                    nonWaitlistRegistrantCount() {
                        return this.registrationEntryState.registrants.filter(r => !r.isOnWaitList).length;
                    },
                    nonWaitlistRegistrantCountWord() {
                        return number_1.toWord(this.nonWaitlistRegistrantCount);
                    }
                },
                methods: {
                    pluralConditional: string_1.pluralConditional,
                    restart() {
                        this.isLoading = true;
                        location.reload();
                    },
                    close() {
                        this.isModalVisible = false;
                        this.$nextTick(() => {
                            this.spotsSecured = null;
                            this.isLoading = false;
                        });
                    },
                    requestRenewal() {
                        var _a;
                        return __awaiter(this, void 0, void 0, function* () {
                            this.spotsSecured = 0;
                            this.isLoading = true;
                            try {
                                const response = yield this.invokeBlockAction("TryToRenewSession", {
                                    registrationSessionGuid: this.registrationEntryState.registrationSessionGuid
                                });
                                if (response.isSuccess && response.data) {
                                    const asDate = rockDateTime_1.RockDateTime.parseISO(response.data.expirationDateTime);
                                    this.registrationEntryState.sessionExpirationDateMs = (_a = asDate === null || asDate === void 0 ? void 0 : asDate.toMilliseconds()) !== null && _a !== void 0 ? _a : null;
                                    this.spotsSecured = response.data.spotsSecured;
                                }
                                else {
                                    this.registrationEntryState.sessionExpirationDateMs = null;
                                    this.spotsSecured = 0;
                                }
                                let deficiency = this.nonWaitlistRegistrantCount - this.spotsSecured;
                                if (!deficiency) {
                                    this.$emit("success");
                                    this.close();
                                    return;
                                }
                                this.registrationEntryState.viewModel.spotsRemaining = this.spotsSecured;
                                if (!this.hasWaitlist) {
                                    this.registrationEntryState.registrants.length = this.spotsSecured;
                                    return;
                                }
                                for (let i = this.allRegistrantCount - 1; i >= 0; i--) {
                                    if (!deficiency) {
                                        break;
                                    }
                                    const registrant = this.registrationEntryState.registrants[i];
                                    if (registrant.isOnWaitList) {
                                        continue;
                                    }
                                    registrant.isOnWaitList = true;
                                    deficiency--;
                                }
                            }
                            finally {
                                this.isLoading = false;
                            }
                        });
                    }
                },
                watch: {
                    isSessionExpired() {
                        if (this.isSessionExpired) {
                            this.spotsSecured = null;
                            this.isModalVisible = true;
                        }
                    }
                },
                template: `
<Dialog :modelValue="isModalVisible" :dismissible="false">
    <template #header>
        <h4 v-if="isLoading || spotsSecured === null">Registration Timed Out</h4>
        <h4 v-else>Request Extension</h4>
    </template>
    <template #default>
        <LoadingIndicator v-if="isLoading" />
        <template v-else-if="hasWaitlist && spotsSecured === 0">
            Due to high demand there is no longer space available.
            You can continue, but your registrants will be placed on the waitlist.
            Do you wish to continue with the registration?
        </template>
        <template v-else-if="spotsSecured === 0">
            Due to high demand there is no longer space available for this registration.
        </template>
        <template v-else-if="hasWaitlist && spotsSecured !== null">
            Due to high demand there is no longer space available for all your registrants.
            Your last {{waitlistRegistrantCountWord}}
            {{pluralConditional(waitlistRegistrantCount, 'registrant', ' registrants')}}
            will be placed on the waitlist.
            Do you wish to continue with the registration?
        </template>
        <template v-else-if="spotsSecured !== null">
            Due to high demand there is no longer space available for all your registrants.
            Only {{nonWaitlistRegistrantCountWord}} {{pluralConditional(nonWaitlistRegistrantCount, 'spot is', 'spots are')}} available.
            Your registration has been updated to only allow
            {{nonWaitlistRegistrantCountWord}} {{pluralConditional(nonWaitlistRegistrantCount, 'registrant', 'registrants')}}. 
            Do you wish to continue with the registration?
        </template>
        <template v-else>
            Your registration has timed out. Do you wish to request an extension in time?
        </template>
    </template>
    <template v-if="!isLoading" #footer>
        <template v-if="!hasWaitlist && spotsSecured === 0">
            <RockButton btnType="link" @click="restart">Close</RockButton>
        </template>
        <template v-else-if="spotsSecured !== null">
            <RockButton btnType="primary" @click="close">Yes</RockButton>
            <RockButton btnType="link" @click="restart">No, cancel registration</RockButton>
        </template>
        <template v-else>
            <RockButton btnType="primary" @click="requestRenewal">Yes</RockButton>
            <RockButton btnType="link" @click="restart">No, cancel registration</RockButton>
        </template>
    </template>
</Dialog>`
            }));
        }
    };
});
//# sourceMappingURL=sessionRenewal.js.map