import React from 'react';
import { useChannelRightPane } from './PanelContext';

// Stub components for different views
const ChannelInfo = () => <div>Channel Info Panel</div>;
const ThreadsView = () => <div>Threads Panel</div>;
const SearchView = () => <div>Search Panel</div>;
const MentionsView = () => <div>Mentions Panel</div>;
const MembersView = () => <div>Members Panel</div>;
const MoreOptions = () => <div>More Options Panel</div>;

export const ChannelRightPane: React.FC = () => {
    const { activePane, closePane } = useChannelRightPane();

    if (!activePane) return null;

    const renderContent = () => {
        switch (activePane) {
            case 'info': return <ChannelInfo />;
            case 'threads': return <ThreadsView />;
            case 'search': return <SearchView />;
            case 'mentions': return <MentionsView />;
            case 'members': return <MembersView />;
            case 'more': return <MoreOptions />;
            default: return null;
        }
    };

    return (
        <div className="rock-channel-right-pane">
            <div className="rock-channel-right-pane-header">
                <h4 className="rock-channel-right-pane-title">{activePane}</h4>
                <button className="rock-channel-right-pane-close" onClick={closePane} aria-label="Close Pane">
                    &times;
                </button>
            </div>
            <div className="rock-channel-right-pane-content">
                {renderContent()}
            </div>
        </div>
    );
};
