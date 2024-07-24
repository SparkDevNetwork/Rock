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

        /*
            7/24/2024 - JJZ

            We need to debounce this because when the country code changes, it also tends to change the phone number
            in order to format it, which will cause multiple changes to be emitted one right after the other. In Vue 3.3,
            the updates fire up and down the tree in the wrong order, so both of the changes get emitted up, but the
            changes propogating back down are missing one of the changes and end overriding that change because they're
            combined into one model now. See following timeline diagram for details:

            Assume the following hierarchy:
            > RockField
              > PhoneNumberField.Edit
                > PhoneNumberBox

            1. PhoneNumberBox: emits "update:countryCode" ⬆️
            2. PhoneNumberField.Edit: emits "update:modelValue" ⬆️ (with countryCode change)
            3. RockField: emit "update:modelValue" ⬆️ (with countryCode change)
            4. PhoneNumberBox: emits "update:modelValue" ⬆️
            5. RockField: receives update from parent to modelValue ⬇️ (only with countryCode change)
            6. PhoneNumberField.Edit: emits "update:modelValue" ⬆️ (with both changes)
            7. RockField: emits "update:modelValue" ⬆️ (with both changes)
            8. PhoneNumberField.Edit: receives update from parent to modelValue ⬇️ (only with countryCode change)
            9. PhoneNumberField.Edit: emits "update:modelValue" ⬆️ (only with countryCode change)
            10. RockField: emits "update:modelValue" ⬆️ (only with countryCode change)
            11. PhoneNumberBox: receives update from parent to countryCode ⬇️

            There is some very strange ordering going on there and since things are out of order, the phone number value
            is reset back to the prior value in RockField and the Field.Edit component, however, the phone number is still
            up-to-date in PhoneNumberBox...

            By adding this short debounce, we ensure that all the updates from the PhoneNumberBox are propogated up to the
            Field.Edit component, THEN we emit a single change up from there to prevent the race from happening and all
            the updates stay in place.

            In Vue 3.4, however, this will not be necessary because the reactivity system has been rewritten to be more
            efficient and predictable in the order of its updates:
            https://blog.vuejs.org/posts/vue-3-4#more-efficient-reactivity-system

            Once we have upgraded to 3.4 or higher, this debounce (and this engineering note) can be removed.
        */
        const emitChanges = debounce(() => {
            emit("update:modelValue", JSON.stringify({ number: phoneNumber.value, countryCode: countryCode.value }));
        }, 3);

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
