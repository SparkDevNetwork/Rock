﻿// <copyright>
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
import { computed, defineComponent, PropType, reactive, ref, watch } from "vue";
import { FormError, FormState, provideFormState } from "../Util/form";
import RockValidation from "./rockValidation";

export default defineComponent({
    name: "RockForm",

    components: {
        RockValidation
    },

    props: {
        submit: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "submit",
        "validationChanged",
        "update:submit"
    ],

    setup(props, { emit }) {
        const errors = ref<Record<string, FormError>>({});
        const submit = ref(props.submit);

        const onInternalSubmit = (): void => {
            submit.value = true;
        };

        const formState = reactive<FormState>({
            submitCount: 0,
            setError: (id: string, name: string, error: string): void => {
                const newErrors = {
                    ...errors.value
                };

                if (error) {
                    newErrors[id] = {
                        name,
                        text: error
                    };
                }
                else {
                    delete newErrors[id];
                }

                errors.value = newErrors;
            }
        });

        const submitCount = computed((): number => formState.submitCount);

        provideFormState(formState);

        watch(() => props.submit, () => {
            if (submit.value !== props.submit) {
                submit.value = props.submit;
            }
        });

        watch(submit, () => {
            if (submit.value) {
                formState.submitCount++;

                if (Object.keys(errors.value).length === 0) {
                    emit("submit");
                }

                submit.value = false;
            }

            emit("update:submit", submit.value);
        });

        watch(errors, () => {
            emit("validationChanged", errors.value);
        });

        return {
            onInternalSubmit,
            submitCount,
            errors
        };
    },

    template: `
<form @submit.prevent.stop="onInternalSubmit()">
    <RockValidation :submitCount="submitCount" :errors="errors" />
    <slot />
</form>
`
});
