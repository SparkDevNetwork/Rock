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
import { FieldType } from "@Obsidian/SystemGuids/fieldType";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { areEqual } from "@Obsidian/Utility/guid";
import { pluralConditional } from "@Obsidian/Utility/stringUtils";
import { MatrixFieldDataBag } from "@Obsidian/ViewModels/Rest/Controls/matrixFieldDataBag";
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
 * when the field type cannot be resolved or yields no text. Matrix values are
 * summarized by their item count instead.
 *
 * @param attribute The attribute the value belongs to.
 * @param value The public "edit" value entered by the user.
 * @returns The display text, or an empty string when the value is blank.
 */
export function formatAttributeValue(attribute: PublicAttributeBag, value: string): string {
    if (!value) {
        return "";
    }

    if (areEqual(attribute.fieldTypeGuid, FieldType.Matrix)) {
        return formatMatrixValue(value);
    }

    const fieldType = attribute.fieldTypeGuid ? getFieldType(attribute.fieldTypeGuid) : null;

    return fieldType?.getTextValue(value, attribute.configurationValues ?? {}) || value;
}

/**
 * Summarizes a matrix editor value by its item count. Used only when the value
 * cannot be rendered as a table. The value is a JSON bag of items plus the
 * editor's attribute definitions, so it carries no display text of its own and
 * is non-empty even when the matrix has no items.
 *
 * @param value The public "edit" value produced by the matrix editor.
 * @returns The item count text, or an empty string when there are no items.
 */
function formatMatrixValue(value: string): string {
    const itemCount = parseMatrixValue(value)?.matrixItems?.length ?? 0;

    if (itemCount === 0) {
        return "";
    }

    return `${itemCount} ${pluralConditional(itemCount, "item", "items")}`;
}

/**
 * Parses a matrix editor value. An unparseable value reads as empty rather
 * than as raw text, which is also how the server treats one when saving.
 *
 * @param value The public "edit" value produced by the matrix editor.
 * @returns The parsed value, or null when it cannot be read.
 */
function parseMatrixValue(value: string): MatrixFieldDataBag | null {
    try {
        return (JSON.parse(value) as MatrixFieldDataBag) ?? null;
    }
    catch {
        return null;
    }
}

/**
 * Builds the table segment for a Matrix attribute's value, laid out with the
 * same columns and row order as the grid the operator entered it in. Each cell
 * holds a public view value that the column's own field type renders, so a
 * defined value, person or file reads the way it does in the editor rather than
 * as raw data.
 *
 * @param attribute The attribute the value belongs to.
 * @param value The public "edit" value entered by the operator.
 * @returns The table segment, or null when the attribute is not a Matrix or has no items.
 */
function changeMatrix(attribute: PublicAttributeBag, value: string): ChangeSegment | null {
    if (!value || !areEqual(attribute.fieldTypeGuid, FieldType.Matrix)) {
        return null;
    }

    const matrixData = parseMatrixValue(value);
    const items = matrixData?.matrixItems ?? [];

    if (items.length === 0) {
        return null;
    }

    // The editor sorts its columns by order alone and shows its rows in array
    // order, so both are mirrored here rather than re-sorted.
    const columns = Object.values(matrixData?.attributes ?? {})
        .sort((first, second) => (first.order ?? 0) - (second.order ?? 0));

    return {
        text: "",
        isChip: false,
        matrix: {
            columns,
            rows: items.map(item => item.viewValues ?? {})
        }
    };
}

/**
 * Builds the per-attribute opt-in items, giving each one its starting value.
 * Only Matrix attributes have one, because the Matrix editor reads its column
 * definitions out of the value and cannot render from an empty string. Every
 * other attribute starts blank.
 *
 * @param attributes The attributes to build items for, in display order.
 * @param attributeValues The starting values keyed by attribute key.
 * @returns One inactive item per attribute.
 */
export function createAttributeUpdateItems(attributes: PublicAttributeBag[], attributeValues: Record<string, string> | null | undefined): AttributeUpdateItem[] {
    return attributes.map(attribute => ({
        attribute,
        isActive: false,
        value: (attribute.key && attributeValues?.[attribute.key]) || ""
    }));
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
 * Builds one change-summary line for an attribute. A Matrix attribute holding
 * items renders its rows as a table; everything else renders as text, with a
 * blank value reading as a clear.
 *
 * @param attribute The attribute being updated.
 * @param value The public "edit" value entered by the operator.
 * @param prefix Label prefix prepended to the line, blank for Person attributes.
 * @returns The summary line.
 */
export function attributeChangeLine(attribute: PublicAttributeBag, value: string, prefix: string): ChangeLine {
    const name = attribute.name || "Attribute";
    const label = prefix ? `${prefix} ` : "";
    const matrixSegment = changeMatrix(attribute, value);

    if (matrixSegment) {
        return [changeText(`Update ${label}`), changeChip(name), changeText(" to value of:"), matrixSegment];
    }

    const displayValue = formatAttributeValue(attribute, value);

    return displayValue
        ? [changeText(`Update ${label}`), changeChip(name), changeText(" to value of "), changeChip(displayValue), changeText(".")]
        : [changeText(`Clear ${label}`), changeChip(name), changeText(".")];
}

/**
 * Builds the change-summary lines for an Add or Update attribute action.
 * Add: emits a line per value that formats to display text. Update: emits a
 * line per toggled item.
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
            const attribute = attrDict[key];

            // An attribute the operator never filled in contributes no line.
            if (!attribute || !formatAttributeValue(attribute, value)) {
                continue;
            }

            lines.push(attributeChangeLine(attribute, value, prefix));
        }
    }
    else if (action === BulkUpdateActionSpecifier.Update) {
        for (const item of updateItems) {
            if (item.isActive) {
                lines.push(attributeChangeLine(item.attribute, item.value, prefix));
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
