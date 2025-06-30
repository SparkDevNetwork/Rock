import React from 'react';

import type { ThreadManagerState } from 'stream-chat';
import { useChatContext, useStateStore } from 'stream-chat-react';


const selector = (nextValue: ThreadManagerState) => ({
    unseenThreadIds: nextValue.unseenThreadIds,
});

export const ChannelThreadListUnseenThreadsBanner = () => {
    const { client } = useChatContext();
    const { unseenThreadIds } = useStateStore(client.threads.state, selector);

    if (!unseenThreadIds.length) return null;

    return (
        <div className='str-chat__unseen-threads-banner'>
            {/* TODO: translate */}
            {unseenThreadIds.length} unread threads
            <button
                className='str-chat__unseen-threads-banner__button'
                onClick={() => client.threads.reload()}
            >
                <i className="fas fa-sync-alt" />
            </button>
        </div>
    )
};