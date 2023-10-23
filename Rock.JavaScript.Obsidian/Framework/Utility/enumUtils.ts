import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/**
 * A function to convert the enums to array of ListItemBag in the frontend.
 *
 * @param description The enum to be converted to an array of listItemBag as a dictionary of value to enum description
 *
 * @returns An array of ListItemBag.
 */
export function enumToListItemBag (description: Record<number, string>): ListItemBag[] {
    const listItemBagList: ListItemBag[] = [];
    for(const property in description) {
        listItemBagList.push({
            text: description[property].toString(),
            value: property.toString()
        });
    }
    return listItemBagList;
}