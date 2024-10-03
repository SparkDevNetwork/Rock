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

import {nextTick, onMounted, onUnmounted, Ref, ref} from "vue";

export function useScreenSize(mobileThreshold: number = 480): { isMobile: Ref<boolean>, screenSize: Ref<number> } {
    const isMobile = ref(false);
    const screenSize = ref(screen.width);

    function onResize(): void {
        screenSize.value = screen.width;
        if (screen.width <= mobileThreshold) {
            isMobile.value = true;
        }
        else {
            isMobile.value = false;
        }
    }

    onMounted(() => {
        // Bit of a wait to make sure everything is fully initialized first.
        nextTick(() => {
            onResize();
        });

        window.addEventListener("resize", onResize);
    });

    onUnmounted(() => {
        window.removeEventListener("resize", onResize);
    });

    return {
        isMobile,
        screenSize
    };
}