System.register(["vue", "./registrant", "../../../Elements/alert"], function (exports_1, context_1) {
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
    var vue_1, registrant_1, alert_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (registrant_1_1) {
                registrant_1 = registrant_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.Registrants",
                components: {
                    Registrant: registrant_1.default,
                    Alert: alert_1.default
                },
                setup() {
                    return {
                        registrationEntryState: vue_1.inject("registrationEntryState"),
                        persistSession: vue_1.inject("persistSession")
                    };
                },
                data() {
                    return {
                        hasCopiedCommonValues: false
                    };
                },
                methods: {
                    onPrevious() {
                        return __awaiter(this, void 0, void 0, function* () {
                            if (this.registrationEntryState.currentRegistrantIndex <= 0) {
                                this.$emit("previous");
                                return;
                            }
                            const lastFormIndex = this.registrationEntryState.viewModel.registrantForms.length - 1;
                            this.registrationEntryState.currentRegistrantIndex--;
                            this.registrationEntryState.currentRegistrantFormIndex = lastFormIndex;
                            yield this.persistSession();
                        });
                    },
                    onNext() {
                        return __awaiter(this, void 0, void 0, function* () {
                            const lastIndex = this.registrationEntryState.registrants.length - 1;
                            if (this.registrationEntryState.currentRegistrantIndex >= lastIndex) {
                                this.$emit("next");
                                return;
                            }
                            if (this.registrationEntryState.currentRegistrantIndex === 0) {
                                this.copyCommonValuesFromFirstRegistrant();
                            }
                            this.registrationEntryState.currentRegistrantIndex++;
                            this.registrationEntryState.currentRegistrantFormIndex = 0;
                            yield this.persistSession();
                        });
                    },
                    copyCommonValuesFromFirstRegistrant() {
                        if (this.hasCopiedCommonValues) {
                            return;
                        }
                        this.hasCopiedCommonValues = true;
                        const firstRegistrant = this.registrants[0];
                        for (let i = 1; i < this.registrants.length; i++) {
                            const currentRegistrant = this.registrants[i];
                            for (const form of this.registrationEntryState.viewModel.registrantForms) {
                                for (const field of form.fields) {
                                    if (!field.isSharedValue) {
                                        continue;
                                    }
                                    const valueToShare = firstRegistrant.fieldValues[field.guid];
                                    if (valueToShare && typeof valueToShare === "object") {
                                        currentRegistrant.fieldValues[field.guid] = Object.assign({}, valueToShare);
                                    }
                                    else {
                                        currentRegistrant.fieldValues[field.guid] = valueToShare;
                                    }
                                }
                            }
                        }
                    }
                },
                computed: {
                    hasWaitlist() {
                        return this.registrationEntryState.registrants.some(r => r.isOnWaitList);
                    },
                    isOnWaitlist() {
                        const currentRegistrant = this.registrationEntryState.registrants[this.registrationEntryState.currentRegistrantIndex];
                        return currentRegistrant.isOnWaitList;
                    },
                    registrantTerm() {
                        return (this.registrationEntryState.viewModel.registrantTerm || "registrant").toLowerCase();
                    },
                    registrants() {
                        return this.registrationEntryState.registrants;
                    },
                    currentRegistrantIndex() {
                        return this.registrationEntryState.currentRegistrantIndex;
                    }
                },
                template: `
<div class="registrationentry-registrant">
    <Alert v-if="hasWaitlist && !isOnWaitlist" alertType="success">
        This {{registrantTerm}} will be fully registered.
    </Alert>
    <Alert v-else-if="isOnWaitlist" alertType="warning">
        This {{registrantTerm}} will be on the waiting list.
    </Alert>
    <template v-for="(r, i) in registrants" :key="r.guid">
        <Registrant v-show="currentRegistrantIndex === i" :currentRegistrant="r" :isWaitList="isOnWaitlist" @next="onNext" @previous="onPrevious" />
    </template>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=registrants.js.map