import { computed, ComputedRef, Ref, ref } from "vue";
import type { Placement } from "@Obsidian/Libs/floating-ui";
export type PopoverOptions = { placement?: Placement, offset?: number, virtualAnchor?: () => DOMRect | null, isSheet?: () => boolean, returnTo?: () => HTMLElement | null, isDismissable?: boolean };
export type PopoverControl = {
    isOpen: Ref<boolean>;
    floating: Ref<HTMLElement | undefined>;
    open: () => Promise<boolean>;
    close: (options?: { isFocusReturned?: boolean }) => void;
    toggle: () => Promise<boolean>;
    floatingStyles: ComputedRef<Record<string, string>>;
    origin: ComputedRef<Record<string, string>>;
    isSheet: ComputedRef<boolean>;
};
export function usePopover(_anchor: Ref<HTMLElement | undefined>, floating: Ref<HTMLElement | undefined>, _options: PopoverOptions = {}): PopoverControl {
    const isOpen = ref(false);
    return { isOpen, floating, open: async () => false, close: () => undefined, toggle: async () => false, floatingStyles: computed(() => ({})), origin: computed(() => ({})), isSheet: computed(() => false) };
}
