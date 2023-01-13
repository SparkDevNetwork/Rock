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
import { defineComponent, PropType, reactive, ref, watch } from "vue";
import { FormError, FormState, provideFormState } from "@Obsidian/Utility/form";
import { updateRefValue } from "@Obsidian/Utility/component";
import RockValidation from "./rockValidation";

export default defineComponent({
    name: "RockForm",

    components: {
        RockValidation
    },

    props: {
        /** True if the form should attempt to submit. */
        submit: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** True if the validation errors should not be displayed. */
        hideErrors: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * This value can be used to reset the form to it's initial state.
         * Any time this value changes the submission count and error list
         * will be reset. This does not effect the values in the form controls.
         */
        formResetKey: {
            type: String as PropType<string>,
            default: ""
        }
    },

    emits: {
        "submit": () => true,
        // This contains all active errors even if the UI is not in sync.
        "validationChanged": (_errors: FormError[]) => true,
        // This contains just the errors that should be currently displayed in the UI.
        "visibleValidationChanged": (_errors: FormError[]) => true,
        "update:submit": (_value: boolean) => true
    },

    setup(props, { emit }) {
        const visibleErrors = ref<FormError[]>([]);
        const errorValues = ref<FormError[]>([]);
        const errors = ref<Record<string, FormError>>({});
        const submit = ref(props.submit);

        const onInternalSubmit = (): void => {
            submit.value = true;
        };

        // Construct the form state.
        const formState = reactive<FormState>({
            submitCount: 0,
            setError: (id: string, name: string, error: string): void => {
                const newErrors = {
                    ...errors.value
                };

                // If this identifier has an error, then set the error.
                // Otherwise clear the error.
                if (error) {
                    newErrors[id] = {
                        name,
                        text: error
                    };
                }
                else {
                    delete newErrors[id];
                }

                updateRefValue(errors, newErrors);
            }
        });

        provideFormState(formState);

        // Watch for requests to submit from the parent component.
        watch(() => props.submit, () => {
            if (submit.value !== props.submit) {
                submit.value = props.submit;
            }
        });

        // Watch for any submit actions and check the validation.
        watch(submit, () => {
            if (submit.value) {
                formState.submitCount++;

                // Update the visible errors.
                visibleErrors.value = errorValues.value;
                emit("visibleValidationChanged", visibleErrors.value);

                if (Object.keys(errors.value).length === 0) {
                    emit("submit");
                }

                submit.value = false;
            }

            emit("update:submit", submit.value);
        });

        // If any errors change then update the list of errors.
        watch(errors, () => {
            const values: FormError[] = [];

            for (const key in errors.value) {
                values.push(errors.value[key]);
            }

            errorValues.value = values;
            emit("validationChanged", errorValues.value);
        });

        watch(() => props.formResetKey, () => {
            formState.submitCount = 0;
            updateRefValue(errors, {});
            updateRefValue(visibleErrors, []);
            emit("visibleValidationChanged", visibleErrors.value);
        });

        return {
            errors,
            visibleErrors,
            onInternalSubmit
        };
    },

    template: `
<form @submit.prevent.stop="onInternalSubmit()">
    <RockValidation v-if="!hideErrors" :errors="visibleErrors" />
    <slot />
</form>
`
});
