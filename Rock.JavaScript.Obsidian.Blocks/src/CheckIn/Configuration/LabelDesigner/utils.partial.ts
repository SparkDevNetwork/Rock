import Konva from "@Obsidian/Libs/konva";
import { BarcodeFieldConfigurationBag, EllipseFieldConfigurationBag, IconFieldConfigurationBag, LineFieldConfigurationBag, RectangleFieldConfigurationBag, StringRecord } from "./types.partial";
import { areEqual, newGuid } from "@Obsidian/Utility/guid";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { LabelFieldType } from "@Obsidian/Enums/CheckIn/Labels/labelFieldType";
import { LabelFieldBag } from "@Obsidian/ViewModels/CheckIn/Labels/labelFieldBag";
import { TextFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/textFieldConfigurationBag";
import { HorizontalTextAlignment } from "@Obsidian/Enums/CheckIn/Labels/horizontalTextAlignment";
import { BarcodeFormat } from "@Obsidian/Enums/CheckIn/Labels/barcodeFormat";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { FilterExpressionType } from "@Obsidian/Enums/Reporting/filterExpressionType";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { IFieldType } from "@Obsidian/Types/fieldType";

// #region Worksurface Utilities

export const pixelsPerInch = 72 * window.devicePixelRatio;

/**
 * Gets the pixel position on the canvas.
 *
 * @param offset The offset position in inches.
 *
 * @returns The pixel position as a floating point number.
 */
export function getPixelForOffset(offset: number): number {
    return offset * pixelsPerInch;
}

/**
 * Gets the offset in inches of the pixel position.
 *
 * @param pixel The pixel position.
 *
 * @returns The offset in inches.
 */
export function getOffsetForPixel(pixel: number): number {
    return pixel / pixelsPerInch;
}

/**
 * Creates a function that will handle position bounding operations for the
 * stage. This ensures drag operations stay within the label and also handles
 * snapping.
 *
 * @param stage The stage the function will operate on.
 * @param snapPixel The function that will handle pixel snapping.
 *
 * @returns A function that can be used to keep a position and size within the stage.
 */
export function usePositionBoundFunc(stage: Konva.Stage, snapPixel: (pixel: number) => number): (pos: Konva.Vector2d, size: Konva.Vector2d) => Konva.Vector2d {
    function positionBoundFunc(pos: Konva.Vector2d, size: Konva.Vector2d): Konva.Vector2d {
        let newX = pos.x;
        let newY = pos.y;
        const width = size.x;
        const height = size.y;

        if (width >= 0) {
            // Clamp the x position to the left edge of the label.
            if (newX <= 0) {
                newX = 0;
            }

            newX = snapPixel(newX);

            // Clamp the x position so the shape doesn't extend past the right
            // edge of the label.
            if (newX >= stage.width() - width) {
                newX = stage.width() - width;
            }
        }
        else {
            // Clamp the x position to the left edge of the label.
            if (newX <= Math.abs(width)) {
                newX = Math.abs(width);
            }

            newX = snapPixel(newX + width) - width;

            // Clamp the x position so the shape doesn't extend past the right
            // edge of the label.
            if (newX >= stage.width()) {
                newX = stage.width();
            }
        }

        if (height >= 0) {
            // Clamp the y position to the top edge of the label.
            if (newY <= 0) {
                newY = 0;
            }

            newY = snapPixel(newY);

            // Clamp the y position so the shape doesn't extend past the bottom
            // edge of the label.
            if (newY >= stage.height() - height) {
                newY = stage.height() - height;
            }
        }
        else {
            // Clamp the y position to the top edge of the label.
            if (newY <= Math.abs(height)) {
                newY = Math.abs(height);
            }

            newY = snapPixel(newY + height) - height;

            // Clamp the y position so the shape doesn't extend past the bottom
            // edge of the label.
            if (newY >= stage.height()) {
                newY = stage.height();
            }
        }

        return {
            x: newX,
            y: newY
        };
    }

    return positionBoundFunc;
}

/**
 * Creates a function that will handle node drag bounding operations for the
 * stage. This ensures drag operations stay within the label and also handles
 * snapping.
 *
 * @param stage The stage the function will operate on.
 * @param snapPixel The function that will handle pixel snapping.
 *
 * @returns A function that can be passed to the `dragBoundFunc` property.
 */
export function useNodeDragBoundFunc(stage: Konva.Stage, snapPixel: (pixel: number) => number): (this: Konva.Node, pos: Konva.Vector2d) => Konva.Vector2d {
    const positionBoundFunc = usePositionBoundFunc(stage, snapPixel);

    function nodeDragBoundFunc(this: Konva.Node, pos: Konva.Vector2d): Konva.Vector2d {
        let width = this.width();
        let height = this.height();

        if (this instanceof Konva.Line) {
            width = this.points()[2];
            height = this.points()[3];
        }

        return positionBoundFunc(pos, { x: width, y: height });
    }

    return nodeDragBoundFunc;
}

/**
 * Creates a new field to be used as a default when dragging a new control
 * onto the worksurface.
 *
 * @param fieldType The field type to create.
 * @param subtype The field subtype to create.
 *
 * @returns A new field instance object.
 */
export function createDefaultField(fieldType: LabelFieldType, subtype: number): LabelFieldBag {
    const field: LabelFieldBag = {
        guid: newGuid(),
        fieldType: fieldType,
        fieldSubType: subtype,
        left: 0,
        top: 0,
        width: 0.75,
        height: 0.75,
        isIncludedOnPreview: true,
        rotationAngle: 0,
        configurationValues: {}
    };

    if (fieldType === LabelFieldType.Text) {
        const config = field.configurationValues as TextFieldConfigurationBag;

        config.fontSize = "14";
        config.horizontalAlignment = `${HorizontalTextAlignment.Left}`;
        config.isBold = "false";
        config.isColorInverted = "false";
        config.isCondensed = "false";
        config.placeholderText = "Text";
        config.staticText = "Text";

        field.width = 1.5;
        field.height = 14 / 72;
    }
    else if (fieldType === LabelFieldType.Rectangle) {
        const config = field.configurationValues as StringRecord<RectangleFieldConfigurationBag>;

        config.isBlack = "true";
        config.isFilled = "true";
        config.borderThickness = "0";
        config.cornerRadius = "0";
    }
    else if (fieldType === LabelFieldType.Line) {
        const config = field.configurationValues as StringRecord<LineFieldConfigurationBag>;

        field.height = 0;
        config.isBlack = "true";
        config.thickness = "1";
    }
    else if (fieldType === LabelFieldType.Ellipse) {
        const config = field.configurationValues as StringRecord<EllipseFieldConfigurationBag>;

        config.isBlack = "true";
        config.isFilled = "true";
        config.borderThickness = "0";
    }
    else if (fieldType === LabelFieldType.Icon) {
        const config = field.configurationValues as StringRecord<IconFieldConfigurationBag>;

        field.width = 0.25;
        field.height = 0.25;
        config.icon = "birthday_cake";
    }
    else if (fieldType === LabelFieldType.Image) {
        // Intentionally blank.
    }
    else if (fieldType === LabelFieldType.Barcode) {
        const config = field.configurationValues as StringRecord<BarcodeFieldConfigurationBag>;

        config.format = BarcodeFormat.QRCode.toString();
        config.isDynamic = "false";
    }

    return field;
}

// #endregion

export const code128Icon = "data:image/png;base64,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";

export const qrcodeIcon = "data:image/png;base64,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";

const birthdayCakeIcon = "data:image/png;base64,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";

const starIcon = "data:image/png;base64,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";

export const IconImageMap: ListItemBag[] = [
    {
        text: "Birthday Cake",
        value: "birthday_cake",
        category: birthdayCakeIcon
    },
    {
        text: "Star",
        value: "star",
        category: starIcon
    }
];

/**
 * Get the friendly formatted title of a filter group. This returns an HTML
 * string.
 *
 * @param group The group that contains the comparison type information.
 *
 * @returns An HTML formatted string with the comparison type text.
 */
export function getFilterGroupTitle(group: FieldFilterGroupBag): string {
    switch (group.expressionType) {
        case FilterExpressionType.GroupAll:
            return "<strong>Show</strong> when <strong>all</strong> of the following match:";

        case FilterExpressionType.GroupAny:
            return "<strong>Show</strong> when <strong>any</strong> of the following match:";

        case FilterExpressionType.GroupAllFalse:
            return "<strong>Hide</strong> when <strong>all</strong> of the following match:";

        case FilterExpressionType.GroupAnyFalse:
            return "<strong>Hide</strong> when <strong>any</strong> of the following match:";

        default:
            return "";
    }
}

/**
 * Get the description of the rule, including the name of the field it depends on.
 *
 * @param rule The rule to be represented.
 * @param sources The field filter sources to use when looking up the source field.
 * @param fields The fields that contain the attribute information.
 *
 * @returns A plain text string that represents the rule in a human friendly format.
 */
export function getFilterRuleDescription(rule: FieldFilterRuleBag, sources: FieldFilterSourceBag[]): string {
    const ruleSource = sources.find(s => (s.attribute && areEqual(s.attribute?.attributeGuid, rule.attributeGuid))
        || (s.property && s.property?.name === rule.propertyName));

    if (!ruleSource) {
        return "";
    }

    if (ruleSource.attribute) {
        const fieldType = getFieldType(ruleSource.attribute.fieldTypeGuid);

        if (!fieldType) {
            return "";
        }

        const descr = fieldType.getFilterValueDescription({
            comparisonType: rule.comparisonType,
            value: rule.value ?? ""
        }, ruleSource.attribute.configurationValues ?? {});

        return `${ruleSource.attribute.name} ${descr}`;
    }
    else if (ruleSource.property) {
        const fieldType = getFieldType(ruleSource.property.fieldTypeGuid);

        if (!fieldType) {
            return "";
        }

        const descr = fieldType.getFilterValueDescription({
            comparisonType: rule.comparisonType,
            value: rule.value ?? ""
        }, ruleSource.property.configurationValues ?? {});

        return `${ruleSource.property.title} ${descr}`;
    }

    return "";
}
