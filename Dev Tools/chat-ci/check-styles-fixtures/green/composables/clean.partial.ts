// A script that sets a value the sheet's recipe reads, and words that are not colors.
export function paint(element: HTMLElement, hue: number): string {
    element.style.setProperty("--chat-av-hue", String(hue));
    return "Marked as read";
}
