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
import { defineComponent } from 'vue';
import { Form } from 'vee-validate';
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
        onInternalSubmit(handleSubmit, $event) {
            this.submitCount++;
            return handleSubmit($event, this.emitSubmit);
        },
        emitSubmit(payload) {
            this.$emit('submit', payload);
        }
    },
    template: `
<Form as="" #default="{errors, handleSubmit}">
    <RockValidation :submitCount="submitCount" :errors="errors" />
    <form @submit="onInternalSubmit(handleSubmit, $event)">
        <slot />
    </form>
</Form>`
});