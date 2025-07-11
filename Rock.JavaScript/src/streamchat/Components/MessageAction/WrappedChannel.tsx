import React, { ReactNode, useEffect, useState } from "react";
import {
    Channel,
    CloseIcon,
    ReactionOptions,
    SpriteImage,
    StreamEmoji,
    ThreadHeaderProps,
    useChannelActionContext,
    useChannelPreviewInfo,
    useChannelStateContext,
    useChatContext,
    useTranslationContext,
} from "stream-chat-react";
// import { RockMessageSimple } from "../Message/RockMessage";
import { RockMessageActionList } from "./RockMessageActionList";
import { RockDateSeperator } from "../DateSeperator/DateSeperator";
import { RockMessage } from "../Message/RockMessage";
import { ChannelPaneHeader } from "../ChannelRightPane/ChannelPaneHeader";
import { useChannelRightPane } from "../ChannelRightPane/ChannelRightPaneContext";
import { useChatConfig } from "../Chat/ChatConfigContext";
import { ChatReactionBag } from "src/streamchat/ChatComponentProps";
import { CommunityMessageInput } from "../MessageInput/CommunityMessageInput";
import { SafeMessageInput } from "../MessageInput/SafeMessageInput";
import { EditMessageForm } from "../MessageInput/EditMessageForm";

interface WrappedChannelProps {
    children: ReactNode;
    channelId?: string;
    jumpToMessageId?: string;
}

export const WrappedChannel: React.FC<WrappedChannelProps> = ({ children, channelId, jumpToMessageId }) => {
    const { client, setActiveChannel } = useChatContext();
    const [channelReady, setChannelReady] = useState(false);
    const { reactions } = useChatConfig();

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

    // const defaultReactionOptions: ReactionOptions = [
    //     { type: 'haha', Component: () => <></>, name: 'Joy' },
    //     { type: 'like', Component: () => <StreamEmoji fallback='👍' type='like' />, name: 'Thumbs up' },
    //     { type: 'love', Component: () => <StreamEmoji fallback='❤️' type='love' />, name: 'Heart' },
    //     { type: 'sad', Component: () => <StreamEmoji fallback='😔' type='sad' />, name: 'Sad' },
    //     { type: 'wow', Component: () => <StreamEmoji fallback='😲' type='wow' />, name: 'Astonished' },
    // ];


    // Loop over the configuration and create the reaction options
    const reactionOptions: ReactionOptions = reactions
        .filter(reaction => typeof reaction.key === "string")
        .map(reaction => ({
            type: reaction.key as string,
            Component: () => <CustomReactionComponent reaction={reaction} />,
        }));

    /**
     * CustomReactionComponent
     *
     * Renders a custom reaction option. If an image is provided (imageUrlSmall or imageUrl),
     * it displays the image. Otherwise, it displays the reaction text.
     *
     * @param reaction - The reaction configuration object
     * @returns {JSX.Element}
     */
    const CustomReactionComponent: React.FC<{ reaction: ChatReactionBag }> = ({ reaction }) => {
        const imageSrc = reaction.imageUrlSmall || reaction.imageUrlMedium || reaction.imageUrl;
        if (imageSrc) {
            return (
                <div className="rock-reaction-option">
                    <img className="rock-reaction-image" src={imageSrc} alt={reaction.key || 'Unknown'} />
                </div>
            );
        }
        // Fallback: show reaction text if no image
        return reaction.reactionText;
    };

    return (
        <Channel CustomMessageActionsList={RockMessageActionList}
            Message={RockMessage}
            DateSeparator={RockDateSeperator}
            AttachmentSelectorInitiationButtonContents={CustomAttachmentSelectorInitiationButtonContents}
            EditMessageInput={EditMessageForm}
            Input={SafeMessageInput}
            reactionOptions={reactionOptions}
            ThreadHeader={CustomThreadHeader}>
            {children}
            {/* if there is a jumpToMessageId, we need to jump to it. this has to be done in the channel context which is why we have a nested component */}
            {jumpToMessageId && <JumpToMessage messageId={jumpToMessageId} />}
        </Channel>
    );
};

export const CustomThreadHeader = (props: ThreadHeaderProps) => {
    const { channel } = useChannelStateContext('');
    const { displayTitle } = useChannelPreviewInfo({
        channel,
    });

    const memberCount = Object.keys(channel?.state?.members || {}).length;
    const titleText = memberCount <= 2 ? 'Direct Message' : displayTitle;

    return (
        <ChannelPaneHeader title={`Thread in ${titleText}`} icon="fas fa-comment-dots" />
    );
};

const CustomAttachmentSelectorInitiationButtonContents: React.FC = () => {
    return (
        <i className="fas fa-upload rock-message-composer-action"></i>
    );
}

const JumpToMessage: React.FC<{ messageId: string }> = ({ messageId }) => {
    const { jumpToMessage } = useChannelActionContext();

    useEffect(() => {
        if (messageId && jumpToMessage) {
            jumpToMessage(messageId);
        }
    }, [messageId, jumpToMessage]);

    return null;
};