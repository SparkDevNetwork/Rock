import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChannelListController } from '../../ChannelList/ChannelListControllerContext';
import { Avatar, useChannelMembershipState, useChannelStateContext, useChatContext, useChannelPreviewInfo } from 'stream-chat-react';
import { useChatConfig } from '../../Chat/ChatConfigContext';
import { DefaultChatChannelNamer } from '../../ChannelNamer/DefaultChannelNamer';
import FavoriteChannelIcon from '../../ChannelHeader/FavoriteChannelIcon';

export const InfoPaneContent: React.FC = () => {
    const { client } = useChatContext();
    const { channel } = useChannelStateContext();
    const membershipState = useChannelMembershipState(channel);
    const { refresh } = useChannelListController();
    const { directMessageChannelTypeKey } = useChatConfig();

    const { displayImage, displayTitle } = useChannelPreviewInfo({
        channel,
    });

    const title = DefaultChatChannelNamer(channel, directMessageChannelTypeKey!, client.userID!) ?? undefined;

    // Use shared FavoriteChannelIcon component
    const isPinned = !!membershipState?.pinned_at;
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
        <div className="channel-info-pane-layout">
            <ChannelPaneHeader title="Channel Information" icon="fas fa-info-circle" />
            <div className="channel-info-header">
                <Avatar className="channel-info-avatar" image={displayImage} name={displayTitle} />

                <div className="channel-info-header-title-layout">
                    <h6 className="rock-channel-info-title">{title}</h6>
                    <FavoriteChannelIcon isPinned={isPinned} onToggle={handleToggle} large />
                </div>
            </div>
        </div>
    );
};