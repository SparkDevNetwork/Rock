import React, { useState } from "react";
import {
    Chat,
    Thread,
} from "stream-chat-react";
import { ChannelSort, ChannelFilters } from "stream-chat";
import { useCreateChatClient } from "stream-chat-react";
import "stream-chat-react/dist/css/v2/index.css";
import "./chatComponent.css";

import type { ChatComponentProps } from "./ChatComponentProps";
import { RockChannelPreview } from "./ChannelPreview/RockChannelPreview";
import { ChatConfigContext } from "./Chat/ChatConfigContext";
import ChannelListHeader from "./ChannelListHeader/ChannelListHeader";
import CreateChannelModal from "./CreateChannel/CreateChannelModal";
import { WrappedChannel } from "./MessageAction/WrappedChannel";
import { WrappedChannelList } from "./ChannelList/WrappedChannelList";
import { getRenderChannelsFn } from "./ChatUtils";
import { ChannelListControllerContext } from "./ChannelList/ChannelListControllerContext";
import { ChatViewStyle } from "./ChatViewStyle";
import { ChannelRightPaneProvider } from "./ChannelRightPane/ChannelRightPaneContext";
import { RockChatWindow } from "./RockChatWindow";
import { ModalProvider } from "./Modal/ModalContext";

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

    // const [showCreateDMModal, setShowCreateDMModal] = useState(false);
    // const handleNewMessage = () => setShowCreateDMModal(true);
    // const handleCloseModal = () => setShowCreateDMModal(false);

    const handleSearch = () => {
        // Implement search functionality here
        console.log("Search functionality not implemented yet.");
    }

    const [, setChannelListKey] = useState(0);

    const refreshChannelList = () => {
        setTimeout(() => {
            setChannelListKey(prev => prev + 1);
        }, 200);
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

                <ModalProvider>
                    <ChannelListControllerContext.Provider value={{ refresh: refreshChannelList }}>
                        <div style={chatContentStyle} className={`${theme}`}>
                            {/* If a channel is passed in, hide the channel list and show the channel directly. */}
                            {!channelId && (
                                <>
                                    <div className={`rock-channel-list`}>
                                        <ChannelListHeader onSearch={handleSearch} />
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

                            {/* Create DM modal
                            {showCreateDMModal && (
                                <CreateChannelModal
                                    onClose={handleCloseModal}
                                />
                            )} */}
                        </div>
                    </ChannelListControllerContext.Provider>
                </ModalProvider>
            </ChatConfigContext.Provider>
        </Chat>
    );
};

export default ChatComponent;