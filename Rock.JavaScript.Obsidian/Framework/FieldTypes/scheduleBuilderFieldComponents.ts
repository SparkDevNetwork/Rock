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

import { defineComponent, ref, useAttrs, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import ScheduleBuilder from "@Obsidian/Controls/scheduleBuilder.obs";

type ScheduleBuilderEditValueBag = {
    scheduleGuid?: string | null;
    iCalendarContent?: string | null;
};

function getEmptyValue(): ScheduleBuilderEditValueBag {
    return {
        scheduleGuid: "",
        iCalendarContent: ""
    };
}

export const EditComponent = defineComponent({
    name: "ScheduleBuilderField.Edit",
    inheritAttrs: false,

    components: {
        ScheduleBuilder
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const attrs = useAttrs();
        const internalValue = ref<ScheduleBuilderEditValueBag>(getEmptyValue());

        watch(() => props.modelValue, () => {
            if (!props.modelValue) {
                internalValue.value = getEmptyValue();
                return;
            }

            try {
                internalValue.value = JSON.parse(props.modelValue) as ScheduleBuilderEditValueBag;
            }
            catch {
                internalValue.value = getEmptyValue();
            }
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            const iCalendarContent = internalValue.value.iCalendarContent || "";

            if (!iCalendarContent) {
                emit("update:modelValue", "");
                return;
            }

            emit("update:modelValue", JSON.stringify({
                scheduleGuid: internalValue.value.scheduleGuid || "",
                iCalendarContent
            }));
        }, { deep: true });

        return {
            attrs,
            internalValue
        };
    },

    template: `
<ScheduleBuilder
    v-bind="attrs"
    v-model="internalValue.iCalendarContent"
    :displayScheduleFriendlyTextBesideLabel="true"
    :showClearButton="true"
/>
`
});

export const ConfigurationComponent = defineComponent({
    name: "ScheduleBuilderField.Configuration",

    template: ``
});
