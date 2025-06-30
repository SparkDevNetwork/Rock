import React, { useMemo } from "react";
import { MessageTimestampProps, useMessageContext, useComponentContext, isDate, TimestampProps, useTranslationContext } from "stream-chat-react";
import { getDateString } from "./utils";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatViewStyle } from "../../ChatViewStyle";


const UnMemoizedRockMessageTimestamp = (props: MessageTimestampProps) => {
    const { message: propMessage, ...timestampProps } = props;
    const { message: contextMessage } = useMessageContext('MessageTimestamp');

    const message = propMessage || contextMessage;

    return <RockTimestamp timestamp={message.created_at} {...timestampProps} />;
};

export const RockMessageTimestamp = React.memo(
    UnMemoizedRockMessageTimestamp,
) as typeof UnMemoizedRockMessageTimestamp;

export function RockTimestamp(props: TimestampProps) {
    const { calendar, calendarFormats, customClass, format, timestamp } = props;

    const { formatDate: contextFormatDate } = useMessageContext('MessageTimestamp');
    const { t, tDateTimeParser } = useTranslationContext('MessageTimestamp');

    const normalizedTimestamp =
        timestamp && isDate(timestamp) ? timestamp.toISOString() : timestamp;

    const { chatViewStyle } = useChatConfig();
    const isCommunityStyle = chatViewStyle === ChatViewStyle.Community;

    const formatDate = isCommunityStyle
        ? (date: Date) =>
            date.toLocaleTimeString('en-US', {
                hour: 'numeric',
                minute: '2-digit',
                hour12: true,
            })
        : contextFormatDate;

    const when = useMemo(
        () =>
            getDateString({
                calendar,
                calendarFormats,
                format,
                formatDate,
                messageCreatedAt: normalizedTimestamp,
                t,
                tDateTimeParser,
                timestampTranslationKey: 'timestamp/MessageTimestamp',
            }),
        [
            calendar,
            calendarFormats,
            format,
            formatDate,
            normalizedTimestamp,
            t,
            tDateTimeParser,
        ],
    );

    if (!when) return null;

    return (
        <time
            className={customClass}
            dateTime={normalizedTimestamp}
            title={normalizedTimestamp}
        >
            {when}
        </time>
    );
}
