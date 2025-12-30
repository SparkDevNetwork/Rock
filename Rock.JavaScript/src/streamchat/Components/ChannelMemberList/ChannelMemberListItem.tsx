import React, { forwardRef } from 'react';
import { ChannelMemberResponse } from 'stream-chat';
import { Avatar } from 'stream-chat-react';

/**
 * ChannelMemberListItemProps
 *
 * Props for the ChannelMemberListItem component.
 * @property member - The channel member to display
 * @property onClick - Optional click handler for selecting the member
 */
interface ChannelMemberListItemProps {
    member: ChannelMemberResponse;
    onClick?: () => void; // Optional: add a click handler prop
}

/**
 * ChannelMemberListItem
 *
 * Renders a single channel member as a list item, including avatar and name.
 * Supports keyboard and mouse selection via the onClick handler.
 *
 * - Uses Stream Chat's Avatar component for user image.
 * - Falls back to user ID if name is not available.
 * - Uses forwardRef to support focus and infinite scroll (for last item detection).
 * - Returns null if member or user is missing (should not happen in normal usage).
 *
 * @param member - The channel member to display
 * @param onClick - Optional click handler for selection
 * @param ref - Ref for the list item (used for infinite scroll)
 * @returns {JSX.Element | null} The rendered list item or null if invalid
 */
export const ChannelMemberListItem = forwardRef<HTMLLIElement, ChannelMemberListItemProps>(
    ({ member, onClick }, ref) => {
        if (!member || !member.user) {
            // Defensive: Should not render if member or user is missing
            return null;
        }

        return (
            <li className="channel-member-list-item" ref={ref}>
                <button type="button" className="member-button" onClick={onClick}>
                    <div className="channel-member-list-item-layout">
                        {/* User avatar (image or fallback) */}
                        <Avatar className="channel-member-avatar" user={member.user} image={member.user.image} />

                        {/* User name or ID */}
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