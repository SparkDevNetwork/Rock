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

export type Coords = {
    x: number;
    y: number;
};

export type ComponentTypeDragLeaveMessage = {
    type: "COMPONENT_TYPE_DRAG_LEAVE";
    componentTypeName: EditorComponentTypeName;
};

export type ComponentTypeDragDropMessage = {
    type: "COMPONENT_TYPE_DRAG_DROP";
    componentTypeName: EditorComponentTypeName;
};

export type ComponentTypeDragEndMessage = {
    type: "COMPONENT_TYPE_DRAG_END";
    componentTypeName: EditorComponentTypeName;
};

export type ComponentTypeDragOverMessage = {
    type: "COMPONENT_TYPE_DRAG_OVER";
    componentTypeName: EditorComponentTypeName;
    coords: Coords;
};

export type EditorComponent = {
    type: EditorComponentType;
    componentElement: HTMLElement;
};
