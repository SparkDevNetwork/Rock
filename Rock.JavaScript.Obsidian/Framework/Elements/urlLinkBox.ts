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
import { normalizeRules, rulesPropType, ValidationRule } from "../Rules/index";
import { useVModelPassthrough } from "../Util/component";
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
        rules: rulesPropType,
        requiresTrailingSlash: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const value = useVModelPassthrough(props, "modelValue", emit);

        const computedRules = computed((): ValidationRule[] => {
            const rules = normalizeRules(props.rules);

            if (rules.indexOf("url") === -1) {
                rules.push("url");
            }

            if (props.requiresTrailingSlash) {
                rules.push("endswith:/");
            }

            return rules;
        });

        return {
            computedRules,
            value
        };
    },

    template: `
<RockFormField
    :modelValue="value"
    formGroupClasses="url-link-box"
    name="urlbox"
    :rules="computedRules">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="input-group">
                <span class="input-group-addon">
                    <i class="fa fa-link"></i>
                </span>
                <input v-model="value" :id="uniqueId" class="form-control" v-bind="field" type="url" />
            </div>
        </div>
    </template>
</RockFormField>`
});
