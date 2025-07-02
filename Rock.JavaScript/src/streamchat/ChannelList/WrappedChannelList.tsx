import React, { useEffect } from "react";
import {
    ChannelList,
    useChatContext,
    ChannelListProps,
} from "stream-chat-react";

interface WrappedChannelListProps extends ChannelListProps {
    selectedChannelId?: string;
}

export const WrappedChannelList: React.FC<WrappedChannelListProps> = ({
    selectedChannelId,
    ...props
}) => {
    const { client, setActiveChannel } = useChatContext();

    useEffect(() => {
        if (!selectedChannelId) return;

        const loadSelectedChannel = async () => {
            try {
                console.log(`Loading channel with cid: ${selectedChannelId}`);

                const [channel] = await client.queryChannels(
                    { cid: { $eq: selectedChannelId } },
                    { last_message_at: -1 },
                    { limit: 1 }
                );

                if (channel) {
                    console.log(`Found channel: ${channel.cid}`);
                    setActiveChannel(channel);
                } else {
                    console.warn(`Channel with cid ${selectedChannelId} not found.`);
                }
            } catch (error) {
                console.error(`Failed to select channel with cid ${selectedChannelId}`, error);
            }
        };

        loadSelectedChannel();
    }, [selectedChannelId, client, setActiveChannel]);

    return (
        <ChannelList
            {...props}
        />
    );
};