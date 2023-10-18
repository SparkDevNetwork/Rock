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

        watch(() => props.modelValue, () => {
            try {
                const fieldValue = JSON.parse(props.modelValue) as PhoneNumberFieldValue;

                phoneNumber.value = fieldValue?.number ?? "";
                countryCode.value = fieldValue?.countryCode ?? "";
            }
            catch(e) {
                phoneNumber.value = "";
                countryCode.value = "";
            }
        }, {immediate:true});

        watch([phoneNumber, countryCode], () => {
            emit("update:modelValue", JSON.stringify({number:phoneNumber.value, countryCode:countryCode.value}));
        });

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
