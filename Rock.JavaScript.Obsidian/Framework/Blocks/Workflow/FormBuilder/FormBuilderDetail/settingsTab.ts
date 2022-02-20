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

import { defineComponent, PropType } from "vue";
import RockForm from "../../../../Controls/rockForm";
import { useVModelPassthrough } from "../../../../Util/component";
import CompletionSettings from "./completionSettings";
import GeneralSettings from "./generalSettings";
import { FormCompletionAction, FormGeneral } from "./types";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.SettingsTab",

    components: {
        GeneralSettings,
        CompletionSettings,
        RockForm
    },

    props: {
        modelValue: {
            type: Object as PropType<FormGeneral>,
            required: true
        },

        completion: {
            type: Object as PropType<FormCompletionAction>,
            required: true
        }
    },

    emits: [
        "update:modelValue",
        "update:completion",
    ],

    setup(props, { emit }) {
        const generalSettings = useVModelPassthrough(props, "modelValue", emit);
        const completionSettings = useVModelPassthrough(props, "completion", emit);

        return {
            generalSettings,
            completionSettings
        };
    },

    template: `
<div class="d-flex flex-column" style="flex-grow: 1; overflow-y: auto;">
    <div class="panel-body">
        <RockForm>
            <GeneralSettings v-model="generalSettings" />

            <CompletionSettings v-model="completionSettings" />
        </RockForm>
    </div>
</div>
`
});
