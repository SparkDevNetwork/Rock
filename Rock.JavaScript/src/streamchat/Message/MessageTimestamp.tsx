import React from 'react';
import { Timestamp as DefaultTimestamp } from './Timestamp';
import { DefaultStreamChatGenerics, TimestampFormatterOptions, StreamMessage, useMessageContext, useComponentContext } from 'stream-chat-react';

export type MessageTimestampProps<
    StreamChatGenerics extends DefaultStreamChatGenerics = DefaultStreamChatGenerics,
> = TimestampFormatterOptions & {
    /* Adds a CSS class name to the component's outer `time` container. */
    customClass?: string;
    /* The `StreamChat` message object, which provides necessary data to the underlying UI components (overrides the value from `MessageContext`) */
    message?: StreamMessage<StreamChatGenerics>;
};

const UnMemoizedMessageTimestamp = <
    StreamChatGenerics extends DefaultStreamChatGenerics = DefaultStreamChatGenerics,
>(
    props: MessageTimestampProps<StreamChatGenerics>,
) => {
    const { message: propMessage, ...timestampProps } = props;
    const { message: contextMessage } =
        useMessageContext<StreamChatGenerics>('MessageTimestamp');
    const { Timestamp = DefaultTimestamp } = useComponentContext('MessageTimestamp');
    const message = propMessage || contextMessage;
    return <Timestamp timestamp={message.created_at} {...timestampProps} />;
};

export const MessageTimestamp = React.memo(
    UnMemoizedMessageTimestamp,
) as typeof UnMemoizedMessageTimestamp;