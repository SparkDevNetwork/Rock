import { Guid } from "@Obsidian/Types";

export type FieldComponent = {
    name: string;

    initialValue: string;

    fieldTypeGuid: Guid;

    initialConfigValues: Record<string, string>;
};
