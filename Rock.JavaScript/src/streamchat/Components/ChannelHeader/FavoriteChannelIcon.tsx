import React from 'react';
import type { Channel } from 'stream-chat';
import { useChannelListController } from '../ChannelList/ChannelListControllerContext';
import { useChannelMembershipState, useChatContext } from 'stream-chat-react';

/**
 * Props for the FavoriteChannelIcon component.
 *
 * @property {boolean} isPinned - Indicates if the channel is currently pinned (favorited) by the user.
 * @property {Channel} channel - The Stream Chat channel instance.
 * @property {() => void} refresh - Callback to refresh the channel list after pin/unpin.
 * @property {boolean} [large] - Optional. If true, renders a larger version of the icon for emphasis or different UI contexts.
 */
interface FavoriteChannelIconProps {
    channel: Channel;
    large?: boolean;
}

/**
 * FavoriteChannelIcon
 *
 * Renders a star icon that visually represents the pin (favorite) state of a channel.
 * Handles pin/unpin logic internally.
 *
 * @param {FavoriteChannelIconProps} props - The component props.
 * @returns {JSX.Element} The rendered favorite (star) icon.
 */
export const FavoriteChannelIcon: React.FC<FavoriteChannelIconProps> = ({ channel, large }) => {

    const { refresh } = useChannelListController();
    const { client } = useChatContext();
    const membershipState = useChannelMembershipState(channel);
    const isPinned = !!membershipState?.pinned_at;

    const handleToggle = async (e: React.MouseEvent) => {
        e.stopPropagation();
        try {
            const shouldAddUser = !membershipState || !membershipState.user_id;

            // If the user is not a member, join the channel first
            if (shouldAddUser) {
                await channel.addMembers([client.userID!]);
            }

            console.log(membershipState);
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
        <div
            className={`rock-channel-header-favorite${large ? '-lg' : ''} ${isPinned ? 'rock-channel-header-favorite--active' : ''}`}
            onClick={handleToggle}
            title={isPinned ? 'Unfavorite' : 'Favorite'}>
            <i className={isPinned ? 'fas fa-star' : 'far fa-star'} />
        </div>
    );
};

export default FavoriteChannelIcon;
