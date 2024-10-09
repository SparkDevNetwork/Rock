import { Ref } from "vue";

export type MenuItem = {
    title: string;
    value: string;
    iconCssClass: string; // TODO JMH This may need to support custom SVGs.
};

export type EditorComponentTypeName =
    "video"
    | "button"
    | "text"
    | "divider"
    | "message"
    | "image"
    | "code"
    | "rsvp"
    | "section"
    | "one-column-section"
    | "two-column-section"
    | "three-column-section"
    | "title";

export type ComponentTypeDragLeaveMessage = {
    type: "COMPONENT_TYPE_DRAG_LEAVE";
};

export type ComponentTypeDragDropMessage = {
    type: "COMPONENT_TYPE_DRAG_DROP";
    componentTypeName: EditorComponentTypeName;
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
    componentTypeName: EditorComponentTypeName;
    componentElement: HTMLElement;
};

export type AccordionManager = {
    register(key: string, isExpanded: Ref<boolean>): void;
};