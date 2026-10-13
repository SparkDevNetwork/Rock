// A hex color inside a longer CSS value in script, which a whole-string check misses.
export function outline(element: HTMLElement): void {
    element.style.border = "1px solid #fff";
}
