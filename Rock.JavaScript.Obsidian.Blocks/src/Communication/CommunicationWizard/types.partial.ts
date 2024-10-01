export type EditorComponentType = {
    readonly typeName:
    "video"
    | "button"
    | "text"
    | "divider"
    | "message"
    | "image"
    | "lava"
    | "code"
    | "rsvp"
    | "section"
    | "one-column-section"
    | "two-column-section"
    | "three-column-section"
    | "title";
    iconCssClass: string;
    title: string;
    createComponentPlaceholder(d: Document): HTMLElement;
    createComponentElement(d: Document): HTMLElement;
};

export type EditorComponentTypeName = EditorComponentType["typeName"];

export type ComponentTypeDragLeaveMessage = {
    type: "COMPONENT_TYPE_DRAG_LEAVE";
};

export type ComponentTypeDragDropMessage = {
    type: "COMPONENT_TYPE_DRAG_DROP";
};

export type ComponentTypeDragEndMessage = {
    type: "COMPONENT_TYPE_DRAG_END";
};

export type ComponentTypeDragOverMessage = {
    type: "COMPONENT_TYPE_DRAG_OVER";
    clientX: number;
    clientY: number;
};

export type EditorComponent = {
    type: EditorComponentType;
    componentElement: HTMLElement;
};
