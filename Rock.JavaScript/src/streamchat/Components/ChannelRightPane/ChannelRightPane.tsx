import React from 'react';
import { useChannelRightPane } from './ChannelRightPaneContext';
import { InfoPaneContent } from './ChannelInfo/InfoPaneContent';
import { ThreadPaneContent } from './Thread/ThreadPaneContent';
import { MessageSearchPaneContent } from './MessageSearch/MessageSearchContent';
import { MentionsListPaneContent } from './MentionsList/MentionsListPaneContent';
import { MembersPaneContent } from './Members/MembersPaneContent';
import { ChannelMemberListProvider } from '../ChannelMemberList/ChannelMemberListContext';

// Stub components for different views
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
            case 'members': return <ChannelMemberListProvider><MembersPaneContent /></ChannelMemberListProvider>;
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
