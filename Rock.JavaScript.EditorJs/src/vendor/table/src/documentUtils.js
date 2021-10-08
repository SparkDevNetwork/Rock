/**
 * Create DOM element with set parameters
 *
 * @param {Object} element - element params to create an HTML element
 * @param {string} element.tagName - Html tag of the element to be created
 * @param {string[]} element.cssClasses - Css classes that must be applied to an element
 * @param {object} element.attrs - Attributes that must be applied to the element
 * @param {Element[]} element.children - child elements of creating element
 * @param {string} element.innerHTML - string with html elements to set before adding childs
 * @param {string} element.textContent - text content to set
 *
 * @returns {HTMLElement} the new element
 */
export function create({
  tagName = 'div',
  cssClasses,
  attrs,
  children,
  innerHTML,
  textContent,
}) {
  const elem = document.createElement(tagName);

  if (cssClasses) {
    elem.classList.add(...cssClasses.filter(className => !!className));
  }

  if (attrs) {
    for (let key in attrs) {
      elem.setAttribute(key, attrs[key]);
    }
  }

  if (textContent) {
    elem.textContent = textContent;
  }

  if (innerHTML) {
    elem.innerHTML = innerHTML;
  }

  if (children) {
    for (let i = 0; i < children.length; i++) {
      if (children[i]) {
        elem.append(children[i]);
      }
    }
  }

  return elem;
}

/**
 * Get item position relative to document
 *
 * @param {HTMLElement} elem - item
 * @returns {{x1: number, y1: number, x2: number, y2: number}} coordinates of the upper left (x1,y1) and lower right(x2,y2) corners
 */
export function getCoords(elem) {
  const rect = elem.getBoundingClientRect();

  return {
    y1: Math.floor(rect.top + window.pageYOffset),
    x1: Math.floor(rect.left + window.pageXOffset),
    x2: Math.floor(rect.right + window.pageXOffset),
    y2: Math.floor(rect.bottom + window.pageYOffset)
  };
}

/**
 * Calculate paddings of the first element relative to the second
 *
 * @param {HTMLElement} firstElem - outer element, if the second element is inside it, then all padding will be positive
 * @param {HTMLElement} secondElem - inner element, if its borders go beyond the first, then the paddings will be considered negative
 * @returns {{fromTopBorder: number, fromLeftBorder: number, fromRightBorder: number, fromBottomBorder: number}}
 */
export function getRelativeCoordsOfTwoElems(firstElem, secondElem) {
  const firstCoords = getCoords(firstElem);
  const secondCoords = getCoords(secondElem);

  return {
    fromTopBorder: secondCoords.y1 - firstCoords.y1,
    fromLeftBorder: secondCoords.x1 - firstCoords.x1,
    fromRightBorder: firstCoords.x2 - secondCoords.x2,
    fromBottomBorder: firstCoords.y2 - secondCoords.y2
  };
}

/**
 * Get the width and height of an element and the position of the cursor relative to it
 *
 * @param {HTMLElement} elem - element relative to which the coordinates will be calculated
 * @param {Event} event - mouse event
 */
export function getCursorPositionRelativeToElement(elem, event) {
  const rect = elem.getBoundingClientRect();
  const { width, height, x, y } = rect;
  const { clientX, clientY } = event;

  return {
    width,
    height,
    x: clientX - x,
    y: clientY - y
  };
}

/**
 * Insert element after the referenced
 *
 * @param {HTMLElement} newNode
 * @param {HTMLElement} referenceNode
 * @returns {HTMLElement}
 */
export function insertAfter(newNode, referenceNode) {
  return referenceNode.parentNode.insertBefore(newNode, referenceNode.nextSibling);
}

/**
 * Insert element after the referenced
 *
 * @param {HTMLElement} newNode
 * @param {HTMLElement} referenceNode
 * @returns {HTMLElement}
 */
export function insertBefore(newNode, referenceNode) {
  return referenceNode.parentNode.insertBefore(newNode, referenceNode);
}

/**
 * Limits the frequency of calling a function
 * 
 * @param {number} delay - delay between calls in milliseconds
 * @param {function} fn - function to be throttled
 */
export function throttled(delay, fn) {
  let lastCall = 0;

  return function (...args) {
    const now = new Date().getTime();

    if (now - lastCall < delay) {
      return;
    }

    lastCall = now;

    return fn(...args);
  };
}
