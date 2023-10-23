import { Guid } from "@Obsidian/Types";
import { newGuid } from "@Obsidian/Utility/guid";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { reactive } from "vue";

/**
 * Convert a simpler set of parameters into PublicAttribute
 * @param name
 * @param fieldTypeGuid
 * @param configValues
 */
export const getAttributeData = (name: string, fieldTypeGuid: Guid, configValues: Record<string, string>): Record<string, PublicAttributeBag> => {
    const configurationValues = configValues;

    return {
        "value1": reactive({
            fieldTypeGuid: fieldTypeGuid,
            name: `${name} 1`,
            key: "value1",
            description: `This is the description of the ${name} without an initial value`,
            configurationValues,
            isRequired: false,
            attributeGuid: newGuid(),
            order: 0,
            categories: []
        }),
        "value2": reactive({
            fieldTypeGuid: fieldTypeGuid,
            name: `${name} 2`,
            key: "value2",
            description: `This is the description of the ${name} with an initial value`,
            configurationValues,
            isRequired: false,
            attributeGuid: newGuid(),
            order: 0,
            categories: []
        })
    };
};

