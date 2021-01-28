import CampusPicker from '../../Controls/CampusPicker.js';
import DefinedValuePicker from '../../Controls/DefinedValuePicker.js';
import CurrencyBox from '../../Elements/CurrencyBox.js';
import { Component, defineComponent, inject, markRaw } from '../../Vendor/Vue/vue.js';
import { FINANCIAL_FREQUENCY } from '../../SystemGuid/DefinedType.js';
import DatePicker from '../../Elements/DatePicker.js';
import RockButton from '../../Elements/RockButton.js';
import { areEqual, Guid, newGuid } from '../../Util/Guid.js';
import Alert from '../../Elements/Alert.js';
import { asFormattedString } from '../../Filters/Number.js';
import { BlockAction } from '../../Controls/RockBlock.js';
import { BlockSettings } from '../../Index.js';
import Toggle from '../../Elements/Toggle.js';
import Person from '../../ViewModels/CodeGenerated/PersonViewModel.js';
import store from '../../Store/Index.js';
import TextBox from '../../Elements/TextBox.js';
import FinancialAccount from '../../ViewModels/CodeGenerated/FinancialAccountViewModel.js';
import { toDatePickerValue } from '../../Filters/Date.js';
import { asCommaAnd } from '../../Filters/String.js';
import Campus from '../../ViewModels/CodeGenerated/CampusViewModel.js';

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
    GiftDate: Date | string;
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
        TextBox
    },
    setup() {
        return {
            blockAction: inject('blockAction') as BlockAction,
            blockSettings: inject('blockSettings') as BlockSettings
        };
    },
    data() {
        return {
            transactionGuid: newGuid(),
            criticalError: '',
            doGatewayControlSubmit: false,
            pageIndex: 1,
            page1Error: '',
            frequencyDefinedTypeGuid: FINANCIAL_FREQUENCY,
            gatewayControl: null as Component | null,
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
                GiftDate: toDatePickerValue(new Date()),
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
        gatewayControlSettings(): unknown {
            const blockSettings = this.blockSettings || {};
            return blockSettings['GatewayControlSettings'] || {};
        },
        currentPerson(): Person | null {
            return store.state.currentPerson;
        },
        accounts(): FinancialAccount[] {
            return this.blockSettings['FinancialAccounts'] as FinancialAccount[] || [];
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
        },
        onPageOneSubmit() {
            if (this.totalAmount <= 0) {
                this.page1Error = 'Please specify an amount';
                return;
            }

            this.page1Error = '';
            this.pageIndex = 2;
        },
        onPageTwoSubmit() {
            this.doGatewayControlSubmit = true;
        },
        onGatewayControlDone() {
            this.pageIndex = 3;
        },
        async onPageThreeSubmit() {
            await this.blockAction('ProcessTransaction', {
                args: this.args,
                transactionGuid: this.transactionGuid
            });
            this.pageIndex = 4;
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
    async created() {
        const controlPath = this.blockSettings['GatewayControlFileUrl'] as string | null;

        if (controlPath) {
            const controlComponentModule = await import(controlPath);
            const gatewayControl = controlComponentModule ?
                (controlComponentModule.default || controlComponentModule) :
                null;

            if (gatewayControl) {
                this.gatewayControl = markRaw(gatewayControl);
            }
        }

        if (!this.gatewayControl) {
            this.criticalError = 'Could not find the correct gateway control';
        }
    },
    template: `
<div class="transaction-entry-v2">
    <Alert v-if="criticalError" danger>
        {{criticalError}}
    </Alert>
    <template v-else-if="!gatewayControl">
        <h4>Welcome to Rock's On-line Giving Experience</h4>
        <p>
            There is currently no gateway configured.
        </p>
    </template>
    <template v-else-if="pageIndex === 1">
        <h2>Your Generosity Changes Lives</h2>
        <template v-for="account in accounts">
            <CurrencyBox :label="account.PublicName" v-model="args.AccountAmounts[account.Guid]" />
        </template>
        <CampusPicker v-model="args.CampusGuid" :showBlankItem="false" />
        <DefinedValuePicker :definedTypeGuid="frequencyDefinedTypeGuid" v-model="args.FrequencyValueGuid" label="Frequency" :showBlankItem="false" />
        <DatePicker label="Process Gift On" v-model="args.GiftDate" />
        <Alert validation v-if="page1Error">{{page1Error}}</Alert>
        <RockButton primary @click="onPageOneSubmit">Give Now</RockButton>
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
            <div class="hosted-payment-control">
                <component :is="gatewayControl" :settings="gatewayControlSettings" :submit="doGatewayControlSubmit" :args="args" @done="onGatewayControlDone" />
            </div>
            <div class="navigation actions">
                <RockButton default @click="goBack" :disabled="doGatewayControlSubmit">Back</RockButton>
                <RockButton primary class="pull-right" @click="onPageTwoSubmit" :disabled="doGatewayControlSubmit">Next</RockButton>
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
            <RockButton default @click="goBack">Back</RockButton>
            <RockButton primary class="pull-right" @click="onPageThreeSubmit">Finish</RockButton>
        </div>
    </template>
</div>`
});
