/**
 * RockChannelPreviewActionButtons Component
 *
 * Renders a set of contextual action buttons for a chat channel preview.
 * Includes support for muting/unmuting, leaving, and hiding channels. Uses Stream's Dialog system for interaction.
 *
 * @template SCG - StreamChat ExtendableGenerics for typed support.
 *
 * @param {ChannelPreviewActionButtonsProps<SCG>} props
 * @param {Channel<SCG>} props.channel - The Stream channel this preview belongs to.
 *
 * @returns {JSX.Element} Action buttons component for a channel preview.
 */
import React, { useRef } from 'react';
import type { Channel } from 'stream-chat';
import {
    useChatContext,
    useTranslationContext,
    DialogAnchor,
    useDialog,
    useDialogIsOpen,
} from 'stream-chat-react';
import 'stream-chat-react/dist/css/v2/index.css';
import { VerticalEllipsisIcon } from './Icons';

export type ChannelPreviewActionButtonsProps = {
    channel: Channel;
};

export function RockChannelPreviewActionButtons({
    channel,
}: ChannelPreviewActionButtonsProps) {
    const { t } = useTranslationContext();
    const { client } = useChatContext();

    const dialogId = `channel-actions-${channel.id}`;
    const dialog = useDialog({ id: dialogId });
    const dialogIsOpen = useDialogIsOpen(dialogId);
    const buttonRef = useRef<HTMLButtonElement | null>(null);

    const isMuted = channel.muteStatus().muted;

    const toggleMute = async () => {
        if (isMuted) {
            await channel.unmute();
        } else {
            await channel.mute();
        }

        dialog?.close();
    };

    const canLeaveChannel = () => {
        const capabilities = channel?.data?.own_capabilities as string[] || [];
        const leavingAllowed = channel?.data?.rock_leaving_allowed === true;
        return capabilities.includes('leave-channel') && leavingAllowed;
    };

    const leaveChannel = async () => {
        await channel.removeMembers([client.userID!]);
    }

    return (
        <div className='str-chat__channel-preview__action-buttons'>
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
                        {canLeaveChannel() && (
                            <button
                                role='option'
                                aria-selected='false'
                                className='str-chat__message-actions-list-item str-chat__message-actions-list-item-button'
                                onClick={async () => {
                                    await leaveChannel();
                                    dialog?.close();
                                }}>
                                {t('Leave group') as string}
                            </button>
                        )}

                        <button
                            role='option'
                            aria-selected='false'
                            className='str-chat__message-actions-list-item str-chat__message-actions-list-item-button'
                            onClick={toggleMute}>
                            {isMuted ? t('Unmute group') as string : t('Mute group') as string}
                        </button>

                        <button
                            role='option'
                            aria-selected='false'
                            className='str-chat__message-actions-list-item str-chat__message-actions-list-item-button'
                            onClick={async () => {
                                await channel.hide();
                                dialog?.close();
                            }}>
                            {t('Hide conversation') as string}
                        </button>
                    </div>
                </div>
            </DialogAnchor>

            <button
                aria-label={t('More options')}
                className='str-chat__channel-preview__action-button'
                onClick={(e) => {
                    e.stopPropagation();
                    dialog?.toggle();
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
