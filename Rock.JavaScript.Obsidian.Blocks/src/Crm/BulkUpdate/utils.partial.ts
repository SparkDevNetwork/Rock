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

import { Ref, nextTick } from "vue";
import { BulkUpdateActionSpecifier } from "@Obsidian/Enums/Crm/bulkUpdateActionSpecifier";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { AttributeUpdateItem, BulkUpdateBlockActionInvoker, ChangeLine, ChangeSegment } from "./types.partial";

/**
 * Builds a plain-text segment for a change-summary line.
 */
export function changeText(value: string): ChangeSegment {
    return { text: value, isChip: false };
}

/**
 * Builds a chip segment (a dynamic entity or value name) for a change-summary line.
 */
export function changeChip(value: string): ChangeSegment {
    return { text: value, isChip: true };
}

/**
 * Resolves a friendly, display-ready text for an attribute value. The value is
 * in the public "edit" format the attribute editor produced (e.g. a JSON
 * envelope for a DefinedValue), so it is run through the field type's
 * getTextValue to recover the human-readable text. Falls back to the raw value
 * when the field type cannot be resolved or yields no text.
 *
 * @param attribute The attribute the value belongs to.
 * @param value The public "edit" value entered by the user.
 * @returns The display text, or an empty string when the value is blank.
 */
export function formatAttributeValue(attribute: PublicAttributeBag, value: string): string {
    if (!value) {
        return "";
    }

    const fieldType = attribute.fieldTypeGuid ? getFieldType(attribute.fieldTypeGuid) : null;

    return fieldType?.getTextValue(value, attribute.configurationValues ?? {}) || value;
}

/**
 * Collects the attribute values to send for an Add or Update action.
 * Add: applies every entered value. Update: includes only items toggled on.
 *
 * @param action The bulk-update action discriminator (Add, Update, Remove).
 * @param addValues The user-entered values keyed by attribute key (Add path).
 * @param updateItems The per-attribute opt-in items (Update path).
 * @returns The attribute values to send, or null if none were collected.
 */
export function collectActiveAttributeValues(
    action: BulkUpdateActionSpecifier,
    addValues: Record<string, string>,
    updateItems: AttributeUpdateItem[]
): Record<string, string> | null {
    const attrs: Record<string, string> = {};

    if (action === BulkUpdateActionSpecifier.Add) {
        Object.assign(attrs, addValues);
    }
    else if (action === BulkUpdateActionSpecifier.Update) {
        for (const item of updateItems) {
            if (item.isActive && item.attribute.key) {
                attrs[item.attribute.key] = item.value;
            }
        }
    }

    return Object.keys(attrs).length > 0 ? attrs : null;
}

/**
 * Builds the change-summary lines for an Add or Update attribute action.
 * Add: emits a line per entered value. Update: emits a line per toggled item.
 *
 * @param action The bulk-update action discriminator (Add, Update, Remove).
 * @param addValues The user-entered values keyed by attribute key (Add path).
 * @param attrDict The attribute lookup keyed by attribute key (for name resolution on the Add path).
 * @param updateItems The per-attribute opt-in items (Update path).
 * @param prefix Label prefix prepended to each summary line (e.g. "Group Member").
 * @returns The summary lines to append to the change list; empty when no lines apply.
 */
export function summarizeActiveAttributes(
    action: BulkUpdateActionSpecifier,
    addValues: Record<string, string>,
    attrDict: Record<string, PublicAttributeBag>,
    updateItems: AttributeUpdateItem[],
    prefix: string
): ChangeLine[] {
    const lines: ChangeLine[] = [];

    if (action === BulkUpdateActionSpecifier.Add) {
        for (const [key, value] of Object.entries(addValues)) {
            if (value) {
                const attribute = attrDict[key];
                const name = attribute?.name || key;
                const displayValue = attribute ? formatAttributeValue(attribute, value) : value;
                lines.push([changeText(`Update ${prefix} `), changeChip(name), changeText(" to value of "), changeChip(displayValue), changeText(".")]);
            }
        }
    }
    else if (action === BulkUpdateActionSpecifier.Update) {
        for (const item of updateItems) {
            if (item.isActive) {
                const name = item.attribute.name ?? "";
                lines.push(item.value
                    ? [changeText(`Update ${prefix} `), changeChip(name), changeText(" to value of "), changeChip(formatAttributeValue(item.attribute, item.value)), changeText(".")]
                    : [changeText(`Clear ${prefix} `), changeChip(name), changeText(".")]);
            }
        }
    }

    return lines;
}

/**
 * Smoothly scrolls a NotificationBox (or any component ref exposing `$el`)
 * into view. Waits one tick so the element is in the DOM after a v-if flip,
 * and uses `block: "nearest"` so already-visible notifications stay put.
 *
 * @param notificationRef The template ref bound to the NotificationBox.
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
 * Creates a typed invoker for the Bulk Update block's `[BlockAction]`
 * methods. Wraps `useInvokeBlockAction` so call sites stay free of magic
 * strings, request-shape boilerplate, and response-type casts.
 *
 * @see BulkUpdateBlockActionInvoker for the contract each method exposes.
 */
export function useInvokeBulkUpdateBlockAction(): BulkUpdateBlockActionInvoker {
    const invokeBlockAction = useInvokeBlockAction();

    return {
        getUpdatePerson(personAliasGuid) {
            return invokeBlockAction("GetUpdatePerson", { personAliasGuid });
        },
        getGraduationYearFromGrade(gradeValueGuid) {
            return invokeBlockAction("GetGraduationYearFromGrade", { gradeValueGuid });
        },
        getGroupRoles(groupGuid) {
            return invokeBlockAction("GetGroupRoles", { groupGuid });
        },
        save(bag, sessionId) {
            return invokeBlockAction("Save", { bag, sessionId });
        },
        getGroupMemberAttributes(groupGuid) {
            return invokeBlockAction("GetGroupMemberAttributes", { groupGuid });
        },
        getStepAttributes(stepTypeGuid) {
            return invokeBlockAction("GetStepAttributes", { stepTypeGuid });
        }
    };
}
