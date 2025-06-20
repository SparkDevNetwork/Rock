import React from "react";
import {
    DateSeparator,
    DateSeparatorProps,
    useTranslationContext
} from "stream-chat-react";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatViewStyle } from "../ChatViewStyle";
import { getDateString } from "../Message/utils";

export const RockDateSeperator = (props: DateSeparatorProps) => {
    const { chatViewStyle } = useChatConfig();
    const { t, tDateTimeParser } = useTranslationContext('DateSeparator');

    const {
        calendar,
        date: messageCreatedAt,
        formatDate,
        position = 'center',
        unread,
        ...restTimestampFormatterOptions
    } = props;

    const formattedDate = getDateString({
        calendar,
        ...restTimestampFormatterOptions,
        formatDate,
        messageCreatedAt,
        t,
        tDateTimeParser,
        timestampTranslationKey: 'timestamp/DateSeparator',
    });

    if (chatViewStyle == ChatViewStyle.Conversational) {
        return <DateSeparator {...props} />;
    }

    // Community style: centered bubble + horizontal lines
    return (
        <div className="rock-date-separator" role="separator">
            <div className="rock-date-separator__bar" />
            <div className="rock-date-separator__bubble">
                <span>
                    {unread ? `${t('New')} – ${formattedDate}` : formattedDate}
                </span>
            </div>
            <div className="rock-date-separator__bar" />
        </div>
    );
};