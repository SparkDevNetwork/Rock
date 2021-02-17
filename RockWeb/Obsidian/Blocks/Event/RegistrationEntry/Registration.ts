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
import { ConfigurationValues } from '../../../Index';
import ProgressBar from '../../../Elements/ProgressBar';
import RockButton from '../../../Elements/RockButton';
import RegistrationTemplateForm from '../../../ViewModels/CodeGenerated/RegistrationTemplateFormViewModel';
import { RegistrantInfo } from '../RegistrationEntry';

export default defineComponent({
    name: 'Event.RegistrationEntry.Registration',
    components: {
        RockButton,
        ProgressBar
    },
    setup() {
        return {
            configurationValues: inject('configurationValues') as ConfigurationValues
        };
    },
    props: {
        registrants: {
            type: Array as PropType<RegistrantInfo[]>,
            required: true
        }
    },
    data() {
        return {
            registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || []) as RegistrationTemplateForm[]
        };
    },
    computed: {
        formCountPerRegistrant(): number {
            return this.registrationTemplateForms.length;
        },
        numberOfPages(): number {
            // All of the steps are 1 page except the "per-registrant"
            return 3 + (this.registrants.length * this.formCountPerRegistrant);
        },
        completionPercentDecimal(): number {
            return (this.numberOfPages - 2) / this.numberOfPages;
        },
        completionPercentInt(): number {
            return this.completionPercentDecimal * 100;
        }
    },
    methods: {
        onPrevious() {
            this.$emit('previous');
        },
        onNext() {
            this.$emit('next');
        }
    },
    template: `
<div class="registrationentry-registration-attributes">
    <h1>Registration Attributes</h1>
    <ProgressBar :percent="completionPercentInt" />

    <div class="actions">
        <RockButton btnType="default" @click="onPrevious">
            Previous
        </RockButton>
        <RockButton btnType="primary" class="pull-right" @click="onNext">
            Next
        </RockButton>
    </div>
</div>`
});