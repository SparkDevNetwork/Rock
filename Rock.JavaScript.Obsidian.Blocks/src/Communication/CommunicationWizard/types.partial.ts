export type EditorComponentType =
    "title" |
    "video" |
    "button" |
    "paragraph" |
    "divider" |
    "message" |
    "image" |
    "lava" |
    "rsvp";

export type EditorComponent = {
    readonly type: EditorComponentType;
    iconCssClass: string;
    name: string;
    createPlaceholder(d: Document): HTMLElement;
    createElement(d: Document): HTMLElement;
};