import React from "react";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatViewStyle } from "../../ChatViewStyle";
import { useChatContext } from "stream-chat-react";
import { Avatar } from "stream-chat-react";
import { useModal } from "../Modal/ModalContext";
import CreateChannelModal from "../CreateChannel/CreateChannelModal";
export interface ChannelListHeaderProps {
    onSearch?: () => void;
}

const ChannelListHeader: React.FC<ChannelListHeaderProps> = ({
    onSearch,
}) => {
    const { chatViewStyle } = useChatConfig();
    const { showModal } = useModal();

    const onNewMessage = () => {
        showModal({
            content: <CreateChannelModal />,
            title: "New Direct Message",
        })
    }
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
        <div className="rock-channel-list-header-container str-chat rocktheme-community">
            <div className="rock-channel-list-header">
                {showProfileImage && (
                    <Avatar image={profileImageSrc} name={client.user?.name || client.user?.id} className="rock-channel-list-header-avatar" />
                )}

                <div className="rock-channel-list-header-actions">
                    {filteredActions.map(({ key, iconClass, action, ariaLabel }) => (
                        <button
                            key={key}
                            onClick={action}
                            aria-label={ariaLabel}
                            className="rock-channel-list-header-icon-button"
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