import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { inject, InjectionKey } from "vue";

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