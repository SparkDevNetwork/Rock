export type EditorComponentType = {
    readonly typeName: "text"
    | "video"
    | "button"
    | "paragraph"
    | "divider"
    | "message"
    | "image"
    | "code"
    | "rsvp"
    | "section"
    | "one-column-section"
    | "two-column-section"
    | "three-column-section";
    iconCssClass: string;
    title: string;
    createComponentPlaceholder(d: Document): HTMLElement;
    createComponentElement(d: Document): HTMLElement;
};

export type EditorComponentTypeName = EditorComponentType["typeName"];