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
import Panel from "@Obsidian/Controls/panel.obs";

/** Provides a generic Rock Block structure */
export default defineComponent({
    name: "Block",

    components: {
        Panel
    },

    props: {
        title: {
            type: String as PropType<string>,
            required: false
        }
    },

    setup() {
        return {
        };
    },

    template: `
<Panel type="block" :title="title">
    <template v-if="$slots.headerActions" #headerActions>
        <slot name="headerActions" />
    </template>

    <template v-if="$slots.drawer" #drawer>
        <slot name="drawer" />
    </template>

    <template v-if="$slots.preBody" #preBody>
        <slot name="preBody" />
    </template>

    <slot />
</Panel>`
});
