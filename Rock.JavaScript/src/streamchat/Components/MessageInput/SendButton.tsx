import React from 'react';
import type { UpdatedMessage } from 'stream-chat';
import { useMessageComposerHasSendableData } from 'stream-chat-react';

export type SendButtonProps = {
    sendMessage: (
        event: React.BaseSyntheticEvent,
        customMessageData?: Omit<UpdatedMessage, 'mentioned_users'>,
    ) => void;
} & React.ComponentProps<'button'>;
export const SendButton = ({ sendMessage, ...rest }: SendButtonProps) => {
    const hasSendableData = useMessageComposerHasSendableData();
    return (
        <button
            aria-label='Send'
            className='rock-send-button'
            data-testid='send-button'
            disabled={!hasSendableData}
            onClick={sendMessage}
            type='button'
            {...rest}
        >
            <i className="fas fa-paper-plane" aria-hidden="true"></i>
        </button>
    );
};