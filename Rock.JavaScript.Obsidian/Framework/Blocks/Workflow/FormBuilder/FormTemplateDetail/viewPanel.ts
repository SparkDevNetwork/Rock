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

import { defineComponent, PropType, ref, watch } from "vue";
import { updateRefValue } from "../../../../Util/util";
import { TemplateDetail } from "./types";

export default defineComponent({
    name: "Workflow.FormTemplateDetail",

    components: {
    },

    props: {
        modelValue: {
            type: Object as PropType<TemplateDetail>,
            default: {}
        }
    },

    setup(props) {
        // Setup all the standard values that we will be displaying.
        const name = ref(props.modelValue.name ?? "");
        const description = ref(props.modelValue.description ?? "");
        const usedByWorkflowTypes = ref(props.modelValue.usedBy ?? []);

        // Watch for changes in our model value and update.
        watch(() => props.modelValue, () => {
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(usedByWorkflowTypes, props.modelValue.usedBy ?? []);
        });

        return {
            description,
            name,
        };
    },

    template: `
<fieldset>
    <dl>
        <dt>Name</dt>
        <dd>{{ name }}</dd>

        <template v-if="description">
            <dt>Description</dt>
            <dd>{{ description }}</dd>
        </template>

        <dt>Used By</dt>
        <dd>
            <ul>
                <li v-for="workflowType in usedByWorkflowTypes" :key="workflowType.value">{{ workflowType.text }}</li>
            </ul>
        </dd>
    </dl>
</fieldset>
`
});
