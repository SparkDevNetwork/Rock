import { ScheduleCategoryExclusionBag } from "@Obsidian/ViewModels/Entities/ScheduleCategoryExclusionBag";

export type ScheduleDetailOptionsBag = {
    nextOccurrence?: string | null

    exclusions?: ScheduleCategoryExclusionBag[]| null;

    hasScheduleWarning?: boolean

    canDelete?: boolean

    hasAttendance?: boolean

    helpText?: boolean
};