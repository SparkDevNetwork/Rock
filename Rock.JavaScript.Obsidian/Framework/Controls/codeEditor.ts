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

import { computed, defineComponent, nextTick, onMounted, PropType, ref, watch } from "vue";
import RockFormField from "@Obsidian/Controls/rockFormField";
import { loadJavaScriptAsync } from "@Obsidian/Utility/page";
import { newGuid } from "@Obsidian/Utility/guid";
import { isFullscreen, enterFullscreen, exitFullscreen } from "@Obsidian/Utility/fullscreen";
import { updateRefValue } from "@Obsidian/Utility/component";


declare global {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any,no-var
    var ace: any | undefined;
}

// The valid theme types for intellisense.
type ThemeTypes = "rock"
    | "chrome" | "crimson_editor" | "dawn" | "dreamweaver"
    | "eclipse" | "solarized_light" | "textmate" | "tomorrow"
    | "xcode" | "github" | "ambiance" | "chaos"
    | "clouds_midnight" | "cobalt" | "idle_fingers" | "kr_theme"
    | "merbivore" | "merbivore_soft" | "mono_industrial" | "monokai"
    | "pastel_on_dark" | "solarized_dark" | "terminal" | "tomorrow_night"
    | "tomorrow_night_blue" | "tomorrow_night_bright" | "tomorrow_night_eighties"
    | "twilight" | "vibrant_ink";

// The valid mode types for intellisense.
type ModeTypes = "text" | "css" | "html" | "lava"
    | "javascript" | "less" | "powershell" | "sql"
    | "typescript" | "csharp" | "markdown" | "xml";

// TODO: Move this to @Obsidan/Types somewhere.
type LiteralUnion<T extends U, U = string> = T | (U & Record<never, never>);

// Start loading the signature pad script so that it is available for us
// to use later when the control becomes visible.
const aceScriptPromise = loadJavaScriptAsync("/Scripts/ace/ace.js", () => !!window.ace);

/**
 * Gets the name of the theme to use with the ACE editor. This handles any
 * name mapping and capitalization issues.
 *
 * @param theme The name of the theme being requested.
 *
 * @returns The name of the actual theme to use with the ACE editor.
 */
function getAceTheme(theme?: string): string {
    if (!theme || theme.toLowerCase() === "rock") {
        return "github";
    }

    return theme.toLowerCase();
}

/**
 * Gets the name of the syntax mode to use with the ACE editor. This handles any
 * name mapping and capitalization issues.
 *
 * @param mode The name of the mode being requested.
 *
 * @returns The name of the actual mode to use with the ACE editor.
 */
function getAceMode(mode?: string): string {
    if (!mode) {
        return "text";
    }

    return mode.toLowerCase();
}

export default defineComponent({
    name: "CodeEditor",

    components: {
        RockFormField
    },

    props: {
        /** The text value of the code editor. */
        modelValue: {
            type: String as PropType<string>,
            default: ""
        },

        /** The name of the theme to use when styling the editor. */
        theme: {
            type: String as PropType<LiteralUnion<ThemeTypes>>,
            default: "rock"
        },

        /** The name of the syntax mode that represents the expected language. */
        mode: {
            type: String as PropType<LiteralUnion<ModeTypes>>,
            default: "text"
        },

        /** If set then line wrapping will be disabled. */
        noLineWrap: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** The height of the editor in pixels. */
        editorHeight: {
            type: Number as PropType<number>,
            required: false
        },

        /** A list of merge fields to make available. Not currently used! */
        mergeFields: {
            type: Array as PropType<string[]>,
            required: false
        },

        /** If set then the the editor will be disabled and read-only. */
        disabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        "update:modelValue": (_value: string) => true
    },

    setup(props, { emit }) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        let editor: any | unknown;
        const internalValue = ref(props.modelValue);
        const uniqueId = newGuid();

        // #region Computed Values

        const codeEditorClass = computed((): string => {
            return "code-editor-container";
        });

        const codeEditorId = computed((): string => {
            return `codeeditor-div-${uniqueId}`;
        });

        const codeEditorStyle = computed((): Record<string, string> => {
            return {
                position: "relative",
                height: `${editorHeight.value}px`
            };
        });

        const hasMergeFields = computed((): boolean => {
            return !!props.mergeFields && props.mergeFields.length > 0;
        });

        const editorHeight = computed((): number => {
            let height = props.editorHeight ?? 200;

            if (hasMergeFields.value) {
                height -= 40;
            }

            return height;
        });

        // #endregion

        // Watch for changes in the ACE configuration.
        watch(() => [props.theme, props.mode, props.noLineWrap, props.disabled], () => {
            if (editor) {
                editor.setTheme(`ace/theme/${getAceTheme(props.theme)}`);
                editor.getSession().setMode(`ace/mode/${getAceMode(props.mode)}`);
                editor.getSession().setUseWrapMode(!props.noLineWrap);
                editor.setReadOnly(props.disabled);
            }
        });

        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue);
        });

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        onMounted(async (): Promise<void> => {
            await aceScriptPromise;
            editor = ace.edit(codeEditorId.value);
            editor.setTheme(`ace/theme/${getAceTheme(props.theme)}`);
            editor.getSession().setMode(`ace/mode/${getAceMode(props.mode)}`);
            editor.getSession().setUseWrapMode(!props.noLineWrap);
            editor.setShowPrintMargin(false);
            editor.setReadOnly(props.disabled);

            // Disable warning about block scrolling.
            editor.$blockScrolling = Infinity;

            // Add custom command to toggle fullscreen mode.
            editor.commands.addCommand({
                name: "Toggle Fullscreen",
                bindKey: "F11",
                exec: async () => {
                    if (isFullscreen()) {
                        exitFullscreen();
                    }
                    else {
                        enterFullscreen(editor.container, () => editor.resize());
                    }

                    editor.resize();
                }
            });

            // Whenever the content of the editor changes, update our value.
            editor.getSession().on("change", () => {
                updateRefValue(internalValue, editor.getValue());
            });

            // Fix issue when code editor is inside a modal.
            nextTick(() => {
                editor.resize();
            });
        });

        return {
            codeEditorClass,
            codeEditorId,
            codeEditorStyle,
            editorHeight,
            internalValue,
            hasMergeFields
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-code-editor"
    name="codeeditor">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div v-if="hasMergeFields" class="codeeditor-header margin-b-md clearfix">
            </div>

            <div :class="codeEditorClass"
                :style="codeEditorStyle">
                <pre v-once
                    :id="codeEditorId"
                    class="position-absolute inset-0 m-0 ace_editor">{{ internalValue }}</pre>
            </div>
        </div>
    </template>
</RockFormField>
`
});
