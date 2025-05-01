import React from "react";
import {
    useChatContext,
    useMessageContext,
} from "stream-chat-react";
import { useChatConfig } from '../Chat/ChatConfigContext';

export const RockMessageActionList: React.FC = () => {
    const { message } = useMessageContext("CustomMessageActionList");
    const { client, setActiveChannel } = useChatContext();
    const chatConfig = useChatConfig();

    const showDirectMessageOption = () => {
        let author = message.user
        if (!author) {
            return false;
        }

        return author.rock_open_direct_message_allowed === true;
    }

    const initiateDirectMessage: () => Promise<void> = async () => {
        // Create a new channel with the direct message channel type
        // and the recipient as the author of the message.
        let author = message.user
        let currentUserId = client.userID

        if (!author || !currentUserId || !chatConfig?.directMessageChannelTypeKey) {
            return;
        }
        const channel = client.channel(chatConfig.directMessageChannelTypeKey, null, {
            members: [author.id, currentUserId]
        })

        await channel.create()
        setActiveChannel(channel)
    }

    return (
        <>
            {showDirectMessageOption() && (
                <button
                    className="str-chat__message-actions-list-item-button"
                    onClick={(event) => {
                        initiateDirectMessage()
                        event.stopPropagation()
                    }}>
                    {("Send message") as string}
                </button>
            )}
        </>
    );
};
