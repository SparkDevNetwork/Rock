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

import { defineComponent, inject } from "vue";
import Registrant from "./registrant.partial.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { RegistrantInfo, RegistrationEntryState } from "./types.partial";
import { smoothScrollToTop } from "@Obsidian/Utility/page";

export default defineComponent({
    name: "Event.RegistrationEntry.Registrants",
    components: {
        Registrant,
        NotificationBox
    },
    setup() {
        return {
            registrationEntryState: inject("registrationEntryState") as RegistrationEntryState,
            persistSession: inject("persistSession") as () => Promise<void>
        };
    },
    data() {
        return {
            hasCopiedCommonValues: false
        };
    },
    methods: {
        /** The event that handles when the user clicks to move to the previous registrant */
        async onPrevious() {
            if (this.registrationEntryState.currentRegistrantIndex <= 0) {
                this.$emit("previous");
                return;
            }

            const lastFormIndex = this.registrationEntryState.viewModel.registrantForms.length - 1;
            this.registrationEntryState.currentRegistrantIndex--;
            this.registrationEntryState.currentRegistrantFormIndex = lastFormIndex;
            await this.persistSession();

            // Wait for the form to be rendered and then scroll to the top.
            setTimeout(() => smoothScrollToTop(), 10);
        },

        /** The event that handles when the user clicks to move to the next registrant */
        async onNext() {
            const lastIndex = this.registrationEntryState.registrants.length - 1;

            if (this.registrationEntryState.currentRegistrantIndex >= lastIndex) {
                this.$emit("next");
                return;
            }

            // If the first registrant was just completed, then copy the common/shared values to other registrants
            if (this.registrationEntryState.currentRegistrantIndex === 0) {
                this.copyCommonValuesFromFirstRegistrant();
            }

            this.registrationEntryState.currentRegistrantIndex++;
            this.registrationEntryState.currentRegistrantFormIndex = 0;
            await this.persistSession();

            // Wait for the form to be rendered and then scroll to the top.
            setTimeout(() => smoothScrollToTop(), 10);
        },

        /** Copy the common values from the first registrant to the others */
        copyCommonValuesFromFirstRegistrant() {
            // Only copy one time
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
                            currentRegistrant.fieldValues[field.guid] = { ...valueToShare };
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
        /** Will some of the registrants have to be added to a waitlist */
        hasWaitlist(): boolean {
            return this.registrationEntryState.registrants.some(r => r.isOnWaitList);
        },

        /** Will this registrant be added to the waitlist? */
        isOnWaitlist(): boolean {
            const currentRegistrant = this.registrationEntryState.registrants[this.registrationEntryState.currentRegistrantIndex];
            return currentRegistrant.isOnWaitList;
        },

        /** What are the registrants called? */
        registrantTerm(): string {
            return (this.registrationEntryState.viewModel.registrantTerm || "registrant").toLowerCase();
        },

        registrants(): RegistrantInfo[] {
            return this.registrationEntryState.registrants;
        },
        currentRegistrantIndex(): number {
            return this.registrationEntryState.currentRegistrantIndex;
        }
    },
    template: `
<div class="registrationentry-registrant">
    <NotificationBox v-if="hasWaitlist && !isOnWaitlist" alertType="success">
        This {{registrantTerm}} will be fully registered.
    </NotificationBox>
    <NotificationBox v-else-if="isOnWaitlist" alertType="warning">
        This {{registrantTerm}} will be on the waiting list.
    </NotificationBox>
    <template v-for="(r, i) in registrants" :key="r.guid">
        <Registrant v-show="currentRegistrantIndex === i" :currentRegistrant="r" :isWaitList="isOnWaitlist" @next="onNext" @previous="onPrevious" />
    </template>
</div>`
});
