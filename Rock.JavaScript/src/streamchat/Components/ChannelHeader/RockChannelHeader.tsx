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
 * Handles both "Community" and "Conversational" chat view styles.
 */
export const RockChannelHeader: React.FC<ChannelHeaderProps> = (props) => {
    // Context and config
    const { channel, client } = useChatContext();
    const { chatViewStyle, directMessageChannelTypeKey } = useChatConfig();

    // If there's no channel yet, render the default header without a custom title
    if (!channel) {
        return <DefaultHeader />;
    }

    const membershipState = useChannelMembershipState(channel);
    const { refresh } = useChannelListController();
    const { setActivePane, activePane } = useChannelRightPane();
    const { closeThread } = useChannelActionContext();
    const { setActiveThread } = useThreadsViewContext();
    const { setSelectedUser } = useChannelMemberListContext();

    // Destructure possible overrides from parent
    const {
        Avatar = DefaultAvatar,
        image: overrideImage,
        live,
        title: overrideTitle,
    } = props;

    // Get display info for the channel
    const { displayImage, displayTitle, groupChannelDisplayInfo } = useChannelPreviewInfo({
        channel,
        overrideImage,
        overrideTitle,
    });

    // Compute the title using custom channel namer
    const title = DefaultChatChannelNamer(
        channel,
        directMessageChannelTypeKey!,
        client.userID!
    ) ?? undefined;

    // Favorite (pin/unpin) icon logic
    const getFavoriteIcon = () => {
        if (!membershipState) return null;
        const isPinned = !!membershipState.pinned_at;
        const handleToggle = async () => {
            try {
                if (!channel) return;
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
        return <FavoriteChannelIcon isPinned={isPinned} onToggle={handleToggle} />;
    };

    // Navigation items for the right pane
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
    ];

    // Community style header
    const communityComponent = () => (
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
                            onClick={onClick}
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
    );

    // Conversational style header
    const conversationalComponent = () => (
        <DefaultHeader title={title} />
    );

    // Render
    return chatViewStyle === ChatViewStyle.Community
        ? communityComponent()
        : conversationalComponent();
};
