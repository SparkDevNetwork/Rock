import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { MessageSearch } from '../../MessageSearch/MessageSearch';

/**
 * MessageSearchPaneContent
 *
 * Displays the message search pane for a chat channel. Provides a header and the message search UI.
 *
 * - Uses ChannelPaneHeader for the section title and icon.
 * - Uses MessageSearch for the search input and results.
 *
 * @returns {JSX.Element} The rendered message search pane content.
 */
export const MessageSearchPaneContent: React.FC = () => {
    return (
        <div className="message-search-content">
            {/* Header for the message search pane */}
            <ChannelPaneHeader title="Search Messages" icon="fas fa-search" />
            {/* Message search input and results */}
            <MessageSearch />
        </div>
    )
}