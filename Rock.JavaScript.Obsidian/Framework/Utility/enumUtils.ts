import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

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