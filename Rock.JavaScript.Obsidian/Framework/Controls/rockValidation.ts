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
import Alert from "../Elements/alert.vue";
import { defineComponent, PropType, ref, watch } from "vue";
import { RockDateTime } from "../Util/rockDateTime";
import { FormError } from "../Util/form";
import { computed } from "vue";

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

    setup(props) {
        const errorsToShow = ref<Record<string, FormError>>({});
        const lastSubmitCount = ref(0);
        const lastErrorChangeMs = ref(0);

        const hasErrors = computed((): boolean => Object.keys(errorsToShow.value).length > 0);

        watch(() => props.submitCount, () => {
            const wasSubmitted = lastSubmitCount.value < props.submitCount;

            if (wasSubmitted) {
                const now = RockDateTime.now().toMilliseconds();
                errorsToShow.value = { ...props.errors };
                lastErrorChangeMs.value = now;
                lastSubmitCount.value = props.submitCount;
            }
        });

        watch(() => props.errors, () => {
            if (props.submitCount === -1) {
                // Do not debounce, just sync. This instance is probably not within a traditional form.
                errorsToShow.value = { ...props.errors };
                return;
            }

            // There are errors that come in at different cycles. Validation of all the form's fields seems to be async.
            // Therefore, we want to allow all of the errors from a single submit to be added to the screen.
            // However, we don't want the screen jumping around as the
            // user fixes errors. The intent here is to have a 500ms window after a submit occurs for errors to be collected.
            // After that window elapses, then no more errors can be added to the screen until the user submits again.
            const now = RockDateTime.now().toMilliseconds();
            const msSinceLastChange = now - lastErrorChangeMs.value;

            if (msSinceLastChange < 500) {
                errorsToShow.value = { ...props.errors };
                lastErrorChangeMs.value = now;
            }
        }, {
            immediate: true
        });

        return {
            errorsToShow,
            hasErrors,
            lastSubmitCount,
            lastErrorChangeMs
        };
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
</Alert>
`
});
