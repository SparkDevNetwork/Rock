import { create } from '../documentUtils';
import toolboxIcon from '../img/toolboxIcon.svg';
import newToLeftIcon from '../img/new-to-left.svg';
import newToRightIcon from '../img/new-to-right.svg';
import closeIcon from '../img/cross.svg';

const CSS = {
  hidden: 'tc-hidden',
  displayNone: 'tc-display-none',
  toolboxColumn: 'tc-toolbox-column',
  toolboxColumnMenu: 'tc-toolbox-column__menu',
  toolboxDelete: 'tc-toolbox-delete',
  toolboxDeleteColumn: 'tc-toolbox-delete--column',
  toolboxOption: 'tc-toolbox-row__option',
  menuAnimation: 'tc-menu-animation',
  deleteConfirm: 'tc-toolbox-delete--confirm'
};

/**
 * Attributes to some elements that don't need separate styles
 * but we want to access them through the DOM
 */
const ATTRS = {
  addColumnRight: { 'add-column-right': '' },
  addColumnLeft: { 'add-column-left': '' },
  deleteColumn: { 'delete-column': '' }
};

/**
 * Toolbox for column manipulating
 */
export class ToolboxColumn {
  /**
   * Creates toolbox buttons and toolbox menus
   *
   * @param {object} api - Editor.js api
   */
  constructor(api) {
    this.api = api;

    this.element = this.createToolboxColumn();

    // column above which the toolbox icon should be displayed, 0 means hide
    this.column = 0;

    // Confirmation for deleting a column
    this.showDeleteConfirmation = false;
  }

  /**
   * Creating a toolbox to open menu for a manipulating columns
   *
   * @returns {HTMLElement} - column toolbox wrapper
   */
  createToolboxColumn() {
    let toolboxColumnMenu = this.createMenu();
    let toolboxColumnElem = create({
      cssClasses: [ CSS.toolboxColumn ],
    });

    toolboxColumnElem.innerHTML = toolboxIcon;
    this.menu = toolboxColumnElem.appendChild(toolboxColumnMenu);

    return toolboxColumnElem;
  }

  /**
   * Creating a tooolbox column menu
   *
   * @returns {HTMLElement} - column menu
   */
  createMenu() {
    let addColumnLeftText = create({
      tagName: 'span',
      textContent: this.api.i18n.t('Add column to left')
    });
    let addColumnRightText = create({
      tagName: 'span',
      textContent: this.api.i18n.t('Add column to right')
    });
    let deleteColumnText = create({
      tagName: 'span',
      textContent: this.api.i18n.t('Delete column')
    });

    let addColumnRight = create({
      innerHTML: newToRightIcon,
      cssClasses: [ CSS.toolboxOption ],
      attrs: ATTRS.addColumnRight,
      children: [ addColumnRightText ]
    });
    let addColumnLeft = create({
      innerHTML: newToLeftIcon,
      cssClasses: [ CSS.toolboxOption ],
      attrs: ATTRS.addColumnLeft,
      children: [ addColumnLeftText ]
    });
    let deleteColumn = create({
      innerHTML: closeIcon,
      cssClasses: [CSS.toolboxDelete, CSS.toolboxOption, CSS.toolboxDeleteColumn],
      attrs: ATTRS.deleteColumn,
      children: [ deleteColumnText ]
    });

    return create({
      cssClasses: [CSS.toolboxColumnMenu, CSS.hidden],
      children: [addColumnLeft, addColumnRight, deleteColumn]
    });
  }

  /**
   * Hide delete column button for event when we only have one column left
   */
  hideDeleteButton() {
    this.menu.querySelector(`.${CSS.toolboxDeleteColumn}`).classList.add(CSS.displayNone);
  }

  /**
   * Unhide delete column button when we have more than one column left again
   */
  unhideDeleteButton() {
    this.menu.querySelector(`.${CSS.toolboxDeleteColumn}`).classList.remove(CSS.displayNone);
  }

  /**
   * Show toolbox column menu when the column toolbox was clicked
   */
  openToolboxMenu() {
    this.showDeleteConfirmation = false;
    this.menu.classList.add(CSS.menuAnimation);
    this.menu.classList.remove(CSS.hidden);
  }

  /**
   * Hide toolbox column menu
   */
  closeToolboxMenu() {
    this.menu.classList.remove(CSS.menuAnimation);
    this.menu.classList.add(CSS.hidden);
    this.unsetDeleteConfiramtion();
  }

  /**
   * Set the class to confirm deletion for the column menu
   */
  setDeleteConfirmation() {
    this.showDeleteConfirmation = true;
    this.menu.querySelector(`.${CSS.toolboxDelete}`).classList.add(CSS.deleteConfirm);
  }

  /**
   * Remove the class to confirm deletion for the column menu
   */
  unsetDeleteConfiramtion() {
    this.menu.querySelector(`.${CSS.toolboxDelete}`).classList.remove(CSS.deleteConfirm);
  }

  /**
   * Change toolbox icon position
   *
   * @param {number} numberOfColumns - number of columns in the table
   * @param {number} column - current column, if 0 then hide toolbox
   */
  updateToolboxIconPosition(numberOfColumns = 0, column = this.column) {
    this.column = column;

    if (this.column <= 0 || this.column > numberOfColumns) {
      this.element.style.opacity = '0';
    } else {
      // calculate padding for the icon
      this.element.style.cssText = 'opacity: 1; ' + `left: calc((100% - var(--cell-size)) / (${numberOfColumns} * 2) * (1 + (${column} - 1) * 2))`;
    }

    if (numberOfColumns == 1) {
      this.hideDeleteButton();
    } else {
      this.unhideDeleteButton();
    }
  }
}