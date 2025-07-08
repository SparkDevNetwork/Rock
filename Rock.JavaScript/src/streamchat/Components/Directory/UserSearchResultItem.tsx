import React from 'react';
import type { UserResponse } from 'stream-chat';
import { Avatar } from 'stream-chat-react';

interface UserSearchResultItemProps {
    user: UserResponse;
}

function formatLastActiveAt(user: UserResponse): string {
    const lastActive = user.last_active || user.updated_at || user.created_at;
    if (!lastActive) return '';
    const date = typeof lastActive === 'string' ? new Date(lastActive) : lastActive;
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

export const UserSearchResultItem: React.FC<UserSearchResultItemProps> = ({ user }) => (
    <li className="directory-search-result-item">
        <div className="directory-search-result-item-container directory-search-result-item-name-cell">
            <div className="directory-channel-name-layout">
                <Avatar image={user.image} name={user.name || user.id} className="directory-channel-avatar" />
                <span className="directory-channel-name">{user.name || user.id}</span>
            </div>
        </div>
        <div className="directory-search-result-item-container directory-search-result-item-members-cell">
            <span>{formatLastActiveAt(user)}</span>
        </div>
    </li>
);
