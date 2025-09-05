import React, { forwardRef } from 'react';
import { Avatar, useChannelActionContext } from 'stream-chat-react';
import type { MessageResponse } from 'stream-chat';
import { MessageSearchResultItemTimestamp } from './MessageSearchResultItemTimestamp';
import { MessageSearchResultItemMessageText } from './MessageSearchResultItemMessageText';

export interface MessageSearchResultItemProps {
    message: MessageResponse;
    searchTerm?: string;
}

export const MessageSearchResultItem = forwardRef<HTMLLIElement, MessageSearchResultItemProps>(
    ({ message, searchTerm }, ref) => {

        const { jumpToMessage } = useChannelActionContext("MessageSearchResultItem");

        const handleClick = () => {
            if (message.id) {
                if (message.parent_id) {
                    jumpToMessage(message.parent_id);
                }
                else {
                    jumpToMessage(message.id);
                }
            }
        };

        return (
            <li className="message-search-result-item" ref={ref} onClick={handleClick}>
                <div className="message-search-result-item-container">
                    <div className="rock-avatar-container">
                        <Avatar
                            image={message.user?.image}
                            name={message.user?.name || message.user?.id}
                            user={message.user!}
                        />
                    </div>

                    <div className="rock-message-layout">
                        <div className="rock-message-title-layout">
                            <span className='rock-message-author'>
                                {message.user?.name || message.user?.id}
                            </span>
                            <div className="rock-message-time">
                                <MessageSearchResultItemTimestamp date={message.created_at!} customClass="rock-message-simple-timestamp" />
                            </div>
                        </div>

                        <div className="rock-message-content">
                            <MessageSearchResultItemMessageText message={message} searchTerm={searchTerm} />
                        </div>
                    </div>
                </div>
            </li>
        );
    }
);
MessageSearchResultItem.displayName = 'MessageSearchResultItem';
