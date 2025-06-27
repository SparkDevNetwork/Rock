import React from 'react';

import { Thread, useChannelStateContext, useThreadsViewContext } from 'stream-chat-react';
import { SafeMessageInput } from '../../MessageInput/SafeMessageInput';
import { ChannelThreadList } from '../../ChannelThreadList/ChannelThreadList';
import { ChannelPaneHeader } from '../ChannelPaneHeader';

export const ThreadPaneContent = () => {
    const { thread } = useChannelStateContext();
    const { activeThread } = useThreadsViewContext();
    if (!thread) {

        return (
            <div style={{ height: '100%', display: 'flex', flexDirection: 'column', width: '100%' }}>
                <ChannelPaneHeader title="Threads" icon="fas fa-comments" />
                <ChannelThreadList />
            </div>
        )
    }

    // if (!thread) return <div>Select a message to view the thread.</div>;
    return (
        <div style={{ height: '100%', display: 'flex', flexDirection: 'column', width: '100%' }}>
            <ChannelPaneHeader title="Thread" icon="fas fa-comment-dots" />
            <Thread Input={SafeMessageInput} />
        </div>
    );
};