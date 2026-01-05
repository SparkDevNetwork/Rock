import React, { } from "react";
import {
    MessageContextValue,
    areMessageUIPropsEqual,
    MessageUIComponentProps,
    useMessageContext,
} from 'stream-chat-react';

// import { MessageTimestamp as DefaultMessageTimestamp } from './MessageTimestamp';
import { useChatConfig } from '../Chat/ChatConfigContext';
import { ChatViewStyle } from '../../ChatViewStyle';
import { CommunityMessage } from "./CommunityMessage";
import { ConversationalMessage } from "./ConversationalMessage";

type RockMessageProps = MessageContextValue;

// Rock Message Custom Component
const RockMessageWithContext = (props: RockMessageProps) => {
    if (!props.message.user) {
        return <></>
    };

    const { chatViewStyle } = useChatConfig();

    return (
        <>
            {chatViewStyle == ChatViewStyle.Community ? (<CommunityMessage {...props} />) : (<ConversationalMessage {...props} />)}
        </>)
}

const MemoizedRockMessage = React.memo(
    RockMessageWithContext,
    areMessageUIPropsEqual,
) as typeof RockMessageWithContext;

/**
 * The default UI component that renders a message and receives functionality and logic from the MessageContext.
 */
export const RockMessage = (props: MessageUIComponentProps) => {
    const messageContext = useMessageContext('MessageSimple');

    return <MemoizedRockMessage {...messageContext} {...props} />;
};