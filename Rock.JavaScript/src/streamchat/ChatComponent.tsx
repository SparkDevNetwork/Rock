import React, { useState } from "react";
import {
    Chat,
    Thread,
    WithComponents,
} from "stream-chat-react";
import { ChannelSort, ChannelFilters } from "stream-chat";
import { useCreateChatClient } from "stream-chat-react";
import "stream-chat-react/dist/css/v2/index.css";
import "./chatComponent.css";

import type { ChatComponentProps } from "./ChatComponentProps";
import { RockChannelPreview } from "./Components/ChannelPreview/RockChannelPreview";
import { ChatConfigContext } from "./Components/Chat/ChatConfigContext";
import ChannelListHeader from "./Components/ChannelListHeader/ChannelListHeader";
import CreateChannelModal from "./Components/CreateChannel/CreateChannelModal";
import { WrappedChannel } from "./Components/MessageAction/WrappedChannel";
import { WrappedChannelList } from "./Components/ChannelList/WrappedChannelList";
import { getRenderChannelsFn } from "./ChatUtils";
import { ChannelListControllerContext } from "./Components/ChannelList/ChannelListControllerContext";
import { ChatViewStyle } from "./ChatViewStyle";
import { ChannelRightPaneProvider } from "./Components/ChannelRightPane/ChannelRightPaneContext";
import { RockChatWindow } from "./RockChatWindow";
import { ModalProvider } from "./Components/Modal/ModalContext";
import { Search } from "stream-chat-react/experimental";

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
    chatViewStyle
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

    const chatContentStyle: React.CSSProperties = {
        display: "flex",
        width: "100%",
        height: "100%",
    };

    const theme = chatViewStyle == ChatViewStyle.Community ? "rocktheme-community" : "rocktheme-conversational";

    return (
        <ChatConfigContext.Provider
            value={{
                sharedChannelTypeKey,
                directMessageChannelTypeKey,
                chatViewStyle: chatViewStyle || ChatViewStyle.Conversational,
                refreshChat,
            }}
        >
            <ModalProvider>
                <ChannelListControllerContext.Provider value={{ refresh: refreshChannelList }}>
                    <ChannelRightPaneProvider>
                        <Chat client={chatClient} theme={theme} key={chatComponentKey}>
                            <div style={chatContentStyle} className={theme}>
                                {!channelId && (
                                    <div className="rock-channel-list">
                                        <ChannelListHeader onSearch={() => console.log("Search clicked")} />
                                        <WrappedChannelList
                                            selectedChannelId={selectedChannelId}
                                            filters={finalFilter}
                                            sort={sort}
                                            options={options}
                                            Preview={RockChannelPreview}
                                            renderChannels={getRenderChannelsFn(chatViewStyle!, directMessageChannelTypeKey!, sharedChannelTypeKey!)}
                                            setActiveChannelOnMount={!selectedChannelId}
                                            showChannelSearch={false}
                                        />
                                    </div>
                                )}

                                <WrappedChannel channelId={channelId} jumpToMessageId={jumpToMessageId}>
                                    <RockChatWindow />
                                </WrappedChannel>
                            </div>
                        </Chat>
                    </ChannelRightPaneProvider>
                </ChannelListControllerContext.Provider>
            </ModalProvider>
        </ChatConfigContext.Provider>
    );
};

export default ChatComponent;