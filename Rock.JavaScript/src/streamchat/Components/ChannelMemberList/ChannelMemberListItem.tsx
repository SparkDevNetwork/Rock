import React, { forwardRef } from 'react';
import { ChannelMemberResponse } from 'stream-chat';
import { Avatar } from 'stream-chat-react';

interface ChannelMemberListItemProps {
    member: ChannelMemberResponse;
    onClick?: () => void; // Optional: add a click handler prop
}

export const ChannelMemberListItem = forwardRef<HTMLLIElement, ChannelMemberListItemProps>(
    ({ member, onClick }, ref) => {
        if (!member || !member.user) {
            return null;
        }

        return (
            <li className="channel-member-list-item" ref={ref}>
                <button type="button" className="member-button" onClick={onClick}>
                    <div className="channel-member-list-item-layout">
                        <Avatar className="channel-member-avatar" user={member.user} image={member.user.image} />

                        <div className="channel-member-name">
                            {member.user.name || member.user.id}
                        </div>
                    </div>
                </button>
            </li>
        );
    }
);
ChannelMemberListItem.displayName = 'ChannelMemberListItem';