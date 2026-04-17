import { FormattingToolbarExtension, ShowSelectionExtension } from "@blocknote/core/extensions";
import { computePosition } from "@floating-ui/dom";
import type { RockBlockNoteEditor } from "./schema";

type ToolbarElement = {
    element: HTMLElement;
    item: IFormattingToolbarItem;
};

export class IconToolbarButton {
    private readonly icon: string;

    constructor(icon: string) {
        this.icon = icon;
    }

    public render(_toolbar: FormattingToolbar): HTMLElement {
        const button = document.createElement('button');
        button.type = 'button';
        button.classList.add('bn-button');
        button.innerHTML = `<i class="${this.icon}"></i>`;
        return button;
    }

    public update(_toolbar: FormattingToolbar, _element: HTMLElement): void {
        // Update logic for the button
    }
}

export class StyleToolbarButton extends IconToolbarButton {
    private readonly style: "bold" | "italic" | "underline" | "strike";

    constructor(icon: string, style: "bold" | "italic" | "underline" | "strike") {
        super(icon);
        this.style = style;
    }

    public override render(toolbar: FormattingToolbar): HTMLElement {
        const button = super.render(toolbar);

        button.addEventListener("click", () => {
            toolbar.editor.toggleStyles({ [this.style]: true });
            toolbar.editor.focus();
        });

        return button;
    }

    public override update(toolbar: FormattingToolbar, element: HTMLElement): void {
        const isActive = toolbar.editor.getActiveStyles()[this.style] || false;

        if (isActive) {
            element.classList.add('bn-active');
        } else {
            element.classList.remove('bn-active');
        }
    }
}

export interface IFormattingToolbarItem {
    render: (toolbar: FormattingToolbar) => HTMLElement;
    update?: (toolbar: FormattingToolbar, element: HTMLElement) => void;
}

export class FormattingToolbar {
    public readonly editor: RockBlockNoteEditor;
    private readonly toolbar: HTMLElement;
    private readonly showSelectionExtension: ReturnType<ReturnType<typeof ShowSelectionExtension>>;
    private readonly formattingToolbarExtension: ReturnType<ReturnType<typeof FormattingToolbarExtension>>;
    private readonly items: ToolbarElement[] = [];

    constructor(editor: RockBlockNoteEditor, items: IFormattingToolbarItem[]) {
        this.editor = editor;
        this.toolbar = this.createToolbar(items);
        this.showSelectionExtension = editor.getExtension(ShowSelectionExtension)!;
        this.formattingToolbarExtension = editor.getExtension(FormattingToolbarExtension)!;

        editor.onChange(this.onEditorChange.bind(this));
        editor.onSelectionChange(this.onEditorChange.bind(this));
        this.formattingToolbarExtension.store.subscribe(this.onStateChange.bind(this));
    }

    private createToolbar(items: IFormattingToolbarItem[]): HTMLElement {
        const toolbar = document.createElement('div');
        toolbar.classList.add("bn-toolbar", "bn-formatting-toolbar");

        for (const item of items) {
            const button = item.render(this);

            toolbar.appendChild(button);

            this.items.push({ element: button, item });
        }

        return toolbar;
    }

    private onStateChange(state: { prevVal: boolean; currentVal: boolean }): void {
        const box = this.editor.getSelectionBoundingBox();
        const isFileOrImage = this.editor.getSelection()?.blocks.some(b => b.type === "file" || b.type === "image");

        if (!state.currentVal || !box || isFileOrImage) {
            this.showSelection(false);
            this.toolbar.remove();
            return;
        }

        this.toolbar.style.display = "none";
        document.body.appendChild(this.toolbar);

        computePosition({ getBoundingClientRect: () => box }, this.toolbar)
            .then(({ x, y }) => {
                this.toolbar.style.left = `${x}px`;
                this.toolbar.style.top = `${y}px`;
                this.toolbar.style.display = "";
            });
    }

    private onEditorChange(): void {
        for (const { element, item } of this.items) {
            item.update?.(this, element);
        }
    }

    public showSelection(shouldShow: boolean): void {
        this.showSelectionExtension.showSelection(shouldShow, "bn-formatting-toolbar");
    }
}
