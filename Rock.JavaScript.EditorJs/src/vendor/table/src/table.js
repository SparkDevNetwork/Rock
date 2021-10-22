import { create, getCursorPositionRelativeToElement, getRelativeCoordsOfTwoElems, throttled, insertBefore } from './documentUtils';
import './styles/table.pcss';
import './styles/toolboxes.pcss';
import './styles/utils.pcss';
import './styles/settings.pcss';
import svgPlusButton from './img/plus.svg';
import { ToolboxColumn } from './toolbox/toolboxColumn';
import { ToolboxRow } from './toolbox/toolboxRow';

const CSS = {
  wrapper: 'tc-wrap',
  table: 'tc-table',
  row: 'tc-row',
  withHeadings: 'tc-table--heading',
  rowSelected: 'tc-row--selected',
  cell: 'tc-cell',
  cellSelected: 'tc-cell--selected',
  addRow: 'tc-add-row',
  addColumn: 'tc-add-column',
  toolboxColumnMenu: 'tc-toolbox-column__menu',
  toolboxRowMenu: 'tc-toolbox-row__menu'
};

// Attributes for getting elements by them
const ATTRS = {
  addRowAbove: 'add-row-above',
  addRowBelow: 'add-row-below',
  deleteRow: 'delete-row',
  addColumnRight: 'add-column-right',
  addColumnLeft: 'add-column-left',
  deleteColumn: 'delete-column'
};

/**
 * Generates and manages table contents.
 */
export class Table {
  /**
   * Creates
   *
   * @constructor
   * @param {boolean} readOnly - read-only mode flag
   * @param {object} api - Editor.js API
   */
  constructor(readOnly, api) {
    this.readOnly = readOnly;
    this.api = api;

    // Toolboxes for managing rows and columns
    this.toolboxColumn = new ToolboxColumn(api);
    this.toolboxRow = new ToolboxRow(api);

    // Create table and wrapper elements
    this.createTableWrapper();

    // Current hovered row index
    this.hoveredRow = 0;

    // Current hovered column index
    this.hoveredColumn = 0;

    // Index of last selected row via toolbox
    this.lastSelectedRow = 0;

    // Index of last selected column via toolbox
    this.lastSelectedColumn = 0;

    // Toggle switches to confirm deletion
    this.showDeleteRowConfirmation = false;
    this.showDeleteColumnConfirmation = false;

    // Additional settings for the table
    this.tunes = {
      withHeadings: false
    }

    /**
     * The cell in which the focus is currently located, if 0 and 0 then there is no focus
     * Uses to switch between cells with buttons
     */
    this.focusedCell = {
      row: 0,
      column: 0
    };

    this.clickOutsideListener = (event) => {
      const outsideMenusClicked = event.target.closest(`.${CSS.table}`);
      const ousideTableClicked = event.target.closest(`.${CSS.wrapper}`) === null;

      if (outsideMenusClicked) {
        this.clickOutsideMenus();
      } else if (ousideTableClicked) {
        this.hideToolboxIconsAndMenus();
      }
    };

    if (!this.readOnly) {
      this.bindEvents();
    }
  }

  /**
   * Hangs the necessary handlers to events
   */
  bindEvents() {
    // set the listener to close toolboxes when click outside
    document.addEventListener('click', this.clickOutsideListener);

    // Update toolboxes position depending on the mouse movements
    this.table.addEventListener('mousemove', throttled(100, (event) => this.onMouseMoveInTable(event)), { passive: true });

    // Controls quick add buttons
    this.wrapper.addEventListener('click', (event) => this.onClickQuickAddButton(event));

    // Controls toolbox with adding and deleting columns
    this.toolboxColumn.element.addEventListener('click', (event) => this.onClickColumnToolbox(event));

    // Controls toolbox with adding and deleting rows
    this.toolboxRow.element.addEventListener('click', (event) => this.onClickRowToolbox(event));

    // Controls some of the keyboard buttons inside the table
    this.table.onkeypress = (event) => this.onKeyPressListener(event);

    // Tab is executed by default before keypress, so it must be intercepted on keydown
    this.table.addEventListener('keydown', (event) => this.onKeyDownListener(event));

    // Determine the position of the cell in focus
    this.table.addEventListener('focusin', event => this.focusInTableListener(event));
  }

  /**
   * When you press enter it moves the cursor down to the next row
   * or creates it if the click occurred on the last one
   */
  moveCursorToNextRow() {
    if (this.focusedCell.row != this.numberOfRows) {
      this.focusedCell.row += 1;
      this.focusCell(this.focusedCell);
    } else {
      this.addRow();
      this.focusedCell.row += 1;
      this.focusCell(this.focusedCell);
      this.updateToolboxesPosition(0, 0);
    }
  }

  /**
   * Get tabel cell
   *
   * @param {number} row - cell row coordinate
   * @param {number} column - cell column coordinate
   * @returns {HTMLElement}
   */
  getCell(row, column) {
    return this.table.querySelector(`.${CSS.row}:nth-child(${row}) .${CSS.cell}:nth-child(${column})`);
  }

  /**
   * Get tabel row
   *
   * @param {number} row - row coordinate
   * @returns {HTMLElement}
   */
  getRow(row) {
    return this.table.querySelector(`.${CSS.row}:nth-child(${row})`);
  }

  /**
   * The parent of the cell which is the row
   *
   * @param {HTMLElement} cell - cell element
   * @returns {HTMLElement}
   */
  getRowByCell(cell) {
    return cell.parentElement;
  }

  /**
   * Add column in table on index place
   * Add cells in each row
   *
   * @param {number} columnIndex - number in the array of columns, where new column to insert, -1 if insert at the end
   */
  addColumn(columnIndex = -1) {
    let numberOfColumns = this.numberOfColumns;
    
    /**
     * Iterate all rows and add a new cell to them for creating a column
     */
    for (let rowIndex = 1; rowIndex <= this.numberOfRows; rowIndex++) {
      let cell;
      const cellElem = this.createCell();

      if (columnIndex > 0 && columnIndex <= numberOfColumns) {
        cell = this.getCell(rowIndex, columnIndex);

        insertBefore(cellElem, cell);
      } else {
        cell = this.getRow(rowIndex).appendChild(cellElem);
      }
    }

    this.addHeadingAttrToFirstRow();
  };

  /**
   * Add row in table on index place
   *
   * @param {number} index - number in the array of rows, where new column to insert, -1 if insert at the end
   * @returns {HTMLElement} row
   */
  addRow(index = -1) {
    let insertedRow;
    let rowElem = create({
      cssClasses: [ CSS.row ],
    });

    if (this.tunes.withHeadings) {
      this.removeHeadingAttrFromFirstRow();
    }

    /**
     * We remember the number of columns, because it is calculated 
     * by the number of cells in the first row
     * It is necessary that the first line is filled in correctly
     */
    let numberOfColumns = this.numberOfColumns;

    if (index > 0 && index <= this.numberOfRows) {
      let row = this.getRow(index);

      insertedRow = insertBefore(rowElem, row);
    } else {
      insertedRow = this.table.appendChild(rowElem);
    }

    this.fillRow(insertedRow, numberOfColumns);

    if (this.tunes.withHeadings) {
      this.addHeadingAttrToFirstRow();
    }

    return insertedRow;
  };

  /**
   * Delete a column by index
   *
   * @param {number} index
   */
  deleteColumn(index) {
    for (let i = 1; i <= this.numberOfRows; i++) {
      const cell = this.getCell(i, index);

      if (!cell) {
        return;
      }

      cell.remove();
    }
  }

  /**
   * Delete a row by index
   *
   * @param {number} index
   */
  deleteRow(index) {
    this.getRow(index).remove();

    this.addHeadingAttrToFirstRow();
  }

  /**
   * Create a wrapper containing a table, toolboxes
   * and buttons for adding rows and columns
   *
   * @returns {HTMLElement} wrapper - where all buttons for a table and the table itself will be
   */
  createTableWrapper() {
    this.wrapper = create({
      cssClasses: [ CSS.wrapper ],
      children: [ this.toolboxRow.element, this.toolboxColumn.element ]
    })

    this.table = this.wrapper.appendChild(create({
      cssClasses: [ CSS.table ],
    }));
    this.wrapper.append(
      create({
        innerHTML: svgPlusButton,
        cssClasses: [ CSS.addColumn ]
      }),
      create({
        innerHTML: svgPlusButton,
        cssClasses: [ CSS.addRow ]
      }));
  }

  /**
   * Fills a row with cells
   *
   * @param {HTMLElement} row
   */
  fillRow(row, numberOfColumns) {
    for (let i = 1; i <= numberOfColumns; i++) {
      const newCell = this.createCell();

      row.appendChild(newCell);
    }
  }

  /**
   * Createing a cell element
   *
   * @return {HTMLElement}
   */
  createCell() {
    return create({
      cssClasses: [ CSS.cell ],
      attrs: {
        contenteditable: !this.readOnly,
      }
    })
  }

  /**
   * Get number of rown in the table
   */
  get numberOfRows() {
    return this.table.childElementCount;
  }

  /**
   * Get number of columns in the table
   */
  get numberOfColumns() {
    if (this.numberOfRows) {
      return this.table.querySelector(`.${CSS.row}:first-child`).childElementCount;
    }

    return 0;
  }

  /**
   * Is the column toolbox menu displayed or not
   *
   * @returns {boolean}
   */
  get isColumnMenuShowing() {
    return this.lastSelectedColumn != 0;
  }

  /**
   * Is the row toolbox menu displayed or not
   *
   * @returns {boolean}
   */
  get isRowMenuShowing() {
    return this.lastSelectedRow != 0;
  }

  /**
   * Recaculate position of toolbox icons
   *
   * @param {Event} event - mouse move event
   */
  onMouseMoveInTable(event) {
    const { row, column } = this.hoveredCell(event);

    this.updateToolboxesPosition(row, column);
  }

  /**
   * Controls buttons for quick adding rows and column
   *
   * @param {Event} event - mouse click event
   */
  onClickQuickAddButton(event) {
    const addRowClicked = event.target.closest(`.${CSS.addRow}`);
    const addColumnClicked = event.target.closest(`.${CSS.addColumn}`);

    if (addRowClicked) {
      this.addRow();
      this.hideToolboxIconsAndMenus();
    }

    if (addColumnClicked) {
      this.addColumn();
      this.hideToolboxIconsAndMenus();
    }
  }

  /**
   * Controls toolbox for controlling columns
   *
   * @param {Event} event - mouse click event
   */
  onClickColumnToolbox(event) {
    event.stopPropagation();

    const toolboxColumnIconClicked = event.target.closest('svg');
    const addColumnRightClicked = event.target.closest(`[${ATTRS.addColumnRight}]`);
    const addColumnLeftClicked = event.target.closest(`[${ATTRS.addColumnLeft}]`);
    const deleteColumnClicked = event.target.closest(`[${ATTRS.deleteColumn}]`);

    if (addColumnRightClicked) {
      this.addColumn(this.lastSelectedColumn + 1);
      this.hideAndUnselect();

      return;
    }

    if (addColumnLeftClicked) {
      this.addColumn(this.lastSelectedColumn);
      this.hideAndUnselect();

      return;
    }

    if (deleteColumnClicked) {
      if (this.toolboxColumn.showDeleteConfirmation) {
        this.deleteColumn(this.lastSelectedColumn);
        this.hideToolboxIconsAndMenus();
      } else {
        this.toolboxColumn.setDeleteConfirmation();
      }

      return;
    }

    // Open/close toolbox column menu
    if (toolboxColumnIconClicked) {
      this.unselectRowAndHideMenu();

      if (this.hoveredColumn == this.lastSelectedColumn) {
        this.unselectColumnAndHideMenu();

        return;
      }

      this.showDeleteColumnConfirmation = false;
      this.selectColumnAndOpenMenu();
    }
  }

  /**
   * Controls toolbox for controlling rows
   *
   * @param {Event} event
   */
  onClickRowToolbox(event) {
    event.stopPropagation();

    const toolboxRowIconClicked = event.target.closest('svg');
    const addRowAboveClicked = event.target.closest(`[${ATTRS.addRowAbove}]`);
    const addRowBelowClicked = event.target.closest(`[${ATTRS.addRowBelow}]`);
    const deleteRowClicked = event.target.closest(`[${ATTRS.deleteRow}]`);

    if (addRowAboveClicked) {
      this.addRow(this.lastSelectedRow);
      this.hideAndUnselect();

      return;
    }

    if (addRowBelowClicked) {
      this.addRow(this.lastSelectedRow + 1);
      this.hideAndUnselect();

      return;
    }

    if (deleteRowClicked) {
      if (this.toolboxRow.showDeleteConfirmation) {
        this.deleteRow(this.lastSelectedRow);
        this.hideToolboxIconsAndMenus();
        this.showDeleteRowConfirmation = false;
      } else {
        this.toolboxRow.setDeleteConfirmation();
      }

      return;
    }

    // Open/close toolbox column menu
    if (toolboxRowIconClicked) {
      this.unselectColumnAndHideMenu();

      if (this.hoveredRow == this.lastSelectedRow) {
        this.unselectRowAndHideMenu();

        return;
      }

      this.showDeleteRowConfirmation = false;
      this.selectRowAndOpenMenu();
    }
  }

  /**
   * Prevents default Enter behaviors
   * Adds Shift+Enter processing
   * 
   * @param {KeyboardEvent} event - keypress event
   */
  onKeyPressListener(event){
    if (event.key == 'Enter') {
      if (event.shiftKey) {
        return true;
      }

      this.moveCursorToNextRow();
    }

    return event.key != 'Enter';
  };

  /**
   * Prevents tab keydown event from bubbling 
   * so that it only works inside the table 
   *
   * @param {KeyboardEvent} event - keydown event
   */
  onKeyDownListener(event) {
    if (event.key == 'Tab') {
      event.stopPropagation();
    }
  }
  
  /**
   * Set the coordinates of the cell that the focus has moved to
   * 
   * @param {FocusEvent} event - focusin event
   */
  focusInTableListener(event) {
    const cell = event.target;
    const row = this.getRowByCell(cell);

    this.focusedCell = {
      row: Array.from(this.table.querySelectorAll(`.${CSS.row}`)).indexOf(row) + 1,
      column: Array.from(row.querySelectorAll(`.${CSS.cell}`)).indexOf(cell) + 1
    };
  }
  /**
   * Close toolbox menu and unselect a row/column
   * but doesn't hide toolbox button
   */
  clickOutsideMenus() {
    this.unselectColumn();
    this.toolboxColumn.closeToolboxMenu();
    this.unselectRow();
    this.toolboxRow.closeMenu();
  }

  /**
   * Unselect row/column
   * Close toolbox menu
   * Hide toolboxes
   */
  hideToolboxIconsAndMenus() {
    this.unselectRow();
    this.unselectColumn();
    this.toolboxRow.closeMenu();
    this.toolboxColumn.closeToolboxMenu();
    this.updateToolboxesPosition(0, 0);
  }

  /**
   * Unselect row/column
   * Close toolbox menu
   * Recalculates the position of the toolbox buttons
   */
  hideAndUnselect() {
    this.unselectRow();
    this.unselectColumn();
    this.toolboxRow.closeMenu();
    this.toolboxColumn.closeToolboxMenu();
    this.updateToolboxesPosition();
  }

  /**
   * Set the cursor focus to the focused cell
   */
  focusCell() {
    this.focusedCellElem.focus();
  }

  /**
   * Get current focused element
   *
   * @returns {HTMLElement} - focused cell
   */
  get focusedCellElem() {
    const { row, column } = this.focusedCell;

    return this.getCell(row, column);
  }

  /**
   * Update toolboxes position
   *
   * @param {number} row - hovered row
   * @param {number} column - hovered column
   */
  updateToolboxesPosition(row = this.hoveredRow, column = this.hoveredColumn) {
    if (!this.isColumnMenuShowing) {
      this.hoveredColumn = column;
      this.toolboxColumn.updateToolboxIconPosition(this.numberOfColumns, column);
    }

    if (!this.isRowMenuShowing) {
      this.hoveredRow = row;
      this.toolboxRow.updateToolboxIconPosition(this.numberOfRows, row, this.table);
    }
  }

  /**
   * Makes the first row headings
   *
   * @param {boolean} withHeadings - use headings row or not
   */
  setHeadingsSetting(withHeadings) {
    this.tunes.withHeadings = withHeadings;

    if (withHeadings) {
      this.table.classList.add(CSS.withHeadings);
      this.addHeadingAttrToFirstRow();
    } else {
      this.table.classList.remove(CSS.withHeadings);
      this.removeHeadingAttrFromFirstRow();
    }
  }

  /**
   * Adds an attribute for displaying the placeholder in the cell
   */
  addHeadingAttrToFirstRow() {
    for (let cellIndex = 1; cellIndex <= this.numberOfColumns; cellIndex++) {
      let cell = this.getCell(1, cellIndex);
      
      if (cell) {
        cell.setAttribute('heading', this.api.i18n.t('Heading'));
      }
    }
  }

  /**
   * Removes an attribute for displaying the placeholder in the cell
   */
  removeHeadingAttrFromFirstRow() {
    for (let cellIndex = 1; cellIndex <= this.numberOfColumns; cellIndex++) {
      let cell = this.getCell(1, cellIndex);
      
      if (cell) {
        cell.removeAttribute('heading');
      }
    }
  }

  /**
   * Add effect of a selected row
   *
   * @param {number} index
   */
  selectRow(index) {
    const row = this.getRow(index);

    if (row) {
      this.lastSelectedRow = index;
      row.classList.add(CSS.rowSelected);
    }
  }

  /**
   * Remove effect of a selected row
   */
  unselectRow() {
    if (this.lastSelectedRow <= 0) {
      return;
    }

    const row = this.table.querySelector(`.${CSS.rowSelected}`);

    if (row) {
      row.classList.remove(CSS.rowSelected);
    }

    this.lastSelectedRow = 0;
  }

  /**
   * Add effect of a selected column
   *
   * @param {number} index
   */
  selectColumn(index) {
    for (let i = 1; i <= this.numberOfRows; i++) {
      const cell = this.getCell(i, index);

      if (cell) {
        cell.classList.add(CSS.cellSelected);
      }
    }

    this.lastSelectedColumn = index;
  }

  /**
   * Remove effect of a selected column
   */
  unselectColumn() {
    if (this.lastSelectedColumn <= 0) {
      return;
    }

    let cells = this.table.querySelectorAll(`.${CSS.cellSelected}`);

    Array.from(cells).forEach(column => {
      column.classList.remove(CSS.cellSelected);
    });

    this.lastSelectedColumn = 0;
  }

  /**
   * Calculates the row and column that the cursor is currently hovering over
   * The search was optimized from O(n) to O (log n) via bin search to reduce the number of calculations
   *
   * @param {Event} event - mousemove event
   * @returns hovered cell coordinates as an integer row and column
   */
  hoveredCell(event) {
    let hoveredRow = this.hoveredRow;
    let hoveredColumn = this.hoveredColumn;
    const { width, height, x, y } = getCursorPositionRelativeToElement(this.table, event);

    // Looking for hovered column
    if (x >= 0) {
      hoveredColumn = this.binSearch(
        this.numberOfColumns,
        (mid) => this.getCell(1, mid),
        ({ fromLeftBorder }) => x < fromLeftBorder,
        ({ fromRightBorder }) => x > (width - fromRightBorder)
      );
    }

    // Looking for hovered row
    if (y >= 0) {
      hoveredRow = this.binSearch(
        this.numberOfRows,
        (mid) => this.getCell(mid, 1),
        ({ fromTopBorder }) => y < fromTopBorder,
        ({ fromBottomBorder }) => y > (height - fromBottomBorder)
      );
    }

    return {
      row: hoveredRow || this.hoveredRow,
      column: hoveredColumn || this.hoveredColumn
    };
  }

  /**
   * Looks for the index of the cell the mouse is hovering over.
   * Cells can be represented as ordered intervals with left and
   * right (upper and lower for rows) borders inside the table, if the mouse enters it, then this is our index
   *
   * @param {number} numberOfCells - upper bound of binary search
   * @param {function} getCell - function to take the currently viewed cell
   * @param {function} beforeTheLeftBorder - determines the cursor position, to the left of the cell or not
   * @param {function} afterTheRightBorder - determines the cursor position, to the right of the cell or not
   * @returns {number}
   */
  binSearch(numberOfCells, getCell, beforeTheLeftBorder, afterTheRightBorder) {
    let leftBorder = 0;
    let rightBorder = numberOfCells + 1;
    let totalIterations = 0;
    let mid;

    while (leftBorder < rightBorder - 1 && totalIterations < 10) {
      mid = Math.ceil((leftBorder + rightBorder) / 2);

      const cell = getCell(mid);
      const relativeCoords = getRelativeCoordsOfTwoElems(this.table, cell);

      if (beforeTheLeftBorder(relativeCoords)) {
        rightBorder = mid;
      } else if (afterTheRightBorder(relativeCoords)) {
        leftBorder = mid;
      } else {
        break;
      }

      totalIterations++;
    }

    return mid;
  }

  /**
   * Remove the selection effect from the column
   * Hide toolbox column menu and remove clickOutside handler
   */
  selectRowAndOpenMenu() {
    this.selectRow(this.hoveredRow);
    this.toolboxRow.openMenu();
  }

  /**
   * Add the selection effect for the column
   * Open toolbox column menu and add clickOutside handler
   */
  selectColumnAndOpenMenu() {
    this.selectColumn(this.hoveredColumn);
    this.toolboxColumn.openToolboxMenu();
  }

  /**
   * Remove selection effect from a column
   * Hide toolbox column menu and remove clickOutside handler
   */
  unselectColumnAndHideMenu() {
    this.unselectColumn();
    this.toolboxColumn.closeToolboxMenu();
  }

  /**
   * Remove the selection effect from the row
   * Hide toolbox column menu and remove clickOutside handler
   */
  unselectRowAndHideMenu() {
    this.unselectRow();
    this.toolboxRow.closeMenu();
  }

  /**
   * Remove listeners on the document
   */
  destroy() {
    document.removeEventListener('click', this.clickOutsideListener);
  }
}
