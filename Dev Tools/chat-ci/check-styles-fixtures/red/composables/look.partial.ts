// A script that animates with its own timing instead of the sheet's, which the gate refuses.
export function slideIn(element: HTMLElement): void {
    element.animate([{ opacity: 0 }, { opacity: 1 }], { duration: 200, easing: "cubic-bezier(.1, .9, .2, 1)" });
}
