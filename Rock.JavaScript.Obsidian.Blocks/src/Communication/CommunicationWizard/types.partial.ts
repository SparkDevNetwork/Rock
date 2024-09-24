export type EditorComponentType = {
    readonly typeName: "title"
    | "video"
    | "button"
    | "paragraph"
    | "divider"
    | "message"
    | "image"
    | "lava"
    | "rsvp";
    iconCssClass: string;
    name: string;
    createComponentPlaceholder(d: Document): HTMLElement;
    createComponentElement(d: Document): HTMLElement;
};

export type EditorComponentTypeName = EditorComponentType["typeName"];