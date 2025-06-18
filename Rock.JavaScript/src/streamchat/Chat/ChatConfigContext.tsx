/**
 * ChatConfigContext
 *
 * Provides shared configuration for chat behavior such as channel type keys.
 */

/**
 * ChatConfigContextType defines the shape of the configuration context.
 * @property {string} [sharedChannelTypeKey] - Optional key to identify shared channels.
 * @property {string} [directMessageChannelTypeKey] - Optional key to identify DM channels.
 */
import { createContext, useContext } from "react";
import { ChatViewStyle } from "../ChatViewStyle";

interface ChatConfigContextType {
    sharedChannelTypeKey?: string;
    directMessageChannelTypeKey?: string;
    chatViewStyle: ChatViewStyle;
}

/**
 * React Context instance for Chat configuration.
 */
export const ChatConfigContext = createContext<ChatConfigContextType | undefined>(undefined);

/**
 * Custom hook to access the chat configuration context.
 * Throws if used outside a ChatConfigContext.Provider.
 *
 * @returns {ChatConfigContextType} The current chat config values.
 */
export const useChatConfig = (): ChatConfigContextType => {
    const context = useContext(ChatConfigContext);
    if (!context) {
        throw new Error("useChatConfig must be used within a ChatConfigProvider");
    }
    return context;
};
