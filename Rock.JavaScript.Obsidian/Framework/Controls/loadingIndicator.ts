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
import { defineComponent, PropType, ref } from "vue";

export default defineComponent({
    name: "LoadingIndicator",

    props: {
        /** The delay in milliseconds to wait before showing the loading indicator. */
        delay: {
            type: Number as PropType<number>,
            default: 0
        }
    },

    setup(props) {
        const isShown = ref(!props.delay);

        if (props.delay) {
            setTimeout(() => isShown.value = true, props.delay);
        }

        return {
            isShown
        };
    },

    template: `
<div v-if="isShown" class="text-center fa-2x">
    <i class="fas fa-spinner fa-pulse"></i>
</div>`
});
