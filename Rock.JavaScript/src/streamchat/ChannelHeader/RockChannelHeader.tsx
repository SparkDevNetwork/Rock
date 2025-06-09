import React from 'react';
import { ChannelHeader as DefaultHeader, useChatContext } from 'stream-chat-react';
import { DefaultChatChannelNamer } from '../ChannelNamer/DefaultChannelNamer';
import { useChatConfig } from '../Chat/ChatConfigContext';
/**
 * Overrides the built-in ChannelHeader to use the DefaultChatChannelNamer
 * for displayTitle, guarding against a null channel.
 */
export const RockChannelHeader: React.FC = () => {
    const { channel, client } = useChatContext();
    const chatConfig = useChatConfig();

    // If there's no channel yet, render the default header without a custom title
    if (!channel) {
        return <DefaultHeader />;
    }

    // Use our namer, falling back to undefined if it returns null
    const title = DefaultChatChannelNamer(channel, chatConfig.directMessageChannelTypeKey!, client.userID!) ?? undefined;

    return <DefaultHeader title={title} />;
};
