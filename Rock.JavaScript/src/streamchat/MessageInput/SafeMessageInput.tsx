/**
 * SafeMessageInput
 *
 * A wrapper around Stream's `MessageInput` component that ensures:
 * - The current individual is a member of the channel before sending a message.
 * - Membership is added dynamically if missing.
 */

import React from "react";
import {
    useChannelActionContext,
    useChannelStateContext,
    useChatContext,
    MessageInput,
    MessageInputProps,
    MessageToSend,
} from "stream-chat-react";

/**
 * A React component to safely send messages.
 *
 * @param {MessageInputProps} props - Standard MessageInput props.
 * @returns {JSX.Element} A wrapped MessageInput with membership guard.
 */
export const SafeMessageInput: React.FC<MessageInputProps> = (props) => {
    const { sendMessage } = useChannelActionContext();
    const { channel } = useChannelStateContext();
    const { client } = useChatContext();

    /**
     * Ensures the current user is a channel member before sending.
     *
     * @param {MessageToSend} message - The message to be sent.
     */
    const overrideSubmitHandler = async (message: MessageToSend) => {
        const userId = client.userID;

        if (!userId) {
            console.error("User ID is not available. Cannot send message.");
            return;
        }

        const cid = channel.cid;
        const isMember = !!channel.state.members[userId];

        if (!isMember) {
            try {
                await channel.addMembers([userId]);
                console.log(`User ${userId} added to channel ${cid}`);
            } catch (error) {
                console.error(`Failed to add user ${userId} to channel ${cid}:`, error);
                return;
            }
        }

        sendMessage(message);
    };

    return <MessageInput {...props} overrideSubmitHandler={overrideSubmitHandler} />;
};
