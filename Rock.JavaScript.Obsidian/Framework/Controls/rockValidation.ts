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
import NotificationBox from "./notificationBox.obs";
import { computed, defineComponent, PropType } from "vue";
import { FormError } from "@Obsidian/Utility/form";
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
    name: "RockValidation",

    components: {
        NotificationBox
    },

    props: {
        /** The errors that should be displayed. */
        errors: {
            type: Array as PropType<FormError[]>,
            required: true
        }
    },
    // LPC CODE
    methods: {
        getLang,
        getErrorText(error: Record<string, FormError>) {
            let errorText = String(error.text);
            if (getLang() == 'es' && errorText == 'is required') {
                errorText = 'es necesario';
            }
            return errorText;
        }
    },
    // END LPC CODE
    setup(props) {
        const hasErrors = computed((): boolean => props.errors.length > 0);

        return {
            hasErrors
        };
    },

    template: `
<NotificationBox v-show="hasErrors" alertType="validation">
    <!-- MODIFIED LPC CODE -->
    {{ getLang() == 'es' ? 'Por favor, corregir lo siguiente:' : 'Please correct the following:' }}
    <!-- END MODIFIED LPC CODE -->
    <ul>
        <li v-for="error of errors">
            <!-- MODIFIED LPC CODE -->
            <strong>{{error.name}}</strong> {{getErrorText(error)}}
            <!-- END MODIFIED LPC CODE -->
        </li>
    </ul>
</NotificationBox>
`
});
