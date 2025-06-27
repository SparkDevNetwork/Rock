import React from 'react';
import { Avatar as DefaultAvatar, ChannelHeaderProps, ChannelHeader as DefaultHeader, useChannelPreviewInfo, useChatContext, useChannelMembershipState } from 'stream-chat-react';
import { DefaultChatChannelNamer } from '../ChannelNamer/DefaultChannelNamer';
import { useChatConfig } from '../Chat/ChatConfigContext';
import { ChatViewStyle } from '../ChatViewStyle';
import { useChannelListController } from '../ChannelList/ChannelListControllerContext';
import { useChannelRightPane } from '../ChannelRightPane/ChannelRightPaneContext';

/**
 * Overrides the built-in ChannelHeader to use the DefaultChatChannelNamer
 * for displayTitle, guarding against a null channel.
 */
export const RockChannelHeader: React.FC = (props: ChannelHeaderProps) => {
    const { channel, client } = useChatContext();
    const { chatViewStyle, directMessageChannelTypeKey } = useChatConfig();

    // If there's no channel yet, render the default header without a custom title
    if (!channel) {
        return <DefaultHeader />;
    }

    const membershipState = useChannelMembershipState(channel);
    const { refresh } = useChannelListController();
    const { setActivePane, activePane } = useChannelRightPane();

    // Use our namer, falling back to undefined if it returns null
    const title = DefaultChatChannelNamer(channel, directMessageChannelTypeKey!, client.userID!) ?? undefined;

    const getFavoriteIcon = () => {
        if (!membershipState) {
            return null;
        }

        const isPinned = !!membershipState.pinned_at;

        return (
            <div
                className={`rock-channel-header-favorite ${isPinned ? 'rock-channel-header-favorite--active' : ''}`}
                onClick={async () => {
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
                }}
                title={isPinned ? "Unfavorite" : "Favorite"}
            >
                <i className={isPinned ? "fas fa-star" : "far fa-star"} />
            </div>
        );
    };

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
            onClick: () => setActivePane('threads'),
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
            onClick: () => setActivePane('members'),
        },
        {
            key: 'more',
            iconClass: 'fas fa-ellipsis-h',
            title: 'More Options',
            onClick: () => setActivePane('more'),
        },
    ];

    const communityComponent = () => {
        return (
            <div className="rock-channel-header">
                <div className="rock-channel-header-left">
                    <Avatar
                        className='str-chat__avatar--channel-header rock-channel-header-avatar'
                        groupChannelDisplayInfo={groupChannelDisplayInfo}
                        image={displayImage}
                        name={displayTitle}
                    />

                    <h6 className="rock-channel-header-title">{title}</h6>
                    {getFavoriteIcon()}
                </div>

                <div className="rock-channel-header-right">
                    <div className="rock-channel-header-right">
                        {navItems.map(({ key, iconClass, title, onClick }) => (
                            <button
                                key={key}
                                onClick={() => {
                                    // Toggle off if already active
                                    if (activePane === key.replace('channelInfo', 'info')) {
                                        setActivePane(null);
                                    } else {
                                        onClick();
                                    }
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

    const {
        Avatar = DefaultAvatar,
        image: overrideImage,
        live,
        title: overrideTitle,
    } = props;

    const { displayImage, displayTitle, groupChannelDisplayInfo } = useChannelPreviewInfo({
        channel,
        overrideImage,
        overrideTitle,
    });

    const conversationalComponent = () => {
        return (
            <DefaultHeader title={title} />
        )
    };

    return (
        <>
            {chatViewStyle == ChatViewStyle.Community ? communityComponent() : conversationalComponent()}
        </>
    );
};
