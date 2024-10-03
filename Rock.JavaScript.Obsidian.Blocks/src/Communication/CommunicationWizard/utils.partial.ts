import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { inject, InjectionKey, Ref } from "vue";
import { AccordionManager } from "./types.partial";
import { newGuid } from "@Obsidian/Utility/guid";

export const FontFamiliesInjectionKey: InjectionKey<ListItemBag[]> = Symbol("font-families");

/**
 * Injects a provided value.
 * Throws an exception if the value is undefined or not yet provided.
 */
function use<T>(key: string | InjectionKey<T>): T {
    const result = inject<T>(key);

    if (result === undefined) {
        throw `Attempted to access ${key.toString()} before a value was provided.`;
    }

    return result;
}

export function useFontFamilies(): ListItemBag[] {
    return use(FontFamiliesInjectionKey);
}

export const AccordionGroupInjectionKey: InjectionKey<AccordionManager> = Symbol("accordion-group");

/** Uses an accordion group if one is set up. */
export function useAccordionGroup(isExpanded: Ref<boolean>): void {
    const accordionKey = newGuid();
    const group = inject(AccordionGroupInjectionKey);
    group?.register(accordionKey, isExpanded);
}