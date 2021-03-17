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
import Alert from '../Elements/Alert';
import { defineComponent, PropType } from 'vue';

export default defineComponent({
    name: 'RockValidation',
    components: {
        Alert
    },
    props: {
        errors: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },
        submitCount: {
            type: Number as PropType<number>,
            required: true
        }
    },
    data() {
        return {
            errorsToShow: {} as Record<string, string>,
            lastSubmitCount: 0,
            lastErrorChangeMs: 0
        };
    },
    computed: {
        hasErrors(): boolean {
            return Object.keys(this.errorsToShow).length > 0;
        }
    },
    methods: {
        syncErrorsDebounced() {
            // There are errors that come in at different cycles. We don't want the screen jumping around as the
            // user fixes errors. But, we do want the validations from the submit cycle to all get through even
            // though they come at different times. The "debounce" 1000ms code is to try to allow all of those
            // through, but then prevent changes once the user starts fixing the form.
            const now = new Date().getTime();
            const msSinceLastChange = now - this.lastErrorChangeMs;
            this.lastErrorChangeMs = now;
            const wasSubmitted = this.lastSubmitCount < this.submitCount;

            if (msSinceLastChange > 1000 || !wasSubmitted) {
                return;
            }

            this.errorsToShow = this.errors;
            this.lastSubmitCount = this.submitCount;
        }
    },
    watch: {
        submitCount() {
            this.syncErrorsDebounced();
        },
        errors: {
            immediate: true,
            handler() {
                this.syncErrorsDebounced();
            }
        }
    },
    template: `
<Alert v-show="hasErrors" alertType="validation">
    Please correct the following:
    <ul>
        <li v-for="(error, fieldLabel) of errorsToShow">
            <strong>{{fieldLabel}}</strong>
            {{error}}
        </li>
    </ul>
</Alert>`
});