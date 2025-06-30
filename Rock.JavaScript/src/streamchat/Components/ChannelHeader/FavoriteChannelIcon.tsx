import React from 'react';

interface FavoriteChannelIconProps {
    isPinned: boolean;
    onToggle: () => Promise<void>;
    large?: boolean;
}

/**
 * FavoriteChannelIcon
 *
 * Renders a star icon for pinning/unpinning a channel.
 * - isPinned: whether the channel is currently pinned
 * - onToggle: async function to call when toggling pin state
 * - large: optional, renders a larger icon if true
 */
export const FavoriteChannelIcon: React.FC<FavoriteChannelIconProps> = ({ isPinned, onToggle, large }) => {
    return (
        <div
            className={`rock-channel-header-favorite${large ? '-lg' : ''} ${isPinned ? 'rock-channel-header-favorite--active' : ''}`}
            onClick={async (e) => {
                e.stopPropagation();
                await onToggle();
            }}
            title={isPinned ? 'Unfavorite' : 'Favorite'}
        >
            <i className={isPinned ? 'fas fa-star' : 'far fa-star'} />
        </div>
    );
};

export default FavoriteChannelIcon;
