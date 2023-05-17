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

import { Guid } from "@Obsidian/Types";
import DropDownList from "@Obsidian/Controls/dropDownList";
import CurrencyBox from "@Obsidian/Controls/currencyBox";
import { defineComponent } from "vue";
import DatePicker from "@Obsidian/Controls/datePicker.obs";
import RockButton from "@Obsidian/Controls/rockButton";
import { newGuid } from "@Obsidian/Utility/guid";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { asFormattedString } from "@Obsidian/Utility/numberUtils";
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import Toggle from "@Obsidian/Controls/toggle";
import { useStore } from "@Obsidian/PageState";
import TextBox from "@Obsidian/Controls/textBox";
import { asCommaAnd } from "@Obsidian/Utility/stringUtils";
import GatewayControl from "@Obsidian/Controls/gatewayControl";
import { provideSubmitPayment } from "@Obsidian/Core/Controls/financialGateway";
import { GatewayControlBag } from "@Obsidian/ViewModels/Controls/gatewayControlBag";
import RockValidation from "@Obsidian/Controls/rockValidation";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import { FinancialAccountBag } from "@Obsidian/ViewModels/Entities/financialAccountBag";

const store = useStore();

export type ProcessTransactionArgs = {
    isGivingAsPerson: boolean;
    email: string;
    phoneNumber: string;
    phoneCountryCode: string;
    accountAmounts: Record<string, number>;
    street1: string;
    street2: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
    firstName: string;
    lastName: string;
    businessName: string;
    financialPersonSavedAccountGuid: Guid | null;
    comment: string;
    transactionEntityId: number | null;
    referenceNumber: string;
    campusGuid: Guid | null;
    businessGuid: Guid | null;
    frequencyValueGuid: Guid;
    giftDate: string;
    isGiveAnonymously: boolean;
};

export default defineComponent({
    name: "Finance.TransactionEntry",

    components: {
        CurrencyBox,
        DropDownList,
        DatePicker,
        RockButton,
        NotificationBox,
        Toggle,
        TextBox,
        GatewayControl,
        RockValidation
    },

    setup() {
        const submitPayment = provideSubmitPayment();

        return {
            submitPayment,
            invokeBlockAction: useInvokeBlockAction(),
            configurationValues: useConfigurationValues<Record<string, unknown>>()
        };
    },

    data() {
        const configurationValues = useConfigurationValues<Record<string, unknown>>();
        const campuses = configurationValues["campuses"] as ListItemBag[] || [];
        const frequencies = configurationValues["frequencies"] as ListItemBag[] || [];

        return {
            loading: false,
            gatewayErrorMessage: "",
            gatewayValidationFields: {} as Record<string, string>,
            transactionGuid: newGuid(),
            criticalError: "",
            pageIndex: 1,
            page1Error: "",
            args: {
                isGivingAsPerson: true,
                email: "",
                phoneNumber: "",
                phoneCountryCode: "",
                accountAmounts: {},
                street1: "",
                street2: "",
                city: "",
                state: "",
                postalCode: "",
                country: "",
                firstName: "",
                lastName: "",
                businessName: "",
                financialPersonSavedAccountGuid: null,
                comment: "",
                transactionEntityId: null,
                referenceNumber: "",
                campusGuid: campuses.length > 0 ? campuses[0].value : "",
                businessGuid: null,
                frequencyValueGuid: frequencies.length > 0 ? frequencies[0].value : "",
                giftDate: RockDateTime.now().toASPString("yyyy-MM-dd"),
                isGiveAnonymously: false
            } as ProcessTransactionArgs
        };
    },

    computed: {
        totalAmount(): number {
            let total = 0;

            for (const accountKey in this.args.accountAmounts) {
                total += this.args.accountAmounts[accountKey];
            }

            return total;
        },

        totalAmountFormatted(): string {
            return `$${asFormattedString(this.totalAmount, 2)}`;
        },

        gatewayControlModel(): GatewayControlBag {
            return this.configurationValues["gatewayControl"] as GatewayControlBag;
        },

        currentPerson(): PersonBag | null {
            return store.state.currentPerson;
        },

        currentPersonFullName(): string | null {
            const currentPerson = this.currentPerson;

            if (currentPerson === null) {
                return null;
            }

            return `${currentPerson.nickName ?? ""} ${currentPerson.lastName ?? ""}`;
        },

        accounts(): FinancialAccountBag[] {
            return this.configurationValues["financialAccounts"] as FinancialAccountBag[] || [];
        },

        campuses(): ListItemBag[] {
            return this.configurationValues["campuses"] as ListItemBag[] || [];
        },

        frequencies(): ListItemBag[] {
            return this.configurationValues["frequencies"] as ListItemBag[] || [];
        },

        campusName(): string | null {
            if (this.args.campusGuid === null) {
                return null;
            }

            const matchedCampuses = this.campuses.filter(c => c.value === this.args.campusGuid);

            return matchedCampuses.length >= 1 ? matchedCampuses[0].text ?? "" : null;
        },

        accountAndCampusString(): string {
            const accountNames = [] as string[];

            for (const accountKey in this.args.accountAmounts) {
                const account = this.accounts.find(a => a.idKey === accountKey);

                if (!account || !account.publicName) {
                    continue;
                }

                accountNames.push(account.publicName);
            }

            if (this.campusName) {
                return `${asCommaAnd(accountNames)} - ${this.campusName}`;
            }

            return asCommaAnd(accountNames);
        }
    },

    methods: {
        goBack(): void {
            this.pageIndex--;
        },

        onPageOneSubmit(): void {
            if (this.totalAmount <= 0) {
                this.page1Error = "Please specify an amount";
                return;
            }

            this.page1Error = "";
            this.pageIndex = 2;
        },

        /** This is the handler for submitting the page with the gateway control on it. This method tells
         *  the gateway control to tokenize the input. Once tokenization is complete, then gateway success,
         *  error, or validation handlers will be invoked. */
        onPageTwoSubmit(): void {
            this.loading = true;
            this.gatewayErrorMessage = "";
            this.gatewayValidationFields = {};
            this.submitPayment();
        },

        /**
         * The gateway indicated success and returned a token
         * @param token
         */
        onGatewayControlSuccess(token: string): void {
            this.loading = false;
            this.args.referenceNumber = token;
            this.pageIndex = 3;
        },

        /**
         * The gateway indicated an error
         * @param message
         */
        onGatewayControlError(message: string): void {
            this.loading = false;
            this.gatewayErrorMessage = message;
        },

        /**
         * The gateway wants the user to fix some fields
         * @param invalidFields
         */
        onGatewayControlValidation(invalidFields: Record<string, string>): void {
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },

        async onPageThreeSubmit(): Promise<void> {
            this.loading = true;

            try {
                await this.invokeBlockAction("ProcessTransaction", {
                    args: this.args,
                    transactionGuid: this.transactionGuid
                });
                this.pageIndex = 4;
            }
            catch (e) {
                console.log(e);
            }
            finally {
                this.loading = false;
            }
        }
    },

    watch: {
        currentPerson: {
            immediate: true,
            handler(): void {
                if (!this.currentPerson) {
                    return;
                }

                this.args.firstName = this.args.firstName || this.currentPerson.firstName || "";
                this.args.lastName = this.args.lastName || this.currentPerson.lastName || "";
                this.args.email = this.args.email || this.currentPerson.email || "";
            }
        }
    },

    template: `
<div class="transaction-entry-v2">
    <NotificationBox v-if="criticalError" danger>{{criticalError}}</NotificationBox>
    <template v-else-if="!gatewayControlModel || !gatewayControlModel.fileUrl">
        <h4>Welcome to Rock's On-line Giving Experience</h4>
        <p>
            There is currently no gateway configured.
        </p>
    </template>
    <template v-else-if="pageIndex === 1">
        <h2>Your Generosity Changes Lives (Vue)</h2>
        <template v-for="account in accounts">
            <CurrencyBox :label="account.publicName" v-model="args.accountAmounts[account.guid]" />
        </template>
        <DropDownList label="Campus" v-model="args.campusGuid" :showBlankItem="false" :items="campuses" />
        <DropDownList label="Frequency" v-model="args.frequencyValueGuid" :showBlankItem="false" :items="frequencies" />
        <DatePicker label="Process Gift On" v-model="args.giftDate" />
        <NotificationBox alertType="validation" v-if="page1Error">{{page1Error}}</NotificationBox>
        <RockButton btnType="primary" @click="onPageOneSubmit">Give Now</RockButton>
    </template>
    <template v-else-if="pageIndex === 2">
        <div class="amount-summary">
            <div class="amount-summary-text">
                {{accountAndCampusString}}
            </div>
            <div class="amount-display">
                {{totalAmountFormatted}}
            </div>
        </div>
        <div>
            <NotificationBox v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</NotificationBox>
            <RockValidation :errors="gatewayValidationFields" />
            <div class="hosted-payment-control">
                <GatewayControl
                    :gatewayControlModel="gatewayControlModel"
                    @success="onGatewayControlSuccess"
                    @error="onGatewayControlError"
                    @validation="onGatewayControlValidation" />
            </div>
            <div class="navigation actions">
                <RockButton btnType="default" @click="goBack" :isLoading="loading">Back</RockButton>
                <RockButton btnType="primary" class="pull-right" @click="onPageTwoSubmit" :isLoading="loading">Next</RockButton>
            </div>
        </div>
    </template>
    <template v-else-if="pageIndex === 3">
        <Toggle v-model="args.isGivingAsPerson">
            <template #on>Individual</template>
            <template #off>Business</template>
        </Toggle>
        <template v-if="args.isGivingAsPerson && currentPerson">
            <div class="form-control-static">
                {{currentPersonFullName}}
            </div>
        </template>
        <template v-else-if="args.isGivingAsPerson">
            <TextBox v-model="args.firstName" placeholder="First Name" class="margin-b-sm" />
            <TextBox v-model="args.lastName" placeholder="Last Name" class="margin-b-sm" />
        </template>
        <div class="navigation actions margin-t-md">
            <RockButton :isLoading="loading" @click="goBack">Back</RockButton>
            <RockButton :isLoading="loading" btnType="primary" class="pull-right" @click="onPageThreeSubmit">Finish</RockButton>
        </div>
    </template>
    <template v-else-if="pageIndex === 4">
        Last Page
    </template>
</div>`
});
