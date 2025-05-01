import React, { ReactNode, useEffect, useState } from "react";
import {
    Channel,
    useChannelActionContext,
    useChatContext,
} from "stream-chat-react";
import { RockMessageSimple } from "../Message/RockMessage";
import { RockMessageActionList } from "./RockMessageActionList";

interface WrappedChannelProps {
    children: ReactNode;
    cid?: string;
    jumpToMessageId?: string;
}

export const WrappedChannel: React.FC<WrappedChannelProps> = ({ children, cid, jumpToMessageId }) => {
    const { client, setActiveChannel } = useChatContext();
    const [channelReady, setChannelReady] = useState(false);

    useEffect(() => {
        const loadChannel = async () => {
            if (cid) {
                try {
                    const [channel] = await client.queryChannels({ cid }, { last_message_at: -1 }, { limit: 1 });
                    if (channel) {
                        setActiveChannel(channel);
                        setChannelReady(true);
                    } else {
                        console.warn(`Channel with CID ${cid} not found.`);
                    }
                } catch (error) {
                    console.error(`Error loading channel with CID ${cid}:`, error);
                }
            }
            else {
                setChannelReady(true); // no cid, so let it render
            }
        };

        loadChannel();
    }, [cid, client, setActiveChannel]);

    if (!channelReady) return null;

    return (
        <Channel CustomMessageActionsList={RockMessageActionList} Message={RockMessageSimple}>
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