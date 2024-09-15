import Konva from "@Obsidian/Libs/konva";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { Surface } from "./utils.partial";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";
import { BarcodeFormat } from "@Obsidian/Enums/CheckIn/Labels/barcodeFormat";
import { HorizontalTextAlignment } from "@Obsidian/Enums/CheckIn/Labels/horizontalTextAlignment";
import { LabelFieldType } from "@Obsidian/Enums/CheckIn/Labels/labelFieldType";
import { TextFieldSubType } from "@Obsidian/Enums/CheckIn/Labels/textFieldSubType";
import { IconItemBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Configuration/LabelDesigner/iconItemBag";
import { AttendeePhotoFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/attendeePhotoFieldConfigurationBag";
import { BarcodeFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/barcodeFieldConfigurationBag";
import { EllipseFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/ellipseFieldConfigurationBag";
import { IconFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/iconFieldConfigurationBag";
import { ImageFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/imageFieldConfigurationBag";
import { LabelFieldBag } from "@Obsidian/ViewModels/CheckIn/Labels/labelFieldBag";
import { LineFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/lineFieldConfigurationBag";
import { RectangleFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/rectangleFieldConfigurationBag";
import { TextFieldConfigurationBag } from "@Obsidian/ViewModels/CheckIn/Labels/textFieldConfigurationBag";
import { convertImageDataToBlackAndWhite } from "./imageProcessing.partial";

// #region Classes

/**
 * Custom rectangle shape so we can properly adjust border insets in outline
 * mode.
 */
export class Rectangle extends Konva.Group {
    public innerShape: Konva.Rect;

    public constructor() {
        super();

        this.innerShape = new Konva.Rect();

        this.innerShape.hitFunc((ctx, rc) => {
            // By default a rectangle in outline mode will only hit test on the
            // outline and not the inner (empty) content. So we override that
            // to ensure the entire outline and fill area match a hit test.
            ctx.beginPath();
            ctx.rect(0, 0, rc.width(), rc.height());
            ctx.closePath();
            ctx._fill(rc);
            ctx._stroke(rc);
        });

        this.add(this.innerShape);

        this.on("widthChange", () => {
            this.innerShape.width(this.width() - this.innerShape.strokeWidth());
        });

        this.on("heightChange", () => {
            this.innerShape.height(this.height() - this.innerShape.strokeWidth());
        });
    }
}

/**
 * Custom ellipse shape so we can properly adjust border insets in outline
 * mode.
 */
export class Ellipse extends Konva.Group {
    public innerShape: Konva.Ellipse;

    public constructor() {
        super();

        this.innerShape = new Konva.Ellipse();

        // By default an ellipse draws itself with x,y at center. Override
        // that to draw the ellipse to fit inside x,y - width,height.
        this.innerShape.sceneFunc((ctx, el) => {
            const strokeWidth = el.strokeEnabled() ? el.strokeWidth() : 0;

            ctx.beginPath();
            ctx.ellipse(el.x() - el.radiusX() - (strokeWidth / 2),
                el.y() - el.radiusY() - (strokeWidth / 2),
                el.radiusX(),
                el.radiusY(),
                el.rotation(),
                0,
                Math.PI * 2,
                false);
            ctx.closePath();
            ctx.fillStrokeShape(el);
        });

        // By default an ellipse in outline mode will only hit test on the
        // outline and not the inner (empty) content. So we override that
        // to ensure the entire outline and fill area match a hit test.
        this.innerShape.hitFunc((ctx, el) => {
            const strokeWidth = el.strokeEnabled() ? el.strokeWidth() : 0;

            ctx.beginPath();
            ctx.ellipse(el.x() - el.radiusX() - (strokeWidth / 2),
                el.y() - el.radiusY() - (strokeWidth / 2),
                el.radiusX(),
                el.radiusY(),
                el.rotation(),
                0,
                Math.PI * 2,
                false);
            ctx.closePath();
            ctx._fill(el);
            ctx._stroke(el);
        });

        this.add(this.innerShape);

        this.on("widthChange heightChange", () => this.resize());
        this.innerShape.on("strokeWidthChange strokeEnabledChange", () => this.resize());
    }

    private resize(): void {
        this.innerShape.x((this.width() / 2));
        this.innerShape.y(this.height() / 2);
        this.innerShape.width(this.width() - this.innerShape.strokeWidth());
        this.innerShape.height(this.height() - this.innerShape.strokeWidth());
    }
}

/**
 * Class to handle transforming a line by showing two anchors at the two line
 * ends and allowing for resize.
 */
export class LineTransformer extends Konva.Group {
    private anchor1: Konva.Circle;
    private anchor2: Konva.Circle;
    private line?: Konva.Line;
    private readonly updateAnchorsCallback = (): void => this.updateAnchors();
    private readonly transformEndCallback: (line: Konva.Line) => void;

    public constructor(surface: Surface, transformEndCallback: (line: Konva.Line) => void) {
        super();

        this.transformEndCallback = transformEndCallback;

        this.anchor1 = new Konva.Circle({
            radius: 5,
            fill: "white",
            stroke: "#60c5ff",
            strokeWidth: 1,
            draggable: true,
            visible: false,
            dragBoundFunc: pos => surface.getBoundedPosition(pos, { x: 0, y: 0 })
        });

        this.anchor2 = new Konva.Circle({
            radius: 5,
            fill: "white",
            stroke: "#60c5ff",
            strokeWidth: 1,
            draggable: true,
            visible: false,
            dragBoundFunc: pos => surface.getBoundedPosition(pos, { x: 0, y: 0 })
        });

        this.anchor1.on("mouseenter", () => this.setCursor());
        this.anchor1.on("mouseout", () => this.resetCursor());
        this.anchor1.on("dragmove", () => this.updateLine());
        this.anchor1.on("dragend", () => {
            if (this.line) {
                this.transformEndCallback(this.line);
            }
        });

        this.anchor2.on("mouseenter", () => this.setCursor());
        this.anchor2.on("mouseout", () => this.resetCursor());
        this.anchor2.on("dragmove", () => this.updateLine());
        this.anchor2.on("dragend", () => {
            if (this.line) {
                this.transformEndCallback(this.line);
            }
        });

        this.add(this.anchor1, this.anchor2);
    }

    /**
     * Updates the the line shape to match the anchors.
     */
    private updateLine(): void {
        if (!this.line) {
            return;
        }

        this.line.off("xChange yChange pointsChange", this.updateAnchorsCallback);

        this.line.x(this.anchor1.x());
        this.line.y(this.anchor1.y());

        this.line.points([
            0,
            0,
            this.anchor2.x() - this.anchor1.x(),
            this.anchor2.y() - this.anchor1.y()
        ]);

        this.line.on("xChange yChange pointsChange", this.updateAnchorsCallback);
    }

    /**
     * Updates the anchors to match the line shape.
     */
    private updateAnchors(): void {
        if (!this.line) {
            return;
        }

        this.anchor1.x(this.line.x());
        this.anchor1.y(this.line.y());
        this.anchor2.x(this.line.x() + this.line.points()[2]);
        this.anchor2.y(this.line.y() + this.line.points()[3]);
    }

    /**
     * Configures the mouse cursor to show the move icon.
     */
    private setCursor(): void {
        const stage = this.anchor1.getStage();

        if (stage) {
            stage.content.style.cursor = "move";
        }
    }

    /**
     * Resets the mouse cursor to the default value.
     */
    private resetCursor(): void {
        const stage = this.anchor1.getStage();

        if (stage) {
            stage.content.style.cursor = "";
        }
    }

    /**
     * Selects a specific line node.
     *
     * @param line The line node that is selected or undefined.
     */
    public node(line: Konva.Line | undefined): void {
        if (line) {
            this.anchor1.visible(true);
            this.anchor2.visible(true);
            this.anchor1.x(line.x());
            this.anchor1.y(line.y());
            this.anchor2.x(line.x() + line.points()[2]);
            this.anchor2.y(line.y() + line.points()[3]);
            line.on("xChange yChange pointsChange", this.updateAnchorsCallback);
        }
        else {
            this.anchor1.visible(false);
            this.anchor2.visible(false);
            this.line?.off("xChange yChange pointsChange", this.updateAnchorsCallback);
        }

        this.line = line;
    }

    /**
     * Gets the currently selected node in this transformer.
     *
     * @returns The currently selected node or `undefined`.
     */
    public getSelectedNode(): Konva.Line | undefined {
        return this.line;
    }
}

// #region Functions

/**
 * Sets the crop values for the image so that the image content is centered
 * and either fits inside the bounds or covers all available space. The crop is
 * set so that the aspect ratio is maintained.
 *
 * @param shape The image shape to be cropped.
 * @param cover `true` if the crop should cover the entire shape.
 */
function applyCenterCrop(shape: Konva.Image, cover?: boolean): void {
    const currentImage = shape.image() as HTMLImageElement;
    const aspectRatio = shape.width() / shape.height();
    const imageRatio = currentImage.naturalWidth / currentImage.naturalHeight;
    let cropWidth: number;
    let cropHeight: number;

    if (cover) {
        if (aspectRatio >= imageRatio) {
            cropWidth = currentImage.naturalWidth;
            cropHeight = currentImage.naturalWidth / aspectRatio;
        }
        else {
            cropWidth = currentImage.naturalHeight * aspectRatio;
            cropHeight = currentImage.naturalHeight;
        }
    }
    else {
        if (aspectRatio < imageRatio) {
            cropWidth = currentImage.naturalWidth;
            cropHeight = currentImage.naturalWidth / aspectRatio;
        }
        else {
            cropWidth = currentImage.naturalHeight * aspectRatio;
            cropHeight = currentImage.naturalHeight;
        }
    }

    const cropX = (currentImage.naturalWidth - cropWidth) / 2;
    const cropY = (currentImage.naturalHeight - cropHeight) / 2;

    shape.setAttrs({
        cropX,
        cropY,
        cropWidth,
        cropHeight
    });
}

/**
 * Creates a new shape that will display a font awesome icon.
 *
 * @returns A new Konva Text shape.
 */
function createFontAwesomeShape(): Konva.Text {
    const shape = new Konva.Text();

    shape.align("center");
    shape.verticalAlign("middle");
    shape.fontFamily("FontAwesome");
    shape.fontSize(12);
    shape.fontStyle("900");

    shape.on("widthChange heightChange textChange", () => {
        // Some fonts are wider than they are tall (such as star). So this is
        // a quick hack to make them fit.
        shape.fontSize(Math.min(shape.width() * 0.85, shape.height()));
    });

    return shape;
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
    else if (fieldType === LabelFieldType.Line) {
        return new Konva.Line();
    }
    else if (fieldType === LabelFieldType.Ellipse) {
        return new Ellipse();
    }
    else if (fieldType === LabelFieldType.Icon) {
        return createFontAwesomeShape();
    }
    else if (fieldType === LabelFieldType.Image) {
        const img = new window.Image(1, 1);

        const image = new Konva.Image({
            image: img
        });

        image.on("widthChange heightChange", () => {
            applyCenterCrop(image);
        });

        img.addEventListener("load", () => {
            applyCenterCrop(image);

            image._requestDraw();
        });

        return image;
    }
    else if (fieldType === LabelFieldType.AttendeePhoto) {
        const shape = createFontAwesomeShape();

        shape.fontStyle("400");
        shape.text("\uF2C1");

        return shape;
    }
    else if (fieldType === LabelFieldType.Barcode) {
        return createFontAwesomeShape();
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
 * @param surface The information about the design surface.
 * @param icons The list of icons available in this block.
 *
 * @returns `true` if the shape was updated or `false` if it could not be updated.
 */
export function updateShapeFromField(shape: Konva.Shape | Konva.Group, field: LabelFieldBag, surface: Surface, icons: IconItemBag[]): boolean {
    if (field.fieldType === LabelFieldType.Text && shape instanceof Konva.Text) {
        updateTextShapeFromField(shape, field, surface);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Line && shape instanceof Konva.Line) {
        updateLineShapeFromField(shape, field, surface);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Ellipse && shape instanceof Ellipse) {
        updateEllipseShapeFromField(shape, field, surface);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Rectangle && shape instanceof Rectangle) {
        updateRectShapeFromField(shape, field, surface);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Icon && shape instanceof Konva.Text) {
        updateIconShapeFromField(shape, field, surface, icons);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Image && shape instanceof Konva.Image) {
        updateImageShapeFromField(shape, field, surface);
        return true;
    }
    else if (field.fieldType === LabelFieldType.AttendeePhoto && shape instanceof Konva.Text) {
        updateAttendeePhotoShapeFromField(shape, field, surface);
        return true;
    }
    else if (field.fieldType === LabelFieldType.Barcode && shape instanceof Konva.Text) {
        updateBarcodeShapeFromField(shape, field, surface);
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
function updateTextShapeFromField(shape: Konva.Text, field: LabelFieldBag, surface: Surface): void {
    const config = (field.configurationValues ?? {}) as TextFieldConfigurationBag;

    const fontSize = toNumberOrNull(config.fontSize) ?? 12;
    const alignment = toNumber(config.horizontalAlignment) as HorizontalTextAlignment;

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.width(surface.getPixelForOffset(field.width));
    shape.height(surface.getPixelForOffset(field.height));

    // Update configured values.
    shape.fontFamily(asBoolean(config.isCondensed) ? "Roboto Condensed" : "Roboto");
    shape.fontStyle(asBoolean(config.isBold) ? "bold" : "normal");
    shape.globalCompositeOperation(asBoolean(config.isColorInverted) ? "xor" : "source-over");
    shape.fontSize(fontSize * window.devicePixelRatio * surface.scale);

    if (field.fieldSubType === TextFieldSubType.Custom) {
        if (asBoolean(config.isDynamicText)) {
            shape.text(config.placeholderText ?? "");
        }
        else {
            shape.text(config.staticText ?? "");
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
function updateRectShapeFromField(shape: Rectangle, field: LabelFieldBag, surface: Surface): void {
    const config = (field.configurationValues ?? {}) as RectangleFieldConfigurationBag;
    const roundingIndex = toNumber(config.cornerRadius);

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.width(surface.getPixelForOffset(field.width));
    shape.height(surface.getPixelForOffset(field.height));

    // Update configured values.
    if (asBoolean(config.isFilled)) {
        shape.innerShape.fillEnabled(true);
        shape.innerShape.strokeEnabled(false);
        shape.innerShape.strokeWidth(0);
        shape.innerShape.fill(asBoolean(config.isBlack) ? "black" : "white");

        // Reset rectangle to fill entire height and width.
        shape.innerShape.setAttrs({
            x: 0,
            y: 0,
            width: shape.width(),
            height: shape.height()
        });
    }
    else {
        shape.innerShape.fillEnabled(false);
        shape.innerShape.strokeEnabled(true);
        shape.innerShape.strokeWidth(Math.max(toNumber(config.borderThickness), 1));
        shape.innerShape.stroke(asBoolean(config.isBlack) ? "black" : "white");

        // The HTML Context always draws borders on center, so we need to adjust
        // the rectangle to simulate an inner border.
        shape.innerShape.setAttrs({
            x: shape.innerShape.strokeWidth() / 2,
            y: shape.innerShape.strokeWidth() / 2,
            width: shape.width() - shape.innerShape.strokeWidth(),
            height: shape.height() - shape.innerShape.strokeWidth()
        });
    }

    shape.innerShape.cornerRadius(roundingIndex / 8 * surface.getPixelForOffset(Math.min(field.width, field.height)) / 2);
}

/**
 * Updates the line shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateLineShapeFromField(shape: Konva.Line, field: LabelFieldBag, surface: Surface): void {
    const config = (field.configurationValues ?? {}) as LineFieldConfigurationBag;

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.points([0, 0, surface.getPixelForOffset(field.width), surface.getPixelForOffset(field.height)]);

    // Update configured values.
    shape.strokeWidth(Math.max(toNumber(config.thickness), 1));
    shape.hitStrokeWidth(Math.max(toNumber(config.thickness), 4));
    shape.stroke(asBoolean(config.isBlack) ? "black" : "white");
}

/**
 * Updates the ellipse shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateEllipseShapeFromField(shape: Ellipse, field: LabelFieldBag, surface: Surface): void {
    const config = (field.configurationValues ?? {}) as EllipseFieldConfigurationBag;

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.width(surface.getPixelForOffset(field.width));
    shape.height(surface.getPixelForOffset(field.height));

    // Update configured values.
    if (asBoolean(config.isFilled)) {
        shape.innerShape.fillEnabled(true);
        shape.innerShape.strokeEnabled(false);
        shape.innerShape.strokeWidth(0);
        shape.innerShape.fill(asBoolean(config.isBlack) ? "black" : "white");
    }
    else {
        shape.innerShape.fillEnabled(false);
        shape.innerShape.strokeEnabled(true);
        shape.innerShape.strokeWidth(Math.max(toNumber(config.borderThickness), 1));
        shape.innerShape.stroke(asBoolean(config.isBlack) ? "black" : "white");
    }
}

/**
 * Updates the icon shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateIconShapeFromField(shape: Konva.Text, field: LabelFieldBag, surface: Surface, icons: IconItemBag[]): void {
    const config = (field.configurationValues ?? {}) as IconFieldConfigurationBag;

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.width(surface.getPixelForOffset(field.width));
    shape.height(surface.getPixelForOffset(field.height));

    const icon = icons.find(i => i.value === config.icon);

    // Update configured values.
    if (icon) {
        shape.fontStyle(`${icon.weight}`);
        shape.text(icon.code ?? "\uF128");
    }
    else {
        shape.fontStyle("900");
        shape.text("\uF128");
    }

    shape.globalCompositeOperation(asBoolean(config.isColorInverted) ? "xor" : "source-over");
}

/**
 * Updates the image shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateImageShapeFromField(shape: Konva.Image, field: LabelFieldBag, surface: Surface): void {
    const config = (field.configurationValues ?? {}) as ImageFieldConfigurationBag;

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.width(surface.getPixelForOffset(field.width));
    shape.height(surface.getPixelForOffset(field.height));

    const currentImage = shape.image() as HTMLImageElement;
    const noImageData = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIW2NoaGj4DwAFhAKAEgUrPQAAAABJRU5ErkJggg==";

    if (!config.imageData) {
        if (currentImage.src !== noImageData) {
            currentImage.src = noImageData;
        }

        return;
    }

    const imageData = config.imageData;
    const brightness = toNumberOrNull(config.brightness) ?? 1;
    const key = `${brightness}:${imageData}`;

    // Update configured values.
    if (currentImage.dataset["originalSource"] !== key) {
        currentImage.dataset["originalSource"] = key;

        convertImageDataToBlackAndWhite(imageData, brightness).then(finalSrc => {
            // Only update if it hasn't been changed by another pass yet.
            if (currentImage.dataset["originalSource"] === key) {
                if (finalSrc) {
                    currentImage.src = `data:image/png;base64,${finalSrc}`;
                }
                else {
                    currentImage.src = noImageData;
                }
            }
        }).catch(err => {
            console.error(err);
        });
    }

    shape.globalCompositeOperation(asBoolean(config.isColorInverted) ? "xor" : "source-over");
}

/**
 * Updates the attendee photo shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateAttendeePhotoShapeFromField(shape: Konva.Text, field: LabelFieldBag, surface: Surface): void {
    const config = (field.configurationValues ?? {}) as AttendeePhotoFieldConfigurationBag;

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.width(surface.getPixelForOffset(field.width));
    shape.height(surface.getPixelForOffset(field.height));

    // Update configured values.
    shape.globalCompositeOperation(asBoolean(config.isColorInverted) ? "xor" : "source-over");
}

/**
 * Updates the barcode shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateBarcodeShapeFromField(shape: Konva.Text, field: LabelFieldBag, surface: Surface): void {
    const config = (field.configurationValues ?? {}) as BarcodeFieldConfigurationBag;

    // Update the position of the shape.
    shape.x(surface.getPixelForOffset(field.left));
    shape.y(surface.getPixelForOffset(field.top));
    shape.width(surface.getPixelForOffset(field.width));
    shape.height(surface.getPixelForOffset(field.height));

    // Update configured values.
    if (config.format === BarcodeFormat.Code128.toString()) {
        shape.text("\uF02A");
    }
    else {
        shape.text("\uF029");
    }
}

// #endregion
