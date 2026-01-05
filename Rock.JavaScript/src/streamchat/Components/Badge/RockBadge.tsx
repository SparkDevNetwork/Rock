/**
 * RockBadge Component
 *
 * Displays a simple badge element with customizable text and optional styling.
 * This component is useful for indicating statuses, labels, or metadata in a compact form.
 *
 * @component
 * @example
 * <RockBadge
 *   badgeText="Admin"
 *   foregroundColor="#fff"
 *   backgroundColor="#007bff"
 * />
 *
 * @param {string} badgeText - The text displayed inside the badge.
 * @param {string} [foregroundColor] - Optional text color (CSS color value).
 * @param {string} [backgroundColor] - Optional background color (CSS color value).
 *
 * @returns {JSX.Element} A styled badge element.
 */
import React from 'react';

interface RockBadgeProps {
    badgeText: string;
    foregroundColor?: string;
    backgroundColor?: string;
}

const RockBadge: React.FC<RockBadgeProps> = ({
    badgeText,
    foregroundColor,
    backgroundColor
}) => {
    const style = {
        color: foregroundColor || '#000',
        backgroundColor: backgroundColor || '#f0f0f0'
    };

    return (
        <div className="rock-badge" style={style}>
            {badgeText}
        </div>
    );
};

export default RockBadge;