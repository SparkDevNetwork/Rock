import React, { useEffect } from "react";
import {
    Chat,
    ChannelList,
    Channel,
    ChannelHeader,
    MessageList,
    MessageInput,
    Thread,
    Window
} from "stream-chat-react";
import { ChannelSort, StreamChat } from "stream-chat";
import "stream-chat-react/dist/css/v2/index.css";
import type { ChatComponentProps } from "./chatComponentConfig";

const ChatComponent: React.FC<ChatComponentProps> = (props) => {
    const chatClient = StreamChat.getInstance(props.apiKey);
    const filters = { members: { $in: [props.userId] } }; // Filter to get channels where the user is a member
    const sort: ChannelSort = { last_message_at: -1 };
    const options = { limit: 20, messages_limit: 30 };

    useEffect(() => {
        chatClient.connectUser(
            {
                id: props.userId,
            },
            props.userToken
        );

        return () => {
            chatClient.disconnectUser();
        };
    }, [chatClient, props.userId, props.userToken]);

    const themeStyles = {
        '--str-chat-primary-color': props.primaryColor,
        '--str-chat-active-primary-color': props.activePrimaryColor,
        '--str-chat-surface-color': props.surfaceColor,
        '--str-chat-secondary-surface-color': props.secondarySurfaceColor,
        '--str-chat-primary-surface-color': props.primarySurfaceColor,
        '--str-chat-primary-surface-color-low-emphasis': props.primarySurfaceColorLowEmphasis,
        '--str-chat-border-radius-circle': props.borderRadiusCircle,
    } as React.CSSProperties;

    return (
        <div style={themeStyles}>
            <Chat client={chatClient} theme="str-chat__theme-custom">
                <ChannelList filters={filters} sort={sort} options={options} />
                <Channel>
                    <Window>
                        <ChannelHeader />
                        <MessageList />
                        <MessageInput />
                    </Window>
                    <Thread />
                </Channel>
            </Chat>
        </div>
    );
};

export default ChatComponent;