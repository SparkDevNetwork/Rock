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

import { defineComponent, PropType, computed, ref, onMounted } from "vue";
import { BasicSuspenseProvider, provideSuspense, useSuspense } from "@Obsidian/Utility/suspense";

/**
 * Prevents the content from displaying until all asynchronous events have completed.
 *
 * Emits a "timeout" event if the timeout is reached which caused the content to
 * display.
 *
 * Emits a "loaded" event if the content successfully loaded and was displayed.
 *
 * Emits a "ready" event any time the content is displayed, whether from timeout or loaded.
 */
export default defineComponent({
    name: "RockSuspense",

    props: {
        delay: {
            type: Number as PropType<number>,
            default: 500
        },

        timeout: {
            type: Number as PropType<number>,
            default: 5000
        }
    },

    emits: {
        loaded: () => true,
        ready: () => true,
        timeout: () => true
    },

    setup(props, { emit }) {
        const isContentLoaded = ref(false);
        const hasDelayElapsed = ref(false);
        const hasTimeoutElapsed = ref(false);

        const parentSuspense = useSuspense();
        const suspense = new BasicSuspenseProvider(parentSuspense);
        provideSuspense(suspense);

        const isContentVisible = computed((): boolean => isContentLoaded.value || hasTimeoutElapsed.value);
        const isLoadingVisible = computed((): boolean => !isContentVisible.value && hasDelayElapsed.value);

        // Start timer that triggers when the delay has elapsed so we can show
        // the loading content after a brief delay.
        setTimeout(() => hasDelayElapsed.value = true, props.delay);

        // Start timer that triggers when the timeout has elapsed so we can show
        // the content even if it hasn't fully loaded yet.
        setTimeout(() => {
            hasTimeoutElapsed.value = true;
            emit("timeout");
            emit("ready");
        }, props.timeout);

        // Wait until we are mounted. This indicates that all child components
        // have been initialized and would have submitted their pending operations
        // to the provider already.
        onMounted(() => {
            if (!suspense.hasPendingOperations()) {
                isContentLoaded.value = true;
                emit("loaded");
                emit("ready");
            }
            else {
                suspense.addFinishedHandler(() => {
                    isContentLoaded.value = true;
                    emit("loaded");
                    emit("ready");
                });
            }
        });

        return {
            isContentVisible,
            isLoadingVisible
        };
    },

    template: `
<div v-show="isContentVisible">
    <slot />
</div>

<template v-if="isLoadingVisible">
    <slot name="loading" />
</template>
`
});
