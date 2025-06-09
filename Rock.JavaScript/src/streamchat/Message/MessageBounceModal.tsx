import React, { ComponentType, PropsWithChildren } from 'react';

import { MessageBouncePromptProps } from './MessageBouncePrompt';
import { ModalProps, Modal, MessageBounceProvider } from 'stream-chat-react';

export type MessageBounceModalProps = PropsWithChildren<
    ModalProps & {
        MessageBouncePrompt: ComponentType<MessageBouncePromptProps>;
    }
>;

export function MessageBounceModal({
    MessageBouncePrompt,
    ...modalProps
}: MessageBounceModalProps) {
    return (
        <Modal className='str-chat__message-bounce-modal' {...modalProps}>
            <MessageBounceProvider>
                <MessageBouncePrompt onClose={modalProps.onClose} />
            </MessageBounceProvider>
        </Modal>
    );
}