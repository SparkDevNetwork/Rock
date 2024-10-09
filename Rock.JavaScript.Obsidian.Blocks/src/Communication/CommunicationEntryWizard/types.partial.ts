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
    | "one-column-section"   // this is a special component type
    | "two-column-section"   // this is a special component type
    | "three-column-section" // this is a special component type
    | "title";

export type ComponentTypeDragStartMessage = {
    type: "COMPONENT_TYPE_DRAG_START";
    componentTypeName: EditorComponentTypeName;
};

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

export type AccordionManager = {
    register(key: string, isExpanded: Ref<boolean>): void;
};