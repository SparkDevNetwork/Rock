import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChatContext } from 'stream-chat-react';
import { MessageSearch } from '../../MessageSearch/MessageSearch';
import { MentionsList } from '../../MentionsList/MentionsList';


export const MentionsListPaneContent: React.FC = () => {
    const { client, channel } = useChatContext();


    return (
        <div className="message-search-content">
            <ChannelPaneHeader title="Mentions" icon="fas fa-at" />
            <MentionsList cid={channel?.cid} />
        </div>
    )
}

