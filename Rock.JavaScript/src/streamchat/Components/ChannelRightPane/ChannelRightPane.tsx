import React from 'react';
import { useChannelRightPane } from './ChannelRightPaneContext';
import { InfoPaneContent } from './ChannelInfo/InfoPaneContent';
import { ThreadPaneContent } from './Thread/ThreadPaneContent';
import { MessageSearchPaneContent } from './MessageSearch/MessageSearchContent';
import { MentionsListPaneContent } from './MentionsList/MentionsListPaneContent';

// Stub components for different views
const MembersView = () => <div>Members Panel</div>;
const MoreOptions = () => <div>More Options Panel</div>;

export const ChannelRightPane: React.FC = () => {
    const { activePane } = useChannelRightPane();

    if (!activePane) return null;

    const renderContent = () => {
        switch (activePane) {
            case 'info': return <InfoPaneContent />;
            case 'threads': return <ThreadPaneContent />;
            case 'search': return <MessageSearchPaneContent />;
            case 'mentions': return <MentionsListPaneContent />;
            case 'members': return <MembersView />;
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
