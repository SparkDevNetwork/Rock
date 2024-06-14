import Konva from "@Obsidian/Libs/konva";
import { IconItem } from "./types.partial";
import { areEqual, newGuid } from "@Obsidian/Utility/guid";
import { LabelFieldType } from "@Obsidian/Enums/CheckIn/Labels/labelFieldType";
import { BarcodeFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/barcodeFieldConfigurationBag";
import { EllipseFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/ellipseFieldConfigurationBag";
import { IconFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/iconFieldConfigurationBag";
import { LabelFieldBag } from "@Obsidian/ViewModels/CheckIn/Labels/labelFieldBag";
import { LineFieldConfigurationBag} from "@Obsidian/ViewModels/CheckIn/Labels/lineFieldConfigurationBag";
import { RectangleFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/rectangleFieldConfigurationBag";
import { TextFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/textFieldConfigurationBag";
import { HorizontalTextAlignment } from "@Obsidian/Enums/CheckIn/Labels/horizontalTextAlignment";
import { BarcodeFormat } from "@Obsidian/Enums/CheckIn/Labels/barcodeFormat";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { FilterExpressionType } from "@Obsidian/Enums/Reporting/filterExpressionType";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";

// #region Worksurface Utilities

export const pixelsPerInch = 72 * window.devicePixelRatio;

/**
 * Small helper class to handle various feature functions that need to be
 * passed around. These are dynamic so we can't just hard code them.
 */
export class Surface {
    /** The stage this surface is attached to. */
    public stage: Konva.Stage | undefined;

    /** The scaling factor that is applied to the surface. */
    public scale: number = 1;

    /* Grid size in 1/n fractions of an inch. */
    public gridSnapFraction: number = 4;

    /** `true` if snapping to the grid is enabled. */
    public snapToGrid: boolean = false;

    /**
     * Gets the pixel position on the canvas.
     *
     * @param offset The offset position in inches.
     *
     * @returns The pixel position as a floating point number.
     */
    public getPixelForOffset(offset: number): number {
        return offset * pixelsPerInch * this.scale;
    }

    /**
     * Gets the offset in inches of the pixel position.
     *
     * @param pixel The pixel position.
     *
     * @returns The offset in inches.
     */
    public getOffsetForPixel(pixel: number): number {
        return pixel / pixelsPerInch / this.scale;
    }

    /**
     * Snaps the pixel position to the nearest grid line if grid snapping is
     * enabled.
     *
     * @param pixel The original pixel position in the canvas.
     *
     * @returns The new pixel position after the snap.
     */
    public snapPixel(pixel: number): number {
        let offset = this.getOffsetForPixel(pixel);

        if (this.snapToGrid) {
            offset = 1.0 / this.gridSnapFraction * Math.round(this.gridSnapFraction * offset);
        }

        return this.getPixelForOffset(offset);
    }

    /**
     * Gets a position that ensures the object stays inside the bounds of the
     * stage. This ensures drag operations stay within the label and also handles
     * snapping.
     *
     * @param pos The x,y position to be bounded.
     * @param size The width and height of the object.
     * @param disableSnap If `true` then snapping will not be performed.
     *
     * @returns A new position that is guaranteed to be inside the bounds.
     */
    public getBoundedPosition(pos: Konva.Vector2d, size: Konva.Vector2d, disableSnap?: boolean): Konva.Vector2d {
        if (!this.stage) {
            return pos;
        }

        let newX = pos.x;
        let newY = pos.y;
        const width = size.x;
        const height = size.y;

        if (width >= 0) {
            // Clamp the x position to the left edge of the label.
            if (newX <= 0) {
                newX = 0;
            }

            newX = disableSnap ? newX : this.snapPixel(newX);

            // Clamp the x position so the shape doesn't extend past the right
            // edge of the label.
            if (newX >= this.stage.width() - width) {
                newX = this.stage.width() - width;
            }
        }
        else {
            // Clamp the x position to the left edge of the label.
            if (newX <= Math.abs(width)) {
                newX = Math.abs(width);
            }

            newX = disableSnap ? newX : this.snapPixel(newX + width) - width;

            // Clamp the x position so the shape doesn't extend past the right
            // edge of the label.
            if (newX >= this.stage.width()) {
                newX = this.stage.width();
            }
        }

        if (height >= 0) {
            // Clamp the y position to the top edge of the label.
            if (newY <= 0) {
                newY = 0;
            }

            newY = disableSnap ? newY : this.snapPixel(newY);

            // Clamp the y position so the shape doesn't extend past the bottom
            // edge of the label.
            if (newY >= this.stage.height() - height) {
                newY = this.stage.height() - height;
            }
        }
        else {
            // Clamp the y position to the top edge of the label.
            if (newY <= Math.abs(height)) {
                newY = Math.abs(height);
            }

            newY = disableSnap ? newY : this.snapPixel(newY + height) - height;

            // Clamp the y position so the shape doesn't extend past the bottom
            // edge of the label.
            if (newY >= this.stage.height()) {
                newY = this.stage.height();
            }
        }

        return {
            x: newX,
            y: newY
        };
    }

    /**
     * Gets a position that ensures the object stays inside the bounds of the
     * stage. This ensures drag operations stay within the label and also handles
     * snapping.
     *
     * @param node The node that will be used for sizing information.
     * @param snapPixel The function that will handle pixel snapping.
     *
     * @returns A function that can be passed to the `dragBoundFunc` property.
     */
    public getNodeBoundedPosition(node: Konva.Node, pos: Konva.Vector2d): Konva.Vector2d {
        let width = node.width();
        let height = node.height();

        if (this instanceof Konva.Line) {
            width = this.points()[2];
            height = this.points()[3];
        }

        return this.getBoundedPosition(pos, { x: width, y: height });
    }
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
        const config = field.configurationValues as RectangleFieldConfigurationBag;

        config.isBlack = "true";
        config.isFilled = "true";
        config.borderThickness = "0";
        config.cornerRadius = "0";
    }
    else if (fieldType === LabelFieldType.Line) {
        const config = field.configurationValues as LineFieldConfigurationBag;

        field.height = 0;
        config.isBlack = "true";
        config.thickness = "1";
    }
    else if (fieldType === LabelFieldType.Ellipse) {
        const config = field.configurationValues as EllipseFieldConfigurationBag;

        config.isBlack = "true";
        config.isFilled = "true";
        config.borderThickness = "0";
    }
    else if (fieldType === LabelFieldType.Icon) {
        const config = field.configurationValues as IconFieldConfigurationBag;

        field.width = 0.25;
        field.height = 0.25;
        config.icon = "birthday_cake";
    }
    else if (fieldType === LabelFieldType.Image) {
        // Intentionally blank.
    }
    else if (fieldType === LabelFieldType.Barcode) {
        const config = field.configurationValues as BarcodeFieldConfigurationBag;

        config.format = BarcodeFormat.QRCode.toString();
        config.isDynamic = "false";
    }

    return field;
}

// #endregion

export const IconImageMap: IconItem[] = [
    {
        text: "Birthday Cake",
        value: "birthday_cake",
        weight: 900,
        code: "\uF1Fd"
    },
    {
        text: "Star",
        value: "star",
        weight: 900,
        code: "\uF005"
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

/**
 * Converts a blob of binary data into a base64 encoded string.
 *
 * @param blob The blob data to be converted to base64.
 *
 * @returns A string that represents the data in base64 or `undefined` if it could not be converted.
 */
export function blobToBase64(blob: Blob): Promise<string | undefined> {
    return new Promise<string | undefined>(resolve => {
        const reader = new FileReader();
        reader.readAsDataURL(blob);
        reader.onloadend = () => {
            if (typeof reader.result !== "string") {
                return resolve(undefined);
            }

            const startIndex = reader.result.indexOf(",") + 1;
            const b64 = reader.result.substring(startIndex);
            resolve(b64);
        };
        reader.onerror = () => resolve(undefined);
    });
}

/**
 * Convert the image data to a black and white representation. The original
 * data string should be a base-64 encoded string of the raw data.
 *
 * @param original The base-64 encoded string that contains the original image data.
 * @param invertColor Invert the white and black colors of the image.
 *
 * @returns A new string that is a base-64 representation of the new image.
 */
export async function convertImageDataToBlackAndWhite(original: string, invertColor: boolean): Promise<string | undefined> {
    const image = document.createElement("img");
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");

    if (!ctx) {
        throw new Error("Could not process image.");
    }

    // Load the image and wait for it to complete.
    const imageLoaded = new Promise<void>((resolve, reject) => {
        image.addEventListener("load", () => resolve());
        image.addEventListener("error", () => reject("Unable to load image data."));
    });

    image.src = `data:image/png;base64,${original}`;
    await imageLoaded;

    // Configure the size of the canvas to match the image and then draw
    // the image onto the full canvas.
    canvas.width = image.naturalWidth;
    canvas.height = image.naturalHeight;
    ctx.drawImage(image, 0, 0);

    // Get the raw RGBA canvas data.
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);

    // Convert the image data to be black and transparent.
    for (let i = 0; i < imageData.data.length; i += 4) {
        const avgColor = (imageData.data[i] + imageData.data[i + 1] + imageData.data[i + 3]) / 3;

        imageData.data[i] = imageData.data[i + 1] = imageData.data[i + 2] = invertColor ? 255 : 0;

        // If the average color is not near white and also not
        // semi-transparent then switch it to full black.
        if (avgColor < 224 && imageData.data[i + 3] > 128) {
            imageData.data[i + 3] = 255;
        }
        else {
            imageData.data[i + 3] = 0;
        }
    }

    // Replace the contents of the canvas with the modified image data.
    ctx.putImageData(imageData, 0, 0, 0, 0, canvas.width, canvas.height);

    // Convert the canvas to a PNG image blob.
    const blob = await new Promise<Blob>((resolve, reject) => {
        canvas.toBlob(b => {
            if (b) {
                resolve(b);
            }
            else {
                reject(new Error("Unable to process image."));
            }
        }, "image/png");
    });

    return blobToBase64(blob);
}
