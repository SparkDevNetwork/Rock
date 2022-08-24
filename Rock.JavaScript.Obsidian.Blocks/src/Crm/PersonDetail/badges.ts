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

import { computed, defineComponent, nextTick, ref } from "vue";
import Alert from "@Obsidian/Controls/alert";
import EntityTagList from "@Obsidian/Controls/entityTagList";
import { EntityType } from "@Obsidian/SystemGuids";
import { BadgesConfigurationBox } from "@Obsidian/ViewModels/Blocks/Crm/PersonDetail/Badges/badgesConfigurationBox";
import { useConfigurationValues } from "@Obsidian/Utility/block";
import { ControlLazyMode } from "@Obsidian/Types/Controls/controlLazyMode";
import { tooltip } from "@Obsidian/Utility/tooltip";
import { popover } from "@Obsidian/Utility/popover";

export default defineComponent({
    name: "Crm.PersonDetail.Badges",

    components: {
        Alert,
        EntityTagList
    },

    setup() {
        const config = useConfigurationValues<BadgesConfigurationBox>();

        // #region Values

        const containerRef = ref<HTMLElement | null>(null);

        // #endregion

        // #region Computed Values

        const topLeftBadges = computed((): string => {
            return config.topLeftBadges?.map(b => b.html ?? "").join("") ?? "";
        });

        const topMiddleBadges = computed((): string => {
            return config.topMiddleBadges?.map(b => b.html ?? "").join("") ?? "";
        });

        const topRightBadges = computed((): string => {
            return config.topRightBadges?.map(b => b.html ?? "").join("") ?? "";
        });

        const bottomLeftBadges = computed((): string => {
            return config.bottomLeftBadges?.map(b => b.html ?? "").join("") ?? "";
        });

        const bottomRightBadges = computed((): string => {
            return config.bottomRightBadges?.map(b => b.html ?? "").join("") ?? "";
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        const script =
            [
                ...config.topLeftBadges ?? [],
                ...config.topMiddleBadges ?? [],
                ...config.topRightBadges ?? [],
                ...config.bottomLeftBadges ?? [],
                ...config.bottomRightBadges ?? []
            ]
                .map(b => b.javaScript ?? "").join("");

        if (script !== "") {
            console.log("script", script);
            // Add the script on the next tick to ensure the HTML has been rendered.
            nextTick(() => {
                const scriptNode = document.createElement("script");
                scriptNode.type = "text/javascript";
                scriptNode.appendChild(document.createTextNode(script));
                document.body.appendChild(scriptNode);
            });
        }

        nextTick(() => {
            if (!containerRef.value) {
                return;
            }

            tooltip(Array.from(containerRef.value.querySelectorAll(".rockbadge[data-toggle=\"tooltip\"]")));
            popover(Array.from(containerRef.value.querySelectorAll(".rockbadge[data-toggle=\"popover\"]")));
        });

        return {
            bottomLeftBadges,
            bottomRightBadges,
            containerRef,
            entityKey: config.personKey,
            entityTypeGuid: EntityType.Person,
            lazyMode: ControlLazyMode.Eager,
            topLeftBadges,
            topMiddleBadges,
            topRightBadges
        };
    },

    template: `
<div ref="containerRef" class="card card-badges">
    <div class="card-badge-top">
        <div class="rockbadge-container" v-html="topLeftBadges"></div>

        <div class="rockbadge-container" v-html="topMiddleBadges"></div>

        <div class="rockbadge-container" v-html="topRightBadges"></div>
    </div>

    <div class="card-badge-bottom">
        <div class="rockbadge-container rockbadge-container-xs" v-html="bottomLeftBadges"></div>

        <div class="rockbadge-container rockbadge-container-xs">
            <EntityTagList :entityTypeGuid="entityTypeGuid"
                :entityKey="entityKey"
                :lazyMode="lazyMode" />
         </div>

        <div class="rockbadge-container rockbadge-container-xs" v-html="bottomRightBadges"></div>
   </div>
</div>
`
});
