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
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import Captcha from "@Obsidian/Controls/captcha.obs";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { ConfigurationPropertyKey } from "./captchaField.partial";
import { CaptchaControlTokenValidateTokenResultBag } from "@Obsidian/ViewModels/Rest/Controls/captchaControlTokenValidateTokenResultBag";

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

        watch(captchaElement, async () => {
            if (captchaElement.value) {
                internalValue.value = await captchaElement.value.getToken();
                const captchaControlTokenValidateTokenResultBag = await captchaElement.value.validateToken(internalValue.value) as CaptchaControlTokenValidateTokenResultBag;
                internalBooleanValue.value = captchaControlTokenValidateTokenResultBag?.isTokenValid;
            }

            emit("update:modelValue", asTrueFalseOrNull(internalBooleanValue.value) || "");
        });

        watch(() => props.modelValue, () => {
            internalBooleanValue.value = asBoolean(props.modelValue);
        });

        return {
            captchaElement,
            internalBooleanValue
        };
    },
    template: `
<Captcha ref="captchaElement" />
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
    <NotificationBox alertType="info">The user will be prompted to complete verify they are human each time this field is displayed in edit mode.</NotificationBox>
    <NotificationBox v-if="notificationWarning" alertType="warning">{{ notificationWarning }}</NotificationBox>
</div>
`
});


