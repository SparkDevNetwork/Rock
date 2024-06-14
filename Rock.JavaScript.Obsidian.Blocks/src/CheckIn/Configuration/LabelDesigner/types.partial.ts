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

export type IconItem = {
    value?: string | null;

    text?: string | null;

    weight: number;

    code?: string | null;
};
