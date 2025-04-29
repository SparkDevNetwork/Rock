import { FieldFilterFunctionBuilder } from "@Obsidian/Core/Reporting/fieldFilter";
import { FieldFilterAttributeData } from "@Obsidian/Types/Reporting/fieldFilterAttributeData";
import { areEqual } from "@Obsidian/Utility/guid";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { EntryFormFieldBag } from "@Obsidian/ViewModels/Workflow/entryFormFieldBag";

/**
 * The context state that represents the instance to be filtered. Essentially
 * we contain the entire form data and the individual rules determine if the
 * current "state" of the form matches the rule.
 */
export type EntryFormFilterContext = {
    /** The fields on the form, used to get to the attribute data. */
    fields: EntryFormFieldBag[];

    /** The current field values on the form. */
    fieldValues: Record<string, string>;
};

/**
 * Provides the filtering logic for conditional visiblity rules on workflow
 * entry forms.
 */
export class EntryFormFilterFunctionBuilder extends FieldFilterFunctionBuilder<EntryFormFilterContext> {
    protected override getInstanceAttributeData(instance: EntryFormFilterContext, rule: FieldFilterRuleBag): FieldFilterAttributeData | undefined {
        if (!rule.attributeGuid) {
            return undefined;
        }

        return instance.fields
            .find(f => areEqual(f.attribute?.attributeGuid, rule.attributeGuid))
            ?.attribute ?? undefined;
    }

    protected override getInstanceAttributeValue(instance: EntryFormFilterContext, rule: FieldFilterRuleBag): string | undefined {
        if (!rule.attributeGuid) {
            return undefined;
        }

        return instance.fieldValues[rule.attributeGuid] ?? "";
    }
}
