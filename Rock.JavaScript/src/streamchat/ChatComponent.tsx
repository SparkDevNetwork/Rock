import React, { useState } from "react";
import {
    Chat,
    ChannelList,
    ChannelHeader,
    MessageList,
    Thread,
    Window,
    DialogManagerProvider,
    Channel,
    MessageActions,
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
import { CustomMessageActions } from "./MessageAction/CustomMessageActions";
import { WrappedChannel } from "./MessageAction/WrappedChannel";
import { WrappedChannelList } from "./ChannelList/WrappedChannelList";

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
}) => {

    const [showModal, setShowModal] = useState(false);
    const handleNewMessage = () => setShowModal(true);
    const handleCloseModal = () => setShowModal(false);

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

    return (
        // Only show the chat if the client is initialized
        <Chat client={chatClient}>
            <ChatConfigContext.Provider
                value={{
                    sharedChannelTypeKey,
                    directMessageChannelTypeKey,
                }}>
                <div style={chatContentStyle}>
                    {/* If a channel is passed in, hide the channel list and show the channel directly. */}
                    {!channelId && (
                        <>
                            <div className="rock__channel-list-container">
                                <ChannelListHeader onNewMessage={handleNewMessage} />
                                <WrappedChannelList
                                    selectedChannelId={selectedChannelId}
                                    filters={finalFilter}
                                    sort={sort}
                                    options={options}
                                    Preview={RockChannelPreview}

                                    // if there is a passed in selectedChannelId, we need to set it as the active channel
                                    setActiveChannelOnMount={selectedChannelId == undefined || selectedChannelId == null || selectedChannelId == ""}
                                />
                            </div>
                        </>
                    )}

                    <WrappedChannel channelId={channelId} jumpToMessageId={jumpToMessageId}>
                        <Window>
                            <RockChannelHeader />
                            <MessageList messageActions={['edit', 'delete', 'flag', 'mute', 'quote', 'react', 'reply']} noGroupByUser />
                            <SafeMessageInput grow />
                        </Window>
                        <Thread />
                    </WrappedChannel>

                    {/* Create DM modal */}
                    {showModal && (
                        <CreateChannelModal
                            onClose={handleCloseModal}
                        />
                    )}
                </div>
            </ChatConfigContext.Provider>
        </Chat>
    );
};

export default ChatComponent;