import Konva from "@Obsidian/Libs/konva";
import { HorizontalTextAlignment, LabelFieldBag, LabelFieldType, LabelTextFieldSubType, LineFieldConfigurationBag, RectangleFieldConfigurationBag, StringRecord, TextFieldConfigurationBag } from "./types.partial";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { getPixelForOffset } from "./utils.partial";
import { asBoolean } from "@Obsidian/Utility/booleanUtils";

// #region Classes

/**
 * Custom rectangle shape so we can properly adjust border insets in outline
 * mode.
 */
export class Rectangle extends Konva.Group {
    public rect: Konva.Rect;

    public constructor() {
        super();

        this.rect = new Konva.Rect();

        this.rect.hitFunc((ctx, rc) => {
            // By default a rectangle in outline mode will only hit test on the
            // outline and not the inner (empty) content. So we override that
            // to ensure the entire outline and fill area match a hit test.
            ctx.beginPath();
            ctx.rect(0, 0, rc.width(), rc.height());
            ctx.closePath();
            ctx._fill(rc);
            ctx._stroke(rc);
        });

        this.add(this.rect);

        this.on("widthChange", () => {
            this.rect.width(this.width() - this.rect.strokeWidth());
        });

        this.on("heightChange", () => {
            this.rect.height(this.height() - this.rect.strokeWidth());
        });
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

    public constructor(positionBoundFunc: (pos: Konva.Vector2d, size: Konva.Vector2d) => Konva.Vector2d, transformEndCallback: (line: Konva.Line) => void) {
        super();

        this.transformEndCallback = transformEndCallback;

        this.anchor1 = new Konva.Circle({
            radius: 5,
            fill: "white",
            stroke: "#60c5ff",
            strokeWidth: 1,
            draggable: true,
            visible: false,
            dragBoundFunc: pos => positionBoundFunc(pos, { x: 0, y: 0 })
        });

        this.anchor2 = new Konva.Circle({
            radius: 5,
            fill: "white",
            stroke: "#60c5ff",
            strokeWidth: 1,
            draggable: true,
            visible: false,
            dragBoundFunc: pos => positionBoundFunc(pos, { x: 0, y: 0 })
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
}

// #region Functions

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
    else if (field.fieldType === LabelFieldType.Line && shape instanceof Konva.Line) {
        updateLineShapeFromField(shape, field);
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
    const config = field.configurationValues ?? {} as StringRecord<TextFieldConfigurationBag>;

    const fontSize = toNumberOrNull(config.fontSize) ?? 12;
    const alignment = toNumber(config.horizontalAlignment) as HorizontalTextAlignment;

    // Update the position of the shape.
    shape.x(getPixelForOffset(field.left));
    shape.y(getPixelForOffset(field.top));
    shape.width(getPixelForOffset(field.width));
    shape.height(getPixelForOffset(field.height));

    // Update configured values.
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
    const config = field.configurationValues ?? {} as StringRecord<RectangleFieldConfigurationBag>;
    const roundingIndex = toNumber(config.cornerRadius);

    // Update the position of the shape.
    shape.x(getPixelForOffset(field.left));
    shape.y(getPixelForOffset(field.top));
    shape.width(getPixelForOffset(field.width));
    shape.height(getPixelForOffset(field.height));

    // Update configured values.
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

/**
 * Updates the line shape with data from the field.
 *
 * @param shape The shape to be updated.
 * @param field The field to use as the source of truth.
 */
function updateLineShapeFromField(shape: Konva.Line, field: LabelFieldBag): void {
    const config = field.configurationValues ?? {} as StringRecord<LineFieldConfigurationBag>;

    // Update the position of the shape.
    shape.x(getPixelForOffset(field.left));
    shape.y(getPixelForOffset(field.top));
    shape.points([0, 0, getPixelForOffset(field.width), getPixelForOffset(field.height)]);

    // Update configured values.
    shape.strokeWidth(Math.max(toNumber(config.thickness), 1));
    shape.hitStrokeWidth(Math.max(toNumber(config.thickness), 4));
    shape.stroke(asBoolean(config.isBlack) ? "black" : "white");
}

// #endregion
