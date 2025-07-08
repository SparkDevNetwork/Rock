import React from 'react';
import { Thread, useChannelStateContext } from 'stream-chat-react';
import { SafeMessageInput } from '../../MessageInput/SafeMessageInput';
import { ChannelThreadList } from '../../ChannelThreadList/ChannelThreadList';
import { ChannelPaneHeader } from '../ChannelPaneHeader';

/**
 * ThreadPaneContent
 *
 * Displays the thread pane for a chat channel. If a thread is selected, shows the thread view with messages and input.
 * If no thread is selected, shows a list of available threads in the channel.
 *
 * - Uses Stream Chat's Thread component for the active thread view.
 * - Uses ChannelThreadList to display all threads when none is selected.
 * - Header is shown only when no thread is selected (otherwise handled by parent Channel component).
 *
 * @returns {JSX.Element} The rendered thread pane content.
 */
export const ThreadPaneContent = () => {
    // Access the current thread from channel state context
    const { thread } = useChannelStateContext();

    // If no thread is selected, show the thread list and header
    if (!thread) {
        return (
            <div className="thread-pane-content">
                <ChannelPaneHeader title="Threads" icon="fas fa-comments" />
                <ChannelThreadList />
            </div>
        )
    }

    // If a thread is selected, show the thread view (header handled by parent)
    return (
        <div className="thread-pane-content">
            {/* Header is handled in the Channel component */}
            <Thread Input={SafeMessageInput} />
        </div>
    );
};