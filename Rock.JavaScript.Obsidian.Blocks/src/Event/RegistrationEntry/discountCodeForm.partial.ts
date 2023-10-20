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
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import RockButton from "@Obsidian/Controls/rockButton";
import TextBox from "@Obsidian/Controls/textBox";
import { asFormattedString } from "@Obsidian/Utility/numberUtils";
import { RegistrationEntryBlockViewModel, RegistrationEntryState, RegistrationEntryBlockArgs } from "./types.partial";

type CheckDiscountCodeResult = {
    discountCode: string;
    registrationUsagesRemaining: number | null;
    discountAmount: number;
    discountPercentage: number;
    discountMaxRegistrants: number;
};

export default defineComponent({
    name: "Event.RegistrationEntry.DiscountCodeForm",
    components: {
        RockButton,
        TextBox,
        NotificationBox
    },
    setup() {
        const getRegistrationEntryBlockArgs = inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs;
        return {
            invokeBlockAction: useInvokeBlockAction(),
            registrationEntryState: inject("registrationEntryState") as RegistrationEntryState,
            getRegistrationEntryBlockArgs

        };
    },
    mounted() {
        this.tryDiscountCode(true);
    },
    data() {
        return {
            /** Is there an AJAX call in-flight? */
            loading: false,

            /** The bound value to the discount code input */
            discountCodeInput: "",

            /** A warning message about the discount code that is a result of a failed AJAX call */
            discountCodeWarningMessage: "",

        /** The success message displayed once a discount code has been applied */
        discountCodeSuccessMessage: ""
        };
    },
    computed: {
        /** Should the discount panel be shown? */
        isDiscountPanelVisible(): boolean {
            return this.viewModel.hasDiscountsAvailable;
        },

        /** Disable the textbox and hide the apply button */
        isDiscountCodeAllowed(): boolean {
            const args = this.getRegistrationEntryBlockArgs();
            if (args.discountCode && args.discountCode.length > 0 && args.registrationGuid != null) {
                return false;
            }

            return true;
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        }
    },
    methods: {
        /** Send a user input discount code to the server so the server can check and send back
         *  the discount amount. */
        async tryDiscountCode(isAutoApply: boolean): Promise<void> {
            this.loading = true;
            try {
                const result = await this.invokeBlockAction<CheckDiscountCodeResult>("CheckDiscountCode", {
                    code: this.discountCodeInput,
                    registrantCount: this.registrationEntryState.registrants.length,
                    registrationGuid: this.viewModel.session?.registrationGuid ?? null,
                    isAutoApply: isAutoApply ?? false
                });

                if (result.isError || !result.data) {
                    if(!isAutoApply) {
                        if(result.errorMessage != null && result.errorMessage !="") {
                            this.discountCodeWarningMessage = result.errorMessage;
                        }
                        else if (this.discountCodeInput != "") {
                            this.discountCodeWarningMessage = `'${this.discountCodeInput}' is not a valid Discount Code.`;
                        }
                    }
                }
                else {
                    this.discountCodeWarningMessage = "";
                    this.discountCodeInput = this.discountCodeInput == "" ? result.data.discountCode : this.discountCodeInput;
                    this.registrationEntryState.discountAmount = result.data.discountAmount;
                    this.registrationEntryState.discountPercentage = result.data.discountPercentage;
                    this.registrationEntryState.discountCode = result.data.discountCode;
                    this.registrationEntryState.discountMaxRegistrants = result.data.discountMaxRegistrants;

                    if (!isAutoApply || this.registrationEntryState.registrants.some(r => !r.isOnWaitList)) {
                        this.discountCodeSuccessMessage = this.getDiscountCodeSuccessMessage();
                    }
                    else {
                        this.discountCodeSuccessMessage = "";
                    }
                }
            }
            finally {
                this.loading = false;
            }
        },

        getDiscountCodeSuccessMessage(): string {
            const discountAmount = this.registrationEntryState.discountAmount;
            const discountPercent = this.registrationEntryState.discountPercentage;
            const discountMaxRegistrants = this.registrationEntryState.discountMaxRegistrants ?? 0;
            const registrantCount = this.registrationEntryState.registrants.length;

            if (!discountPercent && !discountAmount) {
                return "";
            }

            const discountText = discountPercent ?
                `${asFormattedString(discountPercent * 100, 0)}%` :
                `$${asFormattedString(discountAmount, 2)}`;

            if(discountMaxRegistrants != 0 && registrantCount > discountMaxRegistrants) {
                const registrantTerm = discountMaxRegistrants == 1 ? "registrant" : "registrants";
                return `Your ${discountText} discount code was successfully applied to the maximum allowed number of ${discountMaxRegistrants} ${registrantTerm}`;
            }

            return `Your ${discountText} discount code for all registrants was successfully applied.`;
        }
    },
    watch: {
        "registrationEntryState.DiscountCode": {
            immediate: true,
            handler(): void {
                this.discountCodeInput = this.registrationEntryState.discountCode;
            }
        }
    },
    template: `
<div v-if="isDiscountPanelVisible || discountCodeInput" class="clearfix">
    <NotificationBox v-if="discountCodeWarningMessage" alertType="warning">{{discountCodeWarningMessage}}</NotificationBox>
    <NotificationBox v-if="discountCodeSuccessMessage" alertType="success">{{discountCodeSuccessMessage}}</NotificationBox>
    <div class="form-group pull-right">
        <label class="control-label">Discount Code</label>
        <div class="input-group">
            <input type="text" :disabled="loading || !isDiscountCodeAllowed" class="form-control input-width-md input-sm" v-model="discountCodeInput" />
            <RockButton v-if="isDiscountCodeAllowed" btnSize="sm" :isLoading="loading" class="margin-l-sm" @click="tryDiscountCode(false)">
                Apply
            </RockButton>
        </div>
    </div>
</div>`
});
