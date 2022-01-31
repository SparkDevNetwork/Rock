import { getRelativeCoordsOfTwoElems, create } from '../documentUtils';
import toolboxIcon from '../img/toolboxIcon.svg';
import newToUp from '../img/new-to-up.svg';
import newToDown from '../img/new-to-down.svg';
import closeIcon from '../img/cross.svg';

const CSS = {
  hidden: 'tc-hidden',
  displayNone: 'tc-display-none',
  toolboxRow: 'tc-toolbox-row',
  toolboxRowMenu: 'tc-toolbox-row__menu',
  toolboxDelete: 'tc-toolbox-delete',
  toolboxDeleteRow: 'tc-toolbox-delete--row',
  toolboxOption: 'tc-toolbox-row__option',
  menuAnimation: 'tc-menu-animation',
  deleteConfirm: 'tc-toolbox-delete--confirm'
};

/**
 * Attributes to some elements that don't need separate styles
 * but we want to access them through the DOM
 */
const ATTRS = {
  addRowAbove: { 'add-row-above': '' },
  addRowBelow: { 'add-row-below': '' },
  deleteRow: { 'delete-row': '' }
};

/**
 * Toolbox for row manipulating
 */
export class ToolboxRow {
  /**
   * Creates toolbox buttons and toolbox menus
   *
   * @param {object} api - Editor.js api
   */
  constructor(api) {
    this.api = api;

    this.element = this.createToolboxRow();

    // row index to the left of which the toolbox icon should be displayed, 0 means hide
    this.row = 0;

    // Confirmation for deleting a column
    this.showDeleteConfirmation = false;
  }

  /**
   * Creating a toolbox to open menu for a manipulating rows
   *
   * @returns {HTMLElement} - row toolbox wrapper
   */
  createToolboxRow() {
    let toolboxRowMenu = this.createMenu();
    let toolboxRowElem = create({
      cssClasses: [ CSS.toolboxRow ],
    });

    toolboxRowElem.innerHTML = toolboxIcon;
    this.menu = toolboxRowElem.appendChild(toolboxRowMenu);

    return toolboxRowElem;
  }

  /**
   * Creating a tooolbox row menu
   *
   * @returns {HTMLElement} - row menu
   */
  createMenu() {
    let addRowAboveText = create({
      tagName: 'span',
      textContent: this.api.i18n.t('Add row above')
    });
    let addRowBelowText = create({
      tagName: 'span',
      textContent: this.api.i18n.t('Add row below')
    });
    let deleteRowText = create({
      tagName: 'span',
      textContent: this.api.i18n.t('Delete row')
    });

    let addRowAbove = create({
      innerHTML: newToUp,
      cssClasses: [ CSS.toolboxOption ],
      attrs: ATTRS.addRowAbove,
      children: [ addRowAboveText ]
    });
    let addRowBelow = create({
      innerHTML: newToDown,
      cssClasses: [ CSS.toolboxOption ],
      attrs: ATTRS.addRowBelow,
      children: [ addRowBelowText ]
    });
    let deleteRow = create({
      innerHTML: closeIcon,
      cssClasses: [CSS.toolboxDelete, CSS.toolboxOption, CSS.toolboxDeleteRow],
      attrs: ATTRS.deleteRow,
      children: [ deleteRowText ]
    });

    return create({
      cssClasses: [CSS.toolboxRowMenu, CSS.hidden],
      children: [addRowAbove, addRowBelow, deleteRow]
    });
  }

  /**
   * Hide delete row button for event when we only have one row left
   */
  hideDeleteButton() {
    this.menu.querySelector(`.${CSS.toolboxDeleteRow}`).classList.add(CSS.displayNone);
  }

  /**
   * Unhide delete row button when we have more than one row left again
   */
  unhideDeleteButton() {
    this.menu.querySelector(`.${CSS.toolboxDeleteRow}`).classList.remove(CSS.displayNone);
  }

  /**
   * Show toolbox row menu when the toolbox was clicked
   */
  openMenu() {
    this.showDeleteConfirmation = false;
    this.menu.classList.add(CSS.menuAnimation);
    this.menu.classList.remove(CSS.hidden);
  }

  /**
   * Hide toolbox row menu
   */
  closeMenu() {
    this.menu.classList.remove(CSS.menuAnimation);
    this.menu.classList.add(CSS.hidden);
    this.unsetDeleteConfirmation();
  }

  /**
   * Set the class to confirm deletion for the row menu
   */
  setDeleteConfirmation() {
    this.showDeleteConfirmation = true;
    this.menu.querySelector(`.${CSS.toolboxDelete}`).classList.add(CSS.deleteConfirm);
  }

  /**
   * Remove the class to confirm deletion for the row menu
   */
  unsetDeleteConfirmation() {
    this.menu.querySelector(`.${CSS.toolboxDelete}`).classList.remove(CSS.deleteConfirm);
  }

  /**
   * Change toolbox icon position
   *
   * @param {number} numberOfRows - number of rows
   * @param {number} row - hovered row
   * @param {HTMLElement} table - table element
   */
  updateToolboxIconPosition(numberOfRows = 0, row = this.row, table) {
    this.row = row;

    if (this.row <= 0 || this.row > numberOfRows) {
      this.element.style.opacity = '0';
    } else {
      const hoveredRowElement = table.querySelector(`.tc-row:nth-child(${this.row})`);
      const { fromTopBorder } = getRelativeCoordsOfTwoElems(table, hoveredRowElement);
      const { height } = hoveredRowElement.getBoundingClientRect();

      this.element.style.opacity = '1';
      this.element.style.top = `${fromTopBorder + height / 2}px`;
    }

    if (numberOfRows == 1) {
      this.hideDeleteButton();
    } else {
      this.unhideDeleteButton();
    }
  }
}