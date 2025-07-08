import React from "react";
import { useChannelRightPane } from "./ChannelRightPaneContext";
import { useChannelActionContext, useThreadsViewContext } from "stream-chat-react";
import { useChannelMemberListContext } from "../ChannelMemberList/ChannelMemberListContext";

/**
 * ChannelPaneHeaderProps
 *
 * Props for the ChannelPaneHeader component.
 * @property title - The title to display in the header
 * @property icon - Optional FontAwesome icon class for the header
 */
interface ChannelPaneHeaderProps {
    title: string;
    icon?: string;
}

/**
 * ChannelPaneHeader
 *
 * Renders the header for the right pane in the chat UI, including a title, optional icon, and a close button.
 * Handles closing the pane and clearing any thread or member selection context.
 *
 * - Uses useChannelRightPane to close the pane.
 * - Uses useChannelActionContext and useThreadsViewContext to clear thread context.
 * - Uses useChannelMemberListContext to clear selected user context.
 *
 * @param title - The title to display in the header
 * @param icon - Optional FontAwesome icon class
 * @returns {JSX.Element} The rendered header
 */
export const ChannelPaneHeader: React.FC<ChannelPaneHeaderProps> = ({ title, icon }) => {
    // Access right pane close function
    const { closePane } = useChannelRightPane();
    // Access thread close and setActiveThread functions
    const { closeThread } = useChannelActionContext();
    const { setActiveThread } = useThreadsViewContext();
    // Access member list context setter
    const { setSelectedUser } = useChannelMemberListContext();

    /**
     * close
     *
     * Handles closing the right pane. Also clears any thread context and selected user context.
     * Called when the close button is clicked.
     */
    const close = () => {
        closePane();
        // Clear any thread context
        if (closeThread) {
            closeThread();
            setActiveThread(undefined);
        }
        // Clear selected user in member list context
        setSelectedUser(null);
    }

    return (
        <div className="rock-channel-right-pane-header">
            <div className="rock-channel-right-pane-header-title-layout">
                {/* Optional icon for the header */}
                {icon && (
                    <i className={icon} />
                )}
                {/* Header title */}
                <h4 className="rock-channel-right-pane-title">{title}</h4>
            </div>

            {/* Close button for the right pane */}
            <button
                className="rock-channel-right-pane-close"
                onClick={close}
                aria-label="Close Pane"
                type="button">
                <i className="fas fa-times" />
            </button>
        </div>
    );
};