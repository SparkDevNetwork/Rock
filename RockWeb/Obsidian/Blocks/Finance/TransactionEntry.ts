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
import CampusPicker from '../../Controls/CampusPicker';
import DefinedValuePicker from '../../Controls/DefinedValuePicker';
import CurrencyBox from '../../Elements/CurrencyBox';
import { defineComponent, inject } from 'vue';
import { FINANCIAL_FREQUENCY } from '../../SystemGuid/DefinedType';
import DatePicker from '../../Elements/DatePicker';
import RockButton from '../../Elements/RockButton';
import { areEqual, Guid, newGuid } from '../../Util/Guid';
import Alert from '../../Elements/Alert';
import { asFormattedString } from '../../Services/Number';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock';
import { ConfigurationValues } from '../../Index';
import Toggle from '../../Elements/Toggle';
import Person from '../../ViewModels/CodeGenerated/PersonViewModel';
import store from '../../Store/Index';
import TextBox from '../../Elements/TextBox';
import FinancialAccount from '../../ViewModels/CodeGenerated/FinancialAccountViewModel';
import { asCommaAnd } from '../../Services/String';
import Campus from '../../ViewModels/CodeGenerated/CampusViewModel';
import RockDate, { RockDateType } from '../../Util/RockDate';
import GatewayControl, { GatewayControlModel } from '../../Controls/GatewayControl';
import RockValidation from '../../Controls/RockValidation';

export type ProcessTransactionArgs = {
    IsGivingAsPerson: boolean;
    Email: string;
    PhoneNumber: string;
    PhoneCountryCode: string;
    AccountAmounts: Record<Guid, number>;
    Street1: string;
    Street2: string;
    City: string;
    State: string;
    PostalCode: string;
    Country: string;
    FirstName: string;
    LastName: string;
    BusinessName: string;
    FinancialPersonSavedAccountGuid: Guid | null;
    Comment: string;
    TransactionEntityId: number | null;
    ReferenceNumber: string;
    CampusGuid: Guid | null;
    BusinessGuid: Guid | null;
    FrequencyValueGuid: Guid;
    GiftDate: RockDateType;
    IsGiveAnonymously: boolean;
};

export default defineComponent({
    name: 'Finance.TransactionEntry',
    components: {
        CurrencyBox,
        CampusPicker,
        DefinedValuePicker,
        DatePicker,
        RockButton,
        Alert,
        Toggle,
        TextBox,
        GatewayControl,
        RockValidation
    },
    setup() {
        return {
            invokeBlockAction: inject('invokeBlockAction') as InvokeBlockActionFunc,
            configurationValues: inject('configurationValues') as ConfigurationValues
        };
    },
    data() {
        return {
            loading: false,
            gatewayErrorMessage: '',
            gatewayValidationFields: {} as Record<string, string>,
            transactionGuid: newGuid(),
            criticalError: '',
            doGatewayControlSubmit: false,
            pageIndex: 1,
            page1Error: '',
            frequencyDefinedTypeGuid: FINANCIAL_FREQUENCY,
            args: {
                IsGivingAsPerson: true,
                Email: '',
                PhoneNumber: '',
                PhoneCountryCode: '',
                AccountAmounts: {},
                Street1: '',
                Street2: '',
                City: '',
                State: '',
                PostalCode: '',
                Country: '',
                FirstName: '',
                LastName: '',
                BusinessName: '',
                FinancialPersonSavedAccountGuid: null,
                Comment: '',
                TransactionEntityId: null,
                ReferenceNumber: '',
                CampusGuid: '',
                BusinessGuid: null,
                FrequencyValueGuid: '',
                GiftDate: RockDate.newDate(),
                IsGiveAnonymously: false
            } as ProcessTransactionArgs
        };
    },
    computed: {
        totalAmount(): number {
            let total = 0;

            for (const accountGuid in this.args.AccountAmounts) {
                total += this.args.AccountAmounts[accountGuid];
            }

            return total;
        },
        totalAmountFormatted(): string {
            return `$${asFormattedString(this.totalAmount)}`;
        },
        gatewayControlModel(): GatewayControlModel {
            return this.configurationValues['GatewayControl'] as GatewayControlModel;
        },
        currentPerson(): Person | null {
            return store.state.currentPerson;
        },
        accounts(): FinancialAccount[] {
            return this.configurationValues['FinancialAccounts'] as FinancialAccount[] || [];
        },
        campus(): Campus | null {
            return store.getters['campuses/getByGuid'](this.args.CampusGuid) || null;
        },
        accountAndCampusString(): string {
            const accountNames = [] as string[];

            for (const accountGuid in this.args.AccountAmounts) {
                const account = this.accounts.find(a => areEqual(accountGuid, a.Guid));

                if (!account || !account.PublicName) {
                    continue;
                }

                accountNames.push(account.PublicName);
            }

            if (this.campus) {
                return `${asCommaAnd(accountNames)} - ${this.campus.Name}`;
            }

            return asCommaAnd(accountNames);
        }
    },
    methods: {
        goBack() {
            this.pageIndex--;
            this.doGatewayControlSubmit = false;
        },
        onPageOneSubmit() {
            if (this.totalAmount <= 0) {
                this.page1Error = 'Please specify an amount';
                return;
            }

            this.page1Error = '';
            this.pageIndex = 2;
        },

        /** This is the handler for submitting the page with the gateway control on it. This method tells
         *  the gateway control to tokenize the input. Once tokenization is complete, then gateway success,
         *  error, or validation handlers will be invoked. */
        onPageTwoSubmit() {
            this.loading = true;
            this.gatewayErrorMessage = '';
            this.gatewayValidationFields = {};
            this.doGatewayControlSubmit = true;
        },

        /**
         * The gateway indicated success and returned a token
         * @param token
         */
        onGatewayControlSuccess(token: string) {
            this.loading = false;
            this.args.ReferenceNumber = token;
            this.pageIndex = 3;
        },

        /**
         * The gateway indicated an error
         * @param message
         */
        onGatewayControlError(message: string) {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayErrorMessage = message;
        },

        /**
         * The gateway wants the user to fix some fields
         * @param invalidFields
         */
        onGatewayControlValidation(invalidFields: Record<string, string>) {
            this.doGatewayControlSubmit = false;
            this.loading = false;
            this.gatewayValidationFields = invalidFields;
        },
        async onPageThreeSubmit() {
            this.loading = true;

            try {
                await this.invokeBlockAction('ProcessTransaction', {
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
            handler() {
                if (!this.currentPerson) {
                    return;
                }

                this.args.FirstName = this.args.FirstName || this.currentPerson.FirstName || '';
                this.args.LastName = this.args.LastName || this.currentPerson.LastName || '';
                this.args.Email = this.args.Email || this.currentPerson.Email || '';
            }
        }
    },
    template: `
<div class="transaction-entry-v2">
    <Alert v-if="criticalError" danger>
        {{criticalError}}
    </Alert>
    <template v-else-if="!gatewayControlModel || !gatewayControlModel.FileUrl">
        <h4>Welcome to Rock's On-line Giving Experience</h4>
        <p>
            There is currently no gateway configured.
        </p>
    </template>
    <template v-else-if="pageIndex === 1">
        <h2>Your Generosity Changes Lives (Vue)</h2>
        <template v-for="account in accounts">
            <CurrencyBox :label="account.PublicName" v-model="args.AccountAmounts[account.Guid]" />
        </template>
        <CampusPicker v-model="args.CampusGuid" :showBlankItem="false" />
        <DefinedValuePicker :definedTypeGuid="frequencyDefinedTypeGuid" v-model="args.FrequencyValueGuid" label="Frequency" :showBlankItem="false" />
        <DatePicker label="Process Gift On" v-model="args.GiftDate" />
        <Alert alertType="validation" v-if="page1Error">{{page1Error}}</Alert>
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
            <Alert v-if="gatewayErrorMessage" alertType="danger">{{gatewayErrorMessage}}</Alert>
            <RockValidation :errors="gatewayValidationFields" />
            <div class="hosted-payment-control">
                <GatewayControl
                    :gatewayControlModel="gatewayControlModel"
                    :submit="doGatewayControlSubmit"
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
        <Toggle v-model="args.IsGivingAsPerson">
            <template #on>Individual</template>
            <template #off>Business</template>
        </Toggle>
        <template v-if="args.IsGivingAsPerson && currentPerson">
            <div class="form-control-static">
                {{currentPerson.FullName}}
            </div>
        </template>
        <template v-else-if="args.IsGivingAsPerson">
            <TextBox v-model="args.FirstName" placeholder="First Name" class="margin-b-sm" />
            <TextBox v-model="args.LastName" placeholder="Last Name" class="margin-b-sm" />
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
