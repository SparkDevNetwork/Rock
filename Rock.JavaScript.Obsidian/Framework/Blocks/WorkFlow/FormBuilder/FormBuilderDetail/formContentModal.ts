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

import { defineComponent, nextTick, PropType, ref, watch } from "vue";
import Modal from "../../../../Controls/modal";
import RockForm from "../../../../Controls/rockForm";
import RockButton from "../../../../Elements/rockButton";
import TextBox from "../../../../Elements/textBox";
import { useVModelPassthrough } from "../../../../Util/component";
import ConfigurableZone from "./configurableZone";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.FormContentModal",

    components: {
        ConfigurableZone,
        Modal,
        RockButton,
        RockForm,
        TextBox
    },

    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },

        isVisible: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "save",
        "update:modelValue",
        "update:isVisible"
    ],

    setup(props, { emit }) {
        /** The internal value as displayed in the UI. */
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        /** True if the modal is currently visible. */
        const isVisible = useVModelPassthrough(props, "isVisible", emit);

        /** True if the form should start processing. */
        const submitForm = ref(false);

        /** Reference to the element containing the content text box. */
        const contentTextBox = ref<HTMLElement | null>(null);

        /**
         * Event handler called when the user clicks the save button.
         */
        const onStartSave = (): void => {
            // Start the form processing.
            submitForm.value = true;
        };

        /**
         * Event handler when the form has passed validation and is ready to
         * submit.
         */
        const onSubmitForm = (): void => {
            emit("save");
        };

        // When we become visible, make the text box have focus.
        watch(isVisible, () => {
            nextTick(() => {
                if (contentTextBox.value) {
                    const input = contentTextBox.value.querySelector("textarea");

                    input?.focus();
                }
            });
        });

        return {
            contentTextBox,
            internalValue,
            isVisible,
            onSubmitForm,
            onStartSave,
            submitForm,
        };
    },

    template: `
<Modal v-model="isVisible">
    <RockForm v-model:submit="submitForm" @submit="onSubmitForm">
        <div ref="contentTextBox">
            <TextBox v-model="internalValue" label="Content" textMode="multiline" />
        </div>
    </RockForm>

    <template #customButtons>
        <RockButton btnType="primary" @click="onStartSave">Save</RockButton>
    </template>
</Modal>
`
});
