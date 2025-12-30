import React, { useCallback, useEffect } from "react";
import { useTranslationContext, useMessageComposer, useMessageInputContext, MessageInputFlat, useMessageComposerHasSendableData } from "stream-chat-react";
import { SafeMessageInput } from "./SafeMessageInput";

const EditMessageFormSendButton = () => {
    const { t } = useTranslationContext();
    const hasSendableData = useMessageComposerHasSendableData();
    return (
        <button
            className='btn btn-primary'
            data-testid='send-button-edit-form'
            disabled={!hasSendableData}
            type='submit'
        >
            {t('Send')}
        </button>
    );
};
export const EditMessageForm = () => {
    const { t } = useTranslationContext('EditMessageForm');
    const messageComposer = useMessageComposer();
    const { clearEditingState, handleSubmit } = useMessageInputContext('EditMessageForm');

    const cancel = useCallback(() => {
        clearEditingState?.();
        messageComposer.restore();
    }, [clearEditingState, messageComposer]);

    useEffect(() => {
        const onKeyDown = (event: KeyboardEvent) => {
            if (event.key === 'Escape') cancel();
        };

        document.addEventListener('keydown', onKeyDown);
        return () => document.removeEventListener('keydown', onKeyDown);
    }, [cancel]);

    return (
        <form
            autoComplete='off'
            className='str-chat__edit-message-form'
            onSubmit={handleSubmit}>
            <SafeMessageInput hideSendButton />
            <div className='str-chat__edit-message-form-options'>
                <button
                    className='btn btn-secondary'
                    data-testid='cancel-button'
                    onClick={cancel}
                    type='button'
                >
                    {t('Cancel')}
                </button>
                <EditMessageFormSendButton />
            </div>
        </form>
    );
};
