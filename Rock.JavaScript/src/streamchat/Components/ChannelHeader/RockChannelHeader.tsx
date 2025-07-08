import React from 'react';
import {
    Avatar as DefaultAvatar,
    ChannelHeaderProps,
    ChannelHeader as DefaultHeader,
    useChannelPreviewInfo,
    useChatContext,
    useChannelMembershipState,
    useChannelActionContext,
    useThreadsViewContext
} from 'stream-chat-react';
import { DefaultChatChannelNamer } from '../ChannelNamer/DefaultChannelNamer';
import { useChatConfig } from '../Chat/ChatConfigContext';
import { ChatViewStyle } from '../../ChatViewStyle';
import { useChannelListController } from '../ChannelList/ChannelListControllerContext';
import { useChannelRightPane } from '../ChannelRightPane/ChannelRightPaneContext';
import FavoriteChannelIcon from './FavoriteChannelIcon';
import { useChannelMemberListContext } from '../ChannelMemberList/ChannelMemberListContext';

/**
 * RockChannelHeader
 *
 * Custom Channel Header component that overrides the built-in Stream Chat header.
 *
 * Features:
 * - Uses a custom channel namer for the title.
 * - Handles both "Community" and "Conversational" chat view styles.
 * - Provides navigation buttons and a favorite (pin) toggle for the community style.
 *
 * @component
 * @param {ChannelHeaderProps} props - Props passed from stream-chat-react's ChannelHeader
 * @returns {JSX.Element} The rendered channel header component
 */
export const RockChannelHeader: React.FC = (props: ChannelHeaderProps) => {
    // Get the current channel and client from chat context
    // channel: The current active channel object
    // client: The Stream Chat client instance
    const { channel, client } = useChatContext();

    // Get chat configuration (view style, DM channel type, etc.)
    // chatViewStyle: Enum indicating the current chat UI style (Community or Conversational)
    // directMessageChannelTypeKey: String key for identifying DM channels
    const { chatViewStyle, directMessageChannelTypeKey } = useChatConfig();

    // If there's no channel yet, render the default header without a custom title
    // Edge case: This can occur if the chat is still loading or no channel is selected
    if (!channel) {
        return <DefaultHeader />;
    }

    // Get the user's membership state for this channel (e.g., pinned status)
    // membershipState: Contains info like whether the channel is pinned for the user
    const membershipState = useChannelMembershipState(channel);

    // Used to refresh the channel list after pin/unpin
    // refresh: Function to reload the channel list UI
    const { refresh } = useChannelListController();

    // Used to control which right pane is active (info, threads, etc.)
    // setActivePane: Function to set the active right pane
    // activePane: The currently active right pane key
    const { setActivePane, activePane } = useChannelRightPane();

    // Context for thread actions (closing threads, etc.)
    // closeThread: Function to close the currently open thread
    const { closeThread } = useChannelActionContext();
    // setActiveThread: Function to set the active thread (or clear it)
    const { setActiveThread } = useThreadsViewContext();

    // Use our custom channel namer, falling back to undefined if it returns null
    // title: The display name for the channel, as determined by custom logic
    const title = DefaultChatChannelNamer(
        channel,
        directMessageChannelTypeKey!,
        client.userID!
    ) ?? undefined;

    /**
     * Returns the favorite (pin/unpin) icon/button for the channel header.
     * - Uses the shared FavoriteChannelIcon component.
     * - Handles toggling the pin state for the current user.
     *
     * @returns {JSX.Element|null} The favorite icon/button, or null if membership state is unavailable
     *
     * Side effects: Calls channel.pin() or channel.unpin(), then refreshes the channel list.
     * Edge cases: If membershipState is undefined, returns null (e.g., before membership loads)
     */
    const getFavoriteIcon = () => {
        if (!membershipState) {
            // Membership info not loaded yet; don't render the icon
            return null;
        }
        // isPinned: Boolean indicating if the channel is currently pinned for the user
        const isPinned = !!membershipState.pinned_at;
        // handleToggle: Async handler to pin or unpin the channel
        const handleToggle = async () => {
            try {
                if (isPinned) {
                    // Unpin the channel for the user
                    await channel.unpin();
                } else {
                    // Pin the channel for the user
                    await channel.pin();
                }
                // Refresh the channel list to reflect the new pin state
                refresh();
            } catch (error) {
                // Log errors for debugging
                console.error("Failed to toggle pin state", error);
            }
        };
        // Render the FavoriteChannelIcon with the correct state and handler
        return (
            <FavoriteChannelIcon isPinned={isPinned} onToggle={handleToggle} />
        );
    };

    // setSelectedUser: Function to set the selected user in the member list (for details view)
    const { setSelectedUser } = useChannelMemberListContext();

    /**
     * navItems: Array of navigation button configs for the right pane
     * Each item contains:
     * - key: Unique identifier for the pane
     * - iconClass: FontAwesome class for the icon
     * - title: Tooltip and aria-label for accessibility
     * - onClick: Handler to activate the pane or perform related actions
     *
     * Edge cases: Some buttons (e.g., threads, members) have toggle/close logic if already active
     */
    const navItems = [
        {
            key: 'channelInfo',
            iconClass: 'fas fa-info-circle',
            title: 'Channel Information',
            onClick: () => setActivePane('info'),
        },
        {
            key: 'threads',
            iconClass: 'fas fa-comments',
            title: 'Threads',
            onClick: () => {
                // If already active, close the thread and clear selection
                if (activePane == 'threads') {
                    closeThread();
                    setActiveThread(undefined);
                    setActivePane(null);
                } else {
                    setActivePane('threads');
                }
            },
        },
        {
            key: 'search',
            iconClass: 'fas fa-search',
            title: 'Search',
            onClick: () => setActivePane('search'),
        },
        {
            key: 'mentions',
            iconClass: 'fas fa-at',
            title: 'Mentions',
            onClick: () => setActivePane('mentions'),
        },
        {
            key: 'members',
            iconClass: 'fas fa-users',
            title: 'Members',
            onClick: () => {
                // If already active, clear selected user and close pane
                if (activePane == 'members') {
                    setSelectedUser(null);
                    setActivePane(null);
                } else {
                    setActivePane('members');
                }
            }
        },
        // Additional nav items can be added here (e.g., more options)
        // {
        //     key: 'more',
        //     iconClass: 'fas fa-ellipsis-h',
        //     title: 'More Options',
        //     onClick: () => setActivePane('more'),
        // },
    ];

    /**
     * Renders the community-style channel header.
     *
     * Features:
     * - Shows channel avatar, title, favorite icon, and navigation buttons.
     * - Navigation buttons control the right pane (info, threads, search, mentions, members).
     *
     * @returns {JSX.Element} The community-style header
     */
    const communityComponent = () => {
        return (
            <div className="rock-channel-header">
                <div className="rock-channel-header-left">
                    {/* Channel avatar: Shows group/channel image and name */}
                    <Avatar
                        className='str-chat__avatar--channel-header rock-channel-header-avatar'
                        groupChannelDisplayInfo={groupChannelDisplayInfo}
                        image={displayImage}
                        name={displayTitle}
                    />

                    {/* Channel title: Custom or fallback name */}
                    <h6 className="rock-channel-header-title">{title}</h6>
                    {/* Favorite (pin/unpin) icon */}
                    {getFavoriteIcon()}
                </div>

                <div className="rock-channel-header-right">
                    {/* Navigation buttons for right pane */}
                    <div className="rock-channel-header-right">
                        {navItems.map(({ key, iconClass, title, onClick }) => (
                            <button
                                key={key}
                                onClick={() => {
                                    onClick();
                                }}
                                title={title}
                                aria-label={title}
                                // Highlight the button if its pane is active
                                className={`rock-channel-header-nav-button${activePane === key.replace('channelInfo', 'info') ? ' rock-channel-header-nav-button--active' : ''}`}
                            >
                                <i className={iconClass}></i>
                            </button>
                        ))}
                    </div>
                </div>
            </div>
        )
    };

    // Destructure props for possible overrides from parent
    // Avatar: Custom avatar component (optional)
    // image: Optional override for channel image
    // live: Unused, but passed through for compatibility
    // title: Optional override for channel title
    const {
        Avatar = DefaultAvatar,
        image: overrideImage,
        live,
        title: overrideTitle,
    } = props;

    // Get display image, title, and group info for the channel
    // displayImage: URL for the channel avatar
    // displayTitle: Name to display for the channel
    // groupChannelDisplayInfo: Additional info for group channels
    const { displayImage, displayTitle, groupChannelDisplayInfo } = useChannelPreviewInfo({
        channel,
        overrideImage,
        overrideTitle,
    });

    /**
     * Renders the conversational-style channel header.
     *
     * Features:
     * - Uses the default Stream header, but with our custom title.
     *
     * @returns {JSX.Element} The conversational-style header
     */
    const conversationalComponent = () => {
        return (
            <DefaultHeader title={title} />
        )
    };

    // Render the appropriate header style based on chat view style
    // If chatViewStyle is Community, use the custom community header; otherwise, use the default conversational header
    return (
        <>
            {chatViewStyle == ChatViewStyle.Community ? communityComponent() : conversationalComponent()}
        </>
    );
};
