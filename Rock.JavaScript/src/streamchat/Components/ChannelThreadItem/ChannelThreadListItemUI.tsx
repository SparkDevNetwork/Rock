import React, { useCallback } from 'react';
import clsx from 'clsx';

import type { LocalMessage, ThreadState } from 'stream-chat';
import type { ComponentPropsWithoutRef } from 'react';
import { useChatContext, useStateStore, useChannelPreviewInfo, useThreadsViewContext, Avatar, renderText } from 'stream-chat-react';
import { useChannelThreadListItemContext } from './ChannelThreadListItem';
import { RockMessageTimestamp } from '../Message/RockMessageTimestamp';

export type ThreadListItemUIProps = ComponentPropsWithoutRef<'button'>;

/**
 * attachmentTypeIconMap
 *
 * Maps attachment types to their corresponding emoji icons for display in thread previews.
 */
export const attachmentTypeIconMap = {
    audio: '🔈',
    file: '📄',
    image: '📷',
    video: '🎥',
    voiceRecording: '🎙️',
} as const;

/**
 * getTitleFromMessage
 *
 * Returns a preview string for a message, including attachment icons and special handling for deleted or voice messages.
 *
 * @param currentUserId - The current user's ID (for future use)
 * @param message - The message to generate a title for
 * @returns {string} The preview string for the message
 */
const getTitleFromMessage = ({
    currentUserId,
    message,
}: {
    currentUserId?: string;
    message?: LocalMessage;
}) => {
    const attachment = message?.attachments?.at(0);

    let attachmentIcon = '';

    if (attachment) {
        attachmentIcon +=
            attachmentTypeIconMap[
            (attachment.type as keyof typeof attachmentTypeIconMap) ?? 'file'
            ] ?? attachmentTypeIconMap.file;
    }

    if (message?.deleted_at && message.parent_id)
        return "This thread was deleted.";

    if (attachment?.type === 'voiceRecording')
        return clsx(attachmentIcon, 'Voice message');

    return renderText(
        message?.text || '');
};

/**
 * ChannelThreadListItemUI
 *
 * Renders a single thread list item for the channel thread list. Shows a preview of the parent message,
 * including avatar, sender, timestamp, and message preview. Handles selection and accessibility.
 *
 * - Uses context to get the current thread object.
 * - Uses useStateStore to efficiently select thread state.
 * - Handles empty state if no parent message exists.
 * - Highlights the item if it is the active thread.
 *
 * @param props - Additional button props for accessibility and styling
 * @returns {JSX.Element} The rendered thread list item UI
 */
export const ChannelThreadListItemUI = (props: ThreadListItemUIProps) => {
    const { client } = useChatContext();
    // Get the current thread from context (non-null assertion for required context)
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const thread = useChannelThreadListItemContext()!;

    // Selector for thread state (minimizes re-renders)
    const selector = useCallback(
        (nextValue: ThreadState) => ({
            channel: nextValue.channel,
            deletedAt: nextValue.deletedAt,
            latestReply: nextValue.replies.at(-1),
            ownUnreadMessageCount:
                (client.userID && nextValue.read[client.userID]?.unreadMessageCount) || 0,
            parentMessage: nextValue.parentMessage,
        }),
        [client],
    );

    // Get channel and parent message from thread state
    const { channel, parentMessage } =
        useStateStore(thread.state, selector);

    // Get display title for the channel
    const { displayTitle: channelDisplayTitle } = useChannelPreviewInfo({ channel });

    // Get active thread and setter from context
    const { activeThread, setActiveThread } = useThreadsViewContext();

    // If no parent message, show empty state
    if (!parentMessage) {
        return (
            <div className='str-chat__thread-list-item str-chat__thread-list-item--empty'>
                <div className='str-chat__thread-list-item__empty-text'>
                    {channelDisplayTitle} has no threads yet.
                </div>
            </div>
        );
    }

    // Get sender name or fallback
    const parentMessageUser = parentMessage.user?.name || parentMessage.user?.id || 'Unknown sender';

    /**
     * getChannelThreadComponent
     *
     * Renders the main thread list item button, including avatar, sender, timestamp, and message preview.
     * Handles selection and click to activate the thread.
     *
     * @returns {JSX.Element} The rendered thread list item
     */
    const getChannelThreadComponent = () => {
        return (
            <button
                aria-selected={activeThread === thread}
                className='rock-channel-thread-list-item'
                data-thread-id={thread.id}
                onClick={() => setActiveThread(thread)}
                role='option'
                {...props}>

                <div className="rock-channel-thread-list-item-layout">
                    {/* Avatar for the sender of the parent message */}
                    <Avatar {...parentMessage.user} className="rock-channel-thread-item-avatar" />

                    <div className="rock-channel-thread-list-item-parent-message-preview-content">
                        <div className="rock-channel-thread-list-item-parent-message-preview-title-layout">
                            {/* Sender name */}
                            <p className="rock-channel-thread-list-item-parent-message-preview-created-by-text">
                                {parentMessageUser}
                            </p>

                            {/* Timestamp for the parent message */}
                            <RockMessageTimestamp message={parentMessage} customClass="rock-channel-thread-list-item-parent-message-preview-created-at" isChannelThread />
                        </div>

                        {/* Message preview (text or attachment icon) */}
                        <div className="str-chat__message-text" tabIndex={0}>
                            <div
                                className={clsx('str-chat__message-text-inner')}>
                                <div>{getTitleFromMessage({
                                    currentUserId: client.userID,
                                    message: parentMessage,
                                })}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </button>
        )
    }
    return getChannelThreadComponent();
};