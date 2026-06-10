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

import { nextTick, Ref } from "vue";
import { ScheduleCoordinatorNotificationType } from "@Obsidian/Enums/Group/scheduleCoordinatorNotificationType";
import { TimeIntervalUnit } from "@Obsidian/Enums/Core/timeIntervalUnit";
import { FieldType } from "@Obsidian/SystemGuids/fieldType";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { isNullish } from "@Obsidian/Utility/util";
import { TimePickerValue } from "@Obsidian/ViewModels/Controls/timePickerValue";
import { TimeIntervalBag } from "@Obsidian/ViewModels/Utility/timeIntervalBag";
import { PublicEditableAttributeBag } from "@Obsidian/ViewModels/Utility/publicEditableAttributeBag";
import { GroupDetailBlockActionInvoker } from "./types.partial";

/**
 * Smoothly scrolls a NotificationBox (or any Vue component instance
 * with a $el root) into view after the next DOM update tick. Used by
 * the error / warning surfaces in GroupDetail so the user sees the
 * banner even if they were scrolled mid-form when it appeared. No-ops
 * when the ref isn't populated.
 */
export function scrollNotificationIntoView(notificationRef: Ref<{ $el?: Element } | null | undefined>): void {
    nextTick(() => {
        const el = notificationRef.value?.$el;
        if (el) {
            el.scrollIntoView({
                behavior: "smooth",
                block: "nearest"
            });
        }
    });
}

/**
 * Converts a 0-1 decimal multiplier into the 0-100 percent integer the
 * NumberBox edit input expects. Null stays null.
 */
export function percentToInt(value: number | null | undefined): number | null {
    if (value == null) {
        return null;
    }
    return Math.round(value * 100);
}

/**
 * Inverse of percentToInt: converts a 0-100 percent input back into a 0-1
 * decimal for persistence. Null stays null.
 */
export function intToPercent(value: number | null | undefined): number | null {
    if (value == null) {
        return null;
    }
    return value / 100;
}

/**
 * Formats a 0-1 decimal multiplier as an integer placeholder string used on
 * the override inputs to show the inherited group-type value. The "%" sign
 * is intentionally omitted because the input renders a "%" addon next to
 * the value, so including it here would double the symbol.
 */
export function formatPercent(value: number | undefined | null): string {
    if (value == null) {
        return "";
    }
    return Math.round(value * 100).toString();
}

/**
 * Converts the Group's ScheduleCoordinatorNotificationTypes flag bitmask
 * into a list of stringified flag values for the CheckBoxList.
 */
export function extractFlagValues(flags: number): string[] {
    const result: string[] = [];
    if ((flags & ScheduleCoordinatorNotificationType.Decline) !== 0) {
        result.push(ScheduleCoordinatorNotificationType.Decline.toString());
    }
    if ((flags & ScheduleCoordinatorNotificationType.Accept) !== 0) {
        result.push(ScheduleCoordinatorNotificationType.Accept.toString());
    }
    if ((flags & ScheduleCoordinatorNotificationType.SelfSchedule) !== 0) {
        result.push(ScheduleCoordinatorNotificationType.SelfSchedule.toString());
    }
    return result;
}

/**
 * Inverse of extractFlagValues: combines stringified flag values back into
 * a single bitmask for persistence.
 */
export function combineFlagValues(values: string[]): number {
    let combined = 0;
    for (const v of values) {
        combined |= parseInt(v, 10) || 0;
    }
    return combined;
}

/**
 * Encodes an inherit-aware boolean override (true / false / null) as the
 * string value a `<RadioButtonList>` bound to `triStateOverrideItems`
 * consumes: `null` → `""` (Inherit from Group Type), `true` → `"true"`
 * (Yes), `false` → `"false"` (No).
 */
export function inheritOverrideToRadioValue(value: boolean | null | undefined): string {
    if (value == null) {
        return "";
    }
    return value ? "true" : "false";
}

/**
 * Inverse of `inheritOverrideToRadioValue`. Decodes the
 * `<RadioButtonList>` value back into the inherit-aware boolean override.
 * Empty string and unknown values map to `null` (Inherit from Group Type).
 */
export function radioValueToInheritOverride(value: string): boolean | null {
    if (value === "true") {
        return true;
    }
    if (value === "false") {
        return false;
    }
    return null;
}

/**
 * Parses a string into a non-negative integer. Empty, whitespace, and
 * unparseable inputs map to null.
 */
export function nullableInt(value: string): number | null {
    if (!value) {
        return null;
    }
    const n = parseInt(value, 10);
    return Number.isNaN(n) ? null : n;
}

/**
 * Parses an ISO-8601 time-of-day string (e.g., "13:30:00") into the
 * TimePickerValue object the &lt;TimePicker&gt; expects ({ hour, minute }).
 * Returns an empty object when the input is empty.
 */
export function parseTimeString(value: string | null | undefined): TimePickerValue {
    if (!value) {
        return {};
    }
    const parts = value.split(":");
    const hour = parseInt(parts[0] ?? "", 10);
    const minute = parseInt(parts[1] ?? "", 10);
    return {
        hour: Number.isNaN(hour) ? undefined : hour,
        minute: Number.isNaN(minute) ? undefined : minute
    };
}

/**
 * Inverse of parseTimeString: converts a TimePickerValue back into an
 * ISO-8601 time-of-day string ("HH:mm:ss"). Returns null when the input
 * has no hour or minute.
 */
export function formatTimeValue(value: TimePickerValue): string | null {
    if (value.hour == null || value.minute == null) {
        return null;
    }
    const hh = value.hour.toString().padStart(2, "0");
    const mm = value.minute.toString().padStart(2, "0");
    return `${hh}:${mm}:00`;
}

/**
 * Converts a minute count into a TimeIntervalBag using the largest whole
 * unit (days, hours, or minutes). Mirrors the sibling implementation at
 * Rock.JavaScript.Obsidian.Blocks/src/Cms/PersonalizationSegmentDetail/utilities.partial.ts
 * so the two blocks share identical interval-conversion semantics.
 *
 * @param minutes The number of minutes to convert.
 * @returns A TimeIntervalBag representing the interval.
 */
export function minutesToIntervalBag(minutes: number): TimeIntervalBag {
    if (minutes <= 0) {
        return { unit: TimeIntervalUnit.Days, value: 1 };
    }

    let value = minutes;
    let unit;

    if (minutes % 1440 === 0) {
        value = minutes / 1440;
        unit = TimeIntervalUnit.Days;
    }
    else if (minutes % 60 === 0) {
        value = minutes / 60;
        unit = TimeIntervalUnit.Hours;
    }
    else {
        value = minutes;
        unit = TimeIntervalUnit.Minutes;
    }

    return { unit, value };
}

/**
 * Converts a TimeIntervalBag back into a minute count. Mirrors the sibling
 * implementation at PersonalizationSegmentDetail/utilities.partial.ts.
 *
 * @param bag The interval bag to convert.
 * @returns The number of minutes represented by the bag.
 */
export function intervalBagToMinutes(bag: TimeIntervalBag | null): number {
    if (isNullish(bag) || isNullish(bag.value)) {
        return 0;
    }

    const value = bag.value;

    switch (bag.unit) {
        case TimeIntervalUnit.Days:
            return value * 1440;
        case TimeIntervalUnit.Hours:
            return value * 60;
        case TimeIntervalUnit.Minutes:
            return value;
        default:
            return 1440;
    }
}

/**
 * Formats a raw minute count as a human-readable string ("5 minutes",
 * "2 hours", "1 day"). Picks the largest unit that divides cleanly.
 * Returns an empty string for null / undefined input.
 */
export function formatScheduleInterval(minutes: number | null | undefined): string {
    if (minutes == null) {
        return "";
    }
    if (minutes % 1440 === 0) {
        const days = minutes / 1440;
        return `${days} ${days === 1 ? "day" : "days"}`;
    }
    if (minutes % 60 === 0) {
        const hours = minutes / 60;
        return `${hours} ${hours === 1 ? "hour" : "hours"}`;
    }
    return `${minutes} ${minutes === 1 ? "minute" : "minutes"}`;
}

/**
 * Creates a new attribute instance suitable for editing with the AttributeEditor control.
 * Centralized to reduce the risk of fixing attribute-default bugs in one place but not another.
 */
export function createNewAttribute(): PublicEditableAttributeBag {
    return {
        guid: "",
        name: "",
        description: "",
        isActive: true,
        isPublic: false,
        isRequired: false,
        isShowOnBulk: false,
        isShowInGrid: false,
        isAnalytic: false,
        isAnalyticHistory: false,
        isAllowSearch: false,
        isEnableHistory: false,
        isIndexEnabled: false,
        isSystem: false,
        fieldTypeGuid: FieldType.Text,
        configurationValues: {},
        categories: [],
        key: "",
        abbreviatedName: "",
        preHtml: "",
        postHtml: "",
        defaultValue: "",
        isSuppressHistoryLogging: false,
        attributeColor: "",
        iconCssClass: ""
    };
}

/**
 * Creates a typed invoker for the Group Detail block's `[BlockAction]`
 * methods. Wraps `useInvokeBlockAction` so call sites stay free of magic
 * strings, request-shape boilerplate, and response-type casts.
 *
 * @see GroupDetailBlockActionInvoker for the contract each method exposes.
 */
export function useInvokeGroupDetailBlockAction(): GroupDetailBlockActionInvoker {
    const invokeBlockAction = useInvokeBlockAction();

    return {
        edit(key) {
            return invokeBlockAction("Edit", { key });
        },
        save(box) {
            return invokeBlockAction("Save", { box });
        },
        delete(key) {
            return invokeBlockAction("Delete", { key });
        },
        archive(key) {
            return invokeBlockAction("Archive", { key });
        },
        archiveWithChildren(key) {
            return invokeBlockAction("ArchiveWithChildren", { key });
        },
        copy(bag) {
            return invokeBlockAction("Copy", { bag });
        },
        canDeleteEntity(request) {
            return invokeBlockAction("CanDeleteEntity", { request });
        },
        getGroupRequirementOptions(groupTypeId) {
            return invokeBlockAction("GetGroupRequirementOptions", { groupTypeId });
        },
        getGroupSyncOptions() {
            return invokeBlockAction("GetGroupSyncOptions");
        },
        getGroupTypeOptions(groupTypeId) {
            return invokeBlockAction("GetGroupTypeOptions", { groupTypeId });
        },
        getParentGroupInfo(parentGroupKey) {
            return invokeBlockAction("GetParentGroupInfo", { parentGroupKey });
        },
        getAllowedChildGroupTypes(parentGroupKey) {
            return invokeBlockAction("GetAllowedChildGroupTypes", { parentGroupKey });
        },
        getFamilyMemberLocationOptions(groupKey) {
            return invokeBlockAction("GetFamilyMemberLocationOptions", { key: groupKey });
        }
    };
}
