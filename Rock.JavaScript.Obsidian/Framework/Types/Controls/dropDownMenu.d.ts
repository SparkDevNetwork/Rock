/** The type to use for coloring and styling of the action. */
export type MenuActionType = "default" | "danger";

/** A function that will be called in response to an action. */
export type MenuActionCallback = (event: Event) => void | Promise<void>;

/** Defines a single action related to a Panel control. */
export type MenuAction = {
    /**
     * The title of the action, this should be a very short (one or two words)
     * description of the action that will be performed, such as "Delete".
     */
    title?: string;

    /**
     * The CSS class for the icon used when displaying this action.
     */
    iconCssClass?: string;

    /**
     * Additional CSS classes to apply to the action item.
     */
    actionCssClass?: string;

    /** The type of action for styling. */
    type: MenuActionType;

    /** The callback function that will handle the action. */
    handler?: MenuActionCallback;

    /** If true then the action will be disabled and not respond to clicks. */
    disabled?: boolean;
};
