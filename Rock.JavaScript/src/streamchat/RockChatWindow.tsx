import React from 'react';
import {
    MessageList,
    Thread,
    ThreadList,
    Window
} from 'stream-chat-react';

import { RockChannelHeader } from './Components/ChannelHeader/RockChannelHeader';
import { SafeMessageInput } from './Components/MessageInput/SafeMessageInput';
import { ChannelRightPane } from './Components/ChannelRightPane/ChannelRightPane';
import { useChannelRightPane } from './Components/ChannelRightPane/ChannelRightPaneContext';

/**
 * Encapsulates the full chat window view, including header, message list, input, thread, and right-side pane.
 */
export const RockChatWindow: React.FC = () => {
    const { activePane } = useChannelRightPane();

    const windowClassName = `rock-chat-window${activePane ? ' rock-chat-window--with-pane' : ''}`;

    return (
        <Window>
            <RockChannelHeader />

            <div className={windowClassName}>
                <div className="rock-chat-window-content">
                    <div className="rock-chat-window-content-left">
                        <MessageList
                            messageActions={[
                                'edit',
                                'delete',
                                'flag',
                                'mute',
                                'quote',
                                'react',
                                'reply'
                            ]}
                            noGroupByUser
                        />
                        <SafeMessageInput />
                    </div>
                    <div className="rock-chat-window-content-right">
                        {activePane && <ChannelRightPane />}
                        {/* <Thread /> */}
                    </div>
                </div>
            </div>
        </Window>
    );
};
