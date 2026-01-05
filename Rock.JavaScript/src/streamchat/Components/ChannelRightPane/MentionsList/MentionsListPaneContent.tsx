import React from 'react';
import { ChannelPaneHeader } from '../ChannelPaneHeader';
import { useChatContext } from 'stream-chat-react';
import { MentionsList } from '../../MentionsList/MentionsList';

/**
 * MentionsListPaneContent
 *
 * Displays the mentions list pane for a chat channel. Provides a header and the mentions list UI.
 *
 * - Uses ChannelPaneHeader for the section title and icon.
 * - Uses MentionsList to display all messages where the user is mentioned.
 * - Passes the current channel's CID to the MentionsList component.
 *
 * @returns {JSX.Element} The rendered mentions list pane content.
 */
export const MentionsListPaneContent: React.FC = () => {
    // Access the current channel from chat context
    const { channel } = useChatContext();

    return (
        <div className="message-search-content">
            {/* Header for the mentions list pane */}
            <ChannelPaneHeader title="Mentions" icon="fas fa-at" />
            {/* Mentions list for the current channel */}
            <MentionsList cid={channel?.cid} />
        </div>
    )
}

