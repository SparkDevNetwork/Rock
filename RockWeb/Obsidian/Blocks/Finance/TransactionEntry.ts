import CampusPicker from '../../Controls/CampusPicker.js';
import DefinedValuePicker from '../../Controls/DefinedValuePicker.js';
import CurrencyBox from '../../Elements/CurrencyBox.js';
import { defineComponent } from '../../Vendor/Vue/vue.js';
import { FINANCIAL_FREQUENCY } from '../../SystemGuid/DefinedType.js';
import DatePicker from '../../Elements/DatePicker.js';
import RockButton from '../../Elements/RockButton.js';
import { Guid } from '../../Util/Guid.js';

export default defineComponent({
    name: 'Finance.TransactionEntry',
    components: {
        CurrencyBox,
        CampusPicker,
        DefinedValuePicker,
        DatePicker,
        RockButton
    },
    data() {
        return {
            frequencyDefinedTypeGuid: FINANCIAL_FREQUENCY,
            amounts: [null, null] as (number | null)[],
            campusGuid: null as Guid | null,
            frequencyTypeGuid: '' as Guid,
            giftDate: '2021-01-25'
        };
    },
    template: `
<div class="transaction-entry-v2">
    <h2>Your Generosity Changes Lives</h2>
    <CurrencyBox label="General Fund" v-model="amounts[0]" />
    <CurrencyBox label="Building Fund" v-model="amounts[1]" />
    <CampusPicker v-model="campusGuid" />
    <DefinedValuePicker :definedTypeGuid="frequencyDefinedTypeGuid" v-model="frequencyTypeGuid" />
    <DatePicker label="Process Gift On" v-model="giftDate" />
    <RockButton primary>
        Give Now
    </RockButton>
</div>`
});
