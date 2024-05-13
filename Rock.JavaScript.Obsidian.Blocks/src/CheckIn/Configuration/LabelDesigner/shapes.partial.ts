import Konva from "@Obsidian/Libs/konva";

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
