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
import { ruleArrayToString, ruleStringToArray } from '../Rules/Index';
import DateKey from '../Services/DateKey';
import RockFormField from './RockFormField';

export interface BirthdayPickerModel {
    Year: number,
    Month: number;
    Day: number;
}

export function getDefaultBirthdayPickerModel() {
    return {
        Year: 0,
        Month: 0,
        Day: 0
    } as BirthdayPickerModel;
}

export default defineComponent({
    name: 'BirthdayPicker',
    components: {
        RockFormField
    },
    props: {
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        modelValue: {
            type: Object as PropType<BirthdayPickerModel>,
            required: true
        }
    },
    data() {
        const years: number[] = [];
        const count = 120;
        const currentYear = new Date().getFullYear();

        for (let i = 0; i <= count; i++) {
            years.push(currentYear - i);
        }

        return {
            years
        };
    },
    computed: {
        internalDateKey(): string {
            const dateKey = DateKey.toDateKey(this.modelValue.Year, this.modelValue.Month, this.modelValue.Day);
            return dateKey;
        },
        computedRules(): string {
            const rules = ruleStringToArray(this.rules);

            if (rules.indexOf('required') !== -1 && rules.indexOf('datekey') === -1) {
                rules.push('datekey');
            }

            return ruleArrayToString(rules);
        }
    },
    template: `
<RockFormField
    :modelValue="internalDateKey"
    formGroupClasses="birthday-picker"
    name="birthday"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <div class="form-control-group">
                <select :id="uniqueId + '-month'" class="form-control input-width-sm" :disabled="disabled" v-model="modelValue.Month">
                    <option value="0"></option>
                    <option value="1">Jan</option>
                    <option value="2">Feb</option>
                    <option value="3">Mar</option>
                    <option value="4">Apr</option>
                    <option value="5">May</option>
                    <option value="6">Jun</option>
                    <option value="7">Jul</option>
                    <option value="8">Aug</option>
                    <option value="9">Sep</option>
                    <option value="10">Oct</option>
                    <option value="11">Nov</option>
                    <option value="12">Dec</option>
                </select>
                <span class="separator">/</span>
                <select :id="uniqueId + '-day'" class="form-control input-width-sm" v-model="modelValue.Day">
                    <option value="0"></option>
                    <option value="1">1</option>
                    <option value="2">2</option>
                    <option value="3">3</option>
                    <option value="4">4</option>
                    <option value="5">5</option>
                    <option value="6">6</option>
                    <option value="7">7</option>
                    <option value="8">8</option>
                    <option value="9">9</option>
                    <option value="10">10</option>
                    <option value="11">11</option>
                    <option value="12">12</option>
                    <option value="13">13</option>
                    <option value="14">14</option>
                    <option value="15">15</option>
                    <option value="16">16</option>
                    <option value="17">17</option>
                    <option value="18">18</option>
                    <option value="19">19</option>
                    <option value="20">20</option>
                    <option value="21">21</option>
                    <option value="22">22</option>
                    <option value="23">23</option>
                    <option value="24">24</option>
                    <option value="25">25</option>
                    <option value="26">26</option>
                    <option value="27">27</option>
                    <option value="28">28</option>
                    <option value="29">29</option>
                    <option value="30">30</option>
                    <option value="31">31</option>
                </select>
                <span class="separator">/</span>
                <select :id="uniqueId + '-year'" class="form-control input-width-sm" v-model="modelValue.Year">
                    <option value="0"></option>
                    <option v-for="year in years" :value="year.toString()">{{year}}</option>
                </select>
            </div>
        </div>
    </template>
</RockFormField>`
});
