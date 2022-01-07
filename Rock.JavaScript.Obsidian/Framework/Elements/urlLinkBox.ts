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
import { ruleStringToArray, ruleArrayToString } from "../Rules/index";
import { useVModelPassthrough } from '../Util/component';
import { defineComponent, PropType, computed } from "vue";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "UrlLinkBox",

    components: {
        RockFormField
    },

    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        rules: {
            type: String as PropType<string>,
            default: ""
        },
        requiresTrailingSlash: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        let value = useVModelPassthrough(props, "modelValue", emit);

        let computedRules = computed(() => {
            const rules = ruleStringToArray(props.rules);

            if (rules.indexOf("url") === -1) {
                rules.push("url");
            }

            if (props.requiresTrailingSlash) {
                rules.push("endswith:/")
            }

            return ruleArrayToString(rules);
        })

        return { value, computedRules}
    },

    template: `
<RockFormField
    v-model="value"
    formGroupClasses="url-link-box"
    name="urlbox"
    :rules="computedRules">
    <template #default="{uniqueId, field, errors, tabIndex, disabled}">
        <div class="control-wrapper">
            <div class="input-group">
                <span class="input-group-addon">
                    <i class="fa fa-link"></i>
                </span>
                <input :id="uniqueId" class="form-control" v-bind="field" :disabled="disabled" :tabindex="tabIndex" type="url" />
            </div>
        </div>
    </template>
</RockFormField>`
});
