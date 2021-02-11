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
import Alert from '../Elements/Alert.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

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
            errorsToShow: this.errors,
            allowErrorChange: false
        };
    },
    computed: {
        hasErrors(): boolean {
            return Object.keys(this.errorsToShow).length > 0;
        }
    },
    watch: {
        submitCount: {
            immediate: true,
            handler() {
                this.allowErrorChange = this.submitCount > 0;
            }
        },
        errors() {
            if (!this.allowErrorChange || !Object.keys(this.errors).length) {
                return;
            }

            this.errorsToShow = this.errors;
            this.allowErrorChange = false;
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