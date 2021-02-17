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
import { ConfigurationValues } from '../../../Index.js';
import RegistrationInstance from '../../../ViewModels/CodeGenerated/RegistrationInstanceViewModel';
import RegistrationTemplate from '../../../ViewModels/CodeGenerated/RegistrationTemplateViewModel';

export default defineComponent({
    name: 'Event.RegistrationEntry.Intro',
    components: {
        NumberUpDown,
        RockButton
    },
    setup() {
        return {
            configurationValues: inject('configurationValues') as ConfigurationValues
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
            numberOfRegistrants: this.initialRegistrantCount || 1,
            registrationInstance: this.configurationValues['registrationInstance'] as RegistrationInstance | null,
            registrationTemplate: this.configurationValues['registrationTemplate'] as RegistrationTemplate | null
        };
    },
    computed: {
        pluralRegistrantTerm(): string {
            return this.registrationTemplate?.PluralRegistrantTerm?.toLowerCase() || 'registrants';
        },
        registrationInstructions(): string {
            return this.registrationInstance?.RegistrationInstructions || this.registrationTemplate?.RegistrationInstructions || '';
        }
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
    <div class="text-left" v-html="registrationInstructions">
    </div>
    <div class="registrationentry-intro">
        <h1>How many {{pluralRegistrantTerm}} will you be registering?</h1>
        <NumberUpDown v-model="numberOfRegistrants" class="margin-t-sm input-lg" />
    </div>
    <div class="actions">
        <RockButton btnType="primary" class="pull-right" @click="onNext">
            Next
        </RockButton>
    </div>
</div>`
});
