import React from "react";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatViewStyle } from "../ChatViewStyle";
import { useChatContext } from "stream-chat-react";
import { Avatar } from "stream-chat-react";
import { profile } from "console";
export interface ChannelListHeaderProps {
    onNewMessage: () => void;
    onSearch?: () => void;
}

const ChannelListHeader: React.FC<ChannelListHeaderProps> = ({
    onNewMessage,
    onSearch,
}) => {
    const { chatViewStyle } = useChatConfig();

    const iconActions =
        chatViewStyle === ChatViewStyle.Community
            ? [
                {
                    key: "search",
                    iconClass: "fa fa-search",
                    action: onSearch,
                    ariaLabel: "Search",
                },
                {
                    key: "newMessage",
                    iconClass: "fa fa-pencil",
                    action: onNewMessage,
                    ariaLabel: "New DM",
                },
            ]
            : [
                {
                    key: "newMessage",
                    iconClass: "fa fa-pencil",
                    action: onNewMessage,
                    ariaLabel: "New DM",
                },
            ];

    const filteredActions = iconActions.filter(
        (item) => typeof item.action === "function"
    );

    const { client } = useChatContext();
    const profileImageSrc = client.user?.image
    const showProfileImage = chatViewStyle == ChatViewStyle.Community && profileImageSrc;

    return (
        <div className="rock__channel-list-header-container str-chat rocktheme-community">
            <div className="rock__channel-list-header">
                {showProfileImage && (
                    <Avatar image={profileImageSrc} name={client.user?.name || client.user?.id} className="rock__channel-list-header-avatar" />
                )}

                <div className="rock__channel-list-header__actions">
                    {filteredActions.map(({ key, iconClass, action, ariaLabel }) => (
                        <button
                            key={key}
                            onClick={action}
                            aria-label={ariaLabel}
                            className="rock__channel-list-header__icon-button"
                        >
                            <i className={iconClass}></i>
                        </button>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default ChannelListHeader;