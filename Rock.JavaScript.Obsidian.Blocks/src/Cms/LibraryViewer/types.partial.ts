export enum PersonPreferenceKey {
    FilterExperienceLevels = "filter-experience-levels-preference",
    FilterLicenseTypes = "filter-license-types-preference",
    FilterMustBeTrending = "filter-must-be-trending-preference",
    FilterMustBePopular = "filter-must-be-popular-preference",
    FilterOrganizations = "filter-organizations-preference",
    FilterPublishedDate = "filter-published-date-preference",
    FilterTopics = "filter-topics-preference",
    Sort = "sort-preference",
}

export type MenuButton = {
    title?: string | null | undefined;
    subTitle?: string | null | undefined;
};

export type ToolbarMenuDropDownAlignment = "left" | "right";