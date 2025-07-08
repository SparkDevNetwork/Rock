import React from 'react';
import { Avatar as DefaultAvatar, ChannelHeaderProps, ChannelHeader as DefaultHeader, useChannelPreviewInfo, useChatContext, useChannelMembershipState, useChannelActionContext, useThreadsViewContext } from 'stream-chat-react';
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
 * - Uses a custom channel namer for the title.
 * - Handles both "Community" and "Conversational" chat view styles.
 * - Provides navigation buttons and a favorite (pin) toggle for the community style.
 *
 * @param props ChannelHeaderProps from stream-chat-react
 */
export const RockChannelHeader: React.FC = (props: ChannelHeaderProps) => {
    // Get the current channel and client from chat context
    const { channel, client } = useChatContext();
    // Get chat configuration (view style, DM channel type, etc.)
    const { chatViewStyle, directMessageChannelTypeKey } = useChatConfig();

    // If there's no channel yet, render the default header without a custom title
    if (!channel) {
        return <DefaultHeader />;
    }

    // Get the user's membership state for this channel (e.g., pinned status)
    const membershipState = useChannelMembershipState(channel);
    // Used to refresh the channel list after pin/unpin
    const { refresh } = useChannelListController();
    // Used to control which right pane is active (info, threads, etc.)
    const { setActivePane, activePane } = useChannelRightPane();

    const { closeThread } = useChannelActionContext();
    const { setActiveThread } = useThreadsViewContext();

    // Use our custom channel namer, falling back to undefined if it returns null
    const title = DefaultChatChannelNamer(channel, directMessageChannelTypeKey!, client.userID!) ?? undefined;

    /**
     * Returns the favorite (pin/unpin) icon/button for the channel header.
     * - Uses the shared FavoriteChannelIcon component.
     */
    const getFavoriteIcon = () => {
        if (!membershipState) {
            return null;
        }
        const isPinned = !!membershipState.pinned_at;
        const handleToggle = async () => {
            try {
                if (isPinned) {
                    await channel.unpin();
                } else {
                    await channel.pin();
                }
                refresh();
            } catch (error) {
                console.error("Failed to toggle pin state", error);
            }
        };
        return (
            <FavoriteChannelIcon isPinned={isPinned} onToggle={handleToggle} />
        );
    };

    const { setSelectedUser } = useChannelMemberListContext();

    // Navigation items for the right pane (info, threads, search, etc.)
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
                // If already active, close the thread
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
                if (activePane == 'members') {
                    setSelectedUser(null);
                    setActivePane(null);
                } else {
                    setActivePane('members');
                }
            }
        },
        // {
        //     key: 'more',
        //     iconClass: 'fas fa-ellipsis-h',
        //     title: 'More Options',
        //     onClick: () => setActivePane('more'),
        // },
    ];

    /**
     * Community-style channel header component.
     * - Shows avatar, title, favorite icon, and navigation buttons.
     */
    const communityComponent = () => {
        return (
            <div className="rock-channel-header">
                <div className="rock-channel-header-left">
                    {/* Channel avatar */}
                    <Avatar
                        className='str-chat__avatar--channel-header rock-channel-header-avatar'
                        groupChannelDisplayInfo={groupChannelDisplayInfo}
                        image={displayImage}
                        name={displayTitle}
                    />

                    {/* Channel title */}
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

    // Destructure props for possible overrides
    const {
        Avatar = DefaultAvatar,
        image: overrideImage,
        live,
        title: overrideTitle,
    } = props;

    // Get display image, title, and group info for the channel
    const { displayImage, displayTitle, groupChannelDisplayInfo } = useChannelPreviewInfo({
        channel,
        overrideImage,
        overrideTitle,
    });

    /**
     * Conversational-style channel header component.
     * - Just uses the default header with our custom title.
     */
    const conversationalComponent = () => {
        return (
            <DefaultHeader title={title} />
        )
    };

    // Render the appropriate header style based on chat view style
    return (
        <>
            {chatViewStyle == ChatViewStyle.Community ? communityComponent() : conversationalComponent()}
        </>
    );
};
