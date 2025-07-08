import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChannelListController } from '../../ChannelList/ChannelListControllerContext';
import { Avatar, useChannelMembershipState, useChannelStateContext, useChatContext, useChannelPreviewInfo } from 'stream-chat-react';
import { useChatConfig } from '../../Chat/ChatConfigContext';
import { DefaultChatChannelNamer } from '../../ChannelNamer/DefaultChannelNamer';
import { useModal } from '../../Modal/ModalContext';
import FavoriteChannelIcon from '../../ChannelHeader/FavoriteChannelIcon';

/**
 * InfoPaneContent
 *
 * Displays detailed information and actions for the currently selected chat channel.
 * Includes channel avatar, title, pin/favorite toggle, mute/unmute, and leave group actions.
 * Uses Stream Chat context and custom hooks for state and configuration.
 *
 * @returns {JSX.Element} The rendered channel info pane content.
 */
export const InfoPaneContent: React.FC = () => {
    // Access chat client and channel context
    const { client } = useChatContext();
    const { channel } = useChannelStateContext();
    // Membership state for the current user in this channel
    const membershipState = useChannelMembershipState(channel);
    // Refresh function for the channel list
    const { refresh } = useChannelListController();
    // Chat configuration (e.g., DM channel type key, refreshChat)
    const { directMessageChannelTypeKey, refreshChat } = useChatConfig();

    // Get display image and title for the channel
    const { displayImage, displayTitle } = useChannelPreviewInfo({
        channel,
    });

    // Compute the channel title using the default namer if not provided
    const title = DefaultChatChannelNamer(channel, directMessageChannelTypeKey!, client.userID!) ?? undefined;

    // Pin/favorite state and toggle handler
    const isPinned = !!membershipState?.pinned_at;
    /**
     * handleToggle
     *
     * Toggles the pin/favorite state of the channel for the current user.
     * Refreshes the channel list after the operation.
     * Handles errors by logging to the console.
     */
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

    /**
     * ChannelInfoAction
     *
     * Interface for channel action button definitions.
     * @property key - Unique key for the action
     * @property label - Button label
     * @property icon - FontAwesome icon class
     * @property onClick - Click handler function
     * @property dangerous - If true, styles as a dangerous/destructive action
     */
    interface ChannelInfoAction {
        key: string;
        label: string;
        icon: string;
        onClick: () => void;
        dangerous?: boolean;
    }

    // Whether the channel is currently muted for the user
    const isMuted = channel.muteStatus().muted;

    /**
     * toggleMute
     *
     * Mutes or unmutes the channel for the current user.
     * Refreshes the channel list after the operation.
     */
    const toggleMute = async () => {
        if (isMuted) {
            await channel.unmute();
        } else {
            await channel.mute();
        }
        refresh();
    };

    /**
     * canLeaveChannel
     *
     * Determines if the current user is allowed to leave the channel.
     * Checks both capability permissions and a custom flag on the channel data.
     *
     * @returns boolean - True if the user can leave the channel, false otherwise.
     */
    const canLeaveChannel = () => {
        const capabilities = channel?.data?.own_capabilities as string[] || [];
        const leavingAllowed = channel?.data?.rock_leaving_allowed === true;
        return capabilities.includes('leave-channel') && leavingAllowed;
    };

    /**
     * muteOrUnmuteAction
     *
     * Returns the action definition for muting or unmuting the channel.
     *
     * @returns {ChannelInfoAction}
     */
    const muteOrUnmuteAction = (): ChannelInfoAction => {
        return {
            key: 'mute-unmute',
            label: isMuted ? 'Unmute Group' : 'Mute Group',
            icon: isMuted ? 'fas fa-bell-slash' : 'fas fa-bell',
            onClick: toggleMute,
            dangerous: false,
        };
    }

    // Modal context for showing/hiding confirmation dialogs
    const { showModal, hideModal } = useModal();

    /**
     * leaveChannelAction
     *
     * Returns the action definition for leaving the channel, which opens a confirmation modal.
     *
     * @returns {ChannelInfoAction}
     */
    const leaveChannelAction = (): ChannelInfoAction => ({
        key: 'leave-channel',
        label: 'Leave Group',
        icon: 'fas fa-sign-out-alt',
        onClick: () => {
            showModal({
                title: 'Leave Group',
                content: leaveChannelModalContent,
            });
        },
        dangerous: true,
    });

    /**
     * leaveChannel
     *
     * Removes the current user from the channel's member list.
     * This is an async operation and may trigger UI updates elsewhere.
     *
     * Side effect: The user will no longer see the channel in their list.
     */
    const leaveChannel = async () => {
        await channel.removeMembers([client.userID!]);
    }

    /**
     * leaveChannelModalContent
     *
     * Modal content for confirming the leave group action.
     * Includes Cancel and Leave Group buttons.
     */
    const leaveChannelModalContent = (
        <div className="leave-channel-modal-content">
            <p className="leave-channel-modal-verification-text">Are you sure you want to leave this group? You will no longer receive messages or updates from this group.</p>

            <div className="leave-channel-modal-actions">
                <button
                    className="btn btn-secondary"
                    onClick={() => hideModal()}>
                    Cancel
                </button>

                <button
                    className="btn btn-danger"
                    onClick={async () => {
                        try {
                            await leaveChannel();
                            hideModal();
                            refreshChat();
                        } catch (error) {
                            console.error("Failed to leave channel", error);
                        }
                    }}>
                    Leave Group
                </button>
            </div>
        </div>
    );

    /**
     * channelActions
     *
     * Returns the list of available actions for the channel info pane.
     * Includes mute/unmute and leave group (if allowed).
     *
     * @returns {ChannelInfoAction[]}
     */
    const channelActions = (): ChannelInfoAction[] => {
        const actions = [muteOrUnmuteAction()];
        if (canLeaveChannel()) {
            actions.push(leaveChannelAction());
        }
        return actions;
    }

    return (
        <div className="channel-info-pane-layout">
            {/* Header with title and info icon */}
            <ChannelPaneHeader title="Channel Information" icon="fas fa-info-circle" />
            <div className="channel-info-header">
                {/* Channel avatar */}
                <Avatar className="channel-info-avatar" image={displayImage} name={displayTitle} />

                <div className="channel-info-header-title-layout">
                    {/* Channel title */}
                    <h6 className="rock-channel-info-title">{title}</h6>
                    {/* Pin/favorite icon */}
                    <FavoriteChannelIcon isPinned={isPinned} onToggle={handleToggle} large />
                </div>
            </div>

            {/*
                channel-info-actions
                -------------------
                Renders a list of action buttons for the channel (mute/unmute, leave group, etc.).
                Each button is styled and labeled according to its action definition.
                Dangerous actions (like leave group) are styled with a warning color.
            */}
            <div className="channel-info-actions">
                {channelActions().map(action => (
                    <button
                        key={action.key}
                        className={`channel-info-action-btn channel-info-action-btn--${action.key}` + (action.dangerous ? ' channel-info-action-btn--danger' : '')}
                        onClick={action.onClick}
                        type="button">
                        <i className={action.icon} style={{ marginRight: 8 }} />
                        {action.label}
                    </button>
                ))}
            </div>
        </div>
    );
};