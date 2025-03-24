import { computed, ComputedRef, Ref } from "vue";
import { Guid } from "@Obsidian/Types";
import { newGuid } from "@Obsidian/Utility/guid";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { FieldTypeEditorUpdateAttributeConfigurationOptionsBag } from "@Obsidian/ViewModels/Controls/fieldTypeEditorUpdateAttributeConfigurationOptionsBag";

/**
 * Convert a simpler set of parameters into PublicAttribute
 * @param name
 * @param fieldTypeGuid
 * @param editorValue
 */
export const getAttributeData = (name: string, fieldTypeGuid: Guid, editorValue: Ref<FieldTypeEditorUpdateAttributeConfigurationOptionsBag>): ComputedRef<Record<string, PublicAttributeBag>> => {
    return computed(() => {
        return {
            "value1": {
                fieldTypeGuid: fieldTypeGuid,
                name: `${name} 1`,
                key: "value1",
                description: `This is the description of the ${name} without an initial value`,
                configurationValues: editorValue.value.configurationValues,
                isRequired: false,
                attributeGuid: newGuid(),
                order: 0,
                categories: []
            },
            "value2": {
                fieldTypeGuid: fieldTypeGuid,
                name: `${name} 2`,
                key: "value2",
                description: `This is the description of the ${name} with an initial value`,
                configurationValues: editorValue.value.configurationValues,
                isRequired: false,
                attributeGuid: newGuid(),
                order: 0,
                categories: []
            }
        };
    });
};

