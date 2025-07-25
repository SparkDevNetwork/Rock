/**
 * SafeMessageInput
 *
 * A wrapper around Stream's `MessageInput` component that ensures:
 * - The current individual is a member of the channel before sending a message.
 * - Membership is added dynamically if missing.
 */

import React, { useCallback } from "react";
import {
    useChannelActionContext,
    useChannelStateContext,
    useChatContext,
    MessageInput,
    MessageInputProps,
    MessageInputFlat,
} from "stream-chat-react";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatViewStyle } from "../../ChatViewStyle";
import { CommunityMessageInput } from "./CommunityMessageInput";
import { LocalMessage, SendMessageOptions, Message } from "stream-chat";

/**
 * A React component to safely send messages.
 *
 * @param {MessageInputProps} props - Standard MessageInput props.
 * @returns {JSX.Element} A wrapped MessageInput with membership guard.
 */
export const SafeMessageInput: React.FC<MessageInputProps> = (props: MessageInputProps) => {
    const { sendMessage } = useChannelActionContext();
    const { channel } = useChannelStateContext();
    const { client } = useChatContext();
    const { chatViewStyle } = useChatConfig();

    const customSubmitHandler: MessageInputProps["overrideSubmitHandler"] = useCallback(
        async ({ localMessage, message, sendOptions }: {
            localMessage: LocalMessage;
            message: Message;
            sendOptions: SendMessageOptions;
        }) => {

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


            await sendMessage({ localMessage, message, options: sendOptions });
        },
        [sendMessage],
    );

    const conversationalComponent = () => {
        return <MessageInput {...props}
            Input={MessageInputFlat}
            overrideSubmitHandler={customSubmitHandler} />;
    }

    const communityComponent = () => {
        return (
            <MessageInput {...props}
                Input={CommunityMessageInput}
                overrideSubmitHandler={customSubmitHandler} />
        )
    }

    return (
        <>
            {chatViewStyle == ChatViewStyle.Community ? communityComponent() : conversationalComponent()}
        </>
    )
};
