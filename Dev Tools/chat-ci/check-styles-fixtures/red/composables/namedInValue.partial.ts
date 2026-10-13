// A named color inside a string that reads as a CSS value, here a ring drawn as a shadow.
export function ring(element: HTMLElement): void {
    element.style.boxShadow = "0 0 0 2px red";
}
