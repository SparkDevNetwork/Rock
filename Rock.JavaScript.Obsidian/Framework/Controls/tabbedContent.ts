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

export default defineComponent({
    name: "TabbedContent",

    props: {
        tabList: {
            type: Array as PropType<any[]>, // eslint-disable-line @typescript-eslint/no-explicit-any
            required: true
        }
    },

    setup(props) {
        const active = ref(0);
        const classes = ref<string[]>([]);
        let timeout: NodeJS.Timeout;

        watch(() => props.tabList, () => {
            active.value = 0;

            classes.value = props.tabList.map((item, i) => {
                let list = "tab-pane fade";

                if (i == active.value) {
                    list += " active in";
                }

                return list;
            });
        }, {immediate: true});

        watch(active, (current, previous) => {
            classes.value[previous] = "tab-pane fade active";

            clearTimeout(timeout);
            timeout = setTimeout(() => {
                classes.value[previous] = "tab-pane fade";
                classes.value[current] = "tab-pane fade active in";
            }, 150);
        });

        return {
            active,
            classes
        };
    },

    template: `
<div>
    <ul class="nav nav-tabs margin-b-lg">
        <li v-for="(item, i) in tabList" :key="i" @click.prevent="active = i" :class="{active: active == i}">
            <a href="#" :aria-expanded="active == i">
                <slot name="tab" :item="item" />
            </a>
        </li>
    </ul>

    <div class="tab-content">
        <div v-for="(item, i) in tabList" :key="i" :class="classes[i]">
            <slot name="tabpane" :item="item" />
        </div>
    </div>
</div>
`
});
