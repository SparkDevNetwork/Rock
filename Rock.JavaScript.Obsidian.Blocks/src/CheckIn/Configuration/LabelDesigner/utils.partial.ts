import Konva from "@Obsidian/Libs/konva";
import { HorizontalTextAlignment, LabelFieldBag, LabelFieldType, LineFieldConfigurationBag, RectangleFieldConfigurationBag, StringRecord, TextFieldConfigurationBag } from "./types.partial";
import { newGuid } from "@Obsidian/Utility/guid";

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
        const config = field.configurationValues as StringRecord<TextFieldConfigurationBag>;

        config.fontSize = "14";
        config.horizontalAlignment = `${HorizontalTextAlignment.Left}`;
        config.isBold = "false";
        config.isColorInverted = "false";
        config.isCondensed = "false";
        config.placeholderText = "Text 1";

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

    return field;
}

// #endregion
