import Konva from "@Obsidian/Libs/konva";
import { HorizontalTextAlignment, LabelFieldBag, LabelFieldType, LabelTextFieldSubType, RectangleFieldConfigurationBag, StringRecord, TextFieldConfigurationBag } from "./types.partial";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { Rectangle } from "./shapes.partial";
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
 * Creates a function that will handle node drag bounding operations for the
 * stage. This ensures drag operations stay within the label and also handles
 * snapping.
 *
 * @param stage The stage the function will operate on.
 * @param snapPixel The function that will handle pixel snapping.
 * @returns A function that can be passed to the `dragBoundFunc` property.
 */
export function useNodeDragBoundFunc(stage: Konva.Stage, snapPixel: (pixel: number) => number): (this: Konva.Node, pos: Konva.Vector2d) => Konva.Vector2d {
    function nodeDragBoundFunc(this: Konva.Node, pos: Konva.Vector2d): Konva.Vector2d {
        if (!stage) {
            return pos;
        }

        // Get the new x and y positions, clamped to the left and top edges
        // of the label.
        let newX = pos.x <= 0 ? 0 : snapPixel(pos.x);
        let newY = pos.y <= 0 ? 0 : snapPixel(pos.y);

        // Clamp the x position so it doesn't extend past the right edge of
        // the label.
        if (newX >= stage.width() - this.width()) {
            newX = stage.width() - this.width();
        }

        // Clamp the y position so it doesn't extend past the bottom edge of
        // the label.
        if (newY >= stage.height() - this.height()) {
            newY = stage.height() - this.height();
        }

        return {
            x: newX,
            y: newY
        };
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
        field.height = 14/72;
    }
    else if (fieldType === LabelFieldType.Rectangle) {
        const config = field.configurationValues as StringRecord<RectangleFieldConfigurationBag>;

        config.isBlack = "true";
        config.isFilled = "true";
        config.borderThickness = "0";
        config.cornerRadius = "0";
    }

    return field;
}

/**
 * Creates a new shape for the specified field type.
 *
 * @param fieldType The field type that specifies what kind of shape to create.
 *
 * @returns A new shape or `undefined` if the field type was not recognized.
 */
export function createShapeForFieldType(fieldType: LabelFieldType): Konva.Group | Konva.Shape | undefined {
    if (fieldType === LabelFieldType.Text) {
        return new Konva.Text();
    }
    else if (fieldType === LabelFieldType.Rectangle) {
        return new Rectangle();
    }

    return undefined;
}

/**
 * Attempts to update a shape to match the information in the field. The
 * position and size are not updated. If the shape is not the right type then
 * `false` will be returned.
 *
 * @param shape The shape object to be updated.
 * @param field The field to use as the source of truth.
 *
 * @returns `true` if the shape was updated or `false` if it could not be updated.
 */
export function updateShapeFromField(shape: Konva.Shape | Konva.Group, field: LabelFieldBag): boolean {
    if (field.fieldType === LabelFieldType.Text && shape instanceof Konva.Text) {
        updateTextShapeFromField(shape, field);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Rectangle && shape instanceof Rectangle) {
        updateRectShapeFromField(shape, field);
        return true;
    }

    return false;
}

/**
 * Updates the text shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateTextShapeFromField(shape: Konva.Text, field: LabelFieldBag): void {
    if (!field.configurationValues) {
        return;
    }

    const config = field.configurationValues as StringRecord<TextFieldConfigurationBag>;

    const fontSize = toNumberOrNull(config.fontSize) ?? 12;
    const alignment = toNumber(config.horizontalAlignment) as HorizontalTextAlignment;

    shape.fontFamily(asBoolean(config.isCondensed) ? "Roboto Condensed" : "Roboto");
    shape.fontStyle(asBoolean(config.isBold) ? "bold" : "normal");
    shape.globalCompositeOperation(asBoolean(config.isColorInverted) ? "xor" : "source-over");
    shape.fontSize(fontSize * window.devicePixelRatio);

    if (field.fieldSubType === LabelTextFieldSubType.Custom) {
        if (asBoolean(config.isDynamicText)) {
            shape.text(config.placeholderText ?? "");
        }
        else {
            shape.text(config.staticTextTemplate ?? "");
        }
    }
    else {
        shape.text(config.placeholderText ?? "");
    }

    if (alignment === HorizontalTextAlignment.Center) {
        shape.align("center");
    }
    else if (alignment === HorizontalTextAlignment.Right) {
        shape.align("right");
    }
    else {
        shape.align("left");
    }
}

/**
 * Updates the rectangle shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateRectShapeFromField(shape: Rectangle, field: LabelFieldBag): void {
    if (!field.configurationValues) {
        return;
    }

    const config = field.configurationValues as StringRecord<RectangleFieldConfigurationBag>;

    const roundingIndex = toNumber(config.cornerRadius);

    if (asBoolean(config.isFilled)) {
        shape.rect.fillEnabled(true);
        shape.rect.strokeEnabled(false);
        shape.rect.strokeWidth(0);
        shape.rect.fill(asBoolean(config.isBlack) ? "black" : "white");

        // Reset rectangle to fill entire height and width.
        shape.rect.setAttrs({
            x: 0,
            y: 0,
            width: shape.width(),
            height: shape.height()
        });
    }
    else {
        shape.rect.fillEnabled(false);
        shape.rect.strokeEnabled(true);
        shape.rect.strokeWidth(toNumber(config.borderThickness));
        shape.rect.stroke(asBoolean(config.isBlack) ? "black" : "white");

        // The HTML Context always draws borders on center, so we need to adjust
        // the rectangle to simulate an inner border.
        shape.rect.setAttrs({
            x: shape.rect.strokeWidth() / 2,
            y: shape.rect.strokeWidth() / 2,
            width: shape.width() - shape.rect.strokeWidth(),
            height: shape.height() - shape.rect.strokeWidth()
        });
    }

    shape.rect.cornerRadius(roundingIndex / 8 * getPixelForOffset(Math.min(field.width, field.height)) / 2);
}

// #endregion
