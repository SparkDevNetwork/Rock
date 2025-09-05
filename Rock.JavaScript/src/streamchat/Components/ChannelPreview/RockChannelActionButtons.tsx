/**
 * RockChannelPreviewActionButtons Component
 *
 * Renders a set of contextual action buttons for a chat channel preview.
 * Includes support for muting/unmuting, leaving, and hiding channels. Uses Stream's Dialog system for interaction.
 *
 * @template SCG - StreamChat ExtendableGenerics for typed support.
 *
 * @param {ChannelPreviewActionButtonsProps<SCG>} props - Props for the action buttons component.
 * @param {Channel<SCG>} props.channel - The Stream channel this preview belongs to.
 *
 * @returns {JSX.Element} Action buttons component for a channel preview.
 *
 * Usage:
 *   <RockChannelPreviewActionButtons channel={channel} />
 *
 * This component is intended to be used within a channel preview UI, providing users with quick access to channel actions
 * such as muting, leaving, or hiding the channel. It leverages Stream's dialog system for accessibility and focus management.
 */
import React, { useRef } from 'react';
import type { Channel } from 'stream-chat';
import {
    useChatContext, // Provides access to the Stream chat client and user context
    useTranslationContext, // Provides translation function for i18n
    DialogAnchor, // Anchors a dialog to a reference element
    useDialog, // Hook to open/close/toggle a dialog by ID
    useDialogIsOpen, // Hook to check if a dialog is open by ID
} from 'stream-chat-react';
import 'stream-chat-react/dist/css/v2/index.css';
import { VerticalEllipsisIcon } from './Icons';

/**
 * Props for RockChannelPreviewActionButtons
 * @property channel - The Stream Chat channel for which to render action buttons
 */
export type ChannelPreviewActionButtonsProps = {
    channel: Channel;
};

/**
 * RockChannelPreviewActionButtons
 *
 * Renders contextual action buttons (mute/unmute, leave, hide) for a chat channel preview.
 * Handles dialog open/close state, accessibility, and action logic.
 *
 * @param channel - The Stream Chat channel object
 * @returns JSX.Element
 */
export function RockChannelPreviewActionButtons({
    channel,
}: ChannelPreviewActionButtonsProps) {
    // Translation function for i18n
    const { t } = useTranslationContext();
    // Access chat client and current user
    const { client } = useChatContext();

    // Unique dialog ID for this channel's action menu
    const dialogId = `channel-actions-${channel.id}`;
    // Dialog control object (open, close, toggle)
    const dialog = useDialog({ id: dialogId });
    // Whether the dialog is currently open
    const dialogIsOpen = useDialogIsOpen(dialogId);
    // Ref for the action button (for focus and anchoring)
    const buttonRef = useRef<HTMLButtonElement | null>(null);

    // Whether the channel is currently muted for the user
    const isMuted = channel.muteStatus().muted;

    /**
     * toggleMute
     *
     * Mutes or unmutes the channel for the current user.
     * Closes the dialog after the action completes.
     * Handles async errors via Stream's SDK (not surfaced here).
     */
    const toggleMute = async () => {
        if (isMuted) {
            await channel.unmute();
        } else {
            await channel.mute();
        }
        // Close the dialog after muting/unmuting
        dialog?.close();
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
        // Capabilities are provided by Stream's permissions system
        const capabilities = channel?.data?.own_capabilities as string[] || [];
        // Custom flag to allow/disallow leaving
        const leavingAllowed = channel?.data?.rock_leaving_allowed === true;
        return capabilities.includes('leave-channel') && leavingAllowed;
    };

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

    return (
        <div className='str-chat__channel-preview__action-buttons'>
            {/* DialogAnchor attaches the action menu to the button, manages focus and accessibility */}
            <DialogAnchor
                id={dialogId}
                placement='top-start'
                referenceElement={buttonRef.current}
                trapFocus>
                <div
                    className='str-chat__message-actions-box str-chat__message-actions-box--open'
                    data-testid='message-actions-box'>
                    <div
                        aria-label={t('Channel Options')}
                        className='str-chat__message-actions-list'
                        role='listbox'>
                        {/* Leave channel button, only shown if user has permission */}
                        {canLeaveChannel() && (
                            <button
                                role='option'
                                aria-selected='false'
                                className='str-chat__message-actions-list-item str-chat__message-actions-list-item-button'
                                onClick={async () => {
                                    await leaveChannel(); // Remove user from channel
                                    dialog?.close(); // Close the dialog
                                }}>
                                {/* Button label is translated */}
                                {t('Leave group') as string}
                            </button>
                        )}

                        {/* Mute/unmute button, toggles channel mute status */}
                        <button
                            role='option'
                            aria-selected='false'
                            className='str-chat__message-actions-list-item str-chat__message-actions-list-item-button'
                            onClick={toggleMute}>
                            {/* Button label changes based on mute state */}
                            {isMuted ? t('Unmute group') as string : t('Mute group') as string}
                        </button>

                        {/* Hide conversation button, hides the channel from the user's list */}
                        <button
                            role='option'
                            aria-selected='false'
                            className='str-chat__message-actions-list-item str-chat__message-actions-list-item-button'
                            onClick={async () => {
                                await channel.hide(); // Hide the channel
                                dialog?.close(); // Close the dialog
                            }}>
                            {t('Hide conversation') as string}
                        </button>
                    </div>
                </div>
            </DialogAnchor>

            {/* Main action button (vertical ellipsis), toggles the dialog open/close */}
            <button
                aria-label={t('More options')}
                className='str-chat__channel-preview__action-button'
                onClick={(e) => {
                    e.stopPropagation(); // Prevent click from bubbling to parent
                    dialog?.toggle(); // Toggle the dialog open/close
                }}
                title={t('More options')}
                ref={buttonRef}
                aria-expanded={dialogIsOpen}
                aria-haspopup='true'>
                <VerticalEllipsisIcon className='str-chat__channel-preview__action-button-icon' />
            </button>
        </div>
    );
}
