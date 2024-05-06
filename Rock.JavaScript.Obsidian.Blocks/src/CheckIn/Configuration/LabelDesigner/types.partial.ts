import { Guid } from "@Obsidian/Types";

export type Size = {
    width: number;

    height: number;
};

export type LabelBag = {
    width: number;

    height: number;

    fields?: LabelFieldBag[];
};

export type LabelFieldBag = {
    guid?: Guid;

    fieldType: LabelFieldType;

    subFieldType: number;

    isIncludedOnPreview: boolean;

    left: number;

    top: number;

    width: number;

    height: number;

    rotationAngle: number;

    customDataValue?: string;

    configurationValues?: Record<string, string>;
};

export enum LabelFieldType {
    Text = 0,
    Line = 1,
    Rectangle = 2,
    Ellipse = 3,
    Icon = 4,
    Image = 5,
    Barcode = 6
}
