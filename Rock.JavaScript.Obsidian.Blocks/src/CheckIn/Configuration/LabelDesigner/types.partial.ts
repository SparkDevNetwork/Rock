import { Guid } from "@Obsidian/Types";

export type Size = {
    width: number;

    height: number;
};

export type DragData = {
    type: LabelFieldType;

    subtype: number;
};

export type LabelBag = {
    width: number;

    height: number;

    fields?: LabelFieldBag[];
};

export type LabelFieldBag = {
    guid?: Guid;

    fieldType: LabelFieldType;

    fieldSubType: number;

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

export type TextFieldConfigurationBag = {
    sourceKey?: string | null;

    formatterGuid?: Guid | null;

    collectionFormat: TextCollectionFormat;

    placeholderText?: string | null;

    isDynamicText: boolean;

    staticTextTemplate?: string | null;

    dynamicTextTemplate?: string | null;

    fontSize: number;

    variableFontSize?: Record<string, string> | null;

    horizontalAlignment: HorizontalTextAlignment;

    isBold: boolean;

    isColorInverted: boolean;

    isCondensed: boolean;

    maxLength: number;
};

export type RectangleFieldConfigurationBag = {
    isBlack: boolean;

    isFilled: boolean;

    borderThickness: number;

    cornerRadius: number;
};

/** Convert a type so that all properties are optional strings or nulls. */
export type StringRecord<T> = {
    [K in keyof T]?: string | null
};

export enum HorizontalTextAlignment {
    Left = 0,

    Center = 1,

    Right = 2
}

export enum TextCollectionFormat {
    FirstItemOnly = 0,

    CommaDelimited = 1,

    OnePerLine = 2,

    TwoColumn = 3
}

export enum LabelTextFieldSubType {
    Custom = 0,

    AttendeeInfo = 1,

    CheckInInfo = 2,

    AchievementInfo = 3
}
