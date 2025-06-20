// ComunityMessageInput.tsx
import React, { PropsWithChildren } from 'react';
import { CooldownTimer, MessageInputProps, SendButton, TextareaComposer, WithDragAndDropUpload } from 'stream-chat-react';
import {
    AttachmentPreviewList,
    LinkPreviewList,
    QuotedMessagePreview,
    SimpleAttachmentSelector,
    useComponentContext,
    useMessageInputContext,
} from "stream-chat-react";

const SendButtonWithCooldown = () => {
    const { handleSubmit, cooldownRemaining, setCooldownRemaining } =
        useMessageInputContext();
    return cooldownRemaining ? (
        <CooldownTimer
            cooldownInterval={cooldownRemaining}
            setCooldownRemaining={setCooldownRemaining}
        />
    ) : (
        <SendButton sendMessage={handleSubmit} />
    );
};

export const CommunityMessageInput = () => (
    <WithDragAndDropUpload>
        <div className="rock-message-composer">
            <TextareaComposer containerClassName="rock-message-input-container" />
        </div>
    </WithDragAndDropUpload>
);
