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

import { useFormState } from "@Obsidian/Utility/form";
import { newGuid } from "@Obsidian/Utility/guid";
import { defineComponent, onBeforeUnmount, PropType, ref, watch } from "vue";

/**
 * Displays a static error inside a RockForm.
 */
export default defineComponent({
    name: "RockFormFieldError",

    props: {
        label: {
            type: String as PropType<string>,
            required: true
        },

        error: {
            type: String as PropType<string>,
            required: false
        }
    },

    setup(props) {
        /** The reactive state of the form. */
        const formState = useFormState();

        /** The unique identifier used to identify this form field. */
        const uniqueId = `rock-error-${newGuid()}`;

        /** Holds the current error message for this form field. */
        const currentError = ref(props.error);

        // Watch for changes to the modelValue and update our internalValue.
        watch(() => props.error, () => {
            currentError.value = props.error;

            if (currentError.value) {
                formState?.setError(uniqueId, props.label, currentError.value);
            }
            else {
                formState?.setError(uniqueId, props.label, "");
            }
        }, {
            immediate: true
        });

        // If we are removed from the DOM completely, clear the error before we go.
        onBeforeUnmount(() => {
            currentError.value = "";
            formState?.setError(uniqueId, props.label, "");
        });

        return {
        };
    },

    template: `
`
});
