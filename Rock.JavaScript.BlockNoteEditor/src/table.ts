import { createTableBlockSpec, type Extension, type ExtensionOptions } from "@blocknote/core";
import { TableHandlesExtension, type TableHandlesState } from "@blocknote/core/extensions";
import { Hover, Menu } from "./functions";
import type { RockBlockNoteEditor } from "./schema";

const tableBlockSpec = createTableBlockSpec();

// BlockNote's default table spec installs ProseMirror's column-resizing plugin.
// In this project we disable it because we will be displaying content on
// different platforms with different widths available. So tables will always
// be displayed in "auto width" mode.
tableBlockSpec.extensions = tableBlockSpec.extensions?.map(fn => {
    return function (a: Omit<ExtensionOptions<any>, "options">) {
        const instance = typeof fn === "function" ? fn(a) as Extension<any, string> : fn;

        if (instance.tiptapExtensions) {
            const extension = instance.tiptapExtensions.find(ext => ext.name === "BlockNoteTableExtension") as any;

            if (extension?.config.addProseMirrorPlugins) {
                const originalPlugins = extension.config.addProseMirrorPlugins();

                extension.config.addProseMirrorPlugins = function () {
                    const plugins = originalPlugins.filter((plugin: any) => plugin.key !== "tableColumnResizing$");
                    return plugins;
                }
            }
        }

        return instance;
    }
});

/**
 * Manages table UI affordances (add row/column buttons, hover handles, and drag
 * handles) and keeps them positioned in sync with BlockNote's table-handles
 * extension state.
 */
export class TableHandles {
    /** BlockNote editor instance that this handle controller operates on. */
    private readonly editor: RockBlockNoteEditor;

    /** Root container used for positioning and for hosting the handle elements. */
    private readonly container: HTMLElement;

    /**
     * BlockNote TableHandles extension instance used to read state and perform
     * table operations.
     */
    private readonly tableHandles: ReturnType<ReturnType<typeof TableHandlesExtension>>;

    /** Button shown at table edge to append a column. */
    private readonly addColumnButton: HTMLButtonElement;

    /** Button shown at table edge to append a row. */
    private readonly addRowButton: HTMLButtonElement;

    /** Handle shown above a hovered column for column menu / drag. */
    private readonly columnHoverHandle: HTMLButtonElement;

    /** Handle shown left of a hovered row for row menu / drag. */
    private readonly rowHoverHandle: HTMLButtonElement;

    /** True while a row/column drag is active; used to switch positioning logic. */
    private isDragging: boolean = false;

    /**
     * Creates handle elements, wires events, and subscribes to extension
     * state updates.
     *
     * @param editor The BlockNote editor instance.
     * @param container The container element used for positioning and hosting the handle elements.
     */
    constructor(editor: RockBlockNoteEditor, container: HTMLElement) {
        this.editor = editor;
        this.container = container;

        this.tableHandles = editor.getExtension(TableHandlesExtension)!;

        this.addColumnButton = this.createExtendButton();
        this.addRowButton = this.createExtendButton();
        this.columnHoverHandle = this.createHoverHandleButton("column");
        this.rowHoverHandle = this.createHoverHandleButton("row");

        this.columnHoverHandle.draggable = true;
        this.rowHoverHandle.draggable = true;

        this.tableHandles.store.subscribe(this.onStateChanged);
        this.addColumnButton.addEventListener("click", this.onAddColumnClick);
        this.addRowButton.addEventListener("click", this.onAddRowClick);
        this.columnHoverHandle.addEventListener("click", this.onColumnHoverClick);
        this.columnHoverHandle.addEventListener("dragstart", this.onColumnDragStart);
        this.columnHoverHandle.addEventListener("dragend", this.onColumnDragEnd);
        this.rowHoverHandle.addEventListener("click", this.onRowHoverClick);
        this.rowHoverHandle.addEventListener("dragstart", this.onRowDragStart);
        this.rowHoverHandle.addEventListener("dragend", this.onRowDragEnd);
    }

    /**
     * Creates the small "+" button used to extend the table (row/column).
     *
     * @returns The created HTMLButtonElement.
     */
    private createExtendButton(): HTMLButtonElement {
        const button = document.createElement('button');
        button.type = 'button';
        button.classList.add('bn-button', "bn-extend-button");
        button.innerHTML = `<i class="ti ti-plus"></i>`;
        button.style.display = "none";

        this.container.appendChild(button);

        return button;
    }

    /**
     * Creates the row/column hover handle button (grip) used for menu and drag
     * interactions.
     *
     * @param orientation The orientation of the handle ("row" or "column").
     *
     * @returns The created HTMLButtonElement.
     */
    private createHoverHandleButton(orientation: "row" | "column"): HTMLButtonElement {
        const button = document.createElement("button");
        button.type = "button";
        button.classList.add("bn-button", "bn-table-hover-handle");
        button.classList.add(orientation === "row" ? "bn-table-row-handle" : "bn-table-column-handle");

        // Purely visual; functionality (drag/select) can be added later.
        button.innerHTML =
            orientation === "row"
                ? `<i class="ti ti-grip-vertical"></i>`
                : `<i class="ti ti-grip-horizontal"></i>`;

        button.style.display = "none";

        // Attach once to our overlay container and only toggle/position it.
        this.container.appendChild(button);

        return button;
    }

    /**
     * Appends a single column to the current table block (if any).
     */
    private onAddColumnClick = (): void => {
        const block = this.tableHandles.store.state?.block;

        if (!block) {
            return;
        }

        this.editor.updateBlock(block, {
            type: "table",
            content: {
                ...block.content,
                // BlockNote stores the full grid under `content.rows`.
                // Even when adding a column, the helper returns a new `rows` matrix.
                rows: this.tableHandles.addRowsOrColumns(block, "columns", 1),
            } as any, // `updateBlock` typing is narrower than the runtime table schema here.
        });
    }

    /**
     * Appends a single row to the current table block (if any).
     */
    private onAddRowClick = (): void => {
        const block = this.tableHandles.store.state?.block;

        if (!block) {
            return;
        }

        this.editor.updateBlock(block, {
            type: "table",
            content: {
                ...block.content,
                // Helper returns a new `rows` matrix with an extra row.
                rows: this.tableHandles.addRowsOrColumns(block, "rows", 1),
            } as any, // `updateBlock` typing is narrower than the runtime table schema here.
        });
    }

    /**
     * Opens the column context menu at the column hover handle.
     */
    private onColumnHoverClick = (): void => {
        const menu = new Menu();
        const index = this.tableHandles.store.state?.colIndex || 0;

        menu.addItem("Delete column", () => {
            this.tableHandles.removeRowOrColumn(index, "column");
            hover.dispose();
        });

        menu.addItem("Add column left", () => {
            this.tableHandles.addRowOrColumn(index, { orientation: "column", side: "left" });
            hover.dispose();
        });

        menu.addItem("Add column right", () => {
            this.tableHandles.addRowOrColumn(index, { orientation: "column", side: "right" });
            hover.dispose();
        });

        const hover = Hover.showMenu(menu, this.container, this.columnHoverHandle);
    }

    /**
     * Starts a column drag via the table handles extension.
     *
     * @param ev The drag event.
     */
    private onColumnDragStart = (ev: DragEvent): void => {
        this.isDragging = true;
        this.tableHandles.colDragStart(ev);
    }

    /**
     * Ends an active column drag and resets drag state.
     */
    private onColumnDragEnd = (): void => {
        this.isDragging = false;
        this.tableHandles.dragEnd();
    }

    /**
     * Starts a row drag via the table handles extension.
     *
     * @param ev The drag event.
     */
    private onRowDragStart = (ev: DragEvent): void => {
        this.isDragging = true;
        this.tableHandles.rowDragStart(ev);
    }

    /**
     * Ends an active row drag and resets drag state.
     */
    private onRowDragEnd = (): void => {
        this.isDragging = false;
        this.tableHandles.dragEnd();
    }

    /**
     * Opens the row context menu at the row hover handle.
     */
    private onRowHoverClick = (): void => {
        const menu = new Menu();
        const index = this.tableHandles.store.state?.rowIndex || 0;

        menu.addItem("Delete row", () => {
            this.tableHandles.removeRowOrColumn(index, "row");
            hover.dispose();
        });

        menu.addItem("Add row above", () => {
            this.tableHandles.addRowOrColumn(index, { orientation: "row", side: "above" });
            hover.dispose();
        });

        menu.addItem("Add row below", () => {
            this.tableHandles.addRowOrColumn(index, { orientation: "row", side: "below" });
            hover.dispose();
        });

        const hover = Hover.showMenu(menu, this.container, this.rowHoverHandle);
    }

    /**
     * Reacts to TableHandles extension state changes by positioning and showing/hiding
     * hover handles and extend buttons.
     *
     * @param state The current state of the TableHandles extension.
     */
    private onStateChanged = (state: { currentVal: TableHandlesState | undefined }): void => {
        const current = state.currentVal;

        if (!current) {
            return;
        }

        // The extension provides viewport-relative positions; convert them to coordinates
        // local to our container for absolute positioning.
        const containerRect = this.container.getBoundingClientRect();

        if (this.isDragging) {
            if (!current.draggingState) {
                return;
            }

            if (current.draggingState.draggedCellOrientation === "col") {
                // Center the handle on the cursor while dragging columns (X axis only).
                this.columnHoverHandle.style.left = `${current.draggingState.mousePos - containerRect.left - (this.columnHoverHandle.clientWidth / 2)}px`;
            }

            if (current.draggingState.draggedCellOrientation === "row") {
                // Center the handle on the cursor while dragging rows (Y axis only).
                this.rowHoverHandle.style.top = `${current.draggingState.mousePos - containerRect.top - (this.rowHoverHandle.clientHeight / 2)}px`;
            }

            return;
        }


        // Hover handles: shown when the extension says we're over a table cell.
        if (current.show && current.referencePosCell) {
            const cellRect = current.referencePosCell;
            const tableRect = current.referencePosTable;

            this.columnHoverHandle.style.display = "";
            this.rowHoverHandle.style.display = "";

            // Column handle: centered above the hovered cell.
            this.columnHoverHandle.style.left = `${cellRect.left - containerRect.left + (cellRect.width - this.columnHoverHandle.clientWidth) / 2}px`;
            this.columnHoverHandle.style.top = `${tableRect.top - containerRect.top - (this.columnHoverHandle.clientHeight / 2)}px`;

            // Row handle: centered to the left of the hovered cell.
            this.rowHoverHandle.style.left = `${tableRect.left - containerRect.left - (this.rowHoverHandle.clientWidth / 2)}px`;
            this.rowHoverHandle.style.top = `${cellRect.top - containerRect.top + (cellRect.height - this.rowHoverHandle.clientHeight) / 2}px`;
        }
        else {
            this.columnHoverHandle.style.display = "none";
            this.rowHoverHandle.style.display = "none";
        }

        // Handle the add column button.
        if (state.currentVal?.showAddOrRemoveColumnsButton) {
            this.addColumnButton.style.display = "";

            // +2px keeps the button from visually touching the table border.
            this.addColumnButton.style.left = `${state.currentVal.referencePosTable.right - containerRect.left + 2}px`;
            this.addColumnButton.style.top = `${state.currentVal.referencePosTable.top - containerRect.top}px`;
            this.addColumnButton.style.height = `${state.currentVal.referencePosTable.height}px`;
        }
        else {
            this.addColumnButton.style.display = "none";
        }

        // Handle the add row button.
        if (state.currentVal?.showAddOrRemoveRowsButton) {
            this.addRowButton.style.display = "";
            this.addRowButton.style.left = `${state.currentVal.referencePosTable.left - containerRect.left}px`;

            // +2px keeps the button from visually touching the table border.
            this.addRowButton.style.top = `${state.currentVal.referencePosTable.bottom - containerRect.top + 2}px`;
            this.addRowButton.style.width = `${state.currentVal.referencePosTable.width}px`;
        }
        else {
            this.addRowButton.style.display = "none";
        }
    }
}

export { tableBlockSpec };
