import React from 'react';

/**
 * Props for the FavoriteChannelIcon component.
 *
 * @property {boolean} isPinned - Indicates if the channel is currently pinned (favorited) by the user.
 * @property {() => Promise<void>} onToggle - Async callback to toggle the pin state. Should handle pin/unpin logic and any side effects (e.g., UI refresh).
 * @property {boolean} [large] - Optional. If true, renders a larger version of the icon for emphasis or different UI contexts.
 */
interface FavoriteChannelIconProps {
    isPinned: boolean;
    onToggle: () => Promise<void>;
    large?: boolean;
}

/**
 * FavoriteChannelIcon
 *
 * Renders a star icon that visually represents the pin (favorite) state of a channel.
 *
 * - If `isPinned` is true, the icon appears filled (active); otherwise, it appears outlined (inactive).
 * - Clicking the icon triggers the `onToggle` callback, which should handle the pin/unpin logic asynchronously.
 * - The `large` prop optionally increases the icon size for use in different UI contexts.
 *
 * Accessibility & Usability:
 * - The icon has a tooltip (title) that changes based on the pin state ("Favorite" or "Unfavorite").
 * - Uses `stopPropagation` to prevent click events from bubbling up to parent elements (avoiding accidental UI triggers).
 *
 * @param {FavoriteChannelIconProps} props - The component props.
 * @returns {JSX.Element} The rendered favorite (star) icon.
 */
export const FavoriteChannelIcon: React.FC<FavoriteChannelIconProps> = ({ isPinned, onToggle, large }) => {
    return (
        <div
            // CSS classes: base class, optional large modifier, and active state if pinned
            className={`rock-channel-header-favorite${large ? '-lg' : ''} ${isPinned ? 'rock-channel-header-favorite--active' : ''}`}
            // Handle click: prevent event bubbling and call async toggle handler
            onClick={async (e) => {
                e.stopPropagation();
                await onToggle();
            }}
            // Tooltip for accessibility and user feedback
            title={isPinned ? 'Unfavorite' : 'Favorite'}>
            {/* FontAwesome star icon: filled if pinned, outlined if not */}
            <i className={isPinned ? 'fas fa-star' : 'far fa-star'} />
        </div>
    );
};

export default FavoriteChannelIcon;
