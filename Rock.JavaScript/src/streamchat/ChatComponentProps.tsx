import { ChatViewStyle } from "./ChatViewStyle";

export interface ChatComponentProps {
    primaryColor: string;
    apiKey: string;
    userId: string;
    userToken: string;
    filterSharedChannelByCampus: boolean;
    currentCampusId: string | null;
    sharedChannelTypeKey: string;
    directMessageChannelTypeKey: string;
    channelId?: string;
    selectedChannelId?: string;
    jumpToMessageId?: string;
    chatViewStyle?: ChatViewStyle;
    reactions: ChatReactionBag[];
}

/** Represents a reaction that can be used in a chat message, including its key, optional image, and display text. */
export type ChatReactionBag = {
    /** Gets or sets the image URL. */
    imageUrl?: string | null;

    /** Gets or sets the medium image url. */
    imageUrlMedium?: string | null;

    /** Gets or sets the small image URL, which is a scaled down version of the main Rock.ViewModels.Blocks.Communication.Chat.ChatView.ChatReactionBag.ImageUrl. */
    imageUrlSmall?: string | null;

    /** Gets or sets the unique key that identifies the reaction. */
    key?: string | null;

    /** Gets or sets the text that represents the reaction (e.g 😲). */
    reactionText?: string | null;
};