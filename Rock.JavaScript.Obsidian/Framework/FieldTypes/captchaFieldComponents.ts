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
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import Captcha from "@Obsidian/Controls/captcha.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { ConfigurationPropertyKey } from "./captchaField.partial";
import { CaptchaValidateTokenResultBag } from "@Obsidian/ViewModels/Rest/Controls/captchaValidateTokenResultBag";

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
        const internalBooleanValue = ref<boolean | null>(asBooleanOrNull(props.modelValue));
        const internalToken = ref<string | undefined>();

        watch(captchaElement, async () => {
            if (captchaElement.value) {
                internalToken.value = await captchaElement.value.getToken();
                const captchaValidateTokenResultBag = await captchaElement.value.validateToken(internalToken.value) as CaptchaValidateTokenResultBag;
                internalBooleanValue.value = captchaValidateTokenResultBag?.isTokenValid || null;
            }

            emit("update:modelValue", asTrueFalseOrNull(internalBooleanValue.value) || "");
        });

        watch(() => props.modelValue, () => {
            internalBooleanValue.value = asBooleanOrNull(props.modelValue);
        });

        return {
            captchaElement,
            internalBooleanValue
        };
    },
    template: `
<Captcha ref="captchaElement" :isFieldType="true" :isTokenValid="internalBooleanValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "CaptchaField.Configuration",
    components: {
        NotificationBox
    },
    props: getFieldConfigurationProps(),
    setup(props) {
        // Define the properties that will hold the current selections.
        const notificationWarning = ref<string>(props.modelValue[ConfigurationPropertyKey.NotificationWarning]);

        return {
            notificationWarning,
        };
    },


    template: `
<div>
    <NotificationBox alertType="info">The individual will be prompted to verify that they are human each time this field is displayed in edit mode.</NotificationBox>
    <NotificationBox v-if="notificationWarning" alertType="warning">{{ notificationWarning }}</NotificationBox>
</div>
`
});


