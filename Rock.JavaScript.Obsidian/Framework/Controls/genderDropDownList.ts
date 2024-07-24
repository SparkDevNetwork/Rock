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
import { PropType, computed, defineComponent } from "vue";
import { normalizeRules, rulesPropType } from "@Obsidian/ValidationRules";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import DropDownList from "./dropDownList";
import { Gender } from "@Obsidian/Enums/Crm/gender";
// LPC CODE
import { useStore } from "@Obsidian/PageState";
const store = useStore();
/** Gets the lang parameter from the query string.
 * Returns "en" or "es". Defaults to "en" if invalid. */
function getLang(): string {
    var lang = typeof store.state.pageParameters["lang"] === 'string' ? store.state.pageParameters["lang"] : "";
    if (lang != "es") {
        lang = "en";
    }
    return lang;
}
// END LPC CODE

export default defineComponent({
    name: "GenderDropDownList",

    components: {
        DropDownList
    },

    props: {
        rules: rulesPropType
    },

    // LPC CODE
    methods: {
        getLang
    },
    // END LPC CODE

    setup(props) {
        // LPC MODIFIED CODE
        const options: ListItemBag[] = [
            { text: " ", value: Gender.Unknown.toString() },
            { text: getLang() == "es" ? "Masculino" : "Male", value: Gender.Male.toString() },
            { text: getLang() == "es" ? "Femenino" : "Female", value: Gender.Female.toString() }
        ];
        // END LPC MODIFIED CODE

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

    // LPC MODIFIED CODE
    template: `
<DropDownList :label="getLang() == 'es' ? 'Género' : 'Gender'" :items="options" :showBlankItem="false" :rules="computedRules" />
`
    // END LPC MODIFIED CODE
});
