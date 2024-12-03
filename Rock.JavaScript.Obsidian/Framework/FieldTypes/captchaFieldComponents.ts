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

import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import Captcha from "@Obsidian/Controls/captcha.obs";

export const EditComponent = defineComponent({
    name: "CaptchaField.Edit",
    components: {
        Captcha
    },
    props: getFieldEditorProps(),

    emits: ["update:modelValue"],

    setup(props, { emit }) {
        // Internal values
        const captchaElement = ref<InstanceType<typeof Captcha> | undefined>();
        const internalBooleanValue = ref(asBoolean(props.modelValue));
        const internalValue = ref("");

        watch(captchaElement.value, () => {
            if (captchaElement.value) {
                internalValue.value = captchaElement.value.getToken();
                captchaElement.value.refreshToken();
                internalBooleanValue.value = true;
            }

            emit("update:modelValue", internalBooleanValue);
        });

        watch(() => props.modelValue, () => {
            internalBooleanValue.value = asBoolean(props.modelValue);
        });

        return {
            internalBooleanValue
        };
    },
    template: `
<Captcha ref="captchaElement" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "CaptchaField.Configuration",

    template: ``
});


