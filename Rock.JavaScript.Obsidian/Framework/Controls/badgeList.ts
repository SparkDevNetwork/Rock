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

import { Guid } from "@Obsidian/Types";
import { useHttp } from "@Obsidian/Utility/http";
import { popover } from "@Obsidian/Utility/popover";
import { tooltip } from "@Obsidian/Utility/tooltip";
import { BadgeListGetBadgesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/badgeListGetBadgesOptionsBag";
import { RenderedBadgeBag } from "@Obsidian/ViewModels/Crm/renderedBadgeBag";
import { defineComponent, nextTick, PropType, ref, watch } from "vue";

/** Displays a collection of badges for the specified entity. */
export default defineComponent({
    name: "BadgeList",

    props: {
        /** The unique identifier of the type of entity to be rendered. */
        entityTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        /** The identifier key of the entity to be rendered. */
        entityKey: {
            type: String as PropType<string>,
            required: false
        },

        /**
         * The list of badge type unique identifiers to be rendered. If null
         * or empty array then all available badge types are rendered.
         */
        badgeTypeGuids: {
            type: Array as PropType<Guid[]>,
            required: false
        }
    },

    setup(props) {
        // #region Values

        const http = useHttp();
        const badges = ref<string[]>([]);
        const containerRef = ref<HTMLElement | null>(null);

        // #endregion

        // #region Functions

        /** Load the badges from our property data and render the output to the DOM. */
        const loadBadges = async (): Promise<void> => {
            const data: BadgeListGetBadgesOptionsBag = {
                badgeTypeGuids: props.badgeTypeGuids,
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey
            };

            const result = await http.post<RenderedBadgeBag[]>("/api/v2/Controls/BadgeListGetBadges", undefined, data);

            if (result.isSuccess && result.data) {
                // Get all the HTML content to be rendered.
                badges.value = result.data.map(b => b.html ?? "");

                let script = "";

                for (const badge of result.data) {
                    if (badge.javaScript) {
                        script += badge.javaScript;
                    }
                }

                if (script !== "") {
                    // Add the script on the next tick to ensure the HTML has been rendered.
                    nextTick(() => {
                        const scriptNode = document.createElement("script");
                        scriptNode.type = "text/javascript";
                        scriptNode.innerText = script;
                        document.body.appendChild(scriptNode);
                    });
                }

                // Enable tooltips and popovers.
                nextTick(() => {
                    if (!containerRef.value) {
                        return;
                    }

                    tooltip(Array.from(containerRef.value.querySelectorAll(".rockbadge[data-toggle=\"tooltip\"]")));
                    popover(Array.from(containerRef.value.querySelectorAll(".rockbadge[data-toggle=\"popover\"]")));
                });
            }
            else {
                console.error(`Error loading badges: ${result.errorMessage || "Unknown error"}`);
                badges.value = [];
            }
        };

        // #endregion

        watch([() => props.badgeTypeGuids, () => props.entityKey, () => props.entityTypeGuid], () => {
            loadBadges();
        });

        // Start loading the badges immediately.
        loadBadges();

        return {
            badges,
            containerRef
        };
    },

    template: `
<div ref="containerRef" style="display: flex;">
    <div v-for="badge in badges" v-html="badge" />
</div>
`
});
