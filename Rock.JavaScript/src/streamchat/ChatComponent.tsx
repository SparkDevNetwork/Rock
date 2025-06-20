import React, { useState } from "react";
import {
    Chat,
    MessageList,
    Thread,
    Window,
} from "stream-chat-react";
import { ChannelSort, ChannelFilters } from "stream-chat";
import { useCreateChatClient } from "stream-chat-react";
import "stream-chat-react/dist/css/v2/index.css";
import "./chatComponent.css";

import type { ChatComponentProps } from "./ChatComponentProps";
import { RockChannelPreview } from "./ChannelPreview/RockChannelPreview";
import { ChatConfigContext } from "./Chat/ChatConfigContext";
import { SafeMessageInput } from "./MessageInput/SafeMessageInput";
import ChannelListHeader from "./ChannelListHeader/ChannelListHeader";
import CreateChannelModal from "./CreateChannel/CreateChannelModal";
import { RockChannelHeader } from "./ChannelHeader/RockChannelHeader";
import { WrappedChannel } from "./MessageAction/WrappedChannel";
import { WrappedChannelList } from "./ChannelList/WrappedChannelList";
import { getRenderChannelsFn } from "./ChatUtils";
import { ChannelListControllerContext } from "./ChannelList/ChannelListControllerContext";
import { ChatViewStyle } from "./ChatViewStyle";
import { ChannelRightPaneProvider } from "./ChannelRightPane/PanelContext";
import { ChannelRightPane } from "./ChannelRightPane/ChannelRightPane";
import { RockChatWindow } from "./RockChatWindow";

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

    const [showModal, setShowModal] = useState(false);
    const handleNewMessage = () => setShowModal(true);
    const handleCloseModal = () => setShowModal(false);

    const handleSearch = () => {
        // Implement search functionality here
        console.log("Search functionality not implemented yet.");
    }

    const [, setChannelListKey] = useState(0);

    const refreshChannelList = () => {
        setTimeout(() => {
            setChannelListKey(prev => prev + 1);
        }, 200); // delay in milliseconds
    };


    const chatClient = useCreateChatClient({
        apiKey: apiKey,
        tokenOrProvider: userToken,
        userData: { id: userId },
    });

    if (!chatClient) {
        return <></>;
    }

    // Define base filters
    const userFilter: ChannelFilters = { members: { $in: [userId] } };
    const alwaysShownFilter: ChannelFilters = { rock_always_shown: true };
    const notDisabledFilter: ChannelFilters = { disabled: false };

    let finalFilter: ChannelFilters = {
        $or: [userFilter, alwaysShownFilter],
        ...notDisabledFilter,
    };

    // Optionally add campus filter if required
    if (filterSharedChannelByCampus) {
        if (currentCampusId !== null) {
            const campusIdAsNumber = Number(currentCampusId);

            const campusFilter: ChannelFilters = {
                $or: [
                    { rock_campus_id: { $eq: campusIdAsNumber } },
                    { rock_campus_id: { $lte: 0 } }, // shared/global channels
                ],
            };

            finalFilter = {
                $and: [
                    notDisabledFilter,
                    { $or: [userFilter, alwaysShownFilter] },
                    campusFilter,
                ],
            };
        } else {
            console.warn(
                "[ChatComponent] Campus filtering is enabled but no campus ID was provided. Proceeding without campus filter."
            );
        }
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
        // Only show the chat if the client is initialized
        <Chat client={chatClient} theme={theme}>
            <ChatConfigContext.Provider
                value={{
                    sharedChannelTypeKey,
                    directMessageChannelTypeKey,
                    chatViewStyle: chatViewStyle || ChatViewStyle.Conversational,
                }}>
                <ChannelListControllerContext.Provider value={{ refresh: refreshChannelList }}>
                    <div style={chatContentStyle} className={`${theme}`}>
                        {/* If a channel is passed in, hide the channel list and show the channel directly. */}
                        {!channelId && (
                            <>
                                <div className={`rock-channel-list`}>
                                    <ChannelListHeader onNewMessage={handleNewMessage} onSearch={handleSearch} />
                                    <WrappedChannelList
                                        selectedChannelId={selectedChannelId}
                                        filters={finalFilter}
                                        sort={sort}
                                        options={options}
                                        Preview={RockChannelPreview}
                                        renderChannels={getRenderChannelsFn(chatViewStyle!, directMessageChannelTypeKey!, sharedChannelTypeKey!)}
                                        // if there is a passed in selectedChannelId, we need to set it as the active channel
                                        setActiveChannelOnMount={selectedChannelId == undefined || selectedChannelId == null || selectedChannelId == ""}
                                    />
                                </div>
                            </>
                        )}

                        <ChannelRightPaneProvider>
                            <WrappedChannel channelId={channelId} jumpToMessageId={jumpToMessageId}>
                                <RockChatWindow />
                            </WrappedChannel>
                        </ChannelRightPaneProvider>

                        {/* Create DM modal */}
                        {showModal && (
                            <CreateChannelModal
                                onClose={handleCloseModal}
                            />
                        )}
                    </div>
                </ChannelListControllerContext.Provider>
            </ChatConfigContext.Provider>
        </Chat>
    );
};

export default ChatComponent;