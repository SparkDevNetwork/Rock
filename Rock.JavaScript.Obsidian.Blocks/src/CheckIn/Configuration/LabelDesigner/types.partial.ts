import { Guid } from "@Obsidian/Types";
import { LabelFieldType } from "@Obsidian/Enums/CheckIn/Labels/labelFieldType";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { DataSourceBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Configuration/LabelDesigner/dataSourceBag";

export type Size = {
    width: number;

    height: number;
};

export type BlockConfiguration = {
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
    binaryFileGuid?: Guid | null;
};

export type BarcodeFieldConfigurationBag = {
    format?: string | null;

    isDynamic?: string | null;
};

/** Convert a type so that all properties are optional strings or nulls. */
export type StringRecord<T> = {
    [K in keyof T]?: string | null
};

