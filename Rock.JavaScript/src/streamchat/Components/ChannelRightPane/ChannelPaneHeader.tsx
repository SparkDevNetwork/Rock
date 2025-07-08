import React from "react";
import { useChannelRightPane } from "./ChannelRightPaneContext";
import { useChannelActionContext, useThreadsViewContext } from "stream-chat-react";
import { useChannelMemberListContext } from "../ChannelMemberList/ChannelMemberListContext";

interface ChannelPaneHeaderProps {
    title: string;
    icon?: string;
}

export const ChannelPaneHeader: React.FC<ChannelPaneHeaderProps> = ({ title, icon }) => {
    const { closePane } = useChannelRightPane();

    const { closeThread } = useChannelActionContext();
    const { setActiveThread } = useThreadsViewContext();
    const { setSelectedUser } = useChannelMemberListContext();

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
                {icon && (
                    <i className={icon} />
                )}
                <h4 className="rock-channel-right-pane-title">{title}</h4>
            </div>

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