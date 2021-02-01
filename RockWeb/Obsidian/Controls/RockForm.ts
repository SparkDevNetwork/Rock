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
import { defineComponent } from '../Vendor/Vue/vue.js';
import { Form } from '../Vendor/VeeValidate/vee-validate.js';
import RockValidation from './RockValidation.js';

export default defineComponent({
    name: 'RockForm',
    components: {
        Form,
        RockValidation
    },
    data() {
        return {
            errorsToDisplay: [],
            submitCount: 0
        };
    },
    methods: {
        emitSubmit(payload) {
            this.$emit('submit', payload);
        },
        getErrorsToDisplay(errors, submitCount: number) {
            if (submitCount !== this.submitCount) {
                this.submitCount = submitCount;
                this.errorsToDisplay = errors;
            }

            return this.errorsToDisplay;
        }
    },
    template: `
<Form as="" #default="{errors, handleSubmit, submitCount}">
    <RockValidation v-if="submitCount" :errors="getErrorsToDisplay(errors, submitCount)" />
    <form @submit="handleSubmit($event, emitSubmit)">
        <slot />
    </form>
</Form>`
});