import React from "react";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatViewStyle } from "../../ChatViewStyle";
import { useChatContext } from "stream-chat-react";
import { Avatar } from "stream-chat-react";
import { useModal } from "../Modal/ModalContext";
import CreateChannelModal from "../CreateChannel/CreateChannelModal";
import { useDirectoryContext } from "../Directory/DirectoryContext";
import { useChannelRightPane } from "../ChannelRightPane/ChannelRightPaneContext";
import { useChannelMemberListContext } from "../ChannelMemberList/ChannelMemberListContext";

/**
 * Props for ChannelListHeader component.
 *
 * @property {() => void} [onSearch] - Optional callback for when the search icon is clicked.
 */
export interface ChannelListHeaderProps {
    onSearch?: () => void;
}

/**
 * ChannelListHeader
 *
 * Renders the header for the channel list, including profile avatar, search, and new message actions.
 *
 * Features:
 * - Shows the user's profile avatar (in Community view) with click-to-profile functionality.
 * - Displays action icons for search and new direct message, depending on chat view style.
 * - Integrates with modal and directory contexts for advanced UI behaviors.
 *
 * @param {ChannelListHeaderProps} props - The component props.
 * @returns {JSX.Element} The rendered channel list header.
 */
const ChannelListHeader: React.FC<ChannelListHeaderProps> = ({
    onSearch,
}) => {
    // Get the current chat view style (Community or Conversational)
    const { chatViewStyle } = useChatConfig();
    // Modal context for showing modals (e.g., new DM)
    const { showModal } = useModal();
    // Directory context for showing the directory UI
    const { showDirectory } = useDirectoryContext();

    /**
     * Handler for the "New Direct Message" action.
     * Opens a modal with the CreateChannelModal component.
     *
     * Side effects: Triggers a modal popup for DM creation.
     */
    const onNewMessage = () => {
        showModal({
            content: <CreateChannelModal />,
            title: "New Direct Message",
        })
    }

    /**
     * iconActions: Array of action button configs for the header.
     * - Community view: search and new message icons.
     * - Conversational view: only new message icon.
     *
     * Each action includes:
     * - key: Unique identifier
     * - iconClass: FontAwesome class for the icon
     * - action: Click handler function
     * - ariaLabel: Accessibility label
     */
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

    // Only include actions with valid (function) handlers
    const filteredActions = iconActions.filter(
        (item) => typeof item.action === "function"
    );

    // Get current user and channel from chat context
    const { client } = useChatContext();
    // User's profile image URL (if available)
    const profileImageSrc = client.user?.image
    // Show profile image only in Community view and if image exists
    const showProfileImage = chatViewStyle == ChatViewStyle.Community && profileImageSrc;
    // Right pane context for activating member pane
    const { setActivePane } = useChannelRightPane();
    // Member list context for selecting a user
    const { setSelectedUser } = useChannelMemberListContext();

    /**
     * Handler for clicking the user's profile avatar.
     *
     * Side effects:
     * - Sets the selected user in the member list context.
     * - Activates the 'members' pane in the right pane context.
     *
     * Edge case: If user is not defined, does nothing.
     */
    const showUserProfile = () => {
        const user = client.user;
        if (user) {
            setSelectedUser(user);
            setActivePane('members');
        } else {
            // No user found; do nothing (could log or handle as needed)
        }
    }

    const rootClassName = chatViewStyle == ChatViewStyle.Community ? "rocktheme-community" : "rocktheme-conversational"

    return (
        <div className={`${rootClassName} rock-channel-list-header-container str-chat`}>
            <div className="rock-channel-list-header">
                {/* User profile avatar (Community view only) */}
                {showProfileImage && (
                    <Avatar image={profileImageSrc} name={client.user?.name || client.user?.id} className="rock-channel-list-header-avatar" onClick={showUserProfile} />
                )}

                {/* Action icons (search, new message, etc.) */}
                <div className="rock-channel-list-header-actions">
                    {filteredActions.map(({ key, iconClass, action, ariaLabel }) => (
                        <button
                            key={key}
                            onClick={action}
                            aria-label={ariaLabel}
                            className={
                                "rock-channel-list-header-icon-button" +
                                (key === "search" && showDirectory ? " active" : "")
                            }>
                            <i className={iconClass}></i>
                        </button>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default ChannelListHeader;