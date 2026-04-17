import { getDefaultSlashMenuItems, SuggestionMenu as SuggestionMenuExension, type SuggestionMenuState, type DefaultSuggestionItem, filterSuggestionItems } from "@blocknote/core/extensions";
import { Hover } from "./functions";
import type { RockBlockNoteEditor } from "./schema";

const iconTable: Record<string, string> = {
    "heading": "ti ti-h-1",
    "heading_2": "ti ti-h-2",
    "heading_3": "ti ti-h-3",
    "heading_4": "ti ti-h-4",
    "heading_5": "ti ti-h-5",
    "heading_6": "ti ti-h-6",
    "quote": "ti ti-blockquote",
    "numbered_list": "ti ti-list-numbers",
    "bullet_list": "ti ti-list",
    "paragraph": "ti ti-letter-t",
    "code_block": "ti ti-code",
    "table": "ti ti-table",
    "image": "ti ti-photo",
    "video": "ti ti-video",
    "file": "ti ti-file"
};

const supportedBlockKeys = [
    "heading",
    "heading_2",
    "heading_3",
    "heading_4",
    "heading_5",
    "heading_6",
    "quote",
    "numbered_list",
    "bullet_list",
    "paragraph",
    "code_block",
    "table",
    "image",
    "video",
    "file",
]

export class SuggestionMenu {
    private readonly editor: RockBlockNoteEditor;

    private readonly container: HTMLElement;

    private readonly blockSuggestionMenu: ReturnType<ReturnType<typeof SuggestionMenuExension>>;

    private hover: Hover | null = null;
    private menu: HTMLElement | null = null;
    private readonly defaultMenuItems: DefaultSuggestionItem[];
    private filteredMenuItems: DefaultSuggestionItem[] = [];
    private selectedIndex: number = 0;

    constructor(editor: RockBlockNoteEditor, container: HTMLElement) {
        this.editor = editor;
        this.container = container;
        this.defaultMenuItems = getDefaultSlashMenuItems(editor)
            .filter(item => supportedBlockKeys.includes(item.key));

        this.blockSuggestionMenu = this.editor.getExtension(SuggestionMenuExension)!;

        this.blockSuggestionMenu.addSuggestionMenu({
            triggerCharacter: "/",
        });
        this.blockSuggestionMenu.store.subscribe(this.onStateChanged);
    }

    private onStateChanged = (state: { currentVal: SuggestionMenuState | undefined }) => {
        const current = state.currentVal;

        if (!current) {
            return;
        }

        if (!current.show) {
            this.hover?.hide();
            this.hover?.dispose();
            this.hover = null;
            this.editor.domElement?.removeEventListener("keydown", this.onKeyboardEvent, true);
            return;
        }

        if (!this.menu) {
            this.menu = this.createSuggestionMenu();
        }

        if (!this.hover) {
            this.selectedIndex = 0;
            this.hover = new Hover(this.menu, this.container, {
                menu: true,
                autoDismiss: true,
            });

            this.editor.domElement?.addEventListener("keydown", this.onKeyboardEvent, true);
        }

        const filteredItems = filterSuggestionItems(this.defaultMenuItems, current.query);
        this.updateMenu(this.menu, filteredItems);

        this.hover.show(current.referencePos, "bottom-start");
    }

    private createSuggestionMenu(): HTMLElement {
        this.menu = document.createElement("ul");
        this.menu.classList.add("bn-menu", "bn-suggestion-menu");

        for (const item of this.defaultMenuItems) {
            const menuItem = this.createSuggestionMenuItem(item);

            menuItem.addEventListener("click", () => {
                this.blockSuggestionMenu.closeMenu();
                this.blockSuggestionMenu.clearQuery();
                item.onItemClick();
            });
            menuItem.addEventListener("mousedown", ev => ev.preventDefault());

            this.menu.appendChild(menuItem);
        }

        return this.menu;
    }

    private createSuggestionMenuItem(item: DefaultSuggestionItem): HTMLElement {
        const li = document.createElement("li");
        const iconClass = iconTable[item.key] || "ti ti-square";

        li.classList.add("bn-suggestion-menu-item");
        li.dataset.menuKey = item.key;

        const iconContainerElement = document.createElement("span");
        iconContainerElement.classList.add("bn-icon");
        const iconElement = document.createElement("i");
        iconElement.className = iconClass;
        iconContainerElement.appendChild(iconElement);
        li.appendChild(iconContainerElement);

        const titleElement = document.createElement("span");
        titleElement.classList.add("title");
        titleElement.textContent = item.title;
        li.appendChild(titleElement);

        const descriptionElement = document.createElement("span");
        descriptionElement.classList.add("description");
        if (item.key === "image") {
            descriptionElement.textContent = "Add an image";
        }
        else if (item.key === "file") {
            descriptionElement.textContent = "Add a file";
        }
        else {
            descriptionElement.textContent = item.subtext ?? "";
        }
        li.appendChild(descriptionElement);

        const badgeContainerElement = document.createElement("span");
        badgeContainerElement.classList.add("bn-badge");
        const badgeElement = document.createElement("span");
        badgeElement.textContent = item.badge ?? "";
        badgeElement.classList.add("label", "label-default");
        badgeContainerElement.appendChild(badgeElement);
        li.appendChild(badgeContainerElement);

        return li;
    }

    private updateMenu(menu: HTMLElement, items: DefaultSuggestionItem[]) {
        let index = 0;
        this.filteredMenuItems = items;

        for (const li of menu.querySelectorAll("li")) {
            const key = li.dataset.menuKey;

            li.style.display = items.find(item => item.key === key) ? "" : "none";
            li.classList.remove("selected");

            index++;
        }

        const selectedItem = items[this.selectedIndex];
        if (selectedItem) {
            const selectedElement = menu.querySelector(`li[data-menu-key="${selectedItem.key}"]`);
            if (selectedElement) {
                selectedElement.classList.add("selected");
            }
        }
    }

    private setSelectedIndex(index: number): void {
        this.selectedIndex = index;

        if (this.menu) {
            this.updateMenu(this.menu, this.filteredMenuItems);

            const selectedItem = this.filteredMenuItems[this.selectedIndex];

            if (selectedItem) {
                const selectedElement = this.menu.querySelector(`li[data-menu-key="${selectedItem.key}"]`);

                if (selectedElement) {
                    selectedElement.scrollIntoView({ behavior: "instant", block: "nearest" });
                }
            }
        }
    }

    private onKeyboardEvent = (event: KeyboardEvent): boolean => {
        if (!this.menu) {
            return false;
        }

        const items = this.filteredMenuItems;

        if (event.key === "ArrowUp") {
            event.preventDefault();

            if (items.length) {
                this.setSelectedIndex((this.selectedIndex - 1 + items!.length) % items!.length);
            }

            return true;
        }

        if (event.key === "ArrowDown") {
            event.preventDefault();

            if (items.length) {
                this.setSelectedIndex((this.selectedIndex + 1) % items!.length);
            }

            return true;
        }

        if (event.key === "PageUp") {
            event.preventDefault();

            if (items.length) {
                this.setSelectedIndex(0);
            }

            return true;
        }

        if (event.key === "PageDown") {
            event.preventDefault();

            if (items.length) {
                this.setSelectedIndex(items.length - 1);
            }

            return true;
        }

        if (event.key === "Enter") {
            event.preventDefault();
            event.stopPropagation();

            if (items.length) {
                this.blockSuggestionMenu.closeMenu();
                this.blockSuggestionMenu.clearQuery();
                items[this.selectedIndex].onItemClick();
            }

            return true;
        }

        if (event.key === "Escape") {
            event.preventDefault();
            event.stopPropagation();
            this.blockSuggestionMenu.closeMenu();
        }

        return false;
    }
}
