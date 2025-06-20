import React from 'react';

import clsx from 'clsx';
import { isMessageEdited, MessageTimestampProps, useComponentContext, useMessageContext, useTranslationContext } from 'stream-chat-react';
import { Timestamp as DefaultTimestamp } from './Timestamp';

export type MessageEditedTimestampProps = MessageTimestampProps & {
    open: boolean;
};

export function MessageEditedTimestamp({
    message: propMessage,
    open,
    ...timestampProps
}: MessageEditedTimestampProps) {
    const { t } = useTranslationContext('MessageEditedTimestamp');
    const { message: contextMessage } = useMessageContext(
        'MessageEditedTimestamp',
    );
    const { Timestamp = DefaultTimestamp } = useComponentContext('MessageEditedTimestamp');
    const message = propMessage || contextMessage;

    if (!isMessageEdited(message)) {
        return null;
    }

    return (
        <div
            className={clsx(
                'str-chat__message-edited-timestamp',
                open
                    ? 'str-chat__message-edited-timestamp--open'
                    : 'str-chat__message-edited-timestamp--collapsed',
            )}
            data-testid='message-edited-timestamp'
        >
            {t('Edited') as string}{' '}
            <Timestamp timestamp={message.message_text_updated_at} {...timestampProps} />
        </div>
    );
}