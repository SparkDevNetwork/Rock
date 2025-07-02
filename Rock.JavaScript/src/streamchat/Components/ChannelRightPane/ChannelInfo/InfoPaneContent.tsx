import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChannelListController } from '../../ChannelList/ChannelListControllerContext';
import { Avatar, useChannelMembershipState, useChannelStateContext, useChatContext, useChannelPreviewInfo } from 'stream-chat-react';
import { useChatConfig } from '../../Chat/ChatConfigContext';
import { DefaultChatChannelNamer } from '../../ChannelNamer/DefaultChannelNamer';
import { useModal } from '../../Modal/ModalContext';
import FavoriteChannelIcon from '../../ChannelHeader/FavoriteChannelIcon';

export const InfoPaneContent: React.FC = () => {
    const { client } = useChatContext();
    const { channel } = useChannelStateContext();
    const membershipState = useChannelMembershipState(channel);
    const { refresh } = useChannelListController();
    const { directMessageChannelTypeKey, refreshChat } = useChatConfig();

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

    interface ChannelInfoAction {
        key: string;
        label: string;
        icon: string;
        onClick: () => void;
        dangerous?: boolean;
    }

    const isMuted = channel.muteStatus().muted;

    const toggleMute = async () => {
        if (isMuted) {
            await channel.unmute();
        } else {
            await channel.mute();
        }

        refresh();
    };


    const canLeaveChannel = () => {
        const capabilities = channel?.data?.own_capabilities as string[] || [];
        const leavingAllowed = channel?.data?.rock_leaving_allowed === true;
        return capabilities.includes('leave-channel') && leavingAllowed;
    };

    const muteOrUnmuteAction = (): ChannelInfoAction => {
        return {
            key: 'mute-unmute',
            label: isMuted ? 'Unmute Group' : 'Mute Group',
            icon: isMuted ? 'fas fa-bell-slash' : 'fas fa-bell',
            onClick: toggleMute,
            dangerous: false,
        };
    }

    const { showModal, hideModal } = useModal();

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

    const leaveChannel = async () => {
        await channel.removeMembers([client.userID!]);
    }

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

    const channelActions = (): ChannelInfoAction[] => {
        const actions = [muteOrUnmuteAction()];
        if (canLeaveChannel()) {
            actions.push(leaveChannelAction());
        }
        return actions;
    }

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