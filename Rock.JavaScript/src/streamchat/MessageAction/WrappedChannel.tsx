import React, { ReactNode, useEffect, useState } from "react";
import {
    Channel,
    useChannelActionContext,
    useChatContext,
} from "stream-chat-react";
// import { RockMessageSimple } from "../Message/RockMessage";
import { RockMessageActionList } from "./RockMessageActionList";
import { RockDateSeperator } from "../DateSeperator/DateSeperator";
import { RockMessage } from "../Message/RockMessage";

interface WrappedChannelProps {
    children: ReactNode;
    channelId?: string;
    jumpToMessageId?: string;
}

export const WrappedChannel: React.FC<WrappedChannelProps> = ({ children, channelId, jumpToMessageId }) => {
    const { client, setActiveChannel } = useChatContext();
    const [channelReady, setChannelReady] = useState(false);

    useEffect(() => {
        const loadChannel = async () => {
            if (channelId) {
                try {
                    const [channel] = await client.queryChannels({ cid: channelId }, { last_message_at: -1 }, { limit: 1 });
                    if (channel) {
                        setActiveChannel(channel);
                        setChannelReady(true);
                    } else {
                        console.warn(`Channel with CID ${channelId} not found.`);
                    }
                } catch (error) {
                    console.error(`Error loading channel with CID ${channelId}:`, error);
                }
            }
            else {
                setChannelReady(true); // no cid, so let it render
            }
        };

        loadChannel();
    }, [channelId, client, setActiveChannel]);

    if (!channelReady) return null;

    return (
        <Channel CustomMessageActionsList={RockMessageActionList} Message={RockMessage} DateSeparator={RockDateSeperator}>
            {children}
            {/* if there is a jumpToMessageId, we need to jump to it. this has to be done in the channel context which is why we have a nested component */}
            {jumpToMessageId && <JumpToMessage messageId={jumpToMessageId} />}
        </Channel>
    );
};

const JumpToMessage: React.FC<{ messageId: string }> = ({ messageId }) => {
    const { jumpToMessage } = useChannelActionContext();

    useEffect(() => {
        if (messageId && jumpToMessage) {
            jumpToMessage(messageId);
        }
    }, [messageId, jumpToMessage]);

    return null;
};