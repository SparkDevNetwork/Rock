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
import Alert from "../Elements/alert";
import { defineComponent, PropType } from "vue";
import { RockDateTime } from "../Util/rockDateTime";
import { FormError } from "../Util/form";

export default defineComponent({
    name: "RockValidation",
    components: {
        Alert
    },
    props: {
        errors: {
            type: Object as PropType<Record<string, FormError>>,
            required: true
        },
        submitCount: {
            type: Number as PropType<number>,
            default: -1
        }
    },
    data() {
        return {
            errorsToShow: {} as Record<string, FormError>,
            lastSubmitCount: 0,
            lastErrorChangeMs: 0
        };
    },
    computed: {
        hasErrors(): boolean {
            return Object.keys(this.errorsToShow).length > 0;
        }
    },
    watch: {
        submitCount() {
            const wasSubmitted = this.lastSubmitCount < this.submitCount;

            if (wasSubmitted) {
                const now = RockDateTime.now().toMilliseconds();
                this.errorsToShow = { ...this.errors };
                this.lastErrorChangeMs = now;
                this.lastSubmitCount = this.submitCount;
            }
        },
        errors: {
            immediate: true,
            handler() {
                if (this.submitCount === -1) {
                    // Do not debounce, just sync. This instance is probably not within a traditional form.
                    this.errorsToShow = { ...this.errors };
                    return;
                }

                // There are errors that come in at different cycles. Validation of all the form's fields seems to be async.
                // Therefore, we want to allow all of the errors from a single submit to be added to the screen.
                // However, we don't want the screen jumping around as the
                // user fixes errors. The intent here is to have a 500ms window after a submit occurs for errors to be collected.
                // After that window elapses, then no more errors can be added to the screen until the user submits again.
                const now = RockDateTime.now().toMilliseconds();
                const msSinceLastChange = now - this.lastErrorChangeMs;

                if (msSinceLastChange < 500) {
                    this.errorsToShow = { ...this.errors };
                    this.lastErrorChangeMs = now;
                }
            }
        }
    },
    template: `
<Alert v-show="hasErrors" alertType="validation">
    Please correct the following:
    <ul>
        <li v-for="(error) of errorsToShow">
            <strong>{{error.name}}</strong>
            {{error.text}}
        </li>
    </ul>
</Alert>`
});
