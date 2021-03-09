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
import { defineComponent, PropType } from 'vue';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';
import RockLabel from '../Elements/RockLabel';
import TextBox from '../Elements/TextBox';
import { ruleStringToArray } from '../Rules/Index';
import { newGuid } from '../Util/Guid';

export interface AddressControlModel {
    Street1: string;
    Street2: string;
    City: string;
    State: string;
    PostalCode: string;
}

export default defineComponent({
    name: 'AddressControl',
    components: {
        TextBox,
        RockLabel,
        DropDownList
    },
    props: {
        modelValue: {
            type: Object as PropType<AddressControlModel>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            default: 'Address'
        },
        help: {
            type: String as PropType<string>,
            default: ''
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        }
    },
    data() {
        return {
            uniqueId: `rock-addresscontrol-${newGuid()}`,
            states: [
                'AL',
                'AK',
                'AS',
                'AZ',
                'AR',
                'CA',
                'CO',
                'CT',
                'DE',
                'DC',
                'FM',
                'FL',
                'GA',
                'GU',
                'HI',
                'ID',
                'IL',
                'IN',
                'IA',
                'KS',
                'KY',
                'LA',
                'ME',
                'MH',
                'MD',
                'MA',
                'MI',
                'MN',
                'MS',
                'MO',
                'MT',
                'NE',
                'NV',
                'NH',
                'NJ',
                'NM',
                'NY',
                'NC',
                'ND',
                'MP',
                'OH',
                'OK',
                'OR',
                'PW',
                'PA',
                'PR',
                'RI',
                'SC',
                'SD',
                'TN',
                'TX',
                'UT',
                'VT',
                'VI',
                'VA',
                'WA',
                'WV',
                'WI',
                'WY'
            ]
        };
    },
    computed: {
        isRequired(): boolean {
            const rules = ruleStringToArray(this.rules);
            return rules.indexOf('required') !== -1;
        },
        stateOptions(): DropDownListOption[] {
            return this.states.map(s => ({
                key: s,
                value: s,
                text: s
            }));
        }
    },
    template: `
<div class="form-group address-control" :class="isRequired ? 'required' : ''">
    <RockLabel v-if="label || help" :for="uniqueId" :help="help">
        {{label}}
    </RockLabel>
    <div class="control-wrapper">
        <TextBox placeholder="Address Line 1" :rules="rules" v-model="modelValue.Street1" validationTitle="Address Line 1" />
        <TextBox placeholder="Address Line 2" :rules="rules" v-model="modelValue.Street2" validationTitle="Address Line 2" />
        <div class="form-row">
            <TextBox placeholder="City" :rules="rules" v-model="modelValue.City" class="col-sm-6" validationTitle="City" />
            <DropDownList placeholder="State" v-model="modelValue.State" class="col-sm-3" :options="stateOptions" :showBlankItem="true" :rules="rules" />
            <TextBox placeholder="Zip" :rules="rules" v-model="modelValue.PostalCode" class="col-sm-3" validationTitle="Zip" />
        </div>
    </div>
</div>`
});
