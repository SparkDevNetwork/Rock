import React from 'react';

import { Thread, useChannelStateContext } from 'stream-chat-react';
import { SafeMessageInput } from '../../MessageInput/SafeMessageInput';
import { ChannelThreadList } from '../../ChannelThreadList/ChannelThreadList';
import { ChannelPaneHeader } from '../ChannelPaneHeader';

export const ThreadPaneContent = () => {
    const { thread } = useChannelStateContext();

    if (!thread) {
        return (
            <div className="thread-pane-content">
                <ChannelPaneHeader title="Threads" icon="fas fa-comments" />
                <ChannelThreadList />
            </div>
        )
    }

    // if (!thread) return <div>Select a message to view the thread.</div>;
    return (
        <div className="thread-pane-content">
            {/* Header is handled in the Channel component */}
            <Thread Input={SafeMessageInput} />
        </div>
    );
};