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

    setup(props) {
        const hasErrors = computed((): boolean => props.errors.length > 0);

        return {
            hasErrors
        };
    },

    template: `
<NotificationBox v-show="hasErrors" alertType="validation">
    Please correct the following:
    <ul>
        <li v-for="error of errors">
            <strong>{{error.name}}</strong>
            {{error.text}}
        </li>
    </ul>
</NotificationBox>
`
});
