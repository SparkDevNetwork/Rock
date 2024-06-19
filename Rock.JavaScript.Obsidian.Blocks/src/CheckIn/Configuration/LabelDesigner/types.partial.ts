import { DesignedLabelBag } from "@Obsidian/ViewModels/CheckIn/Labels/designedLabelBag";
import { LabelDetailBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Configuration/LabelDesigner/labelDetailBag";
import { LabelFieldBag } from "@Obsidian/ViewModels/CheckIn/Labels/labelFieldBag";
import { LabelFieldType } from "@Obsidian/Enums/CheckIn/Labels/labelFieldType";

/**
 * The size of something measured in width and height. This is used to pass
 * around the label size.
 */
export type Size = {
    /** The width of the item. */
    width: number;

    /** The height of the item. */
    height: number;
};

/**
 * This is a copy of the {@link LabelDetailBag} type with some of the
 * properties re-configured to be required for simplicity.
 */
export type LabelDetail = LabelDetailBag & {
    labelData: DesignedLabel;
};

/**
 * This is a copy of the {@link DesignedLabelBag} type with some of the
 * properties re-configured to be required for simplicity.
 */
export type DesignedLabel = DesignedLabelBag & {
    fields: LabelFieldBag[];
};

/**
 * The custom drag data used to handle drag and drop for new fields.
 */
export type DragData = {
    /** The type of field being dragged. */
    type: LabelFieldType;

    /** The sub-type of the field being dragged. */
    subtype: number;
};

/**
 * The result from the preview request to the block action.
 */
export type PreviewResultBag = {
    /** The ZPL content of the preview. */
    content?: string | null;

    /** The duration in milliseconds it took to generate. */
    duration: number;
};
