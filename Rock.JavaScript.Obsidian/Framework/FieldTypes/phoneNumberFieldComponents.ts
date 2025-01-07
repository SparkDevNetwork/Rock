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
import PhoneNumberBox from "@Obsidian/Controls/phoneNumberBox.obs";
import { debounce } from "@Obsidian/Utility/util";

type PhoneNumberFieldValue = {
    number: string,
    countryCode: string
};

export const EditComponent = defineComponent({
    name: "PhoneNumberField.Edit",

    components: {
        PhoneNumberBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const phoneNumber = ref("");
        const countryCode = ref("");

        // We need to debounce this because when the country code changes, it also tends to change the phone number
        // in order to format it, and if we emit both changes immediately, something on the outside is only
        // catching one of them and so by the time it updates this modelValue prop, it actually overrides one of
        // the changes.
        const emitChanges = debounce(() => {
            emit("update:modelValue", JSON.stringify({ number: phoneNumber.value, countryCode: countryCode.value }));
        }, 5);

        watch(() => props.modelValue, () => {
            try {
                const fieldValue = JSON.parse(props.modelValue) as PhoneNumberFieldValue;

                phoneNumber.value = fieldValue?.number ?? "";
                countryCode.value = fieldValue?.countryCode ?? "";
            }
            catch (e) {
                phoneNumber.value = "";
                countryCode.value = "";
            }
        }, {immediate: true});

        watch([phoneNumber, countryCode], () => emitChanges());

        return {
            phoneNumber,
            countryCode
        };
    },

    template: `
<PhoneNumberBox v-model="phoneNumber" v-model:countryCode="countryCode" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "PhoneNumberField.Configuration",

    template: ``
});
