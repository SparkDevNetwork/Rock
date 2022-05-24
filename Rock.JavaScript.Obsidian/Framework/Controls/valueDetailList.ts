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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import { ValueDetailListItem } from "@Obsidian/Types/Controls/valueDetailListItem";

export default defineComponent({
    name: "ValueDetailList",

    props: {
        modelValue: {
            type: Array as PropType<ValueDetailListItem[]>,
            required: false
        }
    },

    setup(props) {
        const values = ref(props.modelValue ?? []);

        const hasValues = computed((): boolean => {
            return values.value.length > 0;
        });

        watch(() => props.modelValue, () => {
            values.value = props.modelValue ?? [];
        });

        return {
            hasValues,
            values
        };
    },

    template: `
<dl v-if="hasValues">
    <template v-for="value in values">
        <dt>{{ value.title }}</dt>
        <dd v-if="value.htmlValue" v-html="value.htmlValue"></dd>
        <dd v-else>{{ value.textValue }}</dd>
    </template>
</dl>
`
});
