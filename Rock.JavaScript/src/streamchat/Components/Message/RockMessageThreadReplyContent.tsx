import React from "react";
import { MouseEventHandler } from "react";
import { useTranslationContext } from "stream-chat-react";

export type RockMessageThreadTypeReplyContentProps = {
    /* If supplied, adds custom text to the end of a multiple replies message */
    labelPlural?: string;
    /* If supplied, adds custom text to the end of a single reply message */
    labelSingle?: string;
    /* Function to navigate into an existing thread on a message */
    onClick?: MouseEventHandler;
    /* The amount of replies (i.e., threaded messages) on a message */
    reply_count?: number;
};

export const RockMessageThreadReplyContent = (props: RockMessageThreadTypeReplyContentProps) => {
    const { labelPlural, labelSingle, onClick, reply_count = 0 } = props;

    const { t } = useTranslationContext('MessageRepliesCountButton');

    if (!reply_count) return null;

    let replyCountText = t('replyCount', { count: reply_count });

    if (labelPlural && reply_count > 1) {
        replyCountText = `${reply_count} ${labelPlural}`;
    } else if (labelSingle) {
        replyCountText = `1 ${labelSingle}`;
    }

    return (
        <div className="rock-replies-content-container">
            <button
                className="rock-replies-content-thread-reply-button"
                data-testid='replies-count-button'
                onClick={onClick}>
                View thread
            </button>

            <div className="rock-replies-content-count">
                <i className="far fa-comments" />
                <span className="rock-replies-content-count-text">
                    {replyCountText}
                </span>
            </div>
        </div>
    );
};
