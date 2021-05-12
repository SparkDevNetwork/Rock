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
import Location from '../ViewModels/CodeGenerated/LocationViewModel';

export interface AddressControlModel {
    Street1: string;
    Street2: string;
    City: string;
    State: string;
    PostalCode: string;
    Country: string;
}

export function getDefaultAddressControlModel() {
    return {
        Street1: '',
        Street2: '',
        City: '',
        State: 'AZ',
        PostalCode: '',
        Country: 'US'
    } as AddressControlModel;
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
            state: '',
            uniqueId: `rock-addresscontrol-${newGuid()}`,
            stateOptions: [
                { key: 'AL', value: 'AL', text: 'AL' },
                { key: 'AK', value: 'AK', text: 'AK' },
                { key: 'AS', value: 'AS', text: 'AS' },
                { key: 'AZ', value: 'AZ', text: 'AZ' },
                { key: 'AR', value: 'AR', text: 'AR' },
                { key: 'CA', value: 'CA', text: 'CA' },
                { key: 'CO', value: 'CO', text: 'CO' },
                { key: 'CT', value: 'CT', text: 'CT' },
                { key: 'DE', value: 'DE', text: 'DE' },
                { key: 'DC', value: 'DC', text: 'DC' },
                { key: 'FM', value: 'FM', text: 'FM' },
                { key: 'FL', value: 'FL', text: 'FL' },
                { key: 'GA', value: 'GA', text: 'GA' },
                { key: 'GU', value: 'GU', text: 'GU' },
                { key: 'HI', value: 'HI', text: 'HI' },
                { key: 'ID', value: 'ID', text: 'ID' },
                { key: 'IL', value: 'IL', text: 'IL' },
                { key: 'IN', value: 'IN', text: 'IN' },
                { key: 'IA', value: 'IA', text: 'IA' },
                { key: 'KS', value: 'KS', text: 'KS' },
                { key: 'KY', value: 'KY', text: 'KY' },
                { key: 'LA', value: 'LA', text: 'LA' },
                { key: 'ME', value: 'ME', text: 'ME' },
                { key: 'MH', value: 'MH', text: 'MH' },
                { key: 'MD', value: 'MD', text: 'MD' },
                { key: 'MA', value: 'MA', text: 'MA' },
                { key: 'MI', value: 'MI', text: 'MI' },
                { key: 'MN', value: 'MN', text: 'MN' },
                { key: 'MS', value: 'MS', text: 'MS' },
                { key: 'MO', value: 'MO', text: 'MO' },
                { key: 'MT', value: 'MT', text: 'MT' },
                { key: 'NE', value: 'NE', text: 'NE' },
                { key: 'NV', value: 'NV', text: 'NV' },
                { key: 'NH', value: 'NH', text: 'NH' },
                { key: 'NJ', value: 'NJ', text: 'NJ' },
                { key: 'NM', value: 'NM', text: 'NM' },
                { key: 'NY', value: 'NY', text: 'NY' },
                { key: 'NC', value: 'NC', text: 'NC' },
                { key: 'ND', value: 'ND', text: 'ND' },
                { key: 'MP', value: 'MP', text: 'MP' },
                { key: 'OH', value: 'OH', text: 'OH' },
                { key: 'OK', value: 'OK', text: 'OK' },
                { key: 'OR', value: 'OR', text: 'OR' },
                { key: 'PW', value: 'PW', text: 'PW' },
                { key: 'PA', value: 'PA', text: 'PA' },
                { key: 'PR', value: 'PR', text: 'PR' },
                { key: 'RI', value: 'RI', text: 'RI' },
                { key: 'SC', value: 'SC', text: 'SC' },
                { key: 'SD', value: 'SD', text: 'SD' },
                { key: 'TN', value: 'TN', text: 'TN' },
                { key: 'TX', value: 'TX', text: 'TX' },
                { key: 'UT', value: 'UT', text: 'UT' },
                { key: 'VT', value: 'VT', text: 'VT' },
                { key: 'VI', value: 'VI', text: 'VI' },
                { key: 'VA', value: 'VA', text: 'VA' },
                { key: 'WA', value: 'WA', text: 'WA' },
                { key: 'WV', value: 'WV', text: 'WV' },
                { key: 'WI', value: 'WI', text: 'WI' },
                { key: 'WY', value: 'WY', text: 'WY' }
            ] as DropDownListOption[]
        };
    },
    computed: {
        isRequired(): boolean {
            const rules = ruleStringToArray(this.rules);
            return rules.indexOf('required') !== -1;
        }
    },
    template: `
<div class="form-group address-control" :class="isRequired ? 'required' : ''">
    <RockLabel v-if="label || help" :for="uniqueId" :help="help">
        {{label}}
    </RockLabel>
    <div class="control-wrapper">
        <TextBox placeholder="Address Line 1" :rules="rules" v-model="modelValue.Street1" validationTitle="Address Line 1" />
        <TextBox placeholder="Address Line 2" v-model="modelValue.Street2" validationTitle="Address Line 2" />
        <div class="form-row">
            <TextBox placeholder="City" :rules="rules" v-model="modelValue.City" class="col-sm-6" validationTitle="City" />
            <DropDownList :showBlankItem="false" v-model="modelValue.State" class="col-sm-3" :options="stateOptions" />
            <TextBox placeholder="Zip" :rules="rules" v-model="modelValue.PostalCode" class="col-sm-3" validationTitle="Zip" />
        </div>
    </div>
</div>`
});
