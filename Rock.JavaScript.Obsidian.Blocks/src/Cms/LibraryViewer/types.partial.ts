export enum PersonPreferenceKey {
    Sort = "sort-preference",
    Filters = "filters-preference"
}

export type MenuButton = {
    title?: string | null | undefined;
    subTitle?: string | null | undefined;
};

export type ToolbarMenuDropDownAlignment = "left" | "right";