import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChatContext } from 'stream-chat-react';
import { MessageSearch } from '../../MessageSearch/MessageSearch';


export const MessageSearchPaneContent: React.FC = () => {
    const { client } = useChatContext();

    return (
        <div className="message-search-content">
            <ChannelPaneHeader title="Search Messages" icon="fas fa-search" />
            <MessageSearch />
        </div>
    )
}

