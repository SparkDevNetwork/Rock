import { FormattingToolbarExtension, ShowSelectionExtension } from "@blocknote/core/extensions";
import { computePosition, flip, offset, shift } from "@floating-ui/dom";
import { editorHasBlockWithType } from "@blocknote/core";
import type { RockBlock, RockBlockNoteEditor } from "./schema";

type ToolbarElement = {
    element: HTMLElement;
    item: IFormattingToolbarItem;
};

export type BlockTypeSelectItem = {
    name: string;
    type: string;
    props?: Record<string, boolean | number | string>;
    icon: string;
};

const blockTypeSelectItems: BlockTypeSelectItem[] = [
    {
        name: "Paragraph",
        type: "paragraph",
        icon: "ti ti-letter-t",
    },
    {
        name: "Heading 1",
        type: "heading",
        props: { level: 1, isToggleable: false },
        icon: "ti ti-h-1",
    },
    {
        name: "Heading 2",
        type: "heading",
        props: { level: 2, isToggleable: false },
        icon: "ti ti-h-2",
    },
    {
        name: "Heading 3",
        type: "heading",
        props: { level: 3, isToggleable: false },
        icon: "ti ti-h-3",
    },
    {
        name: "Heading 4",
        type: "heading",
        props: { level: 4, isToggleable: false },
        icon: "ti ti-h-4",
    },
    {
        name: "Heading 5",
        type: "heading",
        props: { level: 5, isToggleable: false },
        icon: "ti ti-h-5",
    },
    {
        name: "Heading 6",
        type: "heading",
        props: { level: 6, isToggleable: false },
        icon: "ti ti-h-6",
    },
    {
        name: "Quote",
        type: "quote",
        icon: "ti ti-blockquote",
    },
    {
        name: "Bullet List",
        type: "bulletListItem",
        icon: "ti ti-list",
    },
    {
        name: "Numbered List",
        type: "numberedListItem",
        icon: "ti ti-list-numbers",
    },
    {
        name: "Check List",
        type: "checkListItem",
        icon: "ti ti-list-check",
    },
];

export class IconToolbarButton implements IFormattingToolbarItem {
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

export class BlockTypeToolbarItem implements IFormattingToolbarItem {
    private floatingMenu: HTMLElement;
    private menu: HTMLElement;

    constructor() {
        this.floatingMenu = document.createElement("div");
        this.floatingMenu.classList.add("bn-hover", "bn-hover-menu");

        this.menu = document.createElement("ul");
        this.menu.classList.add("bn-menu");
        this.floatingMenu.appendChild(this.menu);
    }

    public render(toolbar: FormattingToolbar): HTMLElement {
        const button = document.createElement("button");

        button.type = "button";
        button.classList.add("bn-button", "bn-block-type-button");
        button.innerHTML = `<span class="bn-block-type-icon"></span><span class="bn-block-type-name">Block Type</span><span class="bn-block-type-chevron"><i class="ti ti-chevron-down"></i></span>`;

        button.addEventListener("click", (ev) => {
            ev.stopPropagation();
            ev.preventDefault();

            if (this.floatingMenu.parentElement) {
                this.floatingMenu.remove();
                return;
            }

            const container = toolbar.editor.domElement?.closest(".bn-container");
            if (container) {
                this.floatingMenu.style.display = "";
                this.floatingMenu.style.opacity = "0";
                container.appendChild(this.floatingMenu);
            }

            computePosition(button, this.floatingMenu, {
                placement: "bottom",
                middleware: [
                    offset(4),
                    flip(),
                    shift(),
                ]
            })
                .then(({ x, y }) => {
                    this.floatingMenu.style.left = `${x}px`;
                    this.floatingMenu.style.top = `${y}px`;
                    this.floatingMenu.style.opacity = "";
                });
        });

        return button;
    }

    public update(toolbar: FormattingToolbar, element: HTMLElement): void {
        const items = this.getSelectBlockTypeItems(toolbar.editor);
        const selectedItem = items.find((item) => item.isSelected);

        // If no supported block types are selected, hide the picker.
        if (!selectedItem) {
            element.style.display = "none";
            this.hide();
            return;
        }

        this.resetMenu(toolbar);

        element.style.display = "";
        element.children[0].innerHTML = `<i class="${selectedItem.icon}"></i>`;
        element.children[1].textContent = selectedItem.name;
    }

    public hide(): void {
        this.floatingMenu.remove();
    }

    private resetMenu(toolbar: FormattingToolbar): void {
        this.menu.innerHTML = "";
        for (const item of this.getSelectBlockTypeItems(toolbar.editor)) {
            const menuItem = document.createElement("li");
            menuItem.classList.add("bn-block-type-menu-item");
            menuItem.innerHTML = `<span class="bn-block-type-menu-item-icon"><i class="${item.icon}"></i></span><span class="bn-block-type-menu-item-name">${item.name}</span><span class="bn-block-type-menu-item-state"></span>`;

            if (item.isSelected) {
                menuItem.children[2].innerHTML = `<i class="ti ti-check"></i>`;
            }
            else {
                menuItem.children[2].innerHTML = "";
            }

            menuItem.addEventListener("click", (ev) => {
                ev.stopPropagation();
                ev.preventDefault();
                item.onClick();
                this.floatingMenu.remove();
            });
            this.menu.appendChild(menuItem);
        }
    }

    private getFilteredBlockTypeItems(editor: RockBlockNoteEditor): BlockTypeSelectItem[] {
        return blockTypeSelectItems.filter(item => {
            const props = Object.fromEntries(
                Object.entries(item.props || {}).map(([propName, propValue]) => [
                    propName,
                    typeof propValue,
                ]),
            ) as Record<string, "string" | "number" | "boolean">

            return editorHasBlockWithType(editor, item.type, props);
        });
    }

    private getSelectBlockTypeItems(editor: RockBlockNoteEditor): { name: string, icon: string, isSelected: boolean, onClick: () => void }[] {
        const selectedBlocks = this.getSelectedBlocks(editor);
        const firstSelectedBlock = selectedBlocks[0];

        return this.getFilteredBlockTypeItems(editor).map((item) => {
            // If the type matches and all the defined props match, then we
            // consider this block type as selected.
            const typesMatch = item.type === firstSelectedBlock.type;
            const propsMatch = Object.entries(item.props || {}).filter(([propName, propValue]) => {
                return propValue !== (firstSelectedBlock.props as Record<string, unknown>)[propName];
            }).length === 0;

            return {
                name: item.name,
                icon: item.icon,
                onClick: () => {
                    editor.focus();
                    editor.transact(() => {
                        for (const block of selectedBlocks) {
                            editor.updateBlock(block, {
                                type: item.type as any,
                                props: item.props as any,
                            });
                        }
                    });
                },
                isSelected: typesMatch && propsMatch,
            };
        });
    }

    private getSelectedBlocks(editor: RockBlockNoteEditor): RockBlock[] {
        return editor.getSelection()?.blocks || [editor.getTextCursorPosition().block]
    }
}

export interface IFormattingToolbarItem {
    render: (toolbar: FormattingToolbar) => HTMLElement;
    update?: (toolbar: FormattingToolbar, element: HTMLElement) => void;
    hide?: (toolbar: FormattingToolbar, element: HTMLElement) => void;
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
        const ignoredBlockTypes = ["file", "image", "media"];
        const isIgnoredBlock = !this.editor.getSelection()
            || this.editor.getSelection()?.blocks.some(b => ignoredBlockTypes.includes(b.type))

        if (!state.currentVal || !box || isIgnoredBlock) {
            this.hide();

            return;
        }

        const container = this.editor.domElement?.closest(".bn-container");
        if (container) {
            this.toolbar.style.opacity = "0";
            container.appendChild(this.toolbar);
        }

        computePosition({ getBoundingClientRect: () => box }, this.toolbar, {
            placement: "top-start",
            middleware: [
                offset(4),
                flip(),
                shift(),
            ]
        })
            .then(({ x, y }) => {
                this.toolbar.style.left = `${x}px`;
                this.toolbar.style.top = `${y}px`;
                this.toolbar.style.opacity = "";
            });
    }

    private hide(): void {
        this.showSelection(false);
        this.toolbar.remove();

        for (const { element, item } of this.items) {
            item.hide?.(this, element);
        }
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
