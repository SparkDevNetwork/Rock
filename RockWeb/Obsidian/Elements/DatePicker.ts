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
import { newGuid } from '../Util/Guid.js';
import RockLabel from './RockLabel.js';
import RockDate, { RockDateType } from '../Util/RockDate.js';

export default defineComponent({
    name: 'DatePicker',
    components: {
        RockLabel
    },
    props: {
        modelValue: {
            type: String as PropType<RockDateType | null>,
            default: null
        },
        label: {
            type: String as PropType<string>,
            required: true
        },
        help: {
            type: String as PropType<string>,
            default: ''
        },
        rules: {
            type: String as PropType<string>,
            default: ''
        },
        disabled: {
            type: Boolean,
            default: false
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-datepicker-${newGuid()}`,
            internalValue: null as string | null
        };
    },
    computed: {
        isRequired(): boolean {
            return this.rules.includes('required');
        },
        asRockDateOrNull(): RockDateType | null {
            return this.internalValue ? RockDate.toRockDate(new Date(this.internalValue)) : null;
        }
    },
    methods: {
        onChange(arg) {
            console.log('change', arg);
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.asRockDateOrNull);
        },
        modelValue: {
            immediate: true,
            handler() {
                if (!this.modelValue) {
                    this.internalValue = null;
                    return;
                }

                const month = RockDate.getMonth(this.modelValue);
                const day = RockDate.getDay(this.modelValue);
                const year = RockDate.getYear(this.modelValue);
                this.internalValue = `${month}/${day}/${year}`;
            }
        }
    },
    mounted() {
        window['Rock'].controls.datePicker.initialize({
            id: this.uniqueId,
            startView: 0,
            showOnFocus: true,
            format: 'mm/dd/yyyy',
            todayHighlight: true,
            forceParse: true,
            onChangeScript: () => {
                this.internalValue = window['$'](`#${this.uniqueId}`).val();
            }
        });
    },
    template: `
<div class="form-group date-picker required">
    <RockLabel :for="uniqueId" :help="help">{{label}}</RockLabel>
    <div class="control-wrapper">
        <div class="input-group input-width-md js-date-picker date">
            <input type="text" :id="uniqueId" class="form-control" v-model.lazy="internalValue" />
            <span class="input-group-addon">
                <i class="fa fa-calendar"></i>
            </span>
        </div>
    </div>
</div>`
});
