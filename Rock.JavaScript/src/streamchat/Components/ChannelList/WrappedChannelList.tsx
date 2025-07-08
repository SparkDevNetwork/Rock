import React, { useEffect } from "react";
import {
    ChannelList,
    useChatContext,
    ChannelListProps,
} from "stream-chat-react";

/**
 * Props for WrappedChannelList component.
 *
 * @property {string} [selectedChannelId] - Optional. The channel ID (cid) to select and activate when the component mounts or when it changes.
 * Inherits all props from ChannelListProps.
 */
interface WrappedChannelListProps extends ChannelListProps {
    selectedChannelId?: string;
}

/**
 * WrappedChannelList
 *
 * A wrapper around Stream's ChannelList that automatically selects and activates a channel
 * when the `selectedChannelId` prop is provided or changes.
 *
 * Features:
 * - On mount or when `selectedChannelId` changes, queries for the channel by cid and sets it as active.
 * - Handles errors and missing channels gracefully with console warnings.
 * - Passes all other props through to the underlying ChannelList.
 *
 * @param {WrappedChannelListProps} props - The component props.
 * @returns {JSX.Element} The rendered ChannelList component.
 */
export const WrappedChannelList: React.FC<WrappedChannelListProps> = ({
    selectedChannelId,
    ...props
}) => {
    // Access Stream Chat client and setActiveChannel function from context
    const { client, setActiveChannel } = useChatContext();

    useEffect(() => {
        // Only run if a selectedChannelId is provided
        if (!selectedChannelId) return;

        /**
         * Loads and activates the channel matching selectedChannelId.
         *
         * Side effects:
         * - Sets the active channel in the chat context if found.
         * - Logs a warning if the channel is not found.
         * - Logs an error if the query fails.
         */
        const loadSelectedChannel = async () => {
            try {
                // Query for the channel by cid (channel ID)
                const [channel] = await client.queryChannels(
                    { cid: { $eq: selectedChannelId } },
                    { last_message_at: -1 },
                    { limit: 1 }
                );

                if (channel) {
                    // Set the found channel as active in the UI
                    setActiveChannel(channel);
                } else {
                    // Channel not found: warn for debugging
                    console.warn(`Channel with cid ${selectedChannelId} not found.`);
                }
            } catch (error) {
                // Query failed: log error for debugging
                console.error(`Failed to select channel with cid ${selectedChannelId}`, error);
            }
        };

        loadSelectedChannel();
    }, [selectedChannelId, client, setActiveChannel]);

    // Render the ChannelList with all other props
    return (
        <ChannelList {...props} />
    );
};