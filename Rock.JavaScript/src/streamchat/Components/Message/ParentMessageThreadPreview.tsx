import React, { useEffect, useRef, useState } from 'react';
import type { LocalMessage } from 'stream-chat';
import { formatMessage } from 'stream-chat';
import {
    useChatContext,
    useTranslationContext,
    useChannelStateContext,
    useChannelActionContext,
    useMessageContext
} from 'stream-chat-react';
import { useChannelRightPane } from '../ChannelRightPane/ChannelRightPaneContext';

export const ParentMessageThreadPreview = () => {
    const { client } = useChatContext();
    const { t } = useTranslationContext();
    const { channel } = useChannelStateContext();
    const { openThread } = useChannelActionContext();
    const { message } = useMessageContext();

    const [parentMessageText, setParentMessageText] = useState<string | null>(null);
    const parentMessageRef = useRef<LocalMessage | null | undefined>(undefined);
    const { setActivePane } = useChannelRightPane();

    const querySearchParent = () =>
        channel
            .getClient()
            .search({ cid: channel.cid }, { id: message.parent_id })
            .then(({ results }) => {
                if (!results.length) {
                    throw new Error('Thread has not been found');
                }
                const formatted = formatMessage(results[0].message);
                parentMessageRef.current = formatted;
                setParentMessageText(formatted.text || '(no content)');
            })
            .catch((error: Error) => {
                client.notifications.addError({
                    message: t('Thread has not been found'),
                    options: {
                        originalError: error,
                        type: 'api:message:search:not-found',
                    },
                    origin: {
                        context: { threadReply: message },
                        emitter: 'MessageIsThreadReplyInChannelButtonIndicator',
                    },
                });
            });

    useEffect(() => {
        if (!message.parent_id || parentMessageRef.current !== undefined) return;

        const localMessage = channel.state.findMessage(message.parent_id);
        if (localMessage) {
            parentMessageRef.current = localMessage;
            setParentMessageText(localMessage.text || '(no content)');
        }
    }, [channel, message]);

    if (!message.parent_id || parentMessageText === null) return null;

    return (
        <div className='str-chat__message-thread-preview-wrapper'>
            <button
                className='str-chat__message-thread-preview-button'
                onClick={async () => {
                    if (!parentMessageRef.current) {
                        await querySearchParent();
                        if (parentMessageRef.current) {
                            openThread(parentMessageRef.current);
                            setActivePane('threads');

                        } else {
                            parentMessageRef.current = null;
                        }
                        return;
                    }

                    openThread(parentMessageRef.current);
                    setActivePane('threads');

                }}
                type='button'
            >
                <i className='fas fa-comments' />
                <span className='str-chat__message-thread-preview-text'>
                    {parentMessageText?.slice(0, 100) || '(no content)'}
                </span>
            </button>
        </div>
    );
};