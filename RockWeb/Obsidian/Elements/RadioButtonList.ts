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
import { Guid } from '../Util/Guid';
import { DropDownListOption } from './DropDownList';
import RockFormField from './RockFormField';

export default defineComponent({
    name: 'RadioButtonList',
    components: {
        RockFormField
    },
    props: {
        options: {
            type: Array as PropType<DropDownListOption[]>,
            default: []
        },
        modelValue: {
            type: String as PropType<string>,
            default: ''
        }
    },
    emits: [
        'update:modelValue'
    ],
    data() {
        return {
            internalValue: ''
        };
    },
    methods: {
        getOptionUniqueId(uniqueId: Guid, option: DropDownListOption): string {
            return `${uniqueId}-${option.key}`;
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue;
            }
        }
    },
    template: `
<RockFormField formGroupClasses="rock-radio-button-list" #default="{uniqueId}" name="radiobuttonlist" v-model="internalValue">
    <div class="control-wrapper">
        <div class="controls rockradiobuttonlist rockradiobuttonlist-vertical">
            <span>
                <div v-for="option in options" class="radio">
                    <label class="" :for="getOptionUniqueId(uniqueId, option)">
                        <input :id="getOptionUniqueId(uniqueId, option)" :name="uniqueId" type="radio" :value="option.value" v-model="internalValue" />
                        <span class="label-text">{{option.text}}</span>
                    </label>
                </div>
            </span>
        </div>
    </div>
</RockFormField>`
});

/*
<div class="form-group rock-radio-button-list required">
    <label class="control-label" for="ctl00_main_ctl23_ctl02_ctl06_rblFamilyOptions">
        Particip4nt is in the same immediate family as
    </label>
    <div class="control-wrapper">
        <div class="controls js-rockradiobuttonlist rockradiobuttonlist rockradiobuttonlist-vertical">
            <span id="ctl00_main_ctl23_ctl02_ctl06_rblFamilyOptions">
                <div class="radio">
                    <label class="" for="ctl00_main_ctl23_ctl02_ctl06_rblFamilyOptions_0">
                        <input id="ctl00_main_ctl23_ctl02_ctl06_rblFamilyOptions_0" type="radio" name="ctl00$main$ctl23$ctl02$ctl06$rblFamilyOptions" value="8611f5e6-c63f-4c2a-ae5c-859cdb350cd0">
                        <span class="label-text">Joe Hello</span>
                    </label>
                </div>
                <div class="radio">
                    <label class="" for="ctl00_main_ctl23_ctl02_ctl06_rblFamilyOptions_1">
                        <input id="ctl00_main_ctl23_ctl02_ctl06_rblFamilyOptions_1" type="radio" name="ctl00$main$ctl23$ctl02$ctl06$rblFamilyOptions" value="300e6c88-3b7f-43a4-9542-ef44ee5bf73b">
                        <span class="label-text">None of the above</span>
                    </label>
                </div>
            </span>
        </div>
    </div>
    <span id="ctl00_main_ctl23_ctl02_ctl06_rblFamilyOptions_rblFamilyOptions_rfv" class="validation-error help-inline" style="display:none;">Answer to which family is required.</span>
</div>
*/