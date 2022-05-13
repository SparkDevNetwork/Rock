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

import { Component, ComponentPublicInstance, defineComponent, nextTick, onMounted, PropType, ref, unref, watch } from "vue";
import RockButton from "./rockButton";

export default defineComponent({
    name: "CopyButton",

    components: {
        RockButton
    },

    props: {
        value: {
            type: String as PropType<string>,
            required: true
        },
        tooltip: {
            type: String as PropType<string>,
            default: "Copy"
        },
        tooltipPlacement: {
            type: String as PropType<"auto" | "top" | "right" | "bottom" | "left">,
            default: "auto"
        }
    },

    setup(props) {
        const el = ref<ComponentPublicInstance | null>(null);
        let jquery;

        function copy(e: MouseEvent): void {
            e.preventDefault();
            navigator.clipboard.writeText(props.value);

            jquery(el.value?.$el)
                .attr("data-original-title", "Copied")
                .tooltip("show")
                .attr("data-original-title", props.tooltip);

        }

        onMounted(() => {
            if (!el.value) {
                return;
            }

            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            jquery = <any>window[<any>"$"];
            jquery(el.value?.$el).tooltip();
        });

        return {
            el,
            copy
        };
    },

    template: `
<RockButton
    class="btn-copy-to-clipboard"
    :onClick="copy"
    data-trigger="hover"
    data-toggle="tooltip"
    :data-placement="tooltipPlacement"
    data-container="body"
    :data-original-title="tooltip"
    ref="el"
><i class="fa fa-clipboard"></i></RockButton>
`
});