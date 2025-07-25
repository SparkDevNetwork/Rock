import React from 'react';

import type { MouseEventHandler, PropsWithChildren } from 'react';
import { ModalProps, useMessageBounceContext, useTranslationContext } from 'stream-chat-react';


export type MessageBouncePromptProps = PropsWithChildren<Pick<ModalProps, 'onClose'>>;

export function MessageBouncePrompt({ children, onClose }: MessageBouncePromptProps) {
    const { handleDelete, handleEdit, handleRetry } =
        useMessageBounceContext('MessageBouncePrompt');
    const { t } = useTranslationContext('MessageBouncePrompt');

    function createHandler(
        handle: MouseEventHandler<HTMLButtonElement>,
    ): MouseEventHandler<HTMLButtonElement> {
        return (e) => {
            handle(e);
            onClose?.(e);
        };
    }

    return (
        <div className='str-chat__message-bounce-prompt' data-testid='message-bounce-prompt'>
            <div className='str-chat__message-bounce-prompt-header'>
                {children ?? t('This message did not meet our content guidelines') as string}
            </div>
            <div className='str-chat__message-bounce-actions'>
                <button
                    className='str-chat__message-bounce-edit'
                    data-testid='message-bounce-edit'
                    onClick={createHandler(handleEdit)}
                    type='button'
                >
                    {t('Edit Message') as string}
                </button>
                <button
                    className='str-chat__message-bounce-send'
                    data-testid='message-bounce-send'
                    onClick={createHandler(handleRetry)}
                >
                    {t('Send Anyway') as string}
                </button>
                <button
                    className='str-chat__message-bounce-delete'
                    data-testid='message-bounce-delete'
                    onClick={createHandler(handleDelete)}
                >
                    {t('Delete') as string}
                </button>
            </div>
        </div>
    );
}