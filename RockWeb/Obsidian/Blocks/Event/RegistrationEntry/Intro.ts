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

import { defineComponent, inject, PropType } from 'vue';
import NumberUpDown from '../../../Elements/NumberUpDown';
import RockButton from '../../../Elements/RockButton';
import { RegistrationEntryBlockViewModel } from './RegistrationEntryBlockViewModel';

export default defineComponent({
    name: 'Event.RegistrationEntry.Intro',
    components: {
        NumberUpDown,
        RockButton
    },
    setup() {
        return {
            viewModel: inject('configurationValues') as RegistrationEntryBlockViewModel
        };
    },
    props: {
        initialRegistrantCount: {
            type: Number as PropType<number>,
            default: 1
        }
    },
    data() {
        return {
            numberOfRegistrants: this.initialRegistrantCount || 1
        };
    },
    methods: {
        onNext() {
            this.$emit('next', {
                numberOfRegistrants: this.numberOfRegistrants
            });
        },
    },
    template: `
<div class="registrationentry-intro">
    <div class="text-left" v-html="viewModel.InstructionsHtml">
    </div>
    <div class="registrationentry-intro">
        <h1>How many {{viewModel.PluralRegistrantTerm}} will you be registering?</h1>
        <NumberUpDown v-model="numberOfRegistrants" class="margin-t-sm" numberIncrementClasses="input-lg" />
    </div>
    <div class="actions text-right">
        <RockButton btnType="primary" @click="onNext">
            Next
        </RockButton>
    </div>
</div>`
});
