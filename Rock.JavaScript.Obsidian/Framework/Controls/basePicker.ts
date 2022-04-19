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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import Alert from "../Elements/alert.vue";
import LoadingIndicator from "../Elements/loadingIndicator";
import { useVModelPassthrough } from "../Util/component";
import { isPromise } from "../Util/util";

function isDescendant(parent: Node, child: Node): boolean {
    for (let node: Node | null = child; node !== null; node = node.parentNode) {
        if (node === parent) {
            return true;
        }
    }

    return false;
}

export default defineComponent({
    name: "BasePicker",

    components: {
        Alert,
        LoadingIndicator
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        iconClass: {
            type: String as PropType<string>,
            required: false
        },

        text: {
            type: String as PropType<string>,
            required: false
        },

        saveText: {
            type: String as PropType<string>,
            default: "Save"
        },

        cancelText: {
            type: String as PropType<string>,
            default: "Cancel"
        },

        onLoad: {
            type: Function as PropType<() => void | Promise<void>>,
            required: false
        },

        onClear: {
            type: Function as PropType<() => void>,
            required: false
        },

        isEagerLoad: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        save: () => true,
        "update:modelValue": (_value: boolean) => true
    },

    setup(props, { emit }) {
        const isPickerOpen = useVModelPassthrough(props, "modelValue", emit);
        const errorText = ref("");
        const isLoaded = ref(false);
        const isLoading = ref(false);
        const containerElement = ref<HTMLElement | null>(null);

        const iconClass = computed((): string => {
            if (props.iconClass) {
                if (props.iconClass.indexOf("fa-") !== -1) {
                    return `${props.iconClass} fa-fw`;
                }
                else {
                    return props.iconClass;
                }
            }
            else {
                return "";
            }
        });

        const isClearable = computed((): boolean => {
            return !!props.onClear;
        });

        const calculatedWidth = computed((): string => {
            return isClearable.value ? "calc(100% - 25px)" : "100%";
        });

        const pickerMenuStyle = computed((): Record<string, string> => {
            return {
                width: calculatedWidth.value,
                padding: "0px",
                display: isPickerOpen.value ? "inline-block" : "none"
            };
        });

        const pickerStyle = computed((): Record<string, string> => {
            return {
                width: calculatedWidth.value,
                maxWidth: calculatedWidth.value
            };
        });

        const startLoading = async (): Promise<void> => {
            isLoading.value = true;
            try {
                if (props.onLoad) {
                    const result = props.onLoad();

                    if (isPromise(result)) {
                        await result;
                    }
                }

                isLoaded.value = true;
            }
            catch (error) {
                console.error(error);
                errorText.value = "An error occurred while loading this content.";
            }
            finally {
                isLoading.value = false;
            }
        };

        const onPickerClick = (): void => {
            isPickerOpen.value = !isPickerOpen.value;

            // If we have just opened then check if we need to start loading.
            if (isPickerOpen.value) {
                if (!isLoaded.value && !isLoading.value) {
                    startLoading();
                }
            }

        };

        const onCancelClick = (): void => {
            isPickerOpen.value = false;
        };

        const onClearClick = (): void => {
            if (props.onClear) {
                props.onClear();
            }
        };

        const onSaveClick = (): void => {
            emit("save");
        };

        const onWindowClick = (event: MouseEvent): void => {
            if (containerElement.value && event.target && !isDescendant(containerElement.value, event.target as Node)) {
                isPickerOpen.value = false;
            }
        };

        watch(isPickerOpen, () => {
            if (isPickerOpen.value) {
                window.addEventListener("click", onWindowClick);
            }
            else {
                window.removeEventListener("click", onWindowClick);
            }
        });

        if (props.isEagerLoad) {
            startLoading();
        }

        return {
            containerElement,
            errorText,
            iconClass,
            isClearable,
            isLoaded,
            isPickerOpen,
            onCancelClick,
            onClearClick,
            onPickerClick,
            onSaveClick,
            pickerMenuStyle,
            pickerStyle
        };
    },

    template: `
<div ref="containerElement" class="picker picker-select rollover-container" style="width: 100%;">
    <a class="picker-label" href="#" :style="pickerStyle" @click.prevent="onPickerClick">
        <i v-if="iconClass" :class="iconClass"></i>
        <span class="selected-names">{{ text }}</span>
        <b class="fa fa-caret-down pull-right"></b>
    </a>

    <a v-if="isClearable" class="picker-select-none rollover-item" @click.prevent="onClearClick">
        <i class="fa fa-times"></i>
    </a>

    <v-style>
        .scrollbar-thin {
            scrollbar-width: thin;
        }
        .scrollbar-thin::-webkit-scrollbar {
            width: 8px;
            background-color: #bbb;
        }
    </v-style>

    <div class="picker-menu dropdown-menu"
        :style="pickerMenuStyle">
        <div class="scrollbar-thin" style="min-height: 100px; max-height: 300px; overflow-y: auto;">
            <slot v-if="isLoaded" />

            <Alert v-else-if="errorText" type="warning">{{ errorText }}</Alert>

            <LoadingIndicator v-else />
        </div>

        <div class="picker-actions" style="margin: 0px;">
            <button v-if="saveText" class="btn btn-xs btn-primary picker-btn" type="button" @click="onSaveClick">{{ savetText }}</button>
            <button v-if="cancelText" class="btn btn-xs btn-link picker-cancel" type="button" @click="onCancelClick">{{ cancelText }}</button>
        </div>
    </div>
</div>
`
});
