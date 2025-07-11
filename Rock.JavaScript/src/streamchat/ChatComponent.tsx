import React, { useState } from "react";
import {
    Chat,
} from "stream-chat-react";
import { ChannelSort, ChannelFilters } from "stream-chat";
import { useCreateChatClient } from "stream-chat-react";
import "stream-chat-react/dist/css/v2/index.css";
import "./chatComponent.css";

import type { ChatComponentProps } from "./ChatComponentProps";
import { RockChannelPreview } from "./Components/ChannelPreview/RockChannelPreview";
import { ChatConfigContext } from "./Components/Chat/ChatConfigContext";
import ChannelListHeader from "./Components/ChannelListHeader/ChannelListHeader";
import { WrappedChannel } from "./Components/MessageAction/WrappedChannel";
import { WrappedChannelList } from "./Components/ChannelList/WrappedChannelList";
import { getRenderChannelsFn } from "./ChatUtils";
import { ChannelListControllerContext } from "./Components/ChannelList/ChannelListControllerContext";
import { ChatViewStyle } from "./ChatViewStyle";
import { ChannelRightPaneProvider } from "./Components/ChannelRightPane/ChannelRightPaneContext";
import { RockChatWindow } from "./RockChatWindow";
import { ModalProvider } from "./Components/Modal/ModalContext";
import { DirectoryProvider, useDirectoryContext } from "./Components/Directory/DirectoryContext";
import { Directory } from "./Components/Directory/Directory";
import { ChannelMemberListProvider } from "./Components/ChannelMemberList/ChannelMemberListContext";

/**
 * The ChatComponent sets up and renders the Stream Chat UI
 * including channel filtering, sorting, and context provisioning.
 */
const ChatComponent: React.FC<ChatComponentProps> = ({
    apiKey,
    userId,
    userToken,
    currentCampusId,
    filterSharedChannelByCampus,
    sharedChannelTypeKey,
    directMessageChannelTypeKey,
    channelId,
    selectedChannelId,
    jumpToMessageId,
    chatViewStyle,
    reactions
}) => {

    const [, setChannelListKey] = useState(0);
    const [chatComponentKey, setChatComponentKey] = useState(0);

    const refreshChannelList = () => setTimeout(() => setChannelListKey(prev => prev + 1), 200);
    const refreshChat = () => setTimeout(() => setChatComponentKey(prev => prev + 1), 200);

    const chatClient = useCreateChatClient({
        apiKey: apiKey,
        tokenOrProvider: userToken,
        userData: { id: userId },
    });

    if (!chatClient) {
        return <></>;
    }

    const userFilter: ChannelFilters = { members: { $in: [userId] } };
    const alwaysShownFilter: ChannelFilters = { rock_always_shown: true };
    const notDisabledFilter: ChannelFilters = { disabled: false };

    let finalFilter: ChannelFilters = {
        $or: [userFilter, alwaysShownFilter],
        ...notDisabledFilter,
    };

    if (filterSharedChannelByCampus && currentCampusId !== null) {
        const campusFilter: ChannelFilters = {
            $or: [
                { rock_campus_id: { $eq: Number(currentCampusId) } },
                { rock_campus_id: { $lte: 0 } },
            ],
        };

        finalFilter = {
            $and: [
                notDisabledFilter,
                { $or: [userFilter, alwaysShownFilter] },
                campusFilter,
            ],
        };
    }

    const sort: ChannelSort = { last_message_at: -1 };
    const options = { limit: 20, messages_limit: 30 };

    return (
        <ChatConfigContext.Provider
            value={{
                sharedChannelTypeKey,
                directMessageChannelTypeKey,
                chatViewStyle: chatViewStyle || ChatViewStyle.Conversational,
                refreshChat,
                reactions: reactions || []
            }}>
            <ChannelListControllerContext.Provider value={{ refresh: refreshChannelList }}>
                <ChannelMemberListProvider>
                    <ChannelRightPaneProvider>
                        <DirectoryProvider>
                            <Chat client={chatClient} theme={chatViewStyle == ChatViewStyle.Community ? "rocktheme-community" : "rocktheme-conversational"} key={chatComponentKey}>
                                <ModalProvider>
                                    <ChatComponentContent
                                        channelId={channelId}
                                        selectedChannelId={selectedChannelId}
                                        finalFilter={finalFilter}
                                        sort={sort}
                                        options={options}
                                        chatViewStyle={chatViewStyle}
                                        directMessageChannelTypeKey={directMessageChannelTypeKey}
                                        sharedChannelTypeKey={sharedChannelTypeKey}
                                        jumpToMessageId={jumpToMessageId}
                                    />
                                </ModalProvider>
                            </Chat>
                        </DirectoryProvider>
                    </ChannelRightPaneProvider>
                </ChannelMemberListProvider>
            </ChannelListControllerContext.Provider>
        </ChatConfigContext.Provider >
    );
};

// Extracted content to use DirectoryContext
const ChatComponentContent: React.FC<any> = ({
    channelId,
    selectedChannelId,
    finalFilter,
    sort,
    options,
    chatViewStyle,
    directMessageChannelTypeKey,
    sharedChannelTypeKey,
    jumpToMessageId
}) => {
    const { showDirectory, toggleShowDirectory } = useDirectoryContext();
    const chatContentStyle: React.CSSProperties = {
        display: "flex",
        width: "100%",
        height: "100%",
    };
    return (
        <div style={chatContentStyle} className={chatViewStyle == ChatViewStyle.Community ? "rocktheme-community" : "rocktheme-conversational"}>
            {!channelId && (
                <div className="rock-channel-list">
                    <ChannelListHeader onSearch={() => toggleShowDirectory()} />
                    <WrappedChannelList
                        selectedChannelId={selectedChannelId}
                        filters={finalFilter}
                        sort={sort}
                        options={options}
                        Preview={RockChannelPreview}
                        renderChannels={getRenderChannelsFn(chatViewStyle!, directMessageChannelTypeKey!, sharedChannelTypeKey!)}
                        setActiveChannelOnMount={!selectedChannelId}
                        showChannelSearch={false} />
                </div>
            )}

            {showDirectory ? (
                <Directory />
            ) : (
                <WrappedChannel channelId={channelId} jumpToMessageId={jumpToMessageId}>
                    <RockChatWindow />
                </WrappedChannel>
            )}
        </div>
    );
};

export default ChatComponent;