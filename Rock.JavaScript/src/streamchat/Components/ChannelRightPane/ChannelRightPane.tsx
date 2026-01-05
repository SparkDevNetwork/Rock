import React from 'react';
import { useChannelRightPane } from './ChannelRightPaneContext';
import { InfoPaneContent } from './ChannelInfo/InfoPaneContent';
import { ThreadPaneContent } from './Thread/ThreadPaneContent';
import { MessageSearchPaneContent } from './MessageSearch/MessageSearchContent';
import { MentionsListPaneContent } from './MentionsList/MentionsListPaneContent';
import { MembersPaneContent } from './Members/MembersPaneContent';

/**
 * MoreOptions
 *
 * Stub component for the 'more' pane view. Replace with actual content as needed.
 *
 * @returns {JSX.Element} The rendered more options panel.
 */
const MoreOptions = () => <div>More Options Panel</div>;

/**
 * ChannelRightPane
 *
 * Main component for rendering the right pane in the chat UI. Displays different content based on the active pane view.
 *
 * - Uses useChannelRightPane to determine which pane is active.
 * - Renders the appropriate content component for each pane type.
 * - Returns null if no pane is active.
 *
 * @returns {JSX.Element | null} The rendered right pane or null if closed.
 */
export const ChannelRightPane: React.FC = () => {
    // Access the currently active pane from context
    const { activePane } = useChannelRightPane();

    // If no pane is active, render nothing
    if (!activePane) return null;

    /**
     * renderContent
     *
     * Returns the content component for the currently active pane.
     *
     * @returns {JSX.Element | null} The content for the active pane
     */
    const renderContent = () => {
        switch (activePane) {
            case 'info': return <InfoPaneContent />;
            case 'threads': return <ThreadPaneContent />;
            case 'search': return <MessageSearchPaneContent />;
            case 'mentions': return <MentionsListPaneContent />;
            case 'members': return <MembersPaneContent />;
            case 'more': return <MoreOptions />;
            default: return null;
        }
    };

    return (
        <div className="rock-channel-right-pane">
            <div className="rock-channel-right-pane-content">
                {renderContent()}
            </div>
        </div>
    );
};
