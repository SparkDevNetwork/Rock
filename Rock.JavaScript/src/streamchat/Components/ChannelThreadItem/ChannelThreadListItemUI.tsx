import React, { useCallback } from 'react';
import clsx from 'clsx';

import type { LocalMessage, ThreadState } from 'stream-chat';
import type { ComponentPropsWithoutRef } from 'react';
import { useChatContext, useStateStore, useChannelPreviewInfo, useThreadsViewContext, Avatar, MessageText, renderText } from 'stream-chat-react';
import { Timestamp } from '../Message/Timestamp';
import { useChannelThreadListItemContext } from './ChannelThreadListItem';
import { RockMessageTimestamp } from '../Message/RockMessageTimestamp';


export type ThreadListItemUIProps = ComponentPropsWithoutRef<'button'>;

/**
 * TODO:
 * - maybe hover state? ask design
 */

export const attachmentTypeIconMap = {
    audio: '🔈',
    file: '📄',
    image: '📷',
    video: '🎥',
    voiceRecording: '🎙️',
} as const;

// TODO: translations
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

export const ChannelThreadListItemUI = (props: ThreadListItemUIProps) => {
    const { client } = useChatContext();
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const thread = useChannelThreadListItemContext()!;

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

    const { channel, deletedAt, latestReply, ownUnreadMessageCount, parentMessage } =
        useStateStore(thread.state, selector);

    const { displayTitle: channelDisplayTitle } = useChannelPreviewInfo({ channel });

    const { activeThread, setActiveThread } = useThreadsViewContext();

    // The channel thread list item is really just going to be a preview 
    // of the parent message.
    if (!parentMessage) {
        return (
            <div className='str-chat__thread-list-item str-chat__thread-list-item--empty'>
                <div className='str-chat__thread-list-item__empty-text'>
                    {channelDisplayTitle} has no threads yet.
                </div>
            </div>
        );
    }

    const parentMessageUser = parentMessage.user?.name || parentMessage.user?.id || 'Unknown sender';

    const foo = () => {
        return (
            <button
                aria-selected={activeThread === thread}
                className='rock-channel-thread-list-item'
                data-thread-id={thread.id}
                onClick={() => setActiveThread(thread)}
                role='option'
                {...props}>

                <div className="rock-channel-thread-list-item-layout">
                    <Avatar {...parentMessage.user} className="rock-channel-thread-item-avatar" />

                    <div className="rock-channel-thread-list-item-parent-message-preview-content">
                        <div className="rock-channel-thread-list-item-parent-message-preview-title-layout">
                            <p className="rock-channel-thread-list-item-parent-message-preview-created-by-text">
                                {parentMessageUser}
                            </p>

                            <RockMessageTimestamp message={parentMessage} customClass="rock-channel-thread-list-item-parent-message-preview-created-at" isChannelThread />
                        </div>

                        {/* <div className="rock-channel-thread-list-item-parent-message-preview-text">
                            <p className="rock-channel-thread-list-item-parent-message-preview-text-inner">
                                {getTitleFromMessage({
                                    currentUserId: client.userID,
                                    message: parentMessage,
                                })}
                            </p>
                        </div> */}

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
    return foo();
};