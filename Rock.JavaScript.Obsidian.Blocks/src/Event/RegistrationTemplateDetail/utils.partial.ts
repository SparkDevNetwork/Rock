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

import { RegistrationFieldSource } from "@Obsidian/Enums/Event/registrationFieldSource";
import { RegistrationPersonFieldType, RegistrationPersonFieldTypeDescription } from "@Obsidian/Enums/Event/registrationPersonFieldType";
import { FilterExpressionType } from "@Obsidian/Enums/Reporting/filterExpressionType";
import { newGuid } from "@Obsidian/Utility/guid";
import { toCurrencyOrNull } from "@Obsidian/Utility/numberUtils";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { RegistrationTemplateDiscountBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationTemplateDetail/registrationTemplateDiscountBag";
import { RegistrationTemplateFeeItemBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationTemplateDetail/registrationTemplateFeeItemBag";
import { RegistrationTemplateFormBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationTemplateDetail/registrationTemplateFormBag";
import { RegistrationTemplateFormFieldBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationTemplateDetail/registrationTemplateFormFieldBag";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { CurrencyInfoBag } from "@Obsidian/ViewModels/Utility/currencyInfoBag";

/**
 * Formats a number as currency using the organization's currency settings.
 *
 * @param value The value to format. A missing value is treated as zero.
 * @param currencyInfo The currency settings of the organization.
 *
 * @returns The formatted currency text.
 */
export function formatCurrency(value: number | null | undefined, currencyInfo: CurrencyInfoBag | null | undefined): string {
    return toCurrencyOrNull(value ?? 0, currencyInfo ?? null) ?? "";
}

/**
 * Formats a date value for display using the short date pattern.
 *
 * @param value The ISO 8601 date value.
 *
 * @returns The formatted date, or an empty string when there is no value.
 */
export function formatShortDate(value: string | null | undefined): string {
    if (!value) {
        return "";
    }

    return RockDateTime.parseISO(value)?.toASPString("d") ?? "";
}

/**
 * Formats the options of a fee as a single line of text, such as
 * "Small-$10.00 ( max: 5 ), Large-$12.00".
 *
 * @param feeItems The fee items to format.
 * @param currencyInfo The currency settings of the organization.
 *
 * @returns The formatted text.
 */
export function formatFeeItems(feeItems: RegistrationTemplateFeeItemBag[] | null | undefined, currencyInfo: CurrencyInfoBag | null | undefined): string {
    return (feeItems ?? [])
        .map(item => {
            let text = `${item.name ?? ""}-${formatCurrency(item.cost, currencyInfo)}`;

            if (item.maximumUsageCount !== null && item.maximumUsageCount !== undefined) {
                text += ` ( max: ${item.maximumUsageCount} )`;
            }

            return text;
        })
        .join(", ");
}

/**
 * Formats the discount amount or percentage of a discount code.
 *
 * @param discount The discount to format.
 * @param currencyInfo The currency settings of the organization.
 *
 * @returns The formatted discount.
 */
export function formatDiscount(discount: RegistrationTemplateDiscountBag, currencyInfo: CurrencyInfoBag | null | undefined): string {
    if (discount.discountAmount > 0) {
        return formatCurrency(discount.discountAmount, currencyInfo);
    }

    return `${(discount.discountPercentage * 100).toFixed(2)} %`;
}

/**
 * Formats the usage limits of a discount code as a single line of text.
 *
 * @param discount The discount whose limits should be formatted.
 *
 * @returns The formatted limits, or an empty string when there are none.
 */
export function formatDiscountLimits(discount: RegistrationTemplateDiscountBag): string {
    const limits: string[] = [];

    if (discount.maxUsage !== null && discount.maxUsage !== undefined) {
        limits.push(`Max Usage: ${discount.maxUsage}`);
    }

    if (discount.maxRegistrants !== null && discount.maxRegistrants !== undefined) {
        limits.push(`Max Registrants: ${discount.maxRegistrants}`);
    }

    if (discount.minRegistrants !== null && discount.minRegistrants !== undefined) {
        limits.push(`Min Registrants: ${discount.minRegistrants}`);
    }

    if (discount.startDate) {
        limits.push(`Effective: ${formatShortDate(discount.startDate)}`);
    }

    if (discount.endDate) {
        limits.push(`Expires: ${formatShortDate(discount.endDate)}`);
    }

    return limits.join("; ");
}

/**
 * Gets the name to display for a form field.
 *
 * @param field The form field.
 *
 * @returns The display name of the field.
 */
export function getFieldDisplayName(field: RegistrationTemplateFormFieldBag): string {
    if (field.fieldSource === RegistrationFieldSource.PersonField) {
        return RegistrationPersonFieldTypeDescription[field.personFieldType] ?? "";
    }

    return field.name ?? "";
}

/**
 * Determines if a form field references an attribute that no longer exists.
 *
 * @param field The form field.
 *
 * @returns True when the field cannot be resolved to an attribute.
 */
export function isFieldAttributeMissing(field: RegistrationTemplateFormFieldBag): boolean {
    if (field.fieldSource === RegistrationFieldSource.PersonField) {
        return false;
    }

    if (field.fieldSource === RegistrationFieldSource.RegistrantAttribute) {
        return !field.registrantAttribute;
    }

    return !field.attribute?.value;
}

/**
 * Determines if a field is one of the protected first or last name fields of
 * the default form. Those fields cannot be removed or have their source changed.
 *
 * @param field The form field.
 * @param isDefaultForm True when the field belongs to the default form.
 *
 * @returns True when the field is protected.
 */
export function isProtectedField(field: RegistrationTemplateFormFieldBag, isDefaultForm: boolean): boolean {
    return isDefaultForm
        && field.fieldSource === RegistrationFieldSource.PersonField
        && (field.personFieldType === RegistrationPersonFieldType.FirstName || field.personFieldType === RegistrationPersonFieldType.LastName);
}

/**
 * Determines if any form collects the specified person field.
 *
 * @param forms The forms to search.
 * @param personFieldType The person field to look for.
 *
 * @returns True when at least one form collects the person field.
 */
export function hasPersonField(forms: RegistrationTemplateFormBag[], personFieldType: RegistrationPersonFieldType): boolean {
    return forms.some(form => (form.fields ?? []).some(field => field.fieldSource === RegistrationFieldSource.PersonField && field.personFieldType === personFieldType));
}

/**
 * Creates an empty visibility rule group that shows the field when all rules match.
 *
 * @returns A new rule group.
 */
export function createEmptyVisibilityRules(): FieldFilterGroupBag {
    return {
        guid: newGuid(),
        expressionType: FilterExpressionType.GroupAll,
        rules: []
    };
}

/**
 * Gets the order value for an item appended to the end of a list.
 *
 * @param items The existing items.
 *
 * @returns The next order value.
 */
export function getNextOrder(items: { order: number }[]): number {
    return items.length > 0 ? Math.max(...items.map(item => item.order)) + 1 : 0;
}

/**
 * Renumbers the order of the items to match their position in the list.
 *
 * @param items The items in their new order.
 *
 * @returns A new array with the order values updated.
 */
export function renumberOrder<T extends { order: number }>(items: T[]): T[] {
    return items.map((item, index) => ({ ...item, order: index }));
}

/**
 * Moves an item in front of another item, or to the end of the list, and
 * renumbers the order of every item.
 *
 * @param items The items to reorder.
 * @param itemKey The key of the item to move.
 * @param beforeItemKey The key of the item it should be placed before, or null to move it to the end.
 * @param getKey The function that gets the key of an item.
 *
 * @returns A new array with the item moved and the order values updated.
 */
export function moveItem<T extends { order: number }>(items: T[], itemKey: string, beforeItemKey: string | null, getKey: (item: T) => string): T[] {
    const reordered = [...items];
    const index = reordered.findIndex(item => getKey(item) === itemKey);

    if (index < 0) {
        return items;
    }

    const [movedItem] = reordered.splice(index, 1);

    if (beforeItemKey) {
        const beforeIndex = reordered.findIndex(item => getKey(item) === beforeItemKey);

        if (beforeIndex < 0) {
            reordered.push(movedItem);
        }
        else {
            reordered.splice(beforeIndex, 0, movedItem);
        }
    }
    else {
        reordered.push(movedItem);
    }

    return renumberOrder(reordered);
}
