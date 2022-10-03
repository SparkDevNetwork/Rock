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
import { FollowingGetFollowingOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/followingGetFollowingOptionsBag";
import { FollowingGetFollowingResponseBag } from "@Obsidian/ViewModels/Rest/Controls/followingGetFollowingResponseBag";
import { FollowingSetFollowingOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/followingSetFollowingOptionsBag";
import { computed, defineComponent, PropType, ref, watch } from "vue";

/** Displays a following icon for the given entity. */
export default defineComponent({
    name: "Following",

    components: {
    },

    props: {
        /** The unique identifier of the entity type that will be followed. */
        entityTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        /** The identifier key of the entity that will be followed. */
        entityKey: {
            type: String as PropType<string>,
            required: false
        },

        /** True if the following state should not be modified. */
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props) {
        // #region Values

        const http = useHttp();
        const isEntityFollowed = ref<boolean | null>(null);

        // #endregion

        // #region Computed Values

        const isVisible = computed((): boolean => {
            return !!props.entityTypeGuid && !!props.entityKey;
        });

        const followingClass = computed((): string => {
            if (props.disabled) {
                return isEntityFollowed ? "text-primary" : "";
            }

            return isEntityFollowed.value ? "clickable text-primary" : "clickable";
        });

        const iconClass = computed((): string => {
            return isEntityFollowed.value ? "fa fa-star" : "fa fa-star-o";
        });

        const tooltip = computed((): string | null => {
            if (props.disabled) {
                return null;
            }

            return isEntityFollowed.value ? "Click to stop following." : "Click to follow.";
        });

        // #endregion

        // #region Functions

        /**
         * Get the current followed state for the entity we are displaying in
         * this following control.
         */
        const getEntityFollowedState = async (): Promise<void> => {
            // If we don't have an entity then mark the state as "unknown".
            if (!props.entityTypeGuid || !props.entityKey) {
                isEntityFollowed.value = null;
                return;
            }

            const data: FollowingGetFollowingOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey
            };

            const response = await http.post<FollowingGetFollowingResponseBag>("/api/v2/Controls/FollowingGetFollowing", undefined, data);

            isEntityFollowed.value = response.isSuccess && response.data && response.data.isFollowing;
        };

        // #endregion

        // #region Event Handlers

        /**
         * Called when the follow icon has been clicked. Attempt to toggle
         * the followed state of the entity.
         */
        const onFollowClick = async (): Promise<void> => {
            // If we are disabled, don't allow the individual to change the state.
            if (props.disabled) {
                return;
            }

            // Shouldn't really happen, but just make sure we have everything.
            if (isEntityFollowed.value === null || !props.entityTypeGuid || !props.entityKey) {
                return;
            }

            const data: FollowingSetFollowingOptionsBag = {
                entityTypeGuid: props.entityTypeGuid,
                entityKey: props.entityKey,
                isFollowing: !isEntityFollowed.value
            };

            const response = await http.post("/api/v2/Controls/FollowingSetFollowing", undefined, data);

            // If we got a 200 OK response then we can toggle our internal state.
            if (response.isSuccess) {
                isEntityFollowed.value = !isEntityFollowed.value;
            }
            else {
                await alert("Unable to update followed state.");
            }
        };

        // #endregion

        // Watch for property values to change and when they do reload the
        // initial followed state.
        watch([() => props.entityTypeGuid, () => props.entityKey], () => {
            getEntityFollowedState();
        });

        getEntityFollowedState();

        return {
            followingClass,
            iconClass,
            isVisible,
            onFollowClick,
            tooltip
        };
    },

    template: `
<span v-if="isVisible" :class="followingClass" :title="tooltip" @click="onFollowClick">
    <i :class="iconClass"></i>
</span>
`
});
