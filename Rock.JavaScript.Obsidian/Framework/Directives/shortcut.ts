import { Directive, DirectiveBinding } from "vue";

type KeyboardEventKey =
    // Special Values
    | "Unidentified"

    // Modifier Keys
    | "Alt" | "AltGraph" | "CapsLock" | "Control" | "Fn" | "FnLock"
    | "Meta" | "NumLock" | "ScrollLock" | "Shift" | "Super"
    | "Symbol" | "SymbolLock"

    // Whitespace Keys
    | "Enter" | "Tab" | " "

    // Navigation Keys
    | "ArrowDown" | "ArrowLeft" | "ArrowRight" | "ArrowUp"
    | "End" | "Home" | "PageDown" | "PageUp"

    // Editing Keys
    | "Backspace" | "Clear" | "Copy" | "Cut" | "Delete" | "Insert" | "Paste"
    | "Redo" | "Undo"

    // UI Control Keys
    | "Escape" | "Pause" | "PrintScreen"

    // Application Control Keys
    | "ContextMenu" | "Power" | "Sleep" | "WakeUp"

    // Function Keys
    | "F1" | "F2" | "F3" | "F4" | "F5" | "F6" | "F7" | "F8" | "F9" | "F10" | "F11" | "F12"
    | "F13" | "F14" | "F15" | "F16" | "F17" | "F18" | "F19" | "F20" | "F21" | "F22" | "F23" | "F24"

    // Numeric Keys
    | "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"

    // Alphabetic Keys
    | "a" | "b" | "c" | "d" | "e" | "f" | "g" | "h" | "i" | "j" | "k" | "l" | "m"
    | "n" | "o" | "p" | "q" | "r" | "s" | "t" | "u" | "v" | "w" | "x" | "y" | "z"

    // Symbol Keys
    | "!" | "@" | "#" | "$" | "%" | "^" | "&" | "*" | "(" | ")"
    | "-" | "_" | "+" | "=" | "[" | "{" | "]" | "}" | "\\" | "|"
    | ";" | ":" | "'" | "\"" | "," | "<" | "." | ">" | "/" | "?"
    | "`" | "~"

    // Numeric Keypad Keys
    | "NumLock" | "Numpad0" | "Numpad1" | "Numpad2" | "Numpad3" | "Numpad4"
    | "Numpad5" | "Numpad6" | "Numpad7" | "Numpad8" | "Numpad9"
    | "NumpadAdd" | "NumpadSubtract" | "NumpadMultiply" | "NumpadDivide"
    | "NumpadDecimal" | "NumpadEnter" | "NumpadEqual"

    // Media Control Keys
    | "MediaPlayPause" | "MediaStop" | "MediaTrackNext" | "MediaTrackPrevious"
    | "AudioVolumeMute" | "AudioVolumeDown" | "AudioVolumeUp"

    // Browser Control Keys
    | "BrowserBack" | "BrowserForward" | "BrowserRefresh" | "BrowserStop"
    | "BrowserSearch" | "BrowserFavorites" | "BrowserHome"

    // Miscellaneous Keys
    | "LaunchApplication1" | "LaunchApplication2" | "LaunchMail"
    | "Select"
    ;

export const vShortcut: Directive<HTMLElement, KeyboardEventKey | false> = {
    mounted(el: HTMLElement, binding: DirectiveBinding<KeyboardEventKey | false>) {
        if (binding.value === false) {
            el.removeAttribute("data-shortcut-key");
            return;
        }

        el.setAttribute("data-shortcut-key", binding.value.toLowerCase());
    },

    unmounted(el: HTMLElement) {
        el.removeAttribute("data-shortcut-key");
    }
};
