import { LabelFieldType } from "@Obsidian/Enums/CheckIn/Labels/labelFieldType";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { DataSourceBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Configuration/LabelDesigner/dataSourceBag";
import { DesignedLabelBag } from "@Obsidian/ViewModels/CheckIn/Labels/designedLabelBag";
import { LabelType } from "@Obsidian/Enums/CheckIn/Labels/labelType";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { LabelFieldBag } from "@Obsidian/ViewModels/CheckIn/Labels/labelFieldBag";

export type Size = {
    width: number;

    height: number;
};

export type LabelDetail = LabelDetailBag & {
    labelData: DesignedLabel;
};

export type DesignedLabel = DesignedLabelBag & {
    fields: LabelFieldBag[];
};

export type LabelDetailBag = {
    labelData?: DesignedLabelBag | null;

    conditionalVisibility?: FieldFilterGroupBag | null;
};

export type BlockConfiguration = {
    idKey?: string | null,

    label?: LabelDetailBag | null,

    labelName?: string | null;

    labelType: LabelType;

    dataSources?: DataSourceBag[] | null;

    filterSources?: FieldFilterSourceBag[] | null;
};

export type DragData = {
    type: LabelFieldType;

    subtype: number;
};

export type RectangleFieldConfigurationBag = {
    isBlack: boolean;

    isFilled: boolean;

    borderThickness: number;

    cornerRadius: number;
};

export type LineFieldConfigurationBag = {
    isBlack: boolean;

    thickness: number;
};

export type EllipseFieldConfigurationBag = {
    isBlack: boolean;

    isFilled: boolean;

    borderThickness: number;
};

export type IconFieldConfigurationBag = {
    icon: string;
};

export type ImageFieldConfigurationBag = {
    imageData?: string | null;

    isInverted?: string | null;
};

export type BarcodeFieldConfigurationBag = {
    format?: string | null;

    isDynamic?: string | null;

    /**
     * The dynamic text template to use for a custom text field. The text
     * is used as a lava template to generate the final text to render.
     */
    dynamicTextTemplate?: string | null;
};

export type IconItem = {
    value?: string | null;

    text?: string | null;

    weight: number;

    code?: string | null;
};

/** Convert a type so that all properties are optional strings or nulls. */
export type StringRecord<T> = {
    [K in keyof T]?: string | null
};

