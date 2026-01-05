import React, { useMemo } from "react";
import { MessageTimestampProps, useMessageContext, useComponentContext, isDate, TimestampProps, useTranslationContext } from "stream-chat-react";
import { getDateString } from "./utils";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatViewStyle } from "../../ChatViewStyle";

interface RockMessageTimestampProps extends MessageTimestampProps {
    isChannelThread?: boolean;
}

const UnMemoizedRockMessageTimestamp = (props: RockMessageTimestampProps) => {
    const { message: propMessage, ...timestampProps } = props;
    const { message: contextMessage } = useMessageContext('MessageTimestamp');

    const message = propMessage || contextMessage;

    return <RockTimestamp timestamp={message.created_at} {...timestampProps} />;
};

interface RockTimestampProps extends TimestampProps {
    isChannelThread?: boolean;
}

export const RockMessageTimestamp = React.memo(
    UnMemoizedRockMessageTimestamp,
) as typeof UnMemoizedRockMessageTimestamp;

export function RockTimestamp(props: RockTimestampProps) {
    const { calendar, calendarFormats, customClass, format, timestamp } = props;

    const { formatDate: contextFormatDate } = useMessageContext('MessageTimestamp');
    const { t, tDateTimeParser } = useTranslationContext('MessageTimestamp');

    const normalizedTimestamp =
        timestamp && isDate(timestamp) ? timestamp.toISOString() : timestamp;

    const { chatViewStyle } = useChatConfig();
    const isCommunityStyle = chatViewStyle === ChatViewStyle.Community;

    const communityMessageDateFormat = (date: Date) => {
        return date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        });
    }

    const threadMessageDateFormat = (date: Date) => {
        const datePart = date.toLocaleDateString('en-US', {
            year: '2-digit',
            month: '2-digit',
            day: '2-digit',
        });

        const timePart = date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
        }).replace(/\s/g, ''); // remove space before AM/PM

        return `${datePart} ${timePart}`;
    };

    const customDateFormat = isCommunityStyle
        ? (date: Date) => {
            return props.isChannelThread
                ? threadMessageDateFormat(date)
                : communityMessageDateFormat(date);
        }
        : contextFormatDate;

    const when = useMemo(
        () =>
            getDateString({
                calendar,
                calendarFormats,
                format,
                formatDate: customDateFormat,
                messageCreatedAt: normalizedTimestamp,
                t,
                tDateTimeParser,
                timestampTranslationKey: 'timestamp/MessageTimestamp',
            }),
        [
            calendar,
            calendarFormats,
            format,
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
