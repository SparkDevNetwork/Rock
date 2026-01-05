import React, { forwardRef } from 'react';
import type { Channel } from 'stream-chat';
import { DefaultChatChannelNamer } from '../ChannelNamer/DefaultChannelNamer';
import { Avatar, useChannelPreviewInfo } from 'stream-chat-react';
import { useChatConfig } from '../Chat/ChatConfigContext';

interface DirectoryChannelResultItemProps {
    channel: Channel;
    onClick?: () => void;
}

function formatLastMessageAt(channel: Channel): string {
    const lastMessageAt = channel.state?.last_message_at || channel.data?.last_message_at;
    if (!lastMessageAt) return '';
    const date = typeof lastMessageAt === 'string' ? new Date(lastMessageAt) : lastMessageAt;
    if (isNaN(date.getTime())) return '';
    // Format as e.g. '7/03/2025 3:45 PM'
    return date.toLocaleString(undefined, {
        year: 'numeric',
        month: 'numeric',
        day: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    });
}

export const DirectoryChannelResultItem = forwardRef<HTMLLIElement, DirectoryChannelResultItemProps>(
    ({ channel, onClick }, ref) => {
        const { directMessageChannelTypeKey } = useChatConfig();
        const name = DefaultChatChannelNamer(channel, directMessageChannelTypeKey) || channel.data?.name || channel.id;

        const memberCount = channel.state?.members ? Object.keys(channel.state.members).length : (channel.data?.member_count || 0);
        const { displayImage } = useChannelPreviewInfo({ channel });
        const lastMessageAt = formatLastMessageAt(channel);

        return (
            <li
                className="directory-search-result-item"
                ref={ref}
                onClick={onClick}
            >
                <div className="directory-search-result-item-container directory-search-result-item-name-cell">
                    <div className="directory-channel-name-layout">
                        <Avatar image={displayImage} className="directory-channel-avatar" name={name} />
                        <span className="directory-channel-name">{name}</span>
                    </div>
                </div>
                <div className="directory-search-result-item-container directory-search-result-item-members-cell">
                    <span className="directory-channel-member-count">{memberCount}</span>
                </div>
                <div className="directory-search-result-item-container directory-search-result-item-last-message-cell">
                    <span className="directory-channel-last-message-at">{lastMessageAt}</span>
                </div>
            </li>
        );
    }
);
DirectoryChannelResultItem.displayName = 'DirectoryChannelResultItem';
