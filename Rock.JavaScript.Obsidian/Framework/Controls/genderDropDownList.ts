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
import { computed, defineComponent } from "vue";
import { normalizeRules, rulesPropType } from "@Obsidian/ValidationRules";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import DropDownList from "./dropDownList";
import { Gender } from "@Obsidian/Enums/Crm/gender";

export default defineComponent({
    name: "GenderDropDownList",

    components: {
        DropDownList
    },

    props: {
        rules: rulesPropType
    },

    setup(props) {
        const options: ListItemBag[] = [
            { text: " ", value: Gender.Unknown.toString() },
            { text: "Male", value: Gender.Male.toString() },
            { text: "Female", value: Gender.Female.toString() }
        ];

        const computedRules = computed(() => {
            const rules = normalizeRules(props.rules);
            const notEqualRule = `notequal:${Gender.Unknown}`;

            if (rules.includes("required") && !rules.includes(notEqualRule)) {
                rules.push(notEqualRule);
            }

            return rules;
        });

        return {
            computedRules,
            options
        };
    },

    template: `
<DropDownList label="Gender" :items="options" :showBlankItem="false" :rules="computedRules" />
`
});
