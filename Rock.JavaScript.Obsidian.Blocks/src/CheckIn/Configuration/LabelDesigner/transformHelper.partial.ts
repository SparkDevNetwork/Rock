import Konva from "@Obsidian/Libs/konva";
import { Surface } from "./utils.partial";

/**
 * Stashed values used by the TransformHelper.
 */
type TransformValues = {
    x: number;
    y: number;
    width: number;
    height: number;
};

/**
 * Provides logic for the transformer used in resizing nodes on the canvas.
 * This clamps the resize operations so they stay within the bounds of the
 * label. It also enforces minimum size constraints.
 */
export class TransformHelper {
    public readonly stage: Konva.Stage;
    public readonly transformer: Konva.Transformer;

    private readonly surface: Surface;

    private original?: TransformValues;
    private last?: TransformValues;
    private lastAnchor: string | null = null;
    private node: Konva.Node | null = null;

    /**
     * Creates a new TransformHelper instance to handle transform logic for
     * the worksurface.
     *
     * @param stage The stage the transformer is attached to.
     * @param snapPixel The function that will handle pixel snapping.
     */
    constructor(stage: Konva.Stage, surface: Surface) {
        this.stage = stage;
        this.surface = surface;

        this.transformer = new Konva.Transformer({
            nodes: [],
            rotationSnaps: [0, 90, 180, 270],
            rotationSnapTolerance: 45,
            rotateAnchorOffset: 25,
            flipEnabled: false,
            keepRatio: false
        });

        this.transformer.on("transformstart", event => this.transformStart(event));
        this.transformer.on("transform", event => this.transform(event));
        this.transformer.on("transformend", () => this.transformEnd());
    }

    /**
     * Destroys the helper by freeing resources.
     */
    public destroy(): void {
        this.transformer.off("transformstart");
        this.transformer.off("transform");
        this.transformer.off("transformstart");
    }

    /**
     * Event handler for the transformstart event on the transformer.
     *
     * @param event The event.
     */
    private transformStart(event: Konva.KonvaEventObject<Event>): void {
        this.node = event.target;
        this.lastAnchor = null;

        this.original = {
            x: this.node.x(),
            y: this.node.y(),
            width: this.node.width(),
            height: this.node.height()
        };

        this.last = { ...this.original };
    }

    /**
     * Event handler for the transformend event on the transformer.
     */
    private transformEnd(): void {
        this.last = undefined;
        this.lastAnchor = null;
        this.node = null;
    }

    /**
     * Event handler for the transform event on the transformer.
     *
     * @param event The event.
     */
    private transform(event: Konva.KonvaEventObject<Event>): void {
        if (!this.node || !this.original || !this.last || event.target.id() !== this.node.id()) {
            return;
        }

        if (this.lastAnchor && this.transformer.getActiveAnchor() !== this.lastAnchor) {
            this.node.setAttrs({
                ...this.last,
                scaleX: 1,
                scaleY: 1
            });

            this.transformer.stopTransform();

            return;
        }

        this.lastAnchor = this.transformer.getActiveAnchor();

        // Determine which anchor we are working with.
        const leftAnchor = ["top-left", "middle-left", "bottom-left"].includes(this.lastAnchor ?? "");
        const topAnchor = ["top-left", "top-center", "top-right"].includes(this.lastAnchor ?? "");
        const rightAnchor = ["top-right", "middle-right", "bottom-right"].includes(this.lastAnchor ?? "");
        const bottomAnchor = ["bottom-left", "bottom-center", "bottom-right"].includes(this.lastAnchor ?? "");

        // Get the new values.
        let newX = this.node.x();
        let newY = this.node.y();
        let newWidth = this.node.width() * this.node.scaleX();
        let newHeight = this.node.height() * this.node.scaleY();

        // If this is a left side anchor then attempt to snap the x position,
        // otherwise if it is a right anchor then attempt to snap the width.
        if (leftAnchor) {
            const snap = this.surface.snapPixel(newX);

            newWidth += newX - snap;
            newX = snap;
        }
        else if (rightAnchor) {
            newWidth = this.surface.snapPixel(newX + newWidth) - newX;
        }

        // If this is a top side anchor then attempt to snap the y position,
        // otherwise if it is a bottom anchor then attempt to snap the height.
        if (topAnchor) {
            const snap = this.surface.snapPixel(newY);

            newHeight += newY - snap;
            newY = snap;
        }
        else if (bottomAnchor) {
            newHeight = this.surface.snapPixel(newY + newHeight) - newY;
        }

        // If this is a left anchor then clamp the x position to the left
        // edge of the label.
        if (leftAnchor && newX < 0) {
            newWidth += newX;
            newX = 0;
        }

        // If this is a top anchor then clamp the y position to the top edge
        // of the label.
        if (topAnchor && newY < 0) {
            newHeight += newY;
            newY = 0;
        }

        // If the new width is too small force it to the minimum size.
        // Otherwise, clamp the width to the right edge of the label.
        if (newWidth < 20) {
            if (leftAnchor) {
                newX = this.last.x;
                newWidth = 20;
            }
            else if (rightAnchor) {
                newWidth = 20;
            }
        }
        else if (newWidth + newX > this.stage.width()) {
            newWidth = this.stage.width() - newX;
        }

        // If the new height is too small force it to the minimum size.
        // Otherwise, clamp the height to the bottom edge of the label.
        if (newHeight < 20) {
            if (topAnchor) {
                newY = this.last.y;
                newHeight = 20;
            }
            else if (bottomAnchor) {
                newHeight = 20;
            }
        }
        else if (newHeight + this.node.y() > this.stage.height()) {
            newHeight = this.stage.height() - this.node.y();
        }

        // Set the new values.
        this.node.setAttrs({
            x: newX,
            y: newY,
            width: newWidth,
            height: newHeight,
            scaleX: 1,
            scaleY: 1
        });

        // Store the last values we set for later use.
        this.last = {
            x: newX,
            y: newY,
            width: newWidth,
            height: newHeight
        };
    }
}
