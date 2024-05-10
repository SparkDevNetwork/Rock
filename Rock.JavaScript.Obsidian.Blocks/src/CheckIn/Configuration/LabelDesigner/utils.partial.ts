import Konva from "@Obsidian/Libs/konva";
import { HorizontalTextAlignment, LabelFieldBag, LabelFieldType, RectangleFieldConfigurationBag, StringRecord, TextFieldConfigurationBag } from "./types.partial";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";

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
 * Creates a new shape for the specified field type.
 *
 * @param fieldType The field type that specifies what kind of shape to create.
 *
 * @returns A new shape or `undefined` if the field type was not recognized.
 */
export function createShapeForFieldType(fieldType: LabelFieldType): Konva.Shape | undefined {
    if (fieldType === LabelFieldType.Text) {
        return new Konva.Text();
    }
    else if (fieldType === LabelFieldType.Rectangle) {
        const rect = new Konva.Rect();

        rect.hitFunc((ctx, rc) => {
            // By default a rectangle in outline mode will only hit test on the
            // outline and not the inner (empty) content. So we override that
            // to ensure the entire outline and fill area match a hit test.
            ctx.beginPath();
            ctx.rect(0, 0, rc.width(), rc.height());
            ctx.closePath();
            ctx._fill(rc);
            ctx._stroke(rc);
        });

        return rect;
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
export function updateShapeFromField(shape: Konva.Shape, field: LabelFieldBag): boolean {
    if (field.fieldType === LabelFieldType.Text && shape instanceof Konva.Text) {
        updateTextShapeFromField(shape, field);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Rectangle && shape instanceof Konva.Rect) {
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

    const fontSize =  toNumberOrNull(config.fontSize) ?? 12;
    const alignment = toNumber(config.horizontalAlignment) as HorizontalTextAlignment;

    shape.fontFamily(asBoolean(config.isCondensed) ? "Roboto Condensed" : "Roboto");
    shape.fontStyle(asBoolean(config.isBold) ? "bold" : "normal");
    shape.fill(asBoolean(config.isColorInverted) ? "white" : "black");
    shape.text(config.placeholderText ?? "");
    shape.fontSize(fontSize * window.devicePixelRatio);

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
function updateRectShapeFromField(shape: Konva.Rect, field: LabelFieldBag): void {
    if (!field.configurationValues) {
        return;
    }

    const config = field.configurationValues as StringRecord<RectangleFieldConfigurationBag>;

    const roundingIndex = toNumber(config.cornerRadius);

    if (asBoolean(config.isFilled)) {
        shape.fillEnabled(true);
        shape.strokeEnabled(false);
        shape.fill(asBoolean(config.isBlack) ? "black" : "white");
        shape.strokeWidth(0);
    }
    else {
        shape.fillEnabled(false);
        shape.strokeEnabled(true);
        shape.strokeScaleEnabled(false);
        shape.strokeWidth(toNumber(config.borderThickness));
        shape.stroke(asBoolean(config.isBlack) ? "black" : "white");
    }

    shape.cornerRadius(roundingIndex / 8 * getPixelForOffset(Math.min(field.width, field.height)) / 2);
}

// #endregion
