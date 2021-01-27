import CampusPicker from '../../Controls/CampusPicker.js';
import DefinedValuePicker from '../../Controls/DefinedValuePicker.js';
import CurrencyBox from '../../Elements/CurrencyBox.js';
import { Component, defineComponent, inject, markRaw } from '../../Vendor/Vue/vue.js';
import { FINANCIAL_FREQUENCY } from '../../SystemGuid/DefinedType.js';
import DatePicker from '../../Elements/DatePicker.js';
import RockButton from '../../Elements/RockButton.js';
import { Guid } from '../../Util/Guid.js';
import Alert from '../../Elements/Alert.js';
import { asFormattedString } from '../../Filters/Number.js';
import { BlockAction } from '../../Controls/RockBlock.js';
import { BlockSettings } from '../../Index.js';

export default defineComponent({
    name: 'Finance.TransactionEntry',
    components: {
        CurrencyBox,
        CampusPicker,
        DefinedValuePicker,
        DatePicker,
        RockButton,
        Alert
    },
    setup() {
        return {
            blockAction: inject('blockAction') as BlockAction,
            blockSettings: inject('blockSettings') as BlockSettings
        };
    },
    data() {
        return {
            criticalError: '',
            token: '',
            doGatewayControlSubmit: false,
            pageIndex: 1,
            page1Error: '',
            frequencyDefinedTypeGuid: FINANCIAL_FREQUENCY,
            amounts: [null, null] as (number | null)[],
            campusGuid: '' as Guid,
            frequencyDefinedValueGuid: '' as Guid,
            giftDate: '2021-01-25',
            gatewayControl: null as Component | null
        };
    },
    computed: {
        totalAmount(): number {
            if (!this.amounts) {
                return 0;
            }

            let total = 0;

            for (const amount of this.amounts) {
                total += (amount || 0);
            }

            return total;
        },
        totalAmountFormatted(): string {
            return `$${asFormattedString(this.totalAmount)}`;
        },
        gatewayControlSettings(): unknown {
            const blockSettings = this.blockSettings || {};
            return blockSettings['GatewayControlSettings'] || {};
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
        receiveToken(token: string) {
            this.token = token;
            this.pageIndex = 3;
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
    <template v-else-if="pageIndex === 1">
        <h2>Your Generosity Changes Lives</h2>
        <CurrencyBox label="General Fund" v-model="amounts[0]" />
        <CurrencyBox label="Building Fund" v-model="amounts[1]" />
        <CampusPicker v-model="campusGuid" :showBlankItem="false" />
        <DefinedValuePicker :definedTypeGuid="frequencyDefinedTypeGuid" v-model="frequencyDefinedValueGuid" label="Frequency" :showBlankItem="false" />
        <DatePicker label="Process Gift On" v-model="giftDate" />
        <Alert validation v-if="page1Error">{{page1Error}}</Alert>
        <RockButton primary @click="onPageOneSubmit">Give Now</RockButton>
    </template>
    <template v-else-if="pageIndex === 2">
        <div class="amount-summary">
            <div class="amount-summary-text">
                <span class="account-names">General Fund</span>
                -
                <span class="account-campus">Main Campus</span>
            </div>
            <div class="amount-display">
                {{totalAmountFormatted}}
            </div>
        </div>
        <div>
            <div class="hosted-payment-control">
                <component :is="gatewayControl" :settings="gatewayControlSettings" :submit="doGatewayControlSubmit" @token="receiveToken" />
            </div>
            <div class="navigation actions">
                <RockButton default @click="goBack">Back</RockButton>
                <RockButton primary class="pull-right" @click="onPageTwoSubmit">Next</RockButton>
            </div>
        </div>
    </template>
</div>`
});
