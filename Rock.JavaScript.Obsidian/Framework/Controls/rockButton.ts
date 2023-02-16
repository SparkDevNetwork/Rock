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
import { computed, defineComponent, PropType, ref } from "vue";
import { isPromise } from "@Obsidian/Utility/promiseUtils";

export enum BtnType {
    Default = "default",
    Primary = "primary",
    Danger = "danger",
    Warning = "warning",
    Success = "success",
    Info = "info",
    Link = "link"
}

export enum BtnSize {
    Default = "",
    ExtraSmall = "xs",
    Small = "sm",
    Large = "lg"
}

export default defineComponent({
    name: "RockButton",

    props: {
        isLoading: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        loadingText: {
            type: String as PropType<string>,
            default: "Loading..."
        },
        type: {
            type: String as PropType<string>,
            default: "button"
        },
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        btnType: {
            type: String as PropType<BtnType>,
            default: BtnType.Default
        },
        btnSize: {
            type: String as PropType<BtnSize>,
            default: BtnSize.Default
        },
        autoLoading: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * Automatically disables the button when it is in a loading state or
         * the click handler is processing. This can prevent duplicate clicks.
         */
        autoDisable: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        onClick: {
            type: Function as PropType<((event: MouseEvent) => void | PromiseLike<void>)>,
            required: false
        },

        /** Change button proportions to make it a square. Used for buttons with only an icon. */
        isSquare: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
    ],

    setup(props) {
        const isProcessing = ref(false);

        const isButtonDisabled = computed((): boolean => {
            return props.disabled || (props.autoDisable && isProcessing.value) || props.isLoading;
        });

        const isButtonLoading = computed((): boolean => {
            return props.isLoading || (props.autoLoading && isProcessing.value);
        });

        const typeClass = computed((): string => {
            return `btn-${props.btnType}`;
        });

        const sizeClass = computed((): string => {
            if (!props.btnSize) {
                return "";
            }

            return `btn-${props.btnSize}`;
        });

        const cssClass = computed((): string => {
            return `btn ${typeClass.value} ${sizeClass.value} ${props.isSquare ? "btn-square" : ""}`;
        });

        const onButtonClick = async (event: MouseEvent): Promise<void> => {
            if (isButtonDisabled.value || isButtonLoading.value) {
                return;
            }

            isProcessing.value = true;

            try {
                const clickHandler = props.onClick;

                if (clickHandler) {
                    const result = clickHandler(event);

                    if (isPromise(result)) {
                        await result;
                    }
                }
            }
            finally {
                isProcessing.value = false;
            }
        };

        return {
            cssClass,
            isButtonDisabled,
            isButtonLoading,
            onButtonClick
        };
    },

    template: `
<button :class="cssClass" :disabled="isButtonDisabled" @click="onButtonClick" :type="type">
    <template v-if="isButtonLoading">
        {{loadingText}}
    </template>
    <slot v-else />
</button>`
});
